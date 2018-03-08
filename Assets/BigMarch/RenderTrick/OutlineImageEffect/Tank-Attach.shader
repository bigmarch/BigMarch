Shader "Tank/Attach"
{
	Properties
	{
		[NoScaleOffset]_MainTex("MainTex", 2D) = "white" {}
		[Gamma]_Metallic("Metallic", Range(0, 1)) = 0.5
		_Smoothness("Smoothness", Range(0, 1)) = 1

		[Header(Control by script)]
		_AlbedoMultiplier("AlbedoMultiplier", Color) = (1,1,1,1)
	}

	SubShader
	{
		Stencil
		{
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass[_StencilPass]
			ZFail[_StencilZFail]
		}

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+2" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma exclude_renderers xbox360 xboxone ps4 psp2 n3ds wiiu 
		#pragma surface surf Standard vertex:vert keepalpha addshadow fullforwardshadows exclude_path:deferred 

		struct Input 
		{
			// xy存着顶点uv0，zw存着顶点uv1。
			float2 texcoord;
		};

		void vert (inout appdata_full v, out Input o) 
		{ 
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.texcoord = v.texcoord.xy;
		}

		uniform sampler2D _MainTex;
		uniform half _Metallic;
		uniform half _Smoothness;

		uniform sampler2D _NormalMap0;

		uniform half4 _AlbedoMultiplier;

		void surf(Input i , inout SurfaceOutputStandard o )
		{
			fixed4 _MainTex_var = tex2D(_MainTex, i.texcoord);
			o.Albedo = _MainTex_var.rgb*_AlbedoMultiplier;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness*_MainTex_var.a;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}