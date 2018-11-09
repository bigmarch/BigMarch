using UnityEngine;

[DefaultExecutionOrder(-999)]
public class TimeAnalyzerStart : MonoBehaviour
{
	private static TimeAnalyzerStart _instance;

	public static TimeAnalyzerStart Instance
	{
		get { return _instance; }
	}

	private void Awake()
	{
		_instance = this;
	}

	private float _fixedUpdateTime;

	public float FixedUpdateTime
	{
		get { return _fixedUpdateTime; }
	}

	private void FixedUpdate()
	{
//		Debug.Log("FixedUpdate Start   " + Time.frameCount);
		_fixedUpdateTime = Time.realtimeSinceStartup;
	}

	private float _updateTime;

	public float UpdateTime
	{
		get { return _updateTime; }
	}

	void Update()
	{
//		Debug.Log("Update Start   " + Time.frameCount);
		_updateTime = Time.realtimeSinceStartup;
	}

	private float _lateUpdateTime;

	public float LateUpdateTime
	{
		get { return _lateUpdateTime; }
	}

	void LateUpdate()
	{
//		Debug.Log("LateUpdate Start   " + Time.frameCount);
		_lateUpdateTime = Time.realtimeSinceStartup;
	}
}