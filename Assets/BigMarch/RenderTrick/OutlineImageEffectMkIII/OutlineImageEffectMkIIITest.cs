using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineImageEffectMkIIITest : MonoBehaviour
{
	public OutlineImageEffectMkIII Outline;
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
			Outline.SetTargetRenderer(TargetSmr);
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Outline.SetTargetRenderer(null);
		}
	}
}
