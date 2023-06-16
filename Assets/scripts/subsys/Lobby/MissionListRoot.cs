using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionListRoot : MonoBehaviour
{
    static readonly string[] strMain = new string[] {
        "일일 미션 달성도",
        "주간 미션 달성도",
        "업적 달성도",
        "퀘스트 달성도",
    };

    bool bStrGuideInit; // strGuide가 업데이트 되었는지 여부. 최초 1회만 동작하기위함
    static string[] strGuide = new string[] {
        "일일 미션은 [c][F600FF]매일 {0} {1}시[-][/c]에 갱신 됩니다.",
        "주간 미션은 [c][F600FF]{2}요일 {0} {1}시[-][/c]에 갱신 됩니다.",
        "업적은 [c][F600FF]여러 단계[-][/c]로 구성 됩니다.",
        "퀘스트는 [c][F600FF]현재 퀘스트를 클리어 해야[-][/c] 다음퀘스트가 열립니다.",
    };

    [SerializeField] MissionType type;
    [SerializeField] UIGrid grid;

    public MissionType ListType { get { return type; } }

    int lvlIdx; // 퀘스트에서는 현재 출력되는 퀘스트 페이지 ( 1 ~ )
                // 업적에서는 받을 수 있는 보상아이템 개수 ( 0 ~ )

    System.Action cbClickTaken;
    protected MissionBundle data;
    List<MissionListItemScript> list = new List<MissionListItemScript>();

    public MissionBundle Data { get { return data; } }

    public virtual void Init(MissionBundle _data, System.Action _cbClickTaken)
    {
        data = _data;
        cbClickTaken = _cbClickTaken;

        grid.onCustomSort = ListSort;
        grid.sorting = UIGrid.Sorting.Custom;

        for (int j = 0; j < list.Count; ++j)
        {
            list[j].transform.parent = GameCore.Instance.Ui_root;
            Destroy(list[j].gameObject);
        }
        list.Clear();

        var itemDatas = _data.datas;
        for (int j = 0; j < itemDatas.Count; ++j)
        {
            var item = MissionListItemScript.Create(transform);
            item.Init(type, j, itemDatas[j], CBOnClickTaken);
            list.Add(item);
        }

        grid.enabled = true;

        if (bStrGuideInit == false)
        {
            bStrGuideInit = true;

            var constData = GameCore.Instance.DataMgr.GetMissionConstData();
            string am = constData.resetMissionTime < 12 ? "오전" : "오후";
            int hour = ((constData.resetMissionTime - 1) % 12) + 1;

            strGuide[0] = string.Format(strGuide[0], am, hour);
            strGuide[1] = string.Format(strGuide[1], am, hour, DailyDungeonUI.GetWeekStr(constData.ResetMissionWeek));
        }
    }

    public string GetMainText()
    {
        if (type == MissionType.Quest)
            return string.Format("{0}{1}", strMain[(int)type], GameCore.Instance.DataMgr.GetMissionAccumRewardData(data.topData.UID).level);
        else
            return strMain[(int)type];
    }

    public string GetGuideText()
    {
        return strGuide[(int)type];
    }

    public bool IsTakable()
    {
        var topData = GameCore.Instance.DataMgr.GetMissionAccumRewardData(data.topData.UID);
        //return data.topData.REWARD == false && topData.value <= GetCompleteCount();
        return data.topData.REWARD == false && topData.value <= data.topData.VALUE;
    }

    #region Complete Count

    public bool IsComplete()
    {
        //return GetTargetCompleteCount() <= GetCompleteCount();
        return data.topData.VALUE >= GameCore.Instance.DataMgr.GetMissionAccumRewardData(data.topData.UID).value;
    }

    public string GetCompletCounteText()
    {
        //return string.Format("{0}[c]/{1}", GetCompleteCount(), GetTargetCompleteCount());
        return string.Format("{0}[c]/{1}", data.topData.VALUE, GameCore.Instance.DataMgr.GetMissionAccumRewardData(data.topData.UID).value);
    }


    public int GetTargetCompleteCount()
    {
        //switch (type)
        //{
        //    case MissionType.Daily:
        //    case MissionType.Weekly:    return data.datas.Length;
        //    case MissionType.Achieve:   return 10;
        //    case MissionType.Quest:     return data.datas.Length; //GetTargetCompleteCountByLevel(lvlIdx);
        //}
        //return -1;
        return GameCore.Instance.DataMgr.GetMissionAccumRewardData(data.topData.UID).value;
    }

    public int GetCompleteCount()
    {
        switch (type)
        {
            case MissionType.Daily:
            case MissionType.Weekly:   return GetCompleteCountInternal();
            case MissionType.Achieve:  return GetCompleteCountByLevel();
            case MissionType.Quest:    return GetCompleteCountInternal(); //GetCompleteCountByLevel(lvlIdx);
        }
        return -1;
    }

    int GetCompleteCountInternal()
    {
        int cnt = 0;
        for (int i = 0; i < data.datas.Count; ++i)
            if (data.datas[i].state == MissionState.Complete)
                cnt++;
        return cnt;
    }

    int GetCompleteCountByLevel()
    {
        int cnt = 0;
        for (int i = 0; i < data.datas.Count; ++i)
        {
            cnt += GameCore.Instance.DataMgr.GetMissionAchieveData(data.datas[i].UID).level-1;
            if (data.datas[i].state == MissionState.Complete)
                cnt += 1;
        }
        return cnt;
    }

    internal virtual void UpdateData(MissionSData _sdata)
    {
        switch(_sdata.type)
        {
            case MissionType.Daily:
            case MissionType.Weekly:
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i].GetID() == _sdata.UID)
                    {
                        list[i].Init(_sdata.type, i, _sdata, cbClickTaken);
                        break;
                    }
                }
                break;

            case MissionType.Achieve:
                var defineKey = GameCore.Instance.DataMgr.GetMissionData(_sdata.type, _sdata.UID).defineKey;
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i].GetDefineKey() == defineKey)
                    {
                        list[i].Init(_sdata.type, i, _sdata, cbClickTaken);
                        break;
                    }
                }
                break;

            case MissionType.Quest:
                break;

        }
        if(grid != null)
            grid.enabled = true;
    }
    #endregion

    void CBOnClickTaken()
    {
        if (cbClickTaken != null) cbClickTaken();
    }

    int ListSort(Transform _1, Transform _2)
    {
        var data1 = _1.GetComponent<MissionListItemScript>();
        var data2 = _2.GetComponent<MissionListItemScript>();

        var state1 = (int)data1.GetState();
        var state2 = (int)data2.GetState();
        if (state1 != state2)
            return state1.CompareTo(state2);

        return data1.GetIndex().CompareTo(data2.GetIndex());
    }
}
