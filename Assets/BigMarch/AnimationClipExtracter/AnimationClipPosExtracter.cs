using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class AnimationClipPosExtracter : EditorWindow
{
	[MenuItem("BigMarch/Animation Clip Extracter")]
	static void ShowWindow()
	{
		AnimationClipPosExtracter window =
			GetWindow<AnimationClipPosExtracter>(false, "Animation Clip Extracter", true);
		window.minSize = new Vector2(352f, 132f);
	}

	public AnimationClip Clip;

	private Dictionary<string, AnimationCurveVector3> _dic = new Dictionary<string, AnimationCurveVector3>();

	public void OnGUI()
	{
		Clip = (AnimationClip) EditorGUILayout.ObjectField(Clip, typeof(AnimationClip), false);

		if (GUILayout.Button("Parse Clip"))
		{
			RefreshDic();
		}

		foreach (KeyValuePair<string, AnimationCurveVector3> pair in _dic)
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Extract", GUILayout.Width(100)))
			{
				List<Vector3> vecArr = Extract(pair.Value, pair.Value.Length);

				string log = "[";
				for (int i = 0; i < vecArr.Count; i++)
				{
					log += vecArr[i].ToString("0.00");

					if (i != vecArr.Count - 1)
					{
						log += ",";
					}
				}

				log += "]";
				log = log.Replace(" ", "");
				Debug.Log(log);
			}

			GUILayout.Label(pair.Key + ":");
			GUI.enabled = false;
			EditorGUILayout.CurveField(pair.Value.X, GUILayout.Width(100));
			EditorGUILayout.CurveField(pair.Value.Y, GUILayout.Width(100));
			EditorGUILayout.CurveField(pair.Value.Z, GUILayout.Width(100));
			GUI.enabled = true;
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
		}
	}

	private void RefreshDic()
	{
		_dic.Clear();
		EditorCurveBinding[] curveBinding = AnimationUtility.GetCurveBindings(Clip);
		
		foreach (var cb in curveBinding)
		{
			if (cb.propertyName.StartsWith("m_LocalPosition"))
			{
				AnimationCurveVector3 c;
				if (!_dic.TryGetValue(cb.path, out c))
				{
					c = new AnimationCurveVector3();
					_dic.Add(cb.path, c);
				}

				if (cb.propertyName.EndsWith(".x"))
				{
					c.X = AnimationUtility.GetEditorCurve(Clip, cb);
				}
				else if (cb.propertyName.EndsWith(".y"))
				{
					c.Y = AnimationUtility.GetEditorCurve(Clip, cb);
				}
				else if (cb.propertyName.EndsWith(".z"))
				{
					c.Z = AnimationUtility.GetEditorCurve(Clip, cb);
				}

				c.Length = Clip.length;
			}
		}
	}

	private List<Vector3> Extract(AnimationCurveVector3 c, float length)
	{
		List<Vector3> result = new List<Vector3>();
		int frameCount = (int) (length / Time.fixedDeltaTime);

		for (int i = 0; i < frameCount + 1; i++)
		{
			float time = i * Time.fixedDeltaTime;
			float x = c.X.Evaluate(time);
			float y = c.Y.Evaluate(time);
			float z = c.Y.Evaluate(time);
			result.Add(new Vector3(x, y, z));
		}

		return result;
	}

	private class AnimationCurveVector3
	{
		public float Length;
		public AnimationCurve X;
		public AnimationCurve Y;
		public AnimationCurve Z;
	}
}