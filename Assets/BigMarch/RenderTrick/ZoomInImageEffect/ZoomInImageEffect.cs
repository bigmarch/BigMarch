using System;
using UnityEngine;

[ExecuteInEditMode]
public class ZoomInImageEffect : MonoBehaviour
{
	[Range(0, 1)] public float ZoomInFactor = .4f;
	public Vector2 ZoomInCenter = new Vector2(.5f, .4f);

	public Shader ZoomInShader;
	private Material _zoomInMaterial;

	void OnEnable()
	{
		ZoomInShader = Shader.Find("Hidden/ZoomIn");

		_zoomInMaterial = new Material(ZoomInShader);
		_zoomInMaterial.hideFlags = HideFlags.HideAndDontSave;
	}

	// Called by camera to apply image effect
	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_zoomInMaterial == null)
		{
			_zoomInMaterial = new Material(ZoomInShader);
			_zoomInMaterial.hideFlags = HideFlags.HideAndDontSave;
		}

		_zoomInMaterial.SetVector("_ZoomInCenter", ZoomInCenter);
		_zoomInMaterial.SetFloat("_ZoomInFactor", ZoomInFactor);

		Graphics.Blit(source, destination, _zoomInMaterial);
	}
}
