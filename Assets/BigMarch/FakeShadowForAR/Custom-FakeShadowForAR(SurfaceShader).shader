// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom-FakeShadowForAR(SurfaceShader)"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_Albedo("Albedo", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags{   
			"IgnoreProjector"="True"
            "Queue"="Geometry+1"
            "RenderType"="Transparent"}

		Blend DstColor Zero
        ZWrite Off
		
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit 
		struct Input
		{
			fixed filler;
		};

		uniform float4 _Albedo;

		inline fixed4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return fixed4 (atten.xxx, 1);
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			//o.Alpha = _Albedo.a;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
	FallBack "Diffuse"
}
/*ASEBEGIN
Version=6104
2065;29;1669;996;1280.1;385.8;1;True;True
Node;AmplifyShaderEditor.ColorNode;2;-666.5,239;Float;False;Property;_Color;Color;0;0;1,1,1,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;5;-312.1,-33.79999;Float;False;Property;_Albedo;Albedo;0;0;1,1,1,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-440.1,102.2;Float;False;0;COLOR;0.0,0,0,0;False;1;COLOR;0.0;False;COLOR
Node;AmplifyShaderEditor.SamplerNode;3;-750.1,-84.79999;Float;True;Property;_MainTex;MainTex;1;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;Float;ASEMaterialInspector;0;Unlit;TestShadow;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Transparent;0.5;True;False;0;False;Transparent;Transparent;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;Relative;0;;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;13;OBJECT;0.0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False
WireConnection;4;0;3;0
WireConnection;4;1;2;0
WireConnection;0;9;5;4
ASEEND*/
//CHKSM=0503F112087A9ED8E5E4B30229125ECDE7CCBEBD