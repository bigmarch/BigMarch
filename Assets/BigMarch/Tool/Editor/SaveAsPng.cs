using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace BigMarch.Tool
{
	/// <summary>
	/// 把图片，按照unity中的texture import setting的分辨率，来导出png。
	/// </summary>
	public class SaveAsPng : EditorWindow
	{
		private Vector2 _scroll;
		private bool _lock;
		private bool _forceSquare;
		private Object[] _cachedSelection;

		// Add menu named "My Window" to the Window menu
		[MenuItem("BigMarch/SaveAsPng")]
		static void Init()
		{
			SaveAsPng window = (SaveAsPng)GetWindow(typeof(SaveAsPng));
			window.Show();
		}

		private void OnGUI()
		{
			_lock = EditorGUILayout.Toggle("Lock", _lock);
			_forceSquare = EditorGUILayout.Toggle("Force Square", _forceSquare);

			if (GUILayout.Button("SaveAs"))
			{
				Save();
			}

			if (!_lock)
			{
				_cachedSelection = Selection.objects;
			}

			if (_cachedSelection != null)
			{
				_scroll = EditorGUILayout.BeginScrollView(_scroll);
				for (int i = 0; i < _cachedSelection.Length; i++)
				{
					Texture t = _cachedSelection[i] as Texture;
					if (t)
					{
						GUILayout.BeginHorizontal();
						if (GUILayout.Button("S", GUILayout.Width(25)))
						{
							Selection.activeObject = t;
						}
						EditorGUILayout.ObjectField(t, typeof(Texture), false);
						string path = AssetDatabase.GetAssetPath(t);

						TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
						GUILayout.Box(importer.maxTextureSize.ToString());

						GUILayout.Label(path);
						GUILayout.EndHorizontal();
					}
				}
				EditorGUILayout.EndScrollView();
			}
		}

		private void OnSelectionChange()
		{
			Repaint();
		}

		private void Save()
		{
			string saveFolderPath = EditorUtility.SaveFolderPanel("SaveFolder", Application.dataPath.Replace("Assets", ""), "");

			if (string.IsNullOrEmpty(saveFolderPath))
			{
				return;
			}

			for (int i = 0; i < _cachedSelection.Length; i++)
			{
				EditorUtility.DisplayProgressBar("", "Save as png", i * 1f / _cachedSelection.Length);

				Texture2D t = _cachedSelection[i] as Texture2D;
				if (t)
				{
					string path = AssetDatabase.GetAssetPath(t);
					TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
					bool isReadable = importer.isReadable;

					// 缓存format，然后临时改成不压缩，用于导出png。
#if UNITY_5_6_OR_NEWER
					TextureImporterCompression textureCompression = importer.textureCompression;
					importer.textureCompression = TextureImporterCompression.Uncompressed;
#else
					TextureImporterFormat format = importer.textureFormat;
					importer.textureFormat = TextureImporterFormat.RGBA32;
#endif

					importer.isReadable = true;

					importer.SaveAndReimport();

					byte[] bytes;
					if (_forceSquare)
					{
						int max = Mathf.Max(t.height, t.width);

						Texture2D square = Instantiate(t);
						//square.Resize(max, max, TextureFormat.RGBA32, false);
						TextureScale.Bilinear(square, max, max);
						bytes = square.EncodeToPNG();
						DestroyImmediate(square);
					}
					else
					{
						bytes = t.EncodeToPNG();
					}
					File.WriteAllBytes(Path.Combine(saveFolderPath, t.name + ".png"), bytes);


					// 复位format
#if UNITY_5_6_OR_NEWER
					importer.textureCompression = textureCompression;
#else
					importer.textureFormat = format;
#endif
					importer.isReadable = isReadable;

					importer.SaveAndReimport();
				}
			}
			EditorUtility.ClearProgressBar();
		}
	}
}






