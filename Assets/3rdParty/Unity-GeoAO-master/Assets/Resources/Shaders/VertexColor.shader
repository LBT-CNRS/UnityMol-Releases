Shader "Vertex Colored" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader {
        Tags {"RenderType" = "Opaque"}
        LOD 200
       
        CGPROGRAM
        #pragma surface surf Lambert 
       
        sampler2D _MainTex;
 
        struct Input {
            float2 uv_MainTex;
            float4 color: Color; // Vertex color
        };
 
        void surf (Input IN, inout SurfaceOutput o) {
            half4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb * IN.color.rgb; // vertex RGB
            o.Alpha = c.a * IN.color.a; // vertex Alpha
        }
        ENDCG
    }
    FallBack "Diffuse"
}