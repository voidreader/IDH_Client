using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class MakeSys : SubSysBase
{
	MakeUI ui;
	
	internal MakeSys() : base(SubSysType.Make)
	{
        needInitDataTypes = new InitDataType[] {
            InitDataType.Item,
        };
    }

	protected override void EnterSysInner(ParaBase _para)
	{
		Name = "제조";

		ui = MakeUI.CreateMakeUI();
		ui.Init();

        if( _para != null)
        {
            var para = _para.GetPara<StoryPara>();
            ui.SwitchingTab(para.storyKey);
        }
        GameCore.Instance.SoundMgr.SetMainBGMSound();
        //GameCore.Instance.SndMgr.PlayBGM(BGM.MakeScene);

        AutonomyTutorial.AutoRunTutorial(AutonomyTutoType.Manufact, 0, 1);

        base.EnterSysInner(_para);
	}

	protected override void RegisterHandler()
	{
		handlerMap = new Dictionary<GameEventType, Func<ParaBase, bool>>();
		handlerMap.Add(GameEventType.ANS_MAKE_START, ANS_MAKE);
		handlerMap.Add(GameEventType.ANS_MAKE_DONE, ANS_MAKE_DONE);
        handlerMap.Add(GameEventType.ANS_MAKE_TAKEALL, ANS_MAKE_TAKEALL);
        handlerMap.Add(GameEventType.ANS_MAKE_QUICK, ANS_MAKE_QUICK);
		handlerMap.Add(GameEventType.ANS_MAKE_UNLOCK, ANS_MAKE_UNLOCK);
		base.RegisterHandler();
	}

	protected override void ExitSysInner(ParaBase _para)
	{
		GameObject.Destroy(ui.gameObject);
		base.ExitSysInner(_para);
	}


	private bool ANS_MAKE(ParaBase _para)
	{
		var para = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		para.GetField(ref code, "result");

		switch (code)
		{
			case 0:
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Make_Start);
                GameCore.Instance.PlayerDataMgr.SetMakeData(para.GetField("MAKING"));

				var rewardList = GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
                //GameCore.Instance.ShowReceiveItem(rewardList); // 빠진것 뿐이기 때문에. 출력할 필요 없음

                AutonomyTutorial.AutoRunTutorial(AutonomyTutoType.Manufact, 1, 1);

                ui.UpdateSlotsNowTab();
				return true;

			case 1:		GameCore.Instance.ShowNotice("제조 실패", "쿼리 오류", 0); break;
			case 2:		GameCore.Instance.ShowNotice("제조 실패", "재료가 부족합니다.", 0); break;
			case 3:		GameCore.Instance.ShowNotice("제조 실패", "제조 불가 상태", 0); break;
			default:	GameCore.Instance.ShowNotice("제조 실패", "알 수 없는 에러 " + code, 0); break;
		}

		return false;
	}


	private bool ANS_MAKE_DONE(ParaBase _para)
	{
		var para = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		para.GetField(ref code, "result");

		switch (code)
		{
			case 0:
                // Update Make Slot
                GameCore.Instance.PlayerDataMgr.SetMakeData(para.GetField("MAKING"));
				var rewardList = GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
				GameCore.Instance.ShowReceiveItem(rewardList);
				ui.UpdateSlotsNowTab();
				
				return true;

			case 1: GameCore.Instance.ShowNotice("제조 실패", "쿼리 오류", 0); break;
            case 3: GameCore.Instance.ShowNotice("제조 실패", "제조 중", 0); break;
			case 4: GameCore.Instance.ShowNotice("제조 실패", "잘못된 MAKING_ID.", 0); break;
			case 5: GameCore.Instance.ShowNotice("제조 실패", "잘못된 보상아이템", 0); break;
			default: GameCore.Instance.ShowNotice("제조 실패", "알 수 없는 에러 " + code, 0); break;

            case 2:
                GameCore.Instance.ShowNotice("제조 실패", "슬롯이 부족합니다. 공간을 확보해주세요.", () =>
                {
                    GameCore.Instance.CloseMsgWindow();
                    if (ui.switchingNum == 1)   // 장비 제조라면
                        GameCore.Instance.ChangeSubSystem(SubSysType.Inven, new StoryPara(1, false));
                    else                        // 영웅 제조라면
                        GameCore.Instance.ChangeSubSystem(SubSysType.Inven, null);
                }, 0);
                break;
        }

		return false;
	}

    
    private bool ANS_MAKE_TAKEALL(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                GameCore.Instance.PlayerDataMgr.SetMakeData(para.GetField("MAKING"));
                var rewardList = GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
                GameCore.Instance.ShowReceiveItem(rewardList);
                ui.UpdateSlotsNowTab();

                return true;

            case 1: GameCore.Instance.ShowNotice("제조 실패", "쿼리 오류", 0); break;
            case 3: GameCore.Instance.ShowNotice("제조 실패", "완료 가능한 제조가 없습니다.", 0); break;
            default: GameCore.Instance.ShowNotice("제조 실패", "알 수 없는 에러 " + code, 0); break;

            case 2:
                GameCore.Instance.ShowNotice("제조 실패", "슬롯이 부족합니다. 공간을 확보해주세요.", () =>
                {
                    GameCore.Instance.CloseMsgWindow();
                    if (ui.switchingNum == 1)   // 장비 제조라면
                        GameCore.Instance.ChangeSubSystem(SubSysType.Inven, new StoryPara(1, false));
                    else                        // 영웅 제조라면
                        GameCore.Instance.ChangeSubSystem(SubSysType.Inven, null);
                }, 0);
                break;

        }

        return false;
    }


    private bool ANS_MAKE_QUICK(ParaBase _para)
	{
		var para = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		para.GetField(ref code, "result");

		switch (code)
		{
			case 0:
				// Update Player Data
				GameCore.Instance.PlayerDataMgr.SetMakeData(para.GetField("MAKING"));

				var rewardList = GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
				GameCore.Instance.ShowReceiveItem(rewardList);

				ui.UpdateSlotsNowTab();
				return true;

			case 1: GameCore.Instance.ShowNotice("제조 실패", "쿼리 오류", 0); break;
			case 3: GameCore.Instance.ShowNotice("빠른 제조 불가", "제조권이 부족하여 제조를 완료할 수 없습니다.", 0); break;
			case 4: GameCore.Instance.ShowNotice("제조 실패", "잘못된 아이디", 0); break;
			case 5: GameCore.Instance.ShowNotice("제조 실패", "잘못된 보상아이템 에러", 0); break;
			default: GameCore.Instance.ShowNotice("제조 실패", "알 수 없는 에러 " + code, 0); break;

            case 2:
                GameCore.Instance.ShowNotice("제조 실패", "슬롯이 부족합니다. 공간을 확보해주세요.", () =>
                {
                    GameCore.Instance.CloseMsgWindow();
                    if (ui.switchingNum == 1)   // 장비 제조라면
                        GameCore.Instance.ChangeSubSystem(SubSysType.Inven, new StoryPara(1, false));
                    else                        // 영웅 제조라면
                        GameCore.Instance.ChangeSubSystem(SubSysType.Inven, null);
                }, 0);
                break;
        }

		return false;
	}

	internal bool ANS_MAKE_UNLOCK(ParaBase _para)
	{
		var para = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		para.GetField(ref code, "result");

		switch (code)
		{
			case 0:
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Make_Open);
                // Update Player Data
                GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
				GameCore.Instance.PlayerDataMgr.SetMakeData(para.GetField("MAKING"));
				ui.UpdateSlotsNowTab();
				GameCore.Instance.CommonSys.UpdateMoney();
				return true;

			case 1: GameCore.Instance.ShowNotice("슬롯 확장 실패", "쿼리 오류", 0); break;
			case 2: GameCore.Instance.ShowNotice("슬롯 확장 실패", "재화부족", 0); break;
			case 3: GameCore.Instance.ShowNotice("슬롯 확장 실패", "갱신해야 함", 0);
				GameCore.Instance.PlayerDataMgr.SetMakeData(para.GetField("MAKING"));
				ui.UpdateSlotsNowTab();
				GameCore.Instance.CommonSys.UpdateMoney();
				break;
			default: GameCore.Instance.ShowNotice("슬롯 확장 실패", "알 수 없는 에러 " + code, 0); break;
		}

		return false;
	}

    internal override void ClickBackButton()
    {
        if(GameCore.Instance.CommonSys.tbUi.GetFriendType() == true) GameCore.Instance.CommonSys.tbUi.OnClickCloseFriend();
        else if (GameCore.Instance.CommonSys.tbUi.GetActiveMenu()) GameCore.Instance.CommonSys.tbUi.OnCloseMenu();
        else if (!ui.GetNowGachaPlaying()) GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
    }
}
