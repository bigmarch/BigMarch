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
//		Profiler.BeginSample("0");
		_tempList0.Clear();
		_tempList0.Add(v0);
		_tempList0.Add(v1);
		_tempList0.Add(v2);
//		Profiler.EndSample();

//		Profiler.BeginSample("1");
		_tempList1.Clear();
		Clip(_tempList0, _tempList1, right);

		_tempList0.Clear();
		Clip(_tempList1, _tempList0, left);

		_tempList1.Clear();
		Clip(_tempList0, _tempList1, top);

		_tempList0.Clear();
		Clip(_tempList1, _tempList0, bottom);

		_tempList1.Clear();
		Clip(_tempList0, _tempList1, front);

		_tempList0.Clear();
		Clip(_tempList1, _tempList0, back);
//		Profiler.EndSample();

		return _tempList0;
	}

	private static void Clip(List<Vector3> input, List<Vector3> output, Plane plane)
	{
		for (int i = 0; i < input.Count; i++)
		{
			int next = (i + 1) % input.Count;
			Vector3 v1 = input[i];
			Vector3 v2 = input[next];

//			Profiler.BeginSample("get side");

			bool getSideV1 = plane.GetSide(v1);
			bool getSideV2 = plane.GetSide(v2);

			if (getSideV1)
			{
				output.Add(v1);
			}
			if (getSideV1 != getSideV2)
			{
				output.Add(PlaneLineCast(plane, v1, v2));
			}

//			Profiler.EndSample();
		}
	}

	private static Vector3 PlaneLineCast(Plane plane, Vector3 a, Vector3 b)
	{
		float dis;
		Ray ray = new Ray(a, b - a);
		plane.Raycast(ray, out dis);
		return ray.GetPoint(dis);
	}
}
