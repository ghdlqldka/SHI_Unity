Shader "Custom/PointMeshSizeDX11Circle (Packed)"
{
	Properties 
	{
	    _Color ("ColorTint", Color) = (1,1,1,1)
		_Size ("Size", Float) = 30
		_Offset("Offset", Vector) = (0,0,0,0)
	}

	SubShader 
	{
		Tags { "RenderType"="Opaque" "IgnoreProjector" = "True" "Queue"="Geometry"}
		LOD 200

		Pass
		{
			CGPROGRAM
			#pragma vertex VS_Main
			#pragma fragment FS_Main
			#pragma geometry GS_Main
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#pragma multi_compile_fog 
            #include "AutoLight.cginc"
			#include "UnityCG.cginc"
			
			struct appdata
			{
				float4 vertex : POSITION;
				float3 color : COLOR;
			};
		
			struct GS_INPUT
			{
				float4	pos	: POSITION;
				fixed3 color : COLOR;
                SHADOW_COORDS(1)
				UNITY_FOG_COORDS(2)
			};

			struct FS_INPUT
			{
				float4	pos : POSITION;
				float2 uv : TEXCOORD0;
				fixed3 color : COLOR;
                SHADOW_COORDS(1)
				UNITY_FOG_COORDS(2)
			};

			float _Size;
	        fixed4 _Color;
			float _GridSizeAndPackMagic;
			float4 _Offset;

			float2 SuperUnpacker(float f)
			{
				return float2(f - floor(f), floor(f) / _GridSizeAndPackMagic);
			}

			// TODO calculate normals.. for light..


			GS_INPUT VS_Main(appdata v)
			{
				GS_INPUT o = (GS_INPUT)0;
				
				float2 xr = SuperUnpacker(v.vertex.x);
				float2 yg = SuperUnpacker(v.vertex.y);
				float2 zb = SuperUnpacker(v.vertex.z);

				//float4 p = float4(xr.y,yg.y,zb.y,v.vertex.w);
				float3 p = float3(xr.y + _Offset.x, yg.y + _Offset.y, zb.y + _Offset.z);

				o.pos = UnityObjectToClipPos(p);
				fixed3 col = fixed3(saturate(xr.x), saturate(yg.x), saturate(zb.x)) * 1.02;

				//fixed4 col = v.color;
				#if !UNITY_COLORSPACE_GAMMA
				col = col*col; // linear
				#endif
				o.color = col*_Color.xyz;
				return o;
			}

			// source https://thebookofshaders.com/07/
			float circle(float2 _st, float _radius)
			{
				float2 dist = _st-float2(0.5,0.5);
				return 1.-smoothstep(_radius-(_radius*0.01),_radius+(_radius*0.01),dot(dist,dist)*4.0);
			}

			[maxvertexcount(4)]
			void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
			{
				FS_INPUT newVert;
				float width = _Size*(_ScreenParams.z-1);
				float height = _Size*(_ScreenParams.w-1);
				float4 vertPos = p[0].pos;
				newVert.pos = vertPos + float4(-width,-height,0,0);
				newVert.color = p[0].color.xyz;
				UNITY_TRANSFER_FOG(newVert, newVert.pos);
                TRANSFER_SHADOW(newVert);
				newVert.uv = float2(0,0);
				triStream.Append(newVert);
				newVert.pos = vertPos + float4(width,-height,0,0);
				newVert.uv = float2(1,0);
				UNITY_TRANSFER_FOG(newVert, newVert.pos);
                TRANSFER_SHADOW(newVert);
				triStream.Append(newVert);
				newVert.pos = vertPos + float4(-width,height,0,0);
				newVert.uv = float2(0,1);
				UNITY_TRANSFER_FOG(newVert, newVert.pos);
                TRANSFER_SHADOW(newVert);
				triStream.Append(newVert);
				newVert.pos = vertPos + float4(width,height,0,0);
				newVert.uv = float2(1,1);
				UNITY_TRANSFER_FOG(newVert, newVert.pos);
                TRANSFER_SHADOW(newVert);
				triStream.Append(newVert);										
			}

			fixed3 FS_Main(FS_INPUT input) : COLOR
			{
				fixed3 col = input.color.xyz;
                col *= SHADOW_ATTENUATION(input);
				clip(circle(input.uv,0.9) - 0.999);
				return col;
			}
			ENDCG
		} // Pass

		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
		
			CGPROGRAM
			#pragma vertex VS_Main
			#pragma fragment FS_Main
			#pragma geometry GS_Main
			#pragma multi_compile_shadowcaster
			#pragma multi_compile_instancing // allow instanced shadow pass for most of the shaders
			#include "UnityCG.cginc"
			
			struct appdata
			{
				float4 vertex : POSITION;
				//float3 color : COLOR;
			};
		
			struct GS_INPUT
			{
				V2F_SHADOW_CASTER;
				//UNITY_VERTEX_OUTPUT_STEREO
			};


			struct FS_INPUT
			{
				V2F_SHADOW_CASTER;
				//UNITY_VERTEX_OUTPUT_STEREO
			};

			float _Size;
	        fixed4 _Color;
			float _GridSizeAndPackMagic;
			float4 _Offset;

			float2 SuperUnpacker(float f)
			{
				return float2(f - floor(f), floor(f) / _GridSizeAndPackMagic);
			}

			GS_INPUT VS_Main(appdata v)
			{
				GS_INPUT o = (GS_INPUT)0;
				
				float2 xr = SuperUnpacker(v.vertex.x);
				float2 yg = SuperUnpacker(v.vertex.y);
				float2 zb = SuperUnpacker(v.vertex.z);

				//float4 p = float4(xr.y,yg.y,zb.y,v.vertex.w);
				float3 p = float3(xr.y + _Offset.x, yg.y + _Offset.y, zb.y + _Offset.z);
				o.pos = UnityObjectToClipPos(p);
				
				//UNITY_SETUP_INSTANCE_ID(v);
				//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				return o;
			}

			// source https://thebookofshaders.com/07/
			float circle(float2 _st, float _radius)
			{
				float2 dist = _st-float2(0.5,0.5);
				return 1.-smoothstep(_radius-(_radius*0.01),_radius+(_radius*0.01),dot(dist,dist)*4.0);
			}

			[maxvertexcount(4)]
			void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
			{
				float width = _Size*(_ScreenParams.z-1);
				float height = _Size*(_ScreenParams.w-1);
				float4 vertPos = p[0].pos;
				FS_INPUT newVert;
				newVert.pos = vertPos + float4(-width,-height,0,0);
				triStream.Append(newVert);
				newVert.pos = vertPos + float4(width,-height,0,0);
				triStream.Append(newVert);
				newVert.pos = vertPos + float4(-width,height,0,0);
				triStream.Append(newVert);
				newVert.pos = vertPos + float4(width,height,0,0);
				triStream.Append(newVert);										
			}

			fixed3 FS_Main(FS_INPUT input) : COLOR
			{
				//clip(circle(input.uv,0.9) - 0.999);
//				return input.color;
				SHADOW_CASTER_FRAGMENT(input)
			}
			ENDCG
		} // Pass

	} // SubShader
} // Shader
