using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

namespace BigMarch.UIGlow
{
	public class UIGlowEffectCamera : MonoBehaviour
	{
		private static UIGlowEffectCamera _instance;

		public static UIGlowEffectCamera Instance
		{
			get
			{
				if (!_instance)
				{
					GameObject go = new GameObject("UIGlowEffectCamera");
					_instance = go.AddComponent<UIGlowEffectCamera>();
					_instance.Init();
				}
				return _instance;
			}
		}

		private Mesh _mesh;
		private Material _matTakePhoto;
		private Texture _tex;
		private RenderTexture _rt;

		private CommandBuffer _cb;
		private Camera _cam;

		//模特
		private Renderer _modelRenderer;
		private MeshFilter _modelFilter;

		private int _blurIterations;
		private Material _blurMaterial;

		private float _strength;

		private void Init()
		{
			//初始化成员变量
			_matTakePhoto = new Material(Shader.Find("Hidden/UITextToImage"));
			_cb = new CommandBuffer();
			_cam = gameObject.AddComponent<Camera>();
			_cam.clearFlags = CameraClearFlags.Nothing;
			_cam.cullingMask = 0;
			_cam.orthographic = true;
			_cam.orthographicSize = 100;
			_cam.enabled = false;
			_cam.nearClipPlane = -10;
			_cam.farClipPlane = 10;

			//创建model
			GameObject goModel = new GameObject("Model");
			Transform tModel = goModel.transform;
			tModel.parent = transform;
			tModel.localPosition = Vector3.forward;
			tModel.localScale = Vector3.one;

			_modelRenderer = goModel.AddComponent<MeshRenderer>();
			_modelFilter = goModel.AddComponent<MeshFilter>();

			_blurMaterial = new Material(Shader.Find("Hidden/UIGlowStretch"));
		}

		/// <summary>
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="tex"></param>
		/// <param name="expectedWidth">这个rt将被按照这个高度来用，需要参照这个值来计算rt大小。这个值肯定会比mesh的高大。</param>
		/// <param name="downSample"></param>
		/// <param name="blurIterations"></param>
		/// <param name="strength"></param>
		/// <returns></returns>
		public RenderTexture CreateGlowRenderTexture(
			Mesh mesh,
			Texture tex,
			float expectedWidth,
			int downSample,
			int blurIterations,
			float strength
		)
		{
			_mesh = mesh;
			_tex = tex;
			_modelFilter.mesh = _mesh;

			//mesh bound大小
			int meshWidth = (int) mesh.bounds.size.x;
			int meshHeight = (int) mesh.bounds.size.y;

			//rt的预期大小。
			int rtWidthInt = (int) expectedWidth;
			int delta = (rtWidthInt - meshWidth) / 2;
			int rtHeightInt = meshHeight + delta * 2;

			float rtAspect = 1f * rtWidthInt / rtHeightInt;

			//调整摄像机区域。完全按照rt的大小来设。
			_cam.aspect = rtAspect;
			_cam.orthographicSize = rtHeightInt * .5f;

			//调整model的位置，让摄像机正好能包括它。
			//乘-1是因为model是子物体，正好跟center相反。
			_modelRenderer.transform.localPosition = mesh.bounds.center * -1f;

			//创造rt时，应用downsample
			_rt = new RenderTexture(rtWidthInt / downSample, rtHeightInt / downSample, 0, RenderTextureFormat.ARGB32);

			_blurIterations = blurIterations;

			_strength = strength;

			_modelRenderer.enabled = true;

			//render这个方法会导致OnRenderImage被执行。
			_cam.Render();

			_modelRenderer.enabled = false;

			return _rt;
		}

		//	// Use this for initialization
		//	void Start()
		//	{
		//	}
		//	
		//	// Update is called once per frame
		//	void Update () {
		//	}
		//
		//	void OnEnable()
		//	{
		//	}
		//
		//
		//	void OnPreRender()
		//	{
		//	}

		void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			_cb.Clear();
			_cb.ClearRenderTarget(true, true, new Color(1, 1, 1, 0));
			_matTakePhoto.mainTexture = _tex;
			_matTakePhoto.SetFloat("_Strength", _strength);

			_cb.DrawRenderer(_modelRenderer, _matTakePhoto);
			//		_cb.DrawMesh(
			//			_mesh, 
			//			Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one),
			//			_mat
			//			);
			Graphics.SetRenderTarget(_rt);
			Graphics.ExecuteCommandBuffer(_cb);

			//横纵两个PASS拉伸
			Vector4 screenSize = new Vector4(1.0f / _rt.width, 1.0f / _rt.height, 0.0f, 0.0f);
			_blurMaterial.SetVector("_ScreenSize", screenSize);

			for (int i = 0; i < _blurIterations; i++)
			{
				//			float iterationOffs = (i*1.0f);
				//			blurMaterial.SetVector("_Parameter", new Vector4(blurSize + iterationOffs, -blurSize - iterationOffs, 0.0f, 0.0f));

				RenderTexture tempRt = RenderTexture.GetTemporary(_rt.width, _rt.height, 0, _rt.format);
				tempRt.filterMode = FilterMode.Bilinear;

				// horizontal blur
				Graphics.Blit(_rt, tempRt, _blurMaterial, 2);

				// vertical blur
				Graphics.Blit(tempRt, _rt, _blurMaterial, 3);

				RenderTexture.ReleaseTemporary(tempRt);
			}
		}


		//	void OnPostRender()
		//	{
		////		Debug.Log("OnPostRender");
		////		Mat.SetPass(0);
		////		Graphics.DrawMeshNow(Mesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one*100));
		//
		//	}
	}
}
