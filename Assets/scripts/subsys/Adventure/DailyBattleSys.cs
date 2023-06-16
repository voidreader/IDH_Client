using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class DailyBattleSys : BattleSysBase
{
    int stageKey;

    public DailyBattleSys() : base(SubSysType.DailyBattle)
    {
    }
    protected override void Init()
    {
        var battlePara = para.GetPara<BattlePara>();
        stageKey = battlePara.stageId;
        var dungeonData = GameCore.Instance.DataMgr.GetDailyDungeonData(battlePara.stageId);
        StageDataMap stageData = GameCore.Instance.DataMgr.GetStageData(dungeonData.stageKey);
        if (stageData == null)
        {
            Debug.LogError("Error: Read Failed Stage Data");
            return;
        }
        targetCount = stageData.teamCnt;

        unitPool.Init(battlePara, stageData);
        battleField = BattleField.Create(GameCore.Instance.world_root);
        battleField.Init(stageData.bg_Ids);
        ui = BattleUI.Create(GameCore.Instance.ui_root);
        ui.Init(battlePara);

        //Instantiate Unit
        for (int i = 0; i <= targetCount; ++i)
        {
            var tf = InstantiateTeam(i);
            if (i == 0)
                battleField.SetMoveTeam(tf);
        }
        if (CheckFriendAble(battlePara))
        {
            var friendTf = InstantiateFriendTeam(-1);
            battleField.SetFriendTeam(friendTf);
            Vector3 friendPos = new Vector3(-7, 6, 0);
            GameCore.Instance.DoWaitCall(() =>
            {
                for (int i = 0; i < 6; i++)
                {
                    BattleUnitData unitData = unitPool.GetFriendUnitByTeam(-1, i);
                    if (unitData == null)
                        continue;
                    unitData.Transform.localPosition += friendPos;
                }
            });
        }
    }

    protected override void EndBattle()
    {
        GameCore.Instance.CloseMsgWindow();
        var resultPara = new ResultDailyPara()
        {
            type = gameType,
            DDungeonKey = stageKey,
            clear = playerWin,
            playerTeamIdx = para.playerTeam,
            playTime = ui.GetPlayTime(),
            isDead = CheckDead()
        };
        GameCore.Instance.ChangeSubSystem(SubSysType.Result, resultPara);
    }

}
