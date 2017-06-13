// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// 注意，使用这个shader时，如果背景片儿还是receive shadow的话，那么会导致阴影很重，并且阴影强度无法调控。
// 所以要保证，fake shadow 透明片之后的物体，都关掉receive shadow。
Shader "Custom/FakeShadowForAR" {
    Properties {
		_ShadowStrength("Shadow Strength", Range(0, 1)) = 1  
		_FakeShadowTexture ("Fake Shadow Texture", 2D) = "white" {}
		_FakeShadowAdd("Fake Shadow Add", Range(0, 0.25)) = 0.125
    }
    SubShader {
		Tags {
            "IgnoreProjector"="True"
            "Queue"="Geometry+1"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            //Blend SrcAlpha OneMinusSrcAlpha
            Blend DstColor Zero
            //Blend One Zero
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 2.0
            struct VertexInput {
                float4 vertex : POSITION;
				float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                LIGHTING_COORDS(0,1)
				float2 uv0 : TEXCOORD2;
            };

			uniform float _ShadowStrength;
			uniform float _FakeShadowAdd;
			uniform sampler2D _FakeShadowTexture;

            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos(v.vertex );
				o.uv0 = v.texcoord0;
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
				float4 fakeShadow = saturate(tex2D(_FakeShadowTexture, i.uv0)+_FakeShadowAdd);

////// Lighting:
                float attenuation = lerp(1, LIGHT_ATTENUATION(i), _ShadowStrength);
////// Emissive:
                float3 emissive = float3(attenuation,attenuation,attenuation);
                float3 finalColor = emissive*fakeShadow.rgb;
                return fixed4(finalColor, 1);
            }
            ENDCG
        }
    }
	    FallBack "Diffuse"

}
