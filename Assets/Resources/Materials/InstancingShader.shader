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
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 vertex    : SV_POSITION;
                float4  color     : COLOR0;
                float4  highlight : COLOR1;
                float3 normal : TEXCOORD0;
            };

            StructuredBuffer<float4x4> _Matrices;
            StructuredBuffer<float4> _Colors;
            StructuredBuffer<float4> _Highlights;

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID) {
                v2f o;
                float4x4 modelMatrix = _Matrices[instanceID];
                float3 worldNormal = normalize(mul((float3x3)modelMatrix, i.normal)); // rotate normal
                o.vertex = UnityObjectToClipPos(mul(modelMatrix, i.vertex));
                o.color = _Colors[instanceID];
                o.highlight = _Highlights[instanceID];
                o.normal = worldNormal;
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                float3 lightDir = normalize(float3(0.3, 0.7, 0.5)); // fake directional light
                float diff = saturate(dot(i.normal, lightDir)) * 0.5 + 0.5; // keep it soft
                float4 baseColor = lerp(i.color, i.highlight, 0.5);
                return baseColor * diff;
            }

            ENDCG
        }
    }

    FallBack "Diffuse"
}
