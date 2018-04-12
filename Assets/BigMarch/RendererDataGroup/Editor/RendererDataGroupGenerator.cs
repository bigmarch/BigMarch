using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class RendererDataGroupGeneratorWindow : EditorWindow
{
	private const string OutputFolderPath = "Assets/Tank/RendererData/";

	// Add menu item named "My Window" to the Window menu
	[MenuItem("Window/TankRendererDataGenerator")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		RendererDataGroupGeneratorWindow w = GetWindow<RendererDataGroupGeneratorWindow>();
	}

	void OnGUI()
	{
		GUILayout.Label("输出路径: " + OutputFolderPath);

		if (Selection.objects.Length == 0)
		{
			GUI.enabled = false;
		}
		if (GUILayout.Button("生成 renderer data"))
		{
			string allSelectionName = "";
			for (int i = 0; i < Selection.objects.Length; i++)
			{
				allSelectionName += Selection.objects[i].name + "\n";
			}

			if (EditorUtility.DisplayDialog("", "是否为以下 prefab 生成 renderer data:\n\n" + allSelectionName, "ok", "cancel"))
			{
				string error = CheckError(Selection.objects);
				if (error != "")
				{
					EditorUtility.DisplayDialog("error", "错误!!! :\n\n" + error, "ok");
				}
				else
				{
					if (!Directory.Exists(OutputFolderPath))
					{
						Directory.CreateDirectory(OutputFolderPath);
					}

					RendererDataGroupUtility.GenerateRendererDatas(Selection.objects, OutputFolderPath);
				}
			}
		}
		GUI.enabled = true;
	}

	private string CheckError(Object[] allObj)
	{
		string error = "";

		foreach (Object o in allObj)
		{
			string path = AssetDatabase.GetAssetPath(o);
			if (string.IsNullOrEmpty(path))
			{
				error += o.name + " 无法获取该资源路径。是 prefab 吗？\n";
			}
			else if (!path.EndsWith(".prefab"))
			{
				error += o.name + " 不是prefab。\n";
			}
		}
		return error;
	}

	private void OnSelectionChange()
	{
		Repaint();
	}
}
