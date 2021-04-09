Shader "Custom/TerrainShader"
{
    Properties
    {
        _FlatColor("Flat Color", Color) = (0, 0, 0, 0)
        _SteepColor("Steep Color", Color) = (0, 0, 0, 0)
        _SlopeThreshold("Slope Threshold", Range(0, 1)) = 0.5
        _Blend("Blend", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        fixed4 _FlatColor;
        fixed4 _SteepColor;
        half _SlopeThreshold;
        half _Blend;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float slope = 1 - IN.worldNormal.y;
            float flatBlendHeight = _SlopeThreshold * (1 - _Blend);
            float flatWeight = 1 - saturate((slope - flatBlendHeight) / (_SlopeThreshold - flatBlendHeight));
            o.Albedo = _FlatColor * flatWeight + _SteepColor * (1 - flatWeight);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
