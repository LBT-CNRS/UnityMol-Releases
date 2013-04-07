// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'
// Upgrade NOTE: replaced 'texRECTproj' with 'tex2Dproj'

// Per pixel bumped refraction.
// Uses a normal map to distort the image behind, and
// an additional texture to tint the color.

Shader "FX/Glass/Stained BumpDistort" {
Properties {
	_BumpAmt  ("Distortion", range (0,128)) = 10
	_MainTex ("Tint Color (RGB)", 2D) = "white" {}
	_BumpMap ("Bumpmap (RGB)", 2D) = "bump" {}
}

Category {

	// We must be transparent, so other objects are drawn before this one.
	Tags { "Queue"="Transparent" "RenderType"="Opaque" }
	
	// ------------------------------------------------------------------
	//  ARB fragment program
	
	SubShader {

		// This pass grabs the screen behind the object into a texture.
		// We can access the result in the next pass as _GrabTexture
		GrabPass {							
			Name "BASE"
			Tags { "LightMode" = "Always" }
 		}
 		
 		// Main pass: Take the texture grabbed above and use the bumpmap to perturb it
 		// on to the screen
		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }
			
CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it does not contain a surface program or both vertex and fragment programs.
#pragma exclude_renderers gles
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest 
#pragma fragmentoption ARB_fog_exp2

sampler2D _GrabTexture : register(s0);
float4 _GrabTexture_TexelSize;
sampler2D _BumpMap : register(s1);
sampler2D _MainTex : register(s2);

struct v2f {
	float4 uvgrab : TEXCOORD0;
	float2 uvbump : TEXCOORD1;
	float2 uvmain : TEXCOORD2;
};

uniform float _BumpAmt;

half4 frag( v2f i ) : COLOR
{
	// calculate perturbed coordinates
	half2 bump = tex2D( _BumpMap, i.uvbump ).rg * 2 - 1;
	float2 offset = bump * _BumpAmt;
	#ifdef SHADER_API_D3D9
	offset *= _GrabTexture_TexelSize.xy;
	#endif
	i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
	
	half4 col = tex2Dproj( _GrabTexture, i.uvgrab.xyw );
	half4 tint = tex2D( _MainTex, i.uvmain );
	
	return col * tint;
}

ENDCG
			// Set up the textures for this pass
			SetTexture [_GrabTexture] {}	// Texture we grabbed in the pass above
			SetTexture [_BumpMap] {}		// Perturbation bumpmap
			SetTexture [_MainTex] {}		// Color tint
		}
	}
	
	// ------------------------------------------------------------------
	//  Radeon 9000
	
	#warning Upgrade NOTE: SubShader commented out because of manual shader assembly
/*SubShader {

		GrabPass {							
			Name "BASE"
			Tags { "LightMode" = "Always" }
 		}
 		
		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }
			
			Program "" {
				SubProgram {
				Local 0, ([_BumpAmt],0,0,0.001)
"!!ATIfs1.0
StartConstants;
	CONSTANT c0 = program.local[0];
EndConstants;

StartPrelimPass;
	PassTexCoord r0, t0.stq_dq;	# refraction position
	SampleMap r1, t1.str;		# bumpmap
	MAD r0, r1.2x.bias, c0.r, r0;
EndPass;

StartOutputPass;
	SampleMap r0, r0.str;	# sample modified refraction texture
	SampleMap r2, t2.str;		# Get main color texture
	
	MUL r0, r0, r2;
EndPass; 
"
				}
			}
			SetTexture [_GrabTexture] {}
			SetTexture [_BumpMap] {}
			SetTexture [_MainTex] {}
		}
	}*/
	
	// ------------------------------------------------------------------
	// Fallback for older cards and Unity non-Pro
	
	SubShader {
		Blend DstColor Zero
		Pass {
			Name "BASE"
			SetTexture [_MainTex] {	combine texture }
		}
	}
}

}
