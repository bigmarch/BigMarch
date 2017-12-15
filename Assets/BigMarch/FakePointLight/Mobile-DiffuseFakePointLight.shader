// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Custom/Mobile-DiffuseFakePointLight" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}

	_Color("Main Color", Color) = (1,1,1,1)
	//_Global_FakePointLightPosition("Global_FakePointLightPosition", Vector) = (0, 0, 0, 0.5)
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM

#pragma multi_compile _USE_FAKEPOINTLIGHT __
#pragma multi_compile _ LOD_FADE_CROSSFADE
#pragma surface surf Lambert noforwardadd


sampler2D _MainTex;
uniform half4 _Global_FakePointLightPosition;
uniform half4 _Global_FakePointLightColor;
uniform half _Global_FakePointLightStrength;

uniform half4 _Color;


struct Input {
	float2 uv_MainTex;
	float3 worldPos;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

			#if LOD_FADE_CROSSFADE
			o.Albedo = _Color.rgb * c.rgb * unity_LODFade.x;
			#else
			o.Albedo = _Color.rgb * c.rgb;
			#endif
	o.Alpha = 1;

#if _USE_FAKEPOINTLIGHT

	float dist = distance(IN.worldPos.xyz, _Global_FakePointLightPosition.xyz);
	float pointLightStrength = 1 - saturate(dist / _Global_FakePointLightPosition.w);

	o.Emission += pointLightStrength.r*_Global_FakePointLightColor*_Global_FakePointLightStrength;
#endif
}
ENDCG
}

Fallback "Diffuse"
}
