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

            StructuredBuffer<float4x4> _HelixOffsets;
            StructuredBuffer<uint> _HelixIndices;


            v2f vert(appdata_t i, uint instanceID: SV_InstanceID) {
                v2f o;

                uint helixIndex = _HelixIndices[instanceID];
                float4x4 world = mul(_HelixOffsets[helixIndex], _Matrices[instanceID]);
                o.vertex = UnityObjectToClipPos(mul(world, i.vertex));
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
