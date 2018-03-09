#pragma warning disable 3009,3005,3001,3002,3003

using UnityEngine;

public class GrayscaleImageEffect : MonoBehaviour
{
	public Shader SaturationShader;
	private Material _saturationMaterial;
	private float _saturate = 1;

	void Awake()
	{
		SaturationShader = Shader.Find("Hidden/Saturation");
	}

	void OnEnable()
	{
		_saturationMaterial = new Material(SaturationShader);
	}

	void OnDisable()
	{
		DestroyImmediate(_saturationMaterial);
	}

	// Called by camera to apply image effect
	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		_saturationMaterial.SetFloat("_Saturation", _saturate);
		Graphics.Blit(source, destination, _saturationMaterial);
	}

	private float _to1Speed;
	private float _to0Speed;
	private float _lastTimeCalledSetSaturation;
	private float _stayZeroTime;
	private float _moveToOneTime;
	private float _moveToZeroTime;
	private float _lag;

	/// <summary>
	/// 迅速变黑白，停留一会，然后缓缓变为彩色。
	/// </summary>
	[ContextMenu("Dead")]
	public void Dead()
	{
		SetSaturationToZeroThenMoveToOne(1.12f, .8f, 2, 5);
	}

	/// <summary>
	/// 迅速变彩色
	/// </summary>
	[ContextMenu("Born")]
	public void Born()
	{
		SetSaturationToZeroThenMoveToOne(0f, 0f, 0f, .8f);
	}

	public void GameOver(float timeToGray)
	{
		SetSaturationToZeroThenMoveToOne(0f, timeToGray, 60f, 1f);
	}

	/// <summary>
	/// 把饱和度过度到0，等一会，再过渡到1。
	/// </summary>
	/// <param name="lag">延迟多久再执行</param>
	/// <param name="moveToZeroTime">过度到0需要的时间。</param>
	/// <param name="stayZeroTime">在0停留的时间</param>
	/// <param name="moveToOneTime">从0过度到1需要的时间</param>
	private void SetSaturationToZeroThenMoveToOne(float lag, float moveToZeroTime, float stayZeroTime, float moveToOneTime)
	{
		_to1Speed = 1f / moveToOneTime;
		_to0Speed = 1f / moveToZeroTime;

		_lastTimeCalledSetSaturation = Time.timeSinceLevelLoad;
		_stayZeroTime = stayZeroTime;
		_moveToOneTime = moveToOneTime;
		_moveToZeroTime = moveToZeroTime;
		_lag = lag;

		enabled = true;
	}

	private void Update()
	{
		if (Time.timeSinceLevelLoad - _lastTimeCalledSetSaturation < _moveToZeroTime + _stayZeroTime + _moveToOneTime + _lag)
		{
			if (Time.timeSinceLevelLoad - _lastTimeCalledSetSaturation < _lag)
			{
				//什么都不做
			}
			//move到0的状态
			else if (Time.timeSinceLevelLoad - _lastTimeCalledSetSaturation < _moveToZeroTime + _lag)
			{
				_saturate = Mathf.MoveTowards(_saturate, 0, _to0Speed * Time.deltaTime);
			}
			//stay到0的状态
			else if (Time.timeSinceLevelLoad - _lastTimeCalledSetSaturation < _moveToZeroTime + _stayZeroTime + _lag)
			{
				_saturate = 0;
			}
			//move到1的状态
			else
			{
				_saturate = Mathf.MoveTowards(_saturate, 1, _to1Speed * Time.deltaTime);
			}
		}
		else
		{
			_saturate = 1;
			enabled = false;
		}
	}
}

