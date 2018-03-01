using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BackUpFirstPostEffectSourceRenderTexture : MonoBehaviour
{
	private RenderTexture _colorRt;

	public RenderTexture ColorRt
	{
		get { return _colorRt; }
	}

	private RenderTexture _depthRt;

	public RenderTexture DepthRt
	{
		get { return _depthRt; }
	}

	void OnEnable()
	{
		_colorRt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.Default);
		_colorRt.Create();

		_depthRt = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
		_depthRt.Create();

		Camera cam = GetComponent<Camera>();
		cam.SetTargetBuffers(_colorRt.colorBuffer, _depthRt.depthBuffer);
	}

	void OnDisable()
	{
		Camera cam = GetComponent<Camera>();
		cam.targetTexture = null;

		_colorRt.Release();
		_colorRt = null;

		_depthRt.Release();
		_depthRt = null;
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Graphics.Blit(_colorRt, dest);
	}
}
