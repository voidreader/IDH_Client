using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaidRankComponent : MonoBehaviour
{
    [SerializeField] UIButton[] tabButtons; // 0:myRank, 1:TOP50, 2:Reward
    [SerializeField] UIGrid listRank;
    [SerializeField] UIGrid listReward;
    int tabIdx = 0;

    [SerializeField] UILabel lbDifficult;
    [SerializeField] GameObject goDiffComboRoot;
    [SerializeField] UIButton[] btDiffItem;
    int selectedDiff = 0;

    [SerializeField] Transform[] rewardRoots;
    [SerializeField] UILabel[] lbRewardTexts;
    CardBase[] rewardItems;

    List<RaidRankSData>[,] datas = new List<RaidRankSData>[3,2]; // [Diff, tab]
    List<RaidRankListItemScript> items = new List<RaidRankListItemScript>();

    bool bWaitResponse;

    GameObject goScrollPanel;

    private void Awake()
    {
        for (int i = 0; i < tabButtons.Length; ++i)
        {
            var n = i;
            tabButtons[i].GetComponent<UIButton>().onClick.Add(new EventDelegate(() =>
            {
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
                //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
                SwitchingTab(n);
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
        btDiffItem[0].GetComponentInChildren<UILabel>().color = CommonType.COLOR_02;

        rewardItems = new CardBase[rewardRoots.Length];
    }

    private void OnEnable()
    {
        SwitchingTab(tabIdx);
    }

    public void Init()
    {
        var uid = GameCore.Instance.PlayerDataMgr.PvPData.userUID;
        //var rankDatas = new RaidRankListItemScript.Data[]{
        //    new RaidRankListItemScript.Data(1,1,99,"남바완", 1000001, 12345, 9943, 25154 ),
        //    new RaidRankListItemScript.Data(2,2,56,"남바투", 1000033, 12345, 9943, 25154 ),
        //    new RaidRankListItemScript.Data(uid,3,49,"사실 주인공인거임!", 1000066, 99991, 9943, 25154 ),
        //    new RaidRankListItemScript.Data(3,4,30,"남바뽀", 1000022, 12235, 9943, 25154 ),
        //    new RaidRankListItemScript.Data(4,5,16,"남바빠", 1000055, 1245, 9943, 25154 ),
        //    new RaidRankListItemScript.Data(5,6,32,"남바씪", 1000066, 1345, 9943, 25154 ),
        //};
        //datas[0, 0] = new List<RaidRankListItemScript.Data>(rankDatas);
        //datas[0, 1] = new List<RaidRankListItemScript.Data>(rankDatas);
        //datas[1, 0] = new List<RaidRankListItemScript.Data>(rankDatas);
        //datas[1, 1] = new List<RaidRankListItemScript.Data>(rankDatas);
        //datas[2, 0] = new List<RaidRankListItemScript.Data>(rankDatas);
        //datas[2, 1] = new List<RaidRankListItemScript.Data>(rankDatas);

        //OnChangeDiffcult(0);
        //SwitchingTab(0);
    }


    private void SwitchingTab(int _n) // 0 ~ 2
    {
        tabButtons[tabIdx].GetComponent<UISprite>().spriteName = CommonType.BTN_5_NORMAL;
        //listRoot[tabIdx].gameObject.SetActive(false);

        tabIdx = _n;

        tabButtons[tabIdx].GetComponent<UISprite>().spriteName = CommonType.BTN_5_ACTIVE;
        //listRoot[tabIdx].gameObject.SetActive(true);


        if (tabIdx != 2 && datas[selectedDiff, tabIdx] == null)
        {
            if (tabIdx == 0)
                GameCore.Instance.NetMgr.Req_Raid_MyRank(GameCore.Instance.PlayerDataMgr.GetRaidSDataByDifficult(selectedDiff).key);
            else 
                GameCore.Instance.NetMgr.Req_Raid_Rank50(GameCore.Instance.PlayerDataMgr.GetRaidSDataByDifficult(selectedDiff).key);

            bWaitResponse = true;
        }
        else
        {
            ShowPage(selectedDiff, tabIdx);
        }
    }

    void ShowPage(int _diff, int _idx)
    {
        if (_idx < 2)
        {
            int idx = 0;
            for (; idx < datas[_diff, _idx].Count; ++idx)
            {
                RaidRankListItemScript item;
                if (idx < items.Count)
                {
                    item = items[idx];
                    item.gameObject.SetActive(true);
                }
                else
                {
                    item = RaidRankListItemScript.Create(listRank.transform);
                    items.Add(item);
                }

                item.Init(datas[_diff, _idx][idx]);
            }
            for (; idx < items.Count; ++idx)
                items[idx].gameObject.SetActive(false);

            listRank.enabled = true;
            listRank.gameObject.SetActive(true);
            listReward.gameObject.SetActive(false);
        }
        else
        {
            listRank.gameObject.SetActive(false);
            listReward.gameObject.SetActive(true);

            for (int i = 0; i < rewardRoots.Length; ++i)
            {
                var data = GameCore.Instance.DataMgr.GetRaidRankRewardData(CommonType.DEFTABLEKEY_RAIDRANKREWARD + 1 + i + (8 * selectedDiff));

                if(rewardItems[i] != null)
                {
                    Destroy(rewardItems[i].gameObject);
                    rewardItems[i] = null;
                }
                rewardItems[i] = CardBase.CreateBigCardByKey(data.item, rewardRoots[i]);
                rewardItems[i].SetCount(data.itemCount);

                lbRewardTexts[i].text = data.text;
            }
        }

        if (goScrollPanel == null)
            goScrollPanel = GetComponentInChildren<UIScrollView>().gameObject;
        SpringPanel.Begin(goScrollPanel, Vector3.zero, 8);
    }

    public void SetData(List<RaidRankSData> _sdata)
    {
        if (tabIdx < 2)
        {
            var key = GameCore.Instance.PlayerDataMgr.GetRaidSDataByDifficult(selectedDiff).key;
            var data = GameCore.Instance.DataMgr.GetRaidData(key);
            var diff = data.difficult - 1;

            datas[diff, tabIdx] = new List<RaidRankSData>();
            for (int i = 0; i < _sdata.Count; i++)
                datas[diff, tabIdx].Add(_sdata[i]);
        }
        else
        {
            // Todo
        }

        ShowPage(selectedDiff, tabIdx);
        bWaitResponse = false;
    }


    void OnChangeDiffcult(int _diff) // 0 ~ 2
    {
        if (bWaitResponse)
            return;

        lbDifficult.text = StoryDataMap.GetStrDiffcult(_diff + 1);

        btDiffItem[selectedDiff].GetComponentInChildren<UILabel>().color = Color.white;
        btDiffItem[_diff].GetComponentInChildren<UILabel>().color = CommonType.COLOR_02;
        selectedDiff = _diff;

        OnClickOffDifficult();
        SwitchingTab(tabIdx);
    }

    public void OnClickDifficultButton()
    {
        if (bWaitResponse)
            return;

        goDiffComboRoot.SetActive(!goDiffComboRoot.activeSelf);
    }

    public void OnClickOffDifficult()
    {
        goDiffComboRoot.SetActive(false);
    }
}
