/// @file SurfaceMATCAP.shader
/// @brief Details to be specified
/// @author FvNano/LBT team
/// @author Marc Baaden <baaden@smplinux.de>
/// @date   2013-4
///
/// Copyright Centre National de la Recherche Scientifique (CNRS)
///
/// contributors :
/// FvNano/LBT team, 2010-13
/// Marc Baaden, 2010-13
///
/// baaden@smplinux.de
/// http://www.baaden.ibpc.fr
///
/// This software is a computer program based on the Unity3D game engine.
/// It is part of UnityMol, a general framework whose purpose is to provide
/// a prototype for developing molecular graphics and scientific
/// visualisation applications. More details about UnityMol are provided at
/// the following URL: "http://unitymol.sourceforge.net". Parts of this
/// source code are heavily inspired from the advice provided on the Unity3D
/// forums and the Internet.
///
/// This software is governed by the CeCILL-C license under French law and
/// abiding by the rules of distribution of free software. You can use,
/// modify and/or redistribute the software under the terms of the CeCILL-C
/// license as circulated by CEA, CNRS and INRIA at the following URL:
/// "http://www.cecill.info".
/// 
/// As a counterpart to the access to the source code and rights to copy, 
/// modify and redistribute granted by the license, users are provided only 
/// with a limited warranty and the software's author, the holder of the 
/// economic rights, and the successive licensors have only limited 
/// liability.
///
/// In this respect, the user's attention is drawn to the risks associated 
/// with loading, using, modifying and/or developing or reproducing the 
/// software by the user in light of its specific status of free software, 
/// that may mean that it is complicated to manipulate, and that also 
/// therefore means that it is reserved for developers and experienced 
/// professionals having in-depth computer knowledge. Users are therefore 
/// encouraged to load and test the software's suitability as regards their 
/// requirements in conditions enabling the security of their systems and/or 
/// data to be ensured and, more generally, to use and operate it in the 
/// same conditions as regards security.
///
/// The fact that you are presently reading this means that you have had 
/// knowledge of the CeCILL-C license and that you accept its terms.
///
/// $Id: SurfaceMATCAP.shader 210 2013-04-06 20:52:41Z baaden $
///
/// References : 
/// If you use this code, please cite the following reference : 	
/// Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
/// "Game on, Science - how video game technology may help biologists tackle
/// visualization challenges" (2013), PLoS ONE 8(3):e57990.
/// doi:10.1371/journal.pone.0057990
///
/// If you use the HyperBalls visualization metaphor, please also cite the
/// following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
/// B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
/// using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
/// J. Comput. Chem., 2011, 32, 2924
///

Shader "Mat Cap Cut" {
Properties {
	_Color ("Main Color", Color) = (0.5,0.5,0.5,1) 				//couleur général de la surface  
	_ColorIN ("Color Cut" , Color) = (0.5,0.5,0.5,1) 			// Couleur du plan de coupe
	_SpecColor ("Specular Color", Color) = (0, 0, 0, 1)		// Couleur des reflet de la surface
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125	// Brillance de la surface inactif ici
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}		// Texture de la surface 
    _BumpMap ("Bumpmap (RGB)", 2D) = "bump" {}	
	_texture ("activation texture",float)= 0				// activation des texture
    _MatCap ("MatCap (RGB)", 2D) = "white" {}				// texture MAtCap
   	_depthcut("depth", float) = 0							// distance de cut de la surface
	_cut("Active cut", float) = 0							// active ou non le cut
	_cutX("axe X", float)=1
	_cutY("axe Y", float)=1									// axe pour le cut fixe
	_cutZ("axe Z", float)=1
	_colormode("color", float) = 0							// activation du dégradé de couleur	
}

SubShader {
	Tags { "RenderType"="Transparent" }
            /* Upgrade NOTE: commented out, possibly part of old style per-pixel lighting: Blend AppSrcAdd AppDstAdd */
    Fog { Color [_AddFog] } 

    Pass { 
                Name "BASE"
                Tags { "LightMode" = "Always" } 
                cull back
                CGPROGRAM    
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members uv,TtoV0,TtoV1)
#pragma exclude_renderers d3d11 xbox360
// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct v2f members uv,TtoV0,TtoV1)
				#pragma exclude_renderers xbox360
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_fog_exp2
                #pragma fragmentoption ARB_precision_hint_fastest
                #include "UnityCG.cginc"
		  
	  		
		  		float4 pos;
				float4 worldPos;
				fixed4 _ColorIn;
				float _cut;
				float _depthcut;
				float _cutX;
				float _cutY;
				float _cutZ;	
				float3 _SurfacePos;
				float _texture;
				float _colormode;
				  
				struct appdata {
   				 	float4 vertex : POSITION;
   				 	float4 texcoord : TEXCOORD0;
    				float4 color : COLOR;
       				float3 normal : NORMAL;
    				float4 tangent : TANGENT;
				};
				  
				  
                struct v2f {
                    float4 pos : SV_POSITION;
                    float2  uv;
                    float3  TtoV0;
                    float3  TtoV1; 
           	    	float4  worldPos : TEXCOORD1 ;
  					fixed4 color : COLOR;

                };
 
 

				
                uniform float4 _BumpMap_ST;
 
                v2f vert (appdata v)
                {
                    v2f o;
                    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                    o.uv = TRANSFORM_TEX(v.texcoord,_BumpMap);
		  			o.worldPos = v.vertex;
   					o.color = v.color;
  
                    TANGENT_SPACE_ROTATION;
                    o.TtoV0 = mul(rotation, UNITY_MATRIX_MV[0].xyz);
                    o.TtoV1 = mul(rotation, UNITY_MATRIX_MV[1].xyz);
                    return o;
                }
               
                uniform float4 _Color;
                uniform sampler2D _BumpMap;
                uniform sampler2D _MatCap;
               
                float4 frag (v2f i) : COLOR
                { 
                
		            	pos = mul (UNITY_MATRIX_MVP, float4(i.worldPos.x+_SurfacePos.x,i.worldPos.y+_SurfacePos.y,i.worldPos.z+_SurfacePos.z,0));
						
						// Cut of the molecule
						if ( _cut== 1f ){ // active le cut fixe
								if ((_depthcut + _cutX*(i.worldPos.x+_SurfacePos.x) + _cutY*(i.worldPos.y+_SurfacePos.y) + _cutZ*(i.worldPos.z+_SurfacePos.z))<0){
									clip(-1);
								}//else {
									//clip((_depthcut + _cutX*(i.worldPos.x+_SurfacePos.x) + _cutY*(i.worldPos.y+_SurfacePos.y) + _cutZ*(i.worldPos.z+_SurfacePos.z)));
									//}
						}else if ( _cut== 2f ){
								clip (frac(-(_depthcut + pos.z)/500) -0.5);
						}
		                    // get normal from the normal map
		                    float3 normal = tex2D(_BumpMap, i.uv).xyz * 2 - 1;
		                    half2 vn;
		                    vn.x = dot(i.TtoV0, normal);
		                    vn.y = dot(i.TtoV1, normal);
		                   
		                    float4 matcapLookup = tex2D(_MatCap, vn*0.5 + 0.5);
		                    
		                    // coment for benoist video
		                   	//if (_colormode == 0f){
		                   	//	_Color=i.color;
		                   	// } 
		                   
		                    return _Color*matcapLookup*1.5;

	                }
	                ENDCG
	            } // End PASS

		Pass{
			Tags { } // tag tres important, la modification peut entrainer la dispartition du plan de coupe
			Cull front
			
			CGPROGRAM
			#include "UnityCG.cginc"
		
	// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct appdata members vertex)
			#pragma exclude_renderers xbox360
			#pragma exclude_renderers gles
	//		#pragma surface surf BlinnPhong finalcolor:mycolor
			#pragma target 3.0
			#pragma vertex Surface_v
			#pragma fragment Surface_p
			#pragma multi_compile_builtin
		
	
		float4 pos;
		float4 worldPos;
		fixed4 _ColorIN;
		float _cut;
		float _depthcut;
		float 	_cutX;
		float 	_cutY;
		float 	_cutZ;	
		float3 _SurfacePos;
			
					
																	
		struct v2p {
	    	float4 worldPos : TEXCOORD1 ;
	    	float4 pos : SV_POSITION ;
	    	float2 texcoord : TEXCOORD0;
			};
			
		struct fragment_out 
			{
			  float4 Color    : COLOR0;
 			};
		
		
		 uniform float4 _MainTex_ST;
		// VERTEX SHADER IMPLEMENTATION =============================
			v2p Surface_v(appdata_base v)
			{
			
			v2p o; // Shader output
			
			o.worldPos = v.vertex;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
			return o;
			}
			
		// FRAGMENT SHADER IMPLEMENTATION =========================== 
			fragment_out Surface_p(v2p i)
			{
				fragment_out OUT;
				pos = mul (UNITY_MATRIX_MVP, float4(i.worldPos.x+_SurfacePos.x,i.worldPos.y+_SurfacePos.y,i.worldPos.z+_SurfacePos.z,0));
				
				// Cut of the molecule
				if ( _cut== 1f ){ // active le cut fixe
						if ((_depthcut + _cutX*(i.worldPos.x+_SurfacePos.x) + _cutY*(i.worldPos.y+_SurfacePos.y) + _cutZ*(i.worldPos.z+_SurfacePos.z))<0){
							clip(-1);
						}//else {
							//clip((_depthcut + _cutX*(i.worldPos.x+_SurfacePos.x) + _cutY*(i.worldPos.y+_SurfacePos.y) + _cutZ*(i.worldPos.z+_SurfacePos.z)));
							//}
				}else if ( _cut== 2f ){
						clip (frac(-(_depthcut + pos.z)/500) -0.5);
				}
				
				if ( _cut !=  0f ){
				
					// amelioration de l'aspect du plan de coupe	
						
					
//					float3 normal = normalize(mul(ModelViewIT,mul(mat,M1)).xyz);
//					float3 normal = normalize(mul(UNITY_MATRIX_IT_MV,float4(_cutX,_cutY,_cutZ,1)).xyz);
	
					float3 normal =  normalize(float3(pos.x,pos.y,pos.z));
	
			// normalisation a retrouver 
	
					float3 lightvec = normalize(float3(0,0,1));
					float3 eyepos = float3(0,0,1.0);
					float3 halfvec = normalize(lightvec + eyepos);
					
					float diffuse = dot(normal,lightvec);
					float shininess = 500.0;				
					float specular = pow(max(dot(normal, halfvec),0.0),shininess);
				   
					float3 diffusecolor;	
					float3 specularcolor = float3(1.0,1.0,1.0);
				
					
	//				if ( frac(_cutX*(i.worldPos.x+_SurfacePos.x) + _cutY*(i.worldPos.y+_SurfacePos.y) + _cutZ*(i.worldPos.z+_SurfacePos.z)) < 0.5 ){


			
//					diffusecolor = float3(102f/255,102f/255,102f/255);
					diffusecolor = _ColorIN.rgb;
				
					
					// ajout de rayure sur le plan #############################


//					float2 screenUV = pos.xy / pos.w;
//					if ( frac(pos.x/2) > 0.5 ){
//				    	diffusecolor = float3(0,0,0);
//				    }else{ 
//				    	diffusecolor = float3(1,1,0);
//				    }
				
					
					// ambiant sur l'interieur de la molecule ###################################
					
//					diffusecolor = float3((pos.z/20)-0.2,pos.z/20-0.2,pos.z/20-0.2);
//				
//					if(dot(normal,eyepos) < 0.5)
//						OUT.Color.rgb = float3(0,0,0);
//					else{
//					    OUT.Color.rgb = diffuse * diffusecolor + specular*specularcolor;
//					    OUT.Color.rgb = diffusecolor;			       
//				    }
				    
				    // Coloration uni basique ####################################

					OUT.Color.a = 0.2;
					
//						OUT.Color.rgb = float3(102f/255,102f/255,102f/255);
					OUT.Color.rgb = _ColorIN.rgb;
	
//			OUT.Color.a = 1f;
				}else if ( _cut== 0f )
					clip(-1);
				return OUT;
				
			} 


ENDCG
		}// end pass
		 
		} // end sub shader
	FallBack "Specular"


}