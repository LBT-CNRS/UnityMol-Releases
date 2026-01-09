Shader "UCLA Game Lab/Wireframe Double Sided"
{
    Properties 
    {
        _Color ("Line Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _Thickness ("Thickness", Float) = 0.1
        _Firmness ("Line Firmness", Float) = 10

        [HideInInspector]
        _ZWrite("_ZWrite", Float) = 1.0

        [HideInInspector]
        _Cull("_Cull", Float) = 2.0

        [Toggle] _UseFog ("Enable fog", Float) = 0.0
        _FogStart ("Fog start", Float) = 0.0
        _FogDensity ("Fog density", Float) = 0.5

        [Toggle] _LimitedView ("Enable limited view", Float) = 0.0
        _LimitedViewRadius ("Limited view Radius", Float) = 10.0
        _LimitedViewCenter ("Limited view Center", Vector) = (0.0, 0.0, 0.0)

    }

    CustomEditor "UCLAGameLabWireframeMaterialEditor"

    SubShader 
    {
        Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }

        // Render back faces first
        Pass
        {
            Name "BACKSIDE"
            
            Blend SrcAlpha OneMinusSrcAlpha
            // ZWrite[_ZWrite]
            ZWrite Off
            Cull Front

            CGPROGRAM
            #include "UnityCG.cginc"
            #include "UCLA GameLab Wireframe Shaders.cginc"
            // #pragma target 5.0
            #pragma vertex UCLAGL_vert
            #pragma geometry UCLAGL_geom
            #pragma fragment UCLAGL_frag
            #pragma shader_feature UCLAGL_CUTOUT
            #pragma shader_feature UCLAGL_DISTANCE_AGNOSTIC

            ENDCG
        }

        // Then front faces
        Pass
        {
            Name "FRONTSIDE"

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite[_ZWrite]
            // ZWrite On
            Cull[_Cull]

            CGPROGRAM
            #include "UnityCG.cginc"
            #include "UCLA GameLab Wireframe Shaders.cginc"
            // #pragma target 5.0
            #pragma vertex UCLAGL_vert
            #pragma geometry UCLAGL_geom
            #pragma fragment UCLAGL_frag
            #pragma shader_feature UCLAGL_CUTOUT
            #pragma shader_feature UCLAGL_DISTANCE_AGNOSTIC

            ENDCG
        }
    }

    SubShader
    {
        Tags{ "LightMode" = "ShadowCaster" }

        Pass
        {
            Name "SHADOWS"

            CGPROGRAM
            #define UCLAGL_CUTOUT 1
            
            #include "UnityCG.cginc"
            #include "UCLA GameLab Wireframe Shaders.cginc"
            // #pragma target 5.0
            
            #pragma vertex UCLAGL_vert
            #pragma geometry UCLAGL_geom
            #pragma fragment UCLAGL_frag

            ENDCG
        }
    }
}
