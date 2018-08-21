using System;
using BigMarch.BlendTree;
using UnityEngine;

public class CameraRigBlendData : BlendData
{
	public Vector3 LocalPosition;
	public Quaternion LocalRotation;
	public float FovRatio;

	public override void CaculateLerp(BlendData from, BlendData to, float lerpRatio)
	{
		CameraRigBlendData f = (CameraRigBlendData) from;
		CameraRigBlendData t = (CameraRigBlendData) to;

		LocalPosition = Vector3.Lerp(f.LocalPosition, t.LocalPosition, lerpRatio);
		LocalRotation = Quaternion.Lerp(f.LocalRotation, t.LocalRotation, lerpRatio);
		FovRatio = Mathf.Lerp(f.FovRatio, t.FovRatio, lerpRatio);
	}

	public override void CopyFrom(BlendData target)
	{
		CameraRigBlendData t = (CameraRigBlendData) target;
		LocalPosition = t.LocalPosition;
		LocalRotation = t.LocalRotation;
		FovRatio = t.FovRatio;
	}

	public override void CaculateAdd(BlendData lhs, BlendData rhs)
	{
		CameraRigBlendData l = (CameraRigBlendData) lhs;
		CameraRigBlendData r = (CameraRigBlendData) rhs;

		LocalPosition = l.LocalPosition + r.LocalPosition;
		LocalRotation = l.LocalRotation * r.LocalRotation;
		FovRatio = l.FovRatio + r.FovRatio;
	}
}
