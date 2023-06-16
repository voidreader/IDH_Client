using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class PvPResultPara : ParaBase
{
    internal InGameType type;
	internal bool win;						// 클리어 여부
	internal int playerTeamIdx;   // 사용한 팀 번호
	internal int myCombat;				// 플레이어 전투력
	internal int oppCombet;				// 상대 전투력
	internal TimeSpan playTime;   // 플레이 시간
	public long historyUID = -1; // Revenge일경우 UID. 아닐경우 -1
}

internal class RaidResultPara : ParaBase
{
    internal InGameType type;
    internal bool win;
    internal int playerTeamIdx;

    internal int raidStageKey;
    internal int addedDmg;
}

internal class PvPResultSys : SubSysBase
{
	UIPvPResult ui;

	internal PvPResultSys() : base(SubSysType.PvPResult)
	{
        needInitDataTypes = new InitDataType[] {
            InitDataType.Character,
            InitDataType.Team,
        };

        preloadingBundleKeys = new int[] {
                CommonType.DEF_KEY_BGM_RESULT_W,
                CommonType.DEF_KEY_BGM_RESULT_L,
                CommonType.DEF_KEY_SFX_UI
        };
    }

	protected override void EnterSysInner(ParaBase _para)
	{
		base.EnterSysInner(_para);

		var para = _para.GetPara<PvPResultPara>();

		bool isPlacement = !GameCore.Instance.PlayerDataMgr.PvPData.placement;

		if (ui == null)
        {
            if (!UIManager.Instance.GetParentPage())
            {
                GameObject uiRoot = GameObject.Find("UI Root");
                GameObject pageRoot = UtilityFunc.Inst.GetChildObj(uiRoot, "Page");
                if (!pageRoot)
                    pageRoot = new GameObject("Page");
                pageRoot.transform.parent = uiRoot.transform;
                NGUITools.SetLayer(pageRoot, LayerMask.NameToLayer("UI"));

                UIManager.Instance.SetParentPage(pageRoot);
                pageRoot.transform.localScale = new Vector3(1, 1, 1);

            }


			ui = UIManager.Instance.GetPage<UIPvPResult>(EUIPage.UIPvPResult);
            //ui.Init(para.type, para.win, para.playerTeamIdx);
        }
        if (para.win)
            GameCore.Instance.SoundMgr.SetBGMSound(BGMType.BMSND_Mission_Success, false, false);
        //GameCore.Instance.SndMgr.PlayBGM(BGM.Result_Win, false);
        else
            GameCore.Instance.SoundMgr.SetBGMSound(BGMType.BMSND_Mission_Fail, false, false);
        //GameCore.Instance.SndMgr.PlayBGM(BGM.Result_Lose, false);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.Sfx_StageClear);

        if (para.historyUID != -1)
			GameCore.Instance.NetMgr.Req_MyRoomFinishRevenge(para.win, para.historyUID);
		else if(isPlacement)
			GameCore.Instance.NetMgr.Req_PvPFinishPlacement(para.win);
		else
			GameCore.Instance.NetMgr.Req_PvPFinishBattle(para.myCombat, para.oppCombet, para.win);
	}

	protected override void RegisterHandler()
	{
		handlerMap = new Dictionary<GameEventType, Func<ParaBase, bool>>();
		handlerMap.Add(GameEventType.ANS_PVP_FINISHPLACEMENT, ANS_PVP_FINISHPLACEMENT);
		handlerMap.Add(GameEventType.ANS_PVP_FINISHBATTLE, ANS_PVP_FINISHBATTLE);
		handlerMap.Add(GameEventType.ANS_MYROOM_FINISH_REVENGE, ANS_MYROOM_FINISH_REVENGE);
		base.RegisterHandler();
	}

	protected override void ExitSysInner(ParaBase _para)
	{
		base.ExitSysInner(_para);

		GameObject.Destroy(ui.gameObject);
		ui = null;

        GameCore.Instance.SoundMgr.SetMainBGMSound();
        //GameCore.Instance.SndMgr.PlayBGM(BGM.Lobby);
    }
	private bool ANS_PVP_FINISHPLACEMENT(ParaBase _para)
	{
		var para = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		para.GetField(ref code, "result");

		switch (code)
		{
			case 0:
				GameCore.Instance.PlayerDataMgr.PvPData.SetData(para.GetField("PVP")[0]);
				var grade = GameCore.Instance.PlayerDataMgr.PvPData.grade;
				ui.UpdatePlacement(grade);

				return true;

			case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
			case 2: GameCore.Instance.ShowNotice("실패", "요청 파라미터 누락", 0); break;
			default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
		}

		return false;
	}

	private bool ANS_PVP_FINISHBATTLE(ParaBase _para)
	{
		var para = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		para.GetField(ref code, "result");

		switch (code)
		{
			case 0:
				GameCore.Instance.PlayerDataMgr.PvPData.SetData(para.GetField("PVP")[0]);
				int addedScore = 0;
				para.GetField(ref addedScore, "SCORE");
				var rank = 0;
				para.GetField(ref rank, "GROUP_RANK");
				var rankGap = rank - GameCore.Instance.PlayerDataMgr.PvPGroupRank;
				GameCore.Instance.PlayerDataMgr.PvPGroupRank = rank;


				// Todo : Update Point Animation
				ui.UpdateResult(addedScore, rankGap);

				return true;

			case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
			case 2: GameCore.Instance.ShowNotice("실패", "요청 파라미터 누락", 0); break;
			case 3: GameCore.Instance.ShowNotice("실패", "시즌 종료", 0);
				GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
				break;
			default:GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0);	break;
		}

		return false;
	}

	private bool ANS_MYROOM_FINISH_REVENGE(ParaBase _para)
	{
		var para = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		para.GetField(ref code, "result");
		switch (code)
		{
			case 0:
				ui.PlayerUnitPSirtes();
				// Do Nothing
				return true;
			case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
			default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
		}
		return false;
	}
}
