using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class SelectStagePara : ParaBase
{
}

internal class SelectStageSys : SubSysBase
{
	SelectStageUI ui;

	public SelectStageSys() : base(SubSysType.SelectStage)
	{
        needInitDataTypes = new InitDataType[] {
            InitDataType.Story,
        };
    }

	protected override void EnterSysInner(ParaBase _para)
	{
		base.EnterSysInner(_para);

		if( ui == null)
		{
			ui = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("SelectStage/PanelSelectStageUI", GameCore.Instance.ui_root).GetComponent<SelectStageUI>();
			ui.Init(CBClickStage, CBClickBack);
		}

        GameCore.Instance.SoundMgr.SetMainBGMSound();
        //GameCore.Instance.SndMgr.PlayBGM(BGM.SelectStage);
    }

	protected override void ExitSysInner(ParaBase _para)
	{
		base.ExitSysInner(_para);
		if (ui != null )
		{
			GameObject.Destroy(ui.gameObject);
			ui = null;
		}
	}


	private void CBClickStage()
	{
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        GameCore.Instance.ChangeSubSystem(SubSysType.Battle, null);//new BattlePara() { playerTeam = 0, stageId = 1 });
	}

	private void CBClickBack()
	{
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Cancel);
        GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, new LobbyPara() { });
	}
}
