Shader "Custom/StandardSpecular"
{
	Properties
	{
		[HideInInspector] __dirty("", Int) = 1
		[NoScaleOffset]_MainTex("Albedo", 2D) = "white" {}
		_Color("AlbedoColor", Color) = (1,1,1,1)
		[NoScaleOffset]_SpecGlossMap("Specular", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0 , 1)) = 0.5
		[Normal][NoScaleOffset]_BumpMap("Normal", 2D) = "bump" {}
		[NoScaleOffset]_EmissionMap("Emission", 2D) = "white" {}
		_EmissionColor("EmissionColor", Color) = (0,0,0,0)

		[Space][Header(rim light)]
		[Toggle(_USE_RIMLIGHT)] _UseRimLight("Use Rim Light", Float) = 0
		_RimColor("Rim Color", Color) = (1, 1, 1, 1)
		_RimPower("Rim Power", Range(0.5, 8.0)) = 3.0

		[Space][Header(stencil)]
		_Stencil("Stencil ID", Float) = 0
	}

	SubShader
	{
		Tags{}
		Cull Front

		Stencil
		{
			Ref[_Stencil]
			Comp always
			Pass replace
			//ZFail keep
		}

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma exclude_renderers xbox360 xboxone ps4 psp2 n3ds wiiu
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows novertexlights nolightmap  nodynlightmap nodirlightmap nometa vertex:vertexDataFunc

		#pragma multi_compile _USE_RIMLIGHT __
		#pragma multi_compile _USE_FAKEPOINTLIGHT __

		struct Input
		{
			half2 texcoord_0;
			half3 viewDir;
			half3 worldPos;
			half3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _BumpMap;
		uniform half4 _Color;
		uniform sampler2D _MainTex;
		uniform sampler2D _EmissionMap;
		uniform half4 _EmissionColor;
		uniform sampler2D _SpecGlossMap;
		uniform half _Glossiness;

		float4 _RimColor;
		float _RimPower;

		uniform half4 _Global_FakePointLightPosition;
		uniform half4 _Global_FakePointLightColor;
		uniform half _Global_FakePointLightStrength;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.texcoord_0.xy = v.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
		}

		void surf(Input i , inout SurfaceOutputStandardSpecular o )
		{
			o.Normal = UnpackNormal( tex2D( _BumpMap,i.texcoord_0) ).xyz;
			o.Albedo = ( _Color * tex2D( _MainTex,i.texcoord_0) ).xyz;
			o.Emission = ( tex2D( _EmissionMap,i.texcoord_0) * _EmissionColor ).rgb;
			half4 tex2DNode4 = tex2D( _SpecGlossMap, i.texcoord_0);
			o.Specular = tex2DNode4.xyz;
			o.Smoothness = ( tex2DNode4.a * _Glossiness );
			o.Alpha = 1;

#if _USE_FAKEPOINTLIGHT
			// fake point light
			half3 posDelta = _Global_FakePointLightPosition.xyz - i.worldPos.xyz;
			half nDotL = dot(WorldNormalVector(i, o.Normal).rgb, posDelta);
			float dist = distance(i.worldPos.xyz, _Global_FakePointLightPosition.xyz);
			float pointLightStrength = 1 - saturate(dist / _Global_FakePointLightPosition.w);
			pointLightStrength *= saturate(nDotL);
			o.Emission += pointLightStrength.r*_Global_FakePointLightColor*_Global_FakePointLightStrength;
#endif

#if _USE_RIMLIGHT
			half rim = 1.0 - saturate(dot(normalize(i.viewDir), o.Normal));
			o.Emission += _RimColor.rgb * pow(rim, _RimPower);
#endif
		}
		ENDCG	
	}
	Fallback "Diffuse"
}
