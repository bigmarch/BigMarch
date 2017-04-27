// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "!Debug/Vertex Color" {
SubShader {
    Pass {
        Fog { Mode Off }
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		// vertex input: position, UV
		struct appdata {
			float4 vertex : POSITION;
			float4 color : COLOR;
		};

		struct v2f {
			float4 pos : SV_POSITION;
			float4 color : TEXCOORD0;
		};

		v2f vert (appdata v) {
			v2f o;
			o.pos = UnityObjectToClipPos( v.vertex );
			o.color = v.color;
			return o;
		}
		half4 frag( v2f i ) : COLOR {
			return i.color;
		}
		ENDCG
    }
}
}