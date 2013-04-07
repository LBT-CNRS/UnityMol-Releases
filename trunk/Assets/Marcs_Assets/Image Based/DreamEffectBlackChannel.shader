// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'
// Upgrade NOTE: replaced 'texRECT' with 'tex2D'

Shader "Hidden/DreamEffect BlackChannel" {
	Properties {
		_MainTex ("", RECT) = "white" {}
		_ContrastPower ("ContrastPower", Float ) = 1.0
		_ContrastBias ("ContrastBias", Float ) = 0.5
	}

	SubShader {
		Pass {
			ZTest Always Cull Off ZWrite Off Fog { Mode off }
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#include "UnityCG.cginc"
			
			uniform sampler2D _MainTex;
			uniform float _ContrastPower;
			uniform float _ContrastBias;
			
			float4 frag (v2f_img i) : COLOR
			{
				float4 original = tex2D(_MainTex, i.uv);
				float blackChannel;
				if( ( 1 - original.r ) <= ( 1 - original.g ) ) blackChannel = original.r;
				else blackChannel = original.g;
				if( ( 1 - blackChannel ) >= ( 1 - original.b ) ) blackChannel = original.b;
				blackChannel = ((blackChannel - _ContrastBias ) * _ContrastPower ) + _ContrastBias;
				if( blackChannel < 0.0 ) blackChannel = 0.0;
				else if( blackChannel > 1.0 ) blackChannel = 1.0;
				original.r = blackChannel;
				original.g = blackChannel;
				original.b = blackChannel;
				original.a = 1;
				return original;
			}
		ENDCG
		}
	}
	Fallback off
}