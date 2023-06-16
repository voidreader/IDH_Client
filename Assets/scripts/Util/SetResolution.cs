using System;
using System.Collections.Generic;
using UnityEngine;

public class SetResolution : MonoBehaviour
{
    public int width = 1280;
    public int height = 720;

    void Start()
    {
//#if UNITY_STANDALONE || UNITY_WEBPLAYER
    //Application.targetFrameRate = 60;
    //if (Screen.width != width || Screen.height != height)
        //Screen.SetResolution(width, height, false);
//#endif
        Destroy(this);
    }

}

