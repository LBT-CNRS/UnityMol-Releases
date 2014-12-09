Shader "FvNano/Ball HyperBalls OpenGL" {


// Properties exposed to the interface
Properties {
	_Rayon ("Rayon de la Sphere", float) = 0.1
	_Color ("Couleur de la Sphere", Color) = (1,0.1,0.1,1.0)
	_TexPos ("Position de la sphere", Vector) = (0.0,0.0,0.0,1.0)     // important que w != 0 !!
	_Visibilite ("Visibilite de la Sphere", float) = 1.0
	_Light ("Light vector", Vector) = (1,0,0,0)
	_Equation("Equation", Vector) = (1,1,1,1)
	_MatCap ("MatCap (RGB)", 2D) = "white" {}
	_Attenuation ("Attenuation", float) = 0
	_Brightness ("Brightness", float) = 1.0
}


// ==========================================================
// L'actuel Shader Cg =======================================
// ==========================================================

SubShader {
	ZWrite On // doesn't seem to help
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
#pragma multi_compile_builtin_noshadows
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
int _Attenuation;
float _Brightness;

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

float sum (float4 v) {
	return v.x + v.y + v.z + v.w;
}

float sum (float3 v) {
	return v.x + v.y + v.z;
}

//inverse function
float4x4 mat_inverse(float4x4 my_mat) {
	
	float4x4 inv;	
	float det = determinant(my_mat);
	float invdet = 1/det;
	
	inv[0][0] = invdet*( my_mat[1][1]*my_mat[2][2]*my_mat[3][3] + my_mat[1][2]*my_mat[2][3]*my_mat[3][1] + my_mat[1][3]*my_mat[2][1]*my_mat[3][2] - 
						  my_mat[1][1]*my_mat[2][3]*my_mat[3][2] - my_mat[1][2]*my_mat[2][1]*my_mat[3][3] - my_mat[1][3]*my_mat[2][2]*my_mat[3][1] ) ; 

	inv[0][1] = invdet*( my_mat[0][1]*my_mat[2][3]*my_mat[3][2] + my_mat[0][2]*my_mat[2][1]*my_mat[3][3] + my_mat[0][3]*my_mat[2][2]*my_mat[3][1] - 
						  my_mat[0][1]*my_mat[2][2]*my_mat[3][3] - my_mat[0][2]*my_mat[2][3]*my_mat[3][1] - my_mat[0][3]*my_mat[2][1]*my_mat[3][2] ) ; 

	inv[0][2] = invdet*( my_mat[0][1]*my_mat[1][2]*my_mat[3][3] + my_mat[0][2]*my_mat[1][3]*my_mat[3][1] + my_mat[0][3]*my_mat[1][1]*my_mat[3][2] - 
						  my_mat[0][1]*my_mat[1][3]*my_mat[3][2] - my_mat[0][2]*my_mat[1][1]*my_mat[3][3] - my_mat[0][3]*my_mat[1][2]*my_mat[3][1] ) ; 
						  
	inv[0][3] = invdet*( my_mat[0][1]*my_mat[1][3]*my_mat[2][2] + my_mat[0][2]*my_mat[1][1]*my_mat[2][3] + my_mat[0][3]*my_mat[1][2]*my_mat[2][1] - 
						  my_mat[0][1]*my_mat[1][2]*my_mat[2][3] - my_mat[0][2]*my_mat[1][3]*my_mat[2][1] - my_mat[0][3]*my_mat[1][1]*my_mat[2][2] ) ; 						 

	inv[1][0] = invdet*( my_mat[1][0]*my_mat[2][3]*my_mat[3][2] + my_mat[1][2]*my_mat[2][0]*my_mat[3][3] + my_mat[1][3]*my_mat[2][2]*my_mat[3][0] - 
						  my_mat[1][0]*my_mat[2][2]*my_mat[3][3] - my_mat[1][2]*my_mat[2][3]*my_mat[3][0] - my_mat[1][3]*my_mat[2][0]*my_mat[3][2] ) ; 

	inv[1][1] = invdet*( my_mat[0][0]*my_mat[2][2]*my_mat[3][3] + my_mat[0][2]*my_mat[2][3]*my_mat[3][0] + my_mat[0][3]*my_mat[2][0]*my_mat[3][2] - 
						  my_mat[0][0]*my_mat[2][3]*my_mat[3][2] - my_mat[0][2]*my_mat[2][0]*my_mat[3][3] - my_mat[0][3]*my_mat[2][2]*my_mat[3][0] ) ; 

	inv[1][2] = invdet*( my_mat[0][0]*my_mat[1][3]*my_mat[3][2] + my_mat[0][2]*my_mat[1][0]*my_mat[3][3] + my_mat[0][3]*my_mat[1][2]*my_mat[3][0] - 
						  my_mat[0][0]*my_mat[1][2]*my_mat[3][3] - my_mat[0][2]*my_mat[1][3]*my_mat[3][0] - my_mat[0][3]*my_mat[1][0]*my_mat[3][2] ) ; 
						  
	inv[1][3] = invdet*( my_mat[0][0]*my_mat[1][2]*my_mat[2][3] + my_mat[0][2]*my_mat[1][3]*my_mat[2][0] + my_mat[0][3]*my_mat[1][0]*my_mat[2][2] - 
						  my_mat[0][0]*my_mat[1][3]*my_mat[2][2] - my_mat[0][2]*my_mat[1][0]*my_mat[2][3] - my_mat[0][3]*my_mat[1][2]*my_mat[2][0] ) ; 	
						  
	inv[2][0] = invdet*( my_mat[1][0]*my_mat[2][1]*my_mat[3][3] + my_mat[1][1]*my_mat[2][3]*my_mat[3][0] + my_mat[1][3]*my_mat[2][0]*my_mat[3][1] - 
						  my_mat[1][0]*my_mat[2][3]*my_mat[3][1] - my_mat[1][1]*my_mat[2][0]*my_mat[3][3] - my_mat[1][3]*my_mat[2][1]*my_mat[3][0] ) ; 

	inv[2][1] = invdet*( my_mat[0][0]*my_mat[2][3]*my_mat[3][1] + my_mat[0][1]*my_mat[2][0]*my_mat[3][3] + my_mat[0][3]*my_mat[2][1]*my_mat[3][0] - 
						  my_mat[0][0]*my_mat[2][1]*my_mat[3][3] - my_mat[0][1]*my_mat[2][3]*my_mat[3][0] - my_mat[0][3]*my_mat[2][0]*my_mat[3][1] ) ; 

	inv[2][2] = invdet*( my_mat[0][0]*my_mat[1][1]*my_mat[3][3] + my_mat[0][1]*my_mat[1][3]*my_mat[3][0] + my_mat[0][3]*my_mat[1][0]*my_mat[3][1] - 
						  my_mat[0][0]*my_mat[1][3]*my_mat[3][1] - my_mat[0][1]*my_mat[1][0]*my_mat[3][3] - my_mat[0][3]*my_mat[1][1]*my_mat[3][0] ) ; 
						  
	inv[2][3] = invdet*( my_mat[0][0]*my_mat[1][3]*my_mat[2][1] + my_mat[0][1]*my_mat[1][0]*my_mat[2][3] + my_mat[0][3]*my_mat[1][1]*my_mat[2][0] - 
						  my_mat[0][0]*my_mat[1][1]*my_mat[2][3] - my_mat[0][1]*my_mat[1][3]*my_mat[2][0] - my_mat[0][3]*my_mat[1][0]*my_mat[2][1] ) ;						  
	
	inv[3][0] = invdet*( my_mat[1][0]*my_mat[2][2]*my_mat[3][1] + my_mat[1][1]*my_mat[2][0]*my_mat[3][2] + my_mat[1][2]*my_mat[2][1]*my_mat[3][0] - 
						  my_mat[1][0]*my_mat[2][1]*my_mat[3][2] - my_mat[1][1]*my_mat[2][2]*my_mat[3][0] - my_mat[1][2]*my_mat[2][0]*my_mat[3][1] ) ; 

	inv[3][1] = invdet*( my_mat[0][0]*my_mat[2][1]*my_mat[3][2] + my_mat[0][1]*my_mat[2][2]*my_mat[3][0] + my_mat[0][2]*my_mat[2][0]*my_mat[3][1] - 
						  my_mat[0][0]*my_mat[2][2]*my_mat[3][1] - my_mat[0][1]*my_mat[2][0]*my_mat[3][2] - my_mat[0][2]*my_mat[2][1]*my_mat[3][0] ) ; 

	inv[3][2] = invdet*( my_mat[0][0]*my_mat[1][2]*my_mat[3][1] + my_mat[0][1]*my_mat[1][0]*my_mat[3][2] + my_mat[0][2]*my_mat[1][1]*my_mat[3][0] - 
						  my_mat[0][0]*my_mat[1][1]*my_mat[3][2] - my_mat[0][1]*my_mat[1][2]*my_mat[3][0] - my_mat[0][2]*my_mat[1][0]*my_mat[3][1] ) ; 
						  
	inv[3][3] = invdet*( my_mat[0][0]*my_mat[1][1]*my_mat[2][2] + my_mat[0][1]*my_mat[1][2]*my_mat[2][0] + my_mat[0][2]*my_mat[1][0]*my_mat[2][1] - 
						  my_mat[0][0]*my_mat[1][2]*my_mat[2][1] - my_mat[0][1]*my_mat[1][0]*my_mat[2][2] - my_mat[0][2]*my_mat[1][1]*my_mat[2][0] ) ;

	return inv ;
}

float distance_attenuation(int attenuate, float4 eye, float4 pos){
	float dist = distance(pos, eye);
	float attenuation = 1/(0.00001 + 0.0001 * dist);
	return min(attenuation, 1);
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
	
	float4 mcoef_dir = mul(matrix_coef, direction);
	
	float a = dot(direction,	mcoef_dir) ;
	float b = dot(origin,	mcoef_dir) ;
	float c = dot(origin,	mul(matrix_coef,origin));
	
	float delta = b*b - a*c;
	if (delta < 0)
		clip(-1);
	float sqDelta = sqrt(delta);
	float t1 = (-b - sqDelta) / a  ;
	float t2 = (-b + sqDelta) / a  ;	  
	float t = (t1 < t2) ? t1 : t2;
	q.s1 = r.origin + t * r.direction ;
	q.s2 = q.s1;
	return q;
}

//Intersection of a ray r through o and a plane p
float3 isect_plane(Ray r, float3 o, float4 p){
	float3 d = r.direction;
	float lambda = -(sum(o*p.xyz) + p.w) / sum(d);
	float3 i = o + d*lambda;
	
	return i;
}

// Launches a primary ray in world-space through *this* fragment.
Ray primary_ray(float4 near1, float4 far1) {
	float3 near = near1.xyz / near1.w ;
	float3 far = far1.xyz / far1.w ;
	Ray ray;
	ray.origin = near;
	ray.direction = far - near;
	return ray;
}


// Updates the Z-buffer according to the world-space point M.
float update_z_buffer(float3 M, float4x4 ModelViewP) {
	float4 Ms = mul(ModelViewP,float4(M,1.0)) ;
	float depth1 = 0.5 + (Ms.z / (2 * Ms.w));
	return depth1;
}


// VERTEX SHADER IMPLEMENTATION =============================

v2p ballimproved_v (appdata v) {
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
		spaceposition.xyz += v.vertex.xyz * (2.0 * _Rayon);
	
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
	
	float4 equation1 = float4(_Equation.xyz, _Rayon);
	float4 eq1TexPos = equation1 * _TexPos;
	float4 eq1TexSq = eq1TexPos * _TexPos;

	o.colonne1 = float4(equation1.x,	0.0,			0.0,			-eq1TexPos.x);
	o.colonne2 = float4(0.0,			equation1.y,	0.0,			-eq1TexPos.y);
	o.colonne3 = float4(0.0,			0.0,			equation1.z,	-eq1TexPos.z);
	o.colonne4 = float4(-eq1TexPos.x,	-eq1TexPos.y,	-eq1TexPos.z,	-equation1.w*equation1.w + eq1TexSq.x + eq1TexSq.y + eq1TexSq.z);
	return o;    
}



// PIXEL SHADER IMPLEMENTATION ===============================

fragment_out ballimproved_p (v2p i) {

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
    //OUT.depth = -3;
	
	//Transform normal to model space to view-space
	float4 M1 = float4(M,1.0);
	float4 M2 = mul(mat,M1);

	float3 normal = normalize(mul(ModelViewIT,M2).xyz);

	float3 lightvec = float3(0, 0, 1);
	float3 eyepos = float3(0, 0, 1);
	//float3 halfvec = normalize(lightvec + eyepos);
	
	float diffuse = dot(normal,lightvec);
	
	
	//float shininess = 200.0;
	//float specular = pow(max(dot(normal, halfvec),0.0),shininess);
    //float4 lighting = lit(diffuse, specular, 32);

	float3 diffusecolor;	

	diffusecolor = _Color.xyz; 
	
	//float3 specularcolor = float3(1.0,1.0,1.0);

	//LitSPhere / MatCap
	half2 vn = normal.xy;
   
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
	OUT.Color = OUT.Color * matcapLookup * 1.25 * _Brightness;
	
	
	if(_Attenuation == 1) {
		float attenuation = distance_attenuation(1, i.i_near, i.i_far);
		OUT.Color = OUT.Color * attenuation;
	}
	
	
	
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
	//OUT.depth = pow(OUT.depth,1000);
	//OUT.depth = OUT.depth;
	//OUT.Color = float4(pow(OUT.depth,1000));
  return OUT;

}
ENDCG
	}

} 
// Pour les cartes graphiques ne supportant pas nos Shaders
//Fallback "VertexLit" // automatically enables shadow casting, which probably eats resources and does't work anyway
}
