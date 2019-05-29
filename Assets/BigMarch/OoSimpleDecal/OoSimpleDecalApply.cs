using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OoSimpleDecalApply : MonoBehaviour
{
	private const string DecalInstanceName = "[DecalInstance]";
	private const string NoParentDecalInstanceName = "[NoParentDecalInstance]";

	public DecalItemSet Set;
	public List<Material> MatList;

	public void SpawnDecalInstance()
	{
//		Debug.Assert(dis.DecalItemList.Count == matList.Count);

		for (var i = 0; i < Set.DecalItemList.Count; i++)
		{
			DecalItemSet.DecalItem decalItem = Set.DecalItemList[i];

			if (!decalItem.DecalInstancePrefab)
			{
				continue;
			}

			GameObject go = Instantiate(decalItem.DecalInstancePrefab, transform, true);

			// 先放到 root 下面，设置 local 值。
			go.transform.localPosition = decalItem.LocalPositionInRoot;
			go.transform.localEulerAngles = decalItem.LocalEulerAngleInRoot;
			go.transform.localScale = Vector3.one;

			// 设置正确的 parent
			Transform targetParent = transform.Find(decalItem.ParentPath);
			if (targetParent)
			{
				go.transform.parent = targetParent;
				go.name = DecalInstanceName + " " + i;
			}
			else
			{
				go.name = NoParentDecalInstanceName + " " + i;
			}

			if (i < MatList.Count)
			{
				go.GetComponent<MeshRenderer>().sharedMaterial = MatList[i];
			}
		}
	}
}