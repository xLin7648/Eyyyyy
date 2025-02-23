using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using System.Collections.Generic;

public sealed class EF : ScriptableRendererFeature
{
    public override void Create() { }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // 此检查确保不会将效果渲染到反射探针或预览相机，因为通常不需要在这些地方进行后处理
        if (renderingData.cameraData.cameraType == CameraType.Preview || renderingData.cameraData.cameraType == CameraType.Reflection)
            return;

        var efInstance = EfManager.Instance;
        if (EfManager.Instance != null)
        {
            var m_passs = efInstance.ConsumePasss();

            if (m_passs == null) return;

            // 将队列中的所有成员取出并添加到列表中
            while (m_passs.Count > 0)
            {
                var pass = m_passs.Dequeue();

                if (pass == null) continue;

                pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
                pass.ConfigureInput(ScriptableRenderPassInput.Color);

                renderer.EnqueuePass(pass);
            }

            m_passs.Clear();
        }
    }
}

public sealed class EFPostRenderPass : ScriptableRenderPass
{
    // 用于渲染后处理效果的材质
    private Material m_Material;
    private Material m_defaultBlitMaterial;

    // 定义Pass的名称
    private readonly string k_PassName;

    // 模糊纹理的描述符
    private RenderTextureDescriptor blurTextureDescriptor;

    // 存储不同类型的uniform值
    private static Dictionary<string, float> floatUniforms = new Dictionary<string, float>();
    private static Dictionary<string, Vector4> vectorUniforms = new Dictionary<string, Vector4>();

    public EFPostRenderPass(string passName, Material material, Material defaultBlitMaterial)
    {
        if (material == null)
        {
            throw new System.ArgumentNullException(nameof(material));
        }

        k_PassName = passName;
        m_Material = material;
        m_defaultBlitMaterial = defaultBlitMaterial;

        profilingSampler = new ProfilingSampler(passName);
        blurTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
    }

    public void RecordUniform<T>(string name, T value)
    {
        switch (value)
        {
            case int i:
                floatUniforms[name] = i;
                break;
            case float f:
                floatUniforms[name] = f;
                break;
            case Vector2 v2:
                vectorUniforms[name] = v2;
                break;
            case Vector3 v3:
                vectorUniforms[name] = v3;
                break;
            case Vector4 v4:
                vectorUniforms[name] = v4;
                break;
            default:
                Debug.LogError("Unsupported uniform type: " + typeof(T));
                break;
        }
    }

    // 更新设置
    private void UpdateUniforms()
    {
        if (m_Material == null) return;

        foreach (var value in floatUniforms)
        {
            m_Material.SetFloat(value.Key, value.Value);
        }

        foreach (var value in vectorUniforms)
        {
            m_Material.SetVector(value.Key, value.Value);
        }
    }

    // 记录 Render Graph 的渲染 Pass
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        if (resourceData.isActiveTargetBackBuffer)
            return;

        blurTextureDescriptor = cameraData.cameraTargetDescriptor;
        blurTextureDescriptor.depthBufferBits = 0;

        // 创建临时纹理
        var tempTextureDesc = new TextureDesc(blurTextureDescriptor)
        {
            name = "EFPostProcessTemp",
            clearBuffer = true,
            clearColor = Color.clear
        };
        var tempTexture = renderGraph.CreateTexture(tempTextureDesc);

        var srcCamColor = resourceData.activeColorTexture;

        UpdateUniforms();

        // 第一步：应用材质效果
        var effectParams = new RenderGraphUtils.BlitMaterialParameters(
            srcCamColor,
            tempTexture,
            m_Material,
            0
        );
        renderGraph.AddBlitPass(effectParams, k_PassName);

        // 第二步：使用URP内置材质回拷
        var copyBackParams = new RenderGraphUtils.BlitMaterialParameters(
            tempTexture,
            srcCamColor,
            m_defaultBlitMaterial,
            0
        );
        renderGraph.AddBlitPass(copyBackParams, "CopyBackToCameraColor");
    }
}