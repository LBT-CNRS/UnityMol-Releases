Shader "UMol/Ball HyperBalls Merged"
{

    // Properties exposed to the interface
    Properties {
        _MainTex       ("Parameters texture",2D) = "white"{}
        _Brightness    ("Brightness",Float) = 1.0
        _NBParam       ("Texture size in x",Float) = 12.0
        _NBAtoms       ("Texture size in y",Float) = 10.0
        _Shininess     ("Shininess",float) = 0.0
        _SpecularColor ("Specular color",Color) = (1,1,1,1)
        _SelectedColor ("Color when selected",Color) = (1,0.68,0,1)
        _MatCap        ("MatCap  (RGB)", 2D) = "white" {}

    }
    SubShader {

        Tags { "DisableBatching" = "True" "RenderType"="Opaque"}

        Pass {
            Tags {"LightMode" = "ForwardBase"} 

            CGPROGRAM

            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "shared_hyperball.cginc"


            uniform sampler2D _MainTex;
            
            uniform sampler2D _MatCap;

            uniform float _Brightness;
            uniform float _NBParam;
            uniform float _NBAtoms;
            uniform float _Shininess;
            uniform float4 _SpecularColor;
            uniform float4 _LightColor0;
            uniform float4 _SelectedColor;



            // vertex input: position
            struct appdata
            {
                float4 vertex      : POSITION;
                float2 uv_vetexids : TEXCOORD0;//Id of the sphere in the texture for each vertex
            };


            // From vertex shader to fragment shader
            struct v2p {
                float4 p                : SV_POSITION;
                float4 i_near           : TEXCOORD0;
                float4 i_far            : TEXCOORD1;
                float4 colonne1         : TEXCOORD2;
                float4 colonne2         : TEXCOORD3;
                float4 colonne3         : TEXCOORD4;
                float4 colonne4         : TEXCOORD5;
                float4 worldpos         : TEXCOORD6;
                float4 color            : COLOR0;
                int2 atomTypeAndSel     : TEXCOORD7;
                UNITY_FOG_COORDS(8)
            };



            struct fragmentOutput 
            {
              float4 color : SV_Target;
              float depth  : SV_Depth;
            };

            // VERTEX SHADER IMPLEMENTATION =============================
         
            v2p vert (appdata v) {
                // OpenGL matrices
                float4x4 ModelViewProj = UNITY_MATRIX_MVP; // Matrix for screen coordinates
                float4x4 ModelViewProjI = mat_inverse(ModelViewProj);

                v2p o; // Shader output


                float NBParamm1 = _NBParam - 1;
                float vertexid = v.uv_vetexids[0];
                float x_texfetch = vertexid/(_NBAtoms -1);


                float4 sphereposition = tex2Dlod(_MainTex,float4(x_texfetch,0,0,0));

                half visibility = tex2Dlod(_MainTex,float4(x_texfetch,7/NBParamm1,0,0)).x;

                float4 baseposition = tex2Dlod(_MainTex,float4(x_texfetch,4/NBParamm1,0,0));

                float4 equation = tex2Dlod(_MainTex,float4(x_texfetch,6/NBParamm1,0,0));

                float atomTypetmp = tex2Dlod(_MainTex,float4(x_texfetch,5/NBParamm1,0,0)).x;

                // o.textureType = (int)(atomTypetmp *50);
                float scale = tex2Dlod(_MainTex,float4(x_texfetch,8/NBParamm1,0,0)).x;

                float sel = tex2Dlod(_MainTex,float4(x_texfetch,9/NBParamm1,0,0)).x;

                o.atomTypeAndSel.y = (int)(sel >= 0.9);



                //Fetch the encoded radius of the sphere
                float rayon = scale * visibility * tex2Dlod(_MainTex,float4(x_texfetch,1/NBParamm1,0,0)).x;

                o.color = tex2Dlod(_MainTex,float4(x_texfetch,2/NBParamm1,0,0));

                o.atomTypeAndSel.x = (int) 0;//Not used anymore

                float4 spaceposition;
    
                //Center to 0,0,0 + make the bounding box larger + re-translate to position
                spaceposition.xyz = (v.vertex.xyz - baseposition.xyz)*(2.0*rayon) + sphereposition.xyz;
                spaceposition.w = 1.0;

                o.p = mul(ModelViewProj, spaceposition);
                v.vertex = o.p;
    
                o.worldpos = o.p;
    
                float4 near = o.p ; 
                near.z = 0.0f ;
                near = mul(ModelViewProjI, near) ;

                float4 far = o.p ; 
                far.z = far.w ;
                o.i_far = mul(ModelViewProjI,far) ;
                o.i_near = near;

                #if UNITY_VERSION >= 550 && !SHADER_API_GLCORE && !SHADER_API_GLES && !SHADER_API_GLES3 //Since Unity 5.5 near and far plane are inverted 
                    o.i_near =  o.i_far;
                    o.i_far = near;
                #endif

                UNITY_TRANSFER_FOG(o,o.p);

                float4 eq1TexPos,eq1TexSq;
                // float4 equation = float4(equationx,equationy,equationz,equationw);
                float4 equation1 = float4(equation.xyz,rayon);


                eq1TexPos = equation1 * sphereposition;
                eq1TexSq =  eq1TexPos * sphereposition;

                o.colonne1 = float4(equation1.x, 0.0f,   0.0f,   -eq1TexPos.x);
                o.colonne2 = float4(0.0f,   equation1.y, 0.0f,   -eq1TexPos.y);
                o.colonne3 = float4(0.0f,   0.0f,   equation1.z, -eq1TexPos.z);
                o.colonne4 = float4(-eq1TexPos.x, -eq1TexPos.y, -eq1TexPos.z, -equation1.w*equation1.w + eq1TexSq.x + eq1TexSq.y + eq1TexSq.z);

                return o;  
            }



            // PIXEL SHADER IMPLEMENTATION ===============================

            fragmentOutput frag (v2p i) {


                fragmentOutput o;

                float4x4 ModelViewProj = UNITY_MATRIX_MVP; // Matrix for screen coordinates
                float4x4 ModelViewIT = UNITY_MATRIX_IT_MV; 

                //create matrix for the quadric equation of the sphere 
                float4x4 mat = float4x4(i.colonne1,i.colonne2,i.colonne3,i.colonne4);  
        
                Ray ray = primary_ray(i.i_near,i.i_far) ;

                Quadric q = isect_surf_ball(ray, mat);
                float3 M = q.s1;
                float4 clipHit = UnityObjectToClipPos(float4(M,1));
                o.depth = update_z_buffer(clipHit);
        
                //Transform normal to model space to view-space
                float4 M1 = float4(M,1.0);
                float4 M2 = mul(mat,M1);

                float3 normal = normalize(mul(ModelViewIT,M2).xyz);

                 //LitSPhere / MatCap
                half2 vn = normal.xy;



                float4 matcapLookup = tex2D(_MatCap, vn*0.5 + 0.5);   

                float3 L = normalize( mul(UNITY_MATRIX_V,float4(normalize(_WorldSpaceLightPos0.xyz),0)));
                float NdotL = saturate(dot(normal,L));
                
                float4 diffuseTerm = NdotL*_LightColor0;    

                float4 ambient = UNITY_LIGHTMODEL_AMBIENT *1.5;
                float4 inColor = float4(i.color.xyz,1);
                inColor = lerp(inColor, _SelectedColor , i.atomTypeAndSel.y*(_Time.y % 1.0));

                o.color = (ambient + diffuseTerm) * inColor;

                if(_Shininess){
                    float specular = pow(max(dot(normal, L),0.0),_Shininess);
                    o.color += specular*_SpecularColor;
                }

                

                o.color *= matcapLookup * 1.25 * _Brightness;

                

                UNITY_APPLY_FOG(i.fogCoord, o.color);
                return o;

            }


            ENDCG
        }

    }
}