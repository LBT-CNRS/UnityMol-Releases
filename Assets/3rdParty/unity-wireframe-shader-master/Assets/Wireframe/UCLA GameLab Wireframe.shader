Shader "UCLA Game Lab/Wireframe" 
{   
    Properties 
    {
        _Color ("Line Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _Thickness ("Thickness", Float) = 1
        _Firmness("Line Firmness", Float) = 1

        [HideInInspector]
        _ZWrite("_ZWrite", Float) = 1.0

        [HideInInspector]
        _Cull("_Cull", Float) = 2.0
    }

    CustomEditor "UCLAGameLabWireframeMaterialEditor"

    SubShader
    {
        Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }

        UsePass "UCLA Game Lab/Wireframe Double Sided/FRONTSIDE"
    }
    
    // TODO: UsePass didn't seem to be working here?
    SubShader
    {
        Tags{ "LightMode" = "ShadowCaster" }

        // UsePass "UCLA Game Lab/Wireframe Double Sided/FRONTSIDE"
        Pass
        {    
            CGPROGRAM
            #define UCLAGL_CUTOUT 1
            
            #include "UnityCG.cginc"
            #include "UCLA GameLab Wireframe Shaders.cginc"
            #pragma target 5.0
            
            #pragma vertex UCLAGL_vert
            #pragma geometry UCLAGL_geom
            #pragma fragment UCLAGL_frag

            ENDCG
        }
    }
}
