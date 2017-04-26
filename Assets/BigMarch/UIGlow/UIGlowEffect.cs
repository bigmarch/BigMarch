using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using BigMarch.Tool;

namespace BigMarch.UIGlow
{
	[RequireComponent(typeof(ILayoutElement))]
	public class UIGlowEffect : BaseMeshEffect
	{
		[Header("向下采样次数，越高图片越小。")] [Range(1, 8)] public int DownSample = 2;

		[Header("迭代次数，越高越匀称。")] [Range(1, 4)] public int BlurIterations = 1;

		[Header("横向会多出来的像素，太大会浪费，太小会导致光被切。")] public int WidthMargin = 2;

		[Header("Glow会向上偏移的像素。")] public Vector2 Offset = new Vector2(0, 1);

		[Header("glow图片的强度，当控件本身分辨率过低时，适当提高这个值。")] [Range(1, 5)] public float Strength = 5;

		[Header("glow的颜色。")] public Color32 GlowColor = new Color32(0, 255, 166, 192);

		[Header("按钮：点击刷新一次，等于执行RefreshGlowRawImage()。")] public bool TriggerManualyRefresh;

		private RenderTexture _rt;
		private Mesh _mesh;
		private RawImage _glowRawImage;

//		public RawImage GlowRawImage
//		{
//			get { return _glowRawImage; }
//		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (Application.isPlaying)
			{
				if (!_mesh)
				{
					_mesh = new Mesh();
					_mesh.MarkDynamic();
				}
				vh.FillMesh(_mesh);
			}
		}

		//创建raw image
		private void CreateGlowRawImage()
		{
			//创建raw image物体、
			GameObject go = new GameObject(graphic.name + "'s glow");
			_glowRawImage = go.AddComponent<RawImage>();
			_glowRawImage.rectTransform.SetParent(graphic.rectTransform.parent);
			_glowRawImage.rectTransform.position = graphic.rectTransform.position;
			_glowRawImage.raycastTarget = false;
			_glowRawImage.gameObject.SetActive(false);

			//调换glow和text的顺序，让raw image显示在原ui后面。
			int siblingIndex = graphic.rectTransform.GetSiblingIndex();
			_glowRawImage.rectTransform.SetSiblingIndex(siblingIndex);
			graphic.rectTransform.SetSiblingIndex(siblingIndex + 1);
		}

		//重新拍照，并给这个raw image赋值。
		[ContextMenu("Refresh Glow Raw Image")]
		public void RefreshGlowRawImage()
		{
			if (!_glowRawImage)
			{
				Debug.LogError("此时glowRawImage不存在，不应该调用Refresh方法。");
				return;
			}
			if (_rt)
			{
				_rt.Release();
			}

			//开始创造RT
			ILayoutElement l = graphic as ILayoutElement;
			//多出20像素。
			float rawImageWidth = l.preferredWidth + WidthMargin;
			_rt = UIGlowEffectCamera.Instance.CreateGlowRenderTexture(
				_mesh,
				graphic.mainTexture,
				rawImageWidth,
				DownSample,
				BlurIterations,
				Strength
			);

			//设置图片
			_glowRawImage.texture = _rt;

			_glowRawImage.color = GlowColor;

			float rtAspect = 1f * _rt.width / _rt.height;
			//设置raw image的大小。
			_glowRawImage.rectTransform.sizeDelta = new Vector2(rawImageWidth, rawImageWidth / rtAspect);
			//如果这个是text，pivot不同，导致中心点不同，这里要加上mesh的bounds做一下偏移。
			//UpOffset也要在这里应用。
			_glowRawImage.rectTransform.position = graphic.rectTransform.position + _mesh.bounds.center +
			                                       new Vector3(Offset.x, Offset.y, 0);

			if (!_glowRawImage.gameObject.activeSelf)
			{
				_glowRawImage.gameObject.SetActive(true);				
			}

			//每次刷新glow image之后，重新设置follow组件，保证glow image的相对位置是正确的。
			FollowTransform ct = _glowRawImage.gameObject.GetComponent<FollowTransform>();
			if (!ct)
			{
				ct = _glowRawImage.gameObject.AddComponent<FollowTransform>();
			}
			ct.SetFollowTarget(transform);
			ct.FollowPosition = true;
			ct.FollowRotation = true;
			ct.FollowLocalScale = true;
		}

		private void DestroyGlowRawImage()
		{
			if (_glowRawImage)
			{
				Destroy(_glowRawImage.gameObject);
				_glowRawImage = null;
			}

			if (_rt)
			{
				_rt.Release();
				_rt = null;
			}
		}

		protected override void OnEnable()
		{
			if (Application.isPlaying)
			{
				CreateGlowRawImage();

				graphic.SetVerticesDirty();
			}
		}

		protected override void OnDisable()
		{
			if (Application.isPlaying)
			{
				DestroyGlowRawImage();
			}
		}

		protected new void OnValidate()
		{
			//在hierarchy中，才执行修改material，在project面板中不执行。
			if (!gameObject.activeInHierarchy)
			{
				return;
			}

			if (TriggerManualyRefresh)
			{
				TriggerManualyRefresh = false;

				if (Application.isPlaying)
				{
					RefreshGlowRawImage();
				}
			}
		}
	}
}