using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class GlobalRampFogImageEffect : MonoBehaviour
{
	public Shader FogShader;
	private Material _fogMaterial;

	[Header("Distance Fog")] public bool DistanceFog = true;
	public float DistanceFogMultiplier = 1;
	public Texture DistanceRampTexture;
	public Vector2 DistanceStartEnd = new Vector2(10, 40);

	[Header("Height Fog")] public bool HeightFog = true;
	public float HeightFogMultiplier = 1;
	public Texture HeightRampTexture;
	public Vector2 HeightStartEnd = new Vector2(10, 0);

	void Awake()
	{
		FogShader = Shader.Find("Hidden/GlobalRampFog");
	}

	void OnEnable()
	{
		_fogMaterial = new Material(FogShader);

		CheckRampTexture(DistanceRampTexture);
		CheckRampTexture(HeightRampTexture);
	}

	private void CheckRampTexture(Texture t)
	{
#if UNITY_EDITOR
		if (!t)
		{
			return;
		}
		string path = AssetDatabase.GetAssetPath(t);
		TextureImporter ti = (TextureImporter) AssetImporter.GetAtPath(path);
		if (ti.mipmapEnabled)
		{
			Debug.LogError("mipmap 必须关闭 " + path);
		}

		if (ti.filterMode == FilterMode.Point)
		{
			Debug.LogError("filterMode 建议别用 point " + path);
		}
#endif
	}

	void OnDisable()
	{
		DestroyImmediate(_fogMaterial);
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Camera cam = GetComponent<Camera>();
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
		_fogMaterial.SetVector("_CameraWS", camPos);

		_fogMaterial.SetVector("_DistanceParams",
			new Vector4(DistanceStartEnd.x, DistanceStartEnd.y, HeightStartEnd.x, HeightStartEnd.y));

		_fogMaterial.SetTexture("_RampTexture0", DistanceRampTexture);
		_fogMaterial.SetTexture("_RampTexture1", HeightRampTexture);

		_fogMaterial.SetTexture("_DepthTex", GetComponent<BackUpFirstPostEffectSourceRenderTexture>().DepthRt);

		_fogMaterial.SetFloat("_DistanceFogMultiplier", DistanceFogMultiplier);

		_fogMaterial.SetFloat("_HeightFogMultiplier", HeightFogMultiplier);

		if (DistanceFog)
		{
			_fogMaterial.EnableKeyword("_DISTANCE");
		}
		else
		{
			_fogMaterial.DisableKeyword("_DISTANCE");
		}

		if (HeightFog)
		{
			_fogMaterial.EnableKeyword("_HEIGHT");
		}
		else
		{
			_fogMaterial.DisableKeyword("_HEIGHT");
		}

		CustomGraphicsBlit(source, destination, _fogMaterial, 0);
	}

	static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr)
	{
		RenderTexture.active = dest;

		fxMaterial.SetTexture("_MainTex", source);

		GL.PushMatrix();
		GL.LoadOrtho();

		fxMaterial.SetPass(passNr);

		GL.Begin(GL.QUADS);

		GL.MultiTexCoord2(0, 0.0f, 0.0f);
		GL.Vertex3(0.0f, 0.0f, 3.0f); // BL

		GL.MultiTexCoord2(0, 1.0f, 0.0f);
		GL.Vertex3(1.0f, 0.0f, 2.0f); // BR

		GL.MultiTexCoord2(0, 1.0f, 1.0f);
		GL.Vertex3(1.0f, 1.0f, 1.0f); // TR

		GL.MultiTexCoord2(0, 0.0f, 1.0f);
		GL.Vertex3(0.0f, 1.0f, 0.0f); // TL

		GL.End();
		GL.PopMatrix();
	}
}

