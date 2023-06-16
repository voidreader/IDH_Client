using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// 蜡聪萍 包访 窃荐甸阑 埃祈窍霸 父甸绢 初篮 巴.
/// (哪器惩飘 眠啊/昏力/茫扁, 霸烙坷宏璃飘 茫扁 殿 )
/// Todo : 滚瓢 妮归窃荐 殿废 棺 秦力 窃荐 父甸扁
/// </summary>
public class UnityCommonFunc
{
	public static T GetComponentByName<T>(GameObject go, string name)
			where T : Component
	{
		T[] buffer = go.GetComponentsInChildren<T>(true);
		if (buffer != null)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				if (buffer[i] != null && buffer[i].name == name)
				{
					return buffer[i];
				}
			}
		}
		return null;
	}

	public static T[] GetComponentsByName<T>(GameObject go)
			where T : Component
	{
		T[] buffer = go.GetComponentsInChildren<T>(true);

		return buffer;
	}

	public static GameObject GetGameObjectByName(GameObject objInput, string strFindName)
	{
		GameObject ret = null;
		if (objInput != null)
		{
			Transform[] objChildren = objInput.GetComponentsInChildren<Transform>(true);
			if (objChildren != null)
			{
				for (int i = 0; i < objChildren.Length; ++i)
				{
					if ((objChildren[i].name == strFindName))
					{
						ret = objChildren[i].gameObject;
						break;
					}
				}
			}
		}
		return ret;
	}

	public static List<GameObject> GetGameObjectsByName(GameObject objInput, string strFindName)
	{
		List<GameObject> list = new List<GameObject>();
		Transform[] objChildren = objInput.GetComponentsInChildren<Transform>(true);
		for (int i = 0; i < objChildren.Length; ++i)
		{
			if ((objChildren[i].name.Contains(strFindName)))
			{
				list.Add(objChildren[i].gameObject);
			}
		}

		return list;
	}

	public static void ResetTransform(Transform _tf)
	{
		_tf.localPosition = Vector3.zero;
		_tf.localScale = Vector3.one;
		_tf.localRotation = Quaternion.identity;
	}

	/// <summary>
	/// ugui添加按钮点击事件
	/// </summary>
	/// <param name="go"></param>
	/// <param name="UICallback"></param>
	//public static void UAddBtnMsg(GameObject go, System.Action<GameObject> UICallback)
	//{
	//    UnityEngine.UI.Button btn = go.GetComponent<UnityEngine.UI.Button>();
	//    if (btn != null)
	//    {
	//        btn.onClick.RemoveAllListeners();
	//        btn.onClick.AddListener(() => UICallback(go));
	//    }
	//}

	//public static void AddBtnMsg(UIButton button, EventDelegate.Callback callback, AudioType audio = AudioType.BtnClick)
	//{
	//    EventDelegate.Add(button.onClick,
	//        () => TGameCore.GetIntance().GetEventMgr().SendEvent(AudioEventData.CreateAudioOnEvent(audio)));
	//    EventDelegate.Add(button.onClick, callback);
	//}

	//public static void AddTabMsg(UIToggle tab, EventDelegate.Callback callback, AudioType audio = AudioType.TabChange)
	//{
	//    EventDelegate.Add(tab.onChange,
	//        () =>
	//        {
	//            if (UIToggle.current.value)
	//                TGameCore.GetIntance().GetEventMgr().SendEvent(AudioEventData.CreateAudioOnEvent(audio));
	//        });
	//    EventDelegate.Add(tab.onChange, callback);
	//}

	//public static void AddBtnMsg(GameObject go, UIMsgDelegate UICallback, UIButtonMsg.Trigger funcType = UIButtonMsg.Trigger.OnClick, AudioType audio = AudioType.BtnClick)
	//{
	//    UIButtonMsg btnMsg = null;
	//    if (go == null)
	//    {
	//        TGameCore.LogError("按钮对象go为null！！");
	//        return;
	//    }
	//    // 目前只支持 click声音, 后续再加入其它声音
	//    //if (audio != AudioType.MaxCount)
	//    //if (funcType == UIButtonMsg.Trigger.OnClick && audio == AudioType.BtnClick)
	//    //{
	//    //    btnMsg = go.AddComponent<UIButtonMsg>();
	//    //    btnMsg.trigger = funcType;
	//    //    btnMsg.m_uiMsgDelegate = ((clickGO) =>
	//    //    {
	//    //        TGameCore.GetIntance().GetEventMgr().SendEvent(AudioEventData.CreateAudioOnEvent(AudioType.BtnClick));
	//    //    });
	//    //}

	//    //其他ui事件按钮音效为BtnClick，使用AudioTypeConvert类配置的音效
	//    if (funcType != UIButtonMsg.Trigger.OnClick && audio == AudioType.BtnClick)
	//    {
	//        audio = AudioTypeConvert.GetAudioTyep(funcType.ToString());
	//    }

	//    btnMsg = go.AddComponent<UIButtonMsg>();
	//    btnMsg.trigger = funcType;
	//    btnMsg.m_uiMsgDelegate = ((clickGO) =>
	//        {
	//            TGameCore.GetIntance().GetEventMgr().SendEvent(AudioEventData.CreateAudioOnEvent(audio));
	//        });
	//    btnMsg.m_uiMsgDelegate += UICallback;
	//}

	//public static void ChangeBtnMsg(GameObject go, UIMsgDelegate UICallback, UIButtonMsg.Trigger funcType = UIButtonMsg.Trigger.OnClick, AudioType audio = AudioType.BtnClick)
	//{
	//    DelBtnMsg(go, () => {});
	//    AddBtnMsg(go, UICallback, funcType, audio);
	//}

	//public static void DelBtnMsg(GameObject go, EventDelegate.Callback UICallback)
	//{
	//    UIButtonMsg[] btnMsg = go.GetComponents<UIButtonMsg>();
	//    if (btnMsg != null)
	//    {
	//        for (int i = 0; i < btnMsg.Length; i++)
	//        {
	//            GameObject.Destroy(btnMsg[i]);
	//        }
	//    }
	//}

	public static T AddSingleComponent<T>(GameObject go)
			where T : Component
	{
		T comp = go.GetComponent<T>();
		if (comp == null)
		{
			comp = go.AddComponent<T>();
		}
		return comp;
	}

	public static float FloatLerp(float from, float to, float t)
	{
		t = Mathf.Clamp(t, 0.0f, 1.0f);
		float ret = from + (to - from) * t;
		return ret;
	}

	public static GameObject InstantiatePrefab(GameObject prefab, GameObject parent)
	{
		GameObject obj = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
		obj.name = prefab.name;
		if (parent != null)
		{
			obj.transform.parent = parent.transform;
		}
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localScale = Vector3.one;
		return obj;
	}

	public static string RemoveBlankChar(string oldString)
	{
		string newString = oldString.Replace("\r", string.Empty);
		newString = newString.Replace("\n", string.Empty);
		newString = newString.Replace("\t", string.Empty);
		return newString;
	}

#if UNITY_STANDALONE || UNITY_EDITOR
	public static GameObject CameraRayCollideWithObject(Camera rayCamera)
	{
		GameObject clickedObject = null;
		Ray ray = rayCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit))
		{
			clickedObject = hit.transform.gameObject;
		}
		return clickedObject;
	}
#else
    public static GameObject CameraRayCollideWithObject(Camera rayCamera)
    {
        GameObject clickedObject = null;
        if (Input.touchCount == 1)
        {
            Ray ray = rayCamera.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                clickedObject = hit.transform.gameObject;
            }
        }
        return clickedObject;
    }
#endif

	public static bool GetCameraHitInfo(Camera rayCamera, out RaycastHit hit)
	{
		Ray ray = rayCamera.ScreenPointToRay(Input.mousePosition);
		bool bHitted = Physics.Raycast(ray, out hit);

		return CheckInRect(bHitted, rayCamera, Input.mousePosition);
	}
	/// <summary>
	/// 检测是否在相机视口内点击
	/// </summary>
	/// <param name="bHitted"></param>
	/// <param name="rayCamera"></param>
	/// <param name="pos"></param>
	/// <returns></returns>
	private static bool CheckInRect(bool bHitted, Camera rayCamera, Vector3 pos)
	{
		Rect rect = rayCamera.pixelRect;
		if (rect.y < pos.y && pos.y < (rect.y + rect.height) && rect.x < pos.x && pos.x < (rect.x + rect.width))
		{
			return (bHitted && UICamera.hoveredObject == null);
		}
		return false;
	}

	public static bool GetCameraHitInfo(Camera rayCamera, out RaycastHit hit, params string[] layerMaskName)
	{
#if UNITY_EDITOR
		Ray ray = rayCamera.ScreenPointToRay(Input.mousePosition);
#else
		Ray ray = rayCamera.ScreenPointToRay(Input.GetTouch(0).position);
#endif
		bool bHitted = Physics.Raycast(ray, out hit, 99999, LayerMask.GetMask(layerMaskName));

		return CheckInRect(bHitted, rayCamera, Input.mousePosition);
	}

	public static bool GetCameraHitInfo2D(Camera rayCamera, out RaycastHit2D hit, params string[] layerMaskName)
	{
#if UNITY_EDITOR
		Ray ray = rayCamera.ScreenPointToRay(Input.mousePosition);
#else
		Ray ray = rayCamera.ScreenPointToRay(Input.GetTouch(0).position);
#endif
		hit = Physics2D.Raycast(ray.origin, ray.direction, 99999, LayerMask.GetMask(layerMaskName));
		return hit.collider != null;
	}

	//public static void AddLongPressListener(GameObject go, Action<GameObject> callBack)
	//{
	//    if (go != null)
	//    {
	//        LongPressListener longPressComp = TGCommonFunc.AddSingleComponent<LongPressListener>(go);
	//        longPressComp.OnLongPressUnityEvent.AddListener((g) =>
	//        {
	//            if (callBack != null)
	//            {
	//                callBack(g);
	//            }
	//        });
	//    }
	//}

	//public static void AddBtnScaleAndColor(GameObject go)
	//{
	//    if (go != null)
	//    {
	//        UIButtonColor btnColor = TGCommonFunc.AddSingleComponent<UIButtonColor>(go);
	//        btnColor.tweenTarget = go;
	//        btnColor.defaultColor = Color.white;
	//        btnColor.hover = Color.white;
	//        btnColor.pressed = new Color(200f / 255f, 200f / 255f, 200f / 255f, 1f);
	//        btnColor.disabledColor = Color.white;

	//        UIButtonScale btnScale = TGCommonFunc.AddSingleComponent<UIButtonScale>(go);
	//        btnScale.tweenTarget = go.transform;
	//        btnScale.hover = Vector3.one;
	//        btnScale.pressed = new Vector3(0.9f, 0.9f, 0.9f);
	//    }
	//}
}