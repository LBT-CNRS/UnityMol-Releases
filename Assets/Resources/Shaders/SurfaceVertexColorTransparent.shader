Shader "Custom/SurfaceVertexColorTransparent" {
 Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
}

Category {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    ZWrite Off
    Lighting Off
    Fog { Mode Off }
    Blend SrcAlpha OneMinusSrcAlpha 
    SubShader {

        Pass {


            SetTexture [_MainTex] {
                ConstantColor [_Color]
                Combine previous * constant, constant
            } 
        } 
    }
} 
}