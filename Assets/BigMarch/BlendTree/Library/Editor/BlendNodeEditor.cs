using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BigMarch.BlendTree
{
	[CustomEditor(typeof(BlendNode))]
	public class BlendNodeEditor : Editor
	{
		private bool _showDefaultInspector;

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			BlendNode bn = (BlendNode) target;

			if (GUILayout.Button("Auto Setup Upstream"))
			{
				bn.AutoSetUpstreamList();
			}

			bn.CurrentWeight = EditorGUILayout.Slider("Current Weight: ", bn.CurrentWeight, 0, 1);

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			for (int i = 0; i < bn.UpstreamList.Count; i++)
			{
				BlendNode.Pair pair = bn.UpstreamList[i];
				pair.Threshold = EditorGUILayout.Slider(pair.Node.name, pair.Threshold, 0, 1);
			}

			if (!EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(bn, "BlendNodeEditor");
			}

			EditorGUILayout.Space();
			_showDefaultInspector = EditorGUILayout.ToggleLeft("Show Default Inspector", _showDefaultInspector);
			if (_showDefaultInspector)
			{
				DrawDefaultInspector();
			}
		}
	}
}

