using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class WeChatStyleAgentExample : MonoBehaviour
{
	private List<string> _cachedUrlList;

	void Awake()
	{
		_cachedUrlList = new List<string>();
	}

	// Use this for initialization
	void Start()
	{
		string local = Application.dataPath.Replace("Assets", "WeChatStyleCachedFolder");
		string server = Application.dataPath.Replace("Assets", "WeChatStyleTempServer");
		Func<string> createFileName = () => Time.time.ToString(CultureInfo.InvariantCulture);
		WeChatStyleAgent.Instance.Init(local, server, createFileName);
	}

	// Update is called once per frame
	void Update()
	{
	}

	void OnGUI()
	{
		Rect r0 = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 25, 400, 50);
		Rect r1 = new Rect(Screen.width / 2 - 200, Screen.height / 2 + 25, 400, 50);
		Rect r2 = new Rect(Screen.width / 2 - 200, Screen.height / 2 + 75, 400, 50);

		//If there is a microphone  
		if (WeChatStyleAgent.Instance.CurrentState == WeChatStyleAgent.State.Disable)
		{
			//Print a red "Microphone not connected!" message at the center of the screen  
			GUI.contentColor = Color.red;
			GUI.Label(r0, "Microphone not connected!");
			GUI.contentColor = Color.white;
			if (GUI.Button(r1, "Refresh"))
			{
				WeChatStyleAgent.Instance.Refresh();
			}
		}
		else // No microphone  
		{
			//If the audio from any microphone isn't being captured  
			if (WeChatStyleAgent.Instance.CurrentState == WeChatStyleAgent.State.Ready)
			{
				//Case the 'Record' button gets pressed  
				if (GUI.Button(r0, "Record"))
				{
					WeChatStyleAgent.Instance.StartRecord(() => { Debug.Log("on reach max length"); }, 3);
				}

				//Case the 'Record' button gets pressed  
				if (GUI.Button(r1, "Compress Upload"))
				{
					WeChatStyleAgent.Instance.CompressAndUpload(s =>
					{
						_cachedUrlList.Add(s);
					});
				}

				foreach (var url in _cachedUrlList)
				{
					if (GUILayout.Button(url))
					{
						WeChatStyleAgent.Instance.DownloadAndDecompressAndPlay(url, () => { });
					}
				}
			}
			else if (WeChatStyleAgent.Instance.CurrentState == WeChatStyleAgent.State.Record) //Recording is in progress  
			{
				//Case the 'Stop and Play' button gets pressed  
				if (GUI.Button(r0, "Stop"))
				{
					WeChatStyleAgent.Instance.CompleteRecord();
				}
			}
		}
	}
}
