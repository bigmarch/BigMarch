using System.Collections.Generic;
using UnityEngine;

public class TimeAnalyzerData
{
	private static TimeAnalyzerData _instance;

	public static TimeAnalyzerData Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new TimeAnalyzerData();
				_instance.Init();
			}

			return _instance;
		}
	}

	private TimeAnalyzerStart _start;
	private TimeAnalyzerEnd _end;

	private List<List<TickData>> _list = new List<List<TickData>>();

	private void Init()
	{
		for (int i = 0; i < 32; i++)
		{
			_list.Add(new List<TickData>());
		}
	}

	public void Save(int fps, TickData tickData)
	{
		Debug.Assert(fps > 0 && fps <= 31, fps);

		_list[fps].Add(tickData);

		// 调试的时候，输出看结果。
		tickData.Log();
	}

	private void Clear()
	{
		foreach (var i in _list)
		{
			i.Clear();
		}
	}

	public static void Send()
	{
		// 网络发送逻辑
		// ...
	}

	private GameObject _timeAnalyzerGo;

	public static void StartWork()
	{
		if (Instance._timeAnalyzerGo)
		{
			Debug.LogError("_timeAnalyzerGo 已存在，应该先调用 StopWork");
			return;
		}		

		Instance._timeAnalyzerGo = new GameObject("TimeAnalyzer");
		Instance._timeAnalyzerGo.AddComponent<TimeAnalyzerStart>();
		Instance._timeAnalyzerGo.AddComponent<TimeAnalyzerEnd>();

		Instance.Clear();
	}

	public static void StopWork()
	{
		if (Instance._timeAnalyzerGo)
		{
			Object.Destroy(Instance._timeAnalyzerGo);
		}

		Instance._timeAnalyzerGo = null;
	}
}

public class TickData
{
	// 这个 tick 发生时，的帧率。
	public int Fps;
	// 帧序号
	public int FrameCount;
	// 这个tick发生时的 Time.timeSinceLevelStartUp
	public float TimeSinceLevelStartUp;
	// 当前这个 tick Update() 方法消耗的时间。
	public float UpdateCost;
	// 当前这个 tick LateUpdate() 方法消耗的时间。
	public float LateUpdateCost;
	// 当前这个 tick FixedUpdate() 方法消耗的时间，这个是一个数组，如果这个 tick 内，没有调用FixedUpdate，那么该数组内元素数为0。
	public List<float> FixedUpdateCost;

	public void Log()
	{
		string log = string.Format("{0} : {1} : {2:0.000000} : {3:0.000000} : {4:0.000000} : {5}",
			Fps,
			FrameCount,
			TimeSinceLevelStartUp,
			UpdateCost,
			LateUpdateCost,
			FixedUpdateCost.Count);
		Debug.Log(log);
	}
}