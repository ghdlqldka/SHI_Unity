﻿Shader "CrossSection/Box/Surface" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		_StencilMask("Stencil Mask", Range(0, 255)) = 255
		[HideInInspector][Toggle] _inverse("inverse", Float) = 0
		[Toggle(RETRACT_BACKFACES)] _retractBackfaces("retractBackfaces", Float) = 0 
		[Toggle(VERTEX_COLOR)] _vertexColor("vertexColor", Float) = 0

		//[HideInInspector] _SectionPoint("_SectionPoint", Vector) = (0,0,0,1)	//expose as local properties
		//[HideInInspector] _SectionPlane("_SectionPlane", Vector) = (1,0,0,1)	//expose as local properties
		//[HideInInspector] _SectionPlane2("_SectionPlane2", Vector) = (1,0,0,1)	//expose as local properties

		//[HideInInspector] _SectionCentre("_SectionCentre", Vector) = (0,0,0,1)	//expose as local properties
		//[HideInInspector] _SectionDirX("_SectionDirX", Vector) = (1,0,0,1)	//expose as local properties
		//[HideInInspector] _SectionDirY("_SectionDirY", Vector) = (0,1,0,1)	//expose as local properties
		//[HideInInspector] _SectionDirZ("_SectionDirZ", Vector) = (0,0,1,1)	//expose as local properties
		//[HideInInspector] _SectionScale("_SectionScale", Vector) = (0,0,1,1)	//expose as local properties
	
	}

	CGINCLUDE
	#pragma multi_compile_fragment __ CLIP_BOX CLIP_PIE
	//#pragma multi_compile_local_fragment __ CLIP_BOX CLIP_PIE // to get enumerated keywords as local.
	#pragma shader_feature_local_vertex RETRACT_BACKFACES
	#include "../../crossSection/shaderIncludes/section_clipping_CS.cginc"

	half _BackfaceExtrusion;

	void vert(inout appdata_full v) {
#if RETRACT_BACKFACES
		float3 viewDir = ObjSpaceViewDir(v.vertex);
		float dotProduct = dot(v.normal, viewDir);
		if (dotProduct < 0) {
			float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;
			float3 worldNorm = UnityObjectToWorldNormal(v.normal);
			worldPos -= worldNorm * _BackfaceExtrusion;
			v.vertex.xyz = mul(unity_WorldToObject, float4(worldPos, 1)).xyz;
		}
#endif
	}
	ENDCG

	SubShader {
		Tags {"RenderType" = "Clipping" "Queue" = "Geometry+1"}
		LOD 200

		Stencil
		{
			Ref [_StencilMask]
			CompBack Always
			PassBack Replace

			CompFront Always
			PassFront Zero
		}

		
	Cull Off
	CGPROGRAM
	// Physically based Standard lighting model, and enable shadows on all light types
	#pragma surface surf Standard addshadow vertex:vert
	#pragma shader_feature VERTEX_COLOR
	// Use shader model 3.0 target, to get nicer looking lighting
	//#pragma target 3.0

	sampler2D _MainTex;

	struct Input {
		float2 uv_MainTex;
		float3 worldPos;
#if VERTEX_COLOR
		float4 color: Color;
#endif
	};

	half _Glossiness;
	half _Metallic;
	fixed4 _Color;

	void surf(Input IN, inout SurfaceOutputStandard o) {
		SECTION_CLIP(IN.worldPos);
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color
#if VERTEX_COLOR
			* IN.color;
#else			
			;
#endif
		// Metallic and smoothness come from slider variables
		o.Albedo = c.rgb;
		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;
		o.Alpha = c.a;
	}
	ENDCG


	Cull Off
	ColorMask 0

	CGPROGRAM
	#pragma surface surf NoLighting noambient vertex:vert

	struct Input {
		float3 worldPos;
	};

	fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
	{
		fixed4 c;
		c.rgb = s.Albedo;
		c.a = s.Alpha;
		return c;
	}

	void surf(Input IN, inout SurfaceOutput o)
	{
		SECTION_INTERSECT(IN.worldPos);
		o.Albedo = float3(1,1,1);
		o.Alpha = 1;
	}
	ENDCG

	}
	FallBack "Diffuse"
	
}
