Shader "Custom/TestColorMesh"
{

    // Properties exposed to the interface
    Properties {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
    
        _Brightness    ("Brightness",Float) = 1.0
        _Attenuation   ("Attenuation",Float) = 0.0
        _Shininess     ("Shininess",float) = 0.0
        _SpecularColor ("Specular color",Color) = (1,1,1,1)


    }
    SubShader {


        Pass {
           Tags {"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            // compile shader into multiple variants, with and without shadows
            // (we don't care about any lightmaps yet, so skip these variants)
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            // shadow helper functions and macros
            #include "AutoLight.cginc"


            uniform float _Attenuation;
            uniform float _Brightness;
            uniform float _Shininess;
            uniform float4 _SpecularColor;

            sampler2D _MainTex;


            // vertex input: position
            // struct appdata
            // {
            //     float4 vertex      : POSITION;
            //     float4 color       : COLOR;
            // };


            // From vertex shader to fragment shader
            struct v2p {
                float2 uv : TEXCOORD0;
                SHADOW_COORDS(1) // put shadows data into TEXCOORD1
                fixed3 diff : COLOR0;
                fixed3 ambient : COLOR1;
                float4 pos : SV_POSITION;
                float4 vcolor : COLOR2;
            };


            // VERTEX SHADER IMPLEMENTATION =============================
         
            v2p vert (appdata_full v) {
                v2p o;
                o.vcolor = v.color;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal,1));
                // compute shadows data
                TRANSFER_SHADOW(o)
                
                return o;
            }



            // PIXEL SHADER IMPLEMENTATION ===============================

            fixed4 frag (v2p i) : SV_Target {

                fixed4 col = tex2D(_MainTex, i.uv) ;
                // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                fixed shadow = SHADOW_ATTENUATION(i);
                // darken light's illumination with shadow, keep ambient intact
                fixed3 lighting = i.diff * shadow + i.ambient;
                col.rgb *= lighting * i.vcolor;
                return col;

            }


            ENDCG
        }
    }
    Fallback "Diffuse"
}