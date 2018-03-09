using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameBlendImageEffectExample : MonoBehaviour
{
	public int RealFps = 60;
	public Transform MoveModel;

	// Update is called once per frame
	void Update()
	{
		MoveModel.transform.position = Vector3.right * Mathf.Sin(5 * Time.time) * 4;
		Application.targetFrameRate = RealFps;
		QualitySettings.vSyncCount = 0;
	}
}
