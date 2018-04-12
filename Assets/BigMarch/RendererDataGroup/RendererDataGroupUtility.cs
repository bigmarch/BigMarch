using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class RendererDataGroupUtility
{
	#region 生成

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

		RendererDataGroup rd = ScriptableObject.CreateInstance<RendererDataGroup>();
		rd.name = prefab.name.ToLower();

		rd.RendererDataArr = new List<RendererDataGroup.RendererData>();

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

	private static RendererDataGroup.RendererData GenerateRendererData(Renderer r)
	{
		Material mat = r.sharedMaterial;

		RendererDataGroup.RendererData result = new RendererDataGroup.RendererData();
		result.TargetRendererName = r.name;
		result.ShaderName = mat.shader.name;
		result.Keywords = FilterKeyword(mat.shaderKeywords);

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
				result.TextureProperty.Add(new RendererDataGroup.StringKeyValue(propertyName, tex ? tex.name.ToLower() : ""));
			}
		}

		for (int i = 0; i < floats.arraySize; i++)
		{
			string propertyName = floats.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue;
			if (mat.HasProperty(propertyName))
			{
				float f = mat.GetFloat(propertyName);
				result.FloatProperty.Add(new RendererDataGroup.FloatKeyValue(propertyName, f));
			}
		}

		for (int i = 0; i < colors.arraySize; i++)
		{
			string propertyName = colors.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue;
			if (mat.HasProperty(propertyName))
			{
				Color c = mat.GetColor(propertyName);
				result.ColorProperty.Add(new RendererDataGroup.ColorKeyValue(propertyName, c));
			}
		}

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

	#endregion

	#region 解析（注意，以下代码并未做过测试，只是表达 RendererDataGroup 的思想，分为 editor 下和 runtime，在这里做备忘。）

	// editor 下使用的方法，同步加载
	public Dictionary<string, Material> CreateMaterialEditor(RendererDataGroup rdg, bool hdOrSd)
	{
		Dictionary<string, Material> result = new Dictionary<string, Material>();

		foreach (RendererDataGroup.RendererData rd in rdg.RendererDataArr)
		{
			Material mat = new Material(Shader.Find(rd.ShaderName));
			ApplyFloatProperty(mat, rd.FloatProperty);
			ApplyColorProperty(mat, rd.ColorProperty);
			ApplyTexturePropertyInEditor(mat, rd.TextureProperty, hdOrSd);
			mat.shaderKeywords = rd.Keywords;

			result.Add(rd.TargetRendererName, mat);
		}

		return result;
	}

	// runtime 下使用的方法
	public IEnumerator CreateMaterialRuntime(
		Dictionary<string, Material> result,
		RendererDataGroup rdg,
		bool hdOrSd,
		MonoBehaviour coroutineOwner)
	{
		foreach (RendererDataGroup.RendererData rd in rdg.RendererDataArr)
		{
			Material mat = new Material(Shader.Find(rd.ShaderName));
			ApplyFloatProperty(mat, rd.FloatProperty);
			ApplyColorProperty(mat, rd.ColorProperty);
			yield return coroutineOwner.StartCoroutine(ApplyTexturePropertyInRuntime(mat, rd.TextureProperty, hdOrSd));
			mat.shaderKeywords = rd.Keywords;

			result.Add(rd.TargetRendererName, mat);
		}
	}

	private void ApplyFloatProperty(Material targetMat, List<RendererDataGroup.FloatKeyValue> list)
	{
		foreach (var pair in list)
		{
			targetMat.SetFloat(pair.Key, pair.Value);
		}
	}

	private void ApplyColorProperty(Material targetMat, List<RendererDataGroup.ColorKeyValue> list)
	{
		foreach (var pair in list)
		{
			targetMat.SetColor(pair.Key, pair.Value);
		}
	}

	// 该方法没有经过测试，可能好使，可能不好使，只是表达思想，备忘。
	private IEnumerator ApplyTexturePropertyInRuntime(
		Material targetMat,
		List<RendererDataGroup.StringKeyValue> list,
		bool hdOrSd)
	{
		string hdOrSdString = hdOrSd ? "texturehd" : "texturesd";

		foreach (var pair in list)
		{
			string textureName = pair.Value;
			string firstWord = textureName.Split('_')[0];
			// 举例 tank/texturehd/common 或者 tank/texturesd/is4
			string abName = string.Format("tank/{0}/{1}", hdOrSdString, firstWord);

			// 从未测试过，凭感觉写的 ab 资源加载逻辑。
			AssetBundleCreateRequest abcRequest = AssetBundle.LoadFromFileAsync(abName);
			while (!abcRequest.isDone)
			{
				yield return null;
			}
			AssetBundleRequest abRequest = abcRequest.assetBundle.LoadAssetAsync<Texture>(textureName);
			while (!abRequest.isDone)
			{
				yield return null;
			}
			Texture t = abRequest.asset as Texture;
			if (!t)
			{
				Debug.LogError("asset bundle error");
				continue;
			}

			targetMat.SetTexture(pair.Key, t);
		}
	}

	private void ApplyTexturePropertyInEditor(Material targetMat, List<RendererDataGroup.StringKeyValue> list, bool hdOrSd)
	{
#if UNITY_EDITOR
		string hdOrSdString = hdOrSd ? "texturehd" : "texturesd";

		foreach (var pair in list)
		{
			string textureName = pair.Value;
			string abName = string.Format("tank/{0}/{1}", hdOrSdString, textureName.Split('_')[0]);

			string[] allAssetsPath = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(abName,
				textureName);
			if (allAssetsPath.Length == 0)
			{
				Debug.LogError("不存在这个bundle: " + abName + " -> " + textureName + "\n缺少资源？需要Auto Set All AssetBundle Name？");
				continue;
			}
			Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(allAssetsPath[0]);
			targetMat.SetTexture(pair.Key, tex);
		}
#endif
	}

	#endregion
}
