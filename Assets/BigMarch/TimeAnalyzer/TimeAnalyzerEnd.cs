using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(999)]
public class TimeAnalyzerEnd : MonoBehaviour
{
	private static TimeAnalyzerEnd _instance;

	public static TimeAnalyzerEnd Instance
	{
		get { return _instance; }
	}

	private TickData _currentTickData;

	// 避免GC，在刚开始，把能 new 的都 new 一下。
	private Queue<TickData> _tickDataPool = new Queue<TickData>();

	void Awake()
	{
		for (int i = 0; i < 1000; i++)
		{
			var td = new TickData();
			td.FixedUpdateCost = new List<float>();
			_tickDataPool.Enqueue(td);
		}

		_currentTickData = _tickDataPool.Dequeue();
	}

	private void FixedUpdate()
	{
		float delta = Time.realtimeSinceStartup - TimeAnalyzerStart.Instance.FixedUpdateTime;
		delta *= 1000;

		_currentTickData.FixedUpdateCost.Add(delta);
	}

	// Update is called once per frame
	void Update()
	{
		float delta = Time.realtimeSinceStartup - TimeAnalyzerStart.Instance.UpdateTime;
		delta *= 1000;
		_currentTickData.UpdateCost = delta;
	}

	void LateUpdate()
	{
		if (_tickDataPool.Count == 0)
		{
			enabled = false;
			Debug.LogError("TimeAnalyzerEnd has worked for " + Time.frameCount + " frames. NOW STOP.");
			return;
		}

		int fps = (int) (1f / Time.deltaTime);
		// 超过 30 的，都按31处理。
		if (fps > 30)
		{
			fps = 31;
		}

		float delta = Time.realtimeSinceStartup - TimeAnalyzerStart.Instance.LateUpdateTime;
		delta *= 1000;
		_currentTickData.LateUpdateCost = delta;

		_currentTickData.Fps = fps;
		_currentTickData.FrameCount = Time.frameCount;
		_currentTickData.TimeSinceLevelStartUp = Time.timeSinceLevelLoad;

		if (Time.deltaTime > 0)
		{
			TimeAnalyzerData.Instance.Save(fps, _currentTickData);
		}
		
		// 给下一帧准备 data。
		_currentTickData = _tickDataPool.Dequeue();
	}
}