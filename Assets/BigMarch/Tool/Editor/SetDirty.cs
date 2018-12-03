using UnityEditor;

public class SetDirty
{
	[MenuItem("BigMarch/SetDirty")]
	public static void Do()
	{
		var all = Selection.objects;
		foreach (var o in all)
		{
			EditorUtility.SetDirty(o);
		}

		AssetDatabase.SaveAssets();
	}
}