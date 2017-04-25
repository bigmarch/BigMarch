using UnityEngine;

namespace BigMarch.BlendTree
{
	public abstract class Node : MonoBehaviour
	{
		[SerializeField]
		private BlendTree _blendTree;

		protected BlendTree BlendTree
		{
			get { return _blendTree; }
		}

		public void SetTree(BlendTree bt)
		{
			_blendTree = bt;
		}

		public abstract BlendData GetResult();
	}
}