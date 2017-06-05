using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class EditorUtility
{
	public static void GetTextureSourceFileResolution(TextureImporter importer, out int width, out int height)
	{
		object[] args = new object[2] {0, 0};
		MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight",
			BindingFlags.NonPublic | BindingFlags.Instance);
		mi.Invoke(importer, args);

		width = (int) args[0];
		height = (int) args[1];
	}
}