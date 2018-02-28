using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineTest : MonoBehaviour
{
	public OutlineImageEffectMkII Outline;
	public Renderer[] TargetSmr;

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Outline.SetTarget(TargetSmr);
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Outline.SetTarget(null);
		}
	}
}
