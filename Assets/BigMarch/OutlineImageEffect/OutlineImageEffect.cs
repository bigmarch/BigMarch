using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace BigMarch.OutlineImageEffect
{
	[RequireComponent(typeof(Camera))]
	public class OutlineImageEffect : MonoBehaviour
	{
		[Range(0, 1)] public float PhotoDownSampleMul = .25f;
		[Range(0, 1)] public float StretchDownSampleMul = .2f;

		public Color LineColor = new Color(1, 0, 0, 1);
		[Range(0, 2)] public float LineColorMul = 1.5f;

		private static OutlineImageEffect _instance;

		public static OutlineImageEffect Instance
		{
			get { return _instance; }
		}

		public List<Renderer> AimTargetRenderers;
		private int _aimTargetCachedLayer;

		public string OutlineLayerName = "Outline";
		public string OutlineClipLayerName = "OutlineClip";
		private int _outlineLayer;
		private int _outlineClipLayer;

		public Shader OneColorShader;
		public Shader StretchShader;
		public Shader AddShader;

		private Material _stretchMaterial;
		private Material _addMaterial;

		private Camera _defaultCamera;
		private Camera _outLineCamera;

		private bool _working;

		void Awake()
		{
			_instance = this;
			_defaultCamera = GetComponent<Camera>();

			_outlineLayer = LayerMask.NameToLayer(OutlineLayerName);
			_outlineClipLayer = LayerMask.NameToLayer(OutlineClipLayerName);

			Assert.IsTrue(_outlineLayer > 0, "_outlineLayer>0");
			Assert.IsTrue(_outlineClipLayer > 0, "_outlineClipLayer > 0");

			OneColorShader = Shader.Find("Hidden/OutlineOneColor");
			StretchShader = Shader.Find("Hidden/OutlineStretch");
			AddShader = Shader.Find("Hidden/OutlineAddToScreen");
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
				_outLineCamera.cullingMask = 1 << _outlineLayer;
				_outLineCamera.clearFlags = CameraClearFlags.SolidColor;
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

		void OnPreRender()
		{
			if (!_working)
			{
				return;
			}

			//完全复制主摄像机上的参数。
			_outLineCamera.fieldOfView = _defaultCamera.fieldOfView;
			_outLineCamera.nearClipPlane = _defaultCamera.nearClipPlane;
			_outLineCamera.farClipPlane = _defaultCamera.farClipPlane;
		}


		public void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (!_working)
			{
				Graphics.Blit(source, destination);
				return;
			}

			if (AimTargetRenderers == null)
			{
				Graphics.Blit(source, destination);
				return;
			}

			//确保目标的renderer都存在。
			for (int i = 0; i < AimTargetRenderers.Count; i++)
			{
				if (!AimTargetRenderers[i])
				{
					Graphics.Blit(source, destination);
					return;
				}
			}

			int sourceWidth = (int) (source.width * PhotoDownSampleMul);
			int sourceHeight = (int) (source.height * PhotoDownSampleMul);

			int downWidth = (int) (source.width * StretchDownSampleMul);
			int downHeight = (int) (source.height * StretchDownSampleMul);

			//当前正在瞄准的坦克的照片
			RenderTexture targetRt = RenderTexture.GetTemporary(sourceWidth, sourceHeight, 0, RenderTextureFormat.Default);
			targetRt.filterMode = FilterMode.Bilinear;
			//用于clip的rt
			RenderTexture clipRt = RenderTexture.GetTemporary(sourceWidth, sourceHeight, 0, RenderTextureFormat.Default);
			clipRt.filterMode = FilterMode.Bilinear;


			// SetLayer并RenderWithShader放在PreRender里会有光照方面的错误，放到OnRenderImage一切正常。

			// 第一步，设置target的layer，设置成outline。
			SetLayer(AimTargetRenderers, _outlineLayer);

			// 第二步，给outline这个layer拍一张照片。
			_outLineCamera.cullingMask = 1 << _outlineLayer;
			_outLineCamera.targetTexture = targetRt;
			_outLineCamera.RenderWithShader(OneColorShader, "");

			// 第三步，给tank outline这个layer拍一张照片。
			_outLineCamera.cullingMask = 1 << _outlineClipLayer;
			_outLineCamera.targetTexture = clipRt;
			_outLineCamera.RenderWithShader(OneColorShader, "");

			// 第四步，还原target的layer。
			SetLayer(AimTargetRenderers, _aimTargetCachedLayer);

			RenderTexture buffer0 = RenderTexture.GetTemporary(downWidth, downHeight, 0, RenderTextureFormat.Default);
			buffer0.filterMode = FilterMode.Bilinear;

			RenderTexture buffer1 = RenderTexture.GetTemporary(downWidth, downHeight, 0, RenderTextureFormat.Default);
			buffer1.filterMode = FilterMode.Bilinear;

			//横纵两个PASS拉伸
			Vector4 screenSize = new Vector4(1.0f / buffer0.width, 1.0f / buffer0.height, 0.0f, 0.0f);
			_stretchMaterial.SetVector("_ScreenSize", screenSize);

			//向下采样 + 上下左右拉伸。
			Graphics.Blit(targetRt, buffer0);
			Graphics.Blit(buffer0, buffer1, _stretchMaterial, 2);
			Graphics.Blit(buffer1, buffer0, _stretchMaterial, 3);

			//_rt中的白色部分，从stretch后的图中扣掉。
			_addMaterial.SetColor("_Color", LineColor);
			_addMaterial.SetFloat("_ColorMul", LineColorMul);
			_addMaterial.SetTexture("_ClipTex0", targetRt);
			_addMaterial.SetTexture("_ClipTex1", clipRt);

			_addMaterial.SetTexture("_SourceTex", buffer0);

			Graphics.Blit(source, destination, _addMaterial);

			RenderTexture.ReleaseTemporary(targetRt);
			RenderTexture.ReleaseTemporary(clipRt);
			RenderTexture.ReleaseTemporary(buffer0);
			RenderTexture.ReleaseTemporary(buffer1);
			_outLineCamera.targetTexture = null;
		}

		public void SetTarget(List<Renderer> targetRenderer, Color outlineColor)
		{
			if (targetRenderer == null || targetRenderer.Count == 0)
				return;

			//记录目标中所有的renderer以及对应的layer。
			AimTargetRenderers = targetRenderer;
			_aimTargetCachedLayer = targetRenderer[0].gameObject.layer;

			_working = true;
			enabled = true;

			LineColor = outlineColor;
		}

		public void ClearTarget()
		{
			AimTargetRenderers = null;
			_working = false;
			enabled = false;
		}

		private static void SetLayer(List<Renderer> renderers, int layer)
		{
			for (int i = 0; i < renderers.Count; i++)
			{
				renderers[i].gameObject.layer = layer;
			}
		}
	}
}
