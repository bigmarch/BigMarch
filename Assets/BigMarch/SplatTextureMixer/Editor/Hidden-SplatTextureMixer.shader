// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/SplatTextureMixer" 
{
	Properties
	{
		_MainTex("MainTex", 2D) = "black" {  }
		_Splat0("Splat0", 2D) = "balck" {}
		_Splat1("Splat1", 2D) = "balck" {}
		_Splat2("Splat2", 2D) = "balck" {}
		_Splat3("Splat3", 2D) = "balck" {}

		_Tiling0("Tiling0", Float) = 1
		_Tiling1("Tiling1", Float) = 1
		_Tiling2("Tiling2", Float) = 1
		_Tiling3("Tiling3", Float) = 1

	}

	SubShader
	{
		Pass 
		{
			Fog { Mode Off }
			ZTest Always
			ZWrite Off
			Blend One Zero
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _NORMAL __
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			uniform sampler2D _Splat0;
			uniform float _Tiling0;
			uniform sampler2D _Splat1;
			uniform float _Tiling1;
			uniform sampler2D _Splat2;
			uniform float _Tiling2;
			uniform sampler2D _Splat3;
			uniform float _Tiling3;

			struct v2f_simple
			{
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
			};

			v2f_simple vert(appdata_img v)
			{
				v2f_simple o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0.0)
					o.uv.y = 1.0 - o.uv.y;
#endif
				return o;
			}

			half4 frag(v2f_simple i) : COLOR
			{
				half4 _MainTex_var = tex2D(_MainTex, i.uv);
				half4 _Splat0_var = tex2D(_Splat0, i.uv*_Tiling0);
				half4 _Splat1_var = tex2D(_Splat1, i.uv*_Tiling1);
				half4 _Splat2_var = tex2D(_Splat2, i.uv*_Tiling2);
				half4 _Splat3_var = tex2D(_Splat3, i.uv*_Tiling3);
#ifdef _NORMAL
				float3 mixNormal = _MainTex_var.r*UnpackNormal(_Splat0_var)
					+ _MainTex_var.g*UnpackNormal(_Splat1_var)
					+ _MainTex_var.b*UnpackNormal(_Splat2_var)
					+ _MainTex_var.a*UnpackNormal(_Splat3_var);

				float4 mixColor = float4(mixNormal.rgb*0.5 + float3(0.5, 0.5, 0.5), 1);
#else
				float4 mixColor = _MainTex_var.r*_Splat0_var
					+ _MainTex_var.g*_Splat1_var
					+ _MainTex_var.b*_Splat2_var
					+ _MainTex_var.a*_Splat3_var;
#endif
				return mixColor;
			}
			ENDCG
		}
	}
}
