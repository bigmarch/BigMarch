#pragma warning disable 3009,3005,3001,3002,3003

using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ColorMultiply : MonoBehaviour
{
    public Color Multiplier = Color.white;
    public Texture MaskTexture;
    public Shader BlendShader;

    private Material _blendMaterial;

    void OnEnable()
    {
        BlendShader = Shader.Find("Hidden/BlendTwoTexture");
        _blendMaterial = new Material(BlendShader);
        _blendMaterial.hideFlags = HideFlags.HideAndDontSave;
    }

    void OnDisable()
    {
        if (_blendMaterial)
        {
            DestroyImmediate(_blendMaterial);
            _blendMaterial = null;
        }
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // 如果不使用 blend，就使用 source 当作blend texture。
        _blendMaterial.SetTexture("_BlendTex", src);

        _blendMaterial.EnableKeyword("_BLEND_MASK_TEXTURE");

        _blendMaterial.SetTexture("_Mask", MaskTexture);

        _blendMaterial.SetColor("_BlendColorMul", Multiplier);

        Graphics.Blit(src, dest, _blendMaterial);
    }
}