using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BigMarch.Tool
{
	public class TextureFormatConvert : EditorWindow
	{
		private Texture2D _target;
		// Add menu named "My Window" to the Window menu
		[MenuItem("BigMarch/TextureFormatConvert")]
		static void Init()
		{
			TextureFormatConvert window = (TextureFormatConvert) GetWindow(typeof(TextureFormatConvert));
			window.Show();
		}


		private void OnGUI()
		{
			_target = (Texture2D) EditorGUILayout.ObjectField(_target, typeof(Texture2D), false);

			if (_target)
			{
				if (GUILayout.Button("Save Png", GUILayout.Width(200), GUILayout.Height(50)))
				{
					SaveByte(_target.EncodeToPNG(), _target.name + ".png");
				}
				if (GUILayout.Button("Save Tga", GUILayout.Width(200), GUILayout.Height(50)))
				{
					SaveByte(_target.EncodeToTGA(), _target.name + ".tga");
				}
				if (GUILayout.Button("Save Jpg", GUILayout.Width(200), GUILayout.Height(50)))
				{
					SaveByte(_target.EncodeToJPG(), _target.name + ".jpg");
				}
			}
		}

		private void SaveByte(byte[] bytes, string defaultFileName)
		{
			string extension = Path.GetExtension(defaultFileName);
			string saveFolderPath = EditorUtility.SaveFilePanel(
				"SaveFolder",
				Application.dataPath.Replace("Assets", ""),
				defaultFileName,
				extension);

			if (!string.IsNullOrEmpty(saveFolderPath))
			{
				File.WriteAllBytes(saveFolderPath, bytes);
			}
		}
	}
}
