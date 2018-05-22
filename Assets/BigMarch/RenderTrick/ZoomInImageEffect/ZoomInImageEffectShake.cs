using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomInImageEffectShake : MonoBehaviour
{
	public float LerpSpeed = 1;
	public float ShakeDelta = .02f;
	private ZoomInImageEffect _effect;
	
	void Awake()
	{
		_effect = GetComponent<ZoomInImageEffect>();
	}

	// Update is called once per frame
	void Update()
	{
		_effect.ZoomInFactor = Mathf.Lerp(_effect.ZoomInFactor, 1, Time.deltaTime * LerpSpeed);

		if (Input.GetKeyDown(KeyCode.Space))
		{
			Shake();
		}
	}

	public void Shake()
	{
		_effect.ZoomInFactor -= ShakeDelta;
	}

	public void Shake(Vector2 center)
	{
		_effect.ZoomInCenter = center;
		Shake();
	}
}