// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/GlobalRampFog" 
{
	Properties 
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	uniform sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;

	// 这个东西，xy 是 distance 的开始和结束，zw是 height 的开始和结束。
	uniform half4 _DistanceParams;

	// for fast world space reconstruction
	uniform float4x4 _FrustumCornersWS;
	uniform float4 _CameraWS;
	uniform sampler2D _DepthTex;
	uniform sampler2D _RampTexture0;
	uniform sampler2D _RampTexture1;
	uniform float _DistanceFogMultiplier;
	uniform float _HeightFogMultiplier;

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv_depth : TEXCOORD1;
		float4 interpolatedRay : TEXCOORD2;
	};

	v2f vert(appdata_img v)
	{
		v2f o;
		// vertex 的 z 是表示顶点的索引，在 C# 中设置的，这里读出来之后，重新给一个正常的 z 值。
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
		fixed4 original = tex2D(_MainTex, i.uv);
		float rawDepth = SAMPLE_DEPTH_TEXTURE(_DepthTex, i.uv_depth);

		fixed3 result = original.rgb;

#ifdef _DISTANCE
		// distance fog:
		float linearDepth = LinearEyeDepth(rawDepth);

		float distanceFogFactor = Remap(linearDepth, _DistanceParams.x, _DistanceParams.y, 0, 1);
		fixed4 distanceFogColor = tex2D(_RampTexture0, float2(distanceFogFactor, 0.5f));

		result = lerp(result, distanceFogColor.rgb, saturate(distanceFogColor.a*_DistanceFogMultiplier));
#endif

#ifdef _HEIGHT
		//return fixed4(rgb, original.a);

		// height fog:
		// Reconstruct world space position & direction
		// towards this screen pixel.
		float dpth = Linear01Depth(rawDepth);
		float4 wsDir = dpth * i.interpolatedRay;
		float4 wsPos = _CameraWS + wsDir;

		float heightFogFactor = Remap(wsPos.y, _DistanceParams.z, _DistanceParams.w, 0, 1);
		fixed4 heightFogColor = tex2D(_RampTexture1, float2(heightFogFactor, 0.5f));

		result = lerp(result, heightFogColor.rgb, saturate(heightFogColor.a*_HeightFogMultiplier));
#endif

		return fixed4(result.rgb, original.a);
	}

	ENDCG

	SubShader 
	{
		// 使用这个 shader 的 quad 的四个顶点是定制的，所以这里要加上 ZTest Always 和 Cull Off
		ZTest Always 
		Cull Off 
		ZWrite Off 
		Fog{ Mode Off }

		Pass
		{
			CGPROGRAM
			
			#pragma multi_compile _DISTANCE __
			#pragma multi_compile _HEIGHT __

			#pragma vertex vert
			#pragma fragment frag
		
			ENDCG
		}						
	}
	FallBack Off
}
