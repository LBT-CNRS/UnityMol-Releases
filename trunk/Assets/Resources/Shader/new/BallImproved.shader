/// @file BallImproved.shader
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
/// $Id: BallImproved.shader 208 2013-04-06 20:43:58Z baaden $
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

// $Id: BallImproved.shader 208 2013-04-06 20:43:58Z baaden $
// (c) 2010 by Marc Baaden & Matthieu Chavent, <baaden@smplinux.de> <matthieu.chavent@free.fr>
// Unity3D FvNano shaders coming from FVNano project
//
// On a un cube (mesh) dans la scene -> vertex
// la spherepos du cube est 0, 0, 0


Shader "FvNano/Ball HyperBalls OpenGL" {


// Proprietes exposees a l'interface graphique
Properties {
	_Rayon ("Rayon de la Sphere", float) = 0.1
	_Color ("Couleur de la Sphere", Color) = (1,0.1,0.1,1.0)
	_TexPos ("Position de la sphere", Vector) = (0.0,0.0,0.0,1.0)     // important que w != 0 !!
	_Visibilite ("Visibilite de la Sphere", float) = 1.0
	_Light ("Light vector", Vector) = (1,0,0,0)
	_Equation("Equation", Vector) = (1,1,1,1)
	_MatCap ("MatCap (RGB)", 2D) = "white" {}
}


// ==========================================================
// L'actuel Shader Cg =======================================
// ==========================================================

SubShader {
	Pass {
		//Name "BASE"
        //Tags { "LightMode" = "Always" }		

CGPROGRAM

// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct appdata members vertex)
#pragma exclude_renderers xbox360

// Setup
#pragma target 3.0
#pragma vertex ballimproved_v
#pragma fragment ballimproved_p
#pragma multi_compile_builtin
#include "UnityCG.cginc"

// Variables modifiables dans Unity3D
float _Rayon;
float4 _Color;
float4 _TexPos;
float _Visibilite;
float4 _Light;
float4 _Equation;
uniform sampler2D _MatCap;
float _Cut;
float4 _Cutplane;

//  uniform float4x4 _matMVPI;
		
// vertex input: position
struct appdata {
	float4 vertex : POSITION;
};


// Variables passees du vertex au pixel shader
struct v2p {
	float4 p         		: POSITION;
	float4 i_near	   		: TEXCOORD1;
	float4 i_far	   		: TEXCOORD2;
	float4 colonne1			: TEXCOORD6;
	float4 colonne2			: TEXCOORD7;
	float4 colonne3			: COLOR0;
	float4 colonne4			: TEXCOORD3;
	float4 worldpos			: TEXCOORD4;
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

struct Quadric{
	float3 s1;
	float3 s2;
};

//ray-quadric intersection function
Quadric isect_surf(Ray r, float4x4 matrix_coef) {

   Quadric q;
   float4 direction = float4(r.direction, 0.0);
   float4 origin = float4(r.origin, 1.0);

   float a = dot(direction,mul(matrix_coef,direction)) ;
   float b = dot(origin,mul(matrix_coef,direction)) ;
   float c = dot(origin,mul(matrix_coef,origin));

   float delta = b*b -  a * c ;
   if (delta<0) clip(-1);
   
   float t1 = (-b-sqrt(delta)) / a  ;
   float t2 = (-b+sqrt(delta)) / a  ;	  
   float t =(t1<t2) ? t1 : t2;
   q.s1 = r.origin + t * r.direction ;
   q.s2 = r.origin + t * r.direction ;
   return q;
}

//Intersection of a ray r through o and a plane p
float3 isect_plane(Ray r, float3 o, float4 p){
	float3 d = r.direction;
	float lambda = -(o.x*p.x + o.y*p.y + o.z*p.z + p.w)/(d.x+d.y+d.z);
	float3 i = o + d*lambda;
	
	return i;
}


// Launches a primary ray in world-space through *this* fragment.
Ray primary_ray(float4 near1, float4 far1) {
	float3 near = near1.xyz / near1.w ;
	float3 far = far1.xyz / far1.w ;

	return Ray(near, far - near) ;
}


// Updates the Z-buffer according to the world-space point M.
float update_z_buffer(float3 M, float4x4 ModelViewP) {
	float4 Ms = mul(ModelViewP,float4(M,1.0)) ;
	return (1.0 + Ms.z / Ms.w) / 2.0 ;
//    return float((1.0 + Ms.z / Ms.w) / 2.0) ;
}


// VERTEX SHADER IMPLEMENTATION =============================

v2p ballimproved_v (appdata v)

{
	// OpenGL matrices
	float4x4 ModelViewProj = UNITY_MATRIX_MVP;			// matrice pour passer dans les coordonnees de l'ecran
	float4x4 ModelViewProjI = mat_inverse(ModelViewProj);

	v2p o; // Shader output

	float4 spaceposition;
	spaceposition.xyz = _TexPos.xyz;	
	spaceposition.w =1.0;

	if(_Rayon < 1.0)
		spaceposition.xyz += v.vertex.xyz * 2.0; // * _Rayon);
	else
		spaceposition.xyz += v.vertex.xyz * (2.0*_Rayon);
	
	o.p = mul(ModelViewProj, spaceposition);
	v.vertex = o.p;
	
	o.worldpos = o.p;
	

	float4 near = o.p ; 
	near.z = 0.0 ;
	near = mul(ModelViewProjI, near) ;

	float4 far = o.p ; 
	far.z = far.w ;
	o.i_far = mul(ModelViewProjI,far) ;
	o.i_near = near;
	
	float4 equation1 = float4(_Equation.x, _Equation.y, _Equation.z, _Rayon);

	o.colonne1 = float4(equation1.x,0.0,0.0,-equation1.x*_TexPos.x);
	o.colonne2 = float4(0.0,equation1.y,0.0,-equation1.y*_TexPos.y);
	o.colonne3 = float4(0.0,0.0,equation1.z,-equation1.z*_TexPos.z);
	o.colonne4 = float4(-equation1.x*_TexPos.x,-equation1.y*_TexPos.y,-equation1.z*_TexPos.z, 
						-equation1.w*equation1.w + equation1.x*_TexPos.x*_TexPos.x + equation1.y*_TexPos.y*_TexPos.y +equation1.z*_TexPos.z*_TexPos.z);
	return o;    
}



// PIXEL SHADER IMPLEMENTATION ===============================

fragment_out ballimproved_p (v2p i)
{

	float3 light = _Light.xyz;
	float4x4 ModelViewProj = UNITY_MATRIX_MVP;			// matrice pour passer dans les coordonnees de l'ecran
	float4x4 ModelViewIT = UNITY_MATRIX_IT_MV; 
	float4 pcolor2;                                      // pixel color 2

	fragment_out OUT;

	//create matrix for the quadric equation of the sphere 
	float4x4 mat = float4x4(i.colonne1,i.colonne2,i.colonne3,i.colonne4); 	
 
	Ray ray = primary_ray(i.i_near,i.i_far) ;
	//float4 cut = float4(1,0,0,0.2);
    Quadric q = isect_surf(ray, mat);
    float3 M = q.s1;
    OUT.depth = update_z_buffer(M, ModelViewProj);
	
	//Transform normal to model space to view-space
	float4 M1 = float4(M,1.0);
	float4 M2 = mul(mat,M1);

	float3 normal = normalize(mul(ModelViewIT,M2).xyz);

	float3 lightvec = normalize(float3(0.0,0.0,1.2));
	
	float3 eyepos = float3(0.0,0.0,1.0);
	float3 halfvec = normalize(lightvec + eyepos);
	
	float diffuse = dot(normal,lightvec);
	
	
	float shininess = 200.0;
	
	float specular = pow(max(dot(normal, halfvec),0.0),shininess);
   
    float4 lighting = lit(diffuse, specular, 32);

	float3 diffusecolor;	

	diffusecolor = _Color.xyz; 
	
	float3 specularcolor = float3(1.0,1.0,1.0);

	//LitSPhere / MatCap
	half2 vn;
    vn.x = normal.x;
    vn.y = normal.y;
   
    float4 matcapLookup = tex2D(_MatCap, vn*0.5 + 0.5);    

//	if(dot(normal,eyepos) < 0.5)
//		OUT.Color.rgb = float3(0,0,0);
//	else{
	
//MB - activate/deactivate specular color manually according to your
//preference
	 OUT.Color.rgb = diffuse * diffusecolor;// + specular*specularcolor;
	 //OUT.Color.rgb = diffuse * diffusecolor + specular*specularcolor;
	 OUT.Color.a = _Color.a;	
	 //OUT.Color.a = 0.2;
     OUT.Color = OUT.Color*matcapLookup*1.25;
	
	
	// if(_Cut == 1f)
	// {
	// 	float4 pos = mul(_Object2World,M1);
	// 	//float4 wpos = mul(_Object2World,_TexPos);
	// 	float test = _Cutplane.w + _Cutplane.x*(pos.x) + _Cutplane.y*(pos.y) + _Cutplane.z*(pos.z);
	// 	// if(test > 0)
	// 	// {
	// 	// 	OUT.depth = update_z_buffer(q.s2, ModelViewProj);
	// 	// 	OUT.Color.rgb = diffuse * diffusecolor;
	// 	// 	return OUT;
	// 	// }
	// 	if (test<0){
	// 		clip(-1);
	// 	}
	// }


//	float 
//	float test = cut.w + cut.x*(pos.x) + cut.y*(pos.y) + cut.z*(pos.z);
//    if (test<0){
//    	float wTest = cut.w + cut.x*(wpos.x) + cut.y*(wpos.y) + cut.z*(wpos.z);
//		float lambda = -wTest/(cut.x*cut.x + cut.y*cut.y + cut.z*cut.z);
//		float4 H = float4(lambda*cut.x + wpos.x,
//						  lambda*cut.y + wpos.y,
//						  lambda*cut.z + wpos.z,
//						  0.0);
//		//float d = abs(test)/sqrt(cut.x*cut.x + cut.y*cut.y + cut.z*cut.z);
//		float d = distance(wpos,H);
//		float rayon = sqrt(_Rayon*_Rayon - d*d);
////		
//		float4 H2 = float4(isect_plane(ray, pos, cut),0.0);
//		float dist = distance(H,H2);
//		if(dist < rayon)
//		{    
//			OUT.Color = _Color*0.7;
//			//OUT.depth = update_z_buffer(H2, ModelViewProj);
//			return OUT;
//		}
//		float dist = test/sqrt(cut.x*cut.x + cut.y*cut.y + cut.z*cut.z);
//		float3 isect_cutplane = M + (normalize(ray.direction) * dist);
		//M = ray.origin + test*ray.direction;
		//OUT.depth = update_z_buffer(M, ModelViewProj);
//		return OUT;
//		clip(-1);
//	}
	

//	}
  return OUT;

}
ENDCG
	}

} 
// Pour les cartes graphiques ne supportant pas nos Shaders
Fallback "VertexLit"
}
