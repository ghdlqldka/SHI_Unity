// use with packed color data in v3 tiles viewer

Shader "UnityCoder/PointCloud/DX11/ColorSizeV3-packed-lite"
{
	Properties
	{
		_CutoutTexture ("Cutout Texture (A)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.33

		_Size("Size", Float) = 1.0
		_Offset("Offset", Vector) = (0,0,0,0)
		[KeywordEnum(Off, On)] _UseTexture("Use Cutout Texture",float) = 0
		[KeywordEnum(Off, On)] _Square("Force Square",float) = 0
		[KeywordEnum(Off, On)] _Fog ("Fog",float) = 0
		[KeywordEnum(Off, On)] _Gradient ("Intensity Gradient",float) = 0
		[KeywordEnum(Off, On)] _SizeByDistance("SizeByDistance",float) = 0
		_GradientMap("Gradient Texture", 2D) = "white" {}
		_MinDist("Min Distance", float) = 0
		_MaxDist("Max Distance", float) = 100
		_MinSize("Min Size", float) = 0.01
		_MaxSize("Max Size", float) = 0.1
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
			#pragma multi_compile _USETEXTURE_ON _USETEXTURE_OFF
			#pragma multi_compile _SQUARE_ON _SQUARE_OFF
			#pragma multi_compile _FOG_ON _FOG_OFF
			#pragma multi_compile _GRADIENT_ON _GRADIENT_OFF
			#pragma multi_compile _SIZEBYDISTANCE_ON _SIZEBYDISTANCE_OFF
			#ifdef _FOG_ON
			#pragma multi_compile_fog
			#endif
			

			#include "UnityCG.cginc"
			#pragma fragmentoption ARB_precision_hint_fastest

			StructuredBuffer<half3> buf_Points;

			struct appdata
			{
				//fixed3 color : COLOR;
			};

			struct GS_INPUT
			{
				uint id : VERTEXID;
			};

			struct FS_INPUT
			{
				half4 pos : POSITION;
				fixed3 color : COLOR;

				//#ifdef _SQUARE_ON
				#if defined(_SQUARE_ON) || defined(_USETEXTURE_ON)
				float2 uv : TEXCOORD0;
				#endif

				#ifdef _FOG_ON
				float3 fogCoord : TEXCOORD1;
				#endif
			};

			float _Size;
			float _GridSizeAndPackMagic;
			float4 _Offset;

			#ifdef _GRADIENT_ON
			sampler2D _GradientMap;
			#endif

			#ifdef _USETEXTURE_ON
			sampler2D _CutoutTexture;
			fixed _Cutoff;
			#endif

			#ifdef _SIZEBYDISTANCE_ON
			float _MinDist, _MaxDist;
			float _MinSize, _MaxSize;
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

			[maxvertexcount(3)]
			void GS_Main(point GS_INPUT po[1], inout TriangleStream<FS_INPUT> triStream)
			{
				uint id = po[0].id;
				float3 rawpos = buf_Points[id];
				float2 xr = SuperUnpacker(rawpos.x);
				float2 yg = SuperUnpacker(rawpos.y);
				float2 zb = SuperUnpacker(rawpos.z);
				float3 p = float3(xr.y + _Offset.x, yg.y + _Offset.y, zb.y + _Offset.z);

				// restore colors a bit TODO does this cause overflow in some cases with linear?
				fixed3 col = fixed3(saturate(xr.x), saturate(yg.x), saturate(zb.x)) * 1.02; 
				#ifdef _GRADIENT_ON
				col.rgb = tex2Dlod(_GradientMap, float4(col.r, 0.5f, 0, 0)).rgb;
				#endif
				// TODO draw tree shapes, for certain color index!

				#if !UNITY_COLORSPACE_GAMMA
				col = col * col; // linear
				#endif

				#ifdef _SIZEBYDISTANCE_ON
				float dist = distance(p, _WorldSpaceCameraPos);
				//_Size = _MinSize + (dist - _MinDist) * (_MaxSize - _MinSize) / (_MaxDist - _MinDist);
				float t = (dist - _MinDist) / (_MaxDist - _MinDist);
				t = clamp(t,0,1);
				//_Size = _MinSize + (_MaxSize - _MinSize) * t;
				_Size = lerp(_MinSize,_MaxSize,t);
				//col = lerp(fixed3(1, 0, 0), fixed3(0, 0, 1), t);
 				#endif

				float3 cameraUp = UNITY_MATRIX_IT_MV[1].xyz;
				float3 cameraForward = _WorldSpaceCameraPos - p;
				float3 rightSize = normalize(cross(cameraUp, cameraForward)) * _Size;
				float3 cameraSize = _Size * cameraUp;

				FS_INPUT newVert;
				
				#if defined(_SQUARE_ON) || defined(_USETEXTURE_ON)
				// top right
				newVert.pos = UnityObjectToClipPos(float4(p + rightSize + cameraSize, 1));
				newVert.uv = float2(0, 1);
				// test
				//newVert.color = float4(1,0,0,1);
				#else
				// top middle
				newVert.pos = UnityObjectToClipPos(float4(p + cameraSize, 1));
				#endif
				newVert.color = col;
				// test
				//newVert.color = float4(1,0,0,1);

				#ifdef _FOG_ON
				newVert.fogCoord = 0;			
				UNITY_TRANSFER_FOG(newVert,newVert.pos);
				#endif
				triStream.Append(newVert);
				
				// bottom right
				newVert.pos = UnityObjectToClipPos(float4(p - rightSize - cameraSize, 1));
#if defined(_SQUARE_ON) || defined(_USETEXTURE_ON)
				newVert.uv = float2(1, 0);
#endif
				// test
				//newVert.color = float4(0,1,0,1);

				triStream.Append(newVert);

				// bottom left
				newVert.pos = UnityObjectToClipPos(float4(p + rightSize - cameraSize, 1));
#if defined(_SQUARE_ON) || defined(_USETEXTURE_ON)
				newVert.uv = float2(0, 0);
#endif
				// test
				//newVert.color = float4(0,0,1,1);

				triStream.Append(newVert);
			}

			fixed3 FS_Main(FS_INPUT i) : SV_Target
			{
				float a = 1;
				#ifdef _USETEXTURE_ON
				a = tex2D(_CutoutTexture, i.uv*2).a;
				clip(a - _Cutoff);
				#endif

				#ifdef _SQUARE_ON
				// clip triangle
				clip(-step(0.5,i.uv.x)- step(0.5,i.uv.y));
				#endif

				#ifdef _USETEXTURE_ON
				// clip triangle
				clip(-step(0.5,i.uv.x)- step(0.5,i.uv.y));
				#endif


				fixed3 col = i.color;
				//fixed3 col = float3(i.uv.xy,0);

				#ifdef _FOG_ON
				UNITY_APPLY_FOG(i.fogCoord, col);
				#endif

				return col;
			}
			ENDCG
		}
	}
}