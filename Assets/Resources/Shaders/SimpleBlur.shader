Shader "Custom/SimpleGrabPassBlur" {
   Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _Radius("Radius", Range(1, 255)) = 1
        _MainTex        ("Parameters texture",2D) = "white"{}
    }
 
    Category
    {
        Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Opaque" }
     
        SubShader
        {
            GrabPass
            {
                Tags{ "LightMode" = "Always" }
            }
 
            Pass
            {
                Tags{ "LightMode" = "Always" }
                Blend SrcAlpha OneMinusSrcAlpha
                Cull Off
 
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
                #include "UnityCG.cginc"
                half4 _Color;
                sampler2D _MainTex;
                float4 _MainTex_ST;
 
                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 texcoord: TEXCOORD0;
                };
 
                struct v2f
                {
                    float4 vertex : POSITION;
                    float4 uvgrab : TEXCOORD0;
                    float2 uv     : TEXCOORD1;
                };


                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
                    #if UNITY_UV_STARTS_AT_TOP
                    float scale = -1.0;
                    #else
                    float scale = 1.0;
                    #endif
                    o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
                    o.uvgrab.zw = o.vertex.zw;
                    return o;
                }
 
                sampler2D _GrabTexture;
                float4 _GrabTexture_TexelSize;
                float _Radius;
 
                half4 frag(v2f i) : COLOR
                {
                    fixed4 texcol = tex2D (_MainTex, i.uv);
    
                    half4 sum = half4(0,0,0,0);
 
                    #define GRABXYPIXEL(kernelx, kernely) tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(float4(i.uvgrab.x + _GrabTexture_TexelSize.x * kernelx, i.uvgrab.y + _GrabTexture_TexelSize.y * kernely, i.uvgrab.z, i.uvgrab.w)))
 
                    sum += GRABXYPIXEL(0.0, 0.0);
                    int measurments = 1;
 
                    for (float range = 0.1f; range <= _Radius; range += 0.1f)
                    {
                        sum += GRABXYPIXEL(range, range);
                        sum += GRABXYPIXEL(range, -range);
                        sum += GRABXYPIXEL(-range, range);
                        sum += GRABXYPIXEL(-range, -range);
                        measurments += 4;
                    }
 
                    half4 b = sum / measurments;
                    half4 col = (_Color + b)/2;
                    col.a = _Color.a;
                    return col * texcol;
                }
                ENDCG
            }
            GrabPass
            {
                Tags{ "LightMode" = "Always" }
            }
 
            Pass
            {
                Tags{ "LightMode" = "Always" }
                Blend SrcAlpha OneMinusSrcAlpha
                Cull Off

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
                #include "UnityCG.cginc"
                half4 _Color;
                sampler2D _MainTex;
                float4 _MainTex_ST;


                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 texcoord: TEXCOORD0;
                };
 
                struct v2f
                {
                    float4 vertex : POSITION;
                    float4 uvgrab : TEXCOORD0;
                    float2 uv     : TEXCOORD1;
                };
 
                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
                    #if UNITY_UV_STARTS_AT_TOP
                    float scale = -1.0;
                    #else
                    float scale = 1.0;
                    #endif
                    o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
                    o.uvgrab.zw = o.vertex.zw;
                    return o;
                }
 
                sampler2D _GrabTexture;
                float4 _GrabTexture_TexelSize;
                float _Radius;
 
                half4 frag(v2f i) : COLOR
                {
 
                    fixed4 texcol = tex2D (_MainTex, i.uv);
 
                    half4 sum = half4(0,0,0,0);
                    float radius = 1.41421356237 * _Radius;
 
                    #define GRABXYPIXEL(kernelx, kernely) tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(float4(i.uvgrab.x + _GrabTexture_TexelSize.x * kernelx, i.uvgrab.y + _GrabTexture_TexelSize.y * kernely, i.uvgrab.z, i.uvgrab.w)))
 
                    sum += GRABXYPIXEL(0.0, 0.0);
                    int measurments = 1;
 
                    for (float range = 1.41421356237f; range <= radius * 1.41; range += 1.41421356237f)
                    {
                        sum += GRABXYPIXEL(range, 0);
                        sum += GRABXYPIXEL(-range, 0);
                        sum += GRABXYPIXEL(0, range);
                        sum += GRABXYPIXEL(0, -range);
                        measurments += 4;
                    }
 
                    half4 b = sum / measurments;
                    half4 col = (_Color + b)/2;
                    col.a = _Color.a;
                    return col * texcol;

                }
                ENDCG
            }
        }
    }
}
 