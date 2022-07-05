Shader "Custom/FieldlineAnimation" {
  Properties {
    
    speed ("Speed", Range (0.0, 5.0)) = 1
    size ("Size", Range(0.01, 0.99)) = 0.2

    [Toggle] _UseFog ("Enable fog", Float) = 0.0
    _FogStart ("Fog start", Float) = 0.0
    _FogDensity ("Fog density", Float) = 0.5

  }
  SubShader {
    Cull Off

    Pass {
        CGPROGRAM
        #pragma vertex vert_vct
        #pragma fragment frag_mult
        #pragma fragmentoption ARB_precision_hint_fastest
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        
        float speed;
        float size;
        float _UseFog;
        uniform float _FogStart;
        uniform float _FogDensity;

        struct vin_vct 
        {
            float4 vertex : POSITION;
            fixed4 col : COLOR0;
            float2 texcoord : TEXCOORD0;
            float2 offset : TEXCOORD1;
        };

        struct v2f_vct
        {
            // fixed4 col : COLOR0;
            float4 vertex : POSITION;
            float2 uv : TEXCOORD2;
            float2 offset : TEXCOORD3;
            fixed4 col : COLOR0;
            float3 wPos : TEXCOORD1;
        };

        v2f_vct vert_vct(vin_vct v)
        {
            v2f_vct o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord;
            o.offset = v.offset;
            o.col = v.col;
            o.wPos = mul(unity_ObjectToWorld, v.vertex);
            return o;
        }

        fixed4 frag_mult(v2f_vct i) : COLOR
        {
            if(abs(i.uv.x - ((speed * _Time.y + i.offset.x) % 1.0)) > size){
                clip(-1);
            }

            fixed4 outCol = i.col;
            if(_UseFog){
                // float fogFactor = smoothstep(_FogEnd, _FogStart, i.wPos.z);
                float fogFactor = exp(_FogStart - i.wPos.z  / max(0.0001, _FogDensity));
                outCol.rgb = lerp(unity_FogColor, outCol.rgb, saturate(fogFactor));
            }
            return outCol;
        }

        ENDCG
    }
  }
  // Fallback "Standard"
}
