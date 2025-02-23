// Adapted from https://www.shadertoy.com/view/lsdXDH
Shader "CustomEffects/GrayScale"
{
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        // The Blit.hlsl file provides the vertex shader (Vert),
        // the input structure (Attributes), and the output structure (Varyings)
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    float _Factort;

    float4 Process(Varyings input) : SV_Target
    {
        float3 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord).rgb;
        float3 lum = float3(0.299, 0.587, 0.114);
        float gray = dot(lum, color);
        float3 finalColor = lerp(color, gray, _Factort);
        return float4(finalColor, 1.0);
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "Default"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Process

            ENDHLSL
        }
    }
}