using UnityEditor;
using UnityEngine;

namespace BigMarch.Swizzle
{
	public class SwizzleGenerator : EditorWindow
	{
		private string _codeText;
		private Vector2 _scroll;
		// Add menu named "My Window" to the Window menu
		[MenuItem("BigMarch/SwizzleGenerator")]
		static void Init()
		{
			SwizzleGenerator window = (SwizzleGenerator) GetWindow(typeof(SwizzleGenerator));
			window.Show();
		}

		void OnGUI()
		{
			if (GUILayout.Button("Generate & Copy Swizzle Code"))
			{
				_codeText = GenerateSwizzleCode();
				EditorGUIUtility.systemCopyBuffer = _codeText;

				EditorUtility.DisplayDialog("Complete", "Code has been copied.", "Go paste");
			}

			_scroll = EditorGUILayout.BeginScrollView(_scroll);
			GUILayout.Box(_codeText);
			EditorGUILayout.EndScrollView();
		}

		private string GenerateSwizzleCode()
		{
			string result = "";

			result += GenerateVector2();
			result += GenerateVector3();
			result += GenerateVector4();
			result += GenerateColor();
			result += GenerateColor32();

			return result;
		}

		private string GenerateVector2()
		{
			string[] components = {"x", "y", "0", "1"};
			string result = "";

			for (int x = 0; x < components.Length; x++)
			{
				string componentX = components[x];

				for (int y = 0; y < components.Length; y++)
				{
					string componentY = components[y];

					string methond = string.Format(
						"public static {0} Swizzle_{1}{2}(this {0} v)" +
						"{{return new {0}(v.{1}, v.{2});}}\n",
						"Vector2",
						componentX,
						componentY);

					methond = methond.Replace("v.0", "0");
					methond = methond.Replace("v.1", "1");

					result += methond;
				}
			}

			return result;
		}

		private string GenerateVector3()
		{
			string[] components = {"x", "y", "z", "0", "1" };
			string result = "";

			for (int x = 0; x < components.Length; x++)
			{
				string componentX = components[x];

				for (int y = 0; y < components.Length; y++)
				{
					string componentY = components[y];

					for (int z = 0; z < components.Length; z++)
					{
						string componentZ = components[z];

						string methond = string.Format(
							"public static {0} Swizzle_{1}{2}{3}(this {0} v)" +
							"{{return new {0}(v.{1}, v.{2}, v.{3});}}\n",
							"Vector3",
							componentX,
							componentY,
							componentZ);

						methond = methond.Replace("v.0", "0");
						methond = methond.Replace("v.1", "1");

						result += methond;
					}
				}
			}

			return result;
		}

		private string GenerateVector4()
		{
			string[] components = {"x", "y", "z", "w", "0", "1" };
			string result = "";

			for (int x = 0; x < components.Length; x++)
			{
				string componentX = components[x];

				for (int y = 0; y < components.Length; y++)
				{
					string componentY = components[y];

					for (int z = 0; z < components.Length; z++)
					{
						string componentZ = components[z];

						for (int w = 0; w < components.Length; w++)
						{
							string componentW = components[w];

							string methond = string.Format(
								"public static {0} Swizzle_{1}{2}{3}{4}(this {0} v)" +
								"{{return new {0}(v.{1}, v.{2}, v.{3}, v.{4});}}\n",
								"Vector4",
								componentX,
								componentY,
								componentZ,
								componentW);

							methond = methond.Replace("v.0", "0");
							methond = methond.Replace("v.1", "1");

							result += methond;
						}
					}
				}
			}

			return result;
		}

		private string GenerateColor()
		{
			string[] components = {"r", "g", "b", "a", "0", "1" };
			string result = "";

			for (int x = 0; x < components.Length; x++)
			{
				string componentX = components[x];

				for (int y = 0; y < components.Length; y++)
				{
					string componentY = components[y];

					for (int z = 0; z < components.Length; z++)
					{
						string componentZ = components[z];

						for (int w = 0; w < components.Length; w++)
						{
							string componentW = components[w];

							string methond = string.Format(
								"public static {0} Swizzle_{1}{2}{3}{4}(this {0} v)" +
								"{{return new {0}(v.{1}, v.{2}, v.{3}, v.{4});}}\n",
								"Color",
								componentX,
								componentY,
								componentZ,
								componentW);

							methond = methond.Replace("v.0", "0");
							methond = methond.Replace("v.1", "1");

							result += methond;
						}
					}
				}
			}
			return result;
		}

		private string GenerateColor32()
		{
			string[] components = {"r", "g", "b", "a", "0", "1"};
			string result = "";

			for (int x = 0; x < components.Length; x++)
			{
				string componentX = components[x];

				for (int y = 0; y < components.Length; y++)
				{
					string componentY = components[y];

					for (int z = 0; z < components.Length; z++)
					{
						string componentZ = components[z];

						for (int w = 0; w < components.Length; w++)
						{
							string componentW = components[w];

							string methond = string.Format(
								"public static {0} Swizzle_{1}{2}{3}{4}(this {0} v)" +
								"{{return new {0}(v.{1}, v.{2}, v.{3}, v.{4});}}\n",
								"Color32",
								componentX,
								componentY,
								componentZ,
								componentW);

							methond = methond.Replace("v.0", "0");
							methond = methond.Replace("v.1", "255");

							result += methond;
						}
					}
				}
			}
			return result;
		}
	}
}


