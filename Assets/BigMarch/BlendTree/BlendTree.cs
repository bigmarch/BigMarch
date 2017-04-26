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

		private Dictionary<string, Node> _nodeDic;

		public Node GetNode(string nodeName)
		{
			if (_nodeDic == null)
			{
#if UNITY_EDITOR
				CheckAllChildError(Outlet.transform);
#endif

				Node[] blendNodeArr = GetComponentsInChildren<Node>();
				_nodeDic = new Dictionary<string, Node>();
				foreach (Node bn in blendNodeArr)
				{
					_nodeDic.Add(bn.name, bn);
				}
			}

			Node node;
			if (_nodeDic.TryGetValue(nodeName, out node))
			{
				return node;
			}
			Debug.LogError(nodeName + " not found");
			return null;
		}

		public void Setup(MonoBehaviour owner)
		{
			_owner = owner;
		}

		// 计算结果
		public BlendData GetResult()
		{
			return Outlet.GetResult();
		}

		public void AutoSetup()
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

				AddNode an = allBn[i] as AddNode;
				if (an)
				{
					an.AutoSetUpstreamList();
				}
			}

			CheckAllChildError(Outlet.transform);
		}

		// 未雨绸缪，检查所有错误。
		private static void CheckAllChildError(Transform rootTransform)
		{
			List<string> listForCheckRepetition = new List<string>();

			Transform[] allChildTransform = rootTransform.GetComponentsInChildren<Transform>();

			foreach (Transform t in allChildTransform)
			{
				string errorLog = string.Format("Error in Node: {0}", t.name);

				// NodeName必须有意义，不能为空。
				Assert.IsFalse(string.IsNullOrEmpty(t.name), errorLog);

				// NodeName必须唯一。
				int indexOfResult = listForCheckRepetition.IndexOf(t.name);
				Assert.IsTrue(indexOfResult == -1, errorLog);
				listForCheckRepetition.Add(t.name);

				Node n = t.GetComponent<Node>();
				//必须有node组件
				Assert.IsNotNull(n, errorLog);

				BlendNode bn = n as BlendNode;
				AddNode an = n as AddNode;

				if (bn)
				{
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
				}

				if (an)
				{
					// list不能为空。
					Assert.IsNotNull(an.UpstreamList, errorLog);
					Assert.IsTrue(an.UpstreamList.Count > 0, errorLog);
				}
			}
		}
	}
}