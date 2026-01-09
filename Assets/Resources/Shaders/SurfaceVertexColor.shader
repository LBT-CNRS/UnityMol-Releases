// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/SurfaceVertexColor" {
	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_Metallic ("Metallic", Range(0, 1)) = 0.0
		_AOIntensity ("AO Intensity", Range(0, 20.0)) = 0

		[Toggle] _UseFog ("Enable fog", Float) = 0.0
		_FogStart ("Fog start", Float) = 0.0
		_FogDensity ("Fog density", Float) = 0.5
		[Toggle] _LimitedView ("Enable limited view", Float) = 0.0
		_LimitedViewRadius ("Limited view Radius", Float) = 10.0
		_LimitedViewCenter ("Limited view Center", Vector) = (0.0, 0.0, 0.0)

	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		// Cull Off
		// LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard fullforwardshadows vertex:vert finalcolor:mycolor

		// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0
#pragma multi_compile_instancing

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float aoFactor : TEXCOORD1;
			float4 color : COLOR;
			float3 worldPos;
			float3 objPos;
			float eyeDepth;
			bool hide : TEXCOORD2;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _AOIntensity;

		// uniform sampler2D _CameraDepthTexture;
		float _FogStart;
		float _FogDensity;
		float _UseFog;

		float _LimitedView;
		float _LimitedViewRadius;
		float3 _LimitedViewCenter;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
		// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)


		void vert( inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			COMPUTE_EYEDEPTH(o.eyeDepth);
			o.objPos = v.vertex;
			o.hide = (v.texcoord2.x > 1.5f);
			o.aoFactor = v.texcoord1.x;
		}


		void mycolor (Input IN, SurfaceOutputStandard o, inout fixed4 color) {

			if (_UseFog) {

				float d = IN.worldPos.z;

				float fogFactor = exp(_FogStart - d  / max(0.0001, _FogDensity));

				color.rgb = lerp(unity_FogColor, color.rgb, saturate(fogFactor));
			}

		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			if (IN.hide) {
				clip(-1);
			}

			if (_LimitedView) {
				float d = distance(IN.objPos, _LimitedViewCenter);
				if (d > _LimitedViewRadius) {
					// o.Albedo = float3(1,0,0);
					// return;
					discard;
				}
			}
			half ao = 1.0;
			if (_AOIntensity) {
				ao = saturate(IN.aoFactor * _AOIntensity);
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
