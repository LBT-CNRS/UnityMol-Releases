Shader "Custom/OnlyShadowPass" {
    Properties {
        [Toggle] _LimitedView ("Enable limited view", Float) = 0.0
        _LimitedViewRadius ("Limited view Radius", Float) = 10.0
        _LimitedViewCenter ("Limited view Center", Vector) = (0.0, 0.0, 0.0)
    }

    SubShader{
        Pass {
            ZWrite Off
            ColorMask 0
        }

        Pass {
            Tags { "LightMode" = "ShadowCaster" "RenderType" = "Opaque" "Queue" = "Geometry" }

            CGPROGRAM

#pragma vertex Vert
#pragma fragment Frag
#pragma multi_compile_shadowcaster
#include "UnityCG.cginc"

            float _LimitedView;
            float _LimitedViewRadius;
            float3 _LimitedViewCenter;

            struct Varyings
            {

                V2F_SHADOW_CASTER;
                float3 objPos : TEXCOORD4;
            };

            Varyings Vert(appdata_base v)
            {
                Varyings o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                o.objPos = v.vertex;
                return o;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                if (_LimitedView) {
                    float d = distance(i.objPos, _LimitedViewCenter);
                    if (d > _LimitedViewRadius) {

                        discard;
                    }
                }
                SHADOW_CASTER_FRAGMENT(i)
            }

            ENDCG
        }
    }
}