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
        // �˼��ȷ�����ὫЧ����Ⱦ������̽���Ԥ���������Ϊͨ������Ҫ����Щ�ط����к���
        if (renderingData.cameraData.cameraType == CameraType.Preview || renderingData.cameraData.cameraType == CameraType.Reflection)
            return;

        var efInstance = EfManager.Instance;
        if (EfManager.Instance != null)
        {
            var m_passs = efInstance.ConsumePasss();

            if (m_passs == null) return;

            // �������е����г�Աȡ������ӵ��б���
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
    // ������Ⱦ����Ч���Ĳ���
    private Material m_Material;
    private Material m_defaultBlitMaterial;

    // ����Pass������
    private readonly string k_PassName;

    // ģ�������������
    private RenderTextureDescriptor blurTextureDescriptor;

    // �洢��ͬ���͵�uniformֵ
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

    // ��������
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

    // ��¼ Render Graph ����Ⱦ Pass
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        if (resourceData.isActiveTargetBackBuffer)
            return;

        blurTextureDescriptor = cameraData.cameraTargetDescriptor;
        blurTextureDescriptor.depthBufferBits = 0;

        // ������ʱ����
        var tempTextureDesc = new TextureDesc(blurTextureDescriptor)
        {
            name = "EFPostProcessTemp",
            clearBuffer = true,
            clearColor = Color.clear
        };
        var tempTexture = renderGraph.CreateTexture(tempTextureDesc);

        var srcCamColor = resourceData.activeColorTexture;

        UpdateUniforms();

        // ��һ����Ӧ�ò���Ч��
        var effectParams = new RenderGraphUtils.BlitMaterialParameters(
            srcCamColor,
            tempTexture,
            m_Material,
            0
        );
        renderGraph.AddBlitPass(effectParams, k_PassName);

        // �ڶ�����ʹ��URP���ò��ʻؿ�
        var copyBackParams = new RenderGraphUtils.BlitMaterialParameters(
            tempTexture,
            srcCamColor,
            m_defaultBlitMaterial,
            0
        );
        renderGraph.AddBlitPass(copyBackParams, "CopyBackToCameraColor");
    }
}