  Shader "CrossSection/Legacy/Transparent/Diffuse" {
    Properties {
   	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
      
 }

    
SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	LOD 400
	
		//Zwrite Off
		CGPROGRAM
		#pragma surface surf Lambert alpha
		#pragma exclude_renderers flash
		#pragma debug

		sampler2D _MainTex;
		fixed4 _Color;
		
		#pragma multi_compile_fragment __ CLIP_PLANE 
		 
		#include "../../crossSection/shaderIncludes/section_clipping_CS.cginc"

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			SECTION_CLIP(IN.worldPos);
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = tex.rgb * _Color.rgb;
			o.Alpha = tex.a * _Color.a;
		}
		ENDCG
		
    } 

FallBack "Transparent/Cutout/VertexLit"
  }