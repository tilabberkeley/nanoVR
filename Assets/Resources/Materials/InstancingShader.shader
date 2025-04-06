Shader "Custom/InstancingShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _HighlightColor ("Highlight Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;

        // Add instancing support for this shader. 
        UNITY_INSTANCING_BUFFER_START(Props)
            // Base color per instance
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            // Highlight color per instance (the intensity is fixed in the shader)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _HighlightColor)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Retrieve base color from texture, tinted by the instance-specific _Color
            fixed4 baseColor = tex2D(_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            
            // Retrieve instance-specific highlight color
            fixed4 highlightColor = UNITY_ACCESS_INSTANCED_PROP(Props, _HighlightColor);
            // Fixed highlight intensity at 0.5
            float highlightIntensity = 0.5;
            
            // Blend the base color with the highlight color using a fixed intensity.
            fixed4 finalColor;
            finalColor.rgb = lerp(baseColor.rgb, highlightColor.rgb, highlightIntensity);
            finalColor.a = baseColor.a;
            
            o.Albedo = finalColor.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = finalColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
