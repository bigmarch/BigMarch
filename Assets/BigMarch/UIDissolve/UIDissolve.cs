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
		[Range(-.5f, 1.5f)]
		public float DissolveAmount = 0;
		//public float Uv1Scale = 0.1f;
		public Texture DissolveTexture;
		[Range(0, 1)]
		public float DissolveWidth = .5f;
		[Range(0.001f, 0.1f)]
		public float DissolveTextureScale = 0.01f;

		public Vector2 DissolveTextureOffset = new Vector2();

		private Material _mat;

		private readonly List<UIVertex> _uiVertexList = new List<UIVertex>();

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!graphic.canvas)
			{
				return;
			}

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
			/*for (int i = 0; i < _uiVertexList.Count; i++)
			{
				UIVertex uiV = _uiVertexList[i];
				uiV.uv1 = uiV.position * Uv1Scale;
				_uiVertexList[i] = uiV;
			}*/

			// 把list应用到vertex helper中
			for (var i = 0; i < vh.currentVertCount; i++)
			{
				vh.SetUIVertex(_uiVertexList[i], i);
			}
		}

		void Update()
		{

		}

		protected override void OnEnable()
		{
			Refresh();
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			Refresh();
		}
#endif

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
			_mat.SetFloat("_WorldPositionToDissolveUv", DissolveTextureScale);
			_mat.SetVector("_DissolveUvOffset", DissolveTextureOffset);

			graphic.material = _mat;
		}

		public void Refresh()
		{
			if(graphic)
			{
				graphic.SetVerticesDirty();
			}
			RefreshMaterail();
		}
	}
}

