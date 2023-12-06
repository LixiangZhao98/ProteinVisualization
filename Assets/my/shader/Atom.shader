// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Atom" {
	Properties {
		_Color ("Tint", Color) = (0, 0, 0, 1)
		_MainTex ("Texture", 2D) = "white" {}

        _Diffuse  ("Diffuse",  Color)  =  (1,  1,  1,  1)
        _Specular  ("Specular",  Color)  =  (1,  1,  1,  1)
        _Gloss  ("Gloss",  Range  (8.0,  256))  =  20
	}
	SubShader {
		Tags{ "RenderType"="Transparent" "Queue"="Transparent" "LightMode"="ForwardBase"}
Pass{
		CGPROGRAM
        #include "Unitycg.cginc"
        #include "Lighting.cginc"

        #pragma vertex vert
        #pragma fragment frag



		sampler2D _MainTex;
		fixed4 _Color;

		float4 _Diffuse;
		float4 _Specular;
		float _Gloss;

		struct appdata 
        {
			float4 vertex: POSITION;
            float3 normal: NORMAL;
		};
        struct v2f
        {
        	float4 pos: SV_POSITION;
            float3 color: COLOR;
        };

        v2f vert (appdata v)
        {
        v2f o;
        o.pos=UnityObjectToClipPos(v.vertex);
        fixed3 ambiebt=UNITY_LIGHTMODEL_AMBIENT.xyz;
        fixed3 worldNormal=normalize(mul((float3x3)unity_ObjectToWorld,v.normal));
        fixed3 worldLightDir=normalize(_WorldSpaceLightPos0.xyz);
        fixed3 diffuse=_LightColor0.rgb*_Diffuse.rgb*saturate(dot(worldNormal,worldLightDir));
        fixed3 reflectDir=normalize(reflect(-worldLightDir,worldNormal));
        fixed3 viewDir=normalize(_WorldSpaceCameraPos.xyz-mul(unity_ObjectToWorld,v.vertex).xyz);
        fixed3 specular=_LightColor0.rgb*_Specular.rgb*pow(saturate(dot(reflectDir,viewDir)),_Gloss);
        o.color=ambiebt+diffuse+specular;
        return o;
        }
		fixed4 frag (v2f i) :SV_Target
        {
         return fixed4(i.color,1.0);
		}
		ENDCG
        }
	}
	FallBack "Standard"
}