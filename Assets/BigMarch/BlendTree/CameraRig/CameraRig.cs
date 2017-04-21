using System.Collections;
using System.Collections.Generic;
using BigMarch.BlendTree;
using UnityEngine;

public class CameraRig : MonoBehaviour
{
	public BlendTree BlendTree;
	public Camera Camera;

	public const string BlendNode_ToFreeRotation = "BlendNode_ToFreeRotation";
	public const string BlendNode_ToScope = "BlendNode_ToScope";
	public const string BlendNode_NormalPositions = "BlendNode_NormalPositions";

	[Range(0, 1)] public float Weight_ToFreeRotation;
	[Range(0, 1)] public float Weight_ToScope;
	[Range(0, 1)] public float Weight_NormalPositions;

	void Start()
	{
		BlendTree.Setup(this);		
	}

	// Update is called once per frame
	void Update()
	{
		// 1，设置各个blend node中的weight
		BlendTree.SetBlendNodeWeight(BlendNode_ToFreeRotation, Weight_ToFreeRotation);
		BlendTree.SetBlendNodeWeight(BlendNode_ToScope, Weight_ToScope);
		BlendTree.SetBlendNodeWeight(BlendNode_NormalPositions, Weight_NormalPositions);

		// 2，从tree获得result。
		CameraRigBlendData data = BlendTree.GetResult() as CameraRigBlendData;
		Camera.transform.localPosition = data.LocalPosition;
		Camera.transform.localRotation = data.LocalRotation;		
		Debug.Log(data.LocalPosition + "  " + data.LocalRotation.eulerAngles + "  " + data.FovRatio);
	}
}
