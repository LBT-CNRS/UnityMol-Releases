Shader "Custom/TextureAnimation" {
// Shader "Custom/Vertex Color/Animated Texture/Unlit VC Scroll with Transparency (2x 1 texture)" {
  Properties {
    _MainTex ("Texture", 2D) = "white" {}
    
    speedX ("Speed X", Float) = 1
    speedY ("Speed Y", Float) = 1
    
  }
  SubShader {
    Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}

    //ZWrite Off // on might hide behind pixels, off might miss order
    //Blend SrcAlpha OneMinusSrcAlpha
    Blend SrcAlpha OneMinusSrcAlpha
    // Blend One One
    // ColorMask RGB
    Cull Off
    Lighting Off Fog { Mode Off }

    Pass {
        CGPROGRAM
        #pragma vertex vert_vct
        #pragma fragment frag_mult
        #pragma fragmentoption ARB_precision_hint_fastest
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        
        float speedX;
        float speedY;
    
        float4 _MainTex_ST;

        struct vin_vct 
        {
            float4 vertex : POSITION;
            fixed4 col : COLOR0;
            float2 texcoord : TEXCOORD0;
        };

        struct v2f_vct
        {
            fixed4 col : COLOR0;
            float4 vertex : POSITION;
            float2 uv : TEXCOORD2;
        };

        v2f_vct vert_vct(vin_vct v)
        {
            v2f_vct o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
            o.uv += float2(speedX, speedY) * _Time.x;
            o.col = v.col;

            return o;
        }

        fixed4 frag_mult(v2f_vct i) : COLOR
        {
            fixed4 col1 = tex2D(_MainTex, i.uv);

            return col1 * i.col;
            return col1;

        }

        ENDCG
    }
  }
  Fallback "Transparent/Diffuse"
}