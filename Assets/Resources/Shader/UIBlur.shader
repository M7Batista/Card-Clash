Shader "UI/Blur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Size ("Blur Size", Range(0, 100)) = 70.0
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                float2 texcoord  : TEXCOORD0;
                fixed4 color    : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize; // Largura/Altura de um pixel
            float _Size;

            v2f vert(appdata_t IN) {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target {
                float2 res = _MainTex_TexelSize.xy;
                fixed4 col = fixed4(0,0,0,0);
                
                // Amostragem em cruz (9 amostras) para simular o desfoque
                col += tex2D(_MainTex, IN.texcoord + float2(-1, -1) * res * _Size) * 0.05;
                col += tex2D(_MainTex, IN.texcoord + float2(0, -1) * res * _Size) * 0.15;
                col += tex2D(_MainTex, IN.texcoord + float2(1, -1) * res * _Size) * 0.05;
                col += tex2D(_MainTex, IN.texcoord + float2(-1, 0) * res * _Size) * 0.15;
                col += tex2D(_MainTex, IN.texcoord + float2(0, 0) * res * _Size) * 0.2;
                col += tex2D(_MainTex, IN.texcoord + float2(1, 0) * res * _Size) * 0.15;
                col += tex2D(_MainTex, IN.texcoord + float2(-1, 1) * res * _Size) * 0.05;
                col += tex2D(_MainTex, IN.texcoord + float2(0, 1) * res * _Size) * 0.15;
                col += tex2D(_MainTex, IN.texcoord + float2(1, 1) * res * _Size) * 0.05;

                return col * IN.color;
            }
            ENDCG
        }
    }
}