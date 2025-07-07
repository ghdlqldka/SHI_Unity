    Shader "UnityCoder/PointCloud/DX11/Uber (HDRP VR SinglePass)"
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
                        #ifdef UNITY_VERTEX_OUTPUT_STEREO
                        uint unity_StereoEyeIndex : unity_StereoEyeIndex;
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
     
                #ifdef _ENABLESCALING_ON
                //float3 _Origin;
                float _MinDist, _MaxDist;
                float _MinSize, _MaxSize;
                #endif
     
                #ifdef _ENABLECOLOR_ON
                fixed4 _Tint;
                #endif
     
                GS_INPUT VS_Main(appdata v, uint id : SV_VertexID)//, uint inst : SV_InstanceID)
                {
                    GS_INPUT o = (GS_INPUT)0;

                    //UNITY_SETUP_INSTANCE_ID (id);
                    UNITY_INITIALIZE_OUTPUT(GS_INPUT, o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    //UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    //UNITY_TRANSFER_INSTANCE_ID(v, o);

                    float3 pos = buf_Points[id];
                    #ifdef _ENABLESCALING_ON
                    //pos+=_Origin; // TODO check if issues when transform is moved, since transform.pos gets added to points?
                    float dist = distance(buf_Points[id],_WorldSpaceCameraPos);
                    o.dist = dist;
                    #endif
     
                    o.pos = UnityObjectToClipPos(buf_Points[id]);
     
                    fixed3 col = buf_Colors[id];
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
     
                    // Calculate per-eye screen parameters assuming side-by-side layout:
                    float eyeWidth  = _ScreenParams.x * 0.5;
                    float eyeHeight = _ScreenParams.y;
                    float pixelToClipX = 2.0 / eyeWidth;  // clip space offset per pixel horizontally
                    float pixelToClipY = 2.0 / eyeHeight; // clip space offset per pixel vertically

                    // Now convert your _Size (assumed to be in pixels) to clip space:
                    float offsetX = _Size * pixelToClipX;
                    float offsetY = _Size * pixelToClipY;

                    // Optionally, you can use the stereo eye index if you need to adjust further:
                    #ifdef UNITY_VERTEX_OUTPUT_STEREO
                        uint eyeIndex = p[0].unity_StereoEyeIndex;
                        // In a side-by-side layout the effective width is the same for both eyes,
                        // so the math above applies equally. But if your setup were different,
                        // you could adjust based on eyeIndex.
                    #endif

                    // Retrieve the original vertex position:
                    float4 vertPos = p[0].pos;

                    // Set up the output vertex (using your usual macros):
                    FS_INPUT newVert = (FS_INPUT)0;
                    UNITY_SETUP_INSTANCE_ID(p[0]);
                    UNITY_INITIALIZE_OUTPUT(FS_INPUT, newVert);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(newVert);
                    UNITY_TRANSFER_INSTANCE_ID(p[0], newVert);

                    // Emit four vertices offset by the computed clip-space values:
    
                    // Bottom-left vertex:
                    newVert.pos = vertPos + float4(-offsetX, -offsetY, 0, 0);
                    newVert.color = p[0].color;
                    #ifdef _CIRCLE_ON
                        newVert.uv = float2(0,0);
                    #endif
                    triStream.Append(newVert);

                    // Top-left vertex:
                    newVert.pos = vertPos + float4(-offsetX, offsetY, 0, 0);
                    #ifdef _CIRCLE_ON
                        newVert.uv = float2(0,1);
                    #endif
                    triStream.Append(newVert);

                    // Bottom-right vertex:
                    newVert.pos = vertPos + float4(offsetX, -offsetY, 0, 0);
                    #ifdef _CIRCLE_ON
                        newVert.uv = float2(1,0);
                    #endif
                    triStream.Append(newVert);

                    // Top-right vertex:
                    newVert.pos = vertPos + float4(offsetX, offsetY, 0, 0);
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
