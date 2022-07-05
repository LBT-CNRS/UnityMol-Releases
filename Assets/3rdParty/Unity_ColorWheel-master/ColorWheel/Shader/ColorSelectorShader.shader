// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//Copyright (c) 2015, Felix Kate All rights reserved.
// Usage of this code is governed by a BSD-style license that can be found in the LICENSE file.

Shader "UI/ColorSelector" {
	Properties {

	}
	SubShader {
		Lighting Off
		Blend OneMinusDstColor OneMinusSrcAlpha
	
		Pass{		
			CGPROGRAM
			#pragma vertex vert
	        #pragma fragment frag
			#pragma target 3.0
			
			#include "UnityCG.cginc"
			
			//Prepare the inputs
			struct vertIN{
				float4 vertex : POSITION;
				float4 texcoord0 : TEXCOORD0;
			};
			
			struct fragIN{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			
			//Get the values from outside
			fixed4 _Color;
			
			//Fill the vert struct
			fragIN vert (vertIN v){
				fragIN o;
				
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord0;
				
				return o;
			}
			
			//Draw the circle
			fixed4 frag(fragIN i) : COLOR{
				
				fixed gradient = distance(i.uv.xy, fixed2(0.5, 0.5));
				
				return smoothstep(0.5 - 0.015, 0.5 + 0.015, 1 - gradient) - smoothstep(0.5 - 0.025, 0.5 + 0.025, 1 - gradient * 2);
			}
			
			ENDCG
			
		}
	} 
}
