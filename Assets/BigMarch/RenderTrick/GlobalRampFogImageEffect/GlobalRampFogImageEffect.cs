#pragma warning disable 3001, 3002, 3003, 3005, 3008, 3009, 3024
using System;
using UnityEngine;

//	[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class GlobalRampFogImageEffect : MonoBehaviour
{
	public float Multiplier = 1;
	public Texture RampTexture;
	public Vector2 StartEnd = new Vector2(0, 50);
	public Shader FogShader = null;
	private Material _fogMaterial = null;

	void Awake()
	{
		FogShader = Shader.Find("Hidden/GlobalRampFog");
	}

	void OnEnable()
	{
		_fogMaterial = new Material(FogShader);
	}

	void OnDisable()
	{
		DestroyImmediate(_fogMaterial);
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		/*Camera cam = GetComponent<Camera>();
		Transform camtr = cam.transform;
		float camNear = cam.nearClipPlane;
		float camFar = cam.farClipPlane;
		float camFov = cam.fieldOfView;
		float camAspect = cam.aspect;

		Matrix4x4 frustumCorners = Matrix4x4.identity;

		float fovWHalf = camFov * 0.5f;

		Vector3 toRight = camtr.right * camNear * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * camAspect;
		Vector3 toTop = camtr.up * camNear * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

		Vector3 topLeft = (camtr.forward * camNear - toRight + toTop);
		float camScale = topLeft.magnitude * camFar / camNear;

		topLeft.Normalize();
		topLeft *= camScale;

		Vector3 topRight = (camtr.forward * camNear + toRight + toTop);
		topRight.Normalize();
		topRight *= camScale;

		Vector3 bottomRight = (camtr.forward * camNear + toRight - toTop);
		bottomRight.Normalize();
		bottomRight *= camScale;

		Vector3 bottomLeft = (camtr.forward * camNear - toRight - toTop);
		bottomLeft.Normalize();
		bottomLeft *= camScale;

		frustumCorners.SetRow(0, topLeft);
		frustumCorners.SetRow(1, topRight);
		frustumCorners.SetRow(2, bottomRight);
		frustumCorners.SetRow(3, bottomLeft);

		var camPos = camtr.position;
		_fogMaterial.SetMatrix("_FrustumCornersWS", frustumCorners);
		_fogMaterial.SetVector("_CameraWS", camPos);*/

		_fogMaterial.SetVector("_DistanceParams", new Vector4(StartEnd.x, StartEnd.y, 0, 0));
		_fogMaterial.SetTexture("_RampTexture", RampTexture);

		_fogMaterial.SetTexture("_DepthTex", GetComponent<BackUpFirstPostEffectSourceRenderTexture>().DepthRt);
		_fogMaterial.SetFloat("_Multiplier", Multiplier);

		Graphics.Blit(source, destination, _fogMaterial);
//		CustomGraphicsBlit(source, destination, _fogMaterial, 0);
	}

//	static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr)
//	{
//		RenderTexture.active = dest;
//
//		fxMaterial.SetTexture("_MainTex", source);
//
//		GL.PushMatrix();
//		GL.LoadOrtho();
//
//		fxMaterial.SetPass(passNr);
//
//		GL.Begin(GL.QUADS);
//
//		GL.MultiTexCoord2(0, 0.0f, 0.0f);
//		GL.Vertex3(0.0f, 0.0f, 3.0f); // BL
//
//		GL.MultiTexCoord2(0, 1.0f, 0.0f);
//		GL.Vertex3(1.0f, 0.0f, 2.0f); // BR
//
//		GL.MultiTexCoord2(0, 1.0f, 1.0f);
//		GL.Vertex3(1.0f, 1.0f, 1.0f); // TR
//
//		GL.MultiTexCoord2(0, 0.0f, 1.0f);
//		GL.Vertex3(0.0f, 1.0f, 0.0f); // TL
//
//		GL.End();
//		GL.PopMatrix();
//	}
}

