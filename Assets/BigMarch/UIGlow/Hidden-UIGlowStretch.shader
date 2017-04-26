Shader "Hidden/UIGlowStretch" {
    Properties {
        _MainTex ("Base", 2D) = "white" {  }
    }


CGINCLUDE
	static const half curve7[7] = { 0.0205, 0.0855, 0.232, 0.324, 0.232, 0.0855, 0.0205 };  // gauss'ish blur weights
	static const half curve5[5] = { 0.0205, 0.3175, 0.324, 0.3175, 0.0205 };  // gauss'ish blur weights
	static const half curve3[3] = { 0.106, 0.788, 0.106 };  // gauss'ish blur weights
ENDCG

SubShader {
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
uniform float4 _ScreenSize;
half4 frag(v2f_img i ) : COLOR {
	half4 c=tex2D(_MainTex,i.uv);
	half4 c1=tex2D(_MainTex,float2(i.uv.x-_ScreenSize.x,i.uv.y));
	half4 c2=tex2D(_MainTex,float2(i.uv.x+_ScreenSize.x,i.uv.y));

	half4 pow = c*curve3[1] + c1 *curve3[0] + c2*curve3[2];
	return pow;
}
ENDCG
    }




//◊›œÚ1œÒÀÿ¿≠…Ï
Pass {
	ZTest Always Cull Off ZWrite Off

CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform float4 _ScreenSize;
half4 frag(v2f_img i ) : COLOR {
	
	half4 c=tex2D(_MainTex,i.uv);	
	half4 c1=tex2D(_MainTex,float2(i.uv.x,i.uv.y-_ScreenSize.y));
	half4 c2=tex2D(_MainTex,float2(i.uv.x,i.uv.y+_ScreenSize.y));

	half4 pow = c*curve3[1] + c1 *curve3[0] + c2*curve3[2];
	return pow;
}
ENDCG
    }

		//∫·œÚ2œÒÀÿ¿≠…Ï°£
Pass {
	ZTest Always Cull Off ZWrite Off

CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform float4 _ScreenSize;

half4 frag(v2f_img i ) : COLOR {
	half4 c = tex2D(_MainTex, i.uv);
	half4 c1 = tex2D(_MainTex, float2(i.uv.x - _ScreenSize.x, i.uv.y));
	half4 c2 = tex2D(_MainTex, float2(i.uv.x + _ScreenSize.x, i.uv.y));
	half4 c3 = tex2D(_MainTex, float2(i.uv.x - _ScreenSize.x*2.0f, i.uv.y));
	half4 c4 = tex2D(_MainTex, float2(i.uv.x + _ScreenSize.x*2.0f, i.uv.y));

	half4 pow = c*curve5[2] + c1*curve5[1] + c2*curve5[3] + c3*curve5[0] + c4*curve5[4];
	return pow;
}
ENDCG
    }

//◊›œÚ2œÒÀÿ¿≠…Ï
Pass {
	ZTest Always Cull Off ZWrite Off

CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform float4 _ScreenSize;

half4 frag(v2f_img i ) : COLOR {
	//return half4(0,1,0,1);
	half4 c = tex2D(_MainTex, i.uv);
	half4 c1 = tex2D(_MainTex, float2(i.uv.x, i.uv.y - _ScreenSize.y));
	half4 c2 = tex2D(_MainTex, float2(i.uv.x, i.uv.y + _ScreenSize.y));
	half4 c3 = tex2D(_MainTex, float2(i.uv.x, i.uv.y - _ScreenSize.y*2.0f));
	half4 c4 = tex2D(_MainTex, float2(i.uv.x, i.uv.y + _ScreenSize.y*2.0f));

	half4 pow = c*curve5[2] + c1*curve5[1] + c2*curve5[3] + c3*curve5[0] + c4*curve5[4];
	return pow;
}
ENDCG
    }
	 

}
}
