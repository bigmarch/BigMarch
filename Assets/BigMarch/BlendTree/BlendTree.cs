using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigMarch.BlendTree
{
	public class BlendTree : MonoBehaviour
	{
		public BlendNode Outlet;
		private MonoBehaviour _owner;

		public MonoBehaviour Owner
		{
			get { return _owner; }
		}

		private Dictionary<string, BlendNode> _blendNodeDic;

		public void Setup(MonoBehaviour owner)
		{
			_owner = owner;
		}

		// 得到指定的blend node的weight
		public float GetBlendNodeWeight(string nodeName)
		{
			return _blendNodeDic[nodeName].CurrentWeight;
		}

		// 设置指定的blend node的weight
		public void SetBlendNodeWeight(string nodeName, float weight)
		{
			_blendNodeDic[nodeName].CurrentWeight = weight;
		}

		// 计算结果
		public BlendData GetResult()
		{
			return Outlet.GetResult();
		}

		void Awake()
		{
			BlendNode[] blendNodeArr = GetComponentsInChildren<BlendNode>();
#if UNITY_EDITOR
			CheckAllBlendNodeError(blendNodeArr);
#endif
			_blendNodeDic = new Dictionary<string, BlendNode>();
			foreach (BlendNode bn in blendNodeArr)
			{
				_blendNodeDic.Add(bn.name, bn);
			}
		}

		public void AutoSetupAllBlendNode()
		{
			Assert.IsTrue(transform.childCount == 1, "transform.childCount == 1");

			Outlet = transform.GetChild(0).GetComponent<BlendNode>();

			Node[] allBn = Outlet.GetComponentsInChildren<Node>();
			for (int i = 0; i < allBn.Length; i++)
			{
				allBn[i].SetTree(this);
				BlendNode bn = allBn[i] as BlendNode;
				if (bn)
				{
					bn.AutoSetUpstreamList();
				}
			}

			CheckAllBlendNodeError(allBn);
		}

		// 未雨绸缪，检查所有错误。
		private static void CheckAllBlendNodeError(Node[] allNode)
		{
			List<string> listForCheckRepetition = new List<string>();
			foreach (Node node in allNode)
			{
				BlendNode bn = node as BlendNode;
				if (bn)
				{
					// 所有blend tree下的节点，必须挂着Node组件。
					string log = string.Format("game object: {0} need a Node Component.", bn.name);
					Assert.IsNotNull(bn, log);

					string errorLog = string.Format("Error in Node: {0}", bn.name);

					// list不能为空。
					Assert.IsNotNull(bn.UpstreamList, errorLog);
					Assert.IsTrue(bn.UpstreamList.Count > 0, errorLog);

					// CurrentWeight在0~1之间。
					Assert.IsTrue(bn.CurrentWeight >= 0 && bn.CurrentWeight <= 1, errorLog);

					// Upstream最后一个元素不能超过1，第一个元素不能小于0
					Assert.AreEqual(0, bn.UpstreamList[0].Threshold, bn.name);
					Assert.AreEqual(1, bn.UpstreamList[bn.UpstreamList.Count - 1].Threshold, errorLog);

					// upstreamList中的元素必须递增。
					for (var j = 0; j < bn.UpstreamList.Count - 1; j++)
					{
						BlendNode.Pair curret = bn.UpstreamList[j];
						BlendNode.Pair next = bn.UpstreamList[j + 1];
						Assert.IsTrue(next.Threshold > curret.Threshold, errorLog);
					}

					// NodeName必须有意义，不能为空。
					Assert.IsFalse(string.IsNullOrEmpty(bn.name), errorLog);

					// NodeName必须唯一。
					int indexOfResult = listForCheckRepetition.IndexOf(bn.name);
					Assert.IsTrue(indexOfResult == -1, errorLog);
					listForCheckRepetition.Add(bn.name);
				}
			}
		}
	}
}