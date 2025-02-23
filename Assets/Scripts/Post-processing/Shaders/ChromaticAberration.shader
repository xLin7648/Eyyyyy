// Adapted from https://godotshaders.com/shader/chromatic-abberation/
Shader "CustomEffects/ChromaticAberration"
{
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        // The Blit.hlsl file provides the vertex shader (Vert),
        // the input structure (Attributes), and the output structure (Varyings)
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    float _SampleCount;
    float _Power;

    float4 Process(Varyings input) : SV_Target
    {
        float3 sum = float3(0.0, 0.0, 0.0);
        float3 c = float3(0.0, 0.0, 0.0);
        float2 offset = (input.texcoord - float2(0.5, 0.5)) * float2(1, -1);
        int sample_count = int(_SampleCount);

        for (int i = 0; i < 4; ++i)
        {
            if (i >= sample_count) break;
            float t = 2.0 * float(i) / float(sample_count - 1); // range 0.0->2.0
            float3 slice = float3(1.0 - t, 1.0 - abs(t - 1.0), t - 1.0);
            slice = max(slice, 0.0);
            sum += slice;
            float2 slice_offset = (t - 1.0) * _Power * offset;

            c += slice * SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord + slice_offset).rgb;
        }

        return float4(c / sum, 1.0);
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