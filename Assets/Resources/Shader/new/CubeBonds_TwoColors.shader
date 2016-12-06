// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

/// @file CubeBonds_TwoColors.shader
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
/// $Id: CubeBonds_TwoColors.shader 347 2013-08-20 09:36:34Z erwan $
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

Shader "Custom/CubeBonds_TwoColors" {
	Properties {
		_Color1 ("Color Atom 1", Color)		=	(1,		0.1,	0.1,	1.0)
		_Color2 ("Color Atom 2", Color)		=	(0.1,	0.1,	1.0,	1.0)
	    _Pos1 ("Position Atom 1", Vector)	=	(0.0,	0.0,	0.0,	1.0)
	    _Pos2 ("Position Atom 2", Vector)	=	(3.0,	0.0,	0.0,	1.0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma surface surf Lambert vertex:vert

		float4 _Color1;
		float4 _Color2;
		float3 _Pos1;
		float3 _Pos2;

		struct Input {
			float3 col;
		};

		void vert (inout appdata_full v, out Input o) {
			float3 pos = mul(v.vertex,unity_WorldToObject);
			
			o.col = _Color1.rgb;
			
			if(_Pos1.x > _Pos2.x){
		    	if(pos.x < 0)
		    		o.col = _Color2.rgb;
		    }
		    else{ // if(_Pos1.x < _Pos2.x)
		    	if(pos.x > 0)
		    		o.col = _Color2.rgb;
		    }
		}

			//float minDist = 99999999999.0;
//			float3 pos;
//			float3 pos1;
//			float3 pos2;
			
			
//			pos = v.vertex;
			//pos = mul(v.vertex,_World2Object);
//			pos1 = mul (_Pos1, (float3x3)_World2Object);
//			pos2 = mul (_Pos2, (float3x3)_World2Object);
//			float minDist = distance(pos, _Pos2);
//	    	o.col = _Color2.rgb;
	    	
//		    float dist = distance(pos, _Pos1);
//		    if(dist < minDist)
//		    	o.col = _Color1.rgb;
			
			
			//float3 pos = mul(v.vertex,_World2Object);
			
			
			//o.col = _Color1.rgb;
		    //if(pos.x < 0)
		    	//o.col = _Color2.rgb;		    	
//		}

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = IN.col.rgb;
			o.Alpha = 1;
		}
	ENDCG

	} 
	FallBack "Diffuse"
}
