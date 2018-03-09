#pragma warning disable 3009,3005,3001,3002,3003

using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FrameBlendImageEffect : MonoBehaviour
{
	public bool DownSample = true;
	public int MaxRenderTextureHeight = 800;

	public bool FrameBlendOn = true;
	public float BlendRatio = .333f;

	public RenderTexture Rt0;
	public RenderTexture Rt1;
	public Camera MainCamera;

	public Shader FrameBlendShader;
	private Material _frameBlendMaterial;

	private Camera _thisEmptyShellCamera;

	public enum RenderStep
	{
		Rt0,
		Rt0ToRt1,
		Rt1,
		Rt1ToRt0
	}

	private RenderStep _renderStep;

	void Awake()
	{
		FrameBlendShader = Shader.Find("Hidden/FrameBlend");
		_thisEmptyShellCamera = GetComponent<Camera>();

		_thisEmptyShellCamera.clearFlags = CameraClearFlags.Nothing;
		_thisEmptyShellCamera.cullingMask = 0;
	}

//	// Use this for initialization
//	void Start()
//	{
//
//
//	}
//
//	// Update is called once per frame
//	void Update()
//	{
//	}

	void OnPreRender()
	{
		if (FrameBlendOn)
		{
			if (Rt0 == null || Rt1 == null)
			{
				return;
			}
			switch (_renderStep)
			{
				case RenderStep.Rt0:
					break;
				case RenderStep.Rt0ToRt1:
					MainCamera.targetTexture = Rt1;
					MainCamera.Render();
					break;
				case RenderStep.Rt1:
					break;
				case RenderStep.Rt1ToRt0:
					MainCamera.targetTexture = Rt0;
					MainCamera.Render();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		else
		{
			MainCamera.targetTexture = Rt0;
			MainCamera.Render();
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		int width = source.width;
		int height = source.height;

		if (DownSample)
		{
			if (source.height > MaxRenderTextureHeight)
			{
				float aspect = 1f * source.width / source.height;
				width = (int) (MaxRenderTextureHeight * aspect);
				height = MaxRenderTextureHeight;
			}
			else
			{
				width = source.width;
				height = source.height;
			}
		}


		if (Rt0 == null || Rt0.width != width || Rt0.height != height)
		{
			DestroyImmediate(Rt0);
			Rt0 = new RenderTexture(width, height, 24, RenderTextureFormat.Default);
			Rt0.filterMode = FilterMode.Bilinear;
		}

		if (Rt1 == null || Rt1.width != width || Rt1.height != height)
		{
			DestroyImmediate(Rt1);
			Rt1 = new RenderTexture(width, height, 24, RenderTextureFormat.Default);
			Rt1.filterMode = FilterMode.Bilinear;
		}

		if (FrameBlendOn)
		{
			switch (_renderStep)
			{
				case RenderStep.Rt0:
					//用于blend的texture进行down sample
					//blurbuffer = RenderTexture.GetTemporary(Rt1.width / 4, Rt1.height / 4, 0);
					//Graphics.Blit(Rt1, blurbuffer);

					_frameBlendMaterial.SetTexture("_BlendTex", Rt1);
					_frameBlendMaterial.SetFloat("_BlendRatio", BlendRatio);

					Graphics.Blit(Rt0, destination, _frameBlendMaterial);
					_renderStep = RenderStep.Rt0ToRt1;
					//RenderTexture.ReleaseTemporary(blurbuffer);
					break;
				case RenderStep.Rt0ToRt1:
					//用于blend的texture进行down sample
					//blurbuffer = RenderTexture.GetTemporary(Rt0.width/4, Rt0.height/4, 0);
					//Graphics.Blit(Rt0, blurbuffer);

					_frameBlendMaterial.SetTexture("_BlendTex", Rt0);
					_frameBlendMaterial.SetFloat("_BlendRatio", 1 - BlendRatio);

					Graphics.Blit(Rt1, destination, _frameBlendMaterial);
					_renderStep = RenderStep.Rt1;
					//RenderTexture.ReleaseTemporary(blurbuffer);
					break;
				case RenderStep.Rt1:
					//用于blend的texture进行down sample
					//				blurbuffer = RenderTexture.GetTemporary(Rt0.width / 4, Rt0.height / 4, 0);
					//				Graphics.Blit(Rt0, blurbuffer);

					_frameBlendMaterial.SetTexture("_BlendTex", Rt0);
					_frameBlendMaterial.SetFloat("_BlendRatio", BlendRatio);

					Graphics.Blit(Rt1, destination, _frameBlendMaterial);
					_renderStep = RenderStep.Rt1ToRt0;
					//				RenderTexture.ReleaseTemporary(blurbuffer);
					break;
				case RenderStep.Rt1ToRt0:
					//用于blend的texture进行down sample
					//blurbuffer = RenderTexture.GetTemporary(Rt1.width / 4, Rt1.height / 4, 0);
					//Graphics.Blit(Rt1, blurbuffer);

					_frameBlendMaterial.SetTexture("_BlendTex", Rt1);
					_frameBlendMaterial.SetFloat("_BlendRatio", 1 - BlendRatio);

					Graphics.Blit(Rt0, destination, _frameBlendMaterial);
					_renderStep = RenderStep.Rt0;
					//				RenderTexture.ReleaseTemporary(blurbuffer);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		else
		{
			Graphics.Blit(Rt0, destination);
		}
	}

	void OnEnable()
	{
		_frameBlendMaterial = new Material(FrameBlendShader);

		MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		MainCamera.enabled = false;

		_renderStep = RenderStep.Rt0;

		_thisEmptyShellCamera.enabled = true;
	}

	void OnDisable()
	{
		DestroyImmediate(_frameBlendMaterial);
		_frameBlendMaterial = null;

		Rt0.Release();
		DestroyImmediate(Rt0);
		Rt0 = null;

		Rt1.Release();
		DestroyImmediate(Rt1);
		Rt1 = null;

		MainCamera.enabled = true;
		MainCamera.targetTexture = null;

		_thisEmptyShellCamera.enabled = false;
	}
}
