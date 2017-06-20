using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigMarhc.MeshSequenceFrame
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	public class MeshSequenceFrame : MonoBehaviour
	{
		public Mesh[] AllMesh;
		private MeshFilter _meshFilter;

		//动画传入的必须是float类型，为了使用unity的animation clip。
		public float Index;

		// Use this for initialization
		void Awake()
		{
		}

		// Update is called once per frame
		void Update()
		{
			Refresh();
		}

		private void Refresh()
		{
			if (AllMesh == null || AllMesh.Length == 0)
			{
				return;
			}
			if (!_meshFilter)
			{
				_meshFilter = GetComponent<MeshFilter>();
			}
			Index = Mathf.Clamp(Index, 0, AllMesh.Length - 1);
			_meshFilter.mesh = AllMesh[(int) Index];
		}
	}
}
