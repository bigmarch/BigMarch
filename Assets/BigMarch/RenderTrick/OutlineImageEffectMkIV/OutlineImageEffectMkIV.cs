using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class OutlineImageEffectMkIV : MonoBehaviour
{
	private Renderer[] _expandRenderer;
	private Renderer[] _clipRenderer;

	public Color CurrentLineColor = Color.black;

	public Material OutlineMat;

	private bool _enableOutline = false;

	private Camera _camera;

	public void Awake()
	{
		_camera = GetComponent<Camera>();
		OutlineMat = new Material(Shader.Find("Hidden/VertexOutlineMkIV"));
		OutlineMat.hideFlags = HideFlags.DontSave;
	}

	public void Update()
	{
		OutlineMat.SetColor("_LineColor", CurrentLineColor);
	}

	private void SetEnableOutline(bool enable, Renderer[] expandRenderer = null, Renderer[] clipRenderer = null)
	{
		// 如果 enable，那么更新 command buffer
		if (enable)
		{
			if (expandRenderer != _expandRenderer || clipRenderer != _clipRenderer)
			{
				// outline mat，不等于2，就通过测试。
				SetStencilProperty(OutlineMat, 2, CompareFunction.NotEqual);

				// reset 当前 _allRenderer 的 stencil，还原他们的 stencil 状态。
				SetStencilProperty(_clipRenderer, 0, CompareFunction.Always);

				_expandRenderer = expandRenderer;
				_clipRenderer = clipRenderer;

				// 新的 all renderer，永远通过测试，并且会 replace。
				SetStencilProperty(_clipRenderer, 2, CompareFunction.Always, StencilOp.Replace);

				_camera.RemoveAllCommandBuffers();

				CommandBuffer cb = new CommandBuffer {name = "outline"};

				foreach (var rend in _expandRenderer)
				{
					if (rend.gameObject.activeSelf && rend.enabled)
					{
						cb.DrawRenderer(rend, OutlineMat);
					}
				}

				_camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cb);
			}
		}
		else
		{
			// 如果尝试关闭，则检查当前状态，如果是第一次关闭，那么清空 command buffer。
			if (_enableOutline)
			{
				_camera.RemoveCommandBuffers(CameraEvent.AfterForwardOpaque);

				// reset _allRenderer 的 stencil，永远不会通过测试。
				SetStencilProperty(_clipRenderer, 0, CompareFunction.Always);

				_expandRenderer = null;
				_clipRenderer = null;
			}
		}

		_enableOutline = enable;
	}

//		[ContextMenu("SetEnableOutlineTrue")]
//		private void SetEnableOutlineTrue()
//		{
//			SetEnableOutline(true);
//		}
//
//		[ContextMenu("SetEnableOutlineFalse")]
//		private void SetEnableOutlineFalse()
//		{
//			SetEnableOutline(false);
//		}

	public void SetTargetRenderer(Renderer[] expandRenderer, Renderer[] clipRenderer)
	{
		if (expandRenderer == null || expandRenderer.Length == 0 || expandRenderer[0] == null)
		{
			SetEnableOutline(false);

			return;
		}

		//记录目标中所有的renderer以及对应的layer。				
		SetEnableOutline(true, expandRenderer, clipRenderer);
	}


	private static void SetStencilProperty(
		Renderer[] renderers,
		int stencilRef,
		CompareFunction stencilComp,
		StencilOp stencilPass = StencilOp.Keep,
		StencilOp stencilZFail = StencilOp.Keep)
	{
		if (renderers == null)
		{
			return;
		}

		for (int i = 0; i < renderers.Length; i++)
		{
			if (renderers[i] != null && renderers[i].sharedMaterial != null)
			{
				SetStencilProperty(renderers[i].sharedMaterial, stencilRef, stencilComp, stencilPass, stencilZFail);
			}
		}
	}

	private static void SetStencilProperty(
		Material mat,
		int stencilRef,
		CompareFunction stencilComp,
		StencilOp stencilPass = StencilOp.Keep,
		StencilOp stencilZFail = StencilOp.Keep)
	{
		mat.SetInt("_StencilRef", stencilRef);
		mat.SetInt("_StencilComp", (int) stencilComp);
		mat.SetInt("_StencilPass", (int) stencilPass);
		mat.SetInt("_StencilZFail", (int) stencilZFail);
	}
}