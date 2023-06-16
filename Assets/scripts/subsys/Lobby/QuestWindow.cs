using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestWindow : MissionListRoot
{
    [SerializeField] Transform[] iconRoot;
    [SerializeField] UILabel[] lbLevel;
    [SerializeField] GameObject[] goObtain;
    [SerializeField] GameObject[] effect;

    [Space]
    [SerializeField] MissionListItemScript mission;

    [Space]
    [SerializeField] Transform selectHighlight;

    CardBase[] cards = new CardBase[10];

    Action cbClickTaken;

    int selectedIdx = -1;


    public override void Init(MissionBundle _data, Action _cbClickTaken)
    {
        data = _data;
        cbClickTaken = _cbClickTaken;

        // 기존 데이터 삭제후 재생성
        for (int i = 0; i < cards.Length; ++i)
        {
            if (cards[i] != null)
                Destroy(cards[i].gameObject);

            cards[i] = null;
        }

        // 생성
        int selectIdx = 0;
        int idx = 0;
        for(; idx < _data.datas.Count; ++idx)
        {
            var mData = _data.datas[idx];
            int n = idx;
            var missionData = GameCore.Instance.DataMgr.GetMissionQuestData(mData.UID);
            cards[idx] = CardBase.CreateSmallCard(GameCore.Instance.DataMgr.GetItemData(missionData.rewardKey) , iconRoot[idx], (id) => CBClickIcon(n));
            cards[idx].SetCount(missionData.rewardValue);
            SetIconState(idx, mData);
            var topData = GameCore.Instance.DataMgr.GetMissionAccumRewardData(_data.topData.UID);
            lbLevel[idx].text = string.Format("{0} - {1}", topData.level, idx+1);

            if (mData.state == MissionState.Running ||
                mData.state == MissionState.Takable)
                selectIdx = idx;
        }

        // 10개가 안될경우 나머지를 모두 비움
        for (; idx < iconRoot.Length; ++idx)
        {
            goObtain[idx].SetActive(false);
            effect[idx].SetActive(false);
            lbLevel[idx].text = "";
        }

        CBClickIcon(selectIdx);
    }

    private void SetIconState(int _idx, MissionSData _data)
    {
        effect[_idx].SetActive(_data.state == MissionState.Takable);
        goObtain[_idx].SetActive(_data.state == MissionState.Complete);
        cards[_idx].SetEnable(_data.state == MissionState.Running || _data.state == MissionState.Takable);

        if (_data.state == MissionState.Takable)
            cards[_idx].SetClickCallback(mission.OnClickTake);
        else
            cards[_idx].SetClickCallback(() => CBClickIcon(_idx));
    }

    internal override void UpdateData(MissionSData _sdata)
    {
        for (int i = 0; i < data.datas.Count; ++i)
        {
            if (data.datas[i].UID == _sdata.UID)
            {
                data.datas[i] = _sdata;
                SetIconState(i, _sdata);
                CBClickIcon(i);
                break;
            }
            else
            {
                data.datas[i].state = MissionState.Complete;
                SetIconState(i, data.datas[i]);
            }
        }

        mission.SetState(mission.GetState());
    }

    // 맵에서 아이콘이 클릭되었을때
    void CBClickIcon(int _idx)
    {
        selectHighlight.position = iconRoot[_idx].position;
        selectHighlight.localPosition += new Vector3(32, -32, 0);
        selectHighlight.GetComponent<UITweener>().ResetToBeginning();
        selectedIdx = _idx;
        mission.Init(ListType, _idx, data.datas[_idx], CBClickTaken);
    }

    void CBClickTaken()
    {
        data.datas[selectedIdx].state = MissionState.Complete;
        SetIconState(selectedIdx, data.datas[selectedIdx]);
        if (cbClickTaken != null) cbClickTaken();
    }
}
