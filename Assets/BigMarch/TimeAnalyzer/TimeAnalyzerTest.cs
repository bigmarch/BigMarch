using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeAnalyzerTest : MonoBehaviour
{
	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.I))
		{
			TimeAnalyzerData.StartWork();
		}

		if (Input.GetKeyDown(KeyCode.P))
		{
			TimeAnalyzerData.Send();
		}

		if (Input.GetKeyDown(KeyCode.O))
		{
			TimeAnalyzerData.StopWork();
		}
	}
}