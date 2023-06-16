using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaidPrepareUI : MonoBehaviour
{
    public static RaidPrepareUI Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Adventure/RaidPrepareUI", _parent);
        return go.GetComponent<RaidPrepareUI>();
    }

    [SerializeField] UISprite[] teamButtons;

    [SerializeField] RaidRewardGradeComponent rewardRoot;

    [Header("Raid Info")]
    [SerializeField] UILabel lbTitle;
    [SerializeField] UILabel lbMonsterBuff;
    [SerializeField] UILabel lbMonsterPower;
    [SerializeField] UILabel lbTime;
    [SerializeField] UISlider slider;
    [SerializeField] UISprite spSliderTumb;
    [SerializeField] UISprite spSliderForGround;

    [SerializeField] UI2DSprite spMonster;
    [SerializeField] UISprite spClear;

    [Header("Team Info")]
    [SerializeField] Transform[] CardRoots;
    [SerializeField] UISprite spSkillIcon;
    [SerializeField] UILabel lbSkillName;
    [SerializeField] UILabel lbTeamPower;

    [Header("Friend List")]
    [SerializeField] FriendSelectComponent friendList;

    [Header("Bottom")]
    [SerializeField] UILabel lbTicketCount;

    [SerializeField] UILabel lbDifficult;
    [SerializeField] GameObject goDiffComboRoot;
    [SerializeField] UIButton[] btDiffItem;

    [SerializeField] RaidRankComponent rankComponent;

    int selectedTeamIdx = 0;
    int ticketCount = -1;
    int selectedDiff = 0;

    bool bClear;
    bool bRunningTime;

    int raidKey;

    public int SelectedTeamIdx { get { return selectedTeamIdx; } }
    public int SelectedFriendindex { get { return friendList.SelectedFriendindex; } }
    public int SelectedDiff { get { return selectedDiff; } }

    private void Awake()
    {
        for (int i = 0; i < teamButtons.Length; ++i)
        {
            var n = i;
            teamButtons[i].GetComponent<UIButton>().onClick.Add(new EventDelegate(() =>
            {
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
                //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
                OnChangeTeam(n);
            }));
        }

        for (int i = 0; i < btDiffItem.Length; ++i)
        {
            var n = i;
            btDiffItem[i].GetComponent<UIButton>().onClick.Add(new EventDelegate(() =>
            {
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
                //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
                OnChangeDiffcult(n);
            }));
        }
    } 

    public void Init()
    {
        StartTimer();
        UpdateTiecket();
        OnChangeDiffcult(selectedDiff);
        OnChangeTeam(selectedTeamIdx);

        rankComponent.Init();
    }

    void StartTimer()
    {
        var time = new System.DateTime(GameCore.nowTime.Year, GameCore.nowTime.Month, GameCore.nowTime.Day, 2, 0, 0) - GameCore.nowTime;
        if (GameCore.nowTime.Hour >= 2)
            time.Add(new TimeSpan(1, 0, 0, 0));

        StartCoroutine(CoTimer());
    }

    IEnumerator CoTimer()
    {
        bRunningTime = true;

        var consts = GameCore.Instance.DataMgr.GetPvEConstData();
        var oneSec = new TimeSpan(0, 0, 1);

        bool nextDay = consts.raidStartTime < consts.raidEndTime;
        int sTime = consts.raidStartTime;
        int eTime = consts.raidEndTime + (nextDay && GameCore.nowTime.Hour < consts.raidEndTime ? 0 : 24);

        var total = new TimeSpan(eTime - sTime, 0, 0);
        var time = new TimeSpan(eTime, 0, 0) - new TimeSpan(GameCore.nowTime.Hour, GameCore.nowTime.Minute, GameCore.nowTime.Second);
        
        while (time.TotalSeconds > 0)
        {
            lbTime.text = string.Format("{0:00}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);
            slider.Set((float)(time.TotalSeconds / total.TotalSeconds));
            yield return new WaitForSecondsRealtime(1f);
            time = time - oneSec;
        }

        lbTime.text = string.Format("{0:00}:{1:00}:{2:00}", 0, 0, 0 );
        slider.Set(0f);
        bRunningTime = false;
        UpdateRaidInfo();
    }

    void UpdateRaidInfo()
    {
        var sdata = GameCore.Instance.PlayerDataMgr.GetRaidSDataByDifficult(selectedDiff);
        if (sdata == null)
            return;

        raidKey = sdata.key;
        var data = GameCore.Instance.DataMgr.GetRaidData(sdata.key);
        var unit = GameCore.Instance.DataMgr.GetUnitData(data.bossKey);
        bClear = sdata.damage >= data.accumDmgs[3];

        rewardRoot.Init(sdata.key);

        GameCore.Instance.SetUISprite(spMonster, 11001 + selectedDiff + ((data.group)*3));
        lbTitle.text = string.Format("[i]{0}", data.stageName);
        //WeekBuff가 날라가면서 주석처리된곳
        //lbMonsterBuff.text = GetBuffString(unit);// string.Format("원거리 / 마법형 공격력 50% 경감");
        lbMonsterPower.text = string.Format("필요 전투력 {0:N0} 이상", data.powerRecommand);

        var power = GameCore.Instance.PlayerDataMgr.GetTeamPower(selectedTeamIdx);
        if (data.powerRecommand <= power)   lbTeamPower.color = CommonType.COLOR_05;
        else                                lbTeamPower.color = CommonType.COLOR_04;

        spClear.gameObject.SetActive(bClear);
        spMonster.GrayScale(bClear || !bRunningTime);
        spSliderForGround.GrayScale(bClear || !bRunningTime);
        spSliderTumb.GrayScale(bClear || !bRunningTime);
    }

    string GetBuffString(UnitDataMap _unit)
    {
        //bool bFirst = true;
        //StringBuilder sb = new StringBuilder();

        //for (int i = 0; i < _unit.weekType.Length; ++i)
        //{
        //    if (_unit.weekType[i] < 0)
        //        continue;

        //    if (!bFirst) sb.Append(" / ");
        //    sb.Append(CardDataMap.GetStrType(_unit.weekType[i]));

        //    bFirst = false;
        //}

        //sb.Append(" 공격 ");
        //sb.Append((int)(_unit.weekType[0] * 100));
        //sb.Append("% 경감");

        //return sb.ToString();
        return null;
    }

    public void SetFriend(FriendSData[] _myFriends, FriendSData[] _rcmdFriends)
    {
        friendList.SetFriend(_myFriends, _rcmdFriends);
    }

    public void SetRankItemData(List<RaidRankSData> _sdata)
    {
        rankComponent.SetData(_sdata);
    }

    internal void OnChangeTeam(int _index)
    {
        //if (selectedTeamIdx == _index)
        //    return;

        if(0 <= selectedTeamIdx)
            teamButtons[selectedTeamIdx].spriteName = "BTN_05_01_01";
        teamButtons[_index].spriteName = "BTN_05_01_02";
        selectedTeamIdx = _index;


        for (int i = 0; i < CardRoots.Length; ++i)
            if (CardRoots[i].childCount == 2)
                Destroy(CardRoots[i].GetChild(1).gameObject);

        int n = 0;
        for (int i = 0; i < 6; ++i)
        {
            var unit = GameCore.Instance.PlayerDataMgr.GetUnitSDataByTeam(selectedTeamIdx, i);
            if (unit != null)
            {
                CardBase.CreateBigCard(unit, CardRoots[n++], null, (k) => GameCore.Instance.ShowCardInfo(GameCore.Instance.PlayerDataMgr.GetUnitSData(k)));
            }
        }

        var teamSkill = GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(selectedTeamIdx);
        if (0 < teamSkill)
        {
            var skillData = GameCore.Instance.DataMgr.GetTeamSkillData(teamSkill);
            GameCore.Instance.SetUISprite(spSkillIcon, skillData.imageID);
            lbSkillName.text = skillData.name;
        }
        else
        {
            GameCore.Instance.SetUISprite(spSkillIcon, CommonType.SP_TEAMSKILL_EMPTY);
            lbSkillName.text = "스킬없음";
        }


        var data = GameCore.Instance.DataMgr.GetRaidData(GameCore.Instance.PlayerDataMgr.GetRaidSDataByDifficult(selectedDiff).key);
        var power = GameCore.Instance.PlayerDataMgr.GetTeamPower(selectedTeamIdx);
        if (data.powerRecommand <= power)   lbTeamPower.color = CommonType.COLOR_05;
        else                                lbTeamPower.color = CommonType.COLOR_04;
        lbTeamPower.text = power.ToString("N0");
    }

    internal void OnChangeDiffcult(int _diff) // 0 ~ 2
    {
        lbDifficult.text = StoryDataMap.GetStrDiffcult(_diff + 1);

        btDiffItem[selectedDiff].GetComponentInChildren<UILabel>().color = Color.white;
        btDiffItem[_diff].GetComponentInChildren<UILabel>().color = CommonType.COLOR_02;
        selectedDiff = _diff;

        UpdateRaidInfo();

        OnClickOffDifficult();
    }

    void UpdateTiecket()
    {
        var count = GameCore.Instance.PlayerDataMgr.GetReousrceCount( ResourceType.Ticket_Raid);
        ticketCount = count;
        lbTicketCount.text = string.Format("x{0}", count);
    }

    public void OnClickEditTeam()
    {
        GameCore.Instance.ChangeSubSystem(SubSysType.EditTeam, new ReturnSysPara(SubSysType.RaidPrepare, selectedTeamIdx, new StoryPara(selectedDiff, false, selectedTeamIdx)));
    }

    public void OnClickDifficultButton()
    {
        goDiffComboRoot.SetActive(!goDiffComboRoot.activeSelf);
    }


    public void OnClickOffDifficult()
    {
        goDiffComboRoot.SetActive(false);
    }

    public void OnClickRanking()
    {
        rankComponent.gameObject.SetActive(!rankComponent.gameObject.activeSelf);
    }

    public void OnClickStart()
    {
        if(bClear)
        {
            GameCore.Instance.ShowNotice("실패", "이미 클리어 하였습니다.", 0);
            return;
        }

        if (!bRunningTime)
        {
            GameCore.Instance.ShowNotice("실패", "레이드 시작이 아닙니다.", 0);
            return;
        }

        if (GameCore.Instance.PlayerDataMgr.GetReousrceCount(ResourceType.Ticket_Raid) <= 0)
        {
            GameCore.Instance.ShowNotice("실패", "티켓이 부족합니다.", 0);
            return;
        }

        if (GameCore.Instance.DataMgr.GetRaidData(raidKey).powerRecommand >
            GameCore.Instance.PlayerDataMgr.GetTeamPower(selectedTeamIdx))
        {
            GameCore.Instance.ShowNotice("실패", "전투력이 부족합니다.", 0);
            return;
        }


        var sdata = GameCore.Instance.PlayerDataMgr.GetRaidSDataByDifficult(selectedDiff);
        //GameCore.Instance.ChangeSubSystem(SubSysType.RaidBattle, new BattlePara() { playerTeam = selectedTeamIdx, type = InGameType.Raid, stageId = sdata.key });

        if (friendList.SelectFriendItem == null)
            GameCore.Instance.NetMgr.Req_Raid_Start(sdata.key, selectedTeamIdx, -1);
        else
            GameCore.Instance.NetMgr.Req_Raid_Start(sdata.key, selectedTeamIdx, friendList.SelectFriendItem.GetUID());
    }

}
