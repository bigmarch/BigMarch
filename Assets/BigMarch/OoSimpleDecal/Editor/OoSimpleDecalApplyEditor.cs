using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OoSimpleDecalApply))]
public class OoSimpleDecalApplyEditor : Editor
{	
	private const string DecalName = "[Decal]";
	private const string DecalInstanceName = "[DecalInstance]";
	private const string NoParentDecalName = "[NoParentDecal]";
	private const string NoParentDecalInstanceName = "[NoParentDecalInstance]";

	private readonly string _alreadyExistErrorLog = string.Format("目标物体下存在 {0} {1} {2} {3} ，需要先进行清空操作。", DecalName,
		DecalInstanceName, NoParentDecalName, NoParentDecalInstanceName);

	private readonly string _noDecalErrorLog = string.Format("目标物体下没有找到任何 {0}", DecalName);

	private readonly string _decalInstanceExist = string.Format("目标物体下不能存在 {0} {1} {2}", DecalInstanceName,
		NoParentDecalName, NoParentDecalInstanceName);

	private OoSimpleDecalApply _decalApply;

	private void OnEnable()
	{
		_decalApply = (OoSimpleDecalApply) target;
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

//		if (GUILayout.Button("IF2", GUILayout.Height(30)))
//		{
//			InitDecalItemSetForIf2();
//		}
//		EditorGUILayout.LabelField("编辑之前，先锁定Inspector面板。");

//		_root = (Transform) EditorGUILayout.ObjectField(_root, typeof(Transform), true);
//		_mat = (Material) EditorGUILayout.ObjectField(_mat, typeof(Material), false);

		GUILayout.Space(20);

		#region 三个按钮

		if (!_decalApply.Set)
		{
			GUI.enabled = false;
		}

		if (_decalApply.Set && _decalApply.MatList.Count != _decalApply.Set.DecalItemList.Count)
		{
			List<Material> newList = new List<Material>();

			for (var i = 0; i < _decalApply.Set.DecalItemList.Count; i++)
			{
				if (i < _decalApply.MatList.Count)
				{
					newList.Add(_decalApply.MatList[i]);
				}
				else
				{
					newList.Add(null);
				}
			}

			_decalApply.MatList = newList;
		}

		Transform root = _decalApply.transform;

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("生成\nDecal", GUILayout.Height(30)))
		{
			if (FindAllChild(root, DecalName).Count != 0
			    || FindAllChild(root, DecalInstanceName).Count != 0
			    || FindAllChild(root, NoParentDecalName).Count != 0
			    || FindAllChild(root, NoParentDecalInstanceName).Count != 0)
			{
				EditorUtility.DisplayDialog("", _alreadyExistErrorLog, "好");
				return;
			}

			StartEditDecal();
		}

		if (GUILayout.Button("保存\nDecalInstance", GUILayout.Height(30)))
		{
			if (FindAllChild(root, DecalInstanceName).Count != 0
			    || FindAllChild(root, NoParentDecalName).Count != 0
			    || FindAllChild(root, NoParentDecalInstanceName).Count != 0)
			{
				EditorUtility.DisplayDialog("", _decalInstanceExist, "好");
				return;
			}

			if (FindAllChild(root, DecalName).Count == 0)
			{
				EditorUtility.DisplayDialog("", _noDecalErrorLog, "好");
				return;
			}

			string confirmText =
				string.Format("目标物体下存在{0}个{1}，是否覆盖保存？", DecalName, FindAllChild(root, DecalName).Count);
			bool confirm = EditorUtility.DisplayDialog("是否保存", confirmText, "好", "不好");
			if (confirm)
			{
				GenerateAndSaveDecalInstance();
			}
		}

		if (GUILayout.Button("预览\nDecalInstance", GUILayout.Height(30)))
		{
			if (FindAllChild(root, DecalName).Count != 0
			    || FindAllChild(root, DecalInstanceName).Count != 0
			    || FindAllChild(root, NoParentDecalName).Count != 0
			    || FindAllChild(root, NoParentDecalInstanceName).Count != 0)
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

//		OoSimpleDecalApply a = (OoSimpleDecalApply)target;
		
//		SerializedProperty listProperty = serializedObject.FindProperty("DecalItemList");
//		GUI.enabled = false;
//		EditorGUILayout.PropertyField(listProperty, true);
//		GUI.enabled = true;

		Repaint();
	}

	private void StartEditDecal()
	{
		SpawnDecalAt(_decalApply.transform, _decalApply.MatList, _decalApply.Set);
	}

	private void GenerateAndSaveDecalInstance()
	{
		// thisPath: Assets/BigMarch/OoSimpleDecalExample/Decal_ii-example.asset
		string thisPath = AssetDatabase.GetAssetPath(_decalApply.Set);
		// folderPath: Assets/BigMarch/OoSimpleDecalExample/Decal_ii-example/
		string saveFolderPath = thisPath.Replace(".asset", "/");

		if (!Directory.Exists(saveFolderPath))
		{
			Debug.LogError("无法存储，找不到同名的文件夹 " + saveFolderPath);
			return;
		}

		DecalItemSet dis = _decalApply.Set;
		dis.DecalItemList.Clear();

		Transform root = _decalApply.transform;
		
		List<GameObject> decalList = FindAllChild(root, DecalName);

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

			// parent 的 local 值
			di.ParentPath = GetTransformPath(root, decal.transform.parent);

			// 放到 root 下面记录 local 值。
			Transform cacheParent = decalObj.transform.parent;
			decalObj.transform.parent = root;
			di.LocalPositionInRoot = decalObj.transform.localPosition;
			di.LocalEulerAngleInRoot = decalObj.transform.localEulerAngles;
			di.LocalScaleInRoot = decalObj.transform.localScale;

			// 放到原本的 parent 下面。
			decalObj.transform.parent = cacheParent;

			di.TargetObjPath = GetTransformPath(root, decal.TargetObjects[0].transform);

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
		_decalApply.SpawnDecalInstance();
	}

	private void ClearAll()
	{
		Transform root = _decalApply.transform;
		
		List<GameObject> decalList = FindAllChild(root, DecalName);
		foreach (GameObject go in decalList)
		{
			DestroyImmediate(go);
		}

		List<GameObject> decalInstanceList = FindAllChild(root, DecalInstanceName);
		foreach (GameObject go in decalInstanceList)
		{
			DestroyImmediate(go);
		}

		List<GameObject> noParentDecalList = FindAllChild(root, NoParentDecalName);
		foreach (GameObject go in noParentDecalList)
		{
			DestroyImmediate(go);
		}

		List<GameObject> noParentDecalInstanceList = FindAllChild(root, NoParentDecalInstanceName);
		foreach (GameObject go in noParentDecalInstanceList)
		{
			DestroyImmediate(go);
		}
	}

	private static void SpawnDecalAt(Transform root, List<Material> matList, DecalItemSet dis)
	{
		for (var i = 0; i < dis.DecalItemList.Count; i++)
		{
			DecalItemSet.DecalItem decalItem = dis.DecalItemList[i];

			OoSimpleDecal decal = CreateDecal();
			decal.AutoUpdateMesh = true;
			decal.TargetObjects = new[] {root.Find(decalItem.TargetObjPath).gameObject};
			decal.UvArea = decalItem.UvArea;
			decal.PushDistance = decalItem.PushDistance;

			// 先放到 root 下面，设置 local 值。
			Transform t = decal.transform;
			t.parent = root;
			t.localPosition = decalItem.LocalPositionInRoot;
			t.localEulerAngles = decalItem.LocalEulerAngleInRoot;
			t.localScale = decalItem.LocalScaleInRoot;

			// 设置正确的 panret
			Transform targetParent = root.Find(decalItem.ParentPath);
			if (targetParent)
			{
				decal.transform.parent = targetParent;
				decal.gameObject.name = DecalName + " " + i;
			}
			else
			{
				decal.gameObject.name = NoParentDecalName + " " + i;
			}

			if (i < matList.Count)
			{
				decal.DecalMaterial = matList[i];
			}
		}
	}


	private static OoSimpleDecal CreateDecal()
	{
		return new GameObject("Decal", typeof(OoSimpleDecal)).GetComponent<OoSimpleDecal>();
	}

	private static string GetTransformPath(Transform root, Transform target)
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
			if (child.name.Contains(targetName))
			{
				result.Add(child.gameObject);
			}
		}

		return result;
	}

//	public static readonly Rect[] RectArr =
//	{
//		new Rect(0, 0, 220, 1024),
//		new Rect(220, 642, 804, 382),
//		new Rect(220 + 527, 0, 109, 642),
//		new Rect(220, 201, 527, 441),
//		new Rect(220 + 527 + 109, 150 + 214, 168, 278),
//		new Rect(220 + 527 + 109, 150, 168, 214),
//		new Rect(220 + 527 + 109, 0, 168, 150),
//		new Rect(220, 0, 527, 201),
//	};
//
//	private void InitDecalItemSetForIf2()
//	{
//		var dis = (DecalItemSet) target;
//		dis.DecalItemList.Clear();
//
//		for (int i = 0; i < RectArr.Length; i++)
//		{
//			DecalItemSet.DecalItem di = new DecalItemSet.DecalItem();
//			di.DecalInstancePrefab = null;
//			di.ParentPath = "";
//			di.LocalPositionInRoot = Vector3.zero;
//			di.LocalEulerAngleInRoot = Vector3.zero;
//			di.LocalScaleInRoot = new Vector3(RectArr[i].width / RectArr[i].height, 1, 1);
//
//			di.TargetObjPath = "";
//
//			di.UvArea = new Rect(RectArr[i].x / 1024, RectArr[i].y / 1024, RectArr[i].width / 1024,
//				RectArr[i].height / 1024);
//			di.PushDistance = .01f;
//
//			dis.DecalItemList.Add(di);
//		}
//
//		EditorUtility.SetDirty(dis);
//	}
}