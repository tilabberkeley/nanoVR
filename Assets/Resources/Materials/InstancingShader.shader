Shader "Custom/InstancingShader"
{
    SubShader {
        Tags { "RenderType" = "Opaque" }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fragment _  LOD_FADE_CROSSFADE

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
            };

            struct v2f {
                float4 vertex    : SV_POSITION;
                float4  color     : COLOR0;
                float4  highlight : COLOR1;
            };

            StructuredBuffer<float4x4> _Matrices;
            StructuredBuffer<float4> _Colors;
            StructuredBuffer<float4> _Highlights;


            v2f vert(appdata_t i, uint instanceID: SV_InstanceID) {
                v2f o;

                float4 pos = mul(_Matrices[instanceID], i.vertex);
                o.vertex = UnityObjectToClipPos(pos);
                o.color = _Colors[instanceID];
                o.highlight = _Highlights[instanceID];

                return o;
            }

            float4 frag(v2f i) : SV_Target {
                return lerp(i.color, i.highlight, 0.5);
            }

            ENDCG
        }
    }

    FallBack "Diffuse"
}
