// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/SurfaceVertexColor" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_AOIntensity ("AO Intensity", Range(0, 1.25)) = 0
		_AOPower ("AO Power", Range(1, 30)) = 10

        [Toggle] _UseFog ("Enable fog", Float) = 0.0
		_FogStart ("Fog start", Float) = 0.0
		_FogDensity ("Fog density", Float) = 0.5

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Cull Off
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert finalcolor:mycolor

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
			float3 worldPos;
			float eyeDepth;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _AOIntensity;
		float _AOPower;

		// uniform sampler2D _CameraDepthTexture;
		float _FogStart;
		float _FogDensity;
		float _UseFog;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)


		void vert( inout appdata_full v, out Input o){
			UNITY_INITIALIZE_OUTPUT(Input, o);
			COMPUTE_EYEDEPTH(o.eyeDepth);
		}

		fixed3 applyFog(fixed3 c, float3 wp){
			float viewDistance = length(_WorldSpaceCameraPos - wp);
			
			return lerp(c, unity_FogColor, saturate(0.5 * viewDistance));
			
		}

    void mycolor (Input IN, SurfaceOutputStandard o, inout fixed4 color) {

    	if(_UseFog){

	    	// color.rgb = applyFog(color.rgb, IN.worldPos);
			// float d = IN.eyeDepth;
			float d = IN.worldPos.z;
			// float d = Linear01Depth(tex2D(_CameraDepthTexture, IN.uv).r) * _ProjectionParams.z;
			// float fogFactor = (_FogStart - _FogFarPlane*d) / (_FogEnd - _FogStart);

			// float fogDens = 1.1;
			// float fogFactor = exp2( - fogDens * fogDens * d *d );

			// float fogFactor = smoothstep(_FogEnd, _FogStart, d);	

			float fogFactor = exp(_FogStart - d  / max(0.0001, _FogDensity));

			color.rgb = lerp(unity_FogColor, color.rgb, saturate(fogFactor));
		}

    }

		void surf (Input IN, inout SurfaceOutputStandard o) {

			half ao = 1.0;
			if(_AOIntensity){
				ao = 1 - pow((1-IN.color.a)*_AOIntensity, _AOPower );
			}


			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color * IN.color * ao;

			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
