using System.Collections;
using System.Collections.Generic;
using BigMarch.BlendTree;
using UnityEngine;

public class CameraRig : MonoBehaviour
{
	public BlendTree BlendTree;
	public Camera Camera;

	private BlendNode _root;
	private BlendNode _fix;
	private BlendNode _normalPositions;

	private CameraRigStateNode_Shake _normalHitShake;
	private CameraRigStateNode_Shake _normalHurtShake;
	private CameraRigStateNode_Shake _scopeHitShake;
	private CameraRigStateNode_Shake _scopeHurtShake;

	[Range(0, 1)] public float Weight_Root;
	[Range(0, 1)] public float Weight_Fix;
	[Range(0, 1)] public float Weight_NormalPositions;

	void Awake()
	{
		BlendTree.Setup(this);

		_root = BlendTree.GetNode("BlendNode_Root") as BlendNode;
		_fix = BlendTree.GetNode("BlendNode_Fix") as BlendNode;
		_normalPositions = BlendTree.GetNode("BlendNode_NormalPositions") as BlendNode;

		_normalHitShake = BlendTree.GetNode("StateNode_NormalHitShake") as CameraRigStateNode_Shake;
		_normalHurtShake = BlendTree.GetNode("StateNode_NormalHurtShake") as CameraRigStateNode_Shake;
		_scopeHitShake = BlendTree.GetNode("StateNode_ScopeHitShake") as CameraRigStateNode_Shake;
		_scopeHurtShake = BlendTree.GetNode("StateNode_ScopeHurtShake") as CameraRigStateNode_Shake;
	}

	// Update is called once per frame
	void Update()
	{
		// 1，设置各个blend node中的weight
		_root.CurrentWeight = Weight_Root;
		_fix.CurrentWeight = Weight_Fix;
		_normalPositions.CurrentWeight = Weight_NormalPositions;

		// 2，从tree获得result。
		CameraRigBlendData data = BlendTree.GetResult() as CameraRigBlendData;
		Camera.transform.localPosition = data.LocalPosition;
		Camera.transform.localRotation = data.LocalRotation;
		Camera.fieldOfView = data.FovRatio * 60;

		Debug.Log(data.LocalPosition + "  " + data.LocalRotation.eulerAngles + "  " + data.FovRatio);
	}

	[ContextMenu("Hit Shake")]
	public void HitShake()
	{
		_normalHitShake.Shake();
		_scopeHitShake.Shake();
	}

	[ContextMenu("Hurt Shake")]
	public void HurtShake()
	{
		_normalHurtShake.Shake();
		_scopeHurtShake.Shake();
	}
}
