using UnityEngine;

namespace BigMarch.BlendTree
{
	public abstract class Node : MonoBehaviour
	{
		public abstract BlendData GetResult();
	}
}