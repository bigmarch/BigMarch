using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookChain : MonoBehaviour
{
	public Vector2 UvSpeed = Vector2.zero;
	public Vector3 StartPoint = new Vector3(0, 0, 10);
	public Vector3 EndPoint = new Vector3(0, 0, 0);

	private LineRenderer _lr;

	void OnEnable()
	{
		_lr = GetComponentInChildren<LineRenderer>();
	}

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		if (_lr)
		{
			_lr.SetPosition(0, StartPoint);
			_lr.SetPosition(1, EndPoint);

			if (UvSpeed != Vector2.zero)
			{
				_lr.material.SetTextureOffset("_MainTex", UvSpeed * Time.time);
			}
		}
	}
}
