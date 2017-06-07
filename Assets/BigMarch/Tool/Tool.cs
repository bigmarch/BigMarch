using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigMarch.Tool
{
	public static class Tool
	{
		public static void GetPointOnEllipse(out float x, out float y, float rightRadiusInRadian, float forwardRadiusInRadian,
			float angle)
		{
			x = rightRadiusInRadian * Mathf.Cos(angle);
			y = forwardRadiusInRadian * Mathf.Sin(angle);
		}

		public static void GetPointOnEllipsoid(
			out Vector3 pointOnEllipsoid,
			Vector3 extents,
			float horiAngleInRadian,
			float vertiAngleInRadian)
		{
			float x = extents.x * Mathf.Cos(vertiAngleInRadian) * Mathf.Cos(horiAngleInRadian);
			float z = extents.z * Mathf.Cos(vertiAngleInRadian) * Mathf.Sin(horiAngleInRadian);
			float y = extents.y * Mathf.Sin(vertiAngleInRadian);
			pointOnEllipsoid = new Vector3(x, y, z);
		}

		public static Vector2 LerpAngle(Vector2 a, Vector2 b, float t)
		{
			Vector2 result;
			result.x = Mathf.LerpAngle(a.x, b.x, t);
			result.y = Mathf.LerpAngle(a.y, b.y, t);
			return result;
		}

		#region component extension

		public static RectTransform RectTransform(this Component cp)
		{
			return cp.transform as RectTransform;
		}

		public static float Remap(this float value, float from1, float to1, float from2, float to2)
		{
			return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
		}

		#endregion

		#region Triangle

		private static float TriangleArea(float v0x, float v0y, float v1x, float v1y, float v2x, float v2y)
		{
			return Mathf.Abs((v0x * v1y + v1x * v2y + v2x * v0y
			                  - v1x * v0y - v2x * v1y - v0x * v2y) / 2f);
		}

		// 判断一个点是否正在三角形当中。
		public static bool IsInTriangle(Vector3 targetPoint, Vector3 v0, Vector3 v1, Vector3 v2)
		{
			float x = targetPoint.x;
			float y = targetPoint.z;

			float v0x = v0.x;
			float v0y = v0.z;

			float v1x = v1.x;
			float v1y = v1.z;

			float v2x = v2.x;
			float v2y = v2.z;

			float t = TriangleArea(v0x, v0y, v1x, v1y, v2x, v2y);
			float a = TriangleArea(v0x, v0y, v1x, v1y, x, y) + TriangleArea(v0x, v0y, x, y, v2x, v2y) +
			          TriangleArea(x, y, v1x, v1y, v2x, v2y);

			if (Mathf.Abs(t - a) <= 0.01f)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool IsInTwoTriangleView(Vector3 targetPoint, Vector3 viewPoint, Vector3 viewDir, float viewAngle,
			float sideLength)
		{
			//四个朝向。
			Quaternion leftTriangleRight = Quaternion.LookRotation(viewDir, Vector3.up);
			Quaternion leftTriangleLeft = leftTriangleRight * Quaternion.AngleAxis(-viewAngle * .5f, Vector3.up);

			Quaternion rightTriangleLeft = Quaternion.LookRotation(viewDir, Vector3.up);
			Quaternion rightTriangleRight = rightTriangleLeft * Quaternion.AngleAxis(+viewAngle * .5f, Vector3.up);

			//三角形的三个顶点
			Vector3 left0 = viewPoint;
			Vector3 left1 = left0 + leftTriangleRight * Vector3.forward * sideLength;
			Vector3 left2 = left0 + leftTriangleLeft * Vector3.forward * sideLength;

			Vector3 right0 = viewPoint;
			Vector3 right1 = right0 + rightTriangleLeft * Vector3.forward * sideLength;
			Vector3 right2 = right0 + rightTriangleRight * Vector3.forward * sideLength;

			bool inleftTriangle = IsInTriangle(targetPoint, left0, left1, left2);
			bool inRIghtTriangle = IsInTriangle(targetPoint, right0, right1, right2);

			Debug.DrawLine(left0, left1);
			Debug.DrawLine(left1, left2);
			Debug.DrawLine(left2, left0);
			Debug.DrawLine(right0, right1);
			Debug.DrawLine(right1, right2);
			Debug.DrawLine(right2, right0);

			if (inleftTriangle || inRIghtTriangle)
			{
				return true;
			}

			return false;
		}

		#endregion
	}
}
