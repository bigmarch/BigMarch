using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SplatTextureMixer : EditorWindow
{
	[MenuItem("BigMarch/SplatTextureMixer")]
	static void Init()
	{
		SplatTextureMixer window = (SplatTextureMixer)GetWindow(typeof(SplatTextureMixer));
		window.Show();
	}

	public bool IsNormal;

	public Texture MainTexture;

	public Texture Splat0;
	public float Tiling0 = 1;
	public Texture Splat1;
	public float Tiling1 = 1;
	public Texture Splat2;
	public float Tiling2 = 1;
	public Texture Splat3;
	public float Tiling3 = 1;

	void OnGUI()
	{
		IsNormal = EditorGUILayout.Toggle("IsNormal", IsNormal);
		if (GUILayout.Button(IsNormal ? "MixNormal!" : "Mix!", GUILayout.Height(25)))
		{
			Mix();
		}


		GUILayout.Space(30);
		MainTexture = (Texture) EditorGUILayout.ObjectField("MainTexture: ", MainTexture, typeof(Texture), false);
		GUILayout.Space(20);
		Splat0 = (Texture) EditorGUILayout.ObjectField("(R)Splat0: ", Splat0, typeof(Texture), false);
		Tiling0 = EditorGUILayout.FloatField("(R)Tiling0: ", Tiling0);
		GUILayout.Space(20);
		Splat1 = (Texture) EditorGUILayout.ObjectField("(G)Splat1: ", Splat1, typeof(Texture), false);
		Tiling1 = EditorGUILayout.FloatField("(G)Tiling1: ", Tiling1);
		GUILayout.Space(20);
		Splat2 = (Texture) EditorGUILayout.ObjectField("(B)Splat2: ", Splat2, typeof(Texture), false);
		Tiling2 = EditorGUILayout.FloatField("(B)Tiling2: ", Tiling2);
		GUILayout.Space(20);
		Splat3 = (Texture) EditorGUILayout.ObjectField("(A)Splat3: ", Splat3, typeof(Texture), false);
		Tiling3 = EditorGUILayout.FloatField("(A)Tiling3: ", Tiling3);
	}

	private void Mix()
	{
		RenderTexture rt = new RenderTexture(MainTexture.width, MainTexture.height, 0);

		Shader shader = Shader.Find("Hidden/SplatTextureMixer");
		if (!shader)
		{
			Debug.LogError("Hidden/SplatTextureMixer 不存在。");
			return;
		}

		Material mat = new Material(shader);
		mat.hideFlags = HideFlags.DontSave;

		mat.SetTexture("_Splat0", Splat0);
		mat.SetTexture("_Splat1", Splat1);
		mat.SetTexture("_Splat2", Splat2);
		mat.SetTexture("_Splat3", Splat3);

		mat.SetFloat("_Tiling0", Tiling0);
		mat.SetFloat("_Tiling1", Tiling1);
		mat.SetFloat("_Tiling2", Tiling2);
		mat.SetFloat("_Tiling3", Tiling3);

		if (IsNormal)
		{
			mat.EnableKeyword("_NORMAL");
		}
		else
		{
			mat.DisableKeyword("_NORMAL");
		}

		Graphics.Blit(MainTexture, rt, mat);

		string mainTexturePath = AssetDatabase.GetAssetPath(MainTexture);
		string resultPath = Path.GetDirectoryName(mainTexturePath) + "/" + Path.GetFileNameWithoutExtension(mainTexturePath) +
		                    (IsNormal ? "(mix_normal).tga" : "(mix).tga");

		SaveRenderTextureToTGA(rt, resultPath);

		AssetDatabase.Refresh();
	}

	private static void SaveRenderTextureToTGA(RenderTexture rt, string path)
	{
		RenderTexture prev = RenderTexture.active;
		RenderTexture.active = rt;
		Texture2D tga = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
		tga.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
		byte[] bytes = tga.EncodeToTGA();
		FileStream file = File.Open(path, FileMode.Create);
		BinaryWriter writer = new BinaryWriter(file);
		writer.Write(bytes);
		file.Close();
		DestroyImmediate(tga);
		RenderTexture.active = prev;

		Debug.Log("printscreen save to : " + path);
	}
}
