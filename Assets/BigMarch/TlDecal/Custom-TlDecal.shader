Shader "Custom/TlDecal"
{
	Properties
	{
		_Shadow ("Shadow", 2D) = "white" {}

		//_Color("Color", Color) = (0.17,0.36,0.81,0.0)
		//_Angle("Angle", Range(0, 360)) = 60
		//_Gradient("Gradient", Range(0, 10)) = 0
		//_FalloffTex("FallOff", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="true" "DisableBatching"="true" }
		//LOD 100

		Pass
		{
            //Lighting Off
			//ZWrite On
			//ColorMask RGB
			//Blend SrcAlpha One
			//Blend One Zero
			//Offset -1, -1
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			//#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			//#include"CGIncludes/TLStudioCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD0;
				//UNITY_FOG_COORDS(1)
			};

//			sampler2D _Shadow;

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

			//float4x4 _CurrentInverseVP;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                //COMPUTE_EYEDEPTH(o.screenPos.z);
                
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{			 
//			    return fixed4(1,1,1,1);
			    
			    float depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos));

                float linear01Depth = Linear01Depth(depth); //转换成[0,1]内的线性变化深度值
                float linearEyeDepth = LinearEyeDepth(depth); //转换到摄像机空间

				return fixed4(linear01Depth.xxx, 1);			

				
				//float sceneZ = LinearEyeDepth (depth);
                //float partZ = i.projPos.z;
                
				//float2 screenUV = i.proj.xy / i.proj.w;
                
               /*
                float4 H = float4(i.screenPos.x * 2 - 1, i.screenPos.y * 2 - 1, depth * 2 - 1, 1); //NDC坐标
                float4 D = mul(_CurrentInverseVP, H);
                //将齐次坐标 w 分量变 1 得到世界坐。
                float4 W = D / D.w; 
                
                return fixed4(W.rgb,1);
                
                */
                // 至此，W 是深度图上某一点的世界坐标。
                
                /*float4 pjUV = W * 0.5 + 0.5;
                pjUV = mul(internal_WorldToProjector, pjUV);


				//fixed4 projPos = fixed4(screenUV.x * 2 - 1, screenUV.y * 2 - 1, -depth * 2 + 1, 1);
				fixed4 projPos = fixed4(screenUV.x * 2 - 1, screenUV.y * 2 - 1, depth * 2 - 1, 1);

				projPos = mul(unity_CameraInvProjection, projPos);
				projPos = mul(unity_MatrixInvV, projPos);
				//half4 clippos = mul(internal_WorldToProjectorClip, projPos);
				//clippos /= clippos.w;
				projPos = mul(internal_WorldToProjector, projPos);
				projPos /= projPos.w;

				fixed2 pjUV = projPos.xy*0.5 + 0.5;

				int2 discardAlpha = step(0, pjUV.xy)*step(pjUV.xy, 1);

				fixed4 col = tex2D(_Shadow, pjUV) * _Color;
				col.a = col.a * _Gradient;
				//fixed4 texF = tex2D(_FalloffTex, clippos.xy);

				//col.rgb *= discardAlpha.x*discardAlpha.y;// *texF.g;
				col.rgb *= discardAlpha.x*discardAlpha.y;


				float x = pjUV.x;
				float y = pjUV.y;
				float deg2rad = 0.017453;	// 角度转弧度
											// 根据角度剔除掉不需要显示的部分
											// 大于180度
				if (_Angle > 180) {
					if (y > 0.5 && abs(0.5 - y) >= abs(0.5 - x) / tan((180 - _Angle / 2) * deg2rad))
						discard;// 剔除
				}
				else    // 180度以内
				{
					if (y > 0.5 || abs(0.5 - y) < abs(0.5 - x) / tan(_Angle / 2 * deg2rad))
						discard;
				}

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;*/
			}
			ENDCG
		}
	}
}
