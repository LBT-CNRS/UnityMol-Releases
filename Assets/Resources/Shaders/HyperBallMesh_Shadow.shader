Shader "UMol/Ball HyperBalls Shadow Merged" {


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

		[Toggle] _UseFog ("Enable fog", Float) = 0.0
		_FogStart ("Fog start", Float) = 0.0
		_FogDensity ("Fog density", Float) = 0.5

        _AOStrength ("Ambient occlusion strength",float) = 1.0
        _AOTex("Ambient occlusion texture",2D) = "white" {}
        _AOTexwidth ("AO width", float) = 1.0
        _AOTexheight ("AO height", float) = 1.0
        _AORes ("AO resolution",float) = 1.0
        _AOcoords ("AO coordinates in the atlas",Vector) = (0,0,0,0)

	}
	CGINCLUDE

	#include "UnityCG.cginc"
	#include "shared_hyperball.cginc"
	#include "AutoLight.cginc"
	#include "Lighting.cginc"

	uniform sampler2D _MainTex;
	uniform	float _Brightness,_NBParam,_NBAtoms;

	ENDCG

	SubShader {
	    Tags { "DisableBatching" = "True" "RenderType"="Opaque"}

		Pass {
			// Lighting On
	        Tags {"LightMode" = "ForwardBase"}	

			CGPROGRAM

			// Setup
			#pragma target 3.0
			#pragma vertex ballimproved_v
			#pragma fragment ballimproved_p
			#pragma multi_compile_fwdbase
            // #pragma multi_compile_fog

			uniform sampler2D _MatCap;



            uniform float _Shininess;
            uniform float4 _SpecularColor;
            uniform float4 _SelectedColor;
            // uniform float4 _LightColor0;
            // uniform sampler2D _ShadowMapTexture;

        float _UseFog;
		float _FogStart;
		float _FogDensity;

            float _AOStrength;
            float _AOTexwidth;
            float _AOTexheight;
            float _AORes;
            sampler2D _AOTex;



			// vertex input: position
			struct appdata {
				float4 vertex      : POSITION;
				float2 uv_vetexids : TEXCOORD0;//Id of the sphere in the texture for each vertex
			};

			// From vertex shader to fragment shader
			struct v2p {
				float4 pos         		: SV_POSITION;
				float4 i_near	   		: TEXCOORD0;
				float4 i_far	   		: TEXCOORD1;
				float4 colonne1			: TEXCOORD2;
				float4 colonne2			: TEXCOORD3;
				float4 colonne3			: TEXCOORD4;
				float4 colonne4			: TEXCOORD5;
				float4 worldpos			: TEXCOORD6;
				float4 color			: COLOR0;
				// float4 _ShadowCoord     : TEXCOORD7;
				LIGHTING_COORDS(7,8)
                // UNITY_FOG_COORDS(9)
                // bool selected           : TEXCOORD10;
                float4 atlasinfo	    : TEXCOORD10;
                float3 spherePos        : TEXCOORD11;
			};

			struct fragment_out 
					{
					  float4 color : SV_Target;
					  float depth  : SV_Depth;
				};



			// VERTEX SHADER IMPLEMENTATION =============================

			v2p ballimproved_v (appdata v) {
				// OpenGL matrices
				float4x4 ModelViewProj = UNITY_MATRIX_MVP;	// Matrix for screen coordinates
				float4x4 ModelViewProjI = mat_inverse(ModelViewProj);
				float NBParamm1 = _NBParam - 1;
				v2p o; // Shader output

				float vertexid = v.uv_vetexids[0];
				float x_texfetch = vertexid/(_NBAtoms-1);

                float4 sphereposition = tex2Dlod(_MainTex,float4(x_texfetch,0,0,0));

                half visibility = tex2Dlod(_MainTex,float4(x_texfetch,7/NBParamm1,0,0)).x;



                float4 baseposition = tex2Dlod(_MainTex,float4(x_texfetch,4/NBParamm1,0,0));

                float4 equation = tex2Dlod(_MainTex,float4(x_texfetch,6/NBParamm1,0,0));


                float scale = tex2Dlod(_MainTex,float4(x_texfetch,8/NBParamm1,0,0)).x;

                // float sel = tex2Dlod(_MainTex,float4(x_texfetch,9/NBParamm1,0,0)).x;
                // o.selected = (sel >= 0.9);


                //Fetch the encoded radius of the sphere
                float rayon = scale * visibility * tex2Dlod(_MainTex,float4(x_texfetch,1/NBParamm1,0,0)).x;

                o.color = tex2Dlod(_MainTex,float4(x_texfetch,2/NBParamm1,0,0));

                float2 atlasid = tex2Dlod(_MainTex,float4(x_texfetch,10/NBParamm1,0,0)).xy;

                o.atlasinfo = float4(atlasid.x, atlasid.y, rayon, vertexid);
                o.spherePos = baseposition;


				float4 spaceposition;
				
				//Center to 0,0,0 + make the bounding box larger + re-translate to position
				spaceposition.xyz = (v.vertex.xyz - baseposition.xyz)*(2*rayon) + sphereposition.xyz;
				spaceposition.w = 1.0;

				o.pos = mul(ModelViewProj, spaceposition);
				v.vertex = o.pos;
				
				o.worldpos = o.pos;
				
				float4 near = o.pos ; 
				near.z = 0.0f ;
				near = mul(ModelViewProjI, near) ;

				float4 far = o.pos ; 
				far.z = far.w ;
				o.i_far = mul(ModelViewProjI,far) ;
				o.i_near = near;

                #if UNITY_VERSION >= 550 && !SHADER_API_GLCORE && !SHADER_API_GLES && !SHADER_API_GLES3//Since Unity 5.5 near and far plane are inverted 
                    o.i_near =  o.i_far;
                    o.i_far = near;
                #endif

                // UNITY_TRANSFER_FOG(o,o.pos);

				float4 eq1TexPos,eq1TexSq;
				float4 equation1 = float4(equation.xyz,rayon);


				eq1TexPos = equation1 * sphereposition;
				eq1TexSq =  eq1TexPos * sphereposition;

				o.colonne1 = float4(equation1.x,	0.0f,			0.0f,			-eq1TexPos.x);
				o.colonne2 = float4(0.0f,			equation1.y,	0.0f,			-eq1TexPos.y);
				o.colonne3 = float4(0.0f,			0.0f,			equation1.z,	-eq1TexPos.z);
				o.colonne4 = float4(-eq1TexPos.x,	-eq1TexPos.y,	-eq1TexPos.z,	-equation1.w*equation1.w + eq1TexSq.x + eq1TexSq.y + eq1TexSq.z);

				// o._ShadowCoord = ComputeScreenPos(o.p);
				TRANSFER_VERTEX_TO_FRAGMENT(o);

				return o;  
			}



			// PIXEL SHADER IMPLEMENTATION ===============================

			fragment_out ballimproved_p (v2p i) {

				fragment_out OUT;

				float4x4 ModelViewProj = UNITY_MATRIX_MVP;	// Matrix for screen coordinates
				float4x4 ModelViewIT = UNITY_MATRIX_IT_MV; 


				//create matrix for the quadric equation of the sphere 
				float4x4 mat = float4x4(i.colonne1,i.colonne2,i.colonne3,i.colonne4); 	
			 
				Ray ray = primary_ray(i.i_near,i.i_far) ;

			    Quadric q = isect_surf_ball(ray, mat);
			    float3 M = q.s1;
			    float4 clipHit = UnityObjectToClipPos(float4(M,1));
			    OUT.depth = update_z_buffer(clipHit);
				
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

				// half shadow = tex2Dproj( _ShadowMapTexture,i._ShadowCoord).x;
				// half shadow = tex2Dproj( _ShadowMapTexture,UNITY_PROJ_COORD(i._ShadowCoord)).x;
                half shadow = LIGHT_ATTENUATION(i);

                float4 ambient = UNITY_LIGHTMODEL_AMBIENT *1.5;
                float4 inColor = float4(i.color.xyz,1);
                // inColor = lerp(inColor, _SelectedColor , i.selected*(_Time.y % 1.0));
                OUT.color = (ambient + (diffuseTerm*shadow)) * inColor ;

                if(_Shininess && shadow > 0.5){
                    float specular = pow(max(dot(normal, L),0.0),_Shininess);
                    OUT.color += specular*_SpecularColor;
                }

				float aoterm = 1.0;

                if(_AOStrength != 0){

                    float radius = i.atlasinfo.z;
                    float3 posModelunit = (M - i.spherePos)/radius;

                    float a = abs(posModelunit.x) + abs(posModelunit.y) + abs(posModelunit.z);
                    float u = (posModelunit.z>0)? sign(posModelunit.x)*(1-abs(posModelunit.y)/a) : posModelunit.x/a;
                    float v = (posModelunit.z>0)? sign(posModelunit.y)*(1-abs(posModelunit.x)/a) : posModelunit.y/a;
                    float2 myuv = float2((u+1)*0.5,(v+1)*0.5);//between 0 and 1

                    float2 posAOpath = (_AORes-1)*myuv;
                    float2 sizePatch = float2(_AORes,_AORes);

                    float2 weight = frac(posAOpath) ;

                    float2 uvtexcale = posAOpath;
                    float2 texelcenter = floor( uvtexcale )  + 0.5;


                    float2 posinAO = round(i.atlasinfo.xy) + texelcenter;
                    // float2 posinAO = round(i.atlasinfo.xy) + posAOpath;

                    // float c4 = tex2D(_AOTex,posinAO / float2(_AOTexwidth,_AOTexheight));
                    float c4 = tex2D(_AOTex,(posinAO + weight) / float2(_AOTexwidth,_AOTexheight));

                    // aoterm *= _AOStrength;
                    aoterm = _AOStrength * c4;
                    // aoterm = pow(c4, _AOStrength);

                    // aoterm = log(_AOStrength*aoterm);
                    // aoterm =  1/(1+exp((-15)*aoterm+(7+_AOStrength*3)));

                    // aoterm = clamp(aoterm,0,1);

                }

				OUT.color *= matcapLookup * 1.25 * _Brightness * aoterm;


                // UNITY_APPLY_FOG(i.fogCoord, OUT.color);
                // OUT.color.rgb = applyFog(OUT.color.rgb, mul(UNITY_MATRIX_M, M1), _Attenuation);
				if(_UseFog){
					// float fogFactor = smoothstep(_FogEnd, _FogStart, mul(UNITY_MATRIX_M, M1).z);		
					float fogFactor = exp(_FogStart - mul(UNITY_MATRIX_M, M1).z  / max(0.0001, _FogDensity));
					OUT.color.rgb = lerp(unity_FogColor, OUT.color.rgb, saturate(fogFactor));
				}

			  return OUT;

			}
			ENDCG
		}


		Pass {

			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster"}


			CGPROGRAM
			#pragma target 3.0
			#pragma vertex ballmerged_v
			#pragma fragment ballmerged_f
			#pragma multi_compile_shadowcaster
			// #pragma fragmentoption ARB_precision_hint_fastest


			struct v2f { 
				float4 pos              : POSITION;
				float4 i_near	   		: TEXCOORD1;
				float4 i_far	   		: TEXCOORD2;
				float4 colonne1			: TEXCOORD6;
				float4 colonne2			: TEXCOORD7;
				float4 colonne3			: COLOR0;
				float4 colonne4			: TEXCOORD3;
				float4 worldpos			: TEXCOORD4;
				int visibility  		: TEXCOORD5;
			};


			// vertex input: position
			struct appdata {
				float4 vertex : POSITION;
				float2 uv_vetexids : TEXCOORD0;//Id of the sphere in the texture for each vertex
			};



			v2f ballmerged_v (appdata v) {
				// OpenGL matrices
				float4x4 ModelViewProj = UNITY_MATRIX_MVP;	// Matrix for screen coordinates
				float4x4 ModelViewProjI = mat_inverse(ModelViewProj);
				float NBParamm1 = _NBParam - 1;
				v2f o; // Shader output

				float vertexid = v.uv_vetexids[0];
				float x_texfetch = vertexid/(_NBAtoms-1);


                float4 sphereposition = tex2Dlod(_MainTex,float4(x_texfetch,0,0,0));

                half visibility = tex2Dlod(_MainTex,float4(x_texfetch,7/NBParamm1,0,0)).x;



                float4 baseposition = tex2Dlod(_MainTex,float4(x_texfetch,4/NBParamm1,0,0));

                float4 equation = tex2Dlod(_MainTex,float4(x_texfetch,6/NBParamm1,0,0));

                float scale = tex2Dlod(_MainTex,float4(x_texfetch,8/NBParamm1,0,0)).x;


                float rayon =  scale * tex2Dlod(_MainTex,float4(x_texfetch,1/NBParamm1,0,0)).x;



				o.visibility = (int)visibility;
				float4 spaceposition;
				
				spaceposition.xyz = (v.vertex.xyz - baseposition.xyz)*(2*rayon) + sphereposition.xyz;

				spaceposition.w = 1.0;

				o.pos = mul(ModelViewProj, spaceposition);
				v.vertex = o.pos;
				
				o.worldpos = o.pos;
				
				float4 near = o.pos ; 
				near.z = 0.0f ;
				near = mul(ModelViewProjI, near) ;

				float4 far = o.pos; 
				far.z = far.w ;
				o.i_far = mul(ModelViewProjI,far) ;
				o.i_near = near;


                #if UNITY_VERSION >= 550 && !SHADER_API_GLCORE && !SHADER_API_GLES && !SHADER_API_GLES3//Since Unity 5.5 near and far plane are inverted 
                    o.i_near =  o.i_far;
                    o.i_far = near;
                #endif



				float4 eq1TexPos,eq1TexSq;
				float4 equation1 = float4(equation.xyz,rayon);


				eq1TexPos = equation1 * sphereposition;
				eq1TexSq =  eq1TexPos * sphereposition;

				o.colonne1 = float4(equation1.x,	0.0f,			0.0f,			-eq1TexPos.x);
				o.colonne2 = float4(0.0f,			equation1.y,	0.0f,			-eq1TexPos.y);
				o.colonne3 = float4(0.0f,			0.0f,			equation1.z,	-eq1TexPos.z);
				o.colonne4 = float4(-eq1TexPos.x,	-eq1TexPos.y,	-eq1TexPos.z,	-equation1.w*equation1.w + eq1TexSq.x + eq1TexSq.y + eq1TexSq.z);


				return o;  
			}


			//TODO Fix the version when we output to a SHADOWS_CUBE
			// #if !defined(SHADOWS_CUBE) && !defined(UNITY_MIGHT_NOT_HAVE_DEPTH_TEXTURE)
			// #define OUTPUT_DEPTH
			// #endif
			 
			// #ifdef OUTPUT_DEPTH
			// #define SHADOW_OUT_PS SV_Depth
			// #define SHADOW_OUT_TYPE float
			// #else
			// #define SHADOW_OUT_PS SV_Target0
			// #define SHADOW_OUT_TYPE float4
			// #endif

			float ballmerged_f (v2f i) : SV_Depth{


				float4x4 ModelViewProj = UNITY_MATRIX_MVP;	// Matrix for screen coordinates
				float4x4 ModelViewIT = UNITY_MATRIX_IT_MV; 

				if(i.visibility!=1)
					clip(-1);

				//create matrix for the quadric equation of the sphere 
				float4x4 mat = float4x4(i.colonne1,i.colonne2,i.colonne3,i.colonne4); 	
			 
				Ray ray = primary_ray(i.i_near,i.i_far) ;

			    Quadric q = isect_surf_ball(ray, mat);
			    float3 M = q.s1;
			    float4 clipHit = UnityObjectToClipPos(float4(M,1));
			    float depth = update_z_buffer(clipHit);

				#if defined(UNITY_REVERSED_Z)
					depth += max(-1,min(unity_LightShadowBias.x/i.pos.w,0));
					float clamped = min(depth, i.pos.w*UNITY_NEAR_CLIP_VALUE);
                #else
					depth += saturate(unity_LightShadowBias.x/i.pos.w);
					float clamped = max(depth, i.pos.w*UNITY_NEAR_CLIP_VALUE);
				#endif


				depth = lerp(depth, clamped, unity_LightShadowBias.y);
				

			  return depth;

			}

		    ENDCG
		}
	}
}