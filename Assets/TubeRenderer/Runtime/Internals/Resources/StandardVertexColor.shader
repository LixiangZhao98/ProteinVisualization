// shader for TubeRenderer

Shader "Hidden/StandardVertexColor"
{
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Emission ("Color", Color) = (0,0,0,0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		
		// Physically based Standard lighting model, and enable shadows on all light types.
		#pragma surface surf Standard fullforwardshadows
	
		// Use shader model 3.0 target, to get nicer looking lighting.
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		fixed4 _Color;
		fixed4 _Emission;
		
		void surf( Input IN, inout SurfaceOutputStandard o )
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex ) * _Color * IN.color;
			o.Albedo = c.rgb;
			o.Alpha = c.a * IN.color.a;
			o.Emission = _Emission;
		}
		ENDCG
	} 
	FallBack "Standard"
}