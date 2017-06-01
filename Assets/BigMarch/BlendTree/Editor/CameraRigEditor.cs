using System.Collections;
using System.Collections.Generic;
using BigMarch.BlendTree;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CameraRig可视化辅助
/// </summary>

[CustomEditor(typeof(CameraRig))]
public class CameraRigEditor : Editor
{
	protected Rect adaptedRect;
	public override void OnInspectorGUI()
	{
		adaptedRect = EditorGUILayout.GetControlRect(false);

		base.OnInspectorGUI();
		BlendNode[] nodes = Selection.activeGameObject.GetComponentsInChildren<BlendNode>();
		if (nodes != null && nodes.Length > 0)
		{
			EditorGUILayout.BeginVertical();
			for (int i = 0; i < nodes.Length; i++)
			{
				if (i > 0)
				{
					EditorGUILayout.Space();
				}
				adaptedRect = EditorGUILayout.GetControlRect(false);
				EditorGUI.DropShadowLabel(adaptedRect, nodes[i].name);
				adaptedRect = EditorGUILayout.GetControlRect(false);
//					nodes[i].CurrentWeight = EditorGUI.Slider(adaptedRect, nodes[i].CurrentWeight, 0.0f, 1.0f);
				EditorGUI.LabelField(adaptedRect, BlendNodeEditor.GetBlendInfo(nodes[i]));
				adaptedRect = EditorGUILayout.GetControlRect(false, 30);
				BlendNodeEditor.DrawDiagram(nodes[i], adaptedRect);
			}
			EditorGUILayout.EndVertical();
		}
	}
}
