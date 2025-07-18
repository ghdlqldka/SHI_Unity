    Shader "UnityCoder/PointCloud/DX11/Uber (VR SinglePass) - Packed"
    {
        Properties
        {
            _Size ("Size", Float) = 1.0
            [KeywordEnum(Off, On)] _Circle ("Circular Point",float) = 0
            [KeywordEnum(Off, On)] _EnableColor ("Enable Tint",float) = 0
            _Tint ("Color Tint", Color) = (0,1,0,1)
            [KeywordEnum(Off, On)] _EnableScaling ("Enable Distance Scaling",float) = 0
            //_Origin ("Origin", vector) = (0,0,0)
            _MinDist ("Min Distance", float) = 0
            _MaxDist ("Max Distance", float) = 100
            _MinSize ("Min Size", float) = 0.1
            _MaxSize ("Max Size", float) = 1.0
    		_Offset("Offset", Vector) = (0,0,0,0)
        }
     
        SubShader
        {
            Pass
            {
                Tags { "Queue" = "Geometry" "RenderType"="Opaque" }
                ZWrite On
                LOD 200
                Cull Off
         
                CGPROGRAM
                #pragma target 4.0
                #pragma vertex VS_Main
                #pragma fragment FS_Main
                #pragma geometry GS_Main
                #pragma multi_compile _CIRCLE_ON _CIRCLE_OFF
                #pragma multi_compile _ENABLECOLOR_ON _ENABLECOLOR_OFF
                #pragma multi_compile _ENABLESCALING_ON _ENABLESCALING_OFF
                #include "UnityCG.cginc"
                #pragma fragmentoption ARB_precision_hint_fastest
     
                StructuredBuffer<half3> buf_Points;
                StructuredBuffer<fixed3> buf_Colors;
     
                struct appdata
                {
                    fixed3 color : COLOR;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };
         
                struct GS_INPUT
                {
                    float4    pos        : POSITION;
                    fixed3 color     : COLOR;
                    #ifdef _ENABLESCALING_ON
                    float dist      : TEXCOORD1;
                    #endif
				    UNITY_VERTEX_INPUT_INSTANCE_ID
				    UNITY_VERTEX_OUTPUT_STEREO
                };
     
                struct FS_INPUT
                {
                    float4    pos        : POSITION;
                    fixed3 color     : COLOR;
                    #ifdef _CIRCLE_ON
                    float2 uv : TEXCOORD0;
                    #endif
				    UNITY_VERTEX_INPUT_INSTANCE_ID
				    UNITY_VERTEX_OUTPUT_STEREO
                };
     
                float _Size;
			    float _GridSizeAndPackMagic;
    			float4 _Offset;
     
                #ifdef _ENABLESCALING_ON
                //float3 _Origin;
                float _MinDist, _MaxDist;
                float _MinSize, _MaxSize;
                #endif
     
                #ifdef _ENABLECOLOR_ON
                fixed4 _Tint;
                #endif

                float2 SuperUnpacker(float f)
			    {
				    return float2(f - floor(f), floor(f) / _GridSizeAndPackMagic);
			    }
     
                GS_INPUT VS_Main(appdata v, uint id : SV_VertexID)//, uint inst : SV_InstanceID)
                {
                    GS_INPUT o = (GS_INPUT)0;

                    //UNITY_SETUP_INSTANCE_ID (id);
                    UNITY_INITIALIZE_OUTPUT(GS_INPUT, o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    //UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    //UNITY_TRANSFER_INSTANCE_ID(v, o);

                    //float3 pos = buf_Points[id];
       				float3 rawpos = buf_Points[id];
				    float2 xr = SuperUnpacker(rawpos.x);
				    float2 yg = SuperUnpacker(rawpos.y);
				    float2 zb = SuperUnpacker(rawpos.z);
				    float3 p = float3(xr.y + _Offset.x, yg.y + _Offset.y, zb.y + _Offset.z);



                    #ifdef _ENABLESCALING_ON
                    //pos+=_Origin; // TODO check if issues when transform is moved, since transform.pos gets added to points?
                    float dist = distance(buf_Points[id],_WorldSpaceCameraPos);
                    o.dist = dist;
                    #endif
     
                    o.pos = UnityObjectToClipPos(p);
     
                    //fixed3 col = buf_Colors[id];
                    fixed3 col = fixed3(saturate(xr.x), saturate(yg.x), saturate(zb.x)) * 1.02; // restore colors a bit, optional

                    #ifdef _ENABLECOLOR_ON
                    col *= _Tint;
                    #endif
     
                    #if !UNITY_COLORSPACE_GAMMA
                    col = col*col; // linear
                    #endif
                    o.color = col;
                    return o;
                }
     
                [maxvertexcount(4)]
                void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
                {
                    #ifdef _ENABLESCALING_ON
                    _Size = _MinSize + (p[0].dist-_MinDist)*(_MaxSize-_MinSize)/(_MaxDist-_MinDist);
                    #endif
     
                    float width = _Size*(_ScreenParams.z-1);
                    float height = _Size*(_ScreenParams.w-1);
                    float4 vertPos = p[0].pos;
                    
                    FS_INPUT newVert = (FS_INPUT)0;
                 
                    //DEFAULT_UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(p[0]);
                    UNITY_SETUP_INSTANCE_ID(p[0]);
				    UNITY_INITIALIZE_OUTPUT(FS_INPUT, newVert);
				    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(newVert);
    				UNITY_TRANSFER_INSTANCE_ID(p[0], newVert);
                 
                    newVert.pos = vertPos + float4(-width,-height,0,0);
                    newVert.color = p[0].color;
                    #ifdef _CIRCLE_ON
                    newVert.uv = float2(0,0);
                    #endif
                    // UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], newVert);
                    triStream.Append(newVert);
     
                    newVert.pos = vertPos + float4(-width,height,0,0);
                    #ifdef _CIRCLE_ON
                    newVert.uv = float2(1,0);
                    #endif
                    // UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], newVert);
                    triStream.Append(newVert);
     
                    newVert.pos = vertPos + float4(width,-height,0,0);
                    #ifdef _CIRCLE_ON
                    newVert.uv = float2(0,1);
                    #endif
                    // UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], newVert);
                    triStream.Append(newVert);
     
                    newVert.pos = vertPos + float4(width,height,0,0);
                    #ifdef _CIRCLE_ON
                    newVert.uv = float2(1,1);
                    #endif
                    // UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], newVert);
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
     
                float4 FS_Main(FS_INPUT input) : SV_Target
                {
                    #ifdef _CIRCLE_ON
                    clip(circle(input.uv,0.9) - 0.999);
                    #endif
                    return float4(input.color,1);
                }
                ENDCG
            }
        }
    }
