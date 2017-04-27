// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "!Debug/UV 2" {
SubShader {
    Pass {
        Fog { Mode Off }
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		// vertex input: position, UV
		struct appdata {
			float4 vertex : POSITION;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
		};

		struct v2f {
			float4 pos : SV_POSITION;
			float4 uv : TEXCOORD0;
			float4 uv1 : TEXCOORD1;
		};

		v2f vert (appdata v) {
			v2f o;
			o.pos = UnityObjectToClipPos( v.vertex );
			o.uv1 = float4( v.texcoord1.xy, 0, 0 );
			return o;
		}
		half4 frag( v2f i ) : COLOR {
			half4 c = frac( i.uv1 );
			if (any(saturate(i.uv1) - i.uv1))
				c.b = 0.5;
			return c;
		}
		ENDCG
    }
}
}