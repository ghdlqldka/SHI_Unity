// Based on Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// Based on "Autodesk Interactive"

Shader "CrossSection/Autodesk Interactive/CapPrepare"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}

		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Roughness", Range(0.0, 1.0)) = 0.5
        _SpecGlossMap("Roughness Map", 2D) = "white" {}


        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

		_BumpScale("Scale", Float) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}

		_Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
		_ParallaxMap ("Height Map", 2D) = "black" {}

		_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
		_OcclusionMap("Occlusion", 2D) = "white" {}

		_EmissionColor("Color", Color) = (0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}

		_DetailMask("Detail Mask", 2D) = "white" {}

		_DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
		_DetailNormalMapScale("Scale", Float) = 1.0
       [Normal] _DetailNormalMap("Normal Map", 2D) = "bump" {}

		[Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0


		// Blending state
		[HideInInspector] _Mode ("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0

		// CrossSection properties
		//[Toggle(USE_SECTION_COLOR)] _useSectionColor("use section color", Float) = 1
		_SectionColor ("Section Color", Color) = (1,0,0,1)
		//[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Int) = 8
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Int) = 0  //Off
		[Toggle] _inverse("inverse", Float) = 0
		[Toggle] _negativeScale("negative scale", Float) = 0
		[Toggle(RETRACT_BACKFACES)] _retractBackfaces("retractBackfaces", Float) = 0

		_StencilMask("Stencil Mask", Range(0, 255)) = 0
		_ReadMask ("ReadMask", Range(0, 255)) = 255
		_WriteMask ("WriteMask", Range(0, 255)) = 255
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilPassFront("PassFront", Int) = 1
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilPassBack("PassBack", Int) = 2
		[Enum(Off,0,On,1)] _CapPrepareZWrite("CapPrepareZWrite", Int) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _CapPrepareZTest("CapPrepareZTest", Int) = 4

		//[HideInInspector] _SectionCentre("_SectionCentre", Vector) = (0,0,0,1)	//expose as local properties
		//[HideInInspector] _SectionDirX("_SectionDirX", Vector) = (1,0,0,1)	//expose as local properties
		//[HideInInspector] _SectionDirY("_SectionDirY", Vector) = (0,1,0,1)	//expose as local properties
		//[HideInInspector] _SectionDirZ("_SectionDirZ", Vector) = (0,0,1,1)	//expose as local properties
		//[HideInInspector] _SectionScale("_SectionScale", Vector) = (0,0,1,1)	//expose as local properties

	}

	CGINCLUDE
		#define UNITY_SETUP_BRDF_INPUT RoughnessSetup
	ENDCG

	SubShader
	{
		Tags { "RenderType"="Clipping" "PerformanceChecks"="False"}
		LOD 300

		// ------------------------------------------------------------------
		// CapPrepare Pass

        Pass {
			Name "CROSS-SECTION-CAPPREPARE"
			Stencil
			{
				Ref [_StencilMask]
				ReadMask [_ReadMask]
				WriteMask [_WriteMask]
				Comp Always
				PassBack [_StencilPassBack]
				PassFront [_StencilPassFront]
			}
            Cull Off
			ZWrite[_CapPrepareZWrite]
			ZTest[_CapPrepareZTest]
			ColorMask 0
        
            CGPROGRAM
			#pragma multi_compile_fragment __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX
			#pragma shader_feature_local_vertex RETRACT_BACKFACES
			#include "UnityCG.cginc"
			#include "../../crossSection/shaderIncludes/section_clipping_CS.cginc"
            #pragma vertex vert
            #pragma fragment frag
			struct appdata {
                float4 vertex : POSITION;
            };
            struct v2f {
                float4 pos : SV_POSITION;
				float3 wpos : TEXCOORD1;
            };
            
			half _BackfaceExtrusion;

            v2f vert(appdata_full v) {
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			#ifdef RETRACT_BACKFACES
				float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
				float dotProduct = dot(v.normal, viewDir);
				if(dotProduct<0) {
					float3 worldNorm = UnityObjectToWorldNormal(v.normal);
					worldPos -= worldNorm * _BackfaceExtrusion;
					v.vertex.xyz = mul(unity_WorldToObject, float4(worldPos, 1)).xyz;
				}
			#endif
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
				o.wpos = worldPos;
                return o;
            }
            half4 frag(v2f i) : SV_Target {
				#if CLIP_BOX || CLIP_PIE
				SECTION_INTERSECT(i.wpos);
				#else
				SECTION_CLIP(i.wpos);
				#endif
                return half4(1,1,1,1);
            }
            ENDCG
        }

		// ------------------------------------------------------------------
		//  Base forward pass (directional light, emission, lightmaps, ...)
		Pass
		{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" }
			Cull[_Cull] // Cull off
			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

			CGPROGRAM
			#pragma target 3.5

			// -------------------------------------

			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature_fragment _EMISSION
			#pragma shader_feature_local _METALLICGLOSSMAP
			#pragma shader_feature_local _SPECGLOSSMAP
			#pragma shader_feature_local_fragment _DETAIL_MULX2
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF
			#pragma shader_feature_local _PARALLAXMAP

			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

			#pragma vertex vertBase
			#pragma fragment fragBase
			#pragma multi_compile __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX // to get enumerated keywords as local.
			#define USE_SECTION_COLOR 1
			#pragma shader_feature_local_vertex RETRACT_BACKFACES
			#include "CGIncludes/UnityStandardCoreForward_CS.cginc"

			ENDCG
		}

		// ------------------------------------------------------------------
		//  Additive forward pass (one light per pass)
		Pass
		{
			Name "FORWARD_DELTA"
			Tags { "LightMode" = "ForwardAdd" }
			Blend [_SrcBlend] One
			Fog { Color (0,0,0,0) } // in additive pass fog should be black
			ZWrite Off
			ZTest LEqual

			CGPROGRAM
			#pragma target 3.5

			// -------------------------------------

			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature_local _METALLICGLOSSMAP
			#pragma shader_feature_local _SPECGLOSSMAP
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _DETAIL_MULX2
			#pragma shader_feature_local _PARALLAXMAP

			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

			#pragma multi_compile __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX // to get enumerated keywords as local.
			#pragma vertex vertAdd
			#pragma fragment fragAdd

			#include "CGIncludes/UnityStandardCoreForward_CS.cginc"

			ENDCG
		}
		// ------------------------------------------------------------------
		//  Shadow rendering pass
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			Cull[_Cull]
			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 3.5

			// ------------------------------------------------------------------

            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP

            #pragma shader_feature_local _PARALLAXMAP
			#pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing



			#pragma vertex vertShadowCasterClip
			#pragma fragment fragShadowCasterClip

			#pragma multi_compile __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX // to get enumerated keywords as local.

			#include "CGIncludes/UnityStandardShadow_CS.cginc"

			ENDCG
		}
		// ------------------------------------------------------------------
		//  Deferred pass
		Pass
		{
			Name "DEFERRED"
			Tags { "LightMode" = "Deferred" }
			Cull[_Cull]
			CGPROGRAM
			#pragma target 3.0
			#pragma exclude_renderers nomrt


			// -------------------------------------

			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature_fragment _EMISSION
			#pragma shader_feature_local _METALLICGLOSSMAP
			#pragma shader_feature_local _SPECGLOSSMAP
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _DETAIL_MULX2
			#pragma shader_feature_local _PARALLAXMAP

			#pragma multi_compile_prepassfinal
			#pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

			#pragma vertex vertDeferredClip
			#pragma fragment fragDeferredClip

			#pragma multi_compile __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX 
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX // to get enumerated keywords as local.
			#define USE_SECTION_COLOR 1
			#pragma shader_feature_local_vertex RETRACT_BACKFACES

			#include "CGIncludes/UnityStandardCore_CS.cginc"
			//#include "UnityStandardCore.cginc"

			ENDCG
		}

		// ------------------------------------------------------------------
		// Extracts information for lightmapping, GI (emission, albedo, ...)
		// This pass it not used during regular rendering.
		Pass
		{
			Name "META"
			Tags { "LightMode"="Meta" }

            Cull Off

			CGPROGRAM
			#pragma vertex vert_meta
			#pragma fragment frag_meta

			#pragma shader_feature_fragment _EMISSION
			#pragma shader_feature_local _METALLICGLOSSMAP
			#pragma shader_feature_local _SPECGLOSSMAP
			#pragma shader_feature_local_fragment _DETAIL_MULX2
			#pragma shader_feature EDITOR_VISUALIZATION

			#pragma multi_compile __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX// to get enumerated keywords as local.

			//#include "UnityStandardMeta.cginc"
			#include "CGIncludes/UnityStandardMeta_CS.cginc"
			ENDCG
		}
	}

	SubShader
	{
		Tags { "RenderType"="Clipping" "PerformanceChecks"="False" }
		LOD 150

		// ------------------------------------------------------------------
		//  Base forward pass (directional light, emission, lightmaps, ...)
		Pass
		{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" }
			Cull[_Cull] // Cull off
			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

			CGPROGRAM
			#pragma target 2.0

			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature_fragment _EMISSION
			#pragma shader_feature_local _METALLICGLOSSMAP
			#pragma shader_feature_local _SPECGLOSSMAP
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF
			// SM2.0: NOT SUPPORTED shader_feature_local _DETAIL_MULX2
			// SM2.0: NOT SUPPORTED shader_feature_local _PARALLAXMAP

			#pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED

			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog

			#pragma vertex vertBase
			#pragma fragment fragBase
			#pragma multi_compile __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX // to get enumerated keywords as local.
			#define USE_SECTION_COLOR 1
			#pragma shader_feature_local_vertex RETRACT_BACKFACES
			#include "CGIncludes/UnityStandardCoreForward_CS.cginc"

			ENDCG
		}
		// ------------------------------------------------------------------
		//  Additive forward pass (one light per pass)
		Pass
		{
			Name "FORWARD_DELTA"
			Tags { "LightMode" = "ForwardAdd" }
			Blend [_SrcBlend] One
			Fog { Color (0,0,0,0) } // in additive pass fog should be black
			ZWrite Off
			ZTest LEqual

			CGPROGRAM

			#pragma target 2.0

			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature_local _METALLICGLOSSMAP
			#pragma shader_feature_local _SPECGLOSSMAP
			#pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
			// SM2.0: NOT SUPPORTED #pragma shader_feature_local _DETAIL_MULX2
			// SM2.0: NOT SUPPORTED shader_feature_local _PARALLAXMAP
			#pragma skip_variants SHADOWS_SOFT

			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog
			#pragma multi_compile __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX // to get enumerated keywords as local.

			#pragma vertex vertAdd
			#pragma fragment fragAdd

			#include "CGIncludes/UnityStandardCoreForward_CS.cginc"

			ENDCG
		}
		// ------------------------------------------------------------------
		//  Shadow rendering pass
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			Cull[_Cull] // Cull off

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 2.0

            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _SPECGLOSSMAP
			#pragma skip_variants SHADOWS_SOFT
			#pragma multi_compile_shadowcaster

			#pragma vertex vertShadowCasterClip
			#pragma fragment fragShadowCasterClip

			#pragma multi_compile __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX // to get enumerated keywords as local.

			#include "CGIncludes/UnityStandardShadow_CS.cginc"

			ENDCG
		}

		// ------------------------------------------------------------------
		// Extracts information for lightmapping, GI (emission, albedo, ...)
		// This pass it not used during regular rendering.
		Pass
		{
			Name "META"
			Tags { "LightMode"="Meta" }

			Cull Off

			CGPROGRAM
			#pragma vertex vert_meta
			#pragma fragment frag_meta

			#pragma shader_feature_fragment _EMISSION
			#pragma shader_feature_local _METALLICGLOSSMAP
			#pragma shader_feature_local _SPECGLOSSMAP
			#pragma shader_feature_local_fragment _DETAIL_MULX2
			#pragma shader_feature EDITOR_VISUALIZATION
			#pragma multi_compile __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX// to get enumerated keywords as local.

			//#include "UnityStandardMeta.cginc"
			#include "CGIncludes/UnityStandardMeta_CS.cginc"
			ENDCG
		}
	}

	FallBack "VertexLit"
	CustomEditor "CrossSectionAutodeskInteractiveShaderGUI"
}