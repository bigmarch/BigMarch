using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class RenameAndCloneWindow : EditorWindow
{
	[MenuItem("BigMarch/RenameAndClone")]
	public static void ShowWindow()
	{
		RenameAndCloneWindow w = GetWindow<RenameAndCloneWindow>();
		w.minSize = new Vector2(480, 320);
		w.Show();
	}

	private string _replaceFrom;
	private string _replaceTo;

	private void OnGUI()
	{
		foreach (Object o in Selection.objects)
		{
			GUILayout.Label(o.name);
		}

		GUILayout.Space(20);

		_replaceFrom = EditorGUILayout.TextField("old string: ", _replaceFrom);
		_replaceTo = EditorGUILayout.TextField("new string: ", _replaceTo);

		if (GUILayout.Button("Rename"))
		{
			foreach (var o in Selection.objects)
			{
				string oldPath = AssetDatabase.GetAssetPath(o);
				string fileName = Path.GetFileNameWithoutExtension(oldPath);
				string newName = fileName.Replace(_replaceFrom, _replaceTo);
				AssetDatabase.RenameAsset(oldPath, newName);
			}
		}

		if (GUILayout.Button("Clone"))
		{
			foreach (var o in Selection.objects)
			{
				string oldPath = AssetDatabase.GetAssetPath(o);
				string oldFileName = Path.GetFileName(oldPath);
				string newFileName = oldFileName.Replace(_replaceFrom, _replaceTo);
				string newPath = Path.GetDirectoryName(oldPath) + "/" + newFileName;
				AssetDatabase.CopyAsset(oldPath, newPath);
			}
		}
	}

	private void OnInspectorUpdate()
	{
	
	}

	private void OnSelectionChange()
	{
		Repaint();
	}
}
