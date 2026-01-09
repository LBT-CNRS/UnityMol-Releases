// Algorithms and shaders based on code from this journal
// http://cgg-journal.com/2008-2/06/index.html
// http://web.archive.org/web/20130322011415/http://cgg-journal.com/2008-2/06/index.html

#ifndef UCLA_GAMELAB_WIREFRAME
#define UCLA_GAMELAB_WIREFRAME

#include "UnityCG.cginc"

// For use in the Geometry Shader
// Takes in 3 vectors and calculates the distance to
// to center of the triangle for each vert
float3 UCLAGL_CalculateDistToCenter(float4 v0, float4 v1, float4 v2) {
    // points in screen space
    float2 ss0 = _ScreenParams.xy * v0.xy / v0.w;
    float2 ss1 = _ScreenParams.xy * v1.xy / v1.w;
    float2 ss2 = _ScreenParams.xy * v2.xy / v2.w;
    
    // edge vectors
    float2 e0 = ss2 - ss1;
    float2 e1 = ss2 - ss0;
    float2 e2 = ss1 - ss0;
    
    // area of the triangle
    float area = abs(e1.x * e2.y - e1.y * e2.x);
    
    // values based on distance to the center of the triangle
    float dist0 = area / length(e0);
    float dist1 = area / length(e1);
    float dist2 = area / length(e2);

    return float3(dist0, dist1, dist2);
}

// Computes the intensity of the wireframe at a point
// based on interpolated distances from center for the
// fragment, thickness, firmness, and perspective correction
// factor.
// w = 1 gives screen-space consistent wireframe thickness
float UCLAGL_GetWireframeAlpha(float3 dist, float thickness, float firmness, float w = 1) {
    // find the smallest distance
    float val = min(dist.x, min(dist.y, dist.z));
    val *= w;

    // calculate power to 2 to thin the line
    val = exp2(-1 / thickness * val * val);
    val = min(val * firmness, 1);
    return val;
}
#endif