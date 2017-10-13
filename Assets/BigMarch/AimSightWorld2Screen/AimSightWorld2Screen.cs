using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class AimSightWorld2Screen : MonoBehaviour
{
	public float ScatterAngle = 1;
	public Image AimSightHud;
	public Transform Shooter;

	private Camera _camera;

	void Awake()
	{
		_camera = GetComponent<Camera>();
	}

	// Update is called once per frame
	void Update()
	{
		float sideLength = CaculateAimSightSize(_camera, Shooter.position, Shooter.forward, ScatterAngle);
		AimSightHud.rectTransform.sizeDelta = new Vector2(sideLength, sideLength);
	}

	private static float CaculateAimSightSize(Camera camera, Vector3 origin, Vector3 direction, float scatterAngle)
	{
		Vector3 left = Quaternion.AngleAxis(scatterAngle, Vector3.up) * direction;
		Vector3 right = Quaternion.AngleAxis(-scatterAngle, Vector3.up) * direction;

		Vector3 farPointLeft = origin + left * camera.farClipPlane;
		Vector3 farPointRight = origin + right * camera.farClipPlane;

		Debug.DrawLine(origin, farPointLeft);
		Debug.DrawLine(origin, farPointRight);

		Vector3 screenFarPointLeft = camera.WorldToScreenPoint(farPointLeft);
		Vector3 screenFarPointRight = camera.WorldToScreenPoint(farPointRight);

		float sideLength = Mathf.Abs(screenFarPointRight.x - screenFarPointLeft.x);

		return sideLength;
	}
}
