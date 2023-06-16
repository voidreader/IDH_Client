using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionUI : MonoBehaviour
{
    public enum State
    {
        Lock,
        Running,
        Takable,
        Complete,
    }

    //public class MissionData
    //{
    //    public State state;
    //    public MissionListRoot.Type type;
    //    public int id;
    //    public int missionKey;
    //    //public int level;
    //    //public string name;
    //    //public string desc;
    //    //internal ItemSData reward;
    //    public int nowCount;
    //    //public int tgCount;

    //    public SubSysType forwardLocation; // 이동시 갈 곳
    //}


    public static MissionUI Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Lobby/PanelMissionWindow", _parent);
        return go.GetComponent<MissionUI>();
    }

    [SerializeField] UIButton[] tabButtons;
    [SerializeField] UIScrollView scView;
    [SerializeField] MissionListRoot[] listRoot;
    MissionListRoot questListRoot; // listRoot[3]과 같아야 한다.


    [SerializeField] UILabel lbHead;
    [SerializeField] UILabel lbComplete;
    [SerializeField] UILabel lbCompleteValue;

    [SerializeField] GameObject goChangeButtonRoot;

    [SerializeField] Transform cardRoot;
    [SerializeField] GameObject goObtain;
    [SerializeField] GameObject goTakableEffect;
    [SerializeField] UILabel lbState;
    [SerializeField] UIButton btTake;
    [SerializeField] UIButton btTakeAll;

    [SerializeField] UILabel lbGuide;


    int questIdx = 0;
    int tabIdx = 0;

    CardBase card;

    public int TabIndex { get { return tabIdx; } }
    public int QuestIndex { get { return questIdx; } }

    private void Awake()
    {
        questListRoot = listRoot[3];

        for (int i = 0; i < tabButtons.Length; ++i)
        {
            var n = i;
            tabButtons[i].onClick.Add(new EventDelegate(() =>
            {
                SwitchingTab(n);
            }));
        }
    }

    public void Init()
    {
        // Create Items
        for (int i = 0; i < listRoot.Length-2; ++i)
            listRoot[i].Init(GameCore.Instance.MissionMgr.GetBundle((MissionType)i), () => { UpdateCount(); SetReward(-1, tabIdx); });

        listRoot[2].Init(GameCore.Instance.MissionMgr.GetBundle(MissionType.Achieve), () => 
        {
            UpdateCount(); SetReward(listRoot[2].Data.topData.UID, tabIdx);
        });

        //Create Quest Items
        questIdx = GameCore.Instance.MissionMgr.GetNowQuestPage();
        CreateQuestItems(questIdx);

        SwitchingTab(0);


        TackNotification();
    }

    public void Show(bool _show)
    {
        gameObject.SetActive(_show);
        if (_show)
            AutonomyTutorial.AutoRunTutorial(AutonomyTutoType.Mission, 0, 1);
    }

    void CreateQuestItems(int _questIdx)
    {
        questListRoot.Init(GameCore.Instance.MissionMgr.GetBundle(MissionType.Quest, _questIdx), () => {
            UpdateCount();
            SetReward(-1, tabIdx);
        });
    }

    private void SwitchingTab(int _n)
    {
        tabButtons[tabIdx].GetComponent<UISprite>().spriteName = "BTN_06_01_01";
        tabButtons[tabIdx].transform.localScale = new Vector3(1f, 1f);
        tabButtons[tabIdx].transform.GetChild(0).gameObject.SetActive(false);
        listRoot[tabIdx].gameObject.SetActive(false);

        tabButtons[_n].GetComponent<UISprite>().spriteName = "BTN_06_01_02";
        tabButtons[_n].transform.localScale = new Vector3(1.1f, 1.1f);
        tabButtons[_n].transform.GetChild(0).gameObject.SetActive(true);
        listRoot[_n].gameObject.SetActive(true);

        tabIdx = _n;

        lbHead.text = tabButtons[tabIdx].GetComponentInChildren<UILabel>().text;
        lbComplete.text = listRoot[tabIdx].GetMainText();
        lbGuide.text = listRoot[tabIdx].GetGuideText();

        UpdateCount();
        goChangeButtonRoot.SetActive(_n == 3);
        if (tabIdx == 3)
        {
            SetReward(GameCore.Instance.MissionMgr.GetBundle(MissionType.Quest, questIdx).topData.UID, tabIdx);
            AutonomyTutorial.AutoRunTutorial(AutonomyTutoType.Mission, 1, 1);
        }
        else
            SetReward(GameCore.Instance.MissionMgr.GetBundle((MissionType)tabIdx).topData.UID, tabIdx);

        scView.transform.localPosition = Vector3.zero;
        var sp = scView.GetComponent<SpringPanel>();
        if (sp != null)
            sp.enabled = false;
    }

    public void UpdateCount()
    {
        lbCompleteValue.text = listRoot[tabIdx].GetCompletCounteText();
    }

    public void SetReward(int _accumRewardKey, int _selectedIndex)
    {
        if (0 < _accumRewardKey)
        {
            var reward = GameCore.Instance.DataMgr.GetMissionAccumRewardData(_accumRewardKey);
            if (reward != null)
            {
                if (card != null)
                    Destroy(card.gameObject);
                var item = GameCore.Instance.DataMgr.GetItemData(reward.rewardKey);
                card = CardBase.CreateSmallCard(item, cardRoot);
                card.SetCount(reward.rewardValue);
            }
        }

        var data = listRoot[_selectedIndex];
        if (!data.IsComplete())
        {
            lbState.text = "미완료";
            btTake.gameObject.SetActive(false);
            card.SetEnable(false);
            goObtain.SetActive(false);
            goTakableEffect.SetActive(false);
        }
        else
        {
            if (data.IsTakable())
            {
                lbState.text = "받기";
                btTake.gameObject.SetActive(true);
                card.SetEnable(true);
                goObtain.SetActive(false);
                goTakableEffect.SetActive(true);
            }
            else
            {
                lbState.text = "완료";
                btTake.gameObject.SetActive(false);
                card.SetEnable(false);
                goObtain.SetActive(true);
                goTakableEffect.SetActive(false);
            }
        }
    }

    internal void SetMissionData(MissionSData _sdata)
    {
        listRoot[(int)_sdata.type].UpdateData(_sdata);
    }

    internal void TackNotification()
    {
        for(int i  =0; i < tabButtons.Length; ++i)
        {
            UnityCommonFunc.GetGameObjectByName(tabButtons[i].gameObject, "highlight").gameObject.SetActive(false);
        }

        var pageCount = GameCore.Instance.MissionMgr.GetQuestPageCount();
        for (int p = 0; p < pageCount; p++)
        {
            var qBundle = GameCore.Instance.MissionMgr.GetBundle(MissionType.Quest, p);
            if (qBundle.topData.state == MissionState.Takable)
            {
                UnityCommonFunc.GetGameObjectByName(tabButtons[3].gameObject, "highlight").gameObject.SetActive(true);
                break;
            }

            for (int i = 0; i < qBundle.datas.Count; ++i)
                if (qBundle.datas[i].state == MissionState.Takable)
                {
                    UnityCommonFunc.GetGameObjectByName(tabButtons[3].gameObject, "highlight").gameObject.SetActive(true);
                    break;
                }
        }

        for (int b = 0; b < 3; ++b)
        {
            var bundle = GameCore.Instance.MissionMgr.GetBundle((MissionType)b);
            if (bundle.topData.state == MissionState.Takable)
            {
                UnityCommonFunc.GetGameObjectByName(tabButtons[b].gameObject, "highlight").gameObject.SetActive(true);
            }

            for (int i = 0; i < bundle.datas.Count; ++i)
                if (bundle.datas[i].state == MissionState.Takable)
                {
                    UnityCommonFunc.GetGameObjectByName(tabButtons[b].gameObject, "highlight").gameObject.SetActive(true);
                    break;
                }
        }
      
    }

    int GetTakableCount(MissionType _type, bool _includeTop = true)
    {
        int cnt = 0;
        if (_type == MissionType.Quest)
        {
            var pageCount = GameCore.Instance.MissionMgr.GetQuestPageCount();
            for (int p = 0; p < pageCount; p++)
            {
                var bundle = GameCore.Instance.MissionMgr.GetBundle(_type, p);
                if (_includeTop && bundle.topData.state == MissionState.Takable)
                    cnt++;

                for (int i = 0; i < bundle.datas.Count; ++i)
                    if (bundle.datas[i].state == MissionState.Takable)
                        cnt++;
            }
        }
        else
        {
            var bundle = GameCore.Instance.MissionMgr.GetBundle(_type);
            if (_includeTop && bundle.topData.state == MissionState.Takable)
                cnt++;

            for (int i = 0; i < bundle.datas.Count; ++i)
                if (bundle.datas[i].state == MissionState.Takable)
                    cnt++;
        }
        return cnt;
    }

    public void OnClickTake()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        GameCore.Instance.NetMgr.Req_Mission_Reward_Top(listRoot[tabIdx].Data.type, listRoot[tabIdx].Data.topData.UID);
    }

    public void OnClickTakeAll()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        if (0 < GetTakableCount((MissionType)tabIdx, false))
        {
            GameCore.Instance.NetMgr.Req_Mission_Reward(listRoot[tabIdx].Data.type, 0);
        }
        else if (0 < GetTakableCount((MissionType)tabIdx, true))
        {
            GameCore.Instance.NetMgr.Req_Mission_Reward_Top(listRoot[tabIdx].Data.type, listRoot[tabIdx].Data.topData.UID);
        }
        else
        {
            GameCore.Instance.ShowNotice("실패", "받을 수 있는 보상이 없습니다.", 0);
        }
    }

    public void OnClickClose()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        Show(false);
        GameCore.Instance.MissionMgr.TakeNotificationCheck();
        GameCore.Instance.NetMgr.Req_Notify_MyRoom_Count();
    }

    public void OnClickChangePrev()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        questIdx = (GameCore.Instance.MissionMgr.GetQuestPageCount() + questIdx - 1) % GameCore.Instance.MissionMgr.GetQuestPageCount();
        CreateQuestItems(questIdx);
        SwitchingTab(tabIdx);
    }

    public void OnLCickChangeNext()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        questIdx = (questIdx + 1) % GameCore.Instance.MissionMgr.GetQuestPageCount();
        CreateQuestItems(questIdx);
        SwitchingTab(tabIdx);
    }

    public bool MissionActive()
    {
        return gameObject.activeSelf;
    }
}
