Shader "Unlit/SurfaceVertexColorNotCull"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		[Toggle] _UseFog ("Enable fog", Float) = 0.0
		_FogStart ("Fog start", Float) = 0.0
		_FogDensity ("Fog density", Float) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				// UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 wPos : TEXCOORD1;
				float4 color : COLOR;
				bool hide : TEXCOORD2;

			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _UseFog;
			float _FogStart;
			float _FogDensity;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				// UNITY_TRANSFER_FOG(o,o.vertex);
				o.wPos = mul(unity_ObjectToWorld, v.vertex);
				o.color = v.color;
				o.hide = (v.uv2.x < 0.00001f);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				if(i.hide){
					clip(-1);
				}
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				// apply fog
				// UNITY_APPLY_FOG(i.fogCoord, col);

				if(_UseFog){
					// float fogFactor = smoothstep(_FogEnd, _FogStart, i.wPos.z);		
					float fogFactor = exp(_FogStart - i.wPos.z  / max(0.0001, _FogDensity));
					col.rgb = lerp(unity_FogColor, col.rgb, saturate(fogFactor));
				}

				return col;
			}
			ENDCG
		}
	}
}
