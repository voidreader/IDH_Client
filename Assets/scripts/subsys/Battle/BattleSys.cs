using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;




internal class BattlePara : BattleParaBase
{
    // Story Datas
    public int stageId;
    public string friendName;
    public List<PvPOppUnitSData> friendUnitList;
    public int friendTeamSkill;
    public int friendIcon;
    public List<MissionInfo> missionInfoList;

    internal static BattlePara CreateStoryPara(int _playerTeamIdx, int _stageID)
    {
        return new BattlePara() {
            type = InGameType.Story,
            playerTeam = _playerTeamIdx,
            stageId = _stageID };
    }
}

/// <summary>
/// 전투 시스템
/// </summary>
internal class BattleSys : BattleSysBase, ISequenceAction
{
    int stageKey;
    public List<MissionInfo> missionInfoList;

    internal BattleSys() : base(SubSysType.Battle)
	{
	}

    protected override void Init()
    {
        var battlePara = para.GetPara<BattlePara>();

        stageKey = battlePara.stageId;
        playerTeam = battlePara.playerTeam;
        StoryDataMap storyData = GameCore.Instance.DataMgr.GetStoryData(battlePara.stageId);

        Debug.Log("storyData : " + storyData.chapter + " " + storyData.stage +  " " + storyData.stageID + " ");
        StageDataMap stageData = GameCore.Instance.DataMgr.GetStageData(storyData.stageID);

        for (int i = 0; i < stageData.monster_Ids.GetLength(0); ++i)
        {
            //var monIds = _csvData[idx++].Split(new char[] { ',' });
            for (int j = 0; j < stageData.monster_Ids.GetLength(1); j++)
            {
                //ToParse(monIds[j], out monster_Ids[i, j]);
                Debug.Log(stageData.monster_Ids[i, j]);
            }

        }

        canUseSkillEnmey = true;

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

        BattleUI battleUI = ui as BattleUI;
        GameCore.Instance.SetTutorialChild(true, 0, GetTutorialActionList, battleUI.GetTutorialTransformList);

        //Instantiate Unit
        for (int i = 0; i <= targetCount; ++i)
        {
            var tf = InstantiateTeam(i);
            if (i == 0)
                battleField.SetMoveTeam(tf);
        }
        if(CheckFriendAble(battlePara))
        {
            var friendTf = InstantiateFriendTeam(-1);
            battleField.SetFriendTeam(friendTf);
            Vector3 friendPos = new Vector3(-7, 6, 0);
            GameCore.Instance.DoWaitCall(() => {
                for (int i = 0; i < 6; i++)
                {
                    BattleUnitData unitData = unitPool.GetFriendUnitByTeam(-1, i);
                    if (unitData == null)
                        continue;
                    unitData.Transform.localPosition += friendPos;
                }
            });
        }
        missionInfoList = battlePara.missionInfoList;
    }

    protected override void EndBattle()
    {
        GameCore.Instance.CloseMsgWindow();
        bool[] missionflag;
        CheckMission(missionInfoList, out missionflag);
        Debug.Log("Mission : " + missionflag[0] + ", " + missionflag[1] + ", " + missionflag[2]);
        var resultPara = new ResultStoryPara()
        {
            type = gameType,
            storyKey = stageKey,
            clear = playerWin,
            DeadMonsters = killedMonsterID.ToArray(),
            mission = missionflag,
            ovkDmg = (int)overKillDmg,
            playerTeamIdx = para.playerTeam,
            playTime = ui.GetPlayTime(),

           isDead = CheckDead()
        };
        GameCore.Instance.ChangeSubSystem(SubSysType.Result, resultPara);
    }

    private void CheckMission(List<MissionInfo> _missionInfoList, out bool[] _missionflag)
    {
        var data = _missionInfoList;
        var missionCleared = GameCore.Instance.PlayerDataMgr.GetStorySData(stageKey).missionClear;
        _missionflag = new bool[_missionInfoList.Count];
        for (int i = 0; i < _missionflag.Length; ++i)
        {
            bool result = false;
            switch (_missionInfoList[i].missionID)
            {
                case CommonType.DEF_KEY_MISSION_DEFINE + 0: // 스테이지 클리어
                    result = playerWin;
                    break;

                case CommonType.DEF_KEY_MISSION_DEFINE + 1: // 클리어시 영웅들의 체력 {0}% 이상으로 클리어
                    result = ((GetTotalHpPercentage() * 100) >= _missionInfoList[i].missionValue1) & playerWin;
                    break;

                case CommonType.DEF_KEY_MISSION_DEFINE + 2: // 받은 데미지 총합 {0} 이하로 클리어
                    result = (totalDamage <= _missionInfoList[i].missionValue1) & playerWin;
                    break;
                case CommonType.DEF_KEY_MISSION_DEFINE + 3: // 오버킬 총합 {0} 이상으로 클리어
                    result = (overKillDmg >= _missionInfoList[i].missionValue1) & playerWin;
                    break;

                case CommonType.DEF_KEY_MISSION_DEFINE + 4: // 아무도 사망하지 않고 클리어
                    result = (playerTeamDeadCount == 0) & playerWin;
                    break;

                case CommonType.DEF_KEY_MISSION_DEFINE + 5: // 팀스킬을 사용하지 않고 클리어
                    result = (useTeamSkillCount == 0) & playerWin;
                    break;
                case CommonType.DEF_KEY_MISSION_DEFINE + 6: // 전투력 총합 {0} 이하로 클리어
                    var teamPower = GameCore.Instance.PlayerDataMgr.GetTeamPower(playerTeam);
                    result = (teamPower <= _missionInfoList[i].missionValue1) & playerWin;
                    break;

                case CommonType.DEF_KEY_MISSION_DEFINE + 7: // {0} 등급 이하의 캐릭터로만 클리어
                    result = (CheckTeamRank(_missionInfoList[i].missionValue1)) & playerWin;
                    break;

                case CommonType.DEF_KEY_MISSION_DEFINE + 8: // {1} 유형 캐릭터를 {0}명 포함하여 클리어
                    result = (GetCountByUnitType(_missionInfoList[i].missionValue1) >= _missionInfoList[i].missionValue2) & playerWin;
                    break;

                case CommonType.DEF_KEY_MISSION_DEFINE + 9: // {0} 유형 캐릭터를 포함하지 않고 클리어
                    result = (GetCountByUnitType(_missionInfoList[i].missionValue1) == 0) & playerWin;
                    break;

                case CommonType.DEF_KEY_MISSION_DEFINE + 10: // {0}명 이하의 영웅만으로 클리어
                    result = (playerTeamCount <= _missionInfoList[i].missionValue1) & playerWin;
                    break;
            }

            _missionflag[i] = result | missionCleared[i];
        }
    }

    float GetTotalHpPercentage()
    {
        float max = 0;
        float now = 0;
        for (int j = 0; j < 6; ++j)
        {
            var unit = unitPool.GetUnitByTeam(0, j);
            if (unit != null)
            {
                max += unit.GetStat(UnitStat.Hp);
                now += unit.NowHp;
            }
            else
            {
                var unitData = GameCore.Instance.PlayerDataMgr.GetUnitDataByTeam(playerTeam, j);
                if (unitData != null)
                {
                    max += unitData.stats.GetStat(UnitStat.Hp);
                    // 추가 스탯이 있다면 추가할 것(강화, 아이템 등...)
                }
            }
        }

        return now / max;
    }

    bool CheckTeamRank(int _targetRank)
    {
        for(int i = 0; i < 6; ++i)
        {
            var data = GameCore.Instance.PlayerDataMgr.GetUnitDataByTeam(playerTeam, i);
            if (data != null && data.rank < _targetRank)
                return false;
        }
        return true;
    }

    int GetCountByUnitType(int _targetType)
    {
        int count = 0;
        for (int i = 0; i < 6; ++i)
        {
            var data = GameCore.Instance.PlayerDataMgr.GetUnitDataByTeam(playerTeam, i);
            if (data != null && data.charType == _targetType)
                count++;
        }
        return count;
    }

    public List<Action> GetTutorialActionList(int tutorialNum)
    {
        List<Action> nActionList = new List<Action>();
        switch (tutorialNum)
        {
            case 1:
                nActionList.Add(() => {
                    
                    //ui.GetUnitCardUI(2).SetCoolTime(0, 0);
                    var cardUI = ui.UnitCards[2];// GetUnitCardUI(unitData.FieldId);
                    BattleUnitData unitData = UnitPool.GetUnitByFieldID(cardUI.fieldId);
                    if (unitData != null)
                    {
                        unitData.SetCoolTimeAcc(0f);
                        cardUI.SetCoolTime(unitData.GetCoolTimeAcc(), unitData.GetCoolTime());
                    }
                    atkCtrl.bPauseTuto = false;
                });
                nActionList.Add(() => {
                    State = BattleSysState.Battle;
                    GameCore.Instance.RevertTime();
                    atkCtrl.bPauseTuto = false;
                });
                nActionList.Add(() => {
                    State = BattleSysState.Battle;
                    GameCore.Instance.RevertTime();
                    atkCtrl.bPauseTuto = false;
                });
                break;
            case 4:
                bBlockTeamSkil = true;
                nActionList.Add(() => {
                    bBlockTeamSkil = false;
                    atkCtrl.teamSkillAcc = 2f;
                    atkCtrl.bPauseTuto = false;
                    ui.SetTeamSkillGauge(atkCtrl.teamSkillAcc);
                });
                nActionList.Add(() => {
                    State = BattleSysState.Battle;
                    GameCore.Instance.RevertTime();
                    atkCtrl.bPauseTuto = false;
                });
                nActionList.Add(() => {
                    State = BattleSysState.Battle;
                    GameCore.Instance.RevertTime();
                    atkCtrl.bPauseTuto = false;
                });
                break;
            case 5:
                bBlockStrikeSkil = true;
                nActionList.Add(() => {
                    bBlockStrikeSkil = false;
                    atkCtrl.StrikeSkillAcc = 1f;
                    ui.SetStrikeSkillGauge(atkCtrl.StrikeSkillAcc);
                    atkCtrl.bPauseTuto = false;
                    Debug.Log("Strike no.1");
                });
                nActionList.Add(() => {
                    State = BattleSysState.Battle;
                    GameCore.Instance.RevertTime();
                    //CBClickStrikeSkill();
                    atkCtrl.bPauseTuto = false;
                    Debug.Log("Strike no.2");
                });
                nActionList.Add(() => {
                    //State = BattleSysState.Battle;
                    GameCore.Instance.RevertTime();
                    atkCtrl.bPauseTuto = false;
                    Debug.Log("Strike no.3");
                });
                break;
            default:
                break;
        }
        return nActionList;
    }
}
