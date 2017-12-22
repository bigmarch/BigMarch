using UnityEngine;
using System.Collections.Generic;

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
		public Vector3 LocalPosition;
		public Quaternion LocalRotation;
		public Vector3 LocalScale;

		// 目标裁切的物体的路径。
		public string TargetObjPath;
		public Rect UvArea;
		public float PushDistance;
	}

	public List<DecalItem> DecalItemList;

	public void SpawnDecalAt(Transform target)
	{
		for (var i = 0; i < DecalItemList.Count; i++)
		{
			DecalItem decalItem = DecalItemList[i];

			OoSimpleDecal decal = CreateDecal();
			decal.TargetObjects = new[] {target.Find(decalItem.TargetObjPath).gameObject};
			decal.UvArea = decalItem.UvArea;
			decal.PushDistance = decalItem.PushDistance;

			decal.transform.parent = target.Find(decalItem.ParentPath);
			decal.transform.localPosition = decalItem.LocalPosition;
			decal.transform.localRotation = decalItem.LocalRotation;
			decal.transform.localScale = decalItem.LocalScale;
		}
	}

	public void SpawnDecalInstanceAt(Transform target)
	{
		for (var i = 0; i < DecalItemList.Count; i++)
		{
			DecalItem decalItem = DecalItemList[i];

			GameObject go = Instantiate(decalItem.DecalInstancePrefab);

			go.transform.parent = target.Find(decalItem.ParentPath);
			go.transform.localPosition = decalItem.LocalPosition;
			go.transform.localRotation = decalItem.LocalRotation;
			go.transform.localScale = Vector3.one;
		}
	}

	private OoSimpleDecal CreateDecal()
	{
		return new GameObject("Decal", typeof(OoSimpleDecal)).GetComponent<OoSimpleDecal>();
	}
}

