#include "UnityCG.cginc"
#include "UCLA GameLab Wireframe Functions.cginc"

// DATA STRUCTURES //
// Vertex to Geometry
struct UCLAGL_v2g
{
float4  pos     : POSITION;     // vertex position
float2  uv      : TEXCOORD0;    // vertex uv coordinate
float3  objpos  : TEXCOORD1;    // object position
half4   col     : COLOR0;
float3 normal   : TEXCOORD2;
float3 worldPos : TEXCOORD3;
};

// Geometry to  UCLAGL_fragment
struct UCLAGL_g2f
{
float4  pos     : POSITION;     // fragment position
float2  uv      : TEXCOORD0;    // fragment uv coordinate
float3  dist    : TEXCOORD1;    // distance to each edge of the triangle
float3  objpos  : TEXCOORD2;    // object position
half4   col     : COLOR0;
float3 normal   : TEXCOORD3;
float3 worldPos : TEXCOORD4;
};

// PARAMETERS //

float _Thickness = 1;       // Thickness of the wireframe line rendering
float _Firmness = 1;        // Thickness of the wireframe line rendering
float4 _Color = {1, 1, 1, 1}; // Color of the line
float4 _MainTex_ST;         // For the Main Tex UV transform
sampler2D _MainTex;         // Texture used for the line

float _LimitedView;
float _LimitedViewRadius;
float3 _LimitedViewCenter;

float _FogStart;
float _FogDensity;
float _UseFog;

uniform float4 _LightColor0;
uniform float4 _SpecColor;
uniform float _Shininess;

// SHADER PROGRAMS //
// Vertex Shader
UCLAGL_v2g UCLAGL_vert(appdata_full v)
{
    UCLAGL_v2g output;
    output.pos = UnityObjectToClipPos(v.vertex);
    output.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
    output.col = half4(v.color.rgb, 1);
    output.objpos = v.vertex;
    output.normal = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
    output.worldPos = mul(unity_ObjectToWorld, v.vertex);

    return output;
}

// Geometry Shader
[maxvertexcount(3)]
void UCLAGL_geom(triangle UCLAGL_v2g p[3], inout TriangleStream<UCLAGL_g2f> triStream)
{
    float3 dist = UCLAGL_CalculateDistToCenter(p[0].pos, p[1].pos, p[2].pos);

    UCLAGL_g2f pIn;

    // add the first point
    pIn.pos = p[0].pos;
    pIn.uv = p[0].uv;
    pIn.dist = float3(dist.x, 0, 0);
    pIn.col = p[0].col;
    pIn.objpos = p[0].objpos;
    pIn.normal = p[0].normal;
    pIn.worldPos = p[0].worldPos;
    triStream.Append(pIn);

    // add the second point
    pIn.pos =  p[1].pos;
    pIn.uv = p[1].uv;
    pIn.dist = float3(0, dist.y, 0);
    pIn.col = p[1].col;
    pIn.objpos = p[1].objpos;
    pIn.normal = p[1].normal;
    pIn.worldPos = p[1].worldPos;
    triStream.Append(pIn);

    // add the third point
    pIn.pos = p[2].pos;
    pIn.uv = p[2].uv;
    pIn.dist = float3(0, 0, dist.z);
    pIn.col = p[2].col;
    pIn.objpos = p[2].objpos;
    pIn.normal = p[2].normal;
    pIn.worldPos = p[2].worldPos;
    triStream.Append(pIn);
}

// Fragment Shader
float4 UCLAGL_frag(UCLAGL_g2f input) : COLOR
{
    float w = input.pos.w;
#if UCLAGL_DISTANCE_AGNOSTIC
    w = 1;
#endif

    float alpha = UCLAGL_GetWireframeAlpha(input.dist, _Thickness, _Firmness, w);
    float4 col = _Color * tex2D(_MainTex, input.uv) * input.col;
    col.a *= alpha;

    if (_LimitedView) {
        float d = distance(input.objpos, _LimitedViewCenter);
        if (d > _LimitedViewRadius)
            discard;
    }

#if UCLAGL_CUTOUT
    if (col.a < 0.5f) discard;
    col.a = 1.0f;
#endif


    // return col;

    float3 normalDirection = normalize(input.normal);
    float3 viewDirection = normalize(_WorldSpaceCameraPos - input.worldPos.xyz);

    float3 vert2LightSource = _WorldSpaceLightPos0.xyz - input.worldPos.xyz;
    float oneOverDistance = 1.0f / length(vert2LightSource);
    float attenuation = lerp(1.0f, oneOverDistance, _WorldSpaceLightPos0.w); //Optimization for spot lights. This isn't needed if you're just getting started.
    float3 lightDirection = _WorldSpaceLightPos0.xyz - input.worldPos.xyz * _WorldSpaceLightPos0.w;

    float3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb; //Ambient component
    float3 diffuseReflection = attenuation * _LightColor0.rgb * _Color.rgb * max(0.0f, dot(normalDirection, lightDirection)); //Diffuse component

    float3 color = (ambientLighting + diffuseReflection) * input.col;

    if (_UseFog) {
        // float fogFactor = smoothstep(_FogEnd, _FogStart, mul(UNITY_MATRIX_M, M1).z);
        float fogFactor = exp(_FogStart - input.worldPos.z  / max(0.0001, _FogDensity));
        color.rgb = lerp(unity_FogColor, color.rgb, saturate(fogFactor));
    }

    return float4(color, col.a);
}