Shader "FX/Glass/Stained BumpDistorted Specular" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,0.5)
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
		_BumpAmt  ("Distortion", range (0,128)) = 10
		_MainTex ("Tint Color (RGB)", 2D) = "white" {}
		_BumpMap ("Bumpmap (RGB)", 2D) = "bump" { }
	} 
 
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Opaque" }
		UsePass "FX/Glass/Stained BumpDistort/BASE"
		UsePass " BumpedSpecular/PPL"
	}
	
	// Fallback shader for older cards & non-PRO Unities.
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Opaque" }
		Blend DstColor Zero
		Pass {
			Name "BASE"
			SetTexture [_MainTex] {	combine texture }
		}
	}
} 
