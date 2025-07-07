// RGB+INT+XYZ Packed v3 format

Shader "UnityCoder/PointCloud/DX11/ColorSizeV3-Int-Packed"
{
	Properties
	{
		_Size("Size", Float) = 1.0
		_Offset("Offset", Vector) = (0,0,0,0)
		[KeywordEnum(Off, On)] _Circle ("Circular Point",float) = 1
		[KeywordEnum(Off, On)] _UseIntensity ("Use Intensity Color",float) = 0
		[KeywordEnum(Off, On)] _Gradient ("Gradient",float) = 0
		_GradientTex ("Gradient Texture", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }
			ZWrite On
			LOD 200
			Cull Off

			CGPROGRAM
			#pragma target 5.0
			#pragma vertex VS_Main
			#pragma fragment FS_Main
			#pragma geometry GS_Main
			#include "UnityCG.cginc"
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile _GRADIENT_ON _GRADIENT_OFF
			#pragma multi_compile _CIRCLE_ON _CIRCLE_OFF
			#pragma shader_feature _USEINTENSITY_ON
			#pragma shader_feature _USEINTENSITY_OFF

			StructuredBuffer<float3> buf_Points;

			struct appdata
			{
				fixed3 color : COLOR;
			};

			struct GS_INPUT
			{
				uint id : VERTEXID;
			};

			struct FS_INPUT
			{
				half4	pos		: POSITION;
				fixed3 color : COLOR;
				#ifdef _CIRCLE_ON
				float2 uv : TEXCOORD0;
				#endif
			};

			float _Size;
			float _GridSizeAndPackMagic;
			float4 _Offset;
			#ifdef _GRADIENT_ON
			sampler2D _GradientTex;
			#endif

			float2 SuperUnpacker(float f)
			{
				return float2(f - floor(f), floor(f) / _GridSizeAndPackMagic);
			}

			GS_INPUT VS_Main(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				GS_INPUT o;
				o.id = id;
				return o;
			}

			float3 UnpackValues(uint packedValue)
			{
				uint r = (packedValue >> 24) & 0xFF;
				uint i = (packedValue >> 16) & 0xFF;
				uint cIntegral = (float)(((packedValue >> 8) &0xFF));
				uint cFractional = (float)(packedValue & 0xFF);
				float fy = ((float)cIntegral + ((float)cFractional/ 255.0f));
				return float3(((float)r)/255, ((float)i)/255, fy);
			}

			[maxvertexcount(4)]
			void GS_Main(point GS_INPUT po[1], inout TriangleStream<FS_INPUT> triStream)
			{
				uint id = po[0].id;
				float3 rawpos = buf_Points[id];
				float2 xr = SuperUnpacker(rawpos.x);
				float3 giy = UnpackValues(asuint(rawpos.y));
				float2 zb = SuperUnpacker(rawpos.z);
				float3 p = float3(xr.y + _Offset.x, giy.z + _Offset.y, zb.y + _Offset.z);
				fixed3 col = 0;

				#ifdef _USEINTENSITY_ON
				float ii = saturate(giy.y);
				col = ii.xxx;// * 1.02;
				#endif
				
				#ifdef _GRADIENT_ON
				col = tex2Dlod(_GradientTex, float4(col[0],0.5f,0,0)).rgb;
				#else
					#if _USEINTENSITY_OFF
					col = fixed3(saturate(xr.x), saturate(giy.x), saturate(zb.x)) * 1.01;
					#endif
				#endif

				#if !UNITY_COLORSPACE_GAMMA
				col = col * col; // linear
				#endif

				float3 cameraUp = UNITY_MATRIX_IT_MV[1].xyz;
				float3 cameraForward = _WorldSpaceCameraPos - p;
				float3 rightSize = normalize(cross(cameraUp, cameraForward)) * _Size;
				float3 cameraSize = _Size * cameraUp;

				FS_INPUT newVert;
				newVert.pos = UnityObjectToClipPos(float4(p + rightSize - cameraSize, 1));
				newVert.color = col;

				#ifdef _CIRCLE_ON
				newVert.uv = float2(0,0);
				#endif
				triStream.Append(newVert);

				newVert.pos = UnityObjectToClipPos(float4(p + rightSize + cameraSize, 1));
				#ifdef _CIRCLE_ON
				newVert.uv = float2(1,0);
				#endif
				triStream.Append(newVert);

				newVert.pos = UnityObjectToClipPos(float4(p - rightSize - cameraSize, 1));
				#ifdef _CIRCLE_ON
				newVert.uv = float2(0,1);
				#endif
				triStream.Append(newVert);

				newVert.pos = UnityObjectToClipPos(float4(p - rightSize + cameraSize, 1));
				#ifdef _CIRCLE_ON
				newVert.uv = float2(1,1);
				#endif
				triStream.Append(newVert);
			}

			#ifdef _CIRCLE_ON
			// source https://thebookofshaders.com/07/
			float circle(float2 _st, float _radius)
			{
				float2 dist = _st-float2(0.5,0.5);
				return 1.-smoothstep(_radius-(_radius*0.01),_radius+(_radius*0.01),dot(dist,dist)*4.0);
			}
			#endif

			fixed3 FS_Main(FS_INPUT input) : SV_Target
			{				
				#ifdef _CIRCLE_ON
				clip(circle(input.uv,0.9) - 0.999);
				#endif
				return input.color;
			}
			ENDCG
		}
	}
}