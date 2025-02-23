using UnityEngine;
using System.Collections.Generic;

public class EfManager : MonoSingleton<EfManager>
{
    public Shader[] shaders;
    public Material DefaultBlitMaterial;
    public string shaderPath = "CustomEffects/";

    [Space(20)]
    [Range(1, 64)]
    public int _SampleCount;

    public float _Power;

    [Range(0f, 1f)]
    public float _Factort;

    private Dictionary<string, EFPostRenderPass> passs;
    private Queue<EFPostRenderPass> eFPostRenders;

    public Queue<EFPostRenderPass> ConsumePasss() => eFPostRenders;

    private void Start()
    {
        eFPostRenders = new Queue<EFPostRenderPass>();
        passs = new Dictionary<string, EFPostRenderPass>();

        foreach (var e in shaders)
        {
            var shaderName = e.name;

            // ÅÐ¶ÏÖ÷×Ö·û´®ÊÇ·ñ°üº¬×Ó×Ö·û´®
            if (!shaderName.Contains(shaderPath)) continue;

            // Èç¹û°üº¬£¬ÔòÌÞ³ý×Ó×Ö·û´®
            shaderName = shaderName.Replace(shaderPath, string.Empty);

            var newPass = new EFPostRenderPass(shaderName, new Material(e), DefaultBlitMaterial);

            passs.Add(shaderName, newPass);
        }
    }

    private void Update()
    {
        if (passs.TryGetValue("ChromaticAberration", out var k))
        {
            k.RecordUniform("_SampleCount", _SampleCount);
            k.RecordUniform("_Power", _Power);

            eFPostRenders.Enqueue(k);
        }

        if (passs.TryGetValue("GrayScale", out var g))
        {
            g.RecordUniform("_Factort", _Factort);

            eFPostRenders.Enqueue(g);
        }
    }
}