using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace BigMarch.Tool
{
	[CustomEditor(typeof(PrintscreenHelper))]
	public class PrintscreenHelperEditor : Editor
	{
		private string GetFolderPath()
		{
			return Path.GetDirectoryName(Application.dataPath);
		}

		public override void OnInspectorGUI()
		{
			PrintscreenHelper p = target as PrintscreenHelper;

			DrawDefaultInspector();

			if (GUILayout.Button("Printscreen"))
			{
				int index = 0;
				string savePath = string.Format(
					"{0}/{1}{2}.png",
					GetFolderPath(),
					p.FileName,
					index);

				while (File.Exists(savePath))
				{
					index++;
					savePath = string.Format(
						"{0}/{1}{2}.png",
						GetFolderPath(),
						p.FileName,
						index);
				}

				p.Printscreen(savePath, p.Width, p.Height);
			}
			if (GUILayout.Button("OpenFolder"))
			{
				System.Diagnostics.Process.Start(GetFolderPath());
			}
		}
	}
}
