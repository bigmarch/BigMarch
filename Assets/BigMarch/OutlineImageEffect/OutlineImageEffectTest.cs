using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BigMarch.OutlineImageEffect
{
	public class OutlineImageEffectTest : MonoBehaviour
	{
		public GameObject Target;

		void OnGUI()
		{
			if (GUILayout.Button("Enable", GUILayout.Height(100)))
			{
				List<Renderer> list0 = new List<Renderer>();
				list0.AddRange(Target.GetComponentsInChildren<SkinnedMeshRenderer>());

				GetComponent<OutlineImageEffect>().SetTarget(list0, Color.red);
			}
			if (GUILayout.Button("Disable", GUILayout.Height(100)))
			{
				GetComponent<OutlineImageEffect>().ClearTarget();
			}
		}
	}

}
