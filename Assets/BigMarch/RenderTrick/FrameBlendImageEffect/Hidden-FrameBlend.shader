Shader "Hidden/FrameBlend" 
{
    Properties 
	{
        _MainTex ("Base", 2D) = "white" {  }
    }

	SubShader 
	{
		//∫·œÚ1œÒÀÿ¿≠…Ï
		Pass {
			Fog { Mode Off }
			ZTest Always
			ZWrite Off
			Blend One Zero
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _BlendTex;
			uniform float _BlendRatio;

			half4 frag(v2f_img i ) : COLOR {
				half4 m = tex2D(_MainTex, i.uv);
				half4 b = tex2D(_BlendTex, i.uv);

				half3 final = lerp(m.rgb, b.rgb, _BlendRatio);

				return half4(final, 1);
			}
			ENDCG
		}
	}
}
