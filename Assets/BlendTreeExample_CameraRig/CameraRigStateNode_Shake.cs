using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BigMarch.BlendTree;

public class CameraRigStateNode_Shake : Node
{
	private CameraRigBlendData _cached;

	void Awake()
	{
		_cached = new CameraRigBlendData();
	}

	void Update()
	{
	}

	public override BlendData GetResult()
	{
		_cached.LocalPosition = Vector3.zero;
		_cached.LocalRotation = Quaternion.identity;
		_cached.FovRatio = 1;

		return _cached;
	}
}
