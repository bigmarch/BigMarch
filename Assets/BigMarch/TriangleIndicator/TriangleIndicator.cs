using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TriangleIndicator : MonoBehaviour
{
	public Material Mat;
	public float Length = 10;
	public float Angle = 30;

	private MeshFilter _mf;
	private MeshRenderer _mr;

	private Mesh _mesh;
	private Vector3[] _vertexArr;
	private Vector2[] _uvArr;
	private int[] _triangleArr;

	void Awake()
	{
		_mf = GetComponent<MeshFilter>();
		_mr = GetComponent<MeshRenderer>();
	}

	void OnEnable()
	{
		_mesh = new Mesh();
		_mesh.hideFlags = HideFlags.DontSave;
		_mesh.MarkDynamic();
		_mf.mesh = _mesh;

		_vertexArr = new Vector3[3];
		_uvArr = new Vector2[3];
		_triangleArr = new int[3];

		_mr.material = Mat;
	}

	void OnDisable()
	{
		if(_mesh)
		{
			DestroyImmediate(_mesh);
		}

		if (Mat)
		{
			DestroyImmediate(Mat);
		}
	}

	// Update is called once per frame
	void Update()
	{
		float halfAngle = Angle * .5f;
		float sideLength = Length / Mathf.Cos(halfAngle * Mathf.Deg2Rad);

		_vertexArr[0] = Vector3.zero;
		_vertexArr[1] = Quaternion.AngleAxis(Angle * .5f, Vector3.up) * Vector3.forward * sideLength;
		_vertexArr[2] = Quaternion.AngleAxis(-Angle * .5f, Vector3.up) * Vector3.forward * sideLength;

		_uvArr[0] = new Vector2(0, 0);
		_uvArr[1] = new Vector2(1, 0);
		_uvArr[2] = new Vector2(1, 1);

		_triangleArr[0] = 0;
		_triangleArr[1] = 1;
		_triangleArr[2] = 2;

		_mesh.vertices = _vertexArr;
		_mesh.uv = _uvArr;
		_mesh.triangles = _triangleArr;
	}
}
