Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _A1Tex ("A1 Texture", 2D) = "white" {}
        _A2Tex ("A2 Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {
            //"Queue" = "Transparent"
             //"RenderType"="Transparent"
             "RenderType"="Opaque"
          }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency

        Pass
        {
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _A1Tex;
            sampler2D _A2Tex;
            float4 _A1Tex_ST;
            float4 _Palette[64];
            int _IslandIndex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _A1Tex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                int a1 = tex2D(_A1Tex, i.uv).a * 255;
                int a2 = tex2D(_A2Tex, i.uv).a * 255;
                
                int paletteIndex = a1 & ((1 << 6) - 1);
                int islandIndex = (a1 >> 6) | (a2 << 2);
                
                float4 col = _Palette[paletteIndex];
                
                col.a = 1 - abs(islandIndex - _IslandIndex);
                //col.a = 1;
                
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
