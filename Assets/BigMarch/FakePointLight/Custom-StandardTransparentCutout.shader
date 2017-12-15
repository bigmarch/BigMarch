Shader "Custom/StandardTransparentCutout"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		[NoScaleOffset]_MainTex("Albedo", 2D) = "white" {}
		_Color("AlbedoColor", Color) = (1,1,1,1)
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[NoScaleOffset]_Metallic("Metallic", 2D) = "white" {}
		_Glossiness("Smoothness", Range( 0 , 1)) = 0.5
		[Normal][NoScaleOffset]_BumpMap("Normal", 2D) = "bump" {}
		[NoScaleOffset]_EmissionMap("Emission", 2D) = "white" {}
		_EmissionColor("EmissionColor", Color) = (0,0,0,0)

		[Space][Header(stencil)]
		_Stencil ("Stencil ID", Float) = 0
	}

	SubShader
	{
			Stencil
		{
			Ref [_Stencil]
			Comp always
			Pass replace
			//ZFail keep
		}

		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Back

		CGPROGRAM
		#pragma multi_compile _USE_FAKEPOINTLIGHT __
		#pragma multi_compile _ LOD_FADE_CROSSFADE
		#pragma target 3.0
		#pragma exclude_renderers xbox360 xboxone ps4 psp2 n3ds wiiu 
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows novertexlights vertex:vertexDataFunc fade
		struct Input
		{
			float2 texcoord_0;
			half3 worldPos;
			half3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _BumpMap;
		uniform half4 _Color;
		uniform sampler2D _MainTex;
		uniform sampler2D _EmissionMap;
		uniform half4 _EmissionColor;
		uniform sampler2D _Metallic;
		uniform half _Glossiness;
		uniform float _Cutoff = 0.5;

		uniform half4 _Global_FakePointLightPosition;
		uniform half4 _Global_FakePointLightColor;
		uniform half _Global_FakePointLightStrength;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.texcoord_0.xy = v.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Normal = UnpackNormal( tex2D( _BumpMap,i.texcoord_0) ).xyz;
			half4 tex2DNode1 = tex2D( _MainTex,i.texcoord_0);

			#if LOD_FADE_CROSSFADE
			o.Albedo = (unity_LODFade.x * _Color * tex2DNode1 ).xyz;
			#else
			o.Albedo = ( _Color * tex2DNode1 ).xyz;
			#endif

			o.Emission = ( tex2D( _EmissionMap,i.texcoord_0) * _EmissionColor ).rgb;

#if _USE_FAKEPOINTLIGHT
			// fake point light
			half3 posDelta = _Global_FakePointLightPosition.xyz - i.worldPos.xyz;
			half nDotL = dot(WorldNormalVector(i, o.Normal).rgb, posDelta);
			float dist = distance(i.worldPos.xyz, _Global_FakePointLightPosition.xyz);
			float pointLightStrength = 1 - saturate(dist / _Global_FakePointLightPosition.w);
			pointLightStrength *= saturate(nDotL);
			o.Emission += pointLightStrength.r*_Global_FakePointLightColor*_Global_FakePointLightStrength;
#endif

			half4 tex2DNode4 = tex2D( _Metallic,i.texcoord_0);
			o.Metallic = tex2DNode4.xyz;
			o.Smoothness = ( tex2DNode4.a * _Glossiness );
			o.Alpha = 1;
			float temp_output_30_0 = ( _Color.a * tex2DNode1.a );
			clip( temp_output_30_0 - _Cutoff );

		}

		ENDCG
	}
	Fallback "Diffuse"
}