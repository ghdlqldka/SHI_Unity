// PointCloud Shader for DX11 Viewer with PointSize and ColorTint

Shader "UnityCoder/PointCloud/DX11/ColorSizeV2-Texture"
{
	Properties
	{
		_MainTexture("Texture", 2D) = "white" {}
		_Size("Size", Float) = 0.01
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.66
	}

		SubShader
	{
		Pass
		{
			Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
			 Blend SrcAlpha OneMinusSrcAlpha // z sort issues
			//Blend One One // Additive
			 //Blend OneMinusDstColor One // Soft additive

			LOD 200
			//ZWrite Off

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
				half4 pos : POSITION;
				fixed3 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTexture;
			float _Size;
			fixed _Cutoff;


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

				fixed3 col = buf_Colors[id];
				#if !UNITY_COLORSPACE_GAMMA
				col = col * col; // linear
				#endif

				FS_INPUT newVert;
				newVert.pos = UnityObjectToClipPos(float4(pos + rightSize - cameraSize,1));
				newVert.color = col;
				newVert.uv = float2(0,0);

				triStream.Append(newVert);
				newVert.pos = UnityObjectToClipPos(float4(pos + rightSize + cameraSize,1));
				newVert.uv = float2(1,0);

				triStream.Append(newVert);
				newVert.pos = UnityObjectToClipPos(float4(pos - rightSize - cameraSize,1));
				newVert.uv = float2(0,1);

				triStream.Append(newVert);
				newVert.pos = UnityObjectToClipPos(float4(pos - rightSize + cameraSize,1));
				newVert.uv = float2(1,1);

				triStream.Append(newVert);
			}

			fixed4 FS_Main(FS_INPUT input) : SV_Target
			{
				fixed4 col = tex2D(_MainTexture, input.uv)*float4(input.color,1);
				//clip(col.a - _Cutoff);
				return col;
			}
			ENDCG
		}
	}
}