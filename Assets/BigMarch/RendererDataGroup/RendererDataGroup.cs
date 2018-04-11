using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "Renderer Data Group.asset", menuName = "Renderer Data Group")]
public class RendererDataGroup : ScriptableObject
{
	public List<RendererData> RendererDataArr;

	[System.Serializable]
	public class RendererData
	{
		public string TargetRendererName;
		public string ShaderName;
		public string[] Keywords;
		public List<StringKeyValue> TextureProperty;
		public List<FloatKeyValue> FloatProperty;
		public List<ColorKeyValue> ColorProperty;

		public RendererData()
		{
			TextureProperty = new List<StringKeyValue>();
			FloatProperty = new List<FloatKeyValue>();
			ColorProperty = new List<ColorKeyValue>();
		}
	}

	[System.Serializable]
	public class StringKeyValue
	{
		public StringKeyValue(string key, string value)
		{
			Key = key;
			Value = value;
		}

		public string Key;
		public string Value;
	}

	[System.Serializable]
	public class FloatKeyValue
	{
		public FloatKeyValue(string key, float value)
		{
			Key = key;
			Value = value;
		}

		public string Key;
		public float Value;
	}

	[System.Serializable]
	public class ColorKeyValue
	{
		public ColorKeyValue(string key, Color value)
		{
			Key = key;
			Value = value;
		}

		public string Key;
		public Color Value;
	}

#if UNITY_EDITOR
	void Reset()
	{
		string tankName = "ii";

		RendererDataArr = new List<RendererData>();

		#region attach

		RendererData attachRd = new RendererData();
		string attach = "attach";
		attachRd.TargetRendererName = attach;
		attachRd.ShaderName = "Tank/Attach";
		attachRd.TextureProperty.Add(
			new StringKeyValue("_MainTex", string.Format("{0}_{1}_{2}", tankName, attach, "d")));
		attachRd.FloatProperty.Add(new FloatKeyValue("_Metallic", 0.5f));
		attachRd.FloatProperty.Add(new FloatKeyValue("_Smoothness", 1));

		#endregion

		#region body

		RendererData bodyRd = new RendererData();
		string body = "body";
		bodyRd.TargetRendererName = body;

		bodyRd.ShaderName = "Tank/Body";

		bodyRd.TextureProperty.Add(new StringKeyValue("_Splat0", string.Format("{0}_{1}_{2}", "common", "splat", "paint01")));
		bodyRd.TextureProperty.Add(new StringKeyValue("_NormalMap0",
			string.Format("{0}_{1}_{2}", "common", "splat", "paint01n")));
		bodyRd.FloatProperty.Add(new FloatKeyValue("_Tiling0", 6));
		bodyRd.FloatProperty.Add(new FloatKeyValue("_Metallic0", .65f));
		bodyRd.FloatProperty.Add(new FloatKeyValue("_Smoothness0", .8f));

		bodyRd.TextureProperty.Add(new StringKeyValue("_Splat1", string.Format("{0}_{1}_{2}", "common", "splat", "rust01")));
		bodyRd.FloatProperty.Add(new FloatKeyValue("_Tiling1", 8));
		bodyRd.FloatProperty.Add(new FloatKeyValue("_Metallic1", .7f));
		bodyRd.FloatProperty.Add(new FloatKeyValue("_Smoothness1", .8f));

		bodyRd.TextureProperty.Add(new StringKeyValue("_Splat2", string.Format("{0}_{1}_{2}", "common", "splat", "metal01")));
		bodyRd.FloatProperty.Add(new FloatKeyValue("_Tiling2", 7));
		bodyRd.FloatProperty.Add(new FloatKeyValue("_Metallic2", .9f));
		bodyRd.FloatProperty.Add(new FloatKeyValue("_Smoothness2", 1));

		bodyRd.TextureProperty.Add(new StringKeyValue("_Splat3", string.Format("{0}_{1}_{2}", "common", "splat", "dirt01")));
		bodyRd.FloatProperty.Add(new FloatKeyValue("_Tiling3", 8));
		bodyRd.FloatProperty.Add(new FloatKeyValue("_Metallic3", 0));
		bodyRd.FloatProperty.Add(new FloatKeyValue("_Smoothness3", .5f));

		bodyRd.TextureProperty.Add(new StringKeyValue("_MainTex", string.Format("{0}_{1}_{2}", tankName, body, "m")));
		bodyRd.TextureProperty.Add(new StringKeyValue("_Ao", string.Format("{0}_{1}_{2}", tankName, body, "a")));

		#endregion

		#region track

		RendererData trackRd = new RendererData();
		string track = "track";
		trackRd.TargetRendererName = track;

		trackRd.ShaderName = "Tank/Track";

		trackRd.TextureProperty.Add(new StringKeyValue("_MainTex",
			string.Format("{0}_{1}_{2}", tankName, track, "d")));
		trackRd.TextureProperty.Add(new StringKeyValue("_Normal",
			string.Format("{0}_{1}_{2}", tankName, track, "n")));
		trackRd.TextureProperty.Add(new StringKeyValue("_MainTex",
			string.Format("{0}_{1}_{2}", tankName, track, "d")));

		#endregion

		#region wheel

		RendererData wheelRd = new RendererData();
		string wheel = "wheel";
		wheelRd.TargetRendererName = wheel;

		wheelRd.ShaderName = "Tank/Wheel";
		wheelRd.TextureProperty.Add(
			new StringKeyValue("_Mask", string.Format("{0}_{1}_{2}", tankName, wheel, "m")));
		wheelRd.TextureProperty.Add(
			new StringKeyValue("_Paint", string.Format("{0}_{1}_{2}", "common", "splat", "paint01")));
		wheelRd.FloatProperty.Add(new FloatKeyValue("_PaintTiling", 1));
		wheelRd.TextureProperty.Add(
			new StringKeyValue("_MainTex", string.Format("{0}_{1}_{2}", tankName, wheel, "d")));

		wheelRd.TextureProperty.Add(new StringKeyValue("_Normal",
			string.Format("{0}_{1}_{2}", tankName, wheel, "n")));
		wheelRd.FloatProperty.Add(new FloatKeyValue("_Metallic", 0.5f));
		wheelRd.FloatProperty.Add(new FloatKeyValue("_Smoothness", 1));

		#endregion

		#region shadow

		RendererData shadowRd = new RendererData();
		string shadow = "shadow";
		shadowRd.TargetRendererName = shadow;
		shadowRd.ShaderName = "Tank/Shadow";
		shadowRd.TextureProperty.Add(new StringKeyValue("_MainTex",
			string.Format("{0}_{1}_{2}", tankName, shadow, "d")));
		shadowRd.FloatProperty.Add(new FloatKeyValue("_Strength", 0.6f));
		shadowRd.FloatProperty.Add(new FloatKeyValue("_Power", 0.5f));

		#endregion

		RendererDataArr.Add(attachRd);
		RendererDataArr.Add(bodyRd);
		RendererDataArr.Add(trackRd);
		RendererDataArr.Add(wheelRd);
		RendererDataArr.Add(shadowRd);
	}
#endif

	public string CheckError()
	{
		if (string.IsNullOrEmpty(name))
		{
			return "RendererDataGroup name 为空！";
		}

		foreach (var rendererData in RendererDataArr)
		{
			if (string.IsNullOrEmpty(rendererData.ShaderName) || !Shader.Find(rendererData.ShaderName))
			{
				return "Attach ShaderName 有误！";
			}
			if (string.IsNullOrEmpty(rendererData.ShaderName) || !Shader.Find(rendererData.ShaderName))
			{
				return " Body ShaderName 有误！";
			}
			// track 不处理，坦克可能没有track。
			/*if (string.IsNullOrEmpty(Track.ShaderName) || !Shader.Find(Attach.ShaderName))
			{
				return "Track ShaderName 有误！";
			}*/
			if (string.IsNullOrEmpty(rendererData.ShaderName) || !Shader.Find(rendererData.ShaderName))
			{
				return "Wheel ShaderName 有误！";
			}
			if (string.IsNullOrEmpty(rendererData.ShaderName) || !Shader.Find(rendererData.ShaderName))
			{
				return "Shadow ShaderName 有误！";
			}
		}

		return "";
	}
}
