using System;
using UnityEngine;

[ExecuteInEditMode]
public class RadialBlurImageEffect : MonoBehaviour
{
	public float ExpandFactor = .4f;
	public Vector2 ExpandCenter = new Vector2(.5f, .4f);

	[Range(0, 1)] public float BlendK01 = 0.5f;

	public Shader BlendShader;
	public Shader RadialBlurShader;
	private Material _blendMaterial;
	private Material _radialBlurMaterial;

	void OnEnable()
	{
		RadialBlurShader = Shader.Find("Hidden/RadialBlurStrech");
		BlendShader = Shader.Find("Hidden/BlendTwoTexture");

		_radialBlurMaterial = new Material(RadialBlurShader);
		_radialBlurMaterial.hideFlags = HideFlags.HideAndDontSave;

		_blendMaterial = new Material(BlendShader);
		_blendMaterial.hideFlags = HideFlags.HideAndDontSave;
		_blendMaterial.EnableKeyword("_BLEND_LERPK");
	}

	// Called by camera to apply image effect
	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		RenderTexture rt = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0);
		rt.filterMode = FilterMode.Bilinear;

		if (_radialBlurMaterial == null)
		{
			_radialBlurMaterial = new Material(RadialBlurShader);
			_radialBlurMaterial.hideFlags = HideFlags.HideAndDontSave;
		}

		_radialBlurMaterial.SetFloat("_ExpandFactor", ExpandFactor);
		_radialBlurMaterial.SetVector("_ExpandCenter", ExpandCenter);

		Graphics.Blit(source, rt, _radialBlurMaterial);

		_blendMaterial.SetTexture("_BlendTex", rt);

		_blendMaterial.SetFloat("_LerpK", BlendK01);

		Graphics.Blit(source, destination, _blendMaterial);

		RenderTexture.ReleaseTemporary(rt);
	}
}
