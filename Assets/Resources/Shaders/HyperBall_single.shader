Shader "UMol/HyperBalls GL_D3D" {


    // Properties exposed to the interface
    Properties {
        _Rayon         ("Rayon de la Sphere", float) = 0.5
        _Color         ("Couleur de la Sphere", Color) = (1,1,1,1.0)
        _TexPos        ("Position de la sphere", Vector) = (0.0,0.0,0.0,1.0)     // important que w != 0 !!
        // _Visibilite ("Visibilite de la Sphere", float) = 1.0
        _Equation      ("Equation", Vector) = (1,1,1,1)
        _MatCap        ("MatCap (RGB)", 2D) = "white" {}
        _Attenuation   ("Attenuation", float) = 0
        _Brightness    ("Brightness", float) = 1.0
        _Shininess     ("Shininess",float) = 0.0
        _SpecularColor ("Specular color",Color) = (1,1,1,1)

        [Toggle] _UseFog ("Enable fog", Float) = 0.0
        _FogStart ("Fog start", Float) = 0.0
        _FogDensity ("Fog density", Float) = 0.5

    }

    SubShader {

        Tags {"DisableBatching" = "True" "RenderType"="Opaque"}
        Pass {
            Tags {"LightMode" = "ForwardBase"}
            CGPROGRAM

            // Setup
            #pragma target 3.0
            #pragma vertex ballimproved_v
            #pragma fragment ballimproved_p
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "shared_hyperball.cginc"

            // Variables modifiables dans Unity3D
            uniform float _Rayon;
            uniform float4 _Color;
            uniform float4 _TexPos;
            uniform float4 _Equation;
            uniform sampler2D _MatCap;
            uniform int _Attenuation;
            uniform float _Brightness;
            uniform float _Shininess;
            uniform float4 _SpecularColor;
            uniform float4x4 _Mat;

            uniform float4 _LightColor0;
            uniform sampler2D _ShadowMapTexture;

            float _UseFog;
            float _FogStart;
            float _FogDensity;
            
            // vertex input: position
            struct appdata {
                float4 vertex : POSITION;
            };


            // Variables passees du vertex au pixel shader
            struct v2p {
                float4 p                : POSITION;
                float4 i_near           : TEXCOORD1;
                float4 i_far            : TEXCOORD2;
                float4 colonne1         : TEXCOORD6;
                float4 colonne2         : TEXCOORD7;
                float4 colonne3         : COLOR0;
                float4 colonne4         : TEXCOORD3;
                float4 _ShadowCoord     : TEXCOORD4;
            };

            struct fragment_out 
            {
              float4 color : SV_Target;
              float depth  : SV_Depth;
            };


            // VERTEX SHADER IMPLEMENTATION =============================

            v2p ballimproved_v (appdata v) {
                // OpenGL matrices
                float4x4 ModelViewProjI = mat_inverse(UNITY_MATRIX_MVP);

                v2p o; // Shader output

                float4 spaceposition;
                spaceposition.xyz = _TexPos.xyz;    
                spaceposition.w = 1.0;

                spaceposition.xyz += v.vertex.xyz * (2.0 * _Rayon);
                // spaceposition = mul(_Mat, spaceposition.xyz);

                spaceposition = mul(UNITY_MATRIX_M, spaceposition);

                o.p = mul(UNITY_MATRIX_VP, spaceposition);
                v.vertex = o.p;
                            

                float4 near = o.p ; 
                near.z = 0.0 ;
                near = mul(ModelViewProjI, near) ;

                float4 far = o.p ; 
                far.z = far.w ;
                o.i_far = mul(ModelViewProjI,far) ;
                o.i_near = near;
                
                #if UNITY_VERSION >= 550 && !SHADER_API_GLCORE && !SHADER_API_GLES && !SHADER_API_GLES3//Since Unity 5.5 near and far plane are inverted 
                    o.i_near =  o.i_far;
                    o.i_far = near;
                #endif
                
                

                float4 equation1 = float4(_Equation.xyz, _Rayon);

                float4 eq1TexPos = equation1 * _TexPos;
                float4 eq1TexSq = eq1TexPos * _TexPos;

                o.colonne1 = float4(equation1.x,    0.0,            0.0,            -eq1TexPos.x);
                o.colonne2 = float4(0.0,            equation1.y,    0.0,            -eq1TexPos.y);
                o.colonne3 = float4(0.0,            0.0,            equation1.z,    -eq1TexPos.z);
                o.colonne4 = float4(-eq1TexPos.x,   -eq1TexPos.y,   -eq1TexPos.z,   -equation1.w*equation1.w + eq1TexSq.x + eq1TexSq.y + eq1TexSq.z);

                o._ShadowCoord = ComputeScreenPos(o.p);

                return o;    
            }



            // PIXEL SHADER IMPLEMENTATION ===============================

            fragment_out ballimproved_p (v2p i) {

                //float3 light = _Light.xyz;
                float4x4 ModelViewProj = UNITY_MATRIX_MVP;          // matrice pour passer dans les coordonnees de l'ecran
                float4x4 ModelViewIT = UNITY_MATRIX_IT_MV; 

                fragment_out OUT;

                //create matrix for the quadric equation of the sphere 
                float4x4 mat = float4x4(i.colonne1,i.colonne2,i.colonne3,i.colonne4);
             
                Ray ray = primary_ray(i.i_near,i.i_far) ;

                Quadric q = isect_surf_ball(ray, mat);
                float3 M = q.s1;


                // OUT.depth = update_z_buffer(M, ModelViewProj);
                float4 clipHit = UnityObjectToClipPos(float4(M,1));
                OUT.depth = update_z_buffer(clipHit);


                //Transform normal from model space to view-space
                float4 M1 = float4(M,1.0);
                float4 M2 = mul(mat,M1);

                float3 normal = normalize(mul(ModelViewIT,M2).xyz);
        
                //LitSPhere / MatCap
                half2 vn = normal.xy;
               
                float4 matcapLookup = tex2D(_MatCap, vn*0.5 + 0.5);

                //------------ blinn phong light ------------------------

                float3 L = normalize( mul(UNITY_MATRIX_V,float4(normalize(_WorldSpaceLightPos0.xyz),0)));
                float NdotL = saturate(dot(normal,L));
                
                half shadow = tex2Dproj( _ShadowMapTexture,i._ShadowCoord).x;

                float4 diffuseTerm = NdotL*_LightColor0;    

                float4 ambient = UNITY_LIGHTMODEL_AMBIENT *1.5;
                OUT.color = (ambient + diffuseTerm * shadow) * _Color ;

                if(_Shininess && shadow == 1.0){
                    float specular = pow(max(dot(normal, L),0.0),_Shininess);
                    OUT.color += specular*_SpecularColor;
                }

                OUT.color = OUT.color * matcapLookup * 1.25 * _Brightness;
                
                if(_UseFog){
                    // float fogFactor = smoothstep(_FogEnd, _FogStart, mul(UNITY_MATRIX_M, M1).z);     
                    float fogFactor = exp(_FogStart - mul(UNITY_MATRIX_M, M1).z  / max(0.0001, _FogDensity));
                    OUT.color.rgb = lerp(unity_FogColor, OUT.color.rgb, saturate(fogFactor));
                }
                
                
              return OUT;

            }
            ENDCG
        }//Pass

        Pass {

            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster"}

            CGPROGRAM
            #pragma target 4.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "shared_hyperball.cginc"

            float _Rayon;
            float4 _TexPos;
            float4 _Equation;

            struct v2f { 
                float4 p                : SV_POSITION;
                float4 i_near           : TEXCOORD1;
                float4 i_far            : TEXCOORD2;
                float4 colonne1         : TEXCOORD3;
                float4 colonne2         : TEXCOORD4;
                float4 colonne3         : TEXCOORD5;
                float4 colonne4         : TEXCOORD6;
            };
            struct appdata {
                float4 vertex : POSITION;
            };

            v2f vert( appdata v ) {
                v2f o;


                // OpenGL matrices
                float4x4 ModelViewProj = UNITY_MATRIX_MVP;          // matrice pour passer dans les coordonnees de l'ecran
                float4x4 ModelViewProjI = mat_inverse(ModelViewProj);


                float4 spaceposition;
                spaceposition.xyz = _TexPos.xyz;    
                spaceposition.w = 1.0;

                spaceposition.xyz += v.vertex.xyz * (2.0 * _Rayon);

                // o.worldpos = mul(ModelViewProj, spaceposition);
                float4 worldpos = mul(ModelViewProj, spaceposition);
                v.vertex = worldpos;

                float4 near = worldpos;
                near.z = 0.0 ;
                near = mul(ModelViewProjI, near) ;

                float4 far = worldpos;
                far.z = far.w ;
                o.i_far = mul(ModelViewProjI,far) ;
                o.i_near = near;

                #if UNITY_VERSION >= 550 && !SHADER_API_GLCORE && !SHADER_API_GLES && !SHADER_API_GLES3//Since Unity 5.5 near and far plane are inverted 
                    o.i_near =  o.i_far;
                    o.i_far = near;
                #endif

                float4 equation1 = float4(_Equation.xyz, _Rayon);
                float4 eq1TexPos = equation1 * _TexPos;
                float4 eq1TexSq = eq1TexPos * _TexPos;

                o.colonne1 = float4(equation1.x,    0.0,            0.0,            -eq1TexPos.x);
                o.colonne2 = float4(0.0,            equation1.y,    0.0,            -eq1TexPos.y);
                o.colonne3 = float4(0.0,            0.0,            equation1.z,    -eq1TexPos.z);
                o.colonne4 = float4(-eq1TexPos.x,   -eq1TexPos.y,   -eq1TexPos.z,   -equation1.w*equation1.w + eq1TexSq.x + eq1TexSq.y + eq1TexSq.z);

                o.p = worldpos;           
                return o;
            }


            float frag( v2f i ) : SV_Depth
            {

                float4x4 ModelViewProj = UNITY_MATRIX_MVP;
                float4x4 ModelViewIT = UNITY_MATRIX_IT_MV;      

                //create matrix for the quadric equation of the sphere 
                float4x4 mat = float4x4(i.colonne1,i.colonne2,i.colonne3,i.colonne4);   

                Ray ray = primary_ray(i.i_near,i.i_far) ;

                Quadric q = isect_surf_ball(ray, mat);
                float3 M = q.s1;

                float4 clipHit = UnityObjectToClipPos(float4(M,1));
                float depth = update_z_buffer(clipHit);

                #if defined(UNITY_REVERSED_Z)
                    depth += max(-1,min(unity_LightShadowBias.x/i.p.w,0));
                    float clamped = min(depth, i.p.w*UNITY_NEAR_CLIP_VALUE);
                #else
                    depth += saturate(unity_LightShadowBias.x/i.p.w);
                    float clamped = max(depth, i.p.w*UNITY_NEAR_CLIP_VALUE);
                #endif


                depth = lerp(depth, clamped, unity_LightShadowBias.y);

                return depth;
            }
            ENDCG
        }

    }
}