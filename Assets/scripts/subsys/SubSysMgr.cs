using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IDH.MyRoom;

public enum SubSysType : byte
{
	None = 255,			    // 프로그램 종료
	Common = 0,             // 공용(메시지 알람)
//	TopBottom,				// TopBottom UI
//  Splash,					// CI
	Loading,				// 시작화면
	Lobby,                  // 로비
	Story,					// 스토리
	SelectStage,		    // 스테이지 선택화면
	Battle,					// 전투
	Result,					// 결과화면

    Bag,                    // 가방
	EditTeam,				// 팀편집
	Farming,                // 파밍
	Inven,                  // 인벤토리
	Gacha,					// 뽑기
	Make,					// 제조
    HeroInfo,               // 영웅정보
    HeroUp,                 // 영웅 강화
    ItemUp,                 // 장비 강화
    EquipItem,              // 장비 장착

    DailyPrepare,           // 요일던전
    DailyBattle,

    RaidPrepare,            // 레이드
    RaidBattle,

	// Todo : 서브 시스템 추가(enum)
    MyRoom,                 // MyRoom
    PvPReady,
    PvPBattle,
    PvPResult,
    PvPGradeTest,
    PvPMatch,

    Shop,                   // 상점.
    Attendance,             // 출첵


}




public class ChangeSysPara : ParaBase
{
	public SubSysType nextSysType;		// 열릴 서브시스템 타입
	public ParaBase para;							// 각 서브 시스템에게 전달될 데이터

	internal ChangeSysPara(SubSysType _type, ParaBase _para)
	{
		nextSysType = _type;
		para = _para;
	}
}

public class SubSysMgr : IEventHandler
{
	private Dictionary<SubSysType, SubSysBase> subSysMap;
	private SubSysBase prevSys;
	private SubSysBase nowSys;

	private Dictionary<SubSysType, SubSysType> subSysReturnMap; // 리턴백 시  이동될 서브시스템 테이블

	public SubSysType NowSysType { get { return nowSys.subSysType; } }

    public bool IsChanging { get; private set; }

    //int connt;
    //public int NeedPreloadDataCnt { get { return connt; } private set { connt = value; Debug.Log(connt); } } 
    public int NeedPreloadDataCnt { get; private set; }  // 씬 진입전 로딩되어야하는 유저데이터의 개수 + 로드되어야하는 번들의 개수

    public SubSysMgr()
	{
		prevSys = null;
		nowSys = null;
		subSysMap = new Dictionary<SubSysType, SubSysBase>();
		SubSysBase subSys = null;
		subSys = new CommonSys();			subSysMap.Add(subSys.subSysType, subSys); subSys.EnterSys(null);	// 이벤트 핸들러 고정
//	subSys = new SplashSys();				subSysMap.Add(subSys.subSysType, subSys);
		subSys = new LoadingSys();			subSysMap.Add(subSys.subSysType, subSys);
		subSys = new LobbySys();				subSysMap.Add(subSys.subSysType, subSys);
		subSys = new SelectStageSys();	        subSysMap.Add(subSys.subSysType, subSys);
		subSys = new BattleSys();				subSysMap.Add(subSys.subSysType, subSys);
		subSys = new ResultSys();				subSysMap.Add(subSys.subSysType, subSys);
		subSys = new EditTeamSys();			    subSysMap.Add(subSys.subSysType, subSys);
		subSys = new FarmingSys();			    subSysMap.Add(subSys.subSysType, subSys);
		subSys = new InvenSys();				subSysMap.Add(subSys.subSysType, subSys);
		subSys = new MyRoomSys();               subSysMap.Add(subSys.subSysType, subSys);
        subSys = new DailyDungeonSys();         subSysMap.Add(subSys.subSysType, subSys);
        subSys = new DailyBattleSys();          subSysMap.Add(subSys.subSysType, subSys);
        subSys = new RaidPrepareSys();          subSysMap.Add(subSys.subSysType, subSys);

		subSys = new GachaSys();				subSysMap.Add(subSys.subSysType, subSys);
		subSys = new MakeSys();					subSysMap.Add(subSys.subSysType, subSys);
		subSys = new StorySys();				subSysMap.Add(subSys.subSysType, subSys);
		subSys = new PvPReadySys();				subSysMap.Add(subSys.subSysType, subSys);
        subSys = new PvPBattleSys();            subSysMap.Add(subSys.subSysType, subSys);
        subSys = new PvPResultSys();            subSysMap.Add(subSys.subSysType, subSys);
        subSys = new PvPGradeTestSys();         subSysMap.Add(subSys.subSysType, subSys);
        subSys = new PvPMatchSys();             subSysMap.Add(subSys.subSysType, subSys);
        subSys = new HeroInfoSys();             subSysMap.Add(subSys.subSysType, subSys);
        subSys = new HeroUpSys();               subSysMap.Add(subSys.subSysType, subSys);
        subSys = new ItemUpSys();               subSysMap.Add(subSys.subSysType, subSys);
        subSys = new EquipItemSys();            subSysMap.Add(subSys.subSysType, subSys);
        subSys = new RaidBattleSys();           subSysMap.Add(subSys.subSysType, subSys);
        subSys = new ShopSys();                 subSysMap.Add(subSys.subSysType, subSys);
        subSys = new AttendanceSys();           subSysMap.Add(subSys.subSysType, subSys);

        // Todo : 서브시스템 추가(테이블)

        // 서브시스템 변경 이벤트 핸들러 등록
        GameCore.Instance.EventMgr.RegisterHandler(this, GameEventType.ChangeSys,
                                                         GameEventType.ANS_INITDATA,
                                                         GameEventType.ANS_MYROOM_GETBUFFLIST);


		// 서브시스템 리턴서브시스템 맵테이블
		subSysReturnMap = new Dictionary<SubSysType, SubSysType>();
		subSysReturnMap.Add(SubSysType.Lobby, SubSysType.None);
		subSysReturnMap.Add(SubSysType.SelectStage, SubSysType.Lobby);
		subSysReturnMap.Add(SubSysType.Battle, SubSysType.Lobby);
		subSysReturnMap.Add(SubSysType.Result, SubSysType.Lobby);
		subSysReturnMap.Add(SubSysType.EditTeam, SubSysType.Lobby);
		subSysReturnMap.Add(SubSysType.Farming, SubSysType.Lobby);

		subSysReturnMap.Add(SubSysType.Inven, SubSysType.Lobby);
        subSysReturnMap.Add(SubSysType.MyRoom, SubSysType.Lobby);
		subSysReturnMap.Add(SubSysType.Gacha, SubSysType.Lobby);
		subSysReturnMap.Add(SubSysType.Make, SubSysType.Lobby);
		subSysReturnMap.Add(SubSysType.Story, SubSysType.Lobby);
		subSysReturnMap.Add(SubSysType.PvPReady, SubSysType.Lobby);
		subSysReturnMap.Add(SubSysType.PvPBattle, SubSysType.Lobby);
		subSysReturnMap.Add(SubSysType.PvPResult, SubSysType.Lobby);
        subSysReturnMap.Add(SubSysType.PvPGradeTest, SubSysType.Lobby);
        subSysReturnMap.Add(SubSysType.PvPMatch, SubSysType.Lobby);
        subSysReturnMap.Add(SubSysType.Shop, SubSysType.Lobby);
        subSysReturnMap.Add(SubSysType.Attendance, SubSysType.Lobby);


        // Todo : 서브시스템 추가(리턴 테이블)

    }

    bool IEventHandler.HandleMessage(GameEvent _evt)
    {
        if (_evt.EvtType == GameEventType.ChangeSys)
        {
            IsChanging = true;

            var para = _evt.Para.GetPara<ChangeSysPara>();

            if (nowSys == null || 
                nowSys.subSysType == SubSysType.Loading ||
                para.nextSysType == SubSysType.Loading ||
                !SetLoadingPage(para.nextSysType))
            {
                GameCore.Instance.StartCoroutine(OpenSys(_evt));
            }
            else
            {
                GameCore.Instance.CommonSys.ShowLoadingPage(true, 
                    () => GameCore.Instance.StartCoroutine(OpenSys(_evt)) );
            }
            return true;
        }
        else if (_evt.EvtType == GameEventType.ANS_INITDATA)
        {
            return ANS_INITDATA(_evt.Para);
        }
        else if (_evt.EvtType == GameEventType.ANS_MYROOM_GETBUFFLIST)
        {
            return ANS_MYROOM_GETBUFFLIST(_evt.Para);
        }

        return false;
    }

    IEnumerator OpenSys(GameEvent _evt)
    {
#if UNITY_EDITOR
        AcumulateTimer timer = new AcumulateTimer();
        timer.Begin(true);
#endif
        prevSys = nowSys;
        var para = _evt.Para.GetPara<ChangeSysPara>();
        nowSys = subSysMap[para.nextSysType];

        GameCore.Instance.CommonSys.ShowTopBottomUI(para.nextSysType);

        if (prevSys != null)
            prevSys.ExitSys(para.para);

        if (para.nextSysType != SubSysType.Loading)
        {
            // 서버에 유저 데이터가 로드되지 않았다면 대기
            while (!GameCore.Instance.PlayerDataMgr.LoadedDataToServer)
                yield return null;

            // 필요 데이터 사전 로딩 (REQ_INITDATA)
            NeedPreloadDataCnt = nowSys.PreLoading();

            while (NeedPreloadDataCnt != 0)
                yield return null;
        }



        nowSys.EnterSys(para.para);

        GameCore.Instance.DoWaitCall(3, ()=>GameCore.Instance.CommonSys.ShowLoadingPage(false,()=> {
            GameCore.Instance.SubsysMgr.nowSys.FuncAfterShowLoadingEnd();
        }));

#if UNITY_EDITOR
        timer.End();
        Debug.Log("Sys Change Time : " + timer.Delay);
#endif

        IsChanging = false;
    }

    bool SetLoadingPage(SubSysType _sysType)
    {
        switch (_sysType)
        {
            case SubSysType.Battle:
            case SubSysType.Result:
            case SubSysType.DailyBattle:
            case SubSysType.RaidBattle:
            case SubSysType.MyRoom:
            case SubSysType.PvPBattle:
            case SubSysType.PvPResult:
                return true;

            default:
                return true;
        }
    }

    public void IncNeedPreloadDataCnt()
    {
        NeedPreloadDataCnt++;
    }

    public void DecNeedPreloadDataCnt()
    {
        NeedPreloadDataCnt--;
    }

    // GameCore에 의해 매프레임 마다 호출
    internal void Update()
	{
		if( nowSys != null && !IsChanging)
			nowSys.UpdateUI();

        if ( Input.GetKeyDown(KeyCode.Escape))
		{
            // 뒤로가기 버튼으로 인한 씬 이동을 막는 조건들
            if ((GameCore.Instance.lobbyTutorial != null && GameCore.Instance.lobbyTutorial.IsRunning) 
                || IsChanging 
                || GameCore.Instance.goBlockPanel.activeInHierarchy 
                || GameCore.Instance.goBlockPanelInvisable.activeInHierarchy)
                return;

            ReturnBackSubSys();
		}
	}

	internal SubSysType GetNowSysType()
	{
		return nowSys.subSysType;
	}

	internal SubSysBase GetSubSys(SubSysType _type)
	{
		return subSysMap[_type];
	}

    internal SubSysBase GetNowSubSys()
    {
        return nowSys;
    }

    internal void ReturnBackSubSys()
	{
        if (NowSysType == SubSysType.Result || NowSysType == SubSysType.PvPResult)
            return;

        if (GameCore.Instance.NetMgr.WaitReceive)
            return;

        if (((CommonSys)subSysMap[SubSysType.Common]).GetMsgComfirmCount() != 0)
        {
            if (((CommonSys)subSysMap[SubSysType.Common]).CanBack())
            {
                if (NowSysType != SubSysType.Farming && NowSysType != SubSysType.Battle && NowSysType != SubSysType.PvPBattle && NowSysType != SubSysType.DailyBattle && NowSysType != SubSysType.RaidBattle)
                {
                    ((CommonSys)subSysMap[SubSysType.Common]).ShowMessage(null);
                }
            }
        }
        else
            nowSys.ClickBackButton();
	}

	private SubSysType nextSubSysType = SubSysType.Lobby;

	private ParaBase nextSubSysPara = null;

	internal void SetNextSubSysType(SubSysType _type, ParaBase _para =null)
	{
		nextSubSysType = _type;
		nextSubSysPara = _para;
	}

	internal SubSysType GetNextSubSysType()
	{
		return nextSubSysType;
	}

	internal ParaBase  GetNextSubSysPara()
	{
		return nextSubSysPara;
	}

    bool ANS_INITDATA(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        if (code == 0)
        {
            int type = 0;
            para.GetField(ref type, "TYPE");
            GameCore.Instance.PlayerDataMgr.InitDataOf((InitDataType)type, para.GetField("LIST"));

            NeedPreloadDataCnt--;
        }
        else
        {
            GameCore.Instance.ShowNotice("실패", "유저데이터 불러오기 실패" + code, 0);
        }

        return true;
    }


    bool ANS_MYROOM_GETBUFFLIST(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        if (code == 0)
        {
            int type = 0;
            para.GetField(ref type, "TYPE");
            GameCore.Instance.PlayerDataMgr.InitDataOf((InitDataType)type, para.GetField("LIST"));

            NeedPreloadDataCnt--;
        }
        else
        {
            GameCore.Instance.ShowNotice("실패", "숙소 버프 리스트 가져오기 실패" + code, 0);
        }

        return true;
    }

}
