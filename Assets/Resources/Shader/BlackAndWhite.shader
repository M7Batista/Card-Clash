Shader "UI/BlackAndWhite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
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
            fixed4 _Color;

            v2f vert(appdata_t IN) {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                // Cálculo de Luminância (Padrão ITU-R BT.709)
                float lum = dot(c.rgb, float3(0.2126, 0.7152, 0.0722));
                return fixed4(lum, lum, lum, c.a);
            }
            ENDCG
        }
    }
}