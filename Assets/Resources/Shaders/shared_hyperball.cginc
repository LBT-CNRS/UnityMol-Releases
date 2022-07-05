

float sum (float4 v) {
    return v.x + v.y + v.z + v.w;
}

float sum (float3 v) {
    return v.x + v.y + v.z;
}

struct Ray {
    float3 origin ;
    float3 direction ;
};

struct Quadric{
	float3 s1;
	float3 s2;
};

float4x4 mat_inverse(float4x4 A)
{
    float4x4 inv;
    float det = determinant(A);
    float invdet = 1 / det;

    inv[0][0] = invdet * (A[1][1] * A[2][2] * A[3][3] + A[1][2] * A[2][3] * A[3][1] + A[1][3] * A[2][1] * A[3][2] - 
                          A[1][1] * A[2][3] * A[3][2] - A[1][2] * A[2][1] * A[3][3] - A[1][3] * A[2][2] * A[3][1]); 

    inv[0][1] = invdet * (A[0][1] * A[2][3] * A[3][2] + A[0][2] * A[2][1] * A[3][3] + A[0][3] * A[2][2] * A[3][1] - 
                          A[0][1] * A[2][2] * A[3][3] - A[0][2] * A[2][3] * A[3][1] - A[0][3] * A[2][1] * A[3][2]);

    inv[0][2] = invdet * (A[0][1] * A[1][2] * A[3][3] + A[0][2] * A[1][3] * A[3][1] + A[0][3] * A[1][1] * A[3][2] - 
                          A[0][1] * A[1][3] * A[3][2] - A[0][2] * A[1][1] * A[3][3] - A[0][3] * A[1][2] * A[3][1]);

    inv[0][3] = invdet * (A[0][1] * A[1][3] * A[2][2] + A[0][2] * A[1][1] * A[2][3] + A[0][3] * A[1][2] * A[2][1] - 
                          A[0][1] * A[1][2] * A[2][3] - A[0][2] * A[1][3] * A[2][1] - A[0][3] * A[1][1] * A[2][2]);                 

    inv[1][0] = invdet * (A[1][0] * A[2][3] * A[3][2] + A[1][2] * A[2][0] * A[3][3] + A[1][3] * A[2][2] * A[3][0] - 
                          A[1][0] * A[2][2] * A[3][3] - A[1][2] * A[2][3] * A[3][0] - A[1][3] * A[2][0] * A[3][2]);

    inv[1][1] = invdet * (A[0][0] * A[2][2] * A[3][3] + A[0][2] * A[2][3] * A[3][0] + A[0][3] * A[2][0] * A[3][2] - 
                          A[0][0] * A[2][3] * A[3][2] - A[0][2] * A[2][0] * A[3][3] - A[0][3] * A[2][2] * A[3][0]);

    inv[1][2] = invdet * (A[0][0] * A[1][3] * A[3][2] + A[0][2] * A[1][0] * A[3][3] + A[0][3] * A[1][2] * A[3][0] - 
                          A[0][0] * A[1][2] * A[3][3] - A[0][2] * A[1][3] * A[3][0] - A[0][3] * A[1][0] * A[3][2]);

    inv[1][3] = invdet * (A[0][0] * A[1][2] * A[2][3] + A[0][2] * A[1][3] * A[2][0] + A[0][3] * A[1][0] * A[2][2] - 
                          A[0][0] * A[1][3] * A[2][2] - A[0][2] * A[1][0] * A[2][3] - A[0][3] * A[1][2] * A[2][0]);    

    inv[2][0] = invdet * (A[1][0] * A[2][1] * A[3][3] + A[1][1] * A[2][3] * A[3][0] + A[1][3] * A[2][0] * A[3][1] - 
                          A[1][0] * A[2][3] * A[3][1] - A[1][1] * A[2][0] * A[3][3] - A[1][3] * A[2][1] * A[3][0]);

    inv[2][1] = invdet * (A[0][0] * A[2][3] * A[3][1] + A[0][1] * A[2][0] * A[3][3] + A[0][3] * A[2][1] * A[3][0] - 
                          A[0][0] * A[2][1] * A[3][3] - A[0][1] * A[2][3] * A[3][0] - A[0][3] * A[2][0] * A[3][1]);

    inv[2][2] = invdet * (A[0][0] * A[1][1] * A[3][3] + A[0][1] * A[1][3] * A[3][0] + A[0][3] * A[1][0] * A[3][1] - 
                          A[0][0] * A[1][3] * A[3][1] - A[0][1] * A[1][0] * A[3][3] - A[0][3] * A[1][1] * A[3][0]);

    inv[2][3] = invdet * (A[0][0] * A[1][3] * A[2][1] + A[0][1] * A[1][0] * A[2][3] + A[0][3] * A[1][1] * A[2][0] - 
                          A[0][0] * A[1][1] * A[2][3] - A[0][1] * A[1][3] * A[2][0] - A[0][3] * A[1][0] * A[2][1]);

    inv[3][0] = invdet * (A[1][0] * A[2][2] * A[3][1] + A[1][1] * A[2][0] * A[3][2] + A[1][2] * A[2][1] * A[3][0] - 
                          A[1][0] * A[2][1] * A[3][2] - A[1][1] * A[2][2] * A[3][0] - A[1][2] * A[2][0] * A[3][1]); 

    inv[3][1] = invdet * (A[0][0] * A[2][1] * A[3][2] + A[0][1] * A[2][2] * A[3][0] + A[0][2] * A[2][0] * A[3][1] - 
                          A[0][0] * A[2][2] * A[3][1] - A[0][1] * A[2][0] * A[3][2] - A[0][2] * A[2][1] * A[3][0]); 

    inv[3][2] = invdet * (A[0][0] * A[1][2] * A[3][1] + A[0][1] * A[1][0] * A[3][2] + A[0][2] * A[1][1] * A[3][0] - 
                          A[0][0] * A[1][1] * A[3][2] - A[0][1] * A[1][2] * A[3][0] - A[0][2] * A[1][0] * A[3][1]); 

    inv[3][3] = invdet * (A[0][0] * A[1][1] * A[2][2] + A[0][1] * A[1][2] * A[2][0] + A[0][2] * A[1][0] * A[2][1] - 
                          A[0][0] * A[1][2] * A[2][1] - A[0][1] * A[1][0] * A[2][2] - A[0][2] * A[1][1] * A[2][0]);

    return inv ;
}

float distance_attenuation( float4 eye, float4 pos, float factor){
    float dist = distance(pos, eye);
    float attenuation = (20-factor)/(0.00001 + 0.0001  *  dist );
    return min(attenuation, 1);
}

bool cutoff_plane (float3 M, float3 cutoff, float3 x3) {
    float l = sum(x3  *  (M - cutoff));
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

    float delta = b * b - a * c;
    if (delta<0)
        clip(-1);

    float t1 = (-b - sqrt(delta)) / a;

    return r.origin.xyz + t1 * r.direction.xyz ;
}

Quadric isect_surf_ball(Ray r, float4x4 matrix_coef) {
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
  float t = t1;
	// float t2 = (-b + sqDelta) / a  ;	  
	// float t = (t1 < t2) ? t1 : t2;
	q.s1 = r.origin + t * r.direction ;
	q.s2 = q.s1;
	return q;
}


// Launches a primary ray in world-space through  * this *  fragment.
Ray primary_ray(float4 near1, float4 far1) {
    float3 near = near1.xyz / near1.w ;
    float3 far = far1.xyz / far1.w ;
    Ray ray;
    ray.origin = near;
    ray.direction = far - near;
    return ray;
}


float update_z_buffer(float4 M) {

    #if SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3
        float depth1 = 0.5 + (M.z / (2  *  M.w));
    #else
        float depth1 = M.z / M.w;
    #endif

    return depth1;
}


float4 calculate_focus(float4 pos1, float4 pos2, 
                       float r1, float r2,
                       float4 e3,
                       float shrinkfactor)
{
    float4 pos1sq = pos1  *  pos1;
    float4 pos2sq = pos2  *  pos2;
    float4 r1sq = r1  *  r1;
    float4 r2sq = r2  *  r2;
    float4 e3sq = e3  *  e3;

    float4 a = pos1sq - pos2sq;
    float  b = r2sq - r1sq;
    float4 c = a + b  *  e3sq / shrinkfactor;

    float4 d = pos1 - pos2;
    d+=0.000000000001;//don't divide by 0

    return c / (2.0  *  d);

}

