Shader "Hidden/RadialBlurStrech" 
{
	Properties 
	{
		_MainTex ("Input", RECT) = "white" {}
		_ExpandFactor ("ExpandFactor", Float) = 0.1
		_ExpandCenter("ExpandCenter", Vector) = (0.5, 0.5, 0)
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
			uniform half _ExpandFactor;
			uniform half3 _ExpandCenter;

			#define SAMPLE_COUNT 4

			static const half samples[SAMPLE_COUNT] = 
			{
				1,
				0.9,
				0.8,
				0.7,
			}; 
 	
			half4 frag (v2f_img i) : COLOR
			{		
				//additional samples towards center of screen
				half4 sum = half4(0, 0, 0, 0);
				for(int n = 0; n < SAMPLE_COUNT; n++)
				{
					half2 expandUv = (i.uv - _ExpandCenter.xy)*pow(samples[n], _ExpandFactor) + _ExpandCenter.xy;
					sum += tex2D(_MainTex, expandUv);
				}
       
				//eleven samples...
				sum /= SAMPLE_COUNT;
      
				return half4(sum.rgb, 1);
			}
			ENDCG
        }
    }
}