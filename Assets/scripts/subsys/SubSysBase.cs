using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 서브시스템의 기본틀
/// </summary>
internal abstract class SubSysBase :  IEventHandler
{
	public readonly SubSysType subSysType;																// 서브시스템의 종류
	protected Dictionary<GameEventType, Func<ParaBase,bool>> handlerMap;  // 핸들러 테이블<이벤트 서브 타입, 해당 서브 타입 이벤트 처리 루틴>

	private string name;
	internal string Name { get { return name; } set { name = value; GameCore.Instance.CommonSys.SetTitle(value); } }

	protected bool bNoDistroy;

    protected InitDataType[] needInitDataTypes; // Preloading 해야하는 데이터의 타입들
    protected int[] preloadingBundleKeys;       // Preloading 해야하는 번들 키들

    /// <summary>
    /// 생성자
    /// </summary>
    internal SubSysBase(SubSysType _subSysType)
	{
		subSysType = _subSysType;
		handlerMap = null;
		bNoDistroy = false;

        // 기본 생성으로 다른거나 이리 로딩하지 않는 경우 NULL로 초기화 해야한다. 
        preloadingBundleKeys = new int[] {
            CommonType.DEF_KEY_BGM_MAIN,
            CommonType.DEF_KEY_SFX_UI,
        };
    }

	/// <summary>
	/// true를 한다면 EixtSys를 해도 삭제되지 않는다.
	/// </summary>
	/// <param name="_active"></param>
	protected void NoDistroy(bool _active)
	{
		bNoDistroy = _active;
	}

	/// <summary>
	/// NoDistroy상태에서 Exit되거나 Enter되었을때 게임오브젝트 활성화 기능
	/// </summary>
	/// <param name="_active"></param>
	protected virtual void SetActive(bool _active)
	{
	}

	/// <summary>
	/// 이벤트 핸들링 처리
	/// </summary>
	/// <param name="_evt">처리할 이벤트 데이터</param>
	/// <returns>이벤트 처리를 하고 이상이 없다면 true</returns>
	public virtual bool HandleMessage(GameEvent _evt)
	{
		if (handlerMap.ContainsKey(_evt.EvtType))
			return handlerMap[_evt.EvtType](_evt.Para);
		return false;
	}

    /// <summary>
    /// 씬 진입 전 필요한 유저 데이터를 요청하고, 그 종류 수를 반환한다.
    /// </summary>
    /// <returns> 초기화되어야하는 유저데이터의 종류 수 </returns>
    public virtual int PreLoading()
    {
        int nNeedPlayerData = 0;
        if (needInitDataTypes != null)
        {
            foreach (var type in needInitDataTypes)
            {
                if (GameCore.Instance.PlayerDataMgr.IsInited(type))
                    continue;

                GameCore.Instance.NetMgr.Req_InitData(type);
                nNeedPlayerData++;
            }
        }
        
        if (preloadingBundleKeys != null)
        {
            foreach (var key in preloadingBundleKeys)
            {
                nNeedPlayerData++;
                GameCore.Instance.ResourceMgr.PreloadingAssetBundle(key, GameCore.Instance.SubsysMgr.DecNeedPreloadDataCnt);
            }
        }

        return nNeedPlayerData;
    }

	/// <summary>
	/// 서브시스템 진입
	/// </summary>
	internal void EnterSys(ParaBase _para)
	{
		if (bNoDistroy)
		{
			SetActive(true);
			bNoDistroy = false;
		}
		else
			EnterSysInner(_para);
	}

	internal void ExitSys(ParaBase _para)
	{
		if (bNoDistroy)
			SetActive(false);
		else
			ExitSysInner(_para);
	}

	/// <summary>
	/// 서브시스템 진입
	/// 이것을 재정의하여 초기화
	/// </summary>
	protected virtual void EnterSysInner(ParaBase _para)
	{
		Debug.Log("*****Enter Sys : " + subSysType + "\t" + Time.time);
		RegisterHandler();
	}

	/// <summary>
	/// 서브시스템 종료
	/// 이것을 제정의하여 메모리 정리
	/// </summary>
	protected virtual void ExitSysInner(ParaBase _para)
	{
        GameCore.Instance.SoundMgr.StopAllAudioSource();

        Debug.Log("****Exit Sys : " + subSysType + '\t' + Time.time);
		UnegisterHandler();
	}


	/// <summary>
	/// 이벤트 핸들러 등록
	/// 이것을 재정의하여 GameCore.Instance.EventMgr에 이벤트 핸들러 등록
	/// 반드시 마지막에 베이스콜을 할것
	/// </summary>
	protected virtual void RegisterHandler()
	{
		if(handlerMap != null)
		{
			var it = handlerMap.GetEnumerator();
			while(it.MoveNext())
				GameCore.Instance.EventMgr.RegisterHandler(this, it.Current.Key);
		}
	}


	/// <summary>
	/// 이벤트 핸들러 등록 해제
	/// </summary>
	protected virtual void UnegisterHandler()
	{
		GameCore.Instance.EventMgr.UnregisterHandler(this);
		//if( handlerMap != null )
		//{
		//	handlerMap.Clear();
		//	handlerMap = null;
		//}
	}

	/// <summary>
	/// 백버튼을 눌렀을때의 동작
	/// 기본적으로 로비화면으로 돌아간다.
	/// </summary>
	internal virtual void ClickBackButton()
	{
        if(GameCore.Instance.CommonSys.tbUi.GetFriendType() == true) GameCore.Instance.CommonSys.tbUi.OnClickCloseFriend();
        else if (GameCore.Instance.CommonSys.tbUi.GetActiveMenu()) GameCore.Instance.CommonSys.tbUi.OnCloseMenu();
        else GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
    }

	/// <summary>
	/// SubSysMgr에 의해 매 프레임 호출된다.
	/// </summary>
  internal virtual void UpdateUI()
	{
		
	}

    public static class CheckSuddenQuit
    {
        public enum SuddenQuitSysType
        {
            Lobby,
            PvPReady,
            PvpMatch,
            PvPGradeTest,
            PvpBattle,
        }
       
        public class SuddenQuitData
        {
            private bool isFirst;
            public bool IsFirst { get { return isFirst; } set { isFirst = value; } }
            private float suddenQuitStartTime;
            private float suddenQuitEndTime;
            private float suddenQuitLimitMin;

            private string suddenQuitTitle;
            private string suddenQuitStr;
            private string suddenQuitBtnName;
            private Action action;

            public void  SetSuddenQuitData(float _suddenQuitStartTime, float _suddenQuitEndTime, float _suddenQuitLimitMin, string _suddenQuitTitle, string _suddenQuitStr, string _suddenQuitBtnName, Action _action)
            {
                isFirst = false;
                suddenQuitStartTime = _suddenQuitStartTime;
                suddenQuitEndTime = _suddenQuitEndTime;
                suddenQuitLimitMin = _suddenQuitLimitMin;

                suddenQuitTitle = _suddenQuitTitle;
                suddenQuitStr = _suddenQuitStr;
                suddenQuitBtnName = _suddenQuitBtnName;
                action = _action;
            }
            public bool CheckSuddenQuitTime()
            {
                if (isFirst) return false;

                DayOfWeek dayOfWeek = GameCore.nowTime.DayOfWeek;
                switch (dayOfWeek)
                {
                    case DayOfWeek.Thursday:
                    case DayOfWeek.Sunday:
                        break;
                    default:
                        return false;
                }
                int hour = GameCore.nowTime.Hour;
                int minute = GameCore.nowTime.Minute;
                //if (DateTime.Now.Hour < 11 || DateTime.Now.Hour >= 22)
                if (hour < suddenQuitEndTime || hour >= suddenQuitStartTime)
                    return false;
                float nowMinutes = hour * 60 + minute;
                //if (nowMinutes >= 12 * 60 + 42)
                if (nowMinutes >= suddenQuitEndTime * 60 + suddenQuitLimitMin)
                {                   
                    isFirst = true;
                    return true;
                }
                return false;
            }
            public void GoToLobby()
            {
                GameCore.Instance.CloseMsgWindow();
                GameCore.Instance.ShowObject(suddenQuitTitle, suddenQuitStr, null, 0,
                            new MsgAlertBtnData[] { new MsgAlertBtnData()
                                                    {   text = suddenQuitBtnName,
                                                        enable = true,
                                                        ed = new EventDelegate(()=>{
                                                            if(action != null)
                                                                action();
                                                            }
                                                        )
                                                    }
                                                   });
            }
        }

        private static SuddenQuitData suddenQuitData;
        public static void SetSuddenQuitData(SuddenQuitSysType _type, Action _action)
        {
            if (suddenQuitData == null)
                suddenQuitData = new SuddenQuitData();
            PvPCommonDataMap pvpCommonData = GameCore.Instance.DataMgr.GetPvPConstData();
            switch (_type)
            {
                case SuddenQuitSysType.Lobby:
                    suddenQuitData.SetSuddenQuitData(pvpCommonData.pvpStartTime,
                                                        pvpCommonData.pvpEndTime,
                                                        0,
                                                        "시즌 종료",
                                                        "이번 시즌이 종료되었습니다.\n05:00까지 등급을 조정합니다.",
                                                        "확인",
                                                        () => {
                                                            if (_action != null)
                                                                _action();
                                                            GameCore.Instance.CloseMsgWindow();
                                                        });
                    break;
                case SuddenQuitSysType.PvPReady:
                case SuddenQuitSysType.PvpMatch:
                case SuddenQuitSysType.PvPGradeTest:
                    var startTime = PvPReadySys.GetRemainPvPSeasonStart();
                    suddenQuitData.SetSuddenQuitData(pvpCommonData.pvpStartTime,
                                                        pvpCommonData.pvpEndTime,
                                                        0,
                                                        "시즌 종료",
                                                        "이번 시즌이 종료되었습니다.\n다음 시즌까지 기다려주세요\n다음 시즌 오픈 시간 : " + startTime.ToString("yyyy/MM/dd HH:mm") ,
                                                        "메인으로",
                                                        ()=> {
                                                            if (_action != null)
                                                                _action();
                                                            GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
                                                            GameCore.Instance.CloseMsgWindow();
                                                        });
                    break;
                case SuddenQuitSysType.PvpBattle:
                    suddenQuitData.SetSuddenQuitData(pvpCommonData.pvpStartTime,
                                                        pvpCommonData.pvpEndTime,
                                                        pvpCommonData.pvpLimitTime,
                                                        "시즌 종료",
                                                        "이번 시즌이 종료되었습니다.\n메인화면으로 이동합니다.",
                                                        "메인으로",
                                                        () => {
                                                            if (_action != null)
                                                                _action();
                                                            GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
                                                            GameCore.Instance.CloseMsgWindow();
                                                        });
                    break;
            }
            
        }
        //반복(update등)에서 체크해야할 경우
        public static void SuddenQuit()
        {
            if (suddenQuitData.CheckSuddenQuitTime() == false) return;
            suddenQuitData.GoToLobby();
        }
        //버튼 클릭 등, 1회 클릭시마다 체크할 경우
        public static bool SuddenQuitReset()
        {
            suddenQuitData.IsFirst = false;
            if (suddenQuitData.CheckSuddenQuitTime() == false) return false;

            suddenQuitData.GoToLobby();
            return true;
        }
        //조건 없이 즉시 로비로 갈 경우
        public static void GoToLobby()
        {
            suddenQuitData.GoToLobby();
        }
    }

    internal virtual void FuncAfterShowLoadingEnd()
    {
    }
}
