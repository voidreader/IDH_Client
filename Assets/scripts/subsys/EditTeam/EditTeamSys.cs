using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ReturnSysPara : ParaBase
{
    public SubSysType returnSys;
    public ParaBase para;
    public int selectedteamIdx;

    public ReturnSysPara(SubSysType _returnSys, int _selectedteamIdx, ParaBase _para)
    {
        returnSys = _returnSys;
        selectedteamIdx = _selectedteamIdx;
        para = _para;
    }
}


internal class EditTeamSys : SubSysBase, IComparer<HeroSData>, ISequenceAction
{
    EditTeamUI ui;
    ReturnSysPara para;
    int nowTeamIdx;
    List<HeroSData> sortHeroListPower;
    List<HeroSData> heroList;
    long[,] tmpTeamData; // 수정되고 있는 상태 임시 저장. 팀변경시 업데이트 됨.

    bool waitForServer; // 백버튼으로 팀변경 저장시 서버의 응답을 기다린다.(응답시 바로 탈출)

    public EditTeamSys() : base(SubSysType.EditTeam)
    {
        tmpTeamData = new long[5, 7]; // [x,6]은 해당 팀의 팀스킬
        needInitDataTypes = new InitDataType[]
        {
            InitDataType.Character,
            InitDataType.Team,
        };

        preloadingBundleKeys = new int[] {
            CommonType.DEF_KEY_BGM_EDITTEAM,
            CommonType.DEF_KEY_SFX_UI,
        };
    }


    protected override void EnterSysInner(ParaBase _para)
    {
        base.EnterSysInner(_para);
        Name = "팀 편집";

        if (_para != null) para = _para.GetPara<ReturnSysPara>();
        else para = new ReturnSysPara(SubSysType.Lobby, 0, null);

        waitForServer = false;

        // 팀 임시 정보 업데이트
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 6; j++)
                tmpTeamData[i, j] = GameCore.Instance.PlayerDataMgr.GetTeamIds(i, j);
            tmpTeamData[i, 6] = GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(i);
        }

        ui = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("EditTeam/PanelEditTeamUI", GameCore.Instance.ui_root).GetComponent<EditTeamUI>();
        ui.Init(CBClickBtnManage, CBClickReset, CBClickClear, OnClickTeam, CBAutoTeamSet, OnClickSubmit);//, OnClickExpend);

        ui.UpdateCardMaxCount(GameCore.Instance.PlayerDataMgr.HeroSlotLimitCount);

        GameCore.Instance.SetTutorialChild(true, 0, GetTutorialActionList, ui.GetTutorialTransformList);

        SetActive(true);
        GameCore.Instance.SoundMgr.SetBGMSound(BGMType.BMSND_Scene, true, false);
        //GameCore.Instance.SndMgr.PlayBGM(BGM.EditTeamScene);
        nowTeamIdx = para.selectedteamIdx;
        CBClickReset();
        heroList = new List<HeroSData>();
        sortHeroListPower = new List<HeroSData>();
    }

    protected override void ExitSysInner(ParaBase _para)
    {
        base.ExitSysInner(_para);
        SetActive(false);
        GameObject.Destroy(ui.gameObject);
    }

    protected override void RegisterHandler()
    {
        handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();
        handlerMap.Add(GameEventType.ANS_UPDATE_TEAM, ANS_UPDATE_TEAM);
        base.RegisterHandler();
    }

    protected override void UnegisterHandler()
    {
        base.UnegisterHandler();
    }


    protected override void SetActive(bool _active)
    {
        ui.gameObject.SetActive(_active);

        //if (_active)
        //{
        //	GameCore.Instance.SetCBToReturnBtn(CBClickReturnBtn);

        //	var submit = GameCore.Instance.CreateButton("배치 완료", 22, "BTN_02_02_0", OnClickSubmit);
        //	GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.AddToBottom, AddToBottomPara.Add(false, submit)));
        //}
        //else
        //{
        //	GameCore.Instance.SetCBToReturnBtn(null);
        //	GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.AddToBottom, AddToBottomPara.Clear(false)));
        //}
    }


    private void CBClickBtnManage(long _id)
    {
        SetTempTeamData(nowTeamIdx);

        if (IsChanged())
        {
            if (IsEmptyTeam(0)) { GameCore.Instance.ShowNotice("대표팀 비어있음", "대표팀에는 반드시 1명 이상의 영웅이 배치되어야 합니다.", 0); return; }
            if (IsEmptyTeam(4)) { GameCore.Instance.ShowNotice("PVP팀 비어있음", "PVP팀에는 반드시 1명 이상의 영웅이 배치되어야 합니다.", 0); return; }

            GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, "팀 수정", new MsgAlertBtnData[] {
            new MsgAlertBtnData("아니요", new EventDelegate( () =>  {
                GameCore.Instance.CloseMsgWindow();
                Debug.Log("Manage " + _id);
                GameCore.Instance.ChangeSubSystem(SubSysType.HeroInfo, new HeroInfoPara(_id, SubSysType.EditTeam, nowTeamIdx));
            })),
            new MsgAlertBtnData("예", new EventDelegate( () =>  {
                OnClickSubmit();
                GameCore.Instance.CloseMsgWindow();
                Debug.Log("Manage " + _id);
                GameCore.Instance.ChangeSubSystem(SubSysType.HeroInfo, new HeroInfoPara(_id, SubSysType.EditTeam, nowTeamIdx));
            })),
            },
            0, "변경된 팀이 저장되지 않았습니다.\n팀편집을 저장하시겠습니까?", null)));

            return;
        }
        else
        {
            Debug.Log("Manage " + _id);
            GameCore.Instance.ChangeSubSystem(SubSysType.HeroInfo, new HeroInfoPara(_id, SubSysType.EditTeam, nowTeamIdx));
        }
    }

    internal void CBAutoTeamSet()
    {
        // var now = DateTime.Now;

        ui.SlotClear();

        //각각의 캐릭터 별 전투력 정렬.
        SortHeroListPower(GameCore.Instance.PlayerDataMgr.GetUserHeroSDataList());
        //1.팀 스킬을 활성화 할 수 있는 팀 중 전투력 합산이 가장 높은 팀 캐릭터 전원
        //heroList = TeamSkillList();
        heroList = new List<HeroSData>();


        //2.남은 자리가 있다면 현재 설정된 캐릭터와 케미스킬을 활성화 할 수 있는 캐릭터 중 가장 전투력이 높은 캐릭터
        //if (heroList.Count < 5)
        //{
        //    TeamChemistryList();
        //}

        //3.남은 자리가 있다면 남은 캐릭터 중 가장 전투력이 높은 캐릭터
        if (heroList.Count < 5)
        {
            LastSortHeroListPower();
            for (int i = 0; i < sortHeroListPower.Count; ++i)
            {
                bool cancel = false;
                if (GameCore.Instance.DataMgr.GetUnitData(sortHeroListPower[i].key).IsExpCard())
                    continue;

                for (int j = 0; j < heroList.Count; ++j)
                {
                    if (GameCore.Instance.DataMgr.GetUnitData(sortHeroListPower[i].key).charId == GameCore.Instance.PlayerDataMgr.GetUnitData(heroList[j].uid).charId)
                    {
                        cancel = true;
                        break;
                    }
                }
                if (!cancel)
                {
                    heroList.Add(sortHeroListPower[i]);
                    if (heroList.Count == 5) break;
                }
            }
        }
        //팀 참가 위치 관련
        //•전열 / 후열에 배치 불가시 후열 / 전열에 배치함(EX - 근거리 4명인 경우 전열에 3명, 후열에 1명)
        int frontIdx = 0;
        int rearIdx = 5;
        HeroSData[] data = new HeroSData[6];
        for (int i = 0; i < heroList.Count; ++i)
        {
            var tmp = GameCore.Instance.DataMgr.GetUnitData(heroList[i].key);

            if (tmp.charType <= 2)
                data[frontIdx++] = heroList[i];
            else
                data[rearIdx--] = heroList[i];
        }
        ui.SetTeam(data);
        ui.UnequipTeamSkill();

        //var gap = DateTime.Now - now;
        //Debug.Log("Ato Team Find Dealy : " + gap);
    }

    public int Compare(HeroSData _x, HeroSData _y)
    {
        var x = GameCore.Instance.DataMgr.GetUnitData(_x.key);
        var y = GameCore.Instance.DataMgr.GetUnitData(_y.key);

        return x.stats.GetStat(UnitStat.Attack).CompareTo(y.stats.GetStat(UnitStat.Attack));
    }

    private void OnClickSubmit()
    {
        SetTempTeamData(nowTeamIdx);

        if (IsEmptyTeam(0)) { GameCore.Instance.ShowNotice("팀 변경", "대표팀은 비워질 수 없습니다.", 0); return; }
        if (IsEmptyTeam(4)) { GameCore.Instance.ShowNotice("팀 변경", "PVP팀에는 반드시 1명 이상의 영웅이 배치되어야 합니다.", 0); return; }

        ui.OffAffectAllOnList();

        List<int> teamList = new List<int>();
        List<int> indexList = new List<int>();
        List<long> unitList = new List<long>();
        List<int> teamskillList = new List<int>();
        int s = (int)Mathf.Pow(10, nowTeamIdx);
        for (int i = 0; i < 5; ++i)
        {
            if (IsChanged(i) == false)
                continue;

            bool isEmpty = true;
            for (int j = 0; j < 6; ++j)
            {
                var id = tmpTeamData[i, j]; //ui.GetSlotUnitID(j);
                if (0 < id)
                {
                    isEmpty = false;
                    teamList.Add(i + 1);
                    indexList.Add(j);
                    unitList.Add(id);
                    teamskillList.Add((int)tmpTeamData[i, 6]);
                }
            }

            if (isEmpty)
            {
                teamList.Add(i + 1);
                indexList.Add(-1);
                unitList.Add(-1);
                teamskillList.Add(-1);
            }

        }
        if (teamList.Count == 0)
        {
            GameCore.Instance.ShowAlert("변경 사항이 없습니다.");
        }
        else
        {
            GameCore.Instance.NetMgr.Req_Update_Team(teamList, indexList, unitList, teamskillList);
            //   i + 1, modf.ToArray(), form.ToArray(), (int)tmpTeamData[i, 6]);
        }
    }

    // 팀 변경
    private void OnClickTeam(int _num)
    {
        SetTempTeamData(nowTeamIdx);

        nowTeamIdx = _num;
        CBClickReset(true);
    }


    private void CBClickReset()
    {
        CBClickReset(false);
    }

    private void CBClickClear()
    {
        for (int i = 0; i < tmpTeamData.GetLength(1); ++i)
            tmpTeamData[nowTeamIdx, i] = -1;
    }

    private void CBClickReset(bool _fromTmpTeamData)
    {
        ui.OffAffectAllOnList();
        ui.SetSelect(-1);

        var cnt = ui.GetSlotCount();
        HeroSData[] unit = new HeroSData[cnt];
        for (int i = 0; i < cnt; ++i)
        {
            long id = -1;
            if (_fromTmpTeamData) id = tmpTeamData[nowTeamIdx, i];
            else id = GameCore.Instance.PlayerDataMgr.GetTeamIds(nowTeamIdx, i);
            unit[i] = GameCore.Instance.PlayerDataMgr.GetUnitSData(id);
        }
        ui.SetTeam(unit);
        if (_fromTmpTeamData) ui.EquipTeamSkill((int)tmpTeamData[nowTeamIdx, 6]);
        else ui.EquipTeamSkill(GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(nowTeamIdx));
        ui.SetTeamNumButton(nowTeamIdx);
    }

    internal override void ClickBackButton()
    {
        SetTempTeamData(nowTeamIdx);

        if (IsChanged())
        {
            if (IsEmptyTeam(0)) { GameCore.Instance.ShowNotice("대표팀 비어있음", "대표팀에는 반드시 1명 이상의 영웅이 배치되어야 합니다.", 0); return; }
            if (IsEmptyTeam(4)) { GameCore.Instance.ShowNotice("PVP팀 비어있음", "PVP팀에는 반드시 1명 이상의 영웅이 배치되어야 합니다.", 0); return; }

            GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, "팀 수정", new MsgAlertBtnData[] {
            new MsgAlertBtnData("아니요", new EventDelegate( () =>  {
                GameCore.Instance.CloseMsgWindow();
				//base.ClickBackButton();
                GameCore.Instance.ChangeSubSystem(para.returnSys, para.para);
            })),
            new MsgAlertBtnData("예", new EventDelegate( () =>  {
                OnClickSubmit();
                GameCore.Instance.CloseMsgWindow();
				//base.ClickBackButton();
                waitForServer = true;

                //GameCore.Instance.ChangeSubSystem(para.returnSys, para.para);
            })),
            },
            0, "변경된 팀이 저장되지 않았습니다.\n팀편집을 저장하시겠습니까?", null)));

            return;
        }
        else if (GameCore.Instance.CommonSys.tbUi.GetFriendType() == true)
        {
            GameCore.Instance.CommonSys.tbUi.OnClickCloseFriend();
        }
        else if (GameCore.Instance.CommonSys.tbUi.GetActiveMenu()) GameCore.Instance.CommonSys.tbUi.OnCloseMenu();
        else
        {
            //base.ClickBackButton();
            GameCore.Instance.ChangeSubSystem(para.returnSys, para.para);
        }
    }

    void ExitSysByServer()
    {
        GameCore.Instance.ChangeSubSystem(para.returnSys, para.para);
    }

    private void SetTempTeamData(int _teamIdx)
    {
        for (int i = 0; i < 6; ++i)
            tmpTeamData[_teamIdx, i] = ui.GetSlotUnitID(i);
        tmpTeamData[_teamIdx, 6] = ui.GetTeamSkillKey();
    }

    internal bool IsChanged()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 6; ++j)
            {
                var preId = GameCore.Instance.PlayerDataMgr.GetTeamIds(i, j);
                var nowId = tmpTeamData[i, j];

                if (preId != nowId)
                {
                    Debug.Log("Change : [" + i + "][" + j + "] " + preId + " -> " + nowId);
                    return true;
                }
            }
            if (tmpTeamData[i, 6] != GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(i))
            {
                Debug.Log("Change Skill : [" + i + "] " + tmpTeamData[i, 6] + " -> " + GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(i));
                return true;
            }
        }
        return false;
    }

    internal bool IsChanged(int _teamIdx)
    {
        for (int j = 0; j < 6; ++j)
        {
            var preId = GameCore.Instance.PlayerDataMgr.GetTeamIds(_teamIdx, j);
            var nowId = tmpTeamData[_teamIdx, j];

            if (preId != nowId)
                return true;
        }
        if (tmpTeamData[_teamIdx, 6] != GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(_teamIdx))
            return true;

        return false;
    }

    internal bool IsEmptyTeam(int _teamIdx)
    {
        for (int j = 0; j < 6; ++j)
        {
            var nowId = tmpTeamData[_teamIdx, j];
            if (0 < nowId)
                return false;
        }

        return true;
    }


    internal List<HeroSData> TeamSkillList()
    {
        var teamSkillTable = ((DataMapCtrl<TeamSkillDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.TeamSkill)).GetEnumerator();
        //실제로 사용할 팀스킬이 가능한 캐릭터들이 담길 리스트.
        List<HeroSData> teamSkillCardList = new List<HeroSData>();
        teamSkillCardList.Clear();
        //임시로 캐릭터들을 담아 두고 있을 리스트.
        List<HeroSData> tempTeamSkillCardList = new List<HeroSData>();

        //현재 가지고 있는 팀 스킬의 갯수만큼 돌린다.
        while (teamSkillTable.MoveNext())
        {
            //새로운 팀스킬을 위해서 캐릭터들을 임시로 담아둘 리스트를 초기화한다.
            tempTeamSkillCardList.Clear();

            for (int i = 0; i < teamSkillTable.Current.Value.needChar.Length; i++)
            {
                for (int j = 0; j < sortHeroListPower.Count; ++j)
                {
                    if (teamSkillTable.Current.Value.needChar[i] == GameCore.Instance.PlayerDataMgr.GetUnitData(sortHeroListPower[j].uid).charIdType)
                    {
                        //팀스킬에 해당하는 캐릭터가 있다면 담아라.
                        tempTeamSkillCardList.Add(sortHeroListPower[j]);
                        break;
                    }
                }
                //팀 스킬에 필요한 캐릭터와 내가 담은 캐릭터의 갯수가 같다면 다 찾은거니 for문을 빠져나와라.
                if (teamSkillTable.Current.Value.needChar.Length == tempTeamSkillCardList.Count) break;
            }
            // 해당 팀스킬에 맞는 캐릭터가 하나거나 그보다 작다면 해당캐릭터가 부족한 것이므로 다음 와일문으로 넘겨라.
            if (tempTeamSkillCardList.Count < teamSkillTable.Current.Value.needChar.Length) continue;
            //여기까지 왔으면 팀스킬이 있는 캐릭터들이 충분이 존재 한다는 것이니까 만약 기존 팀스킬의 리스트가 비어있다면 담아라.

            int tempTeamSkillCardListPower = 0;
            int teamSkillCardListPower = 0;

            for (int i = 0; i < tempTeamSkillCardList.Count; ++i)
            {
                tempTeamSkillCardListPower += (tempTeamSkillCardList[i].GetPower() / tempTeamSkillCardList.Count);
            }
            for (int i = 0; i < teamSkillCardList.Count; ++i)
            {
                teamSkillCardListPower += teamSkillCardList[i].GetPower() / teamSkillCardList.Count;
            }
            if (tempTeamSkillCardListPower > teamSkillCardListPower)
            {
                teamSkillCardList.Clear();
                for (int i = 0; i < tempTeamSkillCardList.Count; ++i)
                {
                    teamSkillCardList.Add(tempTeamSkillCardList[i]);
                }
            }
        }

        return teamSkillCardList;
    }

    internal void TeamChemistryList()
    {
        var teamChemistryTable = ((DataMapCtrl<ChemistryDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.Chemistry)).GetEnumerator();

        List<int> needID = new List<int>();
        //동종을 구분하여 가장높은 전투력을 가진 영웅들만 추린 리스트 기존에 가진 히어로들의 리스트를 제외함.
        var sameKindHeroList = UnSameKindHeroList();

        if (sameKindHeroList == null) return;

        while (teamChemistryTable.MoveNext())
        {
            for (int i = 0; i < heroList.Count; ++i)
            {
                if (teamChemistryTable.Current.Value.tgId == GameCore.Instance.PlayerDataMgr.GetUnitData(heroList[i].uid).charId)
                {
                    needID.Add(teamChemistryTable.Current.Value.needId);
                }
            }
        }
        for (int i = 0; i < needID.Count; ++i)
        {
            for (int j = 0; j < needID.Count; ++j)
            {
                if (needID[i] == needID[j]) needID.RemoveAt(j);
            }
        }
        for (int i = 0; i < needID.Count; ++i)
        {
            for (int j = 0; j < sameKindHeroList.Count; ++j)
            {
                if (needID[i] == GameCore.Instance.PlayerDataMgr.GetUnitData(sameKindHeroList[j].uid).charIdType)
                {
                    if (heroList.Count < 5)
                    {
                        if (checkSlotHero(sameKindHeroList, j))
                            heroList.Add(sameKindHeroList[j]);
                    }
                    if (heroList.Count >= 5)
                        return;
                }
            }
        }
    }

    internal bool checkSlotHero(List<HeroSData> data, int index)
    {
        for (int i = 0; i < heroList.Count; ++i)
        {
            if (GameCore.Instance.PlayerDataMgr.GetUnitData(heroList[i].uid).charId == GameCore.Instance.PlayerDataMgr.GetUnitData(data[index].uid).charId) return false;
        }
        return true;
    }
    internal List<HeroSData> SortHeroListPower(List<HeroSData> sortHeroList)
    {
        sortHeroListPower.Clear();
        var list = from card in sortHeroList
                   //where //!card.dispatch &&
                   //card.dormitory == 0
                   orderby card.GetPower() descending
                   select card;
        foreach (var test in list)
        {
            sortHeroListPower.Add(test);
        }
        return sortHeroListPower;
    }

    internal void LastSortHeroListPower()
    {
        for (int i = 0; i < heroList.Count; ++i)
        {
            for (int j = 0; j < sortHeroListPower.Count; ++j)
            {
                if (sortHeroListPower[j].key == heroList[i].key ||
                    GameCore.Instance.PlayerDataMgr.GetUnitData(heroList[i].uid).charId == GameCore.Instance.PlayerDataMgr.GetUnitData(sortHeroListPower[j].uid).charId)
                {
                    sortHeroListPower.RemoveAt(j);
                    j--;
                }
            }
        }
    }

    bool ANS_UPDATE_TEAM(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        if (code == 0)
        {
            if (waitForServer)
                ExitSysByServer();
        }
        waitForServer = false;
        return true;
    }


    internal List<HeroSData> UnSameKindHeroList()
    {
        List<HeroSData> tempUnSameKindHeroList = new List<HeroSData>();
        tempUnSameKindHeroList.Clear();

        for (int i = 0; i < sortHeroListPower.Count; ++i)
        {
            tempUnSameKindHeroList.Add(sortHeroListPower[i]);
        }

        for (int i = 0; i < tempUnSameKindHeroList.Count; ++i)
        {
            for (int j = 0; j < tempUnSameKindHeroList.Count; ++j)
            {
                if (i != j)
                {
                    if (GameCore.Instance.PlayerDataMgr.GetUnitData(tempUnSameKindHeroList[i].uid).charId == GameCore.Instance.PlayerDataMgr.GetUnitData(tempUnSameKindHeroList[j].uid).charId)
                    {
                        tempUnSameKindHeroList.RemoveAt(j);
                        --j;
                    }
                }
            }
        }

        for (int i = 0; i < tempUnSameKindHeroList.Count; ++i)
        {
            for (int j = 0; j < heroList.Count; ++j)
            {
                if (tempUnSameKindHeroList[i].uid == heroList[j].uid)
                {
                    tempUnSameKindHeroList.RemoveAt(i);
                }
            }
        }
        return tempUnSameKindHeroList;
    }

    public List<Action> GetTutorialActionList(int tutorialNum)
    {
        List<Action> nActionList = new List<Action>();
        switch (tutorialNum)
        {
            case 3:
                nActionList.Add(() => {
                    var tutoData = GameCore.Instance.PlayerDataMgr.TutorialData.Clone();
                    tutoData.main = 3;
                    GameCore.Instance.NetMgr.Req_Account_Change_Tutorial(tutoData);
                    GameCore.Instance.lobbyTutorial.TurnOnTutorial(() => { });
                });
                break;
            default:
                break;
        }
        return nActionList;
    }
}

