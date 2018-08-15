using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PrintscreenHelperAdvance : EditorWindow
{
    [MenuItem("Window/PrintscreenHelperAdvance")]
    static void ShowWindow()
    {
        PrintscreenHelperAdvance window = GetWindow<PrintscreenHelperAdvance>(false, "PrintscreenHelperAdvance", true);
        window.minSize = new Vector2(352f, 132f);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Print"))
        {
            List<Camera> list = new List<Camera>();
            Camera[] allCamArr = FindObjectsOfType<Camera>();
            list.AddRange(allCamArr);

            list.Sort((a, b) =>
            {
                if (a.depth < b.depth)
                {
                    return -1;
                }

                if (a.depth > b.depth)
                {
                    return 1;
                }

                return 0;
            });


            Vector2 size = GetMainGameViewSize();

            RenderTexture rt = new RenderTexture((int) size.x, (int) size.y, 24, RenderTextureFormat.Default);

            foreach (Camera c in list)
            {
                if (c.gameObject.activeInHierarchy && c.enabled && !c.targetTexture)
                {
                    Debug.Log(c.name);
                }

                c.targetTexture = rt;
                c.Render();
                c.targetTexture = null;
            }

            string path = GetPath();
            SaveRenderTextureToPng(rt, path);

            rt.Release();

            string log = string.Format(
                "Congratulation!!!\nSave texture {0}\nResolution: {1} {2}",
                path,
                size.x,
                size.y);
            Debug.Log(log);
        }

        if (GUILayout.Button("OpenFolder"))
        {
            System.Diagnostics.Process.Start(GetFolderPath());
        }
    }

    // 能得到 game view 的分辨率
    private static Vector2 GetMainGameViewSize()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo getSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object res = getSizeOfMainGameView.Invoke(null,null);
        return (Vector2)res;
    }
    
    private string GetFolderPath()
    {
        return Path.GetDirectoryName(Application.dataPath);
    }

    private string GetPath()
    {
        int index = 0;
        string savePath = string.Format(
            "{0}/{1}{2}.png",
            GetFolderPath(),
            FileName,
            index);

        while (File.Exists(savePath))
        {
            index++;
            savePath = string.Format(
                "{0}/{1}{2}.png",
                GetFolderPath(),
                FileName,
                index);
        }

        return savePath;
    }

    public string FileName = "NewPng";

    //将RenderTexture保存成一张png图片  
    private static void SaveRenderTextureToPng(RenderTexture rt, string path)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        byte[] bytes = png.EncodeToPNG();
        FileStream file = File.Open(path, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
        DestroyImmediate(png);
        RenderTexture.active = prev;

        Debug.Log("printscreen save to : " + path);
    }
}