// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/OutlineOneColor" {
/*Properties{
	_Color("Color", Color) = (0.5,0.5,0.5,1)
}*/
SubShader {
    Pass {
        Fog { Mode Off }
CGPROGRAM
#pragma vertex vert
#pragma fragment frag

//uniform float4 _Color;

// vertex input: position, UV
struct appdata {
    float4 vertex : POSITION;
};

struct v2f {
    float4 pos : SV_POSITION;
};
v2f vert (appdata v) {
    v2f o;
    o.pos = UnityObjectToClipPos( v.vertex );
    return o;
}
half4 frag( v2f i ) : COLOR {
    return half4(1,1,1,1);
}
ENDCG
    }
}
}