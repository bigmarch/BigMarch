using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BigMarch.BlendTree;

public class CameraRigStateNode_Fix : Node
{
	public float UpOffset = 4;
	public float DownAngle = 10;
	public float BackOffset = 8;
	public float FovRatio = 1;

	private CameraRigBlendData _cached;

	void Awake()
	{
		_cached = new CameraRigBlendData();
		Debug.Log(BlendTree);
	}

	void Update()
	{
	}

	public override BlendData GetResult()
	{
		_cached.LocalPosition = Vector3.zero;
		_cached.LocalPosition += Vector3.up * UpOffset;
		_cached.LocalPosition += Vector3.back * Mathf.Cos(Mathf.Deg2Rad * DownAngle) * BackOffset;
		_cached.LocalPosition += Vector3.up * Mathf.Sin(Mathf.Deg2Rad * DownAngle) * BackOffset;

		_cached.LocalRotation = Quaternion.Euler(DownAngle, 0, 0);

		_cached.FovRatio = FovRatio;

		return _cached;
	}
}
