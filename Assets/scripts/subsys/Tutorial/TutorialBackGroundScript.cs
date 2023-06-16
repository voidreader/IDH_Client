using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBackGroundScript : MonoBehaviour {
    
    UIDrawCall drawCall;
    UISprite uiTexture;
    Vector4 testVec = new Vector4(0, 0, 0, 0);
    Vector4 prevVec = new Vector4(0, 0, 0, 0);
    Vector4 ResultVec = new Vector4(-300, 100, 100, -100);
    float startTime = 0f;
    float limitTime = 2f;
    float limitLength = 0;
    int addLength = 0;
    private void Start()
    {
        uiTexture = GetComponent<UISprite>();
        drawCall = uiTexture.drawCall;
        //GetComponent<Renderer>().material.SetFloatArray
        ResetDraw(false,0,0,0,0);
    }
    private void GetTargetHolePos(int centerX, int centerY, int sizeX, int sizeY)
    {
        centerX -= 640;
        centerY = 360 - centerY;
        //if (targetUITexture == null) return;
        float halfsizeX = (float)sizeX * 0.5f + limitLength;
        float halfsizeY = (float)sizeY * 0.5f + limitLength;
        ResultVec.x = centerX - halfsizeX;
        ResultVec.y = centerX + halfsizeX;
        ResultVec.z = centerY + halfsizeY;
        ResultVec.w = centerY - halfsizeY;
    }
    public void ResetDraw(bool isActive, int centerX, int centerY, int sizeX, int sizeY)
    {
        startTime = isActive ? 0 : limitTime;
        if (isActive == false)
        {
            testVec.x = 0;
            testVec.y = 0;
            testVec.z = 0;
            testVec.w = 0;
            DrawTexture();
        }
        else
        {
            if (checkPrevPos(centerX, centerY, sizeX, sizeY, ref prevVec) == true) return;
            GetTargetHolePos(centerX, centerY, sizeX, sizeY);
            testVec.x = -(1280 + addLength) * 0.5f;
            testVec.y = (1280 + addLength) * 0.5f;
            testVec.z = (720 + addLength) * 0.5f;
            testVec.w = -(720 + addLength) * 0.5f;
            DrawTexture();
        }
    }
    private bool checkPrevPos(int centerX, int centerY, int sizeX, int sizeY, ref Vector4 prevVec)
    {
        centerX -= 640;
        centerY = 360 - centerY;
        bool returnValue = (prevVec.x == centerX && prevVec.y == centerY && prevVec.z == sizeX && prevVec.w == sizeY);
        if(returnValue == false)
        {
            prevVec.x = centerX;
            prevVec.y = centerY;
            prevVec.z = sizeX;
            prevVec.w = sizeY;
        }
        return returnValue;
    }
    private void MoveBackGroundHole()
    {
        if (startTime >= limitTime)
            return;
        startTime += Time.deltaTime;
        testVec = Vector4.Lerp(testVec, ResultVec, startTime);
        DrawTexture();
    }
    private void DrawTexture()
    {
        drawCall = uiTexture.drawCall;
        if (drawCall != null)
            drawCall.dynamicMaterial.SetVector("_Vector", testVec);
    }
    private void Update()
    {
        //if (Input.GetMouseButtonDown(0))
            //ResetDraw(true,0,0,100,100);

        MoveBackGroundHole();
    }
}
