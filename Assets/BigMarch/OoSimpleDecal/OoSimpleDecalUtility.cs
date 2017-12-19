using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class OoSimpleDecalUtility
{
	private static readonly Plane right = new Plane(Vector3.right, 0.5f);
	private static readonly Plane left = new Plane(Vector3.left, 0.5f);

	private static readonly Plane top = new Plane(Vector3.up, 0.5f);
	private static readonly Plane bottom = new Plane(Vector3.down, 0.5f);

	private static readonly Plane front = new Plane(Vector3.forward, 0.5f);
	private static readonly Plane back = new Plane(Vector3.back, 0.5f);


	private static readonly List<Vector3> _tempList0 = new List<Vector3>();
	private static readonly List<Vector3> _tempList1 = new List<Vector3>();

	public static List<Vector3> Clip(Vector3 v0, Vector3 v1, Vector3 v2)
	{
		_tempList0.Clear();
		_tempList0.Add(v0);
		_tempList0.Add(v1);
		_tempList0.Add(v2);

		// 右侧
		_tempList1.Clear();
		bool outOfRightPlane = ClipByPlane(_tempList0, _tempList1, right);
		if (outOfRightPlane)
		{
			return _tempList1;
		}

		// 左侧
		_tempList0.Clear();
		bool outOfLeftPlane = ClipByPlane(_tempList1, _tempList0, left);
		if (outOfLeftPlane)
		{
			return _tempList0;
		}

		// 上侧
		_tempList1.Clear();
		bool outOfTopPlane = ClipByPlane(_tempList0, _tempList1, top);
		if (outOfTopPlane)
		{
			return _tempList1;
		}


		// 下侧
		_tempList0.Clear();
		bool outOfBottomPlane = ClipByPlane(_tempList1, _tempList0, bottom);
		if (outOfBottomPlane)
		{
			return _tempList0;
		}

		// 前侧
		_tempList1.Clear();
		bool outOfFrontPlane = ClipByPlane(_tempList0, _tempList1, front);
		if (outOfFrontPlane)
		{
			return _tempList1;
		}

		// 后侧
		_tempList0.Clear();
		bool outOfBackPlane = ClipByPlane(_tempList1, _tempList0, back);
		if (outOfBackPlane)
		{
			return _tempList0;
		}

		return _tempList0;
	}

	private static bool ClipByPlane(List<Vector3> input, List<Vector3> output, Plane plane)
	{
		Profiler.BeginSample("clip by plane");
		// 是否所有的顶点，都在指定 plane 的外侧。
		bool allVertexOutOfThePlane = true;
		for (int i = 0; i < input.Count; i++)
		{
			int next = (i + 1) % input.Count;
			Vector3 v1 = input[i];
			Vector3 v2 = input[next];

//			Profiler.BeginSample("get side");
			bool getSideV1 = plane.GetSide(v1);
			bool getSideV2 = plane.GetSide(v2);
//			Profiler.EndSample();

			if (getSideV1)
			{
				output.Add(v1);

				// 只要有一个顶点在 plane 的内侧，结果即为 false。
				allVertexOutOfThePlane = false;
			}
			if (getSideV1 != getSideV2)
			{
//				Profiler.BeginSample("line cast");
				output.Add(PlaneLineCast(plane, v1, v2));
//				Profiler.EndSample();
			}
		}
		Profiler.EndSample();

		return allVertexOutOfThePlane;
	}

	private static Vector3 PlaneLineCast(Plane plane, Vector3 a, Vector3 b)
	{
		float dis;
		Ray ray = new Ray(a, b - a);
		plane.Raycast(ray, out dis);
		return ray.GetPoint(dis);
	}
}
