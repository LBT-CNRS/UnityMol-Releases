Shader "FvNano/Stick HyperBalls OpenGL" {

// Proprietes exposees a l'interface graphique
Properties {
	_Rayon1 ("Rayon de la Sphere1", float) = 0.1
	_Rayon2 ("Rayon de la Sphere2", float) = 0.1
    _Color ("Couleur du lien", Color) = 			(1.0,	0.1,	0.1,	1.0)
    _Color2 ("Couleur du lien2", Color) = 			(1.0,	0.1,	0.1,	1.0)
    _TexPos1 ("Position de la Sphere 1", Vector) = 	(0.0,	0.0,	0.0,	1.0)     // important que w != 0 !!
    _TexPos2 ("Position de la Sphere 2", Vector) = 	(3.0,	0.0,	0.0,	1.0)     // important que w != 0 !!
    _Shrink ("Shrink Factor", float) = 0.1
    _Visibilite ("Visibilite de l'hyperbole", float) = 1.0
    _Light ("Light vector", Vector) =				(1,		0,		0,		0)
    _Scale("Link Scale", float) = 1.0
    _EllipseFactor("Ellipse Factor", float) = 1.0
	_MatCap ("MatCap (RGB)", 2D) = "white" {}
	_Attenuation ("Attenuation", float) = 0
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
#pragma multi_compile_builtin_noshadows

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
float _Attenuation;
uniform sampler2D _MatCap;


//uniform float4x4 _matMVPI;
        
// vertex input: position
struct appdata {
    float4 vertex : POSITION;
};

float sum (float4 v) {
	return v.x + v.y + v.z + v.w;
}

float sum (float3 v) {
	return v.x + v.y + v.z;
}


// Variables passees du vertex au pixel shader
struct v2p {
    float4 p         		: POSITION;
    float4x4 matrix_near	: TEXCOORD2;
    float2x4 matrix_cutoff	: TEXCOORD6;
	float4 color_atom1		: TEXCOORD0;
	float shrink			: TEXCOORD1;
};

struct fragment_out {
  float4 Color : COLOR0;
  float depth  : DEPTH;
};

struct Ray {
   float3 origin ;
   float3 direction ;
};

//inverse function
float4x4 mat_inverse(float4x4 my_mat) {
	
	float4x4 inv;
	float invdet = 1/determinant(my_mat);
	
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


bool cutoff_plane (float3 M, float3 cutoff, float3 x3) {
	float l = sum(x3 * (M - cutoff));
	if (l<0.0)
		return true;
	else
		return false;
}


//ray-quadric intersection function
float3 isect_surf(Ray r, float4x4 matrix_coef) {
	float4 direction = float4(r.direction, 0.0);
	float4 origin = float4(r.origin, 1.0);
	float4 newDir = mul(matrix_coef, direction);
	
	float a = dot(direction, newDir) ;
	float b = dot(origin, newDir) ;
	float c = dot(origin,mul(matrix_coef,origin));
	
	float delta = b*b - a*c;
	if (delta<0)
		clip(-1);
	
	float t1 = (-b - sqrt(delta)) / a;

	return r.origin.xyz + t1*r.direction.xyz ;
}


// Launches a primary ray in world-space through *this* fragment.
Ray primary_ray(float4 near1, float4 far1) {
    float3 near = near1.xyz / near1.w ;
    float3 far = far1.xyz / far1.w ;
    Ray ray;
    ray.origin = near;
    ray.direction = far - near;
    //ray.direction = near - _WorldSpaceCameraPos;
    
    return ray;
}


// Updates the Z-buffer according to the world-space point M.
float update_z_buffer(float3 M, float4x4 ModelViewP) {
	float4 Ms = mul(ModelViewP,float4(M,1.0)) ;
	float depth1 = 0.5 + (Ms.z / (2 * Ms.w));
    return depth1;
}


// VERTEX SHADER IMPLEMENTATION =============================

v2p stickimproved_v (appdata v) {
	// OpenGL matrices
	float4x4 ModelViewProj = UNITY_MATRIX_MVP;			// matrice pour passer dans les coordonnees de l'ecran
	float4x4 ModelViewProjI = mat_inverse(ModelViewProj);

    v2p o; // Shader output

	float4 vertex_position;
	float4 spaceposition;
	
	//Calculate all the stuffs to create parallepipeds that defines the enveloppe for ray-casting
	
	float radius1 = _Rayon1 * _Scale;
	float radius2 = _Rayon2 * _Scale;

    float4 position_atom1 = float4(0,0,0,0);
    float4 position_atom2 = _TexPos2 - _TexPos1;

	o.color_atom1 = _Color;
	float4 color_atom2 = _Color2;
	float atom_distance = distance(position_atom1, position_atom2);

	spaceposition.z = v.vertex.z * atom_distance;
	
	if (radius1 > radius2)
		spaceposition.xy = v.vertex.xy * 2.0 * radius1;
	else
		spaceposition.xy = v.vertex.xy * 2.0 * radius2;
    spaceposition.w = 1;
	
	o.shrink = _Shrink;
	float shrinkfactor = o.shrink;
	
	float4 e3;
	e3.xyz = normalize(position_atom1 - position_atom2);
	if (e3.z == 0.0) { e3.z = 0.0000000000001;}
	if ( (position_atom1.x - position_atom2.x) == 0.0) { position_atom1.x += 0.001;}
    if ( (position_atom1.y - position_atom2.y) == 0.0) { position_atom1.y += 0.001;}
    if ( (position_atom1.z - position_atom2.z) == 0.0) { position_atom1.z += 0.001;}
	
	float4 focus;
	focus = (position_atom1*position_atom1 - position_atom2*position_atom2 + (radius2*radius2 - radius1*radius1) * e3*e3/shrinkfactor) / (2.0 * (position_atom1 - position_atom2));
			
	float3 e1;
	e1.x = 1.0;
	e1.y = 1.0;
	e1.z = ( sum(e3.xyz * focus.xyz) - e1.x*e3.x - e1.y*e3.y)/e3.z;
	float3 e1_temp = e1 - focus.xyz;
	e1 = normalize(e1_temp);
	float3 e2 = normalize(cross(e1,e3.xyz));
	
	float3 colonne1 = float3(e1.x, e2.x, e3.x);
	float3 colonne2 = float3(e1.y, e2.y, e3.y);
	float3 colonne3 = float3(e1.z, e2.z, e3.z);	
		
	float3x3 R = float3x3(colonne1, colonne2, colonne3);

 	// ROTATION:		
	vertex_position.xyz = mul((float3x3)R, (float3)spaceposition.xyz);
	vertex_position.w = 1.0;

	// TRANSLATION:
	vertex_position.xyz += (position_atom1.xyz + position_atom2.xyz)/2;

	o.p = mul(ModelViewProj, vertex_position);
	
	// Calculate origin and direction of ray that we pass to the fragment ----
	float4 i_near, i_far; 
	float4 near = o.p ;
	
	near.z = 0.0 ;
	near = mul(ModelViewProjI, near) ;
	float4 far = o.p ; 
	far.z = far.w ;
	i_far = mul(ModelViewProjI, far) ;
	i_near = near;

	e3.w = color_atom2.x;
	float4 prime1, prime2;
	prime1.xyz = position_atom1.xyz - (position_atom1.xyz - focus.xyz)*o.shrink;
	prime2.xyz = position_atom2.xyz - (position_atom2.xyz - focus.xyz)*o.shrink;
	prime1.w = color_atom2.y;
	prime2.w = color_atom2.z;
	o.matrix_cutoff = float2x4(prime1, prime2);		
	//o.matrix_cutoff = float2x4(position_atom1, position_atom2);		
	
	float4 atom2focus = position_atom1 - focus;
	float4 a2fsq = atom2focus * atom2focus;
	float Rcarre = (radius1*radius1 / shrinkfactor) - sum(a2fsq.xyz);
	focus.w = Rcarre;
	o.matrix_near = float4x4(i_near,i_far, focus, e3);	
	
    return o;
}



// PIXEL SHADER IMPLEMENTATION ===============================

fragment_out stickimproved_p (v2p i) {
	float3 light = _Light.xyz;
	float4x4 ModelViewProj = UNITY_MATRIX_MVP;			// matrice pour passer dans les coordonnees de l'ecran
	float4x4 ModelViewIT = UNITY_MATRIX_IT_MV; 

 	fragment_out OUT;
	
	float4 i_near = i.matrix_near._m00_m01_m02_m03;
	float4 i_far  = i.matrix_near._m10_m11_m12_m13;
	float4 focus = i.matrix_near._m20_m21_m22_m23;
	float3 e3 = i.matrix_near._m30_m31_m32;

	float3 color_atom2 = float3 (i.matrix_near._m33, i.matrix_cutoff._m03, i.matrix_cutoff._m13);

	float3 e1;
	float3 e3focus = e3 * focus.xyz;
	e1.x = 1.0;
 	e1.y = 1.0;
	e1.z = ( sum(e3focus) - e1.x*e3.x - e1.x*e3.y)/e3.z;
 	float3 e1_temp = e1 - focus.xyz;		
 	e1 = normalize(e1_temp);
 	float3 e2 = normalize(cross(e1,e3));
 	float3 cutoff1 = i.matrix_cutoff._m00_m01_m02;
	float3 cutoff2 = i.matrix_cutoff._m10_m11_m12;

 	float4 equation = focus;
 	float shrinkfactor = i.shrink;
	float t1 = -1/(1-shrinkfactor);
	float t2 = 1/shrinkfactor;
 	float4 colonne1, colonne2, colonne3, colonne4;
	float3 equation1 = float3(t2,	t2 * _EllipseFactor,	t1);
	
	float A1 = sum(-e1 * equation.xyz);
	float A2 = sum(-e2 * equation.xyz);
	float A3 = sum(-e3 * equation.xyz);
	
	float3 As = float3(A1, A2, A3);
	
	float3 eqex = equation1 * float3(e1.x, e2.x, e3.x);
	float3 eqey = equation1 * float3(e1.y, e2.y, e3.y);
	float3 eqez = equation1 * float3(e1.z, e2.z, e3.z);
	float3 eqAs = equation1 * As * As;
	float4 e1ext = float4(e1, As.x);
	float4 e2ext = float4(e2, As.y);
	float4 e3ext = float4(e3, As.z);

	float4	An1 = eqex.x * e1ext		+	eqex.y * e2ext			+	eqex.z * e3ext;							// Contains A11, A21, A31, A41
	float3	An2 = eqey.x * e1ext.yzw	+	eqey.y * e2ext.yzw		+	eqey.z * e3ext.yzw;						// Contains A22, A32, A42
	float2	An3 = eqez.x * e1ext.zw		+	eqez.y * e2ext.zw		+	eqez.z * e3ext.zw;						// Contains A33, A43
	float	A44 = eqAs.x				+	eqAs.y					+	eqAs.z				-	equation.w;		// Just A44

    colonne1 = An1;
    colonne2 = float4(An1.y, An2.xyz);
    colonne3 = float4(An1.z, An2.y, An3.xy);
    colonne4 = float4(An1.w, An2.z, An3.y, A44);
    float4x4 mat = float4x4(colonne1,colonne2,colonne3,colonne4);
    
	Ray ray = primary_ray(i_near,i_far);
    float3 M = isect_surf(ray, mat);
  	OUT.depth = update_z_buffer(M, ModelViewProj);

	if (shrinkfactor < 0.0)
		discard;
		
  	if (cutoff_plane(M, cutoff1, -e3) || cutoff_plane(M, cutoff2, e3))
  		discard;

	//------------ blinn phong light try ------------------------
	//Transform normal to model space to view-space
    float4 M1 = float4(M,1.0);
	float3 normal = mul(ModelViewIT, mul(mat,M1)).xyz;
	normal = normalize(normal);
	float3 lightvec = float3(0, 0, 1);
	float3 eyepos = float3(0, 0, 1);
	//float3 halfvec = normalize(lightvec + eyepos);
	float diffuse = dot(normal,lightvec);
	//float specular = dot(normal, halfvec);
	//float shininess = 200.0 ;
	
	//float specular = pow(max(dot(normal, halfvec),0.0),shininess);
	
    //float4 lighting = lit(diffuse, specular, 32);
   	float a = sum((M.xyz - cutoff2) * e3) / distance(cutoff2, cutoff1);
	float3 atom_color = lerp(color_atom2, i.color_atom1.xyz, a );  //ca2 is a float3
   
    float3 diffusecolor = atom_color; 
	//float3 specularcolor = float3(1.0,1.0,1.0);    
	
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

	// MatCap
	half2 vn = normal.xy;
   
    float4 matcapLookup = tex2D(_MatCap, vn*0.5 + 0.5);
    OUT.Color = col * matcapLookup * 1.25;
    
    if(_Attenuation == 1) {
    	float attenuation = distance_attenuation(1, i_near, i_far);
		OUT.Color = OUT.Color * attenuation;
    }
    
    return OUT;
}

ENDCG
    }

} 
// Pour les cartes graphiques ne supportant pas nos Shaders
//Fallback "VertexLit" // automatically enables shadow casting, which probably eats resources and does't work anyway
}


