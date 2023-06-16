using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyDungeonUI : MonoBehaviour
{
    public static DailyDungeonUI Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Adventure/DailyDungeonUI", _parent);
        return go.GetComponent<DailyDungeonUI>();
    }

    [SerializeField] UIButton[] tabButtons;
    [SerializeField] UISprite[] tabButtonIcons;
    [SerializeField] UILabel[] tabButtonNames;
    [SerializeField] UILabel[] tabButtonDays;
    [SerializeField] UISprite[] teamButtons;

    [SerializeField] GameObject goLock;

    [Header("Team Info")]
    [SerializeField] Transform[] CardRoots;
    [SerializeField] UISprite spSkillIcon;
    [SerializeField] UILabel lbSkillName;

    [Header("Condition")]
    [SerializeField] UILabel lbConPower;
    [SerializeField] UILabel lbNowPower;

    [Header("Reward")]
    [SerializeField] UILabel lbReawrdName;
    [SerializeField] UILabel lbReawrdCount;
    [SerializeField] Transform RewardRoot;

    [Header("Friend")]
    //[SerializeField] UIGrid myFriendGrid;
    //[SerializeField] UIGrid rcmdFriendGrid;
    //[SerializeField] UITable friendListRoot;
    [SerializeField]FriendSelectComponent friendList;

    [Header("Bottom")]
    [SerializeField] UILabel lbTicketCount;
    [SerializeField] UILabel lbGuideRefill;

    [SerializeField] UILabel lbDifficult;
    [SerializeField] GameObject goDiffComboRoot;
    [SerializeField] UIButton[] btDiffItem;

    [SerializeField] GameObject btnStart;

    static int cachedTabIdx = -1;
    static int cachedDiff = 0;

    int selectedDiff;
    int selectedTabIdx = -1;
    int selectedTeamIdx = -1;

    int teamPower;
    int condPower;

    int ticketCount;

    //List<FriendSelectItem> friendItems = new List<FriendSelectItem>();
    //int selectedFriendindex = -1;

    private void Awake()
    {
        for (int i = 0; i < tabButtons.Length; ++i)
        {
            var key = GenDailyDungeonKey(i, selectedDiff);
            var data = GameCore.Instance.DataMgr.GetDailyDungeonData(key);
            //tabButtonIcons[i].spriteName = GetResourceIconString(data);
            //var reward = GameCore.Instance.DataMgr.GetDailyDungeonRewardData(data.rewardKey);
            //var item = GameCore.Instance.DataMgr.GetItemData(reward.itemKey);
            //GameCore.Instance.SetUISprite(tabButtonIcons[i], item.GetCardSpriteKey());

            tabButtonNames[i].text = data.name;
            tabButtonDays[i].text = GenDayString(data);

            var n = i;
            if (IsLock(i))
            {
                tabButtons[i].GetComponent<UISprite>().alpha = 0.5f;
                tabButtonIcons[i].GrayScale(true);
            }
            else
            {
                tabButtons[i].GetComponent<UISprite>().alpha = 0.9f;
                tabButtonIcons[i].GrayScale(false);
            }

            tabButtons[i].onClick.Add(new EventDelegate(() =>
            {
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
                //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
                OnClickTabButton(n);
            }));
        }

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

        var hour = GameCore.Instance.DataMgr.GetDailyDungeonConstData().dailyResetTicketTime;
        lbGuideRefill.text = string.Format("요일 도전 티켓은 [c][00F0FF]매일 {0} {1}시[-][/c]에 리셋됩니다.", hour < 12 ? "오전" : "오후", ((hour-1)%12)+1);
    }

    public void Init(int _teamIdx, int _tab = -1, int _diff = -1)
    {
        OnChangeDiffcult(_diff);
        OnChangeTeam(_teamIdx);
        //OnClickTabButton(((int)GameCore.nowTime.DayOfWeek/* - 1*/ + 7) % 7);
        
        UpdateTiecket();

        if (0 <= _tab)
            OnClickTabButton(_tab);
        else if (0 <= cachedTabIdx)
            OnClickTabButton(cachedTabIdx);
        else
            OpenTabByWeek(GameCore.nowTime.DayOfWeek);
    }

    public int GetSelectedTeamIndex()
    {
        return selectedTeamIdx;
    }

    public void OpenTabByWeek(DayOfWeek _week)
    {
        int day = (int)_week;

        for(int idx = 0; idx < tabButtons.Length; ++idx)
        {
            var key = GenDailyDungeonKey(idx, 0);
            var data = GameCore.Instance.DataMgr.GetDailyDungeonData(key);
            if(data == null)
            {
                OnClickTabButton(0);
                return;
            }

            for (int i = 0; i < data.playableDays.Length; ++i)
            {
                if (data.playableDays[i] == day)
                {
                    OnClickTabButton(idx);
                    //이부분이 해당하는 요일인지 체크하는 부분.
                    return;
                }
            }
        }
    }

    void OnChangeTeam(int _index)
    {
        if (selectedTeamIdx == _index)
            return;

        if(0 <= selectedTeamIdx)
            teamButtons[selectedTeamIdx].spriteName = "BTN_05_01_01";
        teamButtons[_index].spriteName = "BTN_05_01_02";
        selectedTeamIdx = _index;

        for (int i = 0; i < CardRoots.Length; ++i)
            if (CardRoots[i].childCount == 2)
                Destroy(CardRoots[i].GetChild(1).gameObject);

        int n = 0;
        int power = 0;
        for (int i = 0; i < 6; ++i)
        {
            var unit = GameCore.Instance.PlayerDataMgr.GetUnitSDataByTeam(selectedTeamIdx, i);
            if (unit != null)
            {
                power += unit.GetPower();
                CardBase.CreateBigCard(unit, CardRoots[n++], null, (k)=> GameCore.Instance.ShowCardInfo(GameCore.Instance.PlayerDataMgr.GetUnitSData(k)));
            }
        }

        var teamSkill = GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(selectedTeamIdx);
        if (0 < teamSkill)
        {
            var skillData = GameCore.Instance.DataMgr.GetTeamSkillData(teamSkill);
            GameCore.Instance.SetUISprite(spSkillIcon, skillData.imageID);
            lbSkillName.text = skillData.name;
            spSkillIcon.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            GameCore.Instance.SetUISprite(spSkillIcon, CommonType.SP_TEAMSKILL_EMPTY);
            lbSkillName.text = "스킬없음";
            spSkillIcon.transform.GetChild(0).gameObject.SetActive(false);
        }

        teamPower = power;
        lbNowPower.text = power.ToString("N0");
        UpdateCondition();
    }

    public void OnClickTabButton(int _index)
    {
        if (selectedTabIdx == _index)
            return;
        
        if (selectedTabIdx != -1)
        {
            tabButtons[selectedTabIdx].GetComponent<UISprite>().spriteName = "BTN_06_01_03";
            tabButtons[selectedTabIdx].GetComponent<UISprite>().alpha = IsLock(selectedTabIdx) ? 0.5f : 0.9f;
            tabButtons[selectedTabIdx].transform.localScale = new Vector3(1f, 1f);
            tabButtons[selectedTabIdx].transform.GetChild(1).gameObject.SetActive(false);
            tabButtons[selectedTabIdx].transform.GetChild(0).localPosition = new Vector3(0, 0, 0);
        }

        if (_index != -1)
        {
            tabButtons[_index].GetComponent<UISprite>().spriteName = "BTN_06_01_04";
            tabButtons[_index].GetComponent<UISprite>().alpha = IsLock(_index) ? 0.8f : 1f;
            tabButtons[_index].transform.localScale = new Vector3(1.1f, 1.1f);
            tabButtons[_index].transform.GetChild(1).gameObject.SetActive(true);
            tabButtons[_index].transform.GetChild(0).localPosition = new Vector3(25, 0, 0);
        }

        if(IsLock(_index))
        {
            Color grayColor = new Color(0.2f, 0.2f, 0.2f);
            btnStart.GetComponent<UISprite>().color = grayColor;
            btnStart.transform.GetChild(0).GetComponent<UILabel>().color = grayColor;
            btnStart.GetComponent<BoxCollider2D>().enabled = false;
            btnStart.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            btnStart.GetComponent<UISprite>().color = new Color(1f,1f,1f);
            btnStart.transform.GetChild(0).GetComponent<UILabel>().color = new Color(1f, 1f, 1f);
            btnStart.GetComponent<BoxCollider2D>().enabled = true;
            btnStart.transform.GetChild(1).gameObject.SetActive(true);
        }

        cachedTabIdx = selectedTabIdx = _index;
        goLock.SetActive(IsLock(_index));

        UpdateStageData();

    }

    void UpdateStageData()
    {
        if (selectedTabIdx < 0)
            return;

        var key = GenDailyDungeonKey(selectedTabIdx, selectedDiff);
        var data = GameCore.Instance.DataMgr.GetDailyDungeonData(key);

        if (data != null)
        {
            condPower = data.powerRecommend;

            var reward = GameCore.Instance.DataMgr.GetDailyDungeonRewardData(data.rewardKey);
            SetReward(reward.itemKey, reward.itemCount);
        }
        lbConPower.text = condPower.ToString("N0");
        UpdateCondition();
    }

    bool IsLock(int _index)
    {
        //int today = (int)GameCore.nowTime.DayOfWeek;

        //if (today == (int)DayOfWeek.Sunday)
        //    return false;

        //if (today == (_index%6)+1 || today == ((_index+1)%6)+1)
        //    return false;
        //else
        //    return true;

        var key = GenDailyDungeonKey(_index, selectedDiff);
        var data = GameCore.Instance.DataMgr.GetDailyDungeonData(key);
        int today = (int)GameCore.nowTime.DayOfWeek;

        for (int i = 0; i < data.playableDays.Length;++i)
            if (today == data.playableDays[i])
                return false;

        return true;
    }


    public int GetSelecteDailyDungeonKey()
    {
        return GenDailyDungeonKey(selectedTabIdx, selectedDiff);
    }
    int GenDailyDungeonKey(int _index, int _diff)
    {
        return CommonType.DEF_KEY_DAILY_DUNGEON + _diff + (_index * 3);
    }

    //요일 아이콘 이미지 지정하는 부분인데 쓰이지 않고 있습니다.
    //string GetResourceIconString(BDDungeonDataMap _data)
    //{
    //    var reward = GameCore.Instance.DataMgr.GetDailyDungeonRewardData(_data.rewardKey);
    //    switch(reward.itemKey)
    //    {
    //        case CommonType.ITEMKEY_COIN_GUARD: return "ICON_TYPE_02_01_S";
    //        case CommonType.ITEMKEY_COIN_NEAR:  return "ICON_TYPE_02_02_S";
    //        case CommonType.ITEMKEY_COIN_MAGIC: return "ICON_TYPE_02_03_S";
    //        case CommonType.ITEMKEY_COIN_SNIP:  return "ICON_TYPE_02_04_S";
    //        case CommonType.ITEMKEY_COIN_SUPPOT:return "ICON_TYPE_02_05_S";
    //        case CommonType.ITEMKEY_GOLD:       return "ICON_MONEY_02";
    //        default:                            return "";
    //    }
    //}

    string GenDayString(BDDungeonDataMap _data)
    {
        bool appended = false;
        StringBuilder sb = new StringBuilder();
        sb.Append("[FFEA00](");
        for (int i = 0; i < _data.playableDays.Length; ++i)
        {
            if (_data.playableDays[i] < 0)
                continue;

            if (appended)
                sb.Append(",");

            sb.Append(GetWeekStr(_data.playableDays[i]));
            appended = true;
        }
        sb.Append(")");

        return sb.ToString();
    }

    public static string GetWeekStr(int _week)
    {
        switch (_week)
        {
            case 1: return "월";
            case 2: return "화";
            case 3: return "수";
            case 4: return "목";
            case 5: return "금";
            case 6: return "토";
            case 0:
            default:return "일";
        }
    }


    void SetReward(int _key, int _count)
    {
        if (RewardRoot.childCount != 0)
            Destroy(RewardRoot.GetChild(0).gameObject);

        var card = CardBase.CreateBigCardByKey(_key, RewardRoot, null, (_k)=>GameCore.Instance.ShowCardInfoNotHave((int)_k));
        card.SetCount(_count);

        lbReawrdName.text = card.Data.name;
        lbReawrdCount.text = string.Format("{0:N0} + α", _count);
    }

    public void SetFriend(FriendSData[] _myFriends, FriendSData[] _rcmdFriends)
    {
        friendList.SetFriend(_myFriends, _rcmdFriends);
    }

    public void UpdateCondition()
    {
        lbNowPower.color = (condPower <= teamPower) ? CommonType.COLOR_05 : CommonType.COLOR_04;
    }

    public void UpdateTiecket()
    {
        var count = GameCore.Instance.PlayerDataMgr.GetReousrceCount( ResourceType.Ticket_daily);
        ticketCount = count;
        lbTicketCount.text =  string.Format("x{0}", count);
    }

    public void OnClickDifficultButton()
    {
        goDiffComboRoot.SetActive(!goDiffComboRoot.activeSelf);
    }

    public void OnClickOffDifficult()
    {
        goDiffComboRoot.SetActive(false);
    }

    internal void OnChangeDiffcult(int _diff) // 0 ~ 2
    {
        if (_diff < 0)
            _diff = cachedDiff;
        lbDifficult.text = StoryDataMap.GetStrDiffcult(_diff+1);
        btDiffItem[selectedDiff].GetComponentInChildren<UILabel>().color = Color.white;
        btDiffItem[_diff].GetComponentInChildren<UILabel>().color = CommonType.COLOR_02;
        cachedDiff = selectedDiff = _diff;

        OnClickOffDifficult();

        UpdateStageData();
    }

    public void OnClickEditTeam()
    {
        GameCore.Instance.ChangeSubSystem(SubSysType.EditTeam, new ReturnSysPara(SubSysType.DailyPrepare, selectedTeamIdx, new StoryPara(selectedTabIdx + (selectedDiff << 8), false, selectedTeamIdx)));
    }
    public void OnClickStart()
    {
        if(ticketCount <= 0)
        {
            var count = GameCore.Instance.PlayerDataMgr.LocalUserData.DailyDungeonRechargeAbleCount;
            if (count == 0)
            {
                GameCore.Instance.ShowNotice("알람", "오늘의 도전 기회를 모두 소진 하였습니다.", 0);
            }
            else
            {
                OnClickTicket();
            }
            return;
        }

        if (teamPower < condPower)
        {
            GameCore.Instance.ShowNotice("알람", "전투력이 부족하여 입장할 수 없습니다.", 0);
            return;
        }

        var key = GetSelecteDailyDungeonKey();
        if (friendList.SelectFriendItem == null)
            GameCore.Instance.NetMgr.Req_Dungeon_Daily_Start(key, selectedTeamIdx, -1);
        else
            GameCore.Instance.NetMgr.Req_Dungeon_Daily_Start(key, selectedTeamIdx, friendList.SelectFriendItem.GetUID());
        //GameCore.Instance.ChangeSubSystem(SubSysType.DailyBattle, new BattlePara() { type = InGameType.Daily, playerTeam = selectedTeamIdx, stageId = 7000001 });
        //GameCore.Instance.ShowNotice("요일 던전 시작", "미구현", 0);
    }
    public void OnClickTicket()
    {
        
        if (GameCore.Instance.PlayerDataMgr.LocalUserData.DailyDungeonRechargeAbleCount <= 0)
        {
            GameCore.Instance.ShowNotice("도전 횟수 추가 구매", "오늘은 더 이상 요일던전 티켓 구매가 불가능합니다.", 0);
        }
        else
        {
            var count = GameCore.Instance.PlayerDataMgr.LocalUserData.DailyDungeonRechargeAbleCount;
            var consts = GameCore.Instance.DataMgr.GetDailyDungeonConstData();
            var cost = consts.dailyDefPurchaseCost + (consts.dailyAddPurchaseCost * (consts.dailyPurchaseCount - count));
            GameCore.Instance.ShowAgree("도전 횟수 추가 구매", 
                string.Format("다음 재화를 사용하여 도전 티켓을\n구매하시겠습니까?(남은횟수 : [C][7F00FF]{0}[-][/C]회)\n[C][FF0000]구매한 도전 횟수는 구매 당일에만 사용 가능합니다.[-][/C]", count),
                string.Format("{0:N0}", cost), MoneyType.Pearl, 0, () => {
                 GameCore.Instance.CloseMsgWindow();
                 GameCore.Instance.NetMgr.Req_Dungeon_Daily_Buy_Ticket();
             });
        }
    }


}
