using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class OoSimpleDecalUtility
{
//	private static readonly Plane right = new Plane(Vector3.right, 0.5f);
//	private static readonly Plane left = new Plane(Vector3.left, 0.5f);
//
//	private static readonly Plane top = new Plane(Vector3.up, 0.5f);
//	private static readonly Plane bottom = new Plane(Vector3.down, 0.5f);
//
//	private static readonly Plane front = new Plane(Vector3.forward, 0.5f);
//	private static readonly Plane back = new Plane(Vector3.back, 0.5f);

	private static readonly List<Vector3> _verticeTempList0 = new List<Vector3>();
	private static readonly List<Vector3> _verticeTempList1 = new List<Vector3>();
	private static readonly List<Vector3> _normalTempList0 = new List<Vector3>();
	private static readonly List<Vector3> _normalTempList1 = new List<Vector3>();
	private static readonly List<Vector2> _uvTempList0 = new List<Vector2>();
	private static readonly List<Vector2> _uvTempList1 = new List<Vector2>();

	public static void Clip(
		out List<Vector3> verticeResult,
		out List<Vector3> normalResult,
		out List<Vector2> uvResult,
		Vector3 v0, Vector3 v1, Vector3 v2,
		Vector3 n0, Vector3 n1, Vector3 n2,
		Vector2 uv0, Vector2 uv1, Vector2 uv2,
		Plane right, Plane left,
		Plane top, Plane bottom,
		Plane front, Plane back)
	{		
		_verticeTempList0.Clear();
		_verticeTempList0.Add(v0);
		_verticeTempList0.Add(v1);
		_verticeTempList0.Add(v2);

		_normalTempList0.Clear();
		_normalTempList0.Add(n0);
		_normalTempList0.Add(n1);
		_normalTempList0.Add(n2);

		_uvTempList0.Clear();
		_uvTempList0.Add(uv0);
		_uvTempList0.Add(uv1);
		_uvTempList0.Add(uv2);

		// 右侧
		_verticeTempList1.Clear();
		_normalTempList1.Clear();
		_uvTempList1.Clear();

		bool outOfRightPlane = ClipByPlane(
			_verticeTempList0,
			_verticeTempList1,
			_normalTempList0,
			_normalTempList1,
			_uvTempList0,
			_uvTempList1,
			right);

		if (outOfRightPlane)
		{
			verticeResult = _verticeTempList1;
			normalResult = _normalTempList1;
			uvResult = _uvTempList1;
			return;
		}

		// 左侧
		_verticeTempList0.Clear();
		_normalTempList0.Clear();
		_uvTempList0.Clear();

		bool outOfLeftPlane = ClipByPlane(
			_verticeTempList1,
			_verticeTempList0,
			_normalTempList1,
			_normalTempList0,
			_uvTempList1,
			_uvTempList0,
			left);

		if (outOfLeftPlane)
		{
			verticeResult = _verticeTempList0;
			normalResult = _normalTempList0;
			uvResult = _uvTempList0;
			return;
		}

		// 上侧
		_verticeTempList1.Clear();
		_normalTempList1.Clear();
		_uvTempList1.Clear();

		bool outOfTopPlane = ClipByPlane(
			_verticeTempList0,
			_verticeTempList1,
			_normalTempList0,
			_normalTempList1,
			_uvTempList0,
			_uvTempList1,
			top);

		if (outOfTopPlane)
		{
			verticeResult = _verticeTempList1;
			normalResult = _normalTempList1;
			uvResult = _uvTempList1;
			return;
		}

		// 下侧
		_verticeTempList0.Clear();
		_normalTempList0.Clear();
		_uvTempList0.Clear();

		bool outOfBottomPlane = ClipByPlane(
			_verticeTempList1,
			_verticeTempList0,
			_normalTempList1,
			_normalTempList0,
			_uvTempList1,
			_uvTempList0,
			bottom);

		if (outOfBottomPlane)
		{
			verticeResult = _verticeTempList0;
			normalResult = _normalTempList0;
			uvResult = _uvTempList0;
			return;
		}

		// 前侧
		_verticeTempList1.Clear();
		_normalTempList1.Clear();
		_uvTempList1.Clear();

		bool outOfFrontPlane = ClipByPlane(
			_verticeTempList0,
			_verticeTempList1,
			_normalTempList0,
			_normalTempList1,
			_uvTempList0,
			_uvTempList1,
			front);

		if (outOfFrontPlane)
		{
			verticeResult = _verticeTempList1;
			normalResult = _normalTempList1;
			uvResult = _uvTempList1;
			return;
		}

		// 后侧
		_verticeTempList0.Clear();
		_normalTempList0.Clear();
		_uvTempList0.Clear();

		bool outOfBackPlane = ClipByPlane(
			_verticeTempList1, 
			_verticeTempList0,
			_normalTempList1,
			_normalTempList0,
			_uvTempList1,
			_uvTempList0,
			back);

		if (outOfBackPlane)
		{
			verticeResult = _verticeTempList0;
			normalResult = _normalTempList0;
			uvResult = _uvTempList0;
			return;
		}

		// 至此，一个点都没被剃掉，全都在内部。
		verticeResult = _verticeTempList0;
		normalResult = _normalTempList0;
		uvResult = _uvTempList0;
	}

	private static bool ClipByPlane(
		List<Vector3> verticeInput, 
		List<Vector3> verticeOutput, 
		List<Vector3> normalInput,
		List<Vector3> normalOutput,	
		List<Vector2> uvInput,
		List<Vector2> uvOutput,
		Plane plane)
	{
//		Profiler.BeginSample("clip by plane");
		// 是否所有的顶点，都在指定 plane 的外侧。
		bool allVertexOutOfThePlane = true;
		for (int i = 0; i < verticeInput.Count; i++)
		{
			int next = (i + 1) % verticeInput.Count;

			Vector3 v0 = verticeInput[i];
			Vector3 v1 = verticeInput[next];

			Vector3 n0 = normalInput[i];
			Vector3 n1 = normalInput[next];

			Vector3 uv0 = uvInput[i];
			Vector3 uv1 = uvInput[next];

//			Profiler.BeginSample("get side");
			bool getSideV1 = plane.GetSide(v0);
			bool getSideV2 = plane.GetSide(v1);
//			Profiler.EndSample();

			if (getSideV1)
			{
				verticeOutput.Add(v0);
				normalOutput.Add(n0);
				uvOutput.Add(uv0);

				// 只要有一个顶点在 plane 的内侧，结果即为 false。
				allVertexOutOfThePlane = false;
			}

			if (getSideV1 != getSideV2)
			{
				//				Profiler.BeginSample("line cast");
				Vector3 vertice;
				Vector3 normal;
				Vector2 uv;
				PlaneLineCast(
					out vertice,
					out normal,
					out uv,
					plane,
					v0, v1,
					n0, n1,
					uv0, uv1);

				verticeOutput.Add(vertice);
				normalOutput.Add(normal);
				uvOutput.Add(uv);

//				Debug.DrawRay(vertice, normal * .25f, Color.red);

				//				Profiler.EndSample();
			}
//			else
//			{
//				Debug.DrawRay(v1, n1 * .15f);
//			}
		}
//		Profiler.EndSample();

		return allVertexOutOfThePlane;
	}

	private static void PlaneLineCast(
		out Vector3 resultVertice,
		out Vector3 resultNormal,
		out Vector2 resultUv,
		Plane plane,
		Vector3 verticeA,
		Vector3 verticeB,
		Vector3 normalA,
		Vector3 normalB,
		Vector2 uvA,
		Vector2 uvB)
	{
		float dis;
		Ray ray = new Ray(verticeA, verticeB - verticeA);
		plane.Raycast(ray, out dis);
		resultVertice = ray.GetPoint(dis);

		float lerpRatio = dis / (verticeB - verticeA).magnitude;
		resultNormal = Vector3.Lerp(normalA, normalB, lerpRatio);
		resultUv = Vector2.Lerp(uvA, uvB, lerpRatio);
//		Debug.DrawRay(ray.origin, ray.direction);
	}
}
