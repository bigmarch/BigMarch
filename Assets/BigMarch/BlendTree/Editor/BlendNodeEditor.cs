using System;
using System.Collections;
using System.Collections.Generic;
using BigMarch.BlendTree;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BlendNode可视化辅助
/// </summary>

[CustomEditor(typeof(BlendNode))]
public class BlendNodeEditor : Editor
{
	public static Color START_COLOR = Color.green;
	public static Color END_COLOR = Color.white;
	protected const string BLEND_RESULT_TXT = "{0} <- {1} -> {2}";
	protected const string BLEND_RESULT_PRECISE_TXT = ">> {0} <<";
	protected const string INVALID_TIP = "INVALID";

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EditorGUILayout.BeginVertical();
		Rect adaptableRect = EditorGUILayout.GetControlRect(false, 40);
		DrawDiagram(target as BlendNode, adaptableRect);
		EditorGUILayout.EndVertical();
	}

	public static string GetBlendInfo(BlendNode node)
	{
		if (node == null || node.UpstreamList.Count == 1)
			return string.Empty;

		for (int i = 0; i < node.UpstreamList.Count; i++)
		{
			float w = node.BlendParameter;
			if (Mathf.Abs(w - node.UpstreamList[i].Threshold) < 0.01f)
				return string.Format(BLEND_RESULT_PRECISE_TXT,node.UpstreamList[i].Node.name);
			else if (i < node.UpstreamList.Count - 1 
				&& w > node.UpstreamList[i].Threshold 
				&& w < node.UpstreamList[i + 1].Threshold)
			{
				return string.Format(BLEND_RESULT_TXT
					, node.UpstreamList[i].Node.name
					, w.ToString("f2")
					, node.UpstreamList[i + 1].Node.name);
			}
		}

		return INVALID_TIP;
	}

	public static void DrawDiagram(BlendNode node, Rect adaptableRect)
	{
		EditorGUI.DrawRect(adaptableRect, Color.black);
		if (node.UpstreamList.Count > 0)
		{
			if (node.UpstreamList.Count == 1)
			{
				//stub
			}
			else
			{
				int count = node.UpstreamList.Count;

				Color c = Color.white;
				Rect r = adaptableRect;
				Vector3 p1 = Vector3.zero, p2 = Vector3.zero;
				float w = adaptableRect.width;
				for (int i = 0; i < count; i++)
				{
					c = Color.LerpUnclamped(START_COLOR, END_COLOR, (float) i/(count - 1));
					Handles.color = c;
					if (i > 0)
					{
						p1.x = r.x + node.UpstreamList[i - 1].Threshold*w;
						p1.y = r.y + r.height;
						p2.x = r.x + node.UpstreamList[i].Threshold *w;
						p2.y = r.y;
						Handles.DrawLine(p1, p2);
					}

					if (i < count - 1)
					{
						p1.x = r.x + node.UpstreamList[i].Threshold*w;
						p1.y = r.y;
						p2.x = r.x + node.UpstreamList[i + 1].Threshold*w;
						p2.y = r.y + r.height;
						Handles.DrawLine(p1, p2);
					}
				}

				c = Color.LerpUnclamped(START_COLOR, END_COLOR, node.BlendParameter);

				EditorGUI.DrawRect(new Rect(r.x + node.BlendParameter*r.width, r.y, 2, r.height), c);
			}

		}
	}
}
