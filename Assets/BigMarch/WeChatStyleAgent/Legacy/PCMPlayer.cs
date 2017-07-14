/********************************************************************************
** 作者：ake
** 创始时间：10/28/2015 10:24:03 AM
** 描述：PCMPlayer  
*********************************************************************************/

using UnityEngine;

public class PCMPlayer : MonoBehaviour
{
    public Object pcmFile;

    private AudioSource cachedAudio;
    void Awake()
    {
        cachedAudio = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
    }

    void OnGUI()
    {
        if (GUILayout.Button("CreateAudioClip "))
        {
//            AudioClip res;
//            res = new AudioClip();
//            res.SetData();
        }
    }
}
