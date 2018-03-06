using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OutlineImageEffectMkII : MonoBehaviour
{
	public enum OutlineType
	{
		Always,
		AppearPart,
		AllPartButZTest,
		VertexExpand,
	}

	public OutlineType CurrentOutlineType = OutlineType.VertexExpand;

	[Range(0, 1)] public float StretchDownSampleMul = .6f;

	[Range(1, 5)] public int Width = 1;

	public Color CurrentLineColor = new Color(1, 0, 0, 1);

	[Range(0, 4)] public float LineColorMul = 1f;

	public Renderer[] AimTargetRenderers;
	private int _aimTargetCachedLayer;

	public int OutlineLayer;
	public int OutlineClipLayer;
	public Shader OneColorShader;
	public Shader StretchShader;
	public Shader AddShader;
	public Shader DepthShader;
	public Shader VertexOutlineShader;

	private Material _stretchMaterial;
	private Material _addMaterial;
	private Material _depthMaterial;

	private Camera _mainCamera;
	private Camera _outLineCamera;

	private bool _working;

	private BackUpFirstPostEffectSourceRenderTexture _backUpFirstPostEffectSourceRenderTexture;

	void Awake()
	{
		_mainCamera = GetComponent<Camera>();

		OutlineLayer = LayerMask.NameToLayer("Outline");
		OutlineClipLayer = LayerMask.NameToLayer("OutlineClip");
		OneColorShader = Shader.Find("Hidden/OutlineOneColor");
		StretchShader = Shader.Find("Hidden/OutlineStretch");
		AddShader = Shader.Find("Hidden/OutlineAddToScreen");
		DepthShader = Shader.Find("Hidden/ViewDepthTexture");
		VertexOutlineShader = Shader.Find("Hidden/VertexOutline");

		_backUpFirstPostEffectSourceRenderTexture = GetComponent<BackUpFirstPostEffectSourceRenderTexture>();
		if (!_backUpFirstPostEffectSourceRenderTexture)
		{
			Debug.LogError("can not find this component on camera : BackUpFirstPostEffectSourceRenderTexture" +
			               "\n outline will not works.");
		}
	}

	void OnEnable()
	{
		if (!_outLineCamera)
		{
			GameObject go = new GameObject("OutLintCamera");
			go.transform.parent = transform;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;

			_outLineCamera = go.AddComponent<Camera>();
			_outLineCamera.cullingMask = 1 << OutlineLayer;
			_outLineCamera.clearFlags = CameraClearFlags.Nothing;
			_outLineCamera.backgroundColor = Color.black;
			_outLineCamera.enabled = false;
		}

		_stretchMaterial = new Material(StretchShader);
		_addMaterial = new Material(AddShader);
		_depthMaterial = new Material(DepthShader);
	}

	void OnDisable()
	{
		Destroy(_stretchMaterial);
		Destroy(_addMaterial);
		Destroy(_depthMaterial);
	}

	void OnDestroy()
	{
	}

	private void Update()
	{
	}

	void OnPreRender()
	{
		if (!_working)
		{
			return;
		}

		//完全复制主摄像机上的参数。
		_outLineCamera.fieldOfView = _mainCamera.fieldOfView;
		_outLineCamera.nearClipPlane = _mainCamera.nearClipPlane;
		_outLineCamera.farClipPlane = _mainCamera.farClipPlane;
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!_backUpFirstPostEffectSourceRenderTexture
		    || !_backUpFirstPostEffectSourceRenderTexture.DepthRt
		    || !_working
		    || AimTargetRenderers == null)
		{
			Graphics.Blit(source, destination);
			return;
		}

		RenderTexture depthRt = _backUpFirstPostEffectSourceRenderTexture.DepthRt;

		RenderTexture blackRt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
		blackRt.filterMode = source.filterMode;

		int downWidth = (int) (blackRt.width * StretchDownSampleMul);
		int downHeight = (int) (blackRt.height * StretchDownSampleMul);

		// 清空 target rt
		RenderTexture rt = RenderTexture.active;
		RenderTexture.active = blackRt;
		GL.Clear(false, true, Color.black);
		RenderTexture.active = rt;

		// 第一步，设置target的layer，设置成outline。
		SetLayer(AimTargetRenderers, OutlineLayer);

		// 第二步，给outline这个layer拍一张照片。
		_outLineCamera.cullingMask = 1 << OutlineLayer;

		// 根据类型不同，设定不同的 buffer，overlay 的话，不需要 depth rt，因为画在最近处。
		switch (CurrentOutlineType)
		{
			case OutlineType.Always:
				_outLineCamera.SetTargetBuffers(blackRt.colorBuffer, blackRt.depthBuffer);
				_outLineCamera.RenderWithShader(OneColorShader, "");
				break;
			case OutlineType.AppearPart:
				_outLineCamera.SetTargetBuffers(blackRt.colorBuffer, depthRt.depthBuffer);
				_outLineCamera.RenderWithShader(OneColorShader, "");
				break;
			case OutlineType.AllPartButZTest:
				_outLineCamera.SetTargetBuffers(blackRt.colorBuffer, blackRt.depthBuffer);
				_outLineCamera.RenderWithShader(OneColorShader, "");
				break;
			case OutlineType.VertexExpand:
				_outLineCamera.SetTargetBuffers(blackRt.colorBuffer, depthRt.depthBuffer);
				_outLineCamera.RenderWithShader(VertexOutlineShader, "");
				Shader.SetGlobalInt("_OutlineTankStencil", 2);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		// 第四步，还原target的layer。
		SetLayer(AimTargetRenderers, _aimTargetCachedLayer);

		if (CurrentOutlineType == OutlineType.Always
		    || CurrentOutlineType == OutlineType.AppearPart)
		{
			RenderTexture buffer0 = RenderTexture.GetTemporary(downWidth, downHeight, 0, RenderTextureFormat.Default);
			buffer0.filterMode = FilterMode.Bilinear;

			RenderTexture buffer1 = RenderTexture.GetTemporary(downWidth, downHeight, 0, RenderTextureFormat.Default);
			buffer1.filterMode = FilterMode.Bilinear;

			// 横纵两个PASS拉伸
			Vector4 screenSize = new Vector4(1f * Width / downWidth, 1f * Width / downHeight, 0.0f, 0.0f);
			_stretchMaterial.SetVector("_ScreenSize", screenSize);

			// 向下采样 + 上下左右拉伸。
			Graphics.Blit(blackRt, buffer0);
			Graphics.Blit(buffer0, buffer1, _stretchMaterial, 0);
			buffer0.DiscardContents();
			Graphics.Blit(buffer1, buffer0, _stretchMaterial, 1);

			// _rt中的白色部分，从stretch后的图中扣掉。
			_addMaterial.SetColor("_Color", CurrentLineColor);
			_addMaterial.SetFloat("_ColorMul", LineColorMul);
			_addMaterial.SetTexture("_ClipTex0", blackRt);
			//		_addMaterial.SetTexture("_ClipTex1", clipRt);

			_addMaterial.SetTexture("_SourceTex", buffer0);

			Graphics.Blit(source, destination, _addMaterial);

			//		RenderTexture.ReleaseTemporary(clipRt);
			RenderTexture.ReleaseTemporary(buffer0);
			RenderTexture.ReleaseTemporary(buffer1);
			//		_outLineCamera.targetTexture = null;
		}
		else if (CurrentOutlineType == OutlineType.AllPartButZTest)
		{			
			_mainCamera.depthTextureMode = DepthTextureMode.Depth;
			Graphics.Blit(depthRt, destination, _depthMaterial);
		}
		else
		{
			_addMaterial.SetColor("_Color", CurrentLineColor);
			_addMaterial.SetFloat("_ColorMul", LineColorMul);
			_addMaterial.SetTexture("_ClipTex0", null);
			_addMaterial.SetTexture("_SourceTex", blackRt);
			Graphics.Blit(source, destination, _addMaterial);
		}

		RenderTexture.ReleaseTemporary(blackRt);
	}

	// direction 的含义： 0-前，1-侧面，2-后，
	public void SetTarget(Renderer[] targetRenderer)
	{
		if (targetRenderer == null || targetRenderer.Length == 0 || targetRenderer[0] == null)
		{
			_working = false;
			return;
		}

		//记录目标中所有的renderer以及对应的layer。	
		AimTargetRenderers = targetRenderer;

		_aimTargetCachedLayer = targetRenderer[0].gameObject.layer;

		_working = true;
	}

	public void ClearTarget()
	{
		AimTargetRenderers = null;
		_working = false;
		enabled = false;
	}

	private static void SetLayer(Renderer[] renderers, int layer)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			if (renderers[i] != null)
			{
				renderers[i].gameObject.layer = layer;
			}
		}
	}
}
