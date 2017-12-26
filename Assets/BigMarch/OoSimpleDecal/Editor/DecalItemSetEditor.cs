using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DecalItemSet))]
public class DecalItemSetEditor : Editor
{
	private const string DecalName = "[Decal]";
	private const string DecalInstanceName = "[DecalInstance]";
	private readonly string _alreadyExistErrorLog = string.Format("目标物体下存在 {0} 或者 {1} ，需要先进行清空操作。", DecalName, DecalInstanceName);
	private readonly string _noDecalErrorLog = string.Format("目标物体下没有找到任何 {0}", DecalName);
	private readonly string _decalInstanceExist = string.Format("目标物体下不能存在 {0}", DecalInstanceName);

	private Transform _root;
	private Material _mat;

	public override void OnInspectorGUI()
	{
//		if (GUILayout.Button("IF2", GUILayout.Height(30)))
//		{
//			InitDecalItemSetForIf2();
//		}
		EditorGUILayout.LabelField("编辑之前，先锁定Inspector面板。");

		_root = (Transform) EditorGUILayout.ObjectField(_root, typeof(Transform), true);
		_mat = (Material) EditorGUILayout.ObjectField(_mat, typeof(Material), false);

		GUILayout.Space(20);

		#region 三个按钮

		if (!_root)
		{
			GUI.enabled = false;
		}
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("生成\nDecal", GUILayout.Height(30)))
		{
			if (DecalExist(_root) || DecalInstanceExist(_root))
			{
				EditorUtility.DisplayDialog("", _alreadyExistErrorLog, "好");
				return;
			}
			StartEditDecal();
		}
		if (GUILayout.Button("保存\nDecalInstance", GUILayout.Height(30)))
		{
			if (DecalInstanceExist(_root))
			{
				EditorUtility.DisplayDialog("", _decalInstanceExist, "好");
				return;
			}
			if (!DecalExist(_root))
			{
				EditorUtility.DisplayDialog("", _noDecalErrorLog, "好");
				return;
			}
			string confirmText = string.Format("目标物体下存在{0}个{1}，是否覆盖保存？", DecalName, GetDecalCount(_root));
			bool confirm = EditorUtility.DisplayDialog("是否保存", confirmText, "好", "不好");
			if (confirm)
			{
				GenerateAndSaveDecalInstance();
			}
		}
		if (GUILayout.Button("预览\nDecalInstance", GUILayout.Height(30)))
		{
			if (DecalExist(_root) || DecalInstanceExist(_root))
			{
				EditorUtility.DisplayDialog("", _alreadyExistErrorLog, "好");
				return;
			}
			PreviewDecalInstance();
		}
		if (GUILayout.Button("清空所有", GUILayout.Height(30)))
		{
			ClearAll();
		}

		GUILayout.EndHorizontal();
		GUI.enabled = true;

		#endregion

		GUILayout.Space(20);

		SerializedProperty listProperty = serializedObject.FindProperty("DecalItemList");
		GUI.enabled = false;
		EditorGUILayout.PropertyField(listProperty, true);
		GUI.enabled = true;

		Repaint();
	}

	private void OnDisable()
	{
//		Debug.Log("disable");
	}

	private void OnEnable()
	{
//		Debug.Log("enable");
	}

	private void StartEditDecal()
	{
		SpawnDecalAt(_root, _mat, (DecalItemSet) target);
	}

	private void GenerateAndSaveDecalInstance()
	{
		// thisPath: Assets/BigMarch/OoSimpleDecalExample/Decal_ii-example.asset
		string thisPath = AssetDatabase.GetAssetPath(target);
		// folderPath: Assets/BigMarch/OoSimpleDecalExample/Decal_ii-example/
		string saveFolderPath = thisPath.Replace(".asset", "/");

		if (!Directory.Exists(saveFolderPath))
		{
			Debug.LogError("无法存储，找不到同名的文件夹 " + saveFolderPath);
			return;
		}

		DecalItemSet dis = (DecalItemSet) target;
		dis.DecalItemList.Clear();

		List<GameObject> decalList = FindAllChild(_root, DecalName);
		
		for (var i = 0; i < decalList.Count; i++)
		{
			EditorUtility.DisplayProgressBar("", "saving", i * 1f / decalList.Count);

			GameObject decalObj = decalList[i];
			OoSimpleDecal decal = decalObj.GetComponent<OoSimpleDecal>();

			// 创建 instance
			GameObject decalInstance = decal.CreateDecalInstance();

			// 储存 mesh
			Mesh meshToSave = decalInstance.GetComponent<MeshFilter>().sharedMesh;
			MeshUtility.Optimize(meshToSave);
			string meshName = dis.name + "-DecalMesh-" + i;
			AssetDatabase.CreateAsset(meshToSave, saveFolderPath + meshName + ".asset");

			// 重新关联 mesh
			Mesh loadMesh = AssetDatabase.LoadAssetAtPath<Mesh>(saveFolderPath + meshName + ".asset");
			decalInstance.GetComponent<MeshFilter>().sharedMesh = loadMesh;

			// 存储 prefab
			string prefabName = dis.name + "-DecalInstance-" + i;
			GameObject decalInstancePrefab =
				PrefabUtility.CreatePrefab(saveFolderPath + prefabName + ".prefab",
					decalInstance);

			// 删除 instance
			DestroyImmediate(decalInstance);

			// 记录本次循环的 decal item。
			DecalItemSet.DecalItem di = new DecalItemSet.DecalItem();
			di.DecalInstancePrefab = decalInstancePrefab;
			di.ParentPath = GetTransfomPath(_root, decal.transform.parent);
			di.LocalPosition = decalObj.transform.localPosition;
			di.LocalEulerAngle = decalObj.transform.localEulerAngles;
			di.LocalScale = decalObj.transform.localScale;

			di.TargetObjPath = GetTransfomPath(_root, decal.TargetObjects[0].transform);

			di.UvArea = decal.UvArea;
			di.PushDistance = decal.PushDistance;

			dis.DecalItemList.Add(di);
		}

		EditorUtility.ClearProgressBar();
		EditorUtility.SetDirty(dis);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Repaint();
	}

	private void PreviewDecalInstance()
	{
		SpawnDecalInstanceAt(_root, _mat, (DecalItemSet) target);
	}

	private void ClearAll()
	{
		List<GameObject> decalList = FindAllChild(_root, DecalName);
		foreach (GameObject go in decalList)
		{
			DestroyImmediate(go);
		}

		List<GameObject> decalInstanceList = FindAllChild(_root, DecalInstanceName);
		foreach (GameObject go in decalInstanceList)
		{
			DestroyImmediate(go);
		}
	}

	private bool DecalExist(Transform root)
	{
		List<GameObject> decalObj = FindAllChild(_root, DecalName);
		return decalObj.Count != 0;
	}

	private bool DecalInstanceExist(Transform root)
	{
		List<GameObject> decalInstanceObj = FindAllChild(_root, DecalInstanceName);
		return decalInstanceObj.Count != 0;
	}

	private int GetDecalCount(Transform root)
	{
		List<GameObject> decalObj = FindAllChild(_root, DecalName);
		return decalObj.Count;
	}


	private static void SpawnDecalAt(Transform root, Material mat, DecalItemSet dis)
	{
		for (var i = 0; i < dis.DecalItemList.Count; i++)
		{
			DecalItemSet.DecalItem decalItem = dis.DecalItemList[i];

			OoSimpleDecal decal = CreateDecal();
			decal.AutoUpdateMesh = true;
			decal.TargetObjects = new[] {root.Find(decalItem.TargetObjPath).gameObject};
			decal.UvArea = decalItem.UvArea;
			decal.PushDistance = decalItem.PushDistance;

			decal.transform.parent = root.Find(decalItem.ParentPath);
			decal.transform.localPosition = decalItem.LocalPosition;
			decal.transform.localEulerAngles = decalItem.LocalEulerAngle;
			decal.transform.localScale = decalItem.LocalScale;

			decal.gameObject.name = DecalName;

			decal.DecalMaterial = mat;
		}
	}

	private static void SpawnDecalInstanceAt(Transform root, Material mat, DecalItemSet dis)
	{
		for (var i = 0; i < dis.DecalItemList.Count; i++)
		{
			DecalItemSet.DecalItem decalItem = dis.DecalItemList[i];

			if (!decalItem.DecalInstancePrefab)
			{
				continue;
			}
			GameObject go = Instantiate(decalItem.DecalInstancePrefab);

			go.transform.parent = root.Find(decalItem.ParentPath);
			go.transform.localPosition = decalItem.LocalPosition;
			go.transform.localEulerAngles = decalItem.LocalEulerAngle;
			go.transform.localScale = Vector3.one;

			go.name = DecalInstanceName;

			go.GetComponent<MeshRenderer>().sharedMaterial = mat;
		}
	}

	private static OoSimpleDecal CreateDecal()
	{
		return new GameObject("Decal", typeof(OoSimpleDecal)).GetComponent<OoSimpleDecal>();
	}

	private static string GetTransfomPath(Transform root, Transform target)
	{
		const int maxLoop = 100;
		int i = 0;
		string result = "";
		Transform currentT = target;
		while (currentT != root)
		{
			result = result.Insert(0, currentT.name + "/");

			currentT = currentT.parent;

			i++;
			if (i == maxLoop)
			{
				Debug.LogError("参数错误，root 不是 target 的 parnet。" + root.name + "  " + target.name);
				return "";
			}
		}

		return result;
	}

	private static List<GameObject> FindAllChild(Transform root, string targetName)
	{
		Transform[] allChild = root.GetComponentsInChildren<Transform>(true);
		List<GameObject> result = new List<GameObject>();
		foreach (Transform child in allChild)
		{
			if (child.name == targetName)
			{
				result.Add(child.gameObject);
			}
		}
		return result;
	}

	public static readonly Rect[] RectArr =
	{
		new Rect(0, 0, 220, 1024),
		new Rect(220, 642, 804, 382),
		new Rect(220 + 527, 0, 109, 642),
		new Rect(220, 201, 527, 441),
		new Rect(220 + 527 + 109, 150 + 214, 168, 278),
		new Rect(220 + 527 + 109, 150, 168, 214),
		new Rect(220 + 527 + 109, 0, 168, 150),
		new Rect(220, 0, 527, 201),
	};

	private void InitDecalItemSetForIf2()
	{
		var dis = (DecalItemSet) target;
		dis.DecalItemList.Clear();

		for (int i = 0; i < RectArr.Length; i++)
		{
			DecalItemSet.DecalItem di = new DecalItemSet.DecalItem();
			di.DecalInstancePrefab = null;
			di.ParentPath = "";
			di.LocalPosition = Vector3.zero;
			di.LocalEulerAngle = Vector3.zero;
			di.LocalScale = new Vector3(RectArr[i].width / RectArr[i].height, 1, 1);

			di.TargetObjPath = "";

			di.UvArea = new Rect(RectArr[i].x / 1024, RectArr[i].y / 1024, RectArr[i].width / 1024, RectArr[i].height / 1024);
			di.PushDistance = .01f;

			dis.DecalItemList.Add(di);
		}

		EditorUtility.SetDirty(dis);
	}
}
