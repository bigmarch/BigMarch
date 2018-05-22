Shader "Hidden/ZoomIn" 
{
	Properties 
	{
		_MainTex ("Input", RECT) = "white" {}
		_ZoomInFactor ("ZoomInFactor", Float) = 0.1
		_ZoomInCenter("ZoomInCenter", Vector) = (0.5, 0.5, 0)
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
 
			#include "UnityCG.cginc"
 
			uniform sampler2D _MainTex;
			uniform half _ZoomInFactor;
			uniform half3 _ZoomInCenter;
 	
			half4 frag (v2f_img i) : COLOR
			{		
				half2 expandUv = (i.uv - _ZoomInCenter.xy)* _ZoomInFactor + _ZoomInCenter.xy;
			    half4 result = tex2D(_MainTex, expandUv);      
				return result;
			}
			ENDCG
        }
    }
}