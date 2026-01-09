Shader "UMol/WireframeBakedBarycentricCoordinates" {

    //Adapted from https://forum.unity.com/threads/optimal-way-of-rendering-lines.759713/
    Properties {
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0

        _Thickness ("Line Width (Pixels)", Range(0, 3)) = 0.1

        [Toggle] _UseFog ("Enable fog", Float) = 0.0
        _FogStart ("Fog start", Float) = 0.0
        _FogDensity ("Fog density", Float) = 0.5

        [Toggle] _LimitedView ("Enable limited view", Float) = 0.0
        _LimitedViewRadius ("Limited view Radius", Float) = 10.0
        _LimitedViewCenter ("Limited view Center", Vector) = (0.0, 0.0, 0.0)

    }
//     SubShader {
//         Tags { "Queue" = "Transparent" "RenderType" = "Transparent"  "IgnoreProjector" = "True"}
//         Blend SrcAlpha OneMinusSrcAlpha
//         Lighting Off
//         Cull Off
//         LOD 200
//         // ZWrite Off
//         Fog { Mode Off }

//         Pass {
//             CGPROGRAM
//             #include "UnityCG.cginc"
//             #pragma vertex vert
//             #pragma fragment frag

//             struct v2f {
//                 float4 pos : SV_Position;
//                 float3 color  : COLOR;
//                 float3 coord  : TEXCOORD0;
//                 float3 wPos   : TEXCOORD1;
//                 float3 objPos : TEXCOORD2;
//             };

//             float _Thickness;

//             float _LimitedView;
//             float _LimitedViewRadius;
//             float3 _LimitedViewCenter;

//             float _FogStart;
//             float _FogDensity;
//             float _UseFog;

//             void vert (appdata_full v, out v2f o, uint vid : SV_VertexID ) {
//                 o.pos = UnityObjectToClipPos(v.vertex);

//                 // hack to get barycentric coords on the default plane mesh
//                 vid += (uint)round(v.vertex.z - 1000);
//                 uint colIndex = vid % 3;
//                 o.coord = float3(colIndex == 0, colIndex == 1, colIndex == 2);
//                 o.objPos = v.vertex;
//                 o.wPos = mul(unity_ObjectToWorld, v.vertex);
//                 o.color = v.color;
//             }

//             half4 frag (v2f i) : SV_Target {


//                 if (_LimitedView) {
//                     float d = distance(i.objPos, _LimitedViewCenter);
//                     if (d > _LimitedViewRadius)
//                         discard;
//                 }

//                 float3 coordScale = fwidth(i.coord);

//                 // more accurate alternative to fwidth
//                 // float3 coordScale = sqrt(pow(ddx(i.coord), 2) + pow(ddy(i.coord), 2));


//                 float3 scaledCoord = i.coord / coordScale;
//                 float dist = min(scaledCoord.x, min(scaledCoord.y, scaledCoord.z));
//                 float halfWidth = _Thickness * 0.5;
//                 float wire = smoothstep(halfWidth + 0.5, halfWidth - 0.5, dist);

//                 half4 col = half4(i.color.rgb, wire);
//                 if (_UseFog) {
//                     // float fogFactor = smoothstep(_FogEnd, _FogStart, mul(UNITY_MATRIX_M, M1).z);
//                     float fogFactor = exp(_FogStart - i.wPos.z  / max(0.0001, _FogDensity));
//                     col.rgb = lerp(unity_FogColor, col.rgb, saturate(fogFactor));
//                 }

//                 return col;


//                 // float3 scaledCoord = (i.coord /*- _Thickness * 0.5*/) / coordScale;
//                 // float dist = min(scaledCoord.x, min(scaledCoord.y, scaledCoord.z));
//                 // float wire = smoothstep(_Thickness, -_Thickness, dist);

//                 // return half4(_Color.rgb, _Color.a * wire);

//             }
//         ENDCG
//         }
//     }
// }
SubShader {
    Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
    // Pass {
    //     ColorMask 0
    // }
    Cull Off
    // ZWrite Off
    LOD 200

    CGPROGRAM

        struct Input {
            // float4 pos : SV_POSITION;
            float4 color : COLOR;
            float3 coord ;
            float4 wPos ;
            float4 objPos ;
        };


        #pragma surface surf Standard vertex:vert finalcolor:mycolor alpha:fade noambient
        #pragma target 3.5

        struct myappdata {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float4 color  : COLOR;
            uint vid : SV_VertexID;
        };



        half _Glossiness;
        half _Metallic;

        float _Thickness;

        float _LimitedView;
        float _LimitedViewRadius;
        float3 _LimitedViewCenter;

        float _FogStart;
        float _FogDensity;
        float _UseFog;




        void mycolor (Input IN, SurfaceOutputStandard o, inout fixed4 color) {
            if (_UseFog) {

                float d = IN.wPos.z;
                float fogFactor = exp(_FogStart - d  / max(0.0001, _FogDensity));
                color.rgb = lerp(unity_FogColor, color.rgb, saturate(fogFactor));
            }
        }

        // v2f vert (appdata_full v, uint vid : SV_VertexID)
        void vert(inout myappdata v, out Input o)
        {
            // o.pos = UnityObjectToClipPos(v.vertex);
            o.color = v.color;

            // hack to get barycentric coords on the default plane mesh
            v.vid += (uint)round(v.vertex.z - 1000);
            uint colIndex = v.vid % 3;
            o.coord = float3(colIndex == 0, colIndex == 1, colIndex == 2);
            // o.viewPos = UnityObjectToViewPos(v.vertex);
            o.objPos = v.vertex;
            o.wPos = mul(unity_ObjectToWorld, v.vertex);
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            if (_LimitedView) {
                float d = distance(IN.objPos, _LimitedViewCenter);
                if (d > _LimitedViewRadius) {
                    discard;
                }
            }

            float3 coordScale = fwidth(IN.coord);

            // more accurate alternative to fwidth
            // float3 coordScale = sqrt(pow(ddx(IN.coord), 2) + pow(ddy(IN.coord), 2));


            float3 scaledCoord = IN.coord / coordScale;
            float dist = min(scaledCoord.x, min(scaledCoord.y, scaledCoord.z));
            float halfWidth = _Thickness * 0.5;
            float wire = smoothstep(halfWidth + 0.5, halfWidth - 0.5, dist);


            o.Albedo = IN.color.rgb;
            // o.Emission = IN.color.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;


            o.Alpha = wire;
        }
    // #endif

    ENDCG
    }
    FallBack "Transparent"
}