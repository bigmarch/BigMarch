// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/GlobalRampFog" 
{
	Properties 
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_TexelSize;
			uniform half4 _DistanceParams;

			// for fast world space reconstruction
			uniform float4x4 _FrustumCornersWS;
			uniform float4 _CameraWS;
			uniform sampler2D _DepthTex;
			uniform sampler2D _RampTexture;
			uniform float _Multiplier;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv_depth : TEXCOORD1;
				float4 interpolatedRay : TEXCOORD2;
			};

			v2f vert(appdata_img v)
			{
				v2f o;
				half index = v.vertex.z;
				v.vertex.z = 0.1;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				o.uv_depth = v.texcoord.xy;

		#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1 - o.uv.y;
		#endif				
				o.interpolatedRay = _FrustumCornersWS[(int)index];
				o.interpolatedRay.w = index;

				return o;
			}

			float Remap(float val, float iMin, float iMax, float oMin, float oMax)
			{
				return oMin + (val - iMin) * (oMax - oMin) / (iMax - iMin);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return fixed4(1,0,0,1);
				float linearDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_DepthTex, i.uv_depth));
				//float linearDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_DepthTex, i.uv_depth));

				//return linearDepth.rrrr;
				float u = Remap(linearDepth, _DistanceParams.x, _DistanceParams.y, 0, 1);
				u = saturate(u);
				//return u.xxxx;

				fixed4 fogColor = tex2D(_RampTexture, float2(u, 0.5f));

				fixed4 original = tex2D(_MainTex, i.uv);
				fixed3 rgb = lerp(original.rgb, fogColor.rgb, fogColor.a*_Multiplier);

				return fixed4(rgb, original.a);

				/*float3 worldPos = _WorldSpaceCameraPos + linearDepth * i.interpolatedRay.xyz;

				float fogDensity = (_FogEnd - worldPos.y) / (_FogEnd - _FogStart);
				fogDensity = saturate(fogDensity * _FogDensity);

				fixed4 finalColor = tex2D(_MainTex, i.uv);
				finalColor.rgb = lerp(finalColor.rgb, _FogColor.rgb, fogDensity);

				return finalColor;*/
			}

			ENDCG
		}						
	}
	FallBack Off
}
