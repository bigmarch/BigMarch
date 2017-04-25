using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BigMarch.BlendTree;

public class CameraRigStateNode_Shake : Node
{
	public float ShakeFovRatioDelta = 0.1f;
	public float ShakeFovRatioLerpSpeed = 1;

	private CameraRigBlendData _cached;
	private float _deltaFov;

	void Awake()
	{
		_cached = new CameraRigBlendData();
	}

	void Update()
	{
		_deltaFov = Mathf.Lerp(_deltaFov, 0, ShakeFovRatioLerpSpeed * Time.deltaTime);
	}

	public override BlendData GetResult()
	{
		_cached.LocalPosition = Vector3.zero;
		_cached.LocalRotation = Quaternion.identity;
		_cached.FovRatio = _deltaFov;

		return _cached;
	}

	[ContextMenu("Shake")]
	public void Shake()
	{
		_deltaFov += ShakeFovRatioDelta;
	}
}
