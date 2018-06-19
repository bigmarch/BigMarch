using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public class PrefabApplyWindow : EditorWindow
{
	[MenuItem("BigMarch/Prefab Apply Window")]
	public static void ShowMaterialSafeApplyPrefabWindow()
	{
		PrefabApplyWindow window =
			(PrefabApplyWindow) GetWindow(typeof(PrefabApplyWindow));
		window.Show();
	}

	// key 是 instance， value 是 prefab。
	private Dictionary<GameObject, GameObject> _dic = new Dictionary<GameObject, GameObject>();

	private void OnGUI()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Apply"))
		{
			Apply(false);
		}

		if (GUILayout.Button("Material Safe Apply"))
		{
			Apply(true);
		}

		GUILayout.EndHorizontal();

		GUILayout.Label("当前选中的可以 Apply 的 Object :\n ");

		foreach (KeyValuePair<GameObject, GameObject> pair in _dic)
		{
			string prefabPath = AssetDatabase.GetAssetPath(pair.Value);

			GUILayout.BeginHorizontal();

			GUILayout.Box(pair.Key.name, GUILayout.Width(150));

			GUILayout.Label(">", GUILayout.Width(15));

			GUILayout.Box(prefabPath);

			GUILayout.EndHorizontal();
		}
	}

	private void OnEnable()
	{
		OnSelectionChange();
	}

	private void OnSelectionChange()
	{
		_dic.Clear();

		foreach (GameObject go in Selection.gameObjects)
		{
			if (go)
			{
				GameObject rootPrefabInstance = PrefabUtility.FindValidUploadPrefabInstanceRoot(go);

				if (rootPrefabInstance)
				{
					GameObject rootPrefab = PrefabUtility.GetPrefabParent(rootPrefabInstance) as GameObject;

					if (rootPrefab)
					{
						_dic.Add(rootPrefabInstance, rootPrefab);
					}
				}
			}
		}

		Repaint();
	}


	private void Apply(bool materialSafe)
	{
		foreach (KeyValuePair<GameObject, GameObject> pair in _dic)
		{
			if (materialSafe)
			{
				// 从 prefab 上往 game object 上拷贝 material。
				string error = CopyAllMaterial(pair.Value, pair.Key);
				if (error != "")
				{
					EditorUtility.DisplayDialog("error", error, "ok");
				}
				else
				{
					// 进行 apply
					PrefabUtility.ReplacePrefab(pair.Key, pair.Value, ReplacePrefabOptions.ConnectToPrefab);
				}
			}
			else
			{
				PrefabUtility.ReplacePrefab(pair.Key, pair.Value, ReplacePrefabOptions.ConnectToPrefab);
			}
		}
	}

	private static string CopyAllMaterial(GameObject from, GameObject to)
	{
		Renderer[] fromArr = from.GetComponentsInChildren<Renderer>();
		Renderer[] toArr = to.GetComponentsInChildren<Renderer>();

		if (fromArr.Length != toArr.Length)
		{
			string error = string.Format("{0} > {1} 子节点 renderer 数量不一致，停止 apply。", from.name, to.name);
			return error;
		}

		for (int i = 0; i < toArr.Length; i++)
		{
			Renderer fromRenderer = fromArr[i];
			Renderer toRenderer = toArr[i];

			if (fromRenderer.name != toRenderer.name)
			{
				string error = string.Format("{0} > {1} 子节点 renderer 名字不一致，停止 apply。",
					fromRenderer.name,
					toRenderer.name);
				return error;
			}

			toRenderer.sharedMaterial = fromRenderer.sharedMaterial;
		}

		return "";
	}
}