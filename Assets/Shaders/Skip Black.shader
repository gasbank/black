Shader "Black/Skip Black"
{
    Properties
    {
        ColorTexture ("ColorTexture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, ColorTexture);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float4 Color = tex2D(ColorTexture,i.uv);
                float checkZero = Color.r + Color.g + Color.b;
                if (checkZero == 0) {
                    Color = float4(1,1,1,1);
                }
                return Color;
            }
            ENDCG
        }
    }
}
