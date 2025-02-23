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
        // 采样屏幕颜色
        float3 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord).rgb;

        // 灰度化权重
        float3 lum = float3(0.299, 0.587, 0.114);

        // 计算灰度值
        float gray = dot(lum, color);

        // 混合原始颜色和灰度颜色
        float3 finalColor = lerp(color, gray, _Factort);

        // 返回最终颜色
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