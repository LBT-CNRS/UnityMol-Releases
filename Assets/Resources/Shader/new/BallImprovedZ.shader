Shader "Custom/BallImprovedZ" {

Properties {
	_Rayon ("Rayon de la Sphere", float) = 0.1
	_Color ("Couleur de la Sphere", Color) = (1,0.1,0.1,1.0)
	_TexPos ("Position de la sphere", Vector) = (0.0,0.0,0.0,1.0)     // important que w != 0 !!
	_Visibilite ("Visibilite de la Sphere", float) = 1.0
	_Light ("Light vector", Vector) = (1,0,0,0)
	_Equation("Equation", Vector) = (1,1,1,1)
	_MatCap ("MatCap (RGB)", 2D) = "white" {}
}
	
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
    // extra pass that renders to depth buffer only
    Pass {
        ZWrite On
        ColorMask 0
    }
    
    UsePass "FvNano/Ball HyperBalls OpenGL"
    
	} 
	FallBack "Diffuse"
}
