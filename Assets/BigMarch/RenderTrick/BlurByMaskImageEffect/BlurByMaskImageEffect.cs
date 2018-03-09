#pragma warning disable 3009,3005,3001,3002,3003

using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class BlurByMaskImageEffect : MonoBehaviour
{
	#region blur option

	[Header("Blur Option")] public bool UseBlur = false;

	[Range(0, 4)] public int downsample = 3;

	public enum BlurType
	{
		StandardGauss = 0,
		SgxGauss = 1,
	}

	[Range(0.0f, 20.0f)] public float blurSize = 8.43f;

	[Range(1, 4)] public int blurIterations = 2;

	public BlurType blurType = BlurType.StandardGauss;

	public Shader BlurShader = null;
	private Material _blurMaterial = null;

	#endregion

	#region blend option

	public enum BlendModeType
	{
		UseMaskTexture,
		Caculate,
		ExceptLayer
	}

	[Header("Blend Option")] public Shader BlendShader;
	public bool VisualizeBlend = false;
	public Color ColorMultiplier = Color.white;
	public BlendModeType BlendMode = BlendModeType.UseMaskTexture;

	[Header("  --UseMaskTexture")] public Texture MaskTexture;

	[Header("  --Caculate")] public float BlurStrength = 0;
	public float NoBlurRadius = .16f;
	public Vector2 BlurCenter = new Vector2(.5f, .4f);

	[Header("  --ExceptLayer")] public LayerMask LayerMask;

	private Material _blendMaterial;

	#endregion

	private Camera _camera;

	void Awake()
	{
		BlurShader = Shader.Find("Hidden/FastBlur");
		BlendShader = Shader.Find("Hidden/BlendTwoTexture");
	}

	public void OnEnable()
	{
		_blendMaterial = new Material(BlendShader);
		_blurMaterial = new Material(BlurShader);
	}

	public void OnDisable()
	{
		DestroyImmediate(_blendMaterial);
		DestroyImmediate(_blurMaterial);
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		RenderTexture bluredRt = null;
		RenderTexture exceptLayerRt = null;

		#region 对 blur 的处理

		if (UseBlur)
		{
			// 如果使用 blur，那么制作 blur 图片，给到 _blendTex。
			float widthMod = 1.0f / (1.0f * (1 << downsample));

			_blurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
			source.filterMode = FilterMode.Bilinear;

			int rtW = source.width >> downsample;
			int rtH = source.height >> downsample;

			// downsample

			bluredRt = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
			bluredRt.filterMode = FilterMode.Bilinear;
			// down sample
			Graphics.Blit(source, bluredRt, _blurMaterial, 0);

			var passOffs = blurType == BlurType.StandardGauss ? 0 : 2;

			for (int i = 0; i < blurIterations; i++)
			{
				float iterationOffs = (i * 1.0f);
				_blurMaterial.SetVector("_Parameter",
					new Vector4(blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));

				// vertical blur
				RenderTexture tempRt = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
				tempRt.filterMode = FilterMode.Bilinear;
				Graphics.Blit(bluredRt, tempRt, _blurMaterial, 1 + passOffs);
				bluredRt.DiscardContents();
				Graphics.Blit(tempRt, bluredRt, _blurMaterial, 2 + passOffs);

				RenderTexture.ReleaseTemporary(tempRt);
			}

			_blendMaterial.SetTexture("_BlendTex", bluredRt);
		}
		else
		{
			// 如果不使用 blend，就使用 source 当作blend texture。
			_blendMaterial.SetTexture("_BlendTex", source);
		}

		#endregion

		#region 针对不同 blend mode 的处理。

		_blendMaterial.DisableKeyword("_BLEND_MASK_TEXTURE");
		_blendMaterial.DisableKeyword("_BLEND_CACULATE");

		switch (BlendMode)
		{
			case BlendModeType.UseMaskTexture:
				_blendMaterial.EnableKeyword("_BLEND_MASK_TEXTURE");

				_blendMaterial.SetTexture("_Mask", MaskTexture);
				break;
			case BlendModeType.Caculate:
				_blendMaterial.EnableKeyword("_BLEND_CACULATE");

				_blendMaterial.SetFloat("_NoBlendRadius", NoBlurRadius);
				_blendMaterial.SetFloat("_BlendStrength", BlurStrength);
				_blendMaterial.SetVector("_BlendCenter", BlurCenter);
				break;
			case BlendModeType.ExceptLayer:
				_blendMaterial.EnableKeyword("_BLEND_MASK_TEXTURE");
				if (!_camera)
				{
					_camera = new GameObject("BlurWithMaskCamera", typeof(Camera)).GetComponent<Camera>();
					_camera.CopyFrom(GetComponent<Camera>());
					_camera.clearFlags = CameraClearFlags.SolidColor;
					_camera.backgroundColor = Color.clear;
					_camera.transform.parent = transform;
					_camera.transform.localPosition = Vector3.zero;
					_camera.transform.localRotation = Quaternion.identity;
					_camera.enabled = false;
				}
				exceptLayerRt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
				_camera.cullingMask = LayerMask;
				_camera.targetTexture = exceptLayerRt;
				_camera.Render();
				_blendMaterial.SetTexture("_Mask", exceptLayerRt);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		#endregion

		if (VisualizeBlend)
		{
			_blendMaterial.EnableKeyword("_VISUALIZE_BLEND");
		}
		else
		{
			_blendMaterial.DisableKeyword("_VISUALIZE_BLEND");
		}

		_blendMaterial.SetColor("_BlendColorMul", ColorMultiplier);

		Graphics.Blit(source, destination, _blendMaterial);

		if (bluredRt != null)
		{
			RenderTexture.ReleaseTemporary(bluredRt);
		}

		if (exceptLayerRt != null)
		{
			RenderTexture.ReleaseTemporary(exceptLayerRt);
		}
	}
}

