Shader "Black/SDF"
{
    Properties
    {
        ColorTexture ("ColorTexture", 2D) = "white" {}
        SdfTexture ("SdfTexture", 2D) = "white" {}
        ColorCount("ColorCount", Range(1,10)) = 2
        Color0("Color0", COLOR) = (1,1,1,0)
        Color1("Color1", COLOR) = (0,0,0,1)
        StripeCount("StripeCount", Range(1,40)) = 2
        EdgeDistance("EdgeDistance", Range(0.9,1) ) = 0.99
        DistanceOffset("DistanceOffset", Range(0,1) ) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D ColorTexture;
            float4 ColorTexture_ST;
            sampler2D SdfTexture;
            float4 Color0;
            float4 Color1;
            int ColorCount;
            float StripeCount;
            float EdgeDistance;
            float DistanceOffset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, ColorTexture);
                return o;
            }

            float4 GetColor(int Index)
            {
                float4 Colors[2];
                Colors[0] = Color0;
                Colors[1] = Color1;
                return Colors[Index];
            }

            
            fixed4 frag (v2f i) : SV_Target
            {
                float4 Color = tex2D(ColorTexture,i.uv);
                Color.a = 1;

                float Distance = tex2D(SdfTexture,i.uv).a;
                if ( Distance >= EdgeDistance ) {
                    return Color;
                }

                Distance += DistanceOffset;
                Distance *= StripeCount;
                float Stripef = fmod( Distance, ColorCount );
                int Stripe = Stripef;
                return GetColor( Stripe );
            }
            ENDCG
        }
    }
}
