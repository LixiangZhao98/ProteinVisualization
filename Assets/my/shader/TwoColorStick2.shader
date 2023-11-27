Shader "Custom/TwoColorStick2" 
{

    Properties
    {
         _MainTex ("map", 2D) = "white" {}
         _Color1 ("Color1", Color) = (1.0, 0.0, 0.0, 1.0)
         _Color2 ("Color2", Color) = (0.0, 1.0, 0.0, 1.0)
		[PowerSlider(1)] _Pos1("sepPos", Range(0.0, 1.0)) = 0.2
    }
	
    SubShader
    {
        Pass
        { 
            Tags {"Queue" = "Transparent" "RenderType"="Transparent" }

            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct a2v
            {
                float4 pos: POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos: SV_POSITION;
            };

			sampler2D _MainTex;
            fixed4 _Color1;
            fixed4 _Color2;
			float _Pos1;

            v2f vert (a2v v)
            {
                v2f o;
       
                o.pos = UnityObjectToClipPos(v.pos);
                o.uv = v.uv;
                return o;
}
            fixed4 frag (v2f i) : SV_Target
            {
           
                fixed4 col;
      
                float lp = 0.0;
		
				if (i.uv.y >= _Pos1)
				{
				
					lp = (1 - i.uv.y) / (1 - _Pos1);
				
					col = lerp(_Color1, _Color2, lp);
				}

				else
				{
					col = _Color2;
				}

                return tex2D(_MainTex, i.uv) * col;
            }
            ENDCG
        }
    }
}
