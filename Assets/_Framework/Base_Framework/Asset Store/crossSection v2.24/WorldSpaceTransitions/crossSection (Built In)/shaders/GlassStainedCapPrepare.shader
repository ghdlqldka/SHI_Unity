// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Per pixel bumped refraction.
// Uses a normal map to distort the image behind, and
// an additional texture to tint the color.

Shader "CrossSection/FXGlass/CapPrepare" 
{
	Properties 
	{
		_BumpAmt  ("Distortion", range (0,128)) = 10
		_MainTex ("Tint Color (RGB)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}

		[HideInInspector][Toggle] _inverse("inverse", Float) = 0
		_StencilMask("Stencil Mask", Range(0, 255)) = 0
		_ReadMask ("ReadMask", Range(0, 255)) = 255
		_WriteMask ("WriteMask", Range(0, 255)) = 255
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilPassFront("PassFront", Int) = 1
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilPassBack("PassBack", Int) = 2
		[Enum(Off,0,On,1)] _CapPrepareZWrite("CapPrepareZWrite", Int) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _CapPrepareZTest("CapPrepareZTest", Int) = 4
	}


	SubShader 
	{
		Tags { "RenderType" = "Clipping" "Queue"="Transparent" }

		// This pass grabs the screen behind the object into a texture.
		// We can access the result in the next pass as _GrabTexture
		GrabPass 
		{
			Name "GRAB"
			Tags { "LightMode" = "Always" }
		}

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
		
		// Main pass: Take the texture grabbed above and use the bumpmap to perturb it
		// on to the screen

		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			#pragma multi_compile_fragment __ CLIP_PLANE CLIP_PIE CLIP_SPHERE CLIP_CORNER CLIP_TUBES CLIP_BOX
		 
			#include "../../crossSection/shaderIncludes/section_clipping_CS.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord: TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 uvgrab : TEXCOORD0;
				float2 uvbump : TEXCOORD1;
				float2 uvmain : TEXCOORD2;
				float3 wpos: TEXCOORD4;
				UNITY_FOG_COORDS(3)
			};

			float _BumpAmt;
			float4 _BumpMap_ST;
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 worldPos = mul (unity_ObjectToWorld, v.vertex).xyz;
				o.wpos = worldPos;
				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif
				o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
				o.uvgrab.zw = o.vertex.zw;
				o.uvbump = TRANSFORM_TEX( v.texcoord, _BumpMap );
				o.uvmain = TRANSFORM_TEX( v.texcoord, _MainTex );
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;
			sampler2D _BumpMap;
			sampler2D _MainTex;

			half4 frag (v2f i) : SV_Target
			{
				SECTION_CLIP(i.wpos);
	
				#if UNITY_SINGLE_PASS_STEREO
				i.uvgrab.xy = TransformStereoScreenSpaceTex(i.uvgrab.xy, i.uvgrab.w);
				#endif

				// calculate perturbed coordinates
				half2 bump = UnpackNormal(tex2D( _BumpMap, i.uvbump )).rg; // we could optimize this by just reading the x & y without reconstructing the Z
				float2 offset = bump * _BumpAmt * _GrabTexture_TexelSize.xy;
				#ifdef UNITY_Z_0_FAR_FROM_CLIPSPACE //to handle recent standard asset package on older version of unity (before 5.5)
					i.uvgrab.xy = offset * UNITY_Z_0_FAR_FROM_CLIPSPACE(i.uvgrab.z) + i.uvgrab.xy;
				#else
					i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
				#endif

				half4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
				half4 tint = tex2D(_MainTex, i.uvmain);
				col *= tint;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}

	}

	// ------------------------------------------------------------------
	// Fallback for older cards and Unity non-Pro

	SubShader 
	{
		Blend DstColor Zero
		Pass {
			Name "BASE"
			SetTexture [_MainTex] {	combine texture }
		}
	}
}

