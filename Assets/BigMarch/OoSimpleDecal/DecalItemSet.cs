using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu]
//一个tank对应一个tank decal config。
public class DecalItemSet : ScriptableObject
{
	//一个decal item表示，坦克身上的一个小贴花，一大堆decal item组合起来，叫作decal item set。
	[System.Serializable]
	public class DecalItem
	{
		//这个prefab应该是一个mesh filter。实例化出来之后，放到parent下，把local参数都设置好，就是可用状态。
		public GameObject DecalInstancePrefab;

		// decal 和 decal instance 的 parent 的路径。
		public string ParentPath;

		// 以下三个 local 值，是在 root 下的 local 值。
		public Vector3 LocalPositionInRoot;
		public Vector3 LocalEulerAngleInRoot;
		public Vector3 LocalScaleInRoot;

		// 目标裁切的物体的路径。
		public string TargetObjPath;
		public Rect UvArea;
		public float PushDistance;
	}

	public List<DecalItem> DecalItemList;
}

