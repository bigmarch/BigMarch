using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TlDecal : MonoBehaviour
{
	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
	}

	private void OnEnable()
	{
		Camera.main.depthTextureMode |= DepthTextureMode.Depth;
	}

	private void OnPreRender()
	{
		Camera c = Camera.current;

		MeshRenderer mr = GetComponent<MeshRenderer>();
		Matrix4x4 inverseVp = (c.projectionMatrix * c.worldToCameraMatrix).inverse;
		mr.material.SetMatrix("_CurrentInverseVP", inverseVp);
	}
}