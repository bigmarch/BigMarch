using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WeChatStyleServer : MonoBehaviour
{
	public string LocalCachedFolderPath;
	public string ServerUrl;
	public Func<string> CreatName;

	public IEnumerator Upload_Co(byte[] data, WeChatStyleAgent.Container<string> container)
	{
		if (!Directory.Exists(LocalCachedFolderPath))
		{
			Directory.CreateDirectory(LocalCachedFolderPath);
		}

		// 根据玩家的GUID和时间戳。制作唯一的文件名。
		string fileName = CreatName();

		string localPath = Path.Combine(LocalCachedFolderPath, fileName);
		string serverPath = Path.Combine(ServerUrl, fileName);

		File.WriteAllBytes(localPath, data);

		// upload
		//		WWWForm wwwF = new WWWForm();
		//		wwwF.AddBinaryData("", data);
		//		WWW www = new WWW("", wwwF);
		//		yield return www;
		//		container.Value = www.text;

		// 临时把本地磁盘当服务器
		File.WriteAllBytes(serverPath, data);

		container.Value = "file://" + serverPath;
		yield break;
	}

	public IEnumerator Download_Co(string url, WeChatStyleAgent.Container<byte[]> container)
	{
		string fileName = Path.GetFileName(url);
		string localPath = Path.Combine(LocalCachedFolderPath, fileName);

		if (File.Exists(localPath))
		{
			byte[] result = File.ReadAllBytes(localPath);
			container.Value = result;
			yield break;
		}

		Debug.Log("Download : " + url);
		WWW www = new WWW(url);
		yield return www;
		container.Value = www.bytes;

		// 临时把本地磁盘当服务器
		File.WriteAllBytes(localPath, www.bytes);
	}
}
