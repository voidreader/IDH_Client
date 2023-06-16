using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchSlidScript : MonoBehaviour
{

	#region static Conert Toruch Position
	public static readonly float mUIWidth = 1280; //NGUI Camera Width 
	public static readonly float mUIHeight = 720; //NGUI Camera height 
	public static readonly float mScreenWidth = Screen.width;
	public static readonly float mScreenHeight = Screen.height;

	internal static Vector2 GetConvertTouchPosToNGUICoord()
	{
#if UNITY_EDITOR
		return CalcurateTargetPos(Input.mousePosition);
#elif UNITY_ANDROID || UNITY_IOS
		return CalcurateTargetPos(Input.GetTouch(0).position);
#else
		return CalcurateTargetPos(Input.mousePosition);
#endif
	}

	internal static Vector2 CalcurateTargetPos(Vector2 mousePos)
	{
		Vector2 result;

		result.x = Mathf.Lerp(-mUIWidth / 2, mUIWidth / 2, mousePos.x / mScreenWidth);
		result.y = Mathf.Lerp(-mUIHeight / 2, mUIHeight / 2, mousePos.y / mScreenHeight);

		return result;
	}
#endregion

	public bool Pressed { get; private set; }
	public Vector2 Delta { get; private set; }
	public Vector2 Interval { get { return prevPos - startPos; } }

	Action<Vector2> cbEndGrag, cbStartGrag;

	Vector2 startPos, prevPos;
	Coroutine coPress;

    internal int urlPos;
    private List<int> dataIds = new List<int>();



    public void EnqIndex(int _id)
    {
        dataIds.Add(_id);
    }

    public void SetCallbackStartDrag(Action<Vector2> _cbStartGrag)
	{
		cbStartGrag = _cbStartGrag;
	}

    public void SetCallbackEndDrag(Action<Vector2> _cbEndGrag)
	{
		cbEndGrag = _cbEndGrag;
	}

	private void OnClick()
	{
        //배너 링크
        if (GameCore.Instance.SubsysMgr.NowSysType == SubSysType.Lobby)
        {
            var data = GameCore.Instance.DataMgr.GetMainBannerData(dataIds[urlPos]);
            switch (data.type)
            {
                case 0: Application.OpenURL(data.value1); break;
                case 1: OpenShop(data); break;
                case 2: OpenGacha(data); break;
            }
        }
    }


    private void OpenShop(MainBannerDataMap _data)
    {
        int tab = 0;
        int pos = Math.Max(0, _data.value2);
        int.TryParse(_data.value1, out tab);

        switch (tab)
        {
            case 0:
            default:    tab = 0; break;
            case 1:     tab = 2; break;
            case 2:     tab = 3; break;
            case 3:     tab = 1; break;
            case 4:     tab = 4; break;
        }

        GameCore.Instance.ChangeSubSystem(SubSysType.Shop, new ShopPara() { openTab = tab, openPos = pos });
    }


    private void OpenGacha(MainBannerDataMap _data)
    {
        int pos = Mathf.Clamp(_data.value2, 0, 3);
        GameCore.Instance.ChangeSubSystem(SubSysType.Gacha, new StoryPara(pos, false));
    }


	private void OnDragStart()
	{
		Pressed = true;
		if (cbStartGrag != null)
			cbStartGrag(startPos);

		startPos = prevPos = GetConvertTouchPosToNGUICoord();
		Delta = default(Vector2);
	}

	private void OnDrag()
	{
		var nowPos = GetConvertTouchPosToNGUICoord();
		Delta = nowPos - prevPos;
		prevPos = nowPos;
	}

	private void OnDragEnd()
	{
		Pressed = false;
		if (cbEndGrag != null)
			cbEndGrag(Interval);
	}
}
