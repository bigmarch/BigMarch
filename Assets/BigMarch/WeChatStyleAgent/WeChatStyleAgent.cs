using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using SevenZip.Compression.LZMA;

[RequireComponent(typeof(AudioSource))]
public class WeChatStyleAgent : MonoBehaviour
{
	public enum State
	{
		Disable,
		Ready,
		Record,
		CompresseAndUpload,
	}

	private State _state;

	public State CurrentState
	{
		get { return _state; }
	}

	private static WeChatStyleAgent _instance;
	private static WeChatStyleServer _server;

	public static WeChatStyleAgent Instance
	{
		get
		{
			if (!_instance)
			{
				_instance = new GameObject("WeChatStyle", typeof(WeChatStyleAgent)).GetComponent<WeChatStyleAgent>();
				_server = _instance.gameObject.AddComponent<WeChatStyleServer>();
			}
			return _instance;
		}
	}

	//The maximum and minimum available recording frequencies  
	private int _minFreq;
	private int _maxFreq;

	//A handle to the attached AudioSource  
	private AudioSource _audioSource;

	void Awake()
	{
		//Get the attached AudioSource component  
		_audioSource = this.GetComponent<AudioSource>();
		Refresh();
	}

	public void Refresh()
	{
		//Refresh if there is at least one microphone connected  
		if (Microphone.devices.Length <= 0)
		{
			//Throw a warning message at the console if there isn't  
			Debug.LogWarning("Microphone not connected!");
			_state = State.Disable;
		}
		else //At least one microphone is present  
		{
			//Set 'micConnected' to true  
			_state = State.Ready;

			//Get the default microphone recording capabilities  
			Microphone.GetDeviceCaps(null, out _minFreq, out _maxFreq);

			//According to the documentation, if minFreq and maxFreq are zero, the microphone supports any frequency...  
			if (_minFreq == 0 && _maxFreq == 0)
			{
				//...meaning 44100 Hz can be used as the recording sampling rate  
				_maxFreq = 44100;
			}
		}
	}

	public void Init(string localCachedFolderPath, string serverUrl, Func<string> createFileName)
	{
		_server.LocalCachedFolderPath = localCachedFolderPath;
		_server.ServerUrl = serverUrl;
		_server.CreatName = createFileName;
	}

	public void StartRecord(Action onReachMaxLength, int maxLength = 20)
	{
		Debug.Assert(_state == State.Ready, "_state == State.Ready");

		//Start recording and store the audio captured from the microphone at the AudioClip in the AudioSource  
		_audioSource.clip = Microphone.Start(null, false, maxLength, _maxFreq);

		// 开启一个携程，用来记录时间。每帧都要检查condition，如果condition为false，那么终止这个携程。
		// 如果停止了录制，那么这个携程直接终止。
		StartCoroutine(
			DelayCall(
				maxLength,
				() =>
				{
					Debug.Assert(_state == State.Record, "_state == State.Record");
					_state = State.Ready;
					onReachMaxLength();
				},
				() => !Microphone.IsRecording(null))
		);

		_state = State.Record;
	}

	public void CompleteRecord()
	{
		Debug.Assert(_state == State.Record, "_state == State.Record");

		Microphone.End(null); //Stop the audio recording  
		_audioSource.clip = SavWav.TrimSilence(_audioSource.clip, 0.01f);

		_state = State.Ready;
	}

	public void CompressAndUpload(Action<string> onComplete)
	{
		Debug.Assert(_state == State.Ready, "_state == State.Ready");
		Debug.Assert(_audioSource.clip, "_audioSource.clip");

		_state = State.CompresseAndUpload;

		float[] floatArr = new float[_audioSource.clip.samples];
		_audioSource.clip.GetData(floatArr, 0);

		DownSample(floatArr);

		var bytesArr = ToByteArray_MkII(floatArr);

		StartCoroutine(
			CompressAndUpload_Co(
				bytesArr,
				s =>
				{
					onComplete(s);
					_state = State.Ready;
				}));
	}

	public void DownloadAndDecompressAndPlay(string url, Action onPlay)
	{
		StartCoroutine(
			DownloadAndDecompress_Co(
				url,
				bytes =>
				{
					float[] floatArr = ToFloatArray_MkII(bytes);
					_audioSource.clip = AudioClip.Create("playRecordClip", floatArr.Length, 1, 44100, false);
					_audioSource.clip.SetData(floatArr, 0);
					_audioSource.Play();

					onPlay();
				})
		);
	}

	//			bytesArr = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(byteArrCompressed);
	//
	//			floatArr = ToFloatArray_MkII(bytesArr);
	//
	//			_audioSource.clip = AudioClip.Create("playRecordClip", floatArr.Length, 1, 44100, false);
	//
	//			_audioSource.clip.SetData(floatArr, 0);
	//
	//			_audioSource.Play();

	#region Coroutine

	private IEnumerator CompressAndUpload_Co(byte[] data, Action<string> onComplete)
	{
		// compress
		Container<byte[]> byteArrContainer = new Container<byte[]>();
		yield return StartCoroutine(CompressOrDecompress_Co(true, data, byteArrContainer));

		// upload
		Container<string> stringContainer = new Container<string>();
		yield return Upload_Co(byteArrContainer.Value, stringContainer);

		onComplete(stringContainer.Value);
	}

	private IEnumerator DownloadAndDecompress_Co(string url, Action<byte[]> onComplete)
	{
		// download
		Container<byte[]> downloadResultContainer = new Container<byte[]>();
		yield return Download_Co(url, downloadResultContainer);

		// compress
		Container<byte[]> decompressResultContainer = new Container<byte[]>();
		yield return StartCoroutine(CompressOrDecompress_Co(false, downloadResultContainer.Value, decompressResultContainer));

		onComplete(decompressResultContainer.Value);
	}

	private IEnumerator Upload_Co(byte[] data, Container<string> container)
	{
		yield return StartCoroutine(_server.Upload_Co(data, container));
	}

	private IEnumerator Download_Co(string url, Container<byte[]> container)
	{
		yield return StartCoroutine(_server.Download_Co(url, container));
	}

	private IEnumerator CompressOrDecompress_Co(bool compressOrDecompress, byte[] inputData, Container<byte[]> container)
	{
		AsyncEntity entity = new AsyncEntity();
		entity.Init(inputData, compressOrDecompress);
		Thread t = new Thread(entity.Do);
		t.Start();
		while (!entity.IsOver())
		{
			//TODO 根据当前帧率，当台调整unzip这个thread sleep的时间。目前写死成-1。即不sleep。
			//UnzipAsyncEntity.SleepTimeWhenUnzip = -1;
			yield return null;
		}
		if (!string.IsNullOrEmpty(entity.GotExceptionMessage()))
		{
			Debug.Log(entity.GotExceptionMessage());
		}

		container.Value = entity.GetOuputData();

		// 输出压缩或者解压缩的信息。
		string log = string.Format("{0} complete {1} -> {2} : {3}", compressOrDecompress ? "compress" : "decompress",
			inputData.Length, entity.GetOuputData().Length,
			entity.GetOuputData().Length * 1f / inputData.Length);
		Debug.Log(log);
	}

	#endregion

	#region Tool

	public byte[] ToByteArray(float[] floatArray)
	{
		byte[] byteArray = new byte[floatArray.Length * 4];
		int pos = 0;
		foreach (float f in floatArray)
		{
			byte[] data = BitConverter.GetBytes(f);
			Array.Copy(data, 0, byteArray, pos, 4);
			pos += 4;
		}
		return byteArray;
	}

	public byte[] ToByteArray_MkII(float[] floatArray)
	{
		// create a byte array and copy the floats into it...
		var byteArray = new byte[floatArray.Length * 4];
		Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
		return byteArray;
	}

	public float[] ToFloatArray(byte[] byteArray)
	{
		float[] floatArray = new float[byteArray.Length / 4];
		for (int i = 0; i < byteArray.Length; i += 4)
		{
			floatArray[i / 4] = BitConverter.ToSingle(byteArray, i);
		}
		return floatArray;
	}

	public float[] ToFloatArray_MkII(byte[] byteArray)
	{
		// create a second float array and copy the bytes into it...
		var floatArray = new float[byteArray.Length / 4];
		Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
		return floatArray;
	}

	public void DownSample(float[] arr)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			if (i % 2 == 1)
			{
				arr[i] = 0;
			}
		}
	}

	private IEnumerator DelayCall(float delay, Action call, Func<bool> breakCondition)
	{
		float time = 0;
		while (time < delay)
		{
			if (breakCondition())
			{
				yield break;
			}
			yield return new WaitForEndOfFrame();

			time += Time.deltaTime;
		}
		call();
	}

	private class AsyncEntity
	{
		private bool _compressOrDecompress;
		private byte[] _input;
		private byte[] _output;
		private bool _over;
		private string _exceptionMessage;

		//如果这个属性>0,那么在unzip的while中会sleep一定时间。
		public static int SleepTimeWhenUnzip = -1;

		public void Init(byte[] input, bool compressOrDecompress)
		{
			_input = input;
			_over = false;
			_exceptionMessage = "";
			_compressOrDecompress = compressOrDecompress;
		}

		public void Do()
		{
			try
			{
				if (_compressOrDecompress)
				{
					_output = SevenZipHelper.Compress(_input);
				}
				else
				{
					_output = SevenZipHelper.Decompress(_input);
				}
			}
			catch (Exception e)
			{
				_exceptionMessage = e.Message;
			}

			_over = true;
		}

		public bool IsOver()
		{
			return _over;
		}

		public string GotExceptionMessage()
		{
			return _exceptionMessage;
		}

		public byte[] GetOuputData()
		{
			return _output;
		}
	}

	public class Container<T>
	{
		public T Value;
	}

	#endregion
}


