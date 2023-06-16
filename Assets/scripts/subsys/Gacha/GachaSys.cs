using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class GachaSys : SubSysBase, ISequenceAction
{
	GachaUI ui;

	////////////임시 변수들 /////////////////
	internal static DateTime heroN_1;
	internal static int heroN_10;
	internal static int heroS_1;
	internal static int heroS_10;

	internal GachaSys() :base(SubSysType.Gacha)
	{
        needInitDataTypes = new InitDataType[] {
            InitDataType.Gacha,
        };
    }

	protected override void EnterSysInner(ParaBase _para)
	{
		Name = "뽑기";

		ui = GachaUI.Create(GameCore.Instance.ui_root);
		ui.Init();

        if(_para != null)
        {
            var para = _para.GetPara<StoryPara>();
            ui.SwitchingTab(para.storyKey);
        }
        GameCore.Instance.SetTutorialChild(true, 0, GetTutorialActionList, ui.GetTutorialTransformList);
        GameCore.Instance.SoundMgr.SetMainBGMSound();
        //GameCore.Instance.SndMgr.PlayBGM(BGM.GachaScene);
        base.EnterSysInner(_para);
	}

	protected override void RegisterHandler()
	{
		handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();
		handlerMap.Add(GameEventType.ANS_GACHA, ANS_GACHA);
		base.RegisterHandler();
	}

	protected override void ExitSysInner(ParaBase _para)
	{
		GameObject.Destroy(ui.gameObject);
		base.ExitSysInner(_para);
	}



	private bool ANS_GACHA(ParaBase _para)
	{
		var para = _para.GetPara<PacketPara>().data.data;
		int code = -1;
		para.GetField(ref code, "result");

		switch(code)
		{
			case 0:
				var list = GameCore.Instance.PlayerDataMgr.SetRewardItems(para.GetField("REWARD"));
                if (list != null) GameCore.Instance.ShowReceiveItem(list);
                else              GameCore.Instance.ShowNotice("실패", "잘못된 데이터", 0);

                GameCore.Instance.PlayerDataMgr.SetDataGacha(para.GetField("GACHA"), true);
                ui.ResetLists();

				return true;

            case 4:
                GameCore.Instance.ShowNotice("뽑기 실패", "무료 뽑기를 하려면 시간이 지나야 합니다.", 0);

                GameCore.Instance.PlayerDataMgr.SetDataGacha(para.GetField("GACHA"), true);
                ui.ResetLists();

                break;

            case 1: GameCore.Instance.ShowNotice("뽑기 실패", "잘못된 데이터", 0); break;
			case 2: GameCore.Instance.ShowNotice("뽑기 실패", "슬롯이 부족합니다. 공간을 확보해주세요.", ()=> {
                GameCore.Instance.CloseMsgWindow();
                if (ui.switchingNum == 1)   // 장비 뽑기라면
                    GameCore.Instance.ChangeSubSystem(SubSysType.Inven, new StoryPara(1, false));
                else                        // 영웅 뽑기라면
                    GameCore.Instance.ChangeSubSystem(SubSysType.Inven, null);
            }, 0); break;
			case 3: GameCore.Instance.ShowNotice("뽑기 실패", "재화가 부족합니다.", 0); break;
			default:GameCore.Instance.ShowNotice("뽑기 실패", "알 수 없는 에러 : " + code, 0);	break;
		}

		return true;
	}

    internal override void ClickBackButton()
    {
        if(GameCore.Instance.CommonSys.tbUi.GetFriendType() == true) GameCore.Instance.CommonSys.tbUi.OnClickCloseFriend();
        else if (GameCore.Instance.CommonSys.tbUi.GetActiveMenu()) GameCore.Instance.CommonSys.tbUi.OnCloseMenu();
        else if (!ui.GetNowGachaPlaying()) GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
    }

    public List<Action> GetTutorialActionList(int tutorialNum)
    {
        List<Action> nActionList = new List<Action>();
        switch (tutorialNum)
        {
            case 2:
                nActionList.Add(() => { });
                break;
            default:
                break;
        }
        return nActionList;
    }
}
