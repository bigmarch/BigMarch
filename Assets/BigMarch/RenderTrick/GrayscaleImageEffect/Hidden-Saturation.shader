Shader "Hidden/Saturation" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Saturation("0~grayscal, 1~source color", Float) = 0
}

SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off
				
CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform fixed _Saturation;

fixed4 frag (v2f_img i) : SV_Target
{
	fixed4 original = tex2D(_MainTex, i.uv);
	fixed grayscale = Luminance(original.rgb);

	fixed3 finalRgb = lerp(grayscale.rrr, original.rgb, _Saturation);

	return fixed4(finalRgb, original.a);
}
ENDCG

	}
}

Fallback off

}
