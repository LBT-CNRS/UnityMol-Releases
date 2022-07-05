Shader "UMol/AOComputeHB" {


// Properties exposed to the interface
    Properties {
        // _Radius ("Radius", float) = 1
        _ScreenSize ("Screen resolution", Vector) = (512, 512, 0, 0)
        _AOTex ("AO Texture to blend", 2D) = "white"{}
        _Sampling ("Sampling steps", float) = 128
        _AtomPos ("Texture of atom positions", 2D) = "white" {}
        _NBAtoms ("Number of atoms", int) = 1
        _AORes ("AO resolution for each patch", int) = 1
        _IDSample ("Sampling iteration", int) = 0
        _NBParam ("Texture size in x", Float) = 1.0
        _AtomsByCol ("Atoms by column in pos texture", int) = 2000

    }

    SubShader {
        Tags { "RenderType" = "Opaque" }
        ZWrite Off
        Pass {
            Cull Off
            Fog { Mode off }

            CGPROGRAM

#pragma vertex ballimproved_v
#pragma fragment ballimproved_p
#include "UnityCG.cginc"

            sampler2D _AOTex;
            uniform sampler2D _AtomPos;
            float4 _AtomPos_TexelSize;
            uniform float _Sampling;
            // uniform float _Radius;
            uniform int _NBAtoms;
            uniform int _AORes;
            uniform int _IDSample;
            uniform float _NBParam;

// uniform float4 _TexPos;
            uniform float2 _ScreenSize;
            // float4x4 _MyVP;
            // float4x4 _CustomV;
            // float4x4 _CustomP;
            float4x4 _CustomVP;

            uniform int _AtomsByCol;

            float4x4 _InverseView;
            uniform float4x4 _ViewProjInv;

            float4x4 _ClipToWorld;

            sampler2D _CameraDepthTexture;
            sampler2D _LastCameraDepthTexture;

// vertex input: position
            struct appdata {
float4 vertex : POSITION;
float2 uv : TEXCOORD0;
            };


// Variables passees du vertex au pixel shader
            struct v2p {
float4 p : POSITION;
float4 srcPos : TEXCOORD0;

            };

            struct fragment_out
            {
float4 color : SV_Target;
            };

            float3 sphereMapping_uvTo3D(float2 uv) {
                float2 absuv = abs(uv);
                float h = 1.0 - absuv.x - absuv.y;
                if (h >= 0.0)
                    return float3(uv.x, uv.y, h);

                return float4(sign(uv.x) * (1.0 - absuv.y) , sign(uv.y) * (1.0 - absuv.x), h, 1.0);
            }
            float3 wpositionFromDepthTexture(float2 uv) {

                const float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
                const float2 p13_31 = float2(unity_CameraProjection._13, unity_CameraProjection._23);
                const float isOrtho = unity_OrthoParams.w;
                const float near = _ProjectionParams.y;
                const float far = _ProjectionParams.z;

                float d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
                
                #if defined(UNITY_REVERSED_Z)
                    d = 1 - d;
                #endif

                float zOrtho = lerp(near, far, d);

                float zPers = near * far / lerp(far, near, d);
                float vz = lerp(zPers, zOrtho, isOrtho);

                float3 vpos = float3((uv * 2 - 1 - p13_31) / p11_22 * lerp(vz, 1, isOrtho), -vz);
                float4 wpos = mul(_InverseView, float4(vpos, 1));

                return wpos.xyz;
            }


            float3 worldPosFromUV(float2 pixelcoord, float3 centerPos, float radius) {

                float3 pos3DModel = sphereMapping_uvTo3D(pixelcoord);
                pos3DModel.z = -pos3DModel.z;
                float3 dir = normalize(pos3DModel) * radius;

                return centerPos + dir;
            }

            v2p ballimproved_v (appdata v) {
                v2p o; // Shader output

                o.p = UnityObjectToClipPos(v.vertex);
                o.srcPos = ComputeScreenPos(o.p);

                return o;
            }

// float ballimproved_p (v2p i) : COLOR {
            float4 ballimproved_p (v2p i) : COLOR {


                float2 screenPos = (i.srcPos.xy / i.srcPos.w) * _ScreenSize.xy;

                int idx = ((int)screenPos.x) / (float)_AORes;
                int idy = ((int)screenPos.y) / (float)_AORes;

                int idAtom = (_ScreenSize.x / (float)_AORes) * idy + idx;

                if (idAtom >= _NBAtoms)
                    return float4(0, 0, 1, 1);


                int localx = ((int)screenPos.x) % (float)_AORes;
                int localy = ((int)screenPos.y) % (float)_AORes;

                //between 0 & 1
                float2 pixelcoord = float2(localx, localy) / (_AORes - 1);

                //between -1 & 1
                pixelcoord = (pixelcoord - 0.5) * 2;

                float4 info = tex2D(_AtomPos, float2(idAtom % _AtomsByCol / (float)_AtomsByCol,
                                    (idAtom / _AtomsByCol) / (float)(_AtomPos_TexelSize.w - 1)));
                float3 centerPos = info.xyz;
                float Radius = info.w;

                float3 pos3DWorld = worldPosFromUV(pixelcoord, centerPos, Radius);
                float4 posW = float4(pos3DWorld.xyz, 1.0);


                // float4 pos3DProj = mul(_CustomVP, posW );
                // float4 p = ComputeScreenPos(pos3DProj);
                // p.xyz = p.xyz / p.w;
                // float3 wscene = depthFromDepthTexture(p);

//----------------------------------------------------------

                // float4 pos3DProj = mul(mul(_CustomP , _CustomV), posW);
                float4 pos3DProj = mul(_CustomVP, posW);
                // float4 pos3DProj = mul(UNITY_MATRIX_VP, posW );


                // float4 pos3DScreen = pos3DProj * 0.5;
                // pos3DScreen.xy +=  pos3DScreen.w;
                // pos3DScreen.zw = pos3DProj.zw;

                // pos3DScreen.y =  1 - (pos3DScreen.y/pos3DScreen.w);
                // pos3DScreen.x = pos3DScreen.x / pos3DScreen.w;
                float4 p = ComputeScreenPos(pos3DProj);
                p.xyz = p.xyz / p.w;



                float4 one = float4(1, 1, 1, 1);
                float4 zero = float4(0, 0, 0, 1);

                float4 color = one;

                float3 wscene = wpositionFromDepthTexture(p);

                float3 normalWorld = pos3DWorld - centerPos;
                float3 viewDir = WorldSpaceViewDir(float4(centerPos, 1.0));
                // viewDir = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));

                float previousVal = tex2D(_AOTex, i.srcPos.xy / i.srcPos.w);

                if (_IDSample == 0)
                    // previousVal = one;
                    previousVal = zero;


                // float threshold = 0.05;
                float threshold = 0.05f;
                if ( abs(wscene.z - posW.z)  > threshold){
                    // color = zero;
                    color = -one;
                }
                // color = -one;
                if (dot(viewDir, normalWorld) <= 0 )
                color = one;
                // if (dot(viewDir, normalWorld) > 0 )
                // color = zero;


                color = previousVal + (color) / _Sampling;
                return color;


            }
            ENDCG

        }
    }
}
