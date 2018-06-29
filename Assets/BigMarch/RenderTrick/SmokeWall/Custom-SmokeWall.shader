// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/SmokeWall"
{
	Properties
	{
		[NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}
		_TintColor("TintColor", Color) = (0, 0, 0, 0)
		_FlowSpeed("FlowSpeed(xy是第一层uv的滚动速度，zw是第二层uv的滚动速度)", Vector) = (0, -0.2, 0, -0.15)
		_FlowTiling("FlowSpeed(xy是第一层uv的tiling，zw是第二层uv的tiling)", Vector) = (1, 1, 1, 0.5)
		
		_DepthAlphaParam("DepthAlphaParam(xy是对应的是顶点到摄像机的距离，zw对应的是alpha)", Vector) = (1, 20, 0.01, 1)
		_VirtualPointOffset("VirtualPointOffset", Float) = 14
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "RenderType"="Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask RGB
		Cull Off 
		Lighting Off 
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 normal : NORMAL;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _TintColor;
			float4 _FlowSpeed;
			float4 _FlowTiling;
			float4 _DepthAlphaParam;
            float _VirtualPointOffset;
            
            float Remap(float val, float iMin, float iMax, float oMin, float oMax)
			{
				return oMin + (val - iMin) * (oMax - oMin) / (iMax - iMin);
			}
			
			v2f vert (appdata v)
			{
				v2f o;

				//两层uv滚动
				o.uv.xyzw = v.uv.xyxy*_FlowTiling.xyzw + _Time.y*_FlowSpeed.xyzw;

				//float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
				//float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - posWorld.xyz);

				//float3 normalWorld = UnityObjectToWorldNormal(v.normal);

				//float nDotV = dot(normalWorld, viewDirection);

				//nDotV = saturate(abs(nDotV));

				o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                
                // 和摄像机的距离乘数
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float3 toCamera = worldPos.xyz - _WorldSpaceCameraPos.xyz; 
				float cameraDistanceFactor = Remap(length(toCamera), _DepthAlphaParam.x, _DepthAlphaParam.y, 0, 1);
				cameraDistanceFactor  = saturate(cameraDistanceFactor);						
						
				// 计算虚拟点的位置
				// 和虚拟点的距离乘数
				float3 cameraFoward = -UNITY_MATRIX_V[2].xyz;
				float3 virtualPos = _WorldSpaceCameraPos.xyz + cameraFoward*_VirtualPointOffset;
				float3 toVirtualPos = worldPos.xyz - virtualPos.xyz;			
				float virtualPointDistanceFactor = Remap(length(toVirtualPos), _DepthAlphaParam.x, _DepthAlphaParam.y, 0, 1);
				virtualPointDistanceFactor  = saturate(virtualPointDistanceFactor);							
						
				// 复杂高端的角度系数
				float d = dot(normalize(toVirtualPos), normalize(toCamera));
				d = Remap(d, -1, 1, 0, 1);
				
				float alphaMultiplier = cameraDistanceFactor * virtualPointDistanceFactor*d;
				alphaMultiplier = Remap(alphaMultiplier, 0, 1,  _DepthAlphaParam.z,  _DepthAlphaParam.w);
				
		        o.color.a *= alphaMultiplier;

				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			    //return fixed4(i.color.rrr, 1);
			    
				// sample the texture
				fixed4 col0 = tex2D(_MainTex, i.uv.xy);
				fixed4 col1 = tex2D(_MainTex, i.uv.zw);
				fixed4 col = col0 * col1 * _TintColor * 2.0f * i.color;
				
				fixed4 colFinal = col;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, colFinal);
				return colFinal;
			}
			ENDCG
		}
	}
}
