Shader "Hidden/BlendTwoTexture" 
{
	Properties 
	{
		_MainTex ("Input", RECT) = "black" {}
		_BlendTex ("BlendTex", RECT) = "black" {}

		// 使用一个 float 的时候，该属性有用。
		_LerpK("LerpK", Float) = 0.5

		// 使用 mask texture 的时候，该属性有用。
		_Mask ("Mask", RECT) = "black" {}
        _MaskOffset("MaskOffset", Vector) = (0,0,0,0)
		// 使用 caculate 的时候，该属性有用。
		_BlendStrength ("BlendStrength", Float) = 0.5
		_BlendCenter("BlendCenter", Vector) = (0.5, 0.5, 0)
		_NoBlendRadius("NoBlurRadius", Float) = 0.16

		_BlendColorMul("BlendColorMul", Color) = (1, 1, 1, 1)
	}
    SubShader
	{
        Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			
			CGPROGRAM
			
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma shader_feature _VISUALIZE_BLEND
			#pragma multi_compile _BLEND_MASK_TEXTURE _BLEND_LERPK _BLEND_CACULATE
			#include "UnityCG.cginc"
			
			uniform sampler2D _MainTex;
			uniform sampler2D _BlendTex;
			uniform half _LerpK;
			uniform sampler2D _Mask0;
			uniform sampler2D _Mask1;
			uniform half4 _MaskOffset;
			uniform half _MaskBlendK;
			
			uniform half _BlendStrength;
			uniform half3 _BlendCenter;
			uniform half _NoBlendRadius;
			
			uniform half4 _BlendColorMul;
			
			half4 frag (v2f_img i) : COLOR 
			{
				half4 _MainTex_var = tex2D(_MainTex, i.uv);
				half4 _BlendTex_var = tex2D(_BlendTex, i.uv);
       
				half2 dir = _BlendCenter.xy - i.uv;

#ifdef _BLEND_MASK_TEXTURE
                half2 uv = i.uv+_MaskOffset.xy;
				half t0 = tex2D(_Mask0,uv ).r;  
				half t1 = tex2D(_Mask1, uv).r;
				half t = lerp(t0, t1, _MaskBlendK);  
#elif _BLEND_LERPK
				half t = _LerpK;
#elif _BLEND_CACULATE
				//distance to center
				half dist = sqrt(dir.x*dir.x + dir.y*dir.y);

				//weighten blur depending on distance to screen center

				//这里的0.16用来控制到中心点不会模糊的范围
				half t = (dist - _NoBlendRadius) *_BlendStrength;
				//t = pow(dist, _BlendStrength);
				//t = dist * _BlendStrength;
				t = clamp(t, 0.0, 1.0);
				t = 1 - t;
#endif

#ifdef _VISUALIZE_BLEND
				return half4(t.xxx, _MainTex_var.a);
#endif
				half3 rgb = lerp(_BlendTex_var.rgb * _BlendColorMul, _MainTex_var.rgb, t);
				//blend original with blur
				return half4(rgb, _MainTex_var.a);
			}
			ENDCG
        }
    }
}