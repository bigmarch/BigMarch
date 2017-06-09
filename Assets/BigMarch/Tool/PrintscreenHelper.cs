using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BigMarch.Tool
{
	public class PrintscreenHelper : MonoBehaviour
	{
		public int Width = 1920;
		public int Height = 1080;
		public string FileName = "NewPng";

		public void Printscreen(string path, int width, int height)
		{
			Camera c = GetComponent<Camera>();
			RenderTexture rt = new RenderTexture(width, height, 32);
			c.targetTexture = rt;
			c.Render();
			SaveRenderTextureToPNG(rt, path);
			rt.Release();
			c.targetTexture = null;
		}

		//将RenderTexture保存成一张png图片  
		private static void SaveRenderTextureToPNG(RenderTexture rt, string path)
		{
			RenderTexture prev = RenderTexture.active;
			RenderTexture.active = rt;
			Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
			png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
			byte[] bytes = png.EncodeToPNG();
			FileStream file = File.Open(path, FileMode.Create);
			BinaryWriter writer = new BinaryWriter(file);
			writer.Write(bytes);
			file.Close();
			DestroyImmediate(png);
			RenderTexture.active = prev;

			Debug.Log("printscreen save to : " + path);
		}
	}
}
