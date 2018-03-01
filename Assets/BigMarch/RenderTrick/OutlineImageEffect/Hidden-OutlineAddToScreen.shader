// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/OutlineAddToScreen" {
	Properties{
		_MainTex("Base", 2D) = "black" {  }
		_ClipTex0("Base", 2D) = "black" {  }
		_SourceTex("Base", 2D) = "black" {  }
		_ClipTex1("Base", 2D) = "black" {  }
		_Color("Color", Color) = (1,0,0,1)
		_ColorMul("ColorMul", Color) = (1,0,0,1)
	}

SubShader {
    Pass {
        Fog { Mode Off }
		ZTest Always
		ZWrite Off
		Blend One Zero
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform half4 _MainTex_TexelSize;
uniform sampler2D _ClipTex0;
uniform sampler2D _SourceTex;
uniform sampler2D _ClipTex1;
uniform half4 _Color;
uniform half _ColorMul;

struct v2f_simple
{
	float4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
#if UNITY_UV_STARTS_AT_TOP
	half2 uv2 : TEXCOORD1;
#endif
};

v2f_simple vert(appdata_img v)
{
	v2f_simple o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = v.texcoord;
#if UNITY_UV_STARTS_AT_TOP
	o.uv2 = v.texcoord;
	if (_MainTex_TexelSize.y < 0.0)
		o.uv2.y = 1.0 - o.uv2.y;
#endif
	return o;
}

half4 frag(v2f_simple i ) : COLOR {
	half4 mainColor = tex2D(_MainTex,i.uv);
	half2 correctUv = i.uv;
#if UNITY_UV_STARTS_AT_TOP
	correctUv.y = i.uv2.y;
#endif
	half4 clip0 = tex2D(_ClipTex0, correctUv);
	half4 source = tex2D(_SourceTex, correctUv);
	half4 clip1 = tex2D(_ClipTex1, correctUv);
	//return source;
	//return clip1;
	//return clip0;

	// 把需要被clip掉的像素全变成纯黑。
	// 如果clip>=1，那么返回纯黑色。如果为0，那么就是source的颜色。
	half clip = clip0.r + clip1.r;
	half outline = lerp(source, half3(0, 0, 0), clip).r;
	//return half4(result.rgb, 1);

	// sourceColor对应else分支。
	// 把边儿的颜色和背景色做混合。
	half lerpK = saturate(outline)*_Color.a*_ColorMul;
	half4 color = half4(_Color.rgb, 1);
	return lerp(mainColor, color, lerpK);	
}
ENDCG
    }
}
}
