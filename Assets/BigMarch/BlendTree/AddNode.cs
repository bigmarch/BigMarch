using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigMarch.BlendTree
{
	public class AddNode : Node
	{
		public List<Node> UpstreamList;
		private BlendData _cached;

		public override BlendData GetResult()
		{
			for (int i = 0; i < UpstreamList.Count; i++)
			{
				BlendData data = UpstreamList[i].GetResult();
				if (_cached == null)
				{
					_cached = (BlendData) Activator.CreateInstance(data.GetType());
				}

				if (i == 0)
				{
					_cached.CopyFrom(data);
				}
				else
				{
					_cached.CaculateAdd(_cached, data);
				}
			}
			if (_cached == null)
			{
				throw new Exception("can not get blend data, upstream count is 0");
			}
			return _cached;
		}

		// 根据hierarchy总的层级关系，递归的初始化所有的节点。
		public void AutoSetUpstreamList()
		{
			if (UpstreamList == null)
			{
				UpstreamList = new List<Node>();
			}
			else
			{
				UpstreamList.Clear();
			}

			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = transform.GetChild(i);

				Node childNode = child.GetComponent<Node>();

				UpstreamList.Add(childNode);
			}
		}
	}
}
