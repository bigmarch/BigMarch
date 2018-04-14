using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

#endif

// 创建 render data 的数据
public static class RenderDataGroupCreate
{
	// keyword 白名单，只有这个数组里的 keyword 才会被导出，其他的都不管。
	private static readonly string[] KeywoardWhiteList = {"_PAINT_ON", "_SPLAT0USEUV1_ON"};

	public static void GenerateRendererDatas(Object[] allObj, string folderPath)
	{
		foreach (var o in allObj)
		{
			GameObject prefab = o as GameObject;
			if (!prefab)
			{
				Debug.LogError(o.name + " 转换prefab失败！");
				return;
			}
			GenerateRendererDataGroup(prefab, folderPath);
		}
	}

	private static void GenerateRendererDataGroup(GameObject prefab, string folderPath)
	{
#if UNITY_EDITOR

		RenderDataGroup rd = ScriptableObject.CreateInstance<RenderDataGroup>();
		rd.name = prefab.name.ToLower();

		rd.RendererDataArr = new List<RenderDataGroup.RendererData>();

		foreach (Renderer r in prefab.GetComponentsInChildren<Renderer>())
		{
			if (r.sharedMaterials.Length != 1)
			{
				Debug.Assert(r.sharedMaterials.Length != 1);
				continue;
			}
			rd.RendererDataArr.Add(GenerateRendererData(r));
		}

		string resultPath = folderPath + prefab.name + ".asset";
		AssetDatabase.CreateAsset(rd, resultPath.ToLower());
#endif
	}

	private static RenderDataGroup.RendererData GenerateRendererData(Renderer r)
	{
		Material mat = r.sharedMaterial;

		RenderDataGroup.RendererData result = new RenderDataGroup.RendererData();
		result.TargetRendererName = r.name;
		result.ShaderName = mat.shader.name;
		result.Keywords = FilterKeyword(mat.shaderKeywords);

#if UNITY_EDITOR

		SerializedObject psSource = new SerializedObject(mat);
		SerializedProperty properties = psSource.FindProperty("m_SavedProperties");
		SerializedProperty texEnvs = properties.FindPropertyRelative("m_TexEnvs");
		SerializedProperty floats = properties.FindPropertyRelative("m_Floats");
		SerializedProperty colors = properties.FindPropertyRelative("m_Colors");

		for (int i = 0; i < texEnvs.arraySize; i++)
		{
			string propertyName = texEnvs.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue;
			if (mat.HasProperty(propertyName))
			{
				Texture tex = mat.GetTexture(propertyName);
				result.TextureProperty.Add(new RenderDataGroup.StringKeyValue(propertyName, tex ? tex.name.ToLower() : ""));
			}
		}

		for (int i = 0; i < floats.arraySize; i++)
		{
			string propertyName = floats.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue;
			if (mat.HasProperty(propertyName))
			{
				float f = mat.GetFloat(propertyName);
				result.FloatProperty.Add(new RenderDataGroup.FloatKeyValue(propertyName, f));
			}
		}

		for (int i = 0; i < colors.arraySize; i++)
		{
			string propertyName = colors.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue;
			if (mat.HasProperty(propertyName))
			{
				Color c = mat.GetColor(propertyName);
				result.ColorProperty.Add(new RenderDataGroup.ColorKeyValue(propertyName, c));
			}
		}
#endif
		return result;
	}

	private static string[] FilterKeyword(string[] source)
	{
		List<string> result = new List<string>();
		result.AddRange(source);
		for (int i = result.Count - 1; i >= 0; i--)
		{
			if (!KeywoardWhiteList.Contains(result[i]))
			{
				result.RemoveAt(i);
			}
		}
		return result.ToArray();
	}
}
