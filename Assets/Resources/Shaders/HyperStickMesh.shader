// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UMol/Sticks HyperBalls Merged"
{
    // Proprietes exposees a l'interface graphique
    Properties {
        _MainTex        ("Parameters texture",2D) = "white"{}
        _Shrink         ("Shrink Factor", float) = 0.1
        _Scale          ("Link Scale", float) = 1.0
        _EllipseFactor  ("Ellipse Factor", float) = 1.0
        _Brightness     ("Brightness", float) = 1.0
        _NBParam        ("Texture size in x",Float) = 14.0
        _NBSticks       ("Texture size in y",Float) = 100.0
        _Shininess      ("Shininess",float) = 0.0
        _SpecularColor  ("Specular color",Color) = (1,1,1,1)
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


            uniform float _Shrink;
            uniform float _Scale;
            uniform float _EllipseFactor;
            uniform float _Brightness;
            uniform float _NBParam;
            uniform float _NBSticks;
            uniform sampler2D _MainTex;
            uniform float _Shininess;
            uniform float4 _SpecularColor;
            uniform float4 _LightColor0;
            uniform float4 _SelectedColor;


            uniform sampler2D _MatCap;


            // vertex input: position
            struct appdata
            {
                float4 vertex      : POSITION;
                float2 uv_vetexids : TEXCOORD0;
            };

            // Variables passees du vertex au pixel shader
            struct vertexOutput
            {
                float4 pos     : SV_POSITION;
                float4 near    : TEXCOORD1;
                float4 far     : TEXCOORD2;
                float4 focus   : TEXCOORD3;
                float4 cutoff1 : TEXCOORD4;
                float4 cutoff2 : TEXCOORD5;
                float4 e1      : TEXCOORD6;
                float4 e2      : TEXCOORD7;
                float4 e3      : TEXCOORD8;
                bool2 selected  : TEXCOORD9;
                float4 Color1  : COLOR0;
                UNITY_FOG_COORDS(10)

            };

            struct fragmentOutput
            {
              float4 color : SV_Target;
              float depth  : SV_Depth;
            };

            // VERTEX SHADER IMPLEMENTATION =============================

            vertexOutput vert (appdata v) {
                // OpenGL matrices
                float4x4 ModelViewProjI = mat_inverse(UNITY_MATRIX_MVP);

                vertexOutput o; // Shader output

                float4 vertexPosition;
                float NBParamm1 = _NBParam - 1;
                float vertexid = v.uv_vetexids[0];
                float x_texfetch = vertexid/(_NBSticks-1);
                //Calculate all the stuffs to create parallepipeds that defines the enveloppe for ray-casting


                half visibility = tex2Dlod(_MainTex,float4(x_texfetch,10/NBParamm1,0,0)).x;

                half2 scaleAtoms = tex2Dlod(_MainTex,float4(x_texfetch,11/NBParamm1,0,0)).xy;


                float radius1 = scaleAtoms.x * visibility * tex2Dlod(_MainTex,float4(x_texfetch,0,0,0)) * _Scale;
                float radius2 = scaleAtoms.y * visibility * tex2Dlod(_MainTex,float4(x_texfetch,1/NBParamm1,0,0)) * _Scale;

                float4 Color1 = tex2Dlod(_MainTex,float4(x_texfetch,2/NBParamm1,0,0));
                float4 Color2 = tex2Dlod(_MainTex,float4(x_texfetch,3/NBParamm1,0,0));
                o.Color1 = Color1;

                float4 texpos1 = tex2Dlod(_MainTex,float4(x_texfetch,4/NBParamm1,0,0));
                float4 texpos2 = tex2Dlod(_MainTex,float4(x_texfetch,5/NBParamm1,0,0));

                float4 basepos1 = tex2Dlod(_MainTex,float4(x_texfetch,6/NBParamm1,0,0));
                float4 basepos2 = tex2Dlod(_MainTex,float4(x_texfetch,7/NBParamm1,0,0));


                float sel1 = tex2Dlod(_MainTex,float4(x_texfetch,12/NBParamm1,0,0)).x;
                float sel2 = tex2Dlod(_MainTex,float4(x_texfetch,13/NBParamm1,0,0)).x;

                o.selected.x = (sel1 >= 0.9);
                o.selected.y = (sel2 >= 0.9);



                // Calculate distance between particles.
                float4 posAtom1 = texpos1;
                float4 posAtom2 = texpos2;
                float atomDistance = distance(posAtom1, posAtom2);

                // Calculate space position
                float4 spacePosition;
                //Center to 0,0,0
                spacePosition.xy = (v.vertex.xy - basepos1.xy) * 2.0 *(radius1 > radius2 ? radius1 : radius2);
                spacePosition.z = (v.vertex.z - basepos1.z) * atomDistance;
                spacePosition.w = 1.0;

                float4 e3;
                e3.xyz = normalize(posAtom1.xyz - posAtom2.xyz);
                if (e3.z == 0.0) { e3.z = 0.0000000000001;}
                if ( (posAtom1.x - posAtom2.x) == 0.0) { posAtom1.x += 0.001;}
                if ( (posAtom1.y - posAtom2.y) == 0.0) { posAtom1.y += 0.001;}
                if ( (posAtom1.z - posAtom2.z) == 0.0) { posAtom1.z += 0.001;}

                // Calculate focus.
                float4 focus = calculate_focus(posAtom1, posAtom2,
                               radius1, radius2,
                               e3, _Shrink);

                float3 e1;
                e1.x = 1.0;
                e1.y = 1.0;
                e1.z = ( sum(e3.xyz  *  focus.xyz) - e1.x  *  e3.x - e1.y  *  e3.y) / e3.z;
                e1 = normalize(e1 - focus.xyz);
                
                float3 e2 = normalize(cross(e1, e3.xyz));


                // Calculate rotation
                float3x3 R = float3x3(float3(e1.x, e2.x, e3.x),
                                      float3(e1.y, e2.y, e3.y),
                                      float3(e1.z, e2.z, e3.z));

                vertexPosition.xyz = mul(R, spacePosition.xyz);
                vertexPosition.w = 1.0;

                // Calculate translation
                vertexPosition.xyz += (posAtom2.xyz + posAtom1.xyz) / 2;

                o.pos = UnityObjectToClipPos(vertexPosition);


                // Calculate origin and direction of ray that we pass to the fragment ----
                float4 near = o.pos;
                near.z = 0.0 ;
                near = mul(ModelViewProjI, near) ;
                
                float4 far = o.pos ; 
                far.z = far.w ;
                far = mul(ModelViewProjI, far);

                o.near = near;
                o.far = far;
                #if UNITY_VERSION >= 550 && !SHADER_API_GLCORE && !SHADER_API_GLES && !SHADER_API_GLES3 //Since Unity 5.5 near and far plane are inverted 
                    o.near = far;
                    o.far = near;
                #endif


                UNITY_TRANSFER_FOG(o,o.pos);

                float4 prime1, prime2; 
                prime1.xyz = posAtom1.xyz - (posAtom1.xyz - focus.xyz)  *  _Shrink;
                prime2.xyz = posAtom2.xyz - (posAtom2.xyz - focus.xyz)  *  _Shrink;
                prime1.w = Color2.y;
                prime2.w = Color2.z;

                o.cutoff1 = prime1;
                o.cutoff2 = prime2;

                float4 a2fsq = (posAtom1 - focus)  *  (posAtom1 - focus);
                float Rcarre = (radius1 * radius1 / _Shrink) - sum(a2fsq.xyz);
                focus.w = Rcarre;

                e3.w = Color2.x;


                o.focus = focus;
                
                o.e3 = e3; 
                o.e1.xyz = e1;
                o.e2.xyz = e2;


                return o;
            }



            // PIXEL SHADER IMPLEMENTATION ===============================

            fragmentOutput frag (vertexOutput i)
            {

                if (_Shrink < 0.0)
                    discard;
                float4x4 ModelViewProj = UNITY_MATRIX_MVP;
                float4x4 ModelViewIT = UNITY_MATRIX_IT_MV; 

                fragmentOutput o;



                float4 i_near = i.near;
                float4 i_far  = i.far;
                float4 focus = i.focus;

                float3 e1 = i.e1.xyz;
                float3 e2 = i.e2.xyz;
                float3 e3 = i.e3.xyz;

                float3 color_atom2 = float3(i.e3.w, i.cutoff1.w, i.cutoff2.w);

                float3 cutoff1 = i.cutoff1.xyz;
                float3 cutoff2 = i.cutoff2.xyz;

                float t1 = -1 / (1 - _Shrink);
                float t2 =  1 / _Shrink;
                float3 equation1 = float3(t2,   t2  *  _EllipseFactor,    t1);

                float A1 = sum(-e1  *  focus.xyz);
                float A2 = sum(-e2  *  focus.xyz);
                float A3 = sum(-e3  *  focus.xyz);

                float3 As = float3(A1, A2, A3);

                float3 eqex = equation1  *  float3(e1.x, e2.x, e3.x);
                float3 eqey = equation1  *  float3(e1.y, e2.y, e3.y);
                float3 eqez = equation1  *  float3(e1.z, e2.z, e3.z);
                float3 eqAs = equation1  *  As  *  As;
                float4 e1ext = float4(e1, As.x);
                float4 e2ext = float4(e2, As.y);
                float4 e3ext = float4(e3, As.z);

                float4  An1 = eqex.x  *  e1ext     + eqex.y  *  e2ext     + eqex.z  *  e3ext;     // Contains A11, A21, A31, A41
                float3  An2 = eqey.x  *  e1ext.yzw + eqey.y  *  e2ext.yzw + eqey.z  *  e3ext.yzw; // Contains A22, A32, A42
                float2  An3 = eqez.x  *  e1ext.zw  + eqez.y  *  e2ext.zw  + eqez.z  *  e3ext.zw;  // Contains A33, A43
                float   A44 = eqAs.x             + eqAs.y             + eqAs.z - focus.w;   // Just A44

                float4x4 mat = float4x4(An1,
                                        float4(An1.y, An2.xyz),
                                        float4(An1.z, An2.y, An3.xy),
                                        float4(An1.w, An2.z, An3.y, A44));

                Ray ray = primary_ray(i_near,i_far);
                float3 M = isect_surf(ray, mat);
                float4 M1 = float4(M, 1.0);


                float4 clipHit = UnityObjectToClipPos(M1);
                o.depth = update_z_buffer(clipHit);

                if (cutoff_plane(M, cutoff1, -e3) || cutoff_plane(M, cutoff2, e3))
                    discard;

                //------------ blinn phong light try ------------------------
                
                float3 normal = normalize(mul(ModelViewIT, mul(mat, M1)).xyz);


                float a = sum((M.xyz - cutoff2)  *  e3) / distance(cutoff2, cutoff1);

                float4 color_atom1 = float4(i.Color1.xyz, 1);

                color_atom1 = lerp(color_atom1, _SelectedColor , i.selected.x*(_Time.y % 1.0));
                color_atom2 = lerp(color_atom2, _SelectedColor , i.selected.y*(_Time.y % 1.0));


                float4 pcolor = float4(lerp(color_atom2, color_atom1, a),i.Color1.w);

                // MatCap
                half2 vn = normal.xy;

                float4 matcapLookup1 = tex2D(_MatCap, vn*0.5 + 0.5);    
                float4 matcapLookup2 = tex2D(_MatCap, vn*0.5 + 0.5);    


                float4 matcapLookup = lerp(matcapLookup2, matcapLookup1, a);


                float3 L = normalize( mul(UNITY_MATRIX_V,float4(normalize(_WorldSpaceLightPos0.xyz),0)));
                float NdotL = saturate(dot(normal,L));
                float4 diffuseTerm = NdotL*_LightColor0;   

                float4 ambient = UNITY_LIGHTMODEL_AMBIENT *1.5;
                
                o.color = (ambient + diffuseTerm) * pcolor ;

                if(_Shininess){
                    float specular = pow(max(dot(normal, L),0.0),_Shininess);
                    o.color += specular*_SpecularColor;
                }

                o.color *=  matcapLookup  *  1.25  *  _Brightness;

                UNITY_APPLY_FOG(i.fogCoord, o.color);

                return o;
            }


            ENDCG
        }      
    }
}