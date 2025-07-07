// simplest DX11 point color shader, singlepass vr support

Shader "UnityCoder/PointCloud/DX11/Color-SinglePass" 
{
	SubShader 
	{
		Tags { "RenderType"="Opaque"}
		Lighting Off
		Pass 
		{
			CGPROGRAM
			#pragma target 4.5
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			StructuredBuffer<half3> buf_Points;
			StructuredBuffer<fixed3> buf_Colors;

			struct _Attributes // appdata
			{
				fixed3 customColor : COLOR;
				uint id : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Attributes 
			{
				half4 pos : SV_POSITION;
				fixed4 customColor : TEXCOORD1;
			    UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Attributes vert(_Attributes input)
			{
				Attributes output = (Attributes)0;
				UNITY_SETUP_INSTANCE_ID(input)
				UNITY_INITIALIZE_OUTPUT(Attributes, output);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				output.pos = UnityWorldToClipPos(half4(buf_Points[input.id],1.0f));
				fixed3 col = buf_Colors[input.id];
				#if !UNITY_COLORSPACE_GAMMA
				col = col*col; // linear
				#endif
				output.customColor = fixed4(col,1);
				return output;
			}

			float4 frag (Attributes i) : SV_Target
			{
				return i.customColor;
			}
			ENDCG
		}
	}
}