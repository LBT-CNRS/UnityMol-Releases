/// @file FVNano_SingleCubeSphere.shader
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
/// $Id: FVNano_SingleCubeSphere.shader 209 2013-04-06 20:51:49Z baaden $
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

// Upgrade NOTE: replaced 'glstate.matrix.invtrans.modelview[0]' with 'UNITY_MATRIX_IT_MV'
// Upgrade NOTE: replaced 'glstate.matrix.modelview[0]' with 'UNITY_MATRIX_MV'
// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'

// $Id: FVNano_SingleCubeSphere.shader 209 2013-04-06 20:51:49Z baaden $
// (c) 2009 by Marc Baaden, <baaden@smplinux.de>
// Unity3D FvNano shaders for rapid prototyping and debugging
//
// On a un cube (mesh) dans la scene -> vertex
// la spherepos du cube est 0, 0, 0


Shader "FvNano/Single Cube Sphere Raycasting old" {


// Proprietes exposees a l'interface graphique
Properties {
	_Rayon ("Rayon de la Sphere", float) = 0.5
    _Color ("Couleur de la Sphere", Color) = (1,0.1,0.1,1.0)
    _TexPos ("Position de la sphere", Vector) = (0,0,0,1.0)     // important que w != 0 !!
    _Visibilite ("Visibilite de la Sphere", float) = 1
    _Light ("Light vector", Vector) = (1,0,0,0)
    _normMax ("Valeur de normMax", float) = 1.0
}


// ==========================================================
// L'actuel Shader Cg =======================================
// ==========================================================

SubShader {
    Pass {

CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct appdata members vertex)
#pragma exclude_renderers d3d11 xbox360
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct appdata members vertex)
#pragma exclude_renderers xbox360

// Setup
#pragma vertex ballimproved_v
#pragma fragment ballimproved_p


// Variables modifiables dans Unity3D
float _Rayon;
float4 _Color;
float4 _TexPos;
float _Visibilite;
float4 _Light;
float _normMax;


// vertex input: position
struct appdata {
    float4 vertex;
};


// Variables passees du vertex au pixel shader
struct v2p {
    float4 ppos         : POSITION;						// pixel position en sortie
    float3 spherepos : TEXCOORD4;						// position de l'atome correspondant
    float3 V :TEXCOORD3;								// ?? view direction de la camera ??
    float3 rayorigin         : TEXCOORD2;				// origine du rayon
    float3 pnorm	   : TEXCOORD1;						// pixel normal
    half4 pcolor       : COLOR0;						// pixel color
};


// VERTEX SHADER IMPLEMENTATION =============================

v2p ballimproved_v (
	appdata v
//    float2 id		:TEXCOORD0,								// id dans la texture
//    uniform samplerRECT texturePositions :TEXUNIT0,		// position de tous les atomes dans une texture -> _TexPos
//    uniform samplerRECT textureColors :TEXUNIT1,			// couleurs de tous les atomes dans une texture -> _Color
//    uniform samplerRECT textureSizes :TEXUNIT2,			// rayon de chaque atome dans une texture -> _Rayon
//    uniform samplerRECT textureVisibilities :TEXUNIT3,	// visibilite de tous les atomes dans une texture -> Visibilite
)

{
	// OpenGL matrices
	float4x4 ModelViewProj = UNITY_MATRIX_MVP;			// matrice pour passer dans les coordonnees de l'ecran
	float4x4 ModelView = UNITY_MATRIX_MV;		// matrice camera
	float4x4 ModelViewIT = UNITY_MATRIX_IT_MV;
															// matrice inverse transposee de la camera

    v2p o; // Shader output

	float4 spaceposition;									// position du vertex dans l'espace
    half3 normal = v.vertex.xyz;							// la normale est fonction de la position
	
    if(_normMax != 0)										// si ce n'est pas la premiere iteration, on rentre ici
    {
        // Scale from [0,1] to [-normax, normax]
        spaceposition = _TexPos;							// position 3D de l'atome dans la texture

        // Scale atoms
        spaceposition.xyz += v.vertex.xyz*_Rayon*1.42;		// on ajoute la position relative du vertex en tenant
															// compte du rayon
        //spaceposition.a = 1;
    } else {												// premiere iteration ??
        // Do Nothing
        spaceposition = v.vertex;
    }
	
    o.pnorm = mul((float3x3)ModelViewIT, normal);			// on inverse la position du vertex pour le pixel
    o.ppos = mul(ModelViewProj, spaceposition);				// on projete la position dans la camera

    o.pcolor.rgba = _Color;									// recuperation de la couleur

    float visibility = _Visibilite;							// ..et de la visibilite
    if(visibility <= 0.0)
    {
        o.pcolor.a = 0.0;									// pas visible, donc alpha=0, completement transparent
															// peut-etre une possibilite d'optimisation ici ??
    }

    o.rayorigin = float3(0,0,0);							// origine du rayon a lancer

    //V=spaceposition.xyz;
    float4 V4=mul(ModelView, spaceposition);				// projection de la position du vertex dans la camera
    o.V = V4.xyz / V4.w;									// on garde une metrique
	
    float4 s4=mul(ModelView, _TexPos);						// projette la position de la sphere
    o.spherepos = s4.xyz/s4.w;								// garder la metrique

    return o;	// on envoie tout cela au pixel shader
}



// PIXEL SHADER IMPLEMENTATION ===============================

half4 ballimproved_p (v2p i
) : COLOR
{
	float3 light = _Light.xyz;
	float4x4 ModelViewIT = UNITY_MATRIX_IT_MV;

	float4 pcolor2;                                      // pixel color 2

	// get and normalize view direction
	float3 raydir = normalize(i.V);                      // direction du rayon
  
	// compute sphere dir
	float3 spheredir = i.spherepos - i.rayorigin.xyz;    // ?? direction de sphere ??

	//calcul du discriminant
	float scal = dot(raydir, spheredir);                 // produit scalaire des directions rayon et sphere
	float V2 = dot(raydir,raydir);                       // V2, produit scalaire d'un vecteur normalise ?? ne devrait'ce pas etre egal unite ??
	float delta = 4 * pow(scal,2) - 4* V2 * (dot(spheredir, spheredir) - pow(_Rayon,2));
                                                         // delta, discriminant
                                                         // 
														 //          2                                                2
														 //  4 x scal   -  4 x V2 x ( spheredir . spheredir  -  rayon   )
                                                         // 
														 // si on ne s'est pas trompe, c'est egal a  (V2 == unite)
                                                         // 
														 //          2                                           2
														 //  4 x scal   -  4 x ( spheredir . spheredir  -  rayon   )
														 //
														 // calcul l'intersection entre la quadrique et le rayon

	if(delta >= 0){
		float k1 = (-2*scal - sqrt(delta)) / (2*V2);     // premiere solution/intersection
		float k2 = (-2*scal + sqrt(delta)) / (2*V2);     // deuxieme solution/intersection
		float km =min(k1, k2);							 // point minimum des intersections (1D)
		float kM =max(k1, k2);							 // point maximum des intersections
		if(km > 0.0)                                     // front/near clipping plane ??
			km = kM;

		// Intersection de la sphere avec le rayon
		float3 X = -km*raydir + i.rayorigin;			 // point d'intersection 3D
		float3 norm = X - i.spherepos;					 // sa normale

// GESTION DE LA LUMIERE - debut
//		Lighting Antoine
//		float3 N = normalize(norm);						 // la normale normalisee
//		half3 lightNorm = normalize(i.light);				 // norme de la direction de lumiere
//		half diffuse = saturate(dot(N,lightNorm));		 // terme diffus
//		pcolor2.rgb = i.pcolor.xyz*0.5 + 0.5*diffuse*float3(1,1,1);
	
//		Lighting Matthieu
		float3 N = -normalize(mul(ModelViewIT,float4(norm.xyz,1.0)).xyz);
		half3 lightNorm = float3(1,1,1);
		float3 eyeposition = float3(0,0,1);
		float3 matthieu = lightNorm + eyeposition;
		float3 halfVec = normalize(matthieu);
		float diffuse = dot(N,lightNorm);
		float specular = dot(N,halfVec);
		float4 lighting = lit(diffuse,specular,32);
		pcolor2.rgb = i.pcolor.xyz*lighting.y + lighting.z*float3(1,1,1);	
// GESTION DE LA LUMIERE - fin

		pcolor2.a = 1;									 // pixel opaque, aucune transparence
	
	} else { // pas d'intersection
		//pcolor2 = float4(1, 1, 0,0);
		discard;										 // le pixel n'est pas dessine
	}
 
	//pcolor2.rgb = V;
	return pcolor2;										 // on retourne la couleur du pixel

}
ENDCG
    }



} 
// Pour les cartes graphiques ne supportant pas nos Shaders
Fallback "VertexLit"
}