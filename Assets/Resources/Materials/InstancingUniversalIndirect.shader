Shader "Custom/IndirectUniversal"
{
    Properties
    {
        _FallbackColor ("Fallback Color", Color) = (0.6, 0.6, 0.6, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"

            struct InstanceData
            {
                float4x4 modelMatrix;
                float4 color;
            };

            StructuredBuffer<InstanceData> instanceBuffer;
            float4 _FallbackColor;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 col : COLOR;
            };

            v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                InstanceData data = instanceBuffer[instanceID];

                float4 worldPos = mul(data.modelMatrix, v.vertex);
                o.pos = UnityObjectToClipPos(worldPos);
                o.col = data.color;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return i.col;
            }
            ENDCG
        }
    }
}
