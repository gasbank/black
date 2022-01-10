Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Main", 2D) = "white" {}
        _A1Tex ("A1 Texture", 2D) = "white" {}
        _A2Tex ("A2 Texture", 2D) = "white" {}
        _PaletteTex ("Palette Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {
            "Queue" = "Transparent"
             "RenderType"="Transparent"
             //"RenderType"="Opaque"
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

            sampler2D _MainTex; // 안쓰지만 없으면 경고 메시지 나오니까 보기 싫어서 넣어 둔다.
            sampler2D_float _A1Tex;
            sampler2D_float _A2Tex;
            sampler2D_float _PaletteTex;
            
            float4 _A1Tex_ST;
            float4 _Palette[64];
            int _IslandIndex;
            float _FullRender;
            float _SingleIsland;

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
                float a1 = tex2D(_A1Tex, i.uv).a * 255.0f;
                float a2 = tex2D(_A2Tex, i.uv).a * 255.0f;
                
                float paletteIndex = fmod(a1, 64.0f);
                int islandIndex = (int)((a1 / 64.0f) + (a2 * 4.0f));
                
                //float4 col = _Palette[paletteIndex];
                float2 paletteUv;
                paletteUv.x = (paletteIndex + 0.5f) / 64.0f;
                paletteUv.y = 0;
                float4 col = tex2D(_PaletteTex, paletteUv);
                
                // FULL VERSION
                //col.a = lerp(lerp(islandIndex <= _IslandIndex ? 1 : 0, 1 - abs(islandIndex - _IslandIndex), _SingleIsland), 1, _FullRender);
                
                // OPTIMIZED VERSION
                col.a = 1 - abs(islandIndex - _IslandIndex);
                
                return col;
            }
            ENDCG
        }
    }
}
