// PointCloud Shader for DX11 Viewer with PointSize and ColorTint

Shader "UnityCoder/PointCloud/DX11/ColorSizeV2 (Height Gradient)"
{
	Properties
	{
//		_Tint("Tint", Color) = (1,1,1,1)
		_Size("Size", Float) = 0.01
		_GradientMap("Gradient Texture", 2D) = "white" {}
		_MaxY("Maximum Y", float) = 50
		_MinY("Minimum Y", float) = 0
	}

		SubShader
	{
		Pass
		{
			Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#pragma target 5.0
			#pragma vertex VS_Main
			#pragma fragment FS_Main
			#pragma geometry GS_Main
			#include "UnityCG.cginc"

			StructuredBuffer<half3> buf_Points;
			StructuredBuffer<fixed3> buf_Colors;

			struct GS_INPUT
			{
				uint id : VERTEXID;
			};

			struct FS_INPUT
			{
				half4	pos		: POSITION;
				fixed3 color : COLOR;
			};

			float _Size;
//			fixed4 _Tint;
			uniform sampler2D _GradientMap;
			uniform float _MaxY;
			uniform float _MinY;

			GS_INPUT VS_Main(uint id : SV_VertexID)
			{
				GS_INPUT o;// = (GS_INPUT)0;
				o.id = id;
				return o;
			}

			[maxvertexcount(4)]
			void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
			{
				uint id = p[0].id;
				float3 pos = buf_Points[id];

				float3 cameraUp = UNITY_MATRIX_IT_MV[1].xyz;
				float3 cameraForward = _WorldSpaceCameraPos - pos;
				float3 rightSize = normalize(cross(cameraUp, cameraForward))*_Size;
				float3 cameraSize = _Size * cameraUp;

				//fixed3 col = buf_Colors[id] * _Tint.rgb;
				float v = (buf_Points[id].y - _MinY) / (_MaxY - _MinY);
				float3 col = tex2Dlod(_GradientMap, float4(v, 0.5f, 0, 0));

//				#if !UNITY_COLORSPACE_GAMMA
//				col = col * col; // linear
//				#endif

				FS_INPUT newVert;
				newVert.pos = UnityObjectToClipPos(float4(pos + rightSize - cameraSize,1));
				newVert.color = col;
				triStream.Append(newVert);
				newVert.pos = UnityObjectToClipPos(float4(pos + rightSize + cameraSize,1));
				triStream.Append(newVert);
				newVert.pos = UnityObjectToClipPos(float4(pos - rightSize - cameraSize,1));
				triStream.Append(newVert);
				newVert.pos = UnityObjectToClipPos(float4(pos - rightSize + cameraSize,1));
				triStream.Append(newVert);
			}

			fixed3 FS_Main(FS_INPUT input) : COLOR
			{
				return input.color;
			}
			ENDCG
		}
	}
}