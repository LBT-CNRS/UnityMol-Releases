/// @file FieldLineCg.shader
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
/// $Id: FieldLineCg.shader 210 2013-04-06 20:52:41Z baaden $
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

Shader "Custom/FieldLineCg" {

	Properties {
	//variable visite dans l'editeur
		_timeOff("Time",float)= 1.0 							// Temps en millisecond, il permet l'animation des lignes de champs
		_Speed("Speed",float)= 0.1333							// float qui regle la vitesse des lignes de champs
		_Density("Density",float)= 3.4							// float qui regule la densité des lignes de champs
		_Unsynchronize("Alea",float)= 1.0						// float désynchonisant les lignes de champs, evite que deux lignes bouge simultanement
		_Color("Color of particules",Color)= (1.0,1.0,1.0)		// couleur des lignes de champs
		_Length("Length of particules", float)= 0.7				// float reglant la longueur des lignes		
		_depthcut("depth", float) = 0							// profondeur de cut
		_adjust("adjust", float) = 40							// decalage du cut des lignes par rapport a celui de la surface
		_cut("mode",float) = 0 								// active ou non le cut des lignes de champs
		_cutplane("cutplane",Vector) = (0.0,0.0,0.0,1.0)		// cutplane static						
		_colormode("color", float) = 0							// activation du dégradé de couleur	
	    _BumpMap ("Bumpmap (RGB)", 2D) = "bump" {}	
	    _PosLookup("Pos Lookup", 2D) = "white" {}
	    _MinPosLookup("Min Pos Lookup", Vector) = (0.0,0.0,0.0)
	    _WidthPosLookup("Width Pos Lookup", Vector) = (0.0,0.0,0.0)

	}
	
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent-1" }
		
		
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct appdata members vertex)
#pragma exclude_renderers d3d11 xbox360
			
			#include "UnityCG.cginc"
			
			// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
			#pragma exclude_renderers gles
			// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct appdata members vertex)
			#pragma exclude_renderers xbox360
			
			// Setup
			#pragma target 3.0
			#pragma vertex FieldLine_v
			#pragma fragment FieldLine_p
			
			#pragma multi_compile_builtin
			
			// variable accessible dans unity

			float _timeOff;   			// animation grace au temps qui passe
			float _Speed;				// vitesse de l'animation
			float _Density;				// Nombre de particle visible par ligne
			float _Unsynchronize;		// nombre aléatoire decalant les particles
			float _Length;				// longueur des particles apparaissant sur les lignes
			sampler2D _MainTex;			
			float4 _Color;				// couleur des lignes
			uniform float4x4 modelViewProj;
			float _depthcut;			// profondeur de cut
			float _adjust;				// decalage des ligne de champs par rapport a la coupure de la surface
			float _cut;					// mode de coupure
			float4 _cutplane;			//plan de coupe(x,y,z,depthcut)
			float4 pos;
			float3 _SurfacePos;
			float _colormode; 			// aide a l'affichage du dégradé

			//Position lookup
			sampler2D _PosLookup;
			float3 _MinPosLookup;
			float3 _WidthPosLookup;
			
			struct appdata { 		// strucutre d'entré du shader, inutilisé au profit de l'entré standard
				float4 vertex ;
			};
					
			struct v2p {			// structure envoyer du vertex au pixel shader
	    		float4 textureCoordinates : TEXCOORD1;
//	    		float4 p : POSITION0 ;
	    		float4 worldPos : TEXCOORD0 ;
	    		float4 pos : SV_POSITION;
				};
			
			struct fragment_out    // le shader ne regles que la couleur du pixel.
			{
			  float4 Color    : COLOR0;
 			};	
				
						
										

// VERTEX SHADER IMPLEMENTATION =============================
			v2p FieldLine_v(appdata_full v)
			{
			
			v2p o; // Shader output
			
			o.textureCoordinates = v.texcoord ;  		// coordonnées sur la texture 
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);	// coordonné a l'ecran
			o.worldPos = v.vertex;						// coordonné réel (en fonction de la capmera

				
			return o;
			}
			
			

// PIXEL SHADER IMPLEMENTATION ===============================
			
			fragment_out FieldLine_p(v2p i)
			{
						
			fragment_out OUT;
			
			float val = fract((i.textureCoordinates.x+_Unsynchronize+(_timeOff*_Speed))*_Density); // recupere le decimal en fonction de la position le long de la ligne de champs
																								   // prend en compte tout les parametre de longueur, vitesse et densité ...
	
			//pos = mul (_Object2World, float4(i.worldPos.x+_SurfacePos.x,i.worldPos.y+_SurfacePos.y,i.worldPos.z+_SurfacePos.z,1.0));

			//Static cut
			if(_cut == 1){
				//Retrieve world position from the position lookup texture
				pos = tex2D(_PosLookup, i.textureCoordinates.xy);
				float3 wPos = (pos.xyz*_WidthPosLookup) + _MinPosLookup;
				float test = _cutplane.w + _cutplane.x*(wPos.x) + _cutplane.y*(wPos.y) + _cutplane.z*(wPos.z);
				
				if(test< 0)
				{
					clip(-1);
				}
			}

			if (_cut ==2){										// regule le cut des lignes de champs
//				clip (frac(-(_depthcut - _adjust + pos.z)/500) - 0.2);
				clip ((_depthcut - _adjust + pos.z));
//				if (frac(i.worldPos.z)<0.5){
//					clip(-1);
//				}
			}
			
			
			if (val < _Length)
			{
				discard;				// efface toute la partie de la ligne ayaant une val entre 0 et la longueur voulue
			}
			else
			{
				val = smoothstep(0.90, _Length, val);  // normalise le dégradé de val de 0,7 (Length) à 0,9 => 0 à 1
					
				// realisation de la trainée de la ligne de champs 
				//		||||||||||
				//		 ||||||||
				//		  ||||||
				//		   |||
				//			|
				//
			
				if ((val <0.4) && (i.textureCoordinates.y<0.6 ||i.textureCoordinates.y>0.4)) {
					discard;
				} else if ((val <0.5) && (i.textureCoordinates.y<0.5 ||i.textureCoordinates.y>0.5)) {
					discard;
				 
				} else if ((val <0.6) && (i.textureCoordinates.y<0.4 ||i.textureCoordinates.y>0.6)) {
					discard;
					
				} else if ((val <0.7) && (i.textureCoordinates.y<0.3 ||i.textureCoordinates.y>0.7)) {
					discard;
					
				}else if ((val <0.8) && (i.textureCoordinates.y<0.2 ||i.textureCoordinates.y>0.8)) {
					discard;
				}else if ((val <0.9) && (i.textureCoordinates.y<0.1 ||i.textureCoordinates.y>0.9)) {
					discard;
				
			 	}

			
				// arrondi de la tete de la ligne de champs

			 	else if ((val >0.99999) && (i.textureCoordinates.y<0.4 ||i.textureCoordinates.y>0.6)) {
					discard;
				}else if ((val >0.9999) && (i.textureCoordinates.y<0.3 ||i.textureCoordinates.y>0.7)) {
					discard;
				}else if ((val >0.9995) && (i.textureCoordinates.y<0.2 ||i.textureCoordinates.y>0.8)) {
					discard;
				}else if ((val >0.999) && (i.textureCoordinates.y<0.15 ||i.textureCoordinates.y>0.85)) {
					discard;
				}else if ((val >0.995) && (i.textureCoordinates.y<0.10 ||i.textureCoordinates.y>0.9)) {
					discard;
				}

				// coloration en degradé de la lignes grave au dégradé de val realisé au debut
				float3 col = _Color* val;
				
				// couleur dégrader bleu a rouge
				if (_colormode ==0){
					if (i.textureCoordinates.x < 0.142857142857143)
						col = float3(1,0,0);
					else if (i.textureCoordinates.x < 2*0.142857142857143)
						col = float3(1,0.33,0.33);
					else if (i.textureCoordinates.x < 3*0.142857142857143)
						col = float3(1,0.66,0.66);
					else if (i.textureCoordinates.x < 4*0.142857142857143)
						col = float3(1,1,1);
					else if (i.textureCoordinates.x < 5*0.142857142857143)
						col = float3(0.66,0.66,1);
					else if (i.textureCoordinates.x < 6*0.142857142857143)
						col = float3(0.33,0.33,1);
					else
						col = float3(0,0,1);
				}
				
				OUT.Color.rgb = col;
				OUT.Color.a = val;	
			}
			return OUT;
		}
		

		
		
	ENDCG		
		
	}	
	
		
		
						
																
	} 
	FallBack "Diffuse"
}

