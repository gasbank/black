//
// _PaletteIndex로 지정한 팔레트 색상의 모든 칸을
// _RenderColor로 지정한 색상으로 한번에 렌더링
//
Shader "Black/Single Palette Shader"
{

    Properties
    {
        _MainTex ("Main", 2D) = "white" {}
        _A1Tex ("A1 Texture", 2D) = "white" {}
        _A2Tex ("A2 Texture", 2D) = "white" {}
        _RenderColorTex ("Render Color Texture", 2D) = "red" {}
        _FullRender ("Full Render", Float) = 0
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        
        
    }
    
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
        ColorMask [_ColorMask]
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"
            
            

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 screenPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex; // 안쓰지만 없으면 경고 메시지 나오니까 보기 싫어서 넣어 둔다.
            sampler2D_float _A1Tex;
            sampler2D_float _A2Tex;
            sampler2D _RenderColorTex;
            
            float4 _A1Tex_ST;
            float4 _RenderColor;
            int _PaletteIndex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _A1Tex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float a1 = tex2D(_A1Tex, i.uv).a * 255.0f;
                float a2 = tex2D(_A2Tex, i.uv).a * 255.0f;
                
                float paletteIndex = fmod(a1, 64.0f);

                //float4 col = _RenderColor;
                
                float2 sp = i.screenPos.xy * _ScreenParams.xy;
                
                float4 col = tex2D(_RenderColorTex, sp / 50);
                
                col.a = 1 - saturate(abs(paletteIndex - _PaletteIndex));
                
                return col;
            }
            ENDCG
        }
    }
}
