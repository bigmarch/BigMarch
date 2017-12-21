using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

[ExecuteInEditMode]
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
	[Header("Uv Area")]
	public Rect UvArea = new Rect(0, 0, 1, 1);
	[Header("Decal材质")]
	public Material DecalMaterial;
	
	private GameObject _decalObj;

	private MeshFilter _decalMeshFilter;
	private MeshRenderer _decalMeshRenderer;
	private Mesh _decalMesh;

	private OoSimpleDecalMeshBuilder _decalMeshBuilder = new OoSimpleDecalMeshBuilder();

	void OnEnable()
	{
#if UNITY_EDITOR
		// editor 下的代码，目的是在 scene view 中按 F，摄像机能够放到合适的位置。
		MeshFilter mf = GetComponent<MeshFilter>();
		if (!mf)
		{
			mf = gameObject.AddComponent<MeshFilter>();
		}
		mf.hideFlags = HideFlags.HideAndDontSave;


		GameObject cubeGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
		mf.sharedMesh = cubeGo.GetComponent<MeshFilter>().sharedMesh;
		DestroyImmediate(cubeGo);

		MeshRenderer mr = GetComponent<MeshRenderer>();
		if (!mr)
		{
			mr = gameObject.AddComponent<MeshRenderer>();
		}
		mr.hideFlags = HideFlags.HideAndDontSave;
		mr.enabled = false;
#endif

		// 删除所以已有数据。
		DestroyDecal();

		// 创建 decal。
		_decalObj = new GameObject("Decal from [" + gameObject.name + "]");
		_decalMeshFilter = _decalObj.AddComponent<MeshFilter>();
		_decalMeshRenderer = _decalObj.AddComponent<MeshRenderer>();
		_decalMeshRenderer.sharedMaterial = DecalMaterial;

		_decalObj.hideFlags = HideFlags.HideAndDontSave;

		_decalObj.transform.position = Vector3.zero;
		_decalObj.transform.rotation = Quaternion.identity;
		_decalObj.transform.localScale = Vector3.one;

		_decalMesh = new Mesh();
		_decalMesh.name = "Decal Mesh";
		_decalMesh.MarkDynamic();
		_decalMesh.hideFlags = HideFlags.HideAndDontSave;

		_decalMeshFilter.sharedMesh = _decalMesh;

		if (AutoUpdateMesh)
		{
			ReGenerateMesh();
		}
	}

	void OnDisable()
	{
		DestroyDecal();
	}

	private void DestroyDecal()
	{
		if (_decalObj)
		{
			DestroyImmediate(_decalObj);
		}

		if (_decalMesh)
		{
			DestroyImmediate(_decalMesh);
		}
	}

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
		// 世界空间的6个 clip plane。right 就是 transform.right 方向的那个 Plane。
		Plane right = new Plane(-transform.right, transform.position + transform.right * .5f * transform.lossyScale.x);
		Plane left = new Plane(transform.right, transform.position - transform.right * .5f * transform.lossyScale.x);

		Plane top = new Plane(-transform.up, transform.position + transform.up * .5f * transform.lossyScale.y);
		Plane bottom = new Plane(transform.up, transform.position - transform.up * .5f * transform.lossyScale.y);

		Plane front = new Plane(-transform.forward, transform.position + transform.forward * .5f * transform.lossyScale.z);
		Plane back = new Plane(transform.forward, transform.position - transform.forward * .5f * transform.lossyScale.z);

//		_gizmoSize = new Vector3(right.distance, top.distance, front.distance);
//		DrawPlane(right.ClosestPointOnPlane(transform.position), right.normal);
//		DrawPlane(transform.position + transform.right * .5f* transform.lossyScale.x, -transform.right);

		_decalMeshBuilder.Clear();
		for (int i = 0; i < TargetObjects.Length; i++)
		{
			if (TargetObjects[i])
			{
				MeshFilter mf = TargetObjects[i].GetComponent<MeshFilter>();
				if (mf)
				{
					Profiler.BeginSample("Build");
					_decalMeshBuilder.Build(
						mf,
						right, left,
						top, bottom,
						front, back,
						UvArea,
						PushDistance,
						MaxClipAngle);
					Profiler.EndSample();
				}
			}
		}

		Profiler.BeginSample("FillToMeshAndClear");
		_decalMeshBuilder.FillToMeshAndClear(_decalMeshFilter.sharedMesh);
		Profiler.EndSample();
	}

	void OnDrawGizmosSelected()
	{		
		Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}

	private void OnValidate()
	{
		if(_decalMeshRenderer != null)
		{
			_decalMeshRenderer.sharedMaterial = DecalMaterial;
		}
	}

	[ContextMenu("Create Decal Instance")]
	private GameObject CreateDecalInstance()
	{
		GameObject decalInstanceGo = new GameObject("decal instance", typeof(MeshFilter), typeof(MeshRenderer));
		decalInstanceGo.transform.position = transform.position;
		decalInstanceGo.transform.rotation = transform.rotation;
		decalInstanceGo.transform.localScale = Vector3.one;

		Matrix4x4 mat = decalInstanceGo.transform.worldToLocalMatrix * _decalObj.transform.localToWorldMatrix;

		List<Vector3> verticeList = new List<Vector3>();
		List<Vector3> normalList = new List<Vector3>();
		List<Vector2> uv0List = new List<Vector2>();
		List<Vector2> uv1List = new List<Vector2>();
		List<int> triList = new List<int>();

		Mesh oldMesh = _decalMeshFilter.sharedMesh;
		oldMesh.GetVertices(verticeList);
		oldMesh.GetNormals(normalList);
		oldMesh.GetUVs(0, uv0List);
		oldMesh.GetUVs(1, uv1List);
		oldMesh.GetTriangles(triList, 0);

		for (int i = 0; i < oldMesh.vertexCount; i++)
		{
			verticeList[i] = mat.MultiplyPoint(verticeList[i]);
			normalList[i] = mat.MultiplyVector(normalList[i]);
		}

		Mesh newMesh = new Mesh();
		newMesh.SetVertices(verticeList);
		newMesh.SetNormals(normalList);
		newMesh.SetUVs(0, uv0List);
		newMesh.SetUVs(1, uv1List);
		newMesh.SetTriangles(triList, 0);

		decalInstanceGo.GetComponent<MeshFilter>().sharedMesh = newMesh;
		decalInstanceGo.GetComponent<MeshRenderer>().sharedMaterial = _decalMeshRenderer.sharedMaterial;

		return decalInstanceGo;
	}
}

