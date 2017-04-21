using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigMarch.Tool
{
	public static class Utility
	{
		public static void GetPointOnEllipse(out float x, out float y, float rightRadiusInRadian, float forwardRadiusInRadian, float angle)
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
	}
}
