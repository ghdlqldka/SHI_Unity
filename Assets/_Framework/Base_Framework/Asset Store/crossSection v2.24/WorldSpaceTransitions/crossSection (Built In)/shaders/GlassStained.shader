// Per pixel bumped refraction.
// Uses a normal map to distort the image behind, and
// an additional texture to tint the color.
// with shadow pass

Shader "CrossSection/FXGlass/" {
Properties {
	_Color("Color", Color) = (1,1,1,1)
	_BumpAmt  ("Distortion", range (0,128)) = 10
	_MainTex ("Tint Color (RGB)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	[HideInInspector][Toggle] _inverse("inverse", Float) = 0
	_StencilMask("Stencil Mask", Range(0, 255)) = 255
}

Category {

	// We must be transparent, so other objects are drawn before this one.
	Tags { "Queue"="Transparent" "RenderType"="Clipping" "PerformanceChecks"="False" }


	SubShader {

		// This pass grabs the screen behind the object into a texture.
		// We can access the result in the next pass as _GrabTexture
		GrabPass {
			Name "BASE"
			Tags { "LightMode" = "Always" }
		}
		
		// Main pass: Take the texture grabbed above and use the bumpmap to perturb it
		// on to the screen
		Stencil
		{
			Ref[_StencilMask]
			CompBack Always
			PassBack Replace

			CompFront Always
			PassFront Zero
		}
		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }
			Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#define _ALPHABLEND_ON
			#include "UnityCG.cginc"

			#pragma multi_compile_fragment __ CLIP_PLANE CLIP_CORNER
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
			fixed4 _Color;

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
				half4 tint = tex2D(_MainTex, i.uvmain)*_Color;
				col *= tint;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}

		//  Shadow rendering pass
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			Cull Off
			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 3.0

			// -------------------------------------


			#define _ALPHABLEND_ON
			//#pragma shader_feature _METALLICGLOSSMAP
			//#pragma shader_feature _PARALLAXMAP
			#pragma multi_compile_shadowcaster
			#pragma multi_compile_instancing

			#pragma vertex vertShadowCasterClip
			#pragma fragment fragShadowCasterClip

			#pragma multi_compile_fragment __ CLIP_PLANE CLIP_CORNER

			#include "CGIncludes/UnityStandardShadow_CS.cginc"

			ENDCG
		}



	}

	// ------------------------------------------------------------------
	// Fallback for older cards and Unity non-Pro

	SubShader {
		Blend DstColor Zero
		Pass {
			Name "BASE"
			SetTexture [_MainTex] {	combine texture }
		}
	}
}

}