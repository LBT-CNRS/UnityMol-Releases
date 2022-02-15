// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/SimpleTransparent" {
   Properties {
      _MainTex ("RGBA Texture Image", 2D) = "white" {} 
      _Cutoff ("Alpha Cutoff", Float) = 0.5
   }
   SubShader {
      Pass {    
         Cull Off // since the front is partially transparent, 
            // we shouldn't cull the back

         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 
 
         #include "UnityCG.cginc"

         uniform sampler2D _MainTex;
         uniform float4 _MainTex_ST;
         uniform float _Cutoff;
 
         struct vertexInput {
            float4 vertex : POSITION;
            float4 texcoord : TEXCOORD0;
            half4  color    : COLOR0;
         };
         struct vertexOutput {
            float4 pos : SV_POSITION;
            float2 tex : TEXCOORD0;
            half4 color: COLOR0;
         };
 
         vertexOutput vert(vertexInput input) 
         {
            vertexOutput output;
 
            output.tex = TRANSFORM_TEX(input.texcoord, _MainTex);
            output.pos = UnityObjectToClipPos(input.vertex);
            output.color = input.color; 
            return output;
         }

         float4 frag(vertexOutput input) : COLOR
         {
            float4 textureColor = tex2D(_MainTex, input.tex.xy);  
            if (textureColor.a < _Cutoff)
               // alpha value less than user-specified threshold?
            {
               discard; // yes: discard this fragment
            }
            return textureColor * input.color;
         }
 
         ENDCG
      }
   }
   Fallback "Unlit/Transparent Cutout"
}
