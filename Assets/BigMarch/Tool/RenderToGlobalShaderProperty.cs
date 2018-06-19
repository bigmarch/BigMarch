using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BigMarch.Tool
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class RenderToGlobalShaderProperty : MonoBehaviour
	{
		public Camera CopyFrom;
		public LayerMask LayerMask;
//		public int RtWith = 1024;
//		public int RtHeight = 768;
		public string GlobaleShaderProperty = "_BackgroundTexture";

		private Camera _cam;
		private RenderTexture _rt;

		void Awake()
		{
			_cam = GetComponent<Camera>();
			_cam.enabled = false;

			_rt = new RenderTexture(Screen.width, Screen.height, 8);
		}

		// Use this for initialization
		void Start()
		{
		}

		void LateUpdate()
		{
			if (!_cam)
			{
				_cam = GetComponent<Camera>();
				_cam.enabled = false;
			}

			if (!_rt || _rt.width != Screen.width || _rt.height != Screen.height)
			{
				_rt = new RenderTexture(Screen.width, Screen.height, 8);
			}

			_cam.CopyFrom(CopyFrom);
			_cam.cullingMask = LayerMask;
			_cam.targetTexture = _rt;
			_cam.Render();

			Shader.SetGlobalTexture("_BackgroundTexture", _rt);
		}

		// Update is called once per frame
		void OnPreRender()
		{
		}
	}
}
