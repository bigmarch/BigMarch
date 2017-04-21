using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigMarch.BlendTree
{
	public class BlendTree : MonoBehaviour
	{
		public BlendNode Outlet;

		private Dictionary<string, BlendNode> _blendNodeDic;

		public BlendNode GetBlendNode(string nodeName)
		{
			return _blendNodeDic[nodeName];
		}

		public float GetBlendNodeWeight(string nodeName)
		{
			return _blendNodeDic[nodeName].CurrentWeight;
		}

		public void SetBlendNodeWeight(string nodeName, float weight)
		{
			_blendNodeDic[nodeName].CurrentWeight = weight;
		}

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

			BlendNode[] allBn = Outlet.GetComponentsInChildren<BlendNode>();
			for (int i = 0; i < allBn.Length; i++)
			{
				BlendNode bn = allBn[i];
				bn.AutoSetUpstreamList();
			}

			CheckAllBlendNodeError(allBn);
		}

		// 未雨绸缪，检查所有错误。
		private static void CheckAllBlendNodeError(BlendNode[] allNode)
		{
			List<string> listForCheckRepetition = new List<string>();
			foreach (BlendNode node in allNode)
			{
				// 所有blend tree下的节点，必须挂着Node组件。
				string log = string.Format("game object: {0} need a Node Component.", node.name);
				Assert.IsNotNull(node, log);

				string errorLog = string.Format("Error in Node: {0}", node.name);

				// list不能为空。
				Assert.IsNotNull(node.UpstreamList, errorLog);
				Assert.IsTrue(node.UpstreamList.Count > 0, errorLog);

				// CurrentWeight在0~1之间。
				Assert.IsTrue(node.CurrentWeight >= 0 && node.CurrentWeight <= 1, errorLog);

				// Upstream最后一个元素不能超过1，第一个元素不能小于0
				Assert.AreEqual(0, node.UpstreamList[0].Threshold, node.name);
				Assert.AreEqual(1, node.UpstreamList[node.UpstreamList.Count - 1].Threshold, errorLog);

				// upstreamList中的元素必须递增。
				for (var j = 0; j < node.UpstreamList.Count - 1; j++)
				{
					BlendNode.Pair curret = node.UpstreamList[j];
					BlendNode.Pair next = node.UpstreamList[j + 1];
					Assert.IsTrue(next.Threshold > curret.Threshold, errorLog);
				}

				// NodeName必须有意义，不能为空。
				Assert.IsFalse(string.IsNullOrEmpty(node.name), errorLog);

				// NodeName必须唯一。
				int indexOfResult = listForCheckRepetition.IndexOf(node.name);
				Assert.IsTrue(indexOfResult == -1, errorLog);
				listForCheckRepetition.Add(node.name);
			}
		}
	}
}