/*Shader "Custom/TwoColorStick2"   //需要把stick两端enable打开，在generatesticklim设置两端颜色
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
}*/

Shader "Custom/TwoColorStick2" 
{

    Properties
    {
         _MainTex ("map", 2D) = "white" {}
         _Color1 ("Color1", Color) = (1.0, 0.0, 0.0, 1.0)
         _Color2 ("Color2", Color) = (0.0, 1.0, 0.0, 1.0)
         _Rank1 ("Rank1", float) = 1
         _Rank2 ("Rank2", float) = 1
		//[PowerSlider(1)] _Pos1("sepPos", Range(0.0, 1.0)) = 1
         _Length("Length", float) = 1
         _Cutoff("Cutoff", float) = 7
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
            float _Rank1;
            float _Rank2;


			float _Length;
            float _Cutoff;

            v2f vert (a2v v)
            {
                v2f o;
       
                o.pos = UnityObjectToClipPos(v.pos);
                o.uv = v.pos;  //o.uv=v.uv 则圆柱体两头平面也会进行插值运算
                return o;
}
            fixed4 frag (v2f i) : SV_Target
            {
           
                fixed4 col;
                float rank;
                float lp = 0.0;
		
		
				
					lp = (1 - i.uv.y) / (_Length);
				
					col = lerp(_Color1, _Color2, lp);
				rank=lerp(_Rank1,_Rank2,lp);

             if(rank>_Cutoff)
             discard;

                return tex2D(_MainTex, i.uv) * col;
            }
            ENDCG
        }
    }
}

