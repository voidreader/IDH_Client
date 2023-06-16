using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;


internal class RaidBattlePara : BattlePara
{
    internal int damage;
}


internal class RaidBattleSys : BattleSysBase
{
    internal RaidBattleSys() : base(SubSysType.RaidBattle)
    {
    }

    float totalDmg;

    protected override void Init()
    {
        var battlePara = para.GetPara<RaidBattlePara>();

        targetCount = 1;
        totalDmg = battlePara.damage;

        var bg_Ids = new int[3] {
                99001,
                99501,// defualt wall
                99502 // defualt Ground
        };
        battleField = BattleField.Create(GameCore.Instance.world_root);
        battleField.Init(bg_Ids);

        var data = GameCore.Instance.DataMgr.GetRaidData(battlePara.stageId);
        unitPool.Init(battlePara, new HeroSData(data.bossKey));
        ui = RaidBattleUI.Create(GameCore.Instance.ui_root);

        ui.Init(battlePara);

        //Instantiate Unit
        battleField.SetMoveTeam(InstantiateTeam(0));
        battleField.SetEnemyTeam(InstantiateTeam(1,Vector3.zero, true));

        var boss = unitPool.GetUnitByTeam(1, 0);
        //ui.RemoveUnitStatusUI(boss.FieldId);
        boss.StatusUI = ((RaidBattleUI)ui).GetBossStatusUI();
        ui.SetUnitStatusUI(boss.FieldId, boss.StatusUI);
        ((RaidBattleUI)ui).SetBossStatusUI(boss, battlePara.damage);

        //var stausUI = ui.GetUnitStatusUI(unitPool.GetUnitByTeam(1, 0));
        //if (stausUI != null) stausUI.SetHp(hp);
    }

    protected override void ChangedBattleState(BattleSysState _state)
    {
        if(_state == BattleSysState.Battle_Before)
        {
            ((RaidBattleUI)ui).GetBossStatusUI().gameObject.SetActive(true);
        }
    }

    protected override void EndBattle()
    {
        GameCore.Instance.CloseMsgWindow();
        var resultPara = new ResultRaidPara()
        {
            type = gameType,
            clear = playerWin,
            addedDmg = (int)totalDmg,
            raidKey = para.stageId,
            playerTeamIdx = para.playerTeam,
            playTime = ui.GetPlayTime()
        };
        GameCore.Instance.ChangeSubSystem(SubSysType.Result, resultPara);
    }

    internal override float AddUnitHP(int _tgFieldId, float _value)
    {
        int team = BattleUnitPool.GetTeamByFieldId(_tgFieldId);
        if (team == 1)
            totalDmg -= _value;

        return base.AddUnitHP(_tgFieldId, _value);
    }
}
