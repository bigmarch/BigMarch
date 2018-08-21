using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMaterial : MonoBehaviour
{
	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		GetComponent<Renderer>().material.color = (Mathf.Sin(Time.time) * 0.5f + 0.5f) * Color.white;
	}
}