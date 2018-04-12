using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

// 应用 render data 的数据。
// 注意，以下代码并未做过测试，只是表达 RenderDataGroup 的思想，分为 editor 下和 runtime，在这里做备忘。
// 根据不同项目的使用情况，需要对这个类里的逻辑进行修改。
public static class RenderDataGroupApply
{
	#region

	// editor 下使用的方法，同步加载
	public static Dictionary<string, Material> CreateMaterial_Editor(RenderDataGroup rdg, bool hdOrSd)
	{
		Dictionary<string, Material> result = new Dictionary<string, Material>();

		foreach (RenderDataGroup.RendererData rd in rdg.RendererDataArr)
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
	public static IEnumerator CreateMaterial_Runtime(
		Dictionary<string, Material> result,
		RenderDataGroup rdg,
		bool hdOrSd,
		MonoBehaviour coroutineOwner)
	{
		foreach (RenderDataGroup.RendererData rd in rdg.RendererDataArr)
		{
			Material mat = new Material(Shader.Find(rd.ShaderName));
			ApplyFloatProperty(mat, rd.FloatProperty);
			ApplyColorProperty(mat, rd.ColorProperty);
			yield return coroutineOwner.StartCoroutine(ApplyTexturePropertyInRuntime(mat, rd.TextureProperty, hdOrSd));
			mat.shaderKeywords = rd.Keywords;

			result.Add(rd.TargetRendererName, mat);
		}
	}

	private static void ApplyFloatProperty(Material targetMat, List<RenderDataGroup.FloatKeyValue> list)
	{
		foreach (var pair in list)
		{
			targetMat.SetFloat(pair.Key, pair.Value);
		}
	}

	private static void ApplyColorProperty(Material targetMat, List<RenderDataGroup.ColorKeyValue> list)
	{
		foreach (var pair in list)
		{
			targetMat.SetColor(pair.Key, pair.Value);
		}
	}

	// 该方法没有经过测试，可能好使，可能不好使，只是表达思想，备忘。
	private static IEnumerator ApplyTexturePropertyInRuntime(
		Material targetMat,
		List<RenderDataGroup.StringKeyValue> list,
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

	private static void ApplyTexturePropertyInEditor(Material targetMat, List<RenderDataGroup.StringKeyValue> list,
		bool hdOrSd)
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
