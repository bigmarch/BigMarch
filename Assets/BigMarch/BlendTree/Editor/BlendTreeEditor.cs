using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BigMarch.BlendTree
{
	[CustomEditor(typeof(BlendTree))]
	public class BlendTreeEditor : Editor
	{
		private bool _showDefaultInspector;

		public override void OnInspectorGUI()
		{
			BlendTree bt = (BlendTree) target;
//			EditorGUILayout.ObjectField("Root:", bt.Outlet, typeof(BlendNode), false);

			EditorGUI.BeginChangeCheck();

			if (GUILayout.Button("Auto Setup"))
			{
				bt.AutoSetup();
			}

			if (!EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(bt, "BlendTreeEditor");
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

