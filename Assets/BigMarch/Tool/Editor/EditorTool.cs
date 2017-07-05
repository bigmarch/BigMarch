using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace BigMarch.Tool
{
	public class EditorTool
	{
		public static void GetTextureSourceFileResolution(TextureImporter importer, out int width, out int height)
		{
			object[] args = new object[2] {0, 0};
			MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight",
				BindingFlags.NonPublic | BindingFlags.Instance);
			mi.Invoke(importer, args);

			width = (int) args[0];
			height = (int) args[1];
		}

		[MenuItem("Assets/BigMarch/LogDependencyInfo")]
		public static void LogDependencyInfo()
		{
			if (Selection.objects.Length > 0)
			{
				foreach (Object o in Selection.objects)
				{
					string path = AssetDatabase.GetAssetPath(o);

					Debug.Log("Target: " + path);
					string[] result = AssetDatabase.GetDependencies(path, true);

					string log = "";
					foreach (var s in result)
					{
						log += s;
						log += "\n";
					}
					Debug.Log(log);
				}
			}
		}

		[MenuItem("Assets/BigMarch/LogSelectionPath")]
		public static void LoSelectionPath()
		{
			if (Selection.objects.Length > 0)
			{
				string log = "";
				foreach (Object o in Selection.objects)
				{
					string path = AssetDatabase.GetAssetPath(o);
					log += path;
					log += "\n";
				}
				Debug.Log(log);
			}
		}
	}
}