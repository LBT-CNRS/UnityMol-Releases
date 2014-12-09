Shader "Transparent/OIT_RED" {
	// Proprietes exposees a l'interface graphique
	Properties {
		_Color ("Color of surface", Color) = (1,0,0,0.5)
	}

   SubShader {
      Tags { "Queue" = "Transparent" } 
         // draw after all opaque geometry has been drawn
      Pass { 
         Cull Off // draw front and back faces
         ZWrite Off // don't write to depth buffer 
            // in order not to occlude other objects
         Blend Zero OneMinusSrcAlpha // multiplicative blending 
            // for attenuation by the fragment's alpha
 
         CGPROGRAM 
 
         #pragma vertex vert 
         #pragma fragment frag
 
         float4 vert(float4 vertexPos : POSITION) : SV_POSITION 
         {
            return mul(UNITY_MATRIX_MVP, vertexPos);
         }
 
         float4 frag(void) : COLOR 
         {
            return float4(1.0, 0.0, 0.0, 0.3); 
         }
 
         ENDCG  
      }
 
      Pass { 
         Cull Off // draw front and back faces
         ZWrite Off // don't write to depth buffer 
            // in order not to occlude other objects
         Blend SrcAlpha One // additive blending to add colors
 
         CGPROGRAM 
 
         #pragma vertex vert 
         #pragma fragment frag
 
         float4 vert(float4 vertexPos : POSITION) : SV_POSITION 
         {
            return mul(UNITY_MATRIX_MVP, vertexPos);
         }
 
         float4 frag(void) : COLOR 
         {
         	return float4(1.0, 0.0, 0.0, 0.5);
         }
 
         ENDCG  
      }
   }
}