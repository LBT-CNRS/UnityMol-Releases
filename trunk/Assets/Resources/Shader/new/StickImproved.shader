/// @file StickImproved.shader
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
/// $Id: StickImproved.shader 210 2013-04-06 20:52:41Z baaden $
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
// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'

// $Id: StickImproved.shader 210 2013-04-06 20:52:41Z baaden $
// (c) 2010 by Marc Baaden & Matthieu Chavent, <baaden@smplinux.de> <matthieu.chavent@free.fr>
// Unity3D FvNano shaders coming from FVNano project
//
// On a un cube (mesh) dans la scene -> vertex
// la spherepos du cube est 0, 0, 0

Shader "FvNano/Stick HyperBalls OpenGL" {

// Proprietes exposees a l'interface graphique
Properties {
	_Rayon1 ("Rayon de la Sphere1", float) = 0.1
	_Rayon2 ("Rayon de la Sphere2", float) = 0.1
    _Color ("Couleur du lien", Color) = (1,0.1,0.1,1.0)
    _Color2 ("Couleur du lien2", Color) = (1,0.1,0.1,1.0)
    _TexPos1 ("Position de la Sphere 1", Vector) = (0.0,0.0,0.0,1.0)     // important que w != 0 !!
    _TexPos2 ("Position de la Sphere 2", Vector) = (3.0,0.0,0.0,1.0)     // important que w != 0 !!
    _Shrink ("Shrink Factor", float) = 0.1
    _Visibilite ("Visibilite de l'hyperbole", float) = 1.0
    _Light ("Light vector", Vector) = (1,0,0,0)
    _Scale("Link Scale", float) = 1.0
    _EllipseFactor("Ellipse Factor", float) = 1.0
	_MatCap ("MatCap (RGB)", 2D) = "white" {}
}


// ==========================================================
// L'actuel Shader Cg =======================================
// ==========================================================

SubShader {
    Pass {

CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct appdata members vertex)
#pragma exclude_renderers xbox360

// Setup
#pragma target 3.0
#pragma vertex stickimproved_v
#pragma fragment stickimproved_p
#pragma multi_compile_builtin 

// Variables modifiables dans Unity3D
float _Rayon1;
float _Rayon2;
float4 _Color;
float4 _Color2;
float4 _TexPos1;
float4 _TexPos2;
float _Visibilite;
float4 _Light;
float _Shrink;
float _Scale;
float _EllipseFactor;
uniform sampler2D _MatCap;


//uniform float4x4 _matMVPI;
        
// vertex input: position
struct appdata {
    float4 vertex : POSITION;
};


// Variables passees du vertex au pixel shader
struct v2p {
    float4 p         		: POSITION;
    float4x4 matrix_near	   		: TEXCOORD2;
    float2x4 matrix_cutoff	   		: TEXCOORD6;
	float4 color_atom1			: TEXCOORD0;
	float shrink			: TEXCOORD1;
};

struct fragment_out 
{
  float4 Color : COLOR0;
  float depth  : DEPTH;
};

struct Ray {
   float3 origin ;
   float3 direction ;
};

//inverse function
float4x4 mat_inverse(float4x4 matrix) {
	
	float4x4 inv; 
	
	float det = matrix[0][0]*matrix[1][1]*matrix[2][2]*matrix[3][3] + matrix[0][0]*matrix[1][2]*matrix[2][3]*matrix[3][1] + matrix[0][0]*matrix[1][3]*matrix[2][1]*matrix[3][2] + 
			    matrix[0][1]*matrix[1][0]*matrix[2][3]*matrix[3][2] + matrix[0][1]*matrix[1][2]*matrix[2][0]*matrix[3][3] + matrix[0][1]*matrix[1][3]*matrix[2][2]*matrix[3][0] + 
			    matrix[0][2]*matrix[1][0]*matrix[2][1]*matrix[3][3] + matrix[0][2]*matrix[1][1]*matrix[2][3]*matrix[3][0] + matrix[0][2]*matrix[1][3]*matrix[2][0]*matrix[3][1] + 	
			    matrix[0][3]*matrix[1][0]*matrix[2][2]*matrix[3][1] + matrix[0][3]*matrix[1][1]*matrix[2][0]*matrix[3][2] + matrix[0][3]*matrix[1][2]*matrix[2][1]*matrix[3][0] - 	
	            matrix[0][0]*matrix[1][1]*matrix[2][3]*matrix[3][2] - matrix[0][0]*matrix[1][2]*matrix[2][1]*matrix[3][3] - matrix[0][0]*matrix[1][3]*matrix[2][2]*matrix[3][1] - 
	            matrix[0][1]*matrix[1][0]*matrix[2][2]*matrix[3][3] - matrix[0][1]*matrix[1][2]*matrix[2][3]*matrix[3][0] - matrix[0][1]*matrix[1][3]*matrix[2][0]*matrix[3][2] -
	            matrix[0][2]*matrix[1][0]*matrix[2][3]*matrix[3][1] - matrix[0][2]*matrix[1][1]*matrix[2][0]*matrix[3][3] - matrix[0][2]*matrix[1][3]*matrix[2][1]*matrix[3][0] -	
	            matrix[0][3]*matrix[1][0]*matrix[2][1]*matrix[3][2] - matrix[0][3]*matrix[1][1]*matrix[2][2]*matrix[3][0] - matrix[0][3]*matrix[1][2]*matrix[2][0]*matrix[3][1] ;	
	
	inv[0][0] = (1/det)*( matrix[1][1]*matrix[2][2]*matrix[3][3] + matrix[1][2]*matrix[2][3]*matrix[3][1] + matrix[1][3]*matrix[2][1]*matrix[3][2] - 
						  matrix[1][1]*matrix[2][3]*matrix[3][2] - matrix[1][2]*matrix[2][1]*matrix[3][3] - matrix[1][3]*matrix[2][2]*matrix[3][1] ) ; 

	inv[0][1] = (1/det)*( matrix[0][1]*matrix[2][3]*matrix[3][2] + matrix[0][2]*matrix[2][1]*matrix[3][3] + matrix[0][3]*matrix[2][2]*matrix[3][1] - 
						  matrix[0][1]*matrix[2][2]*matrix[3][3] - matrix[0][2]*matrix[2][3]*matrix[3][1] - matrix[0][3]*matrix[2][1]*matrix[3][2] ) ; 

	inv[0][2] = (1/det)*( matrix[0][1]*matrix[1][2]*matrix[3][3] + matrix[0][2]*matrix[1][3]*matrix[3][1] + matrix[0][3]*matrix[1][1]*matrix[3][2] - 
						  matrix[0][1]*matrix[1][3]*matrix[3][2] - matrix[0][2]*matrix[1][1]*matrix[3][3] - matrix[0][3]*matrix[1][2]*matrix[3][1] ) ; 
						  
	inv[0][3] = (1/det)*( matrix[0][1]*matrix[1][3]*matrix[2][2] + matrix[0][2]*matrix[1][1]*matrix[2][3] + matrix[0][3]*matrix[1][2]*matrix[2][1] - 
						  matrix[0][1]*matrix[1][2]*matrix[2][3] - matrix[0][2]*matrix[1][3]*matrix[2][1] - matrix[0][3]*matrix[1][1]*matrix[2][2] ) ; 						 

	inv[1][0] = (1/det)*( matrix[1][0]*matrix[2][3]*matrix[3][2] + matrix[1][2]*matrix[2][0]*matrix[3][3] + matrix[1][3]*matrix[2][2]*matrix[3][0] - 
						  matrix[1][0]*matrix[2][2]*matrix[3][3] - matrix[1][2]*matrix[2][3]*matrix[3][0] - matrix[1][3]*matrix[2][0]*matrix[3][2] ) ; 

	inv[1][1] = (1/det)*( matrix[0][0]*matrix[2][2]*matrix[3][3] + matrix[0][2]*matrix[2][3]*matrix[3][0] + matrix[0][3]*matrix[2][0]*matrix[3][2] - 
						  matrix[0][0]*matrix[2][3]*matrix[3][2] - matrix[0][2]*matrix[2][0]*matrix[3][3] - matrix[0][3]*matrix[2][2]*matrix[3][0] ) ; 

	inv[1][2] = (1/det)*( matrix[0][0]*matrix[1][3]*matrix[3][2] + matrix[0][2]*matrix[1][0]*matrix[3][3] + matrix[0][3]*matrix[1][2]*matrix[3][0] - 
						  matrix[0][0]*matrix[1][2]*matrix[3][3] - matrix[0][2]*matrix[1][3]*matrix[3][0] - matrix[0][3]*matrix[1][0]*matrix[3][2] ) ; 
						  
	inv[1][3] = (1/det)*( matrix[0][0]*matrix[1][2]*matrix[2][3] + matrix[0][2]*matrix[1][3]*matrix[2][0] + matrix[0][3]*matrix[1][0]*matrix[2][2] - 
						  matrix[0][0]*matrix[1][3]*matrix[2][2] - matrix[0][2]*matrix[1][0]*matrix[2][3] - matrix[0][3]*matrix[1][2]*matrix[2][0] ) ; 	
						  
	inv[2][0] = (1/det)*( matrix[1][0]*matrix[2][1]*matrix[3][3] + matrix[1][1]*matrix[2][3]*matrix[3][0] + matrix[1][3]*matrix[2][0]*matrix[3][1] - 
						  matrix[1][0]*matrix[2][3]*matrix[3][1] - matrix[1][1]*matrix[2][0]*matrix[3][3] - matrix[1][3]*matrix[2][1]*matrix[3][0] ) ; 

	inv[2][1] = (1/det)*( matrix[0][0]*matrix[2][3]*matrix[3][1] + matrix[0][1]*matrix[2][0]*matrix[3][3] + matrix[0][3]*matrix[2][1]*matrix[3][0] - 
						  matrix[0][0]*matrix[2][1]*matrix[3][3] - matrix[0][1]*matrix[2][3]*matrix[3][0] - matrix[0][3]*matrix[2][0]*matrix[3][1] ) ; 

	inv[2][2] = (1/det)*( matrix[0][0]*matrix[1][1]*matrix[3][3] + matrix[0][1]*matrix[1][3]*matrix[3][0] + matrix[0][3]*matrix[1][0]*matrix[3][1] - 
						  matrix[0][0]*matrix[1][3]*matrix[3][1] - matrix[0][1]*matrix[1][0]*matrix[3][3] - matrix[0][3]*matrix[1][1]*matrix[3][0] ) ; 
						  
	inv[2][3] = (1/det)*( matrix[0][0]*matrix[1][3]*matrix[2][1] + matrix[0][1]*matrix[1][0]*matrix[2][3] + matrix[0][3]*matrix[1][1]*matrix[2][0] - 
						  matrix[0][0]*matrix[1][1]*matrix[2][3] - matrix[0][1]*matrix[1][3]*matrix[2][0] - matrix[0][3]*matrix[1][0]*matrix[2][1] ) ;						  
	
	inv[3][0] = (1/det)*( matrix[1][0]*matrix[2][2]*matrix[3][1] + matrix[1][1]*matrix[2][0]*matrix[3][2] + matrix[1][2]*matrix[2][1]*matrix[3][0] - 
						  matrix[1][0]*matrix[2][1]*matrix[3][2] - matrix[1][1]*matrix[2][2]*matrix[3][0] - matrix[1][2]*matrix[2][0]*matrix[3][1] ) ; 

	inv[3][1] = (1/det)*( matrix[0][0]*matrix[2][1]*matrix[3][2] + matrix[0][1]*matrix[2][2]*matrix[3][0] + matrix[0][2]*matrix[2][0]*matrix[3][1] - 
						  matrix[0][0]*matrix[2][2]*matrix[3][1] - matrix[0][1]*matrix[2][0]*matrix[3][2] - matrix[0][2]*matrix[2][1]*matrix[3][0] ) ; 

	inv[3][2] = (1/det)*( matrix[0][0]*matrix[1][2]*matrix[3][1] + matrix[0][1]*matrix[1][0]*matrix[3][2] + matrix[0][2]*matrix[1][1]*matrix[3][0] - 
						  matrix[0][0]*matrix[1][1]*matrix[3][2] - matrix[0][1]*matrix[1][2]*matrix[3][0] - matrix[0][2]*matrix[1][0]*matrix[3][1] ) ; 
						  
	inv[3][3] = (1/det)*( matrix[0][0]*matrix[1][1]*matrix[2][2] + matrix[0][1]*matrix[1][2]*matrix[2][0] + matrix[0][2]*matrix[1][0]*matrix[2][1] - 
						  matrix[0][0]*matrix[1][2]*matrix[2][1] - matrix[0][1]*matrix[1][0]*matrix[2][2] - matrix[0][2]*matrix[1][1]*matrix[2][0] ) ;

	return inv ;
	
}


bool cutoff_plane (float3 M, float3 cutoff, float3 x3) {
	float l = x3.x*M.x + x3.y*M.y + x3.z*M.z -x3.x*cutoff.x -x3.y*cutoff.y -x3.z*cutoff.z;
	if (l<0.0) { return true; }
	else { return false;}
}


//ray-quadric intersection function
float3 isect_surf(Ray r, float4x4 matrix_coef) {
   float4 direction = float4(r.direction, 0.0);
   float4 origin = float4(r.origin, 1.0);
   
   float a = dot(direction,mul(matrix_coef,direction)) ;
   float b = dot(origin,mul(matrix_coef,direction)) ;
   float c = dot(origin,mul(matrix_coef,origin));

   float delta = b*b -  a * c ;
   if (delta<0) clip(-1);
   
   float t1 = (-b-sqrt(delta)) / a  ;
   //float t2 = (-b+sqrt(delta)) / a  ;	  
   //float t =(t1<t2) ? t1 : t2; 	
   return r.origin + t1 * r.direction ;
}


// Launches a primary ray in world-space through *this* fragment.
Ray primary_ray(float4 near1, float4 far1) {
    float3 near = near1.xyz / near1.w ;
    float3 far = far1.xyz / far1.w ;

    return Ray(near, far - near) ;
}


// Updates the Z-buffer according to the world-space point M.
float update_z_buffer(float3 M, float4x4 ModelViewP) {
	float  depth1;
	float4 Ms = mul(ModelViewP,float4(M,1.0)) ;
    return depth1 = (1.0 + Ms.z / Ms.w) / 2.0 ;    
}


// VERTEX SHADER IMPLEMENTATION =============================

v2p stickimproved_v (appdata v)

{
	// OpenGL matrices
	float4x4 ModelViewProj = UNITY_MATRIX_MVP;			// matrice pour passer dans les coordonnees de l'ecran
	float4x4 ModelViewProjI = mat_inverse(ModelViewProj);

    v2p o; // Shader output

	float4 vertex_position;
 	float long;
	float4 spaceposition;
	
	//Calculate all the stuffs to create parallepipeds that defines the enveloppe for ray-casting
	
	float radius1 = _Rayon1*_Scale;
	float radius2 = _Rayon2*_Scale;

    float4 position_atom1 = float4(0.0,0.0,0.0,0);
    float4 position_atom2 = _TexPos2-_TexPos1;

	o.color_atom1 = _Color;
	float4 color_atom2 = _Color2;
	
	float distance = sqrt( (position_atom1.x - position_atom2.x)*(position_atom1.x - position_atom2.x) + (position_atom1.y - position_atom2.y)*(position_atom1.y - position_atom2.y) + (position_atom1.z - position_atom2.z)*(position_atom1.z - position_atom2.z) );

	spaceposition.z = v.vertex.z * distance;
	
	if (radius1 > radius2) {
		spaceposition.y = v.vertex.y * 2.0 * radius1;
		spaceposition.x = v.vertex.x * 2.0 * radius1;
	} else {
		spaceposition.y = v.vertex.y * 2.0 * radius2;
		spaceposition.x = v.vertex.x * 2.0 * radius2;		
	}
    spaceposition.w = 1;
	
	o.shrink = _Shrink;
	float shrinkfactor = o.shrink;
	
	float4 e3;
	e3.xyz = normalize(position_atom1-position_atom2);
	if (e3.z == 0.0) { e3.z = 0.0000000000001;}
	if ( (position_atom1.x - position_atom2.x) == 0.0) { position_atom1.x += 0.001;}
    if ( (position_atom1.y - position_atom2.y) == 0.0) { position_atom1.y += 0.001;}
    if ( (position_atom1.z - position_atom2.z) == 0.0) { position_atom1.z += 0.001;}
	
	float4 focus;
	focus.x = ( position_atom1.x*position_atom1.x - position_atom2.x*position_atom2.x + ( radius2*radius2 - radius1*radius1 )*e3.x*e3.x/shrinkfactor )/(2.0*(position_atom1.x - position_atom2.x));
	focus.y = ( position_atom1.y*position_atom1.y - position_atom2.y*position_atom2.y + ( radius2*radius2 - radius1*radius1 )*e3.y*e3.y/shrinkfactor )/(2.0*(position_atom1.y - position_atom2.y));
	focus.z = ( position_atom1.z*position_atom1.z - position_atom2.z*position_atom2.z + ( radius2*radius2 - radius1*radius1 )*e3.z*e3.z/shrinkfactor )/(2.0*(position_atom1.z - position_atom2.z));
			
	float3 e1;
	e1.x = 1.0;
	e1.y = 1.0;
	e1.z = ( (e3.x*focus.x + e3.y*focus.y + e3.z*focus.z) - e1.x*e3.x - e1.y*e3.y)/e3.z;
	float3 e1_temp = e1 - focus.xyz;
	e1 = normalize(e1_temp);
	float3 e2 = normalize(cross(e1,e3));
	
	float3 colonne1 = float3(e1.x,e2.x,e3.x);
	float3 colonne2 = float3(e1.y,e2.y,e3.y);
	float3 colonne3 = float3(e1.z,e2.z,e3.z);	
		
	float3x3 R= float3x3(colonne1, colonne2, colonne3);			
 	// ROTATION:		
	vertex_position.xyz = mul((float3x3)R,(float3)spaceposition.xyz);
	vertex_position.w = 1.0;

	// TRANSLATION:
	vertex_position.x +=  (position_atom1.x+position_atom2.x)/2;
	vertex_position.y +=  (position_atom1.y+position_atom2.y)/2;
	vertex_position.z +=  (position_atom1.z+position_atom2.z)/2;

	o.p = mul(ModelViewProj, vertex_position);
	
	// Calculate origin and direction of ray that we pass to the fragment ----
	float4 i_near, i_far; 
	float4 near = o.p ; 
	
	near.z = 0.0 ;
	near = mul(ModelViewProjI, near) ;
	float4 far = o.p ; 
	far.z = far.w ;
	i_far = mul(ModelViewProjI,far) ;
	i_near = near;

	e3.w = color_atom2.x;
	float4 prime1, prime2; 
	prime1.xyz = position_atom1 - (position_atom1 - focus.xyz)*o.shrink;
	prime2.xyz = position_atom2 - (position_atom2 - focus.xyz)*o.shrink;
	prime1.w = color_atom2.y;
	prime2.w = color_atom2.z;
	o.matrix_cutoff = float2x4(prime1, prime2);		
	//o.matrix_cutoff = float2x4(position_atom1, position_atom2);		

	float Rcarre  = (radius1*radius1/shrinkfactor) - ( (position_atom1.x - focus.x)*(position_atom1.x - focus.x) + (position_atom1.y - focus.y)*(position_atom1.y - focus.y) + (position_atom1.z - focus.z)*(position_atom1.z - focus.z) );
	focus.w = Rcarre;
	o.matrix_near = float4x4(i_near,i_far, focus, e3);	
	
    return o;
}



// PIXEL SHADER IMPLEMENTATION ===============================

fragment_out stickimproved_p (v2p i)
{
	float3 light = _Light.xyz;
	float4x4 ModelViewProj = UNITY_MATRIX_MVP;			// matrice pour passer dans les coordonnees de l'ecran
	float4x4 ModelViewIT = UNITY_MATRIX_IT_MV; 

 	fragment_out OUT;
	
	float4 i_near = i.matrix_near._m00_m01_m02_m03;
	float4 i_far  = i.matrix_near._m10_m11_m12_m13;
	float4 focus = i.matrix_near._m20_m21_m22_m23;
	float3 e3 = i.matrix_near._m30_m31_m32;

	float3 color_atom2;
	color_atom2.x = i.matrix_near._m33;
	color_atom2.y = i.matrix_cutoff._m03;
	color_atom2.z = i.matrix_cutoff._m13;

	float3 e1;
	e1.x = 1.0;
 	e1.y = 1.0;
	e1.z = ( (e3.x*focus.x + e3.y*focus.y + e3.z*focus.z) - e1.x*e3.x - e1.y*e3.y)/e3.z; 
 	float3 e1_temp = e1 - focus.xyz;		
 	e1 = normalize(e1_temp);
 	float3 e2 = normalize(cross(e1,e3));
 	float3 cutoff1 = i.matrix_cutoff._m00_m01_m02;
	float3 cutoff2 = i.matrix_cutoff._m10_m11_m12;

 	float4 equation = focus;
 	float shrinkfactor = i.shrink;
	float t1 = -1/(1-shrinkfactor);
	float t2 = 1/(shrinkfactor);	
 	float4 colonne1, colonne2, colonne3, colonne4;
	float3 equation1 = float3(t2,t2*_EllipseFactor,t1);

	float A1 = - e1.x*equation.x - e1.y*equation.y - e1.z*equation.z; 
	float A2 = - e2.x*equation.x - e2.y*equation.y - e2.z*equation.z; 
	float A3 = - e3.x*equation.x - e3.y*equation.y - e3.z*equation.z; 

	float A11 = equation1.x*e1.x*e1.x +  equation1.y*e2.x*e2.x + equation1.z*e3.x*e3.x;
	float A21 = equation1.x*e1.x*e1.y +  equation1.y*e2.x*e2.y + equation1.z*e3.x*e3.y;
	float A31 = equation1.x*e1.x*e1.z +  equation1.y*e2.x*e2.z + equation1.z*e3.x*e3.z;
	float A41 = equation1.x*e1.x*A1   +  equation1.y*e2.x*A2   + equation1.z*e3.x*A3;

	float A22 = equation1.x*e1.y*e1.y +  equation1.y*e2.y*e2.y + equation1.z*e3.y*e3.y;
	float A32 = equation1.x*e1.y*e1.z +  equation1.y*e2.y*e2.z + equation1.z*e3.y*e3.z;
	float A42 = equation1.x*e1.y*A1   +  equation1.y*e2.y*A2   + equation1.z*e3.y*A3;

	float A33 = equation1.x*e1.z*e1.z +  equation1.y*e2.z*e2.z + equation1.z*e3.z*e3.z;
	float A43 = equation1.x*e1.z*A1   +  equation1.y*e2.z*A2   + equation1.z*e3.z*A3;

	float A44 = equation1.x*A1*A1     +  equation1.y*A2*A2     + equation1.z*A3*A3 - equation.w;

    colonne1 = float4(A11,A21,A31,A41);
    colonne2 = float4(A21,A22,A32,A42);
    colonne3 = float4(A31,A32,A33,A43);
    colonne4 = float4(A41,A42,A43,A44);
    float4x4 mat = float4x4(colonne1,colonne2,colonne3,colonne4);		
	Ray ray = primary_ray(i_near,i_far) ;

    float3 M = isect_surf(ray, mat);	   
  	OUT.depth = update_z_buffer(M, ModelViewProj) ;

	if(shrinkfactor <0.0){discard;}
  	if (cutoff_plane(M, cutoff1, -e3) || cutoff_plane(M, cutoff2, e3)){discard;}

	//------------ blinn phong light try ------------------------
	//Transform normal to model space to view-space
    float4 M1 = float4(M,1.0);
	float3 normal = normalize(mul(ModelViewIT,mul(mat,M1)).xyz);
	float3 lightvec = normalize(float3(0.0,0.0,1.2));
	float3 eyepos = float3(0.0,0.0,1.0);
	float3 halfvec = normalize(lightvec + eyepos);
	float diffuse = dot(normal,lightvec);
	//float specular = dot(normal, halfvec);
	float shininess = 200.0 ;
	
	float specular = pow(max(dot(normal, halfvec),0.0),shininess);
	
    //float4 lighting = lit(diffuse, specular, 32);
   	float a = ((M.x-cutoff2.x)*e3.x + (M.y-cutoff2.y)*e3.y +(M.z-cutoff2.z)*e3.z)/distance(cutoff2,cutoff1); 
	float3 atom_color = lerp( color_atom2, i.color_atom1, a );    
   
   
    float3 diffusecolor= atom_color; 
	float3 specularcolor = float3(1.0,1.0,1.0);    
	
// //	if(dot(normal,eyepos) < 0.5)
// //		OUT.Color.rgb = float3(0,0,0);
// //	else{
	float4 col;
//MB - activate/deactivate specular color manually according to your
//preference
    col.rgb = diffuse * diffusecolor;// + specular*specularcolor;
    //col.rgb = diffuse * diffusecolor + specular*specularcolor;
	col.a = i.color_atom1.a;	
//	}

	 	//LitSPhere / MatCap
	half2 vn;
    vn.x = normal.x;
    vn.y = normal.y;
   
    float4 matcapLookup = tex2D(_MatCap, vn*0.5 + 0.5);
    OUT.Color = col*matcapLookup*1.25;   
    return OUT;

}
ENDCG
    }

} 
// Pour les cartes graphiques ne supportant pas nos Shaders
Fallback "VertexLit"
}
