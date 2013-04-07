/// @file Normal-BumpSpec.shader
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
/// $Id: Normal-BumpSpec.shader 210 2013-04-06 20:52:41Z baaden $
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

Shader "Bumped Specular cut" {
Properties {
	_Color ("Main Color", Color) = (0.5,0.5,0.5,1) 				//couleur général de la surface 
	_ColorIN ("Color Cut" , Color) = (0.5,0.5,0.5,1) 			// Couleur du plan de coupe
	_SpecColor ("Specular Color", Color) = (0, 0, 0, 1)		// Couleur des reflet de la surface
   	_depthcut("depth", float) = 0							// distance de cut de la surface
	_cut("Active cut", float) = 0							// active ou non le cut
	_cutX("axe X", float)=1
	_cutY("axe Y", float)=1									// axe pour le cut fixe
	_cutZ("axe Z", float)=1
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
}

SubShader {
	Tags { "RenderType"="Overlay" }
            /* Upgrade NOTE: commented out, possibly part of old style per-pixel lighting: Blend AppSrcAdd AppDstAdd */
    Fog { Color [_AddFog] } 
    
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
//					OUT.Color.rgb = float3(102f/255,102f/255,102f/255);
					OUT.Color = _ColorIN;
	
			OUT.Color.a = 1f;
				}else if ( _cut== 0f )
					clip(-1);
				return OUT;
				
			} 


ENDCG
		} //   end   PASS
	 		

	 	Cull back
		CGPROGRAM
      	#pragma surface surf BlinnPhong
		#pragma target 3.0
		
	
		// délcaration des variables utiles pour le calcul de la surface et de son cut
		sampler2D _MainTex;
		sampler2D _BumpMap;
		fixed4 _Color;
		float _cut;
		float4 pos;
		float _depthcut;
		float 	_cutX;
		float 	_cutY;
		float 	_cutZ;
		float _texture;
		
		
		// structure d'entrée du shader
		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
		    float3 worldPos;
		
		};
	    
	      
	     // Surface shader remplace le vertex et le pixel shader en un unique shader de surface
		void surf (Input IN, inout SurfaceOutput o) {
		
		

		
				pos = mul (UNITY_MATRIX_MVP, float4(IN.worldPos,0f)); // calcul la position des pixel dans l'espace  camera/
				if ( _cut== 1f ){ // active le cut fixe
	//				if ( (_depthcut + _cutX *IN.worldPos.x+ _cutY *IN.worldPos.y+ _cutZ * IN.worldPos.z) / 500 < 0 ){
	//					clip(-1);
						if ((_depthcut + _cutX*IN.worldPos.x + _cutY*IN.worldPos.y + _cutZ*IN.worldPos.z)<0){
							clip(-1);
						}//else {
	//						clip(_depthcut + _cutX*IN.worldPos.x + _cutY*IN.worldPos.y + _cutZ*IN.worldPos.z);
	//						}
							//				clip(-1);
	//			}
				}else if ( _cut== 2f ){
					clip (frac(-(_depthcut + pos.z)/500) -0.5);
				}
				
				
				// apparence de la surface
				fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
				o.Albedo = tex.rgb * _Color.rgb;
//				o.Albedo = 1;
	
//				o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
	
				o.Gloss = tex.a;
				o.Alpha = tex.a * _Color.a;
				o.Specular = 0.1;
				o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
				
			}
				
			
		ENDCG
	}
	FallBack "Specular"

}
