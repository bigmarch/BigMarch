using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class OutlineImageEffectMkIII : MonoBehaviour
{
	public enum OutlineType
	{
		StretchAll,
		StretchPartClosed,
		StretchPart,
		VertexExpandAll,
		VertexExpandPart,
		ZoomInAll,
		ZoomInPart,
	}

	public OutlineType CurrentOutlineType = OutlineType.VertexExpandPart;

	[Range(0, 1)] public float StretchDownSampleMul = .6f;

	[Range(1, 5)] public int StretchWidth = 1;
	[Range(0, 5)] public float VertexExpandWidth = 0.8f;

	[Range(0, 1)] public float ZoomInFactor = 0.95f;

	public Color CurrentLineColor = new Color(1, 0, 0, 1);

	[Range(0, 4)] public float LineColorMul = 1f;

	public Renderer[] AimTargetRenderers;
	private int _aimTargetCachedLayer;

	public int OutlineLayer;
	public Shader OneColorShader;
	public Shader StretchShader;
	public Shader AddShader;
	public Shader VertexOutlineShader;
	public Shader ZoomInShader;

	private Material _stretchMaterial;
	private Material _zoomInMaterial;
	private Material _addMaterial;

	private Camera _mainCamera;
	private Camera _outLineCamera;

	private bool _working;

	private BackUpFirstPostEffectSourceRenderTexture _backUpFirstPostEffectSourceRenderTexture;

	void Awake()
	{
		_mainCamera = GetComponent<Camera>();

		OutlineLayer = LayerMask.NameToLayer("Outline");
		OneColorShader = Shader.Find("Hidden/OutlineOneColor");
		StretchShader = Shader.Find("Hidden/OutlineStretch");
		AddShader = Shader.Find("Hidden/OutlineAddToScreen");
		VertexOutlineShader = Shader.Find("Hidden/VertexOutline");
		ZoomInShader = Shader.Find("Hidden/ZoomIn");

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
		_zoomInMaterial = new Material(ZoomInShader);
	}

	void OnDisable()
	{
		Destroy(_stretchMaterial);
		Destroy(_addMaterial);
		Destroy(_zoomInMaterial);
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

		// 在这个地方，根据不同的 outline 类型，对 AimTargetRenderers 的 stencil 操作进行设置。
		// PreRender 里设置好之后，摄像机的正常渲染会把 stencil 值写入到 buffer 中。
		switch (CurrentOutlineType)
		{
			case OutlineType.StretchAll:
			case OutlineType.StretchPartClosed:
			case OutlineType.StretchPart:
			case OutlineType.ZoomInAll:
			case OutlineType.ZoomInPart:
				SetStencilProperty(AimTargetRenderers, 0, CompareFunction.Disabled);
				break;
			case OutlineType.VertexExpandPart:
			case OutlineType.VertexExpandAll:
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

		switch (CurrentOutlineType)
		{
			case OutlineType.StretchAll:
			case OutlineType.StretchPartClosed:
			case OutlineType.StretchPart:
				OnRenderImageForStretch(source, destination);
				break;
			case OutlineType.VertexExpandPart:
			case OutlineType.VertexExpandAll:
				OnRenderImageForVertexExpand(source, destination);
				break;
			case OutlineType.ZoomInAll:
			case OutlineType.ZoomInPart:
				OnRenderImageForZoomIn(source, destination);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void OnRenderImageForStretch(RenderTexture source, RenderTexture destination)
	{
		Debug.Assert(CurrentOutlineType == OutlineType.StretchAll
		             || CurrentOutlineType == OutlineType.StretchPart
		             || CurrentOutlineType == OutlineType.StretchPartClosed);

		// 得到深度图
		RenderTexture depthRt = _backUpFirstPostEffectSourceRenderTexture.DepthRt;

		// overlayRt 是目标的剪影照片，这张照片中的目标，不会被遮挡。
		RenderTexture overlayRt = null;

		// culledAbleRt 是目标的剪影照片，这张照片中的目标，可能会被遮挡。
		RenderTexture culledAbleRt = null;

		// 用于 stretch 的 rt
		RenderTexture rtToStretch;
		
		// 用于 clip 的 rt
		RenderTexture rtForClip;

		// 第一步，设置target的layer，设置成outline。
		SetLayer(AimTargetRenderers, OutlineLayer);

		// 第二步，给outline这个layer拍一张照片。
		_outLineCamera.cullingMask = 1 << OutlineLayer;

		// 根据类型不同，设定不同的 buffer，overlay 的话，不需要 depth rt，因为画在最近处。
		switch (CurrentOutlineType)
		{
			case OutlineType.StretchAll:
			{
				// 生成 overlay Rt，并对其进行渲染。
				overlayRt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
				overlayRt.filterMode = source.filterMode;
				ClearRt(overlayRt);

				_outLineCamera.SetTargetBuffers(overlayRt.colorBuffer, overlayRt.depthBuffer);
				_outLineCamera.RenderWithShader(OneColorShader, "");

				rtToStretch = overlayRt;
				rtForClip = overlayRt;
				break;
			}
			case OutlineType.StretchPartClosed:
			{
				// 生成 culled Able Rt，并对其进行渲染。
				culledAbleRt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
				culledAbleRt.filterMode = source.filterMode;
				ClearRt(culledAbleRt);

				_outLineCamera.SetTargetBuffers(culledAbleRt.colorBuffer, depthRt.depthBuffer);
				_outLineCamera.RenderWithShader(OneColorShader, "");

				rtToStretch = culledAbleRt;
				rtForClip = culledAbleRt;

				break;
			}
			case OutlineType.StretchPart:
			{
				// 生成 culled Able Rt，并对其进行渲染。
				culledAbleRt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
				culledAbleRt.filterMode = source.filterMode;
				ClearRt(culledAbleRt);

				_outLineCamera.SetTargetBuffers(culledAbleRt.colorBuffer, depthRt.depthBuffer);
				_outLineCamera.RenderWithShader(OneColorShader, "");

				// 生成 overlay Rt，并对其进行渲染。
				overlayRt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
				overlayRt.filterMode = source.filterMode;
				ClearRt(overlayRt);

				_outLineCamera.SetTargetBuffers(overlayRt.colorBuffer, overlayRt.depthBuffer);
				_outLineCamera.RenderWithShader(OneColorShader, "");

				// 在这个模式下，用于 stretch 的是 culledAbleRt，overlayRt 是用于 clip 的。
				rtToStretch = culledAbleRt;
				rtForClip = overlayRt;

				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
		}

		// 第四步，还原target的layer。
		SetLayer(AimTargetRenderers, _aimTargetCachedLayer);

		// 第五步，开始 stretch。
		int downWidth = (int) (source.width * StretchDownSampleMul);
		int downHeight = (int) (source.height * StretchDownSampleMul);

		RenderTexture buffer0 = RenderTexture.GetTemporary(downWidth, downHeight, 0, RenderTextureFormat.Default);
		buffer0.filterMode = FilterMode.Bilinear;

		RenderTexture buffer1 = RenderTexture.GetTemporary(downWidth, downHeight, 0, RenderTextureFormat.Default);
		buffer1.filterMode = FilterMode.Bilinear;

		// 横纵两个PASS拉伸
		Vector4 screenSize = new Vector4(1f * StretchWidth / downWidth, 1f * StretchWidth / downHeight, 0.0f, 0.0f);
		_stretchMaterial.SetVector("_ScreenSize", screenSize);

		// 向下采样 
		Graphics.Blit(rtToStretch, buffer0);

		// 上下左右拉伸。
		Graphics.Blit(buffer0, buffer1, _stretchMaterial, 0);
		buffer0.DiscardContents();
		Graphics.Blit(buffer1, buffer0, _stretchMaterial, 1);

		// _rt中的白色部分，从stretch后的图中扣掉。
		_addMaterial.SetColor("_Color", CurrentLineColor);
		_addMaterial.SetFloat("_ColorMul", LineColorMul);

		_addMaterial.SetTexture("_ClipTex0", rtForClip);		

		_addMaterial.SetTexture("_SourceTex", buffer0);

		Graphics.Blit(source, destination, _addMaterial);

		RenderTexture.ReleaseTemporary(buffer0);
		RenderTexture.ReleaseTemporary(buffer1);

		if (overlayRt != null)
		{
			RenderTexture.ReleaseTemporary(overlayRt);
		}

		if (culledAbleRt != null)
		{
			RenderTexture.ReleaseTemporary(culledAbleRt);
		}
	}

	private void OnRenderImageForZoomIn(RenderTexture source, RenderTexture destination)
	{
		Debug.Assert(CurrentOutlineType == OutlineType.ZoomInAll
		             || CurrentOutlineType == OutlineType.ZoomInPart);

		// 得到深度图
		RenderTexture depthRt = _backUpFirstPostEffectSourceRenderTexture.DepthRt;

		// overlayRt 是目标的剪影照片，这张照片中的目标，不会被遮挡。
		RenderTexture overlayRt = null;

		// culledAbleRt 是目标的剪影照片，这张照片中的目标，可能会被遮挡。
		RenderTexture culledAbleRt = null;

		// 用于 stretch 的 rt
		RenderTexture rtToZoom;	
		
		// 用于 clip 的 rt
		RenderTexture rtForClip;		

		// 第一步，设置target的layer，设置成outline。
		SetLayer(AimTargetRenderers, OutlineLayer);

		// 第二步，给outline这个layer拍一张照片。
		_outLineCamera.cullingMask = 1 << OutlineLayer;

		// 根据类型不同，设定不同的 buffer，overlay 的话，不需要 depth rt，因为画在最近处。
		switch (CurrentOutlineType)
		{
			case OutlineType.ZoomInAll:
			{
				// 生成 overlay Rt，并对其进行渲染。
				overlayRt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
				overlayRt.filterMode = source.filterMode;
				ClearRt(overlayRt);

				_outLineCamera.SetTargetBuffers(overlayRt.colorBuffer, overlayRt.depthBuffer);
				_outLineCamera.RenderWithShader(OneColorShader, "");

				rtToZoom = overlayRt;
				rtForClip = overlayRt;
				break;
			}
			case OutlineType.ZoomInPart:
			{
				// 生成 culled Able Rt，并对其进行渲染。
				culledAbleRt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
				culledAbleRt.filterMode = source.filterMode;
				ClearRt(culledAbleRt);

				_outLineCamera.SetTargetBuffers(culledAbleRt.colorBuffer, depthRt.depthBuffer);
				_outLineCamera.RenderWithShader(OneColorShader, "");

				rtToZoom = culledAbleRt;
				rtForClip = culledAbleRt;

				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
		}

		// 第四步，还原target的layer。
		SetLayer(AimTargetRenderers, _aimTargetCachedLayer);

		// 第五步，开始 stretch。
		int downWidth = (int) (source.width * StretchDownSampleMul);
		int downHeight = (int) (source.height * StretchDownSampleMul);
		
		RenderTexture buffer0 = RenderTexture.GetTemporary(downWidth, downHeight, 0, RenderTextureFormat.Default);
		buffer0.filterMode = FilterMode.Bilinear;

		RenderTexture buffer1 = RenderTexture.GetTemporary(downWidth, downHeight, 0, RenderTextureFormat.Default);
		buffer1.filterMode = FilterMode.Bilinear;

		// 向下采样 + 定点放大，一次 blit 就完成了。
		Graphics.Blit(rtToZoom, buffer0);

		// 求所有 renderer 的中心点
		Bounds b = AimTargetRenderers[0].bounds;
		for (int i = 0; i < AimTargetRenderers.Length; i++)
		{
			b.Encapsulate(AimTargetRenderers[i].bounds);
		}

		Vector3 viewPos = _mainCamera.WorldToViewportPoint(b.center);
		_zoomInMaterial.SetVector("_ZoomInCenter", viewPos);
		_zoomInMaterial.SetFloat("_ZoomInFactor", ZoomInFactor);

		// 放大
		Graphics.Blit(buffer0, buffer1, _zoomInMaterial);

		// 最后的叠加
		_addMaterial.SetColor("_Color", CurrentLineColor);
		_addMaterial.SetFloat("_ColorMul", LineColorMul);

		_addMaterial.SetTexture("_ClipTex0", rtForClip);

		_addMaterial.SetTexture("_SourceTex", buffer1);

		Graphics.Blit(source, destination, _addMaterial);

		RenderTexture.ReleaseTemporary(buffer0);
		RenderTexture.ReleaseTemporary(buffer1);
		
		if (overlayRt != null)
		{
			RenderTexture.ReleaseTemporary(overlayRt);
		}

		if (culledAbleRt != null)
		{
			RenderTexture.ReleaseTemporary(culledAbleRt);
		}
	}

	private void OnRenderImageForVertexExpand(RenderTexture source, RenderTexture destination)
	{		
		Debug.Assert(CurrentOutlineType == OutlineType.VertexExpandAll
		             || CurrentOutlineType == OutlineType.VertexExpandAll);
		
		RenderTexture depthRt = _backUpFirstPostEffectSourceRenderTexture.DepthRt;

		// overlayRt 是目标的剪影照片，这张照片中的目标，不会被遮挡。
		RenderTexture overlayRt = null;

		// culledAbleRt 是目标的剪影照片，这张照片中的目标，可能会被遮挡。
		RenderTexture culledAbleRt = null;

		// 用于 stretch 的 rt
		RenderTexture rtResult;	

		// 第一步，设置target的layer，设置成outline。
		SetLayer(AimTargetRenderers, OutlineLayer);

		// 第二步，给outline这个layer拍一张照片。
		_outLineCamera.cullingMask = 1 << OutlineLayer;

		// RenderWithShader 参数只能是 shader，那么如果需要在渲染时，更改 property 怎么办，详见：
		// https://forum.unity.com/threads/set-shader-property-by-renderwithshader-oder-replacement.29747/
		// 结论是，修改 renderer 上的 material 的 property 即可。
		// 另外，对 stencil 相关的 property 进行操作，使用 sharedMaterial 即可，因为这些 property 不会被序列化，所以不会影响“source material”。
		SetStencilProperty(AimTargetRenderers, 2, CompareFunction.NotEqual);
		SetFloatProperty(AimTargetRenderers, "_Outline", VertexExpandWidth);

		// 根据类型不同，设定不同的 buffer，overlay 的话，不需要 depth rt，因为画在最近处。
		switch (CurrentOutlineType)
		{
			case OutlineType.VertexExpandAll:
			{
				// 生成 overlay Rt，并对其进行渲染。
				overlayRt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
				overlayRt.filterMode = source.filterMode;
				ClearRt(overlayRt);

				_outLineCamera.SetTargetBuffers(overlayRt.colorBuffer, overlayRt.depthBuffer);
				_outLineCamera.RenderWithShader(VertexOutlineShader, "");

				rtResult = overlayRt;

				break;
			}
			case OutlineType.VertexExpandPart:
			{
				// 生成 culled Able Rt，并对其进行渲染。
				culledAbleRt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
				culledAbleRt.filterMode = source.filterMode;
				ClearRt(culledAbleRt);

				_outLineCamera.SetTargetBuffers(culledAbleRt.colorBuffer, depthRt.depthBuffer);
				_outLineCamera.RenderWithShader(VertexOutlineShader, "");

				rtResult = culledAbleRt;	
				
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
		}

		// 第四步，还原target的layer。
		SetLayer(AimTargetRenderers, _aimTargetCachedLayer);

		_addMaterial.SetColor("_Color", CurrentLineColor);
		_addMaterial.SetFloat("_ColorMul", LineColorMul);
		_addMaterial.SetTexture("_ClipTex0", null);
		_addMaterial.SetTexture("_SourceTex", rtResult);

		Graphics.Blit(source, destination, _addMaterial);

		if (overlayRt != null)
		{
			RenderTexture.ReleaseTemporary(overlayRt);
		}

		if (culledAbleRt != null)
		{
			RenderTexture.ReleaseTemporary(culledAbleRt);
		}
	}

	public void SetTargetRenderer(Renderer[] targetRenderer)
	{
		if (targetRenderer == null || targetRenderer.Length == 0 || targetRenderer[0] == null)
		{
			AimTargetRenderers = null;
			_working = false;
			return;
		}

		//记录目标中所有的renderer以及对应的layer。	
		AimTargetRenderers = targetRenderer;

		_aimTargetCachedLayer = targetRenderer[0].gameObject.layer;

		_working = true;
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

	private static void SetFloatProperty(
		Renderer[] renderers,
		string property,
		float value)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			if (renderers[i] != null)
			{
				renderers[i].sharedMaterial.SetFloat(property, value);
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