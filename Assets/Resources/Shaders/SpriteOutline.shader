Shader "UI/SpriteOutlineSoft"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineSize ("Outline Size", Range(0,10)) = 1
        _Softness ("Outline Softness", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineSize;
            float _Softness;

            v2f vert(appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 c = tex2D(_MainTex, i.uv) * _Color;

                // intensidade do contorno
                float outline = 0.0;

                // verifica pixels vizinhos
                for (int x = -1; x <= 1; x++) {
                    for (int y = -1; y <= 1; y++) {
                        float2 offset = float2(x, y) * _OutlineSize * _MainTex_TexelSize.xy;
                        float alpha = tex2D(_MainTex, i.uv + offset).a;
                        outline = max(outline, alpha);
                    }
                }

                // se não tem sprite aqui, mas vizinhos tem → desenha borda
                if (c.a <= 0.01 && outline > 0.0) {
                    // suavização do contorno
                    float alpha = smoothstep(0.0, _Softness, outline);
                    return fixed4(_OutlineColor.rgb, alpha * _OutlineColor.a);
                }

                return c;
            }
            ENDCG
        }
    }
}
