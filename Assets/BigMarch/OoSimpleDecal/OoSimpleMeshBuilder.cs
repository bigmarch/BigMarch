using System.Collections.Generic;
using BigMarch.Tool;
using UnityEngine;
using UnityEngine.Profiling;

public class OoSimpleDecalMeshBuilder
{
	private readonly List<Vector3> _vertices = new List<Vector3>();
	private readonly List<Vector3> _normals = new List<Vector3>();
	private readonly List<Vector2> _texcoords0 = new List<Vector2>();
	private readonly List<Vector2> _texcoords1 = new List<Vector2>();
	private readonly List<int> _indices = new List<int>();

	//	public void Clear()
	//	{
	//		_vertices.Clear();
	//		_normals.Clear();
	//		_texcoords1.Clear();
	//		_indices.Clear();
	//	}

	// 以下 list 是缓存的源 mesh 的数据。
	private List<Vector3> _vertList = new List<Vector3>();
	private List<Vector3> _normalList = new List<Vector3>();
	private List<Vector2> _uvList = new List<Vector2>();
	private List<int> _triangleList = new List<int>();

	// 世界空间的6个 plane，用于 clip。
	private Plane _rightPlane;
	private Plane _leftPlane;
	private Plane _topPlane;
	private Plane _bottomPlane;
	private Plane _frontPlane;
	private Plane _backPlane;
	private Rect _uvArea;
	private float _pushDistance;

	private float _leftRightDistance;
	private float _topBottomDistance;

	// builder 是一个 mesh 数据的容器。
	public void Build(
		Matrix4x4 targetLocalToWorldMatrix,
		Mesh targetMesh,
		Plane right, Plane left,
		Plane top, Plane bottom,
		Plane front, Plane back,
		Rect uvArea,
		float pushDistance,
		float maxClipAngle)
	{
		if (!targetMesh.isReadable)
		{
			Debug.LogError("目标 mesh 不是 readable，无法生成 decal。mesh name : " + targetMesh.name);
			return;
		}

		_rightPlane = right;
		_leftPlane = left;
		_topPlane = top;
		_bottomPlane = bottom;
		_frontPlane = front;
		_backPlane = back;

		_leftRightDistance = _rightPlane.distance + _leftPlane.distance;
		_topBottomDistance = _topPlane.distance + _bottomPlane.distance;

		_uvArea = uvArea;
		_pushDistance = pushDistance;

		//		// vector 的变换矩阵，是没有缩放信息的。
		//		// 如果带有缩放信息，对于xyz不相等的缩放，原本模型顶点的法线会变形，因此用于变换 vector 的矩阵，缩放为111。
		//		// TRS 出来是 local to world, inverse 出来之后是 world to local。
		//		Matrix4x4 decalWorldToLocalMatrix_vector =
		//			Matrix4x4.TRS(decalMakerTransform.position, decalMakerTransform.rotation, Vector3.one).inverse;
		//		Matrix4x4 objToDecalMatrix_vector = decalWorldToLocalMatrix_vector * targetMeshFilter.transform.localToWorldMatrix;
		//
		//		// point 的变换矩阵，需要包含缩放信息。
		//		// 带有缩放信息时，clip 出来的点，才能紧贴目标模型的表面。
		//		Matrix4x4 decalWorldToLocalMatrix_point = decalMakerTransform.worldToLocalMatrix;
		//		Matrix4x4 objToDecalMatrix_point = decalWorldToLocalMatrix_point * targetMeshFilter.transform.localToWorldMatrix;

		// TODO  在世界空间进行裁剪
		//		Matrix4x4 objToDecalMakerMatrix = decalMakerTransform.worldToLocalMatrix * targetMeshFilter.transform.localToWorldMatrix;

		Matrix4x4 objToWorldMatrix = targetLocalToWorldMatrix;//targetMeshFilter.transform.localToWorldMatrix;

		Mesh mesh = targetMesh;

//		Profiler.BeginSample("Mesh");

		_vertList.Clear();
		_normalList.Clear();
		_uvList.Clear();
		_triangleList.Clear();

		mesh.GetVertices(_vertList);
		mesh.GetNormals(_normalList);
		mesh.GetUVs(0, _uvList);

		mesh.GetTriangles(_triangleList, 0);

//		Profiler.EndSample();

		Profiler.BeginSample("Add");

		for (int i = 0; i < _triangleList.Count; i += 3)
		{
			int i0 = _triangleList[i];
			int i1 = _triangleList[i + 1];
			int i2 = _triangleList[i + 2];

			// 把 target mesh 中的点转换到 decal 的本地空间。
			Vector3 v0 = objToWorldMatrix.MultiplyPoint(_vertList[i0]);
			Vector3 v1 = objToWorldMatrix.MultiplyPoint(_vertList[i1]);
			Vector3 v2 = objToWorldMatrix.MultiplyPoint(_vertList[i2]);

			Vector3 n0 = objToWorldMatrix.MultiplyVector(_normalList[i0]);
			Vector3 n1 = objToWorldMatrix.MultiplyVector(_normalList[i1]);
			Vector3 n2 = objToWorldMatrix.MultiplyVector(_normalList[i2]);

			Vector2 uv0 = _uvList[i0];
			Vector2 uv1 = _uvList[i1];
			Vector2 uv2 = _uvList[i2];

			// 把三个点加到 builder 中。
			AddTriangle(
				v0, v1, v2,
				n0, n1, n2,
				uv0, uv1, uv2,
				maxClipAngle);
		}

		Profiler.EndSample();
	}

	private void AddTriangle(
		Vector3 v0, Vector3 v1, Vector3 v2,
		Vector3 n0, Vector3 n1, Vector3 n2,
		Vector2 uv0, Vector2 uv1, Vector2 uv2,
		float maxClipAngle)
	{
		Vector3 polygonNormal = Vector3.Cross(v1 - v0, v2 - v0).normalized;

		// back.normal 是后侧面儿的法线，跟 transform.forward 相同。
		if (Vector3.Angle(_backPlane.normal, -polygonNormal) <= maxClipAngle)
		{
			Profiler.BeginSample("Clip");

			List<Vector3> vertexList;
			List<Vector3> normalList;
			List<Vector2> uvList;
			OoSimpleDecalUtility.Clip(
				out vertexList,
				out normalList,
				out uvList,
				v0, v1, v2,
				n0, n1, n2,
				uv0, uv1, uv2,
				_rightPlane, _leftPlane,
				_topPlane, _bottomPlane,
				_frontPlane, _backPlane);

			Profiler.EndSample();

			if (vertexList.Count > 0)
			{
				Profiler.BeginSample("AddPolygon");
				AddPolygon(vertexList, normalList, uvList);
				Profiler.EndSample();
			}
		}
	}

	private void AddPolygon(List<Vector3> polyVertice, List<Vector3> polyNormal, List<Vector2> polyUv)
	{
		int ind1 = AddVertex(polyVertice[0], polyNormal[0], polyUv[0]);

		for (int i = 1; i < polyVertice.Count - 1; i++)
		{
			int ind2 = AddVertex(polyVertice[i], polyNormal[i], polyUv[i]);
			int ind3 = AddVertex(polyVertice[i + 1], polyNormal[i + 1], polyUv[i + 1]);

			_indices.Add(ind1);
			_indices.Add(ind2);
			_indices.Add(ind3);
		}
	}

	private int AddVertex(Vector3 vertex, Vector3 normal, Vector2 uv)
	{
		// 存入 push 后的 vertex。
		_vertices.Add(vertex + normal * _pushDistance);
		_normals.Add(normal);

		// uv0 是 decal mesh 的默认 uv，根据上下左右的 Plane 差值而得。
		float u = _leftPlane.GetDistanceToPoint(vertex) / _leftRightDistance;
		float v = _bottomPlane.GetDistanceToPoint(vertex) / _topBottomDistance;

		u = Remap(u, 0, 1, _uvArea.xMin, _uvArea.xMax);
		v = Remap(v, 0, 1, _uvArea.yMin, _uvArea.yMax);
		_texcoords0.Add(new Vector2(u, v));

		// 这里的 uv，是源 mesh 的 uv0，在 decal mesh 中，存在 uv1 中。
		_texcoords1.Add(uv);

		return _vertices.Count - 1;
	}

	// 把 builder 中的数据填充到 mesh 中，并清空。
	public void FillToMeshAndClear(Mesh mesh)
	{
		mesh.Clear(true);

		if (_indices.Count == 0)
		{
			return;
		}

		mesh.SetVertices(_vertices);
		mesh.SetNormals(_normals);
		mesh.SetUVs(0, _texcoords0);
		mesh.SetUVs(1, _texcoords1);
		mesh.SetTriangles(_indices, 0);

		Clear();
	}

	public void Clear()
	{
		_vertices.Clear();
		_normals.Clear();
		_texcoords0.Clear();
		_texcoords1.Clear();
		_indices.Clear();
	}


	public float Remap(float value, float from1, float to1, float from2, float to2)
	{
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}
}