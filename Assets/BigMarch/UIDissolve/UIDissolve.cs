using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BigMarch.UIDissolve
{
	[RequireComponent(typeof(ILayoutElement))]
	public class UIDissolve : BaseMeshEffect
	{
		public float Uv1Scale = 0.1f;
		public Texture DissolveTexture;
		[Range(0, 1)] public float DissolveWidth = .5f;
		[Range(0, 1)] public float DissolveAmount = 0;

		private Material _mat;

		private readonly List<UIVertex> _uiVertexList = new List<UIVertex>();

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!graphic.canvas)
			{
				return;
			}
			
			#if UNITY_5_6_OR_NEWER
			bool uv1Enable = (graphic.canvas.additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord1) != 0;
			Assert.IsTrue(uv1Enable, "Need Canvas config texcoord1");

			if (!IsActive())
			{
				return;
			}

			Assert.IsTrue(vh.currentVertCount >= 3, "vh.currentVertCount >= 3");

			_uiVertexList.Clear();
			// 把vertex helper之中的数据，populate到UIVertex list中。
			for (var i = 0; i < vh.currentVertCount; i++)
			{
				UIVertex vertex = new UIVertex();
				vh.PopulateUIVertex(ref vertex, i);
				_uiVertexList.Add(vertex);
			}

			// 处理uv，使用vertex的position作为uv2。
			for (int i = 0; i < _uiVertexList.Count; i++)
			{
				UIVertex uiV = _uiVertexList[i];
				uiV.uv1 = uiV.position * Uv1Scale;
				_uiVertexList[i] = uiV;
			}

			// 把list应用到vertex helper中
			for (var i = 0; i < vh.currentVertCount; i++)
			{
				vh.SetUIVertex(_uiVertexList[i], i);
			}
			#else
			throw new Exception("THIS FEATURE NEED UNITY5.6+");
			#endif
		}

		protected override void OnEnable()
		{
			Refresh();
		}

		protected override void OnValidate()
		{
			Refresh();
		}

		protected override void OnDisable()
		{
			DestroyImmediate(_mat);
			_mat = null;
			graphic.material = graphic.defaultMaterial;
		}

		private void RefreshMaterail()
		{
			if (!_mat)
			{
				Shader s = Shader.Find("UI/Dissolve");
				_mat = new Material(s);
				_mat.hideFlags = HideFlags.HideAndDontSave;
			}
			_mat.SetTexture("_DissolveTex", DissolveTexture);
			_mat.SetFloat("_DissolveWidth", DissolveWidth);
			_mat.SetFloat("_DissolveAmount", DissolveAmount);

			graphic.material = _mat;
		}

		public void Refresh()
		{
			graphic.SetVerticesDirty();
			RefreshMaterail();
		}
	}
}

