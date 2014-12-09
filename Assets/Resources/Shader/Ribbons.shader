Shader "Custom/Ribbons" {


	SubShader {
		Tags { "RenderType"="Opaque" }
	    Pass {
	    	Cull off
			Name "BASE"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma debug
			
			struct appdata {
				float4 position : POSITION;
				float4 color : COLOR;
				float3 normal : NORMAL;
			};
					  
					  
			struct v2f {
				float4 position : POSITION;
				fixed4 color : COLOR;
				float3 normal : TEXCOORD0;
			};
	 
			v2f vert (appdata i) {
				v2f o;
				o.position = mul (UNITY_MATRIX_MVP, i.position);
				o.color = i.color;
				//o.normal = i.normal;
				o.normal = mul( ((float3x3)UNITY_MATRIX_MVP), i.normal);
				return o;
			}
	        
			float4 frag (v2f i) : COLOR	{
				float sAmbient = 0.08;
				float4 ambientLight = float4(sAmbient,sAmbient,sAmbient,1);
				float3 pos = i.position.xyz;
				float3 normal = normalize(i.normal);
				float4 color = i.color;
				
				float3 lightVector = float3(0,0,-1); // light position hard-coded
				float dotProduct = abs(dot(normal, lightVector));
				float4 diffuse = dotProduct * color; // float4(0,0,1,1);
				
				return diffuse + ambientLight;
			}
			ENDCG
		} // End PASS
			 
	} // end sub shader
	FallBack "Specular"
}