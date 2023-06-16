using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript_JinHyeok : MonoBehaviour {
    public UIButtonScale btnScale;
    private void Start()
    {
        UIEventListener.BoolDelegate eventListner = btnScale.GetComponent<UIEventListener>().onPress;
        //eventListner.BeginInvoke
        //btnScale.
    }
    /*
    public UITexture targetUITexture;
    UIDrawCall drawCall;
    UITexture uiTexture;
    Vector4 testVec = new Vector4(0, 0, 0, 0);
    Vector4 ResultVec = new Vector4(-300, 100, 100, -100);
    float startTime = 0f;
    float limitTime = 2f;
    float limitLength = 10;
    int addLength = 100;
    private void Start()
    {
        uiTexture = GetComponent<UITexture>();
        drawCall = uiTexture.drawCall;
        //GetComponent<Renderer>().material.SetFloatArray
        ResetDraw(false);
    }
    private void GetTargetHolePos()
    {
        //if (targetUITexture == null) return;

        Vector2 centerPos = targetUITexture.transform.localPosition;
        float widthDis = targetUITexture.width * 0.5f + limitLength;
        float HeightDis = targetUITexture.height * 0.5f + limitLength;
        ResultVec.x = centerPos.x - widthDis;
        ResultVec.y = centerPos.x + widthDis;
        ResultVec.z = centerPos.y + HeightDis;
        ResultVec.w = centerPos.y - HeightDis;
    }
    private void ResetDraw(bool isActive)
    {
        startTime = isActive ? 0 : limitTime;
        if (isActive == false)
        {
            testVec.x = 0;
            testVec.y = 0;
            testVec.z = 0;
            testVec.w = 0;
        }
        else
        {
            GetTargetHolePos();
            testVec.x = -(1280 + addLength) * 0.5f;
            testVec.y = (1280 + addLength) * 0.5f;
            testVec.z = (720 + addLength) * 0.5f;
            testVec.w = -(720 + addLength) * 0.5f;
        }
    }
    private void MoveBackGroundHole()
    {
        if (startTime >= limitTime)
            return;
        startTime += Time.deltaTime;
        testVec = Vector4.Lerp(testVec, ResultVec, startTime);
        drawCall = uiTexture.drawCall;
        if (drawCall != null)
            drawCall.dynamicMaterial.SetVector("_Vector", testVec);
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            ResetDraw(true);

        MoveBackGroundHole();
    }
    /*Dictionary<string, UnityEngine.Object> localRscPool;
    public string[] pathes = new string[3];
    // Use this for initialization
    void Start () {
        localRscPool = new Dictionary<string, Object>();

    }
	
	// Update is called once per frame
	void Update () {
		switch(Input.inputString)
        {
            case "1": GetInstanceLocalObject(pathes[0]); break;
            case "2": GetInstanceLocalObject(pathes[1]); break;
            case "3": GetInstanceLocalObject(pathes[2]); break;
        }
	}
    internal GameObject GetInstanceLocalObject(string _name, Transform _parent = null)
    {
        if (!localRscPool.ContainsKey(_name))
        {
            var rsc = Resources.Load<GameObject>(_name);
            if (rsc == null)
            {
                Debug.LogError(_name + " Not find in Resources Folder.");
                return null;
            }
            localRscPool.Add(_name, rsc);
        }
        return GameObject.Instantiate(localRscPool[_name], _parent) as GameObject;
    }
    */
}
