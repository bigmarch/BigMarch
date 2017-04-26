using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BigMarch.BlendTree;

public class CameraRigStateNode_Shake : Node
{
	// fov的变化。
	public float ShakeFovRatioDelta = 0.1f;
	public float ShakeFovRatioLerpSpeed = 1;

	// 上下点头的角度变化。
	public float ShakeDuration = .4f;
	public AnimationCurve ShakeCurve;
	public float Amplitude = .4f;

	private float _lastStartTime;

	private CameraRigBlendData _cached;

	void Awake()
	{
		_cached = new CameraRigBlendData();
	}

	void Update()
	{
		_cached.LocalPosition = Vector3.zero;

		float progress = (Time.timeSinceLevelLoad - _lastStartTime) / ShakeDuration;
		if (progress >= 0 && progress <= 1)
		{
			float curveValue = ShakeCurve.Evaluate(progress);
			_cached.LocalRotation = Quaternion.AngleAxis(-curveValue * Amplitude, Vector3.right);
		}

		_cached.FovRatio = Mathf.Lerp(
			_cached.FovRatio,
			0,
			ShakeFovRatioLerpSpeed * Time.deltaTime);
	}

	public override BlendData GetResult()
	{
		return _cached;
	}

	[ContextMenu("Shake")]
	public void Shake()
	{
		_lastStartTime = Time.timeSinceLevelLoad;

		_cached.FovRatio += ShakeFovRatioDelta;
	}
}
