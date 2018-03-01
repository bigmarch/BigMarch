Shader "Unlit/flag_us"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_FlagColor("flag color", Color) = (1, 0, 0, 1)
		_Frequency("frequency", float) = 1
		_Strength("amplitude strength", float) = 1
		_WaveLength("wave length", float) = 1
			}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _FlagColor;
			float _Frequency;
			float _Strength;
			float _WaveLength;
			v2f vert(appdata v)
			{
				v2f o;
				o.uv = v.uv;
				// 计算偏移之前的顶点位置
				//float4 v_before = mul(unity_ObjectToWorld, v.vertex);

				// 为时间加上顶点位置的x+z*0.6(0.6是自己喜欢的系数，可以自行调节效果修改)作为sin的输入，乘以系数之后将最后的结果作为顶点偏移值
				half offset = sin(_Time.y*_Frequency + v.vertex.x + v.vertex.z*0.4)*_WaveLength*o.uv.x;
				v.vertex.xyz += _Strength*float3(0, offset, 0);
				o.vertex = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//fixed4 col = fixed4(_FlagColor.rgb, 1);
				fixed4 col = tex2D(_MainTex, i.uv)*_FlagColor.rgba;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}