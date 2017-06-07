using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BigMarch.BlendTree;
using BigMarch.Tool;

public class CameraRigStateNode_FreeRotation : Node
{
	public Vector3 Extents = new Vector3(5, 5, 5);

	public Vector2 Angle = new Vector2(0, 45);
	public float FocusPointHeight = 1;
	public float FovRatio = 1;

	private CameraRigBlendData _cached;

	void Awake()
	{
		_cached = new CameraRigBlendData();
	}

	void Update()
	{
		if (Input.GetKey(KeyCode.A))
		{
			Angle.x += Time.deltaTime * 90;
		}

		if (Input.GetKey(KeyCode.D))
		{
			Angle.x -= Time.deltaTime * 90;
		}

		if (Input.GetKey(KeyCode.W))
		{
			Angle.y += Time.deltaTime * 90;
		}

		if (Input.GetKey(KeyCode.S))
		{
			Angle.y -= Time.deltaTime * 90;
		}
	}

	public override BlendData GetResult()
	{
		Vector3 position;
		Tool.GetPointOnEllipsoid(out position, Extents, Angle.x * Mathf.Deg2Rad, Angle.y * Mathf.Deg2Rad);

		_cached.LocalPosition = position;

		Vector3 focusPoint = new Vector3(0, FocusPointHeight, 0);
		_cached.LocalRotation = Quaternion.LookRotation(focusPoint - position);

		_cached.FovRatio = FovRatio;

		return _cached;
	}
}
