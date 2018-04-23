using UnityEngine;

[ExecuteInEditMode]
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
		Refresh();
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

	void Update()
	{
#if UNITY_EDITOR
		if (_colorRt.width != Screen.width
		    || _colorRt.height != Screen.height
		    || _depthRt.width != Screen.width
		    || _depthRt.height != Screen.height)
		{
			Refresh();
		}
#endif
	}

	private void Refresh()
	{
		Debug.Log("Refresh BackUpFirstPostEffectSourceRenderTexture : " + Screen.width + "   " + Screen.height);

		RenderTexture newColorRt = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.Default);
		newColorRt.filterMode = FilterMode.Point;
		newColorRt.Create();
		newColorRt.hideFlags = HideFlags.DontSave;

		RenderTexture newDepthRt = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
		newColorRt.filterMode = FilterMode.Point;
		newDepthRt.Create();
		newDepthRt.hideFlags = HideFlags.DontSave;

		Camera cam = GetComponent<Camera>();

		cam.SetTargetBuffers(newColorRt.colorBuffer, newDepthRt.depthBuffer);

		if (_colorRt)
		{
			_colorRt.Release();
		}
		_colorRt = newColorRt;
		if (_depthRt)
		{
			_depthRt.Release();
		}
		_depthRt = newDepthRt;
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Graphics.Blit(_colorRt, dest);
	}
}
