using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class UIStainComponent : UIBase
{
    // 더러움, 빗자루, 상자
    public enum eIconType { NONE, ICON_MY_01, ICON_MY_02, ICON_MY_03, }

	[SerializeField] UITweener m_Tween = null;
	[SerializeField] UILabel m_txtMessage = null;

    public RoomStain stainInfo;
    public eIconType icon;

    public GameObject foreObject;
    public GameObject timer;
    public float fUpgradeTime;
    public float fStartTime;
    public bool IsUpgradeComplete;

    static int static_Depth = 200;

    public UISprite whiteBgSprite = null;
    public UISprite blackBgSprite = null;
    public UISprite gageSprite = null;
    public UILabel timerLbl;

    public override void Initialize()
	{
		base.Initialize();
	}

    public void Init(RoomStain stainInfo)
    {
        this.stainInfo = stainInfo;
        //this.nIdx = nIdx;
        //lodgingIdx.text = "숙소 " + nIdx.ToString();
        if (!string.IsNullOrEmpty(stainInfo.startTime) && !string.IsNullOrEmpty(stainInfo.endTime))
        {
            DateTime startTime = Convert.ToDateTime(stainInfo.startTime);
            DateTime endTime = Convert.ToDateTime(stainInfo.endTime);

            DateTime serverTime = GameCore.nowTime;
            TimeSpan span = endTime.Subtract(serverTime);

            int result = DateTime.Compare(serverTime, endTime);

            if (result > 0)
            {
                // 서버 타임이 더 크다
                IsUpgradeComplete = true;
                icon = eIconType.ICON_MY_03;
            }
            else if (result < 0)
            {
                // endtime 이 더 크다 ( 업그레이드 중 )
                TimeSpan startSpan = endTime.Subtract(startTime);

                fStartTime = (float)startSpan.TotalSeconds;
                fUpgradeTime = (float)span.TotalSeconds;

                icon = eIconType.ICON_MY_02;

                if(timer.transform.parent != null)
                {
                    if(timer.transform.parent.GetComponent<UISprite>().enabled )
                        timer.SetActive(true);
                    else
                        timer.SetActive(false);
                }

            }
            else if (result == 0)
            {
                // 동일
                IsUpgradeComplete = true;
            }
        }
        else
        {
            icon = eIconType.ICON_MY_01;
        }

        foreObject.GetComponent<UISprite>().spriteName = icon.ToString();

        // 실제 적용 사이즈
        //Debug.LogError("localsize " + foreObject.GetComponent<UISprite>().localSize);

        // 원본 사이즈
        Vector2 vOriSize = new Vector2(foreObject.GetComponent<UISprite>().atlas.GetSprite(icon.ToString()).width,
        foreObject.GetComponent<UISprite>().atlas.GetSprite(foreObject.GetComponent<UISprite>().spriteName).height);

        foreObject.GetComponent<UISprite>().width = (int)vOriSize.x;
        foreObject.GetComponent<UISprite>().height = (int)vOriSize.y;

        transform.GetComponent<UISprite>().depth = static_Depth;
        foreObject.GetComponent<UISprite>().depth = static_Depth + 1;
        whiteBgSprite.depth = static_Depth + 2;
        blackBgSprite.depth = static_Depth + 3;
        gageSprite.depth = static_Depth + 4;
        timerLbl.depth = static_Depth + 5;

        static_Depth += 5;
    }

	public override void Open()
	{
		base.Open();

		m_Tween.ResetToBeginning();
		m_Tween.PlayForward();
	}

	public override void Close()
	{
		base.Close();
	}

    public void OnClickStainComponent()
    {
        Debug.LogError("OnClickStainComponent()");

        if (string.IsNullOrEmpty(stainInfo.startTime) && string.IsNullOrEmpty(stainInfo.endTime))
            GameCore.Instance.NetMgr.Req_MyRoomStartCleanStain(stainInfo);
        else
        {
            if( IsUpgradeComplete)
                GameCore.Instance.NetMgr.Req_MyRoomEndCleanStain(stainInfo);
        }

        //SceneMyRoom.Inst.OpenMyRoom(nIdx);
    }

    public void SetMessage(string strMsg)
	{
		m_txtMessage.text = strMsg;

		Open();
	}

    public void SetMessage(int nIdx, object arg)
    {
        //m_txtMessage.text = string.Format(TableManager.GetString(nIdx), arg);
        Open();
    }

    public void LateUpdate()
    {
        if (timer )
        {
            // 게이지 뒤로감
            //timeSlider.value = (parentShop.fStartTime - (parentShop.fStartTime - parentShop.fUpgradeTime ))/ parentShop.fStartTime;
            if (fUpgradeTime > 0)
            {
                fUpgradeTime -= Time.deltaTime;
                if (fUpgradeTime <= 0)
                {
                    // 시간 strremainedtime에 넣기
                    fUpgradeTime = 0;
                    IsUpgradeComplete = true;
                    icon = eIconType.ICON_MY_03;
                    foreObject.GetComponent<UISprite>().spriteName = icon.ToString();
                    timer.SetActive(false);
                }
                else
                {
                    if (timer.transform.parent != null)
                    {
                        if (timer.transform.parent.GetComponent<UISprite>().enabled)
                            timer.SetActive(true);
                        else
                            timer.SetActive(false);
                    }

                    float fVal = 1 - ((fStartTime - (fStartTime - fUpgradeTime)) / fStartTime);
                    timer.GetComponent<UISlider>().value = fVal;
                    timerLbl.text = UtilityFunc.Inst.ConvertToTime(fUpgradeTime);
                }

            }
        }
    }
}

