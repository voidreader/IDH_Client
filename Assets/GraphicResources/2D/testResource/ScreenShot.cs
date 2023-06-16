using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class ScreenShot : MonoBehaviour
{
    public Camera cam;

    public static string PATH_ROOT;
    public static string PATH_GALLERY;
    public static string PATH_GALLERY_IMAGE;


    // Use this for initialization
    void Start () {
        PATH_ROOT = string.Concat(Application.dataPath, "/UserData/");
        PATH_GALLERY = string.Concat(PATH_ROOT, "Gallery/");
        PATH_GALLERY_IMAGE = string.Concat(PATH_GALLERY, "Image/");

        DirectoryCheck(PATH_ROOT);
        DirectoryCheck(PATH_GALLERY);
        DirectoryCheck(PATH_GALLERY_IMAGE);

        if(cam == null) cam = GetComponent<Camera>();
    }
	
	// Update is called once per frame
	void Update () {


        if (Input.GetKeyDown(KeyCode.P))
        {
            PirntScreenShot();
        }

	}

    private void DirectoryCheck(string path)
    {
        if (Directory.Exists(path)) return;
        Directory.CreateDirectory(path);
    }


    public void PirntScreenShot()
    {
        RenderTexture rt = new RenderTexture(2048, 720, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;

        Texture2D screenShot = new Texture2D(2048, 720, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);
        screenShot.Apply();

        ExportImage(screenShot, string.Concat("ScreenShot_", DateTime.Now.ToString("H mm ss")));

        cam.targetTexture = null;
    }

    public void ExportImage(Texture2D image, string name)
    {
        byte[] bytes = image.EncodeToPNG();

        WriteFile(bytes, PATH_GALLERY_IMAGE, string.Concat(name, ".png"));
    }

    private void WriteFile(byte[] bytes, string path, string fileName)
    {
        FileStream file = new FileStream(string.Concat(path, fileName), FileMode.Create);
        file.Write(bytes, 0, bytes.Length);
        file.Close();
    }



}
