using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigMarch.BlendTree
{
	public sealed class BlendNode : Node
	{
		[Serializable]
		public class Pair
		{
			public Node Node;
			[Range(0, 1)]
			public float Threshold;
		}


		[Range(0, 1)]
		public float CurrentWeight = 0.5f;

		public List<Pair> UpstreamList;

		private BlendData _cached;

		public override BlendData GetResult()
		{
			for (int i = 0; i < UpstreamList.Count; i++)
			{
				//如果循环到了最后一个，那么直接使用最后一个的结果。因为最后一个，之后，没有可以跟它混合的了。
				if (i == UpstreamList.Count - 1)
				{
					BlendData data = UpstreamList[UpstreamList.Count - 1].Node.GetResult();
					if (_cached == null)
					{
						_cached = (BlendData) Activator.CreateInstance(data.GetType());
					}
					_cached.CopyFrom(data);

					return _cached;
				}

				// 除了最后一个之外，都使用，当前的和下一个，做混合。
				Pair currentPair = UpstreamList[i];
				Pair nextPair = UpstreamList[i + 1];

				if (CurrentWeight >= currentPair.Threshold && CurrentWeight < nextPair.Threshold)
				{
					// lerp在 0~1之间。
					float lerp = (CurrentWeight - currentPair.Threshold) /
					             (nextPair.Threshold - currentPair.Threshold);

					BlendData currentBlendData = currentPair.Node.GetResult();
					BlendData nextBlendData = nextPair.Node.GetResult();
					if (_cached == null)
					{
						_cached = (BlendData) Activator.CreateInstance(currentBlendData.GetType());
					}
					_cached.CaculateLerp(currentBlendData, nextBlendData, lerp);
					return _cached;
				}
			}
			throw new Exception(string.Format("can not get blend data, current weight{0}", CurrentWeight));
		}

		// 根据hierarchy总的层级关系，递归的初始化所有的节点。
		public void AutoSetUpstreamList()
		{
			if (UpstreamList == null)
			{
				UpstreamList = new List<Pair>();
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

				var pair = new Pair
				{
					Node = childNode,
					Threshold = 1f * i / (childCount - 1)
				};

				UpstreamList.Add(pair);
			}
		}
	}
}