Shader "!Debug/Normals_WorldSpace" {
    Properties {
	   }

    SubShader {
      Tags { "RenderType" = "Opaque" }

      CGPROGRAM	  
	  #pragma surface surf Lambert nolightmap noforwardadd
      struct Input {
          float3 worldNormal;
      };
      void surf	  (Input IN, inout SurfaceOutput o) {
          o.Emission = normalize(IN.worldNormal) * 0.5 + 0.5;;
      }

      ENDCG
    } 
    Fallback "Diffuse"
}