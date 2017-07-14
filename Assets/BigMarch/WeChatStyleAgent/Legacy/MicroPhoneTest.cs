/////********************************************************************************
////** 作者：ake
////** 创始时间：10/28/2015 1:51:49 PM
////** 描述：MicroPhoneTest  
////*********************************************************************************/
//
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using UnityEngine;
//using System.Collections;
//using SevenZip;
//
//[RequireComponent(typeof(AudioSource))]
//public class MicroPhoneTest : MonoBehaviour
//{
//	public string pacFileName = "record.pcm";
//	public string filePath = "F:\\SvpPro\\VoiceTestProject\\Assets\\0.pcm";
//	private static MicroPhoneTest m_instance;
//
//	public float sensitivity = 100;
//	public float loudness = 0;
//
//	private static string[] micArray = null;
//
//	const int HEADER_SIZE = 44;
//
//	const int RECORD_TIME = 10;
//
//	private AudioSource _audioSource;
//
//	void Awake()
//	{
//		_audioSource = GetComponent<AudioSource>();
//	}
//
//	public static MicroPhoneTest getInstance()
//	{
//		if (m_instance == null)
//		{
//			micArray = Microphone.devices;
//			if (micArray.Length == 0)
//			{
//				Debug.LogError("Microphone.devices is null");
//			}
//			foreach (string deviceStr in Microphone.devices)
//			{
//				Debug.Log("device name = " + deviceStr);
//			}
//			if (micArray.Length == 0)
//			{
//				Debug.LogError("no mic device");
//			}
//
//			GameObject MicObj = new GameObject("MicObj");
//			m_instance = MicObj.AddComponent<MicroPhoneTest>();
//		}
//		return m_instance;
//	}
//
//	void OnGUI()
//	{
//		GUI.Label(new Rect(10, 10, 200, 100), "loudness = " + loudness);
//		GUI.Label(new Rect(10, 210, 200, 100), "Microphone.GetPosition = " + Microphone.GetPosition(null));
//
//		if (GUILayout.Button("Play Record"))
//		{
//			LoadAndPlayRecord();
//		}
//	}
//
//
//	public void StopRecord()
//	{
//		if (micArray.Length == 0)
//		{
//			Debug.Log("No Record Device!");
//			return;
//		}
//		if (!Microphone.IsRecording(null))
//		{
//			return;
//		}
//		Microphone.End(null);
//		_audioSource.Stop();
//
//		Debug.Log("StopRecord");
//		// PlayRecord();
//
//		//调试Int16[] 数据的转化与播放
//		//PlayClipData(GetClipData());
//	}
//
//	public Byte[] GetClipData()
//	{
//		if (_audioSource.clip == null)
//		{
//			Debug.Log("GetClipData audio.clip is null");
//			return null;
//		}
//
//		float[] samples = new float[_audioSource.clip.samples];
//
//		_audioSource.clip.GetData(samples, 0);
//
//
//		Byte[] outData = new byte[samples.Length * 2];
//		//Int16[] intData = new Int16[samples.Length];
//		//converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]
//
//		int rescaleFactor = 32767; //to convert float to Int16
//
//		for (int i = 0; i < samples.Length; i++)
//		{
//			short temshort = (short) (samples[i] * rescaleFactor);
//
//			Byte[] temdata = System.BitConverter.GetBytes(temshort);
//
//			outData[i * 2] = temdata[0];
//			outData[i * 2 + 1] = temdata[1];
//		}
//		if (outData == null || outData.Length <= 0)
//		{
//			Debug.Log("GetClipData intData is null");
//			return null;
//		}
//		//return intData;
//		return outData;
//	}
//
//	public void PlayClipData(Int16[] intArr)
//	{
//		Debug.Log(intArr.Length);
//		if (intArr.Length == 0)
//		{
//			Debug.Log("get intarr clipdata is null");
//			return;
//		}
//		//从Int16[]到float[]
//		float[] samples = new float[intArr.Length];
//		int rescaleFactor = 32767;
//		for (int i = 0; i < intArr.Length; i++)
//		{
//			samples[i] = (float) intArr[i] / rescaleFactor;
//		}
//
//		//从float[]到Clip
//		AudioSource audioSource = this.GetComponent<AudioSource>();
//		if (audioSource.clip == null)
//		{
//			audioSource.clip = AudioClip.Create("playRecordClip", intArr.Length, 1, 8000, false);
//		}
//		audioSource.clip.SetData(samples, 0);
//		audioSource.mute = false;
//		audioSource.Play();
//	}
//
//	void PlayRecord()
//	{
//		if (_audioSource.clip == null)
//		{
//			Debug.Log("audio.clip=null");
//			return;
//		}
//		_audioSource.mute = false;
//		_audioSource.loop = false;
//		_audioSource.Play();
//		Debug.Log("PlayRecord");
//	}
//
//	public void LoadAndPlayRecord()
//	{
//		//string finalFilePath = filePath;
//		string finalFilePath = Application.dataPath + "/" + pacFileName;
//		//Debug.Log (Application.dataPath);
//
//
//
//		FileStream stream = new FileStream(finalFilePath, FileMode.Open);
//
//		long fileLength = stream.Length;
//		byte[] resByte = new byte[fileLength];
//
//		stream.Read(resByte, 0, (int) fileLength);
//		stream.Close();
//		stream.Dispose();
//
//		resByte = EncodeData(resByte);
//		resByte = DecodeData(resByte);
//
//		short[] audioData = new short[fileLength / 2];
//		BinaryBufferReader reader = new BinaryBufferReader(resByte);
//		short tmpValue;
//		for (int i = 0; i < audioData.Length; i++)
//		{
//			if (!reader.ReadInt16(out tmpValue))
//			{
//				break;
//			}
//			audioData[i] = tmpValue;
//		}
//		PlayClipData(audioData);
//	}
//
//	private byte[] EncodeData(byte[] inputData)
//	{
//		Debug.Log(inputData.Length);
//		MemoryStream outPut = new MemoryStream();
//		SevenZipTool.CompressMemoryToMemoryLZMA(inputData, outPut);
//		Debug.Log(outPut.GetBuffer().Length);
//		return outPut.GetBuffer();
//	}
//
//	private byte[] DecodeData(byte[] inputData)
//	{
//		Debug.Log(inputData.Length);
//		MemoryStream outPut = new MemoryStream();
//		SevenZipTool.DecompressMemoryLZMAFromStream(inputData, outPut, null);
//		Debug.Log(outPut.GetBuffer().Length);
//		return outPut.GetBuffer();
//	}
//
//	#region 废弃
//
//	public float GetAveragedVolume()
//	{
//		float[] data = new float[256];
//		float a = 0;
//		_audioSource.GetOutputData(data, 0);
//		foreach (float s in data)
//		{
//			a += Mathf.Abs(s);
//		}
//		return a / 256;
//	}
//
//	//// Update is called once per frame
//	//void Update ()
//	//{
//	//    loudness = GetAveragedVolume () * sensitivity;
//	//    if (loudness > 1) 
//	//    {
//	//        Debug.Log("loudness = "+loudness);
//	//    }
//	//}
//
//
//	public void StartRecord()
//	{
//		_audioSource.Stop();
//		if (micArray.Length == 0)
//		{
//			Debug.Log("No Record Device!");
//			return;
//		}
//		_audioSource.loop = false;
//		_audioSource.mute = true;
//		_audioSource.clip = Microphone.Start(null, false, RECORD_TIME, 44100); //22050 
//		while (!(Microphone.GetPosition(null) > 0))
//		{
//		}
//		_audioSource.Play();
//		Debug.Log("StartRecord");
//		//倒计时
//		StartCoroutine(TimeDown());
//	}
//
//	private IEnumerator TimeDown()
//	{
//		Debug.Log(" IEnumerator TimeDown()");
//
//		int time = 0;
//		while (time < RECORD_TIME)
//		{
//			if (!Microphone.IsRecording(null))
//			{
//				//如果没有录制
//				Debug.Log("IsRecording false");
//				yield break;
//			}
//			Debug.Log("yield return new WaitForSeconds " + time);
//			yield return new WaitForSeconds(1);
//			time++;
//		}
//		if (time >= 10)
//		{
//			Debug.Log("RECORD_TIME is out! stop record!");
//			StopRecord();
//		}
//		yield return 0;
//	}
//
//	#endregion
//
//	public static byte[] CompressByteToStreamLZMA(byte[] data)
//	{
//		LzmaEncoder coder = new LzmaEncoder();
//		MemoryStream input = new MemoryStream(data);
//		MemoryStream output = new MemoryStream();
//
//		// Write the encoder properties
//		coder.WriteCoderProperties(output);
//
//		// Write the decompressed file size.
//		output.Write(BitConverter.GetBytes(input.Length), 0, 8);
//
//		// Encode the file.
//		coder.Code(input, output, input.Length, -1, null);
//		return output.GetBuffer();
//	}
//}