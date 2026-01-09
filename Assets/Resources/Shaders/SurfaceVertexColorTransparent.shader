Shader "Custom/SurfaceVertexColorTransparent" {
    Properties {
        _Color ("Color", Color) = (1, 1, 1, 0.1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0

        [Toggle] _UseFog ("Enable fog", Float) = 0.0
        _FogStart ("Fog start", Float) = 0.0
        _FogDensity ("Fog density", Float) = 0.5

        [Toggle] _LimitedView ("Enable limited view", Float) = 0.0
        _LimitedViewRadius ("Limited view Radius", Float) = 10.0
        _LimitedViewCenter ("Limited view Center", Vector) = (0.0, 0.0, 0.0)
    }
    SubShader {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
        Cull Off
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard vertex:vert finalcolor:mycolor alpha:fade noambient

        // Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
            float4 color : COLOR;
            float3 worldPos;
            float3 objPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // uniform sampler2D _CameraDepthTexture;
        float _FogStart;
        float _FogDensity;
        float _UseFog;

        float _LimitedView;
        float _LimitedViewRadius;
        float3 _LimitedViewCenter;

        fixed3 applyFog(fixed3 c, float3 wp) {
            float viewDistance = length(_WorldSpaceCameraPos - wp);
            return lerp(c, unity_FogColor, saturate(0.5 * viewDistance));
        }

        void vert( inout appdata_full v, out Input o){
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.objPos = v.vertex;
        }

        void mycolor (Input IN, SurfaceOutputStandard o, inout fixed4 color) {

            if (_UseFog) {

                float d = IN.worldPos.z;

                float fogFactor = exp(_FogStart - d  / max(0.0001, _FogDensity));

                color.rgb = lerp(unity_FogColor, color.rgb, saturate(fogFactor));
            }

        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            if (_LimitedView) {
                float d = distance(IN.objPos, _LimitedViewCenter);
                if (d > _LimitedViewRadius) {
                    // o.Albedo = float3(1,0,0);
                    // return;
                    discard;
                }
            }

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color * IN.color;

            // o.Albedo = c.rgb;
            o.Emission = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            // o.Alpha = c.a;
            // float al = max(_Color.a, c.a * 10.0f) * 0.5;

            o.Alpha = _Color.a;
        }
        ENDCG
    }
    FallBack "Transparent"
}
