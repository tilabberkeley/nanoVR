Shader "Custom/HelixRingsShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _RingSpacing ("Ring Spacing", Float) = 0.5
        _RingThickness ("Ring Thickness", Float) = 0.07
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        struct Input { float3 worldPos; };

        fixed4 _Color;
        float  _RingSpacing;
        float  _RingThickness;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 worldUpScaled = mul((float3x3)unity_ObjectToWorld, float3(0,1,0));
            float3 axisDir = normalize(worldUpScaled);
            float  heightAlong = dot(IN.worldPos, axisDir);
            float ringPattern = abs( sin(6.28318 * heightAlong / _RingSpacing) );

            float ringMask = step(_RingThickness, ringPattern);
            float3 ringColor = lerp(float3(0,0,0), _Color.rgb, ringMask);
            o.Albedo = ringColor;
            o.Alpha  = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
