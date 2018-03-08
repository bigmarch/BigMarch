using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class OutlineImageEffectMkII : MonoBehaviour
{
	public enum OutlineType
	{
		Always,
		AppearPart0,
		AppearPart1,
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
	public Shader VertexOutlineShader;

	private Material _stretchMaterial;
	private Material _addMaterial;

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
	}

	void OnDisable()
	{
		Destroy(_stretchMaterial);
		Destroy(_addMaterial);
	}

	void OnDestroy()
	{
	}

//	private void Update()
//	{		
//	}

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

		// 在这个地方，根据不同的 outline 类型，对 AimTargetRenderers 的 stencil 操作进行设置。
		switch (CurrentOutlineType)
		{
			case OutlineType.Always:
			case OutlineType.AppearPart0:
				SetStencilProperty(AimTargetRenderers, 0, CompareFunction.Disabled);
				break;
			case OutlineType.AppearPart1:
				SetStencilProperty(AimTargetRenderers, 2, CompareFunction.Always, StencilOp.Replace, StencilOp.Replace);
				break;
			case OutlineType.VertexExpand:
				SetStencilProperty(AimTargetRenderers, 2, CompareFunction.Always, StencilOp.Replace);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
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

		// overlayRt 是目标的剪影照片，这张照片中的目标，不会被遮挡。
		RenderTexture overlayRt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
		overlayRt.filterMode = source.filterMode;

		// culledAbleRt 是目标的剪影照片，这张照片中的目标，可能会被遮挡。
		RenderTexture culledAbleRt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
		culledAbleRt.filterMode = source.filterMode;

		int downWidth = (int) (overlayRt.width * StretchDownSampleMul);
		int downHeight = (int) (overlayRt.height * StretchDownSampleMul);

		// 清空 target rt
		ClearRt(overlayRt);		
		ClearRt(culledAbleRt);		

		// 第一步，设置target的layer，设置成outline。
		SetLayer(AimTargetRenderers, OutlineLayer);

		// 第二步，给outline这个layer拍一张照片。
		_outLineCamera.cullingMask = 1 << OutlineLayer;

		// 根据类型不同，设定不同的 buffer，overlay 的话，不需要 depth rt，因为画在最近处。
		switch (CurrentOutlineType)
		{
			case OutlineType.Always:
				_outLineCamera.SetTargetBuffers(overlayRt.colorBuffer, overlayRt.depthBuffer);
				_outLineCamera.RenderWithShader(OneColorShader, "");
				break;
			case OutlineType.AppearPart0:
				_outLineCamera.SetTargetBuffers(culledAbleRt.colorBuffer, depthRt.depthBuffer);
				_outLineCamera.RenderWithShader(OneColorShader, "");
				break;
			case OutlineType.AppearPart1:
				_outLineCamera.SetTargetBuffers(culledAbleRt.colorBuffer, depthRt.depthBuffer);
				_outLineCamera.RenderWithShader(OneColorShader, "");

				_outLineCamera.SetTargetBuffers(overlayRt.colorBuffer, overlayRt.depthBuffer);
				_outLineCamera.RenderWithShader(OneColorShader, "");
				break;
			case OutlineType.VertexExpand:
				// RenderWithShader 参数只能是 shader，那么如果需要在渲染时，更改 property 怎么办，详见：
				// https://forum.unity.com/threads/set-shader-property-by-renderwithshader-oder-replacement.29747/
				// 结论是，修改 renderer 上的 material 的 property 即可。
				// 另外，对 stencil 相关的 property 进行操作，使用 sharedMaterial 即可，因为这些 property 不会被序列化，所以不会影响“source material”。
				SetStencilProperty(AimTargetRenderers, 2, CompareFunction.NotEqual);
				_outLineCamera.SetTargetBuffers(culledAbleRt.colorBuffer, depthRt.depthBuffer);
				_outLineCamera.RenderWithShader(VertexOutlineShader, "");
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		// 第四步，还原target的layer。
		SetLayer(AimTargetRenderers, _aimTargetCachedLayer);

		if (CurrentOutlineType == OutlineType.Always
		    || CurrentOutlineType == OutlineType.AppearPart0
			|| CurrentOutlineType == OutlineType.AppearPart1)
		{
			RenderTexture buffer0 = RenderTexture.GetTemporary(downWidth, downHeight, 0, RenderTextureFormat.Default);
			buffer0.filterMode = FilterMode.Bilinear;

			RenderTexture buffer1 = RenderTexture.GetTemporary(downWidth, downHeight, 0, RenderTextureFormat.Default);
			buffer1.filterMode = FilterMode.Bilinear;

			// 横纵两个PASS拉伸
			Vector4 screenSize = new Vector4(1f * Width / downWidth, 1f * Width / downHeight, 0.0f, 0.0f);
			_stretchMaterial.SetVector("_ScreenSize", screenSize);

			// 向下采样 + 上下左右拉伸。
			if (CurrentOutlineType == OutlineType.Always)
			{
				Graphics.Blit(overlayRt, buffer0);
			}
			else if (CurrentOutlineType == OutlineType.AppearPart0
			         || CurrentOutlineType == OutlineType.AppearPart1)
			{
				Graphics.Blit(culledAbleRt, buffer0);
			}

			Graphics.Blit(buffer0, buffer1, _stretchMaterial, 0);
			buffer0.DiscardContents();
			Graphics.Blit(buffer1, buffer0, _stretchMaterial, 1);

			// _rt中的白色部分，从stretch后的图中扣掉。
			_addMaterial.SetColor("_Color", CurrentLineColor);
			_addMaterial.SetFloat("_ColorMul", LineColorMul);

			if (CurrentOutlineType == OutlineType.Always
			    || CurrentOutlineType == OutlineType.AppearPart1)
			{
				_addMaterial.SetTexture("_ClipTex0", overlayRt);
			}
			else if (CurrentOutlineType == OutlineType.AppearPart0)
			{
				_addMaterial.SetTexture("_ClipTex0", culledAbleRt);
			}

			_addMaterial.SetTexture("_SourceTex", buffer0);

			Graphics.Blit(source, destination, _addMaterial);

			//		RenderTexture.ReleaseTemporary(clipRt);
			RenderTexture.ReleaseTemporary(buffer0);
			RenderTexture.ReleaseTemporary(buffer1);
			//		_outLineCamera.targetTexture = null;
		}
		else
		{
			_addMaterial.SetColor("_Color", CurrentLineColor);
			_addMaterial.SetFloat("_ColorMul", LineColorMul);
			_addMaterial.SetTexture("_ClipTex0", null);
			_addMaterial.SetTexture("_SourceTex", culledAbleRt);
			Graphics.Blit(source, destination, _addMaterial);
		}

		RenderTexture.ReleaseTemporary(overlayRt);
		RenderTexture.ReleaseTemporary(culledAbleRt);
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

	private static void SetStencilProperty(
		Renderer[] renderers,
		int stencilRef,
		CompareFunction stencilComp,
		StencilOp stencilPass = StencilOp.Keep,
		StencilOp stencilZFail = StencilOp.Keep)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			if (renderers[i] != null)
			{
				renderers[i].sharedMaterial.SetInt("_StencilRef", stencilRef);
				renderers[i].sharedMaterial.SetInt("_StencilComp", (int) stencilComp);
				renderers[i].sharedMaterial.SetInt("_StencilPass", (int) stencilPass);
				renderers[i].sharedMaterial.SetInt("_StencilZFail", (int) stencilZFail);
			}
		}
	}

	private static void ClearRt(RenderTexture rtToClear)
	{
		// 清空 target rt
		RenderTexture rt = RenderTexture.active;
		RenderTexture.active = rtToClear;
		GL.Clear(true, true, Color.black);
		RenderTexture.active = rt;
	}
}
