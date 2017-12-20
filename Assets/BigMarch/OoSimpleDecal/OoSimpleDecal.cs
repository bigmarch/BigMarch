using System;
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
		DestroyDecal();

		_decalObj = new GameObject("[Decal: " + gameObject.name + "]");
		_decalMeshFilter = _decalObj.AddComponent<MeshFilter>();
		_decalMeshRenderer = _decalObj.AddComponent<MeshRenderer>();
		_decalMeshRenderer.sharedMaterial = DecalMaterial;
		_decalObj.hideFlags = HideFlags.DontSave | HideFlags.HideAndDontSave;

		_decalObj.transform.position = Vector3.zero;
		_decalObj.transform.rotation = Quaternion.identity;
		_decalObj.transform.localScale = Vector3.one;

		_decalMesh = new Mesh();
		_decalMesh.name = "Decal Mesh";
		_decalMesh.MarkDynamic();

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

//		DrawPlane(transform.position - transform.right * .5f * transform.lossyScale.x, transform.right);
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
		_decalMeshBuilder.Push(PushDistance);

		Profiler.BeginSample("FillToMeshAndClear");
		_decalMeshBuilder.FillToMeshAndClear(_decalMeshFilter.sharedMesh);
		Profiler.EndSample();
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);		
	}

	private void OnValidate()
	{
		_decalMeshRenderer.sharedMaterial = DecalMaterial;
	}

	private void DrawPlane(Vector3 position, Vector3 normal)
	{
		Vector3 v3;

		if (normal.normalized != Vector3.forward)
			v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
		else
			v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude;

		var corner0 = position + v3;
		var corner2 = position - v3;
		var q = Quaternion.AngleAxis(90.0f, normal);
		v3 = q * v3;
		var corner1 = position + v3;
		var corner3 = position - v3;

		Debug.DrawLine(corner0, corner2, Color.green);
		Debug.DrawLine(corner1, corner3, Color.green);
		Debug.DrawLine(corner0, corner1, Color.green);
		Debug.DrawLine(corner1, corner2, Color.green);
		Debug.DrawLine(corner2, corner3, Color.green);
		Debug.DrawLine(corner3, corner0, Color.green);
		Debug.DrawRay(position, normal, Color.red);
	}
}

