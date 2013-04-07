/// @file MatCap.shader
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
/// $Id: MatCap.shader 210 2013-04-06 20:52:41Z baaden $
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

// Upgrade NOTE: replaced 'PositionFog()' with multiply of UNITY_MATRIX_MVP by position
// Upgrade NOTE: replaced 'V2F_POS_FOG' with 'float4 pos : SV_POSITION'
// Upgrade NOTE: replaced 'glstate.matrix.modelview[0]' with 'UNITY_MATRIX_MV'

Shader "MatCap" {
        Properties {
            _Color ("Main Color", Color) = (0.5,0.5,0.5,1)
            _BumpMap ("Bumpmap (RGB)", 2D) = "bump" {}
            _MatCap ("MatCap (RGB)", 2D) = "white" {}
        }
       
        Subshader {
            Tags { "RenderType"="Opaque" }
            /* Upgrade NOTE: commented out, possibly part of old style per-pixel lighting: Blend AppSrcAdd AppDstAdd */
            Fog { Color [_AddFog] }
           
            Pass { 
                Name "BASE"
                Tags { "LightMode" = "Always" }
//               	Cull front
                CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members uv,TtoV0,TtoV1)
#pragma exclude_renderers d3d11 xbox360
// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct v2f members uv,TtoV0,TtoV1)
				#pragma exclude_renderers xbox360
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_fog_exp2
                #pragma fragmentoption ARB_precision_hint_fastest
                #include "UnityCG.cginc"
 
                struct v2f {
                    float4 pos : SV_POSITION;
                    float2  uv;
                    float3  TtoV0;
                    float3  TtoV1;
                };
 
                uniform float4 _BumpMap_ST;
 
                v2f vert (appdata_tan v)
                {
                    v2f o;
                    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                    o.uv = TRANSFORM_TEX(v.texcoord,_BumpMap);
 
                    TANGENT_SPACE_ROTATION;
                    o.TtoV0 = mul(rotation, UNITY_MATRIX_MV[0].xyz);
                    o.TtoV1 = mul(rotation, UNITY_MATRIX_MV[1].xyz);
                    return o;
                }
               
                uniform float4 _Color;
                uniform sampler2D _BumpMap;
                uniform sampler2D _MatCap;
               
                float4 frag (v2f i) : COLOR
                {
                    // get normal from the normal map
                    float3 normal = tex2D(_BumpMap, i.uv).xyz * 2 - 1;
                    half2 vn;
                    vn.x = dot(i.TtoV0, normal);
                    vn.y = dot(i.TtoV1, normal);
                   
                    float4 matcapLookup = tex2D(_MatCap, vn*0.5 + 0.5);
                   
                    return _Color*matcapLookup*1.5;
                }
                ENDCG
            }
        }
    }