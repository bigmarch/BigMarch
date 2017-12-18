using UnityEngine;
using UnityEngine.Profiling;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class OoSimpleDecal : MonoBehaviour
{
	[Header("是否自动更新mesh")]
	public bool AutoUpdateMesh = false;
	[Header("需要被裁切的物体")]
	public GameObject[] TargetObjects;
	[Header("裁切三角面的过滤角度")]
	public float MaxClipAngle = 90;
	[Header("新的三角面沿法线方向的外展距离")]
	public float PushDistance = 0.01f;

	private MeshFilter _meshFilter;
	private readonly OoSimpleDecalMeshBuilder _decalMeshBuilder = new OoSimpleDecalMeshBuilder();

	void OnEnable()
	{
		if (!_meshFilter)
		{
			_meshFilter = GetComponent<MeshFilter>();
		}

		if (_meshFilter.sharedMesh != null)
		{
			DestroyImmediate(_meshFilter.sharedMesh);
			_meshFilter.sharedMesh = null;
		}

		Mesh m = new Mesh();
		m.name = "Decal Mesh";
		m.MarkDynamic();
		_meshFilter.sharedMesh = m;

		if (AutoUpdateMesh)
		{
			ReGenerateMesh();
		}
	}

	void OnDisable()
	{
		if (_meshFilter.sharedMesh != null)
		{
			DestroyImmediate(_meshFilter.sharedMesh);
			_meshFilter.sharedMesh = null;
		}
	}

//
//	// Use this for initialization
//	void Start()
//	{
//	}

	// Update is called once per frame
	void Update()
	{
		if (AutoUpdateMesh)
		{
			ReGenerateMesh();
		}
	}

	[ContextMenu("Build Mesh")]
	public void ReGenerateMesh()
	{
		_decalMeshBuilder.Clear();
		for (int i = 0; i < TargetObjects.Length; i++)
		{
			if (TargetObjects[i])
			{
				MeshFilter mf = TargetObjects[i].GetComponent<MeshFilter>();
				if (mf)
				{
					Profiler.BeginSample("Build");
					_decalMeshBuilder.Build(transform, mf, MaxClipAngle);
					Profiler.EndSample();
				}
			}
		}
		_decalMeshBuilder.Push(PushDistance);

		Profiler.BeginSample("FillToMeshAndClear");
		_decalMeshBuilder.FillToMeshAndClear(_meshFilter.sharedMesh);
		Profiler.EndSample();
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}
}

