Shader "Custom/StickColor" 
{

    Properties
    {
         _Cutoff("Cutoff",float)=7
         _AtomPosTex ("AtomPosTex", 2D) = "white" {}
    }
	
    SubShader
    {
        Pass
        { 
            Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
		
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag
   

            struct a2v
            {
                float4 pos: POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float2 instanceID :TEXCOORD2;

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos: SV_POSITION;
                float3 normal : NORMAL;
                fixed4 col:TEXCOORD3;
                float rank:TEXCOORD4;


            };
            
            StructuredBuffer<int> _AtomRank1Buffer;
			StructuredBuffer<int> _AtomRank2Buffer;
            StructuredBuffer<float4> _AtomColor1Buffer;
            StructuredBuffer<float4> _AtomColor2Buffer;
            int _ConnectNum;


			float _Pos1;

            float _Cutoff;

            v2f vert (a2v v)
            {
                v2f o;
       
                o.pos = UnityObjectToClipPos(v.pos);
                float3 worldpos=mul(unity_ObjectToWorld,v.pos);
                
           float4 Color1=_AtomColor1Buffer[ (int)v.instanceID.x];
           float4 Color2=_AtomColor2Buffer[(int)v.instanceID.x];
           uint Rank1=_AtomRank1Buffer[(int)v.instanceID.x];
           uint Rank2=_AtomRank2Buffer[(int)v.instanceID.x]; 


                float lp = 0.0;
				if(dot(v.normal,float3(0,1,0))>0.99)
                {
                o.col =Color1;
				o.rank=Rank1;
                };

                if(dot(v.normal,float3(0,-1,0))>0.99)
                {
                o.col =Color2;
				o.rank=Rank2;
                };

                if(v.normal.y==0)
                {
                
              //  if (v.uv.y >= _Pos1)
				//{
                lp = 1-v.uv.y;
				o.col = lerp(Color1, Color2, lp);
               // }
              //  else
               // {col =  _Color2;}


				o.rank=lerp(Rank1,Rank2,lp);

                };

                
                o.uv = v.uv; 
                o.normal=v.normal;
                return o;
           }


            fixed4 frag (v2f i) : SV_Target
            {

             if(i.rank>_Cutoff)
             discard;

                return float4(i.col.xyz,1) ;
            }
            ENDCG
        }
    }
        Fallback "VertexLit"
}

