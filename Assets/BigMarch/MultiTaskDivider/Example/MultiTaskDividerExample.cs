using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiTaskDividerExample : MonoBehaviour
{
	public bool UseMultiTaskDivider = false;

	public Transform[] RayCastPointArr;

	private MultiTaskDivider _multiTaskDivider = new MultiTaskDivider();
	private Dictionary<string, bool> _castResultDic = new Dictionary<string, bool>();

	// Update is called once per frame
	void Update()
	{
		Debug.Log(Time.frameCount + "  frame start--------------------------------------------------");
		if (UseMultiTaskDivider)
		{
			Update_UseMultiTaskDivider();
		}
		else
		{
			Update_NotUseMultiTaskDivider();
		}
	}

	private void Update_UseMultiTaskDivider()
	{
		foreach (Transform t0 in RayCastPointArr)
		{
			foreach (Transform t1 in RayCastPointArr)
			{
				if (t0 == t1)
				{
					continue;
				}
				DoAnything(t0, t1);
			}
		}
	}

	private void Update_NotUseMultiTaskDivider()
	{
		foreach (Transform t0 in RayCastPointArr)
		{
			foreach (Transform t1 in RayCastPointArr)
			{
				if (t0 == t1)
				{
					continue;
				}

				var t0_copy = t0;
				var t1_copy = t1;
				_multiTaskDivider.AddTaskIfNotExist(
					t0.name + "-" + t1.name,
					() => { DoAnything(t0_copy, t1_copy); });
			}
		}
		_multiTaskDivider.ExecuteOne();
	}

	private void DoAnything(Transform t0, Transform t1)
	{
		Debug.DrawLine(t0.position, t1.position, Color.yellow);
		Debug.Log("Draw line " + t0.name + " " + t1.name);

		bool cast = Physics.Linecast(t0.position, t1.position);
		string key = t0.name + " " + t1.name;
		_castResultDic[key] = cast;
	}
}
