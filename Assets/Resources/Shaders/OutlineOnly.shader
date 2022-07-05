// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

//Adapted from https://gist.github.com/hiepnd/e00324106c6b8d4e6714

Shader "Custom/OutlinedOnly" {

    Properties {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _Outline ("Outline width", Range (0.05, 1.0)) = .5
        _MainTex ("Base (RGB)", 2D) = "white" { }
        _Alpha ("Alpha", Float) = 1
    }

    CGINCLUDE
#include "UnityCG.cginc"

    struct appdata {
half4 vertex : POSITION;
half3 normal : NORMAL;
half2 texcoord : TEXCOORD0;
    };

    struct v2f {
half4 pos : POSITION;
half2 uv : TEXCOORD0;
half3 normalDir : NORMAL;
    };

    uniform half _Outline;
    uniform half4 _OutlineColor;

    ENDCG

    SubShader {
        Tags { "Queue" = "Geometry" }

        Pass {
            Name "STENCIL"
            ZWrite Off
            ZTest Always
            ColorMask 0

            Stencil {
                Ref 2
                Comp always
                Pass replace
                ZFail decrWrap
            }

            CGPROGRAM

#pragma vertex vert2
#pragma fragment frag

            v2f vert2 (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                return o;
            }

            half4 frag (v2f i) : COLOR
            {
                return half4(0, 0, 0, 0);
            }

            ENDCG


        }

        Pass {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            Cull Off
            ZWrite Off
            ColorMask RGB

            Blend SrcAlpha OneMinusSrcAlpha

            Stencil {
                Ref 2
                Comp NotEqual
                Pass replace
                ZFail decrWrap
            }

            CGPROGRAM
#pragma vertex vert
#pragma fragment frag

            v2f vert(appdata v) {
                // just make a copy of incoming vertex data but scaled according to normal direction
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

                // float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                float3 norm = mul(unity_ObjectToWorld, v.normal);
                float2 offset = TransformViewToProjection(norm.xy);

                float dist = length(ObjSpaceViewDir(v.vertex)) ;

                o.pos.xy += offset * o.pos.z * _Outline;
                o.normalDir = half3(0,0,0);
                o.uv = half2(0,0);
                return o;
            }

            half _Alpha;
            half4 frag(v2f i) : COLOR {
                half4 col = _OutlineColor;
                col.a = clamp(_Time.y%1.0, 0.8, 1.0);
                // col.a = _Time.y % 1.0;
                return col;
            }
            ENDCG
        }


    }

    Fallback Off
}