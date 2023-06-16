using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AttendanceUI : MonoBehaviour
{
    [SerializeField] GameObject _tabPrefeb;

    [SerializeField] GameObject _touchBlock;

    [Space][Header(" - Tabs",order = 1)]
    [SerializeField] UIGrid _tabGrid;

    GameObject[] _tabs;
    GameObject[] _tabHighlights;
    GameObject[] _tabSelects;

    int _nowSelectTab = -1; // 0 ~ 


    [Space] [Header(" - Content", order = 1)]
    [SerializeField] UISprite _banner;


    [Space] [Header(" - Content", order = 1)]
    [SerializeField] GameObject _weekContentRoot;
    [SerializeField] GameObject _monthContentRoot;
    AttendanceItem[] _weekContentItems;
    AttendanceItem[] _monthContentItems;

    AttendanceItem[] _activeContentItems; // 포인터

    //AttendanceSData[] datas;
    ACheckDataMap[] _datas;

    // 팝업용 임시 변수들
    int _num;                    // 받는 데이터의 인덱스
    CardDataMap _rewardData;     // 받는 데이터
    CardSData[] _rewardSData;    // 받는 데이터용 SData 배열
    int _rewardCount;            // 받는 아이템 개수



    private void Awake()
    {
        _weekContentItems = new AttendanceItem[_weekContentRoot.transform.childCount];
        _monthContentItems = new AttendanceItem[_monthContentRoot.transform.childCount];

        for (int i = 0; i < _weekContentItems.Length; ++i)
        {
            _weekContentItems[i] = _weekContentRoot.transform.GetChild(i).GetComponent<AttendanceItem>();
        }
        for (int i = 0; i < _monthContentItems.Length; ++i)
        {
            _monthContentItems[i] = _monthContentRoot.transform.GetChild(i).GetComponent<AttendanceItem>();
        } 
    }


    bool noReq = false;
    public void Init(bool _noReq = false)
    {
        var list = GameCore.Instance.DataMgr.GetACheckList();

        // 활성 일자가 맞지 않는 데이터 제외 ( 테이블과 서버가 완료되면 주석 풀기 )
        var nowDate = GameCore.nowTime.Date;
        for (int i = list.Count - 1; 0 <= i; --i)
        {
            if (nowDate < list[i].startDate.Date || list[i].endDate < nowDate)
            {
                list.RemoveAt(i);
            }
        }
            

        list.Sort((a, b) => { return a.order.CompareTo(b.order); });

        _datas = list.ToArray();
        noReq = _noReq;
        CreateTabs();

        if (_nowSelectTab < 0)
        {
            _nowSelectTab = 0;
            SetTab(0);
        }
        else
        {
            SetTab(_nowSelectTab);
        }
           

        noReq = false;
    }

    void CreateTabs()
    {
        if (_tabs != null)
            return;

        _tabs = new GameObject[_datas.Length];
        _tabHighlights = new GameObject[_tabs.Length];
        _tabSelects = new GameObject[_tabs.Length];
        for (int i = 0; i < _tabs.Length; ++i)
        {
            int n = i;

            _tabs[i] = Instantiate(_tabPrefeb, _tabGrid.transform);
            _tabHighlights[i] = UnityCommonFunc.GetGameObjectByName(_tabs[i], "highlight");
            _tabSelects[i] = UnityCommonFunc.GetGameObjectByName(_tabs[i], "spSelect");
            _tabs[i].GetComponent<UIButton>().onClick.Add(new EventDelegate(() => OnClickTab(n)));
            if (IsTakable(_datas[i]))
                _tabHighlights[i].SetActive(true);

            // SetTab Sprite
            GameCore.Instance.SetUISprite(_tabs[i].GetComponent<UISprite>(), _datas[i].tabTextureID);
        }

        _tabGrid.enabled = true;
    }

    internal bool IsTakable(ACheckDataMap _data)
    {
        DateTime takedDate = GameCore.Instance.PlayerDataMgr.GetAttendanceLastTakedDate(_data.id);
        int lastTakedKey = GameCore.Instance.PlayerDataMgr.GetAttendanceLastTakedValue(_data.id);
        int firstKey = GameCore.Instance.DataMgr.GetFirstACheckReward(_data.id);

        return firstKey + (_data.type == ACheckType.Month ? 27 : 6) > lastTakedKey &&
               takedDate.Date < GameCore.nowTime.Date;
    }

    public void OffHighLightByKey(int _key)
    {
        for(int i = 0; i < _datas.Length; ++i)
        {
            if (_datas[i].id == _key)
            {
                _tabHighlights[i].SetActive(false);
            }  
        } 
    }


    public void SetTab(int _idx)
    {
        _tabSelects[_nowSelectTab].gameObject.SetActive(false);
        _tabSelects[_idx].gameObject.SetActive(true);
        _nowSelectTab = _idx;

        // 오브젝트 활성화 
        int dayCnt = 0;
        if (_datas[_idx].type == ACheckType.Month) // 28일 짜리라면
        {
            _weekContentRoot.SetActive(false);
            _monthContentRoot.SetActive(true);
            _activeContentItems = _monthContentItems;
            dayCnt = 28;
        }
        else // 7일 짜리라면
        {
            _weekContentRoot.SetActive(true);
            _monthContentRoot.SetActive(false);
            _activeContentItems = _weekContentItems;
            dayCnt = 7;
        }

        // 항목 초기화
        DateTime takedDate = GameCore.Instance.PlayerDataMgr.GetAttendanceLastTakedDate(_datas[_idx].id);
        int lastTakedKey = GameCore.Instance.PlayerDataMgr.GetAttendanceLastTakedValue(_datas[_idx].id);
        int firstKey = GameCore.Instance.DataMgr.GetFirstACheckReward(_datas[_idx].id);
        var state = AttendanceItemState.Taked;
        bool first = true;
        for (int i = 0; i < dayCnt; ++i)
        {
            var data = GameCore.Instance.DataMgr.GetACheckRewardData(firstKey + i);

            if (data.reward <= 0)
            {
                state = AttendanceItemState.None;
            }
            else if (data.id < lastTakedKey)
            {
                state = AttendanceItemState.Taked;
            }
            else if (data.id == lastTakedKey)
            {
                if (takedDate.Date == GameCore.nowTime.Date)
                {
                    state = AttendanceItemState.Taking;
                }
                else
                {
                    state = AttendanceItemState.Taked;
                }   
            }
            else if (first)
            {
                first = false;
                _num = i;
                
                if (takedDate.Date < GameCore.nowTime.Date)
                {
                    state = AttendanceItemState.Takable;
                }
                else
                {
                    state = AttendanceItemState.Remain;
                }   
            }
            else
            {
                state = AttendanceItemState.Remain;
            }

            _activeContentItems[i].Init(data, state, OnClickTakeItem);
        }

        // Set Banner
        GameCore.Instance.SetUISprite(_banner, _datas[_idx].textureID);

        if (!noReq)
        {
            if (IsTakable(_datas[_idx]) && takedDate.Date < GameCore.nowTime.Date)
            {
                GameCore.Instance.NetMgr.Req_Attendance_Receive(_datas[_nowSelectTab].id);
            }  
        }
    }


    void OnClickTab(int _idx)
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        SetTab(_idx);
    }

    void OnClickTakeItem(int _num)
    {
        //num = _num;
        //GameCore.Instance.NetMgr.Req_Attendance_Receive(datas[nowSelectTab].id);
    }

    public void ShowTakedEffect()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Attendance);

        int firstKey = GameCore.Instance.DataMgr.GetFirstACheckReward(_datas[_nowSelectTab].id);
        var data = GameCore.Instance.DataMgr.GetACheckRewardData(firstKey + _num);
        var key = data.reward;
        bool isItem = CardDataMap.IsItemKey(key);

        _rewardCount = data.rewardValue;
        _rewardData = (isItem ? (CardDataMap)GameCore.Instance.DataMgr.GetItemData(key) :
                               (CardDataMap)GameCore.Instance.DataMgr.GetUnitData(key));

        _rewardSData = new CardSData[] { (isItem ? (CardSData)new ItemSData(key, _rewardCount) :
                                                  (CardSData)new HeroSData(key)) };

        _activeContentItems[_num].SetState(AttendanceItemState.Taking);
        GameCore.Instance.DoWaitCall(1f, CBShowTakePopup);
        _touchBlock.SetActive(true);
    }

    public void CBShowTakePopup()
    {
        _touchBlock.SetActive(false);
        if (_rewardCount == 1)
        {
            GameCore.Instance.ShowReceiveItemPopup("출석보상", string.Format("[7E00FFFF][c]{0}[/c][-] 획득 하였습니다.", _rewardData.name), _rewardSData);
        }
        else
        {
            GameCore.Instance.ShowReceiveItemPopup("출석보상", string.Format("[7E00FFFF][c]{0} {1}[/c][-]개 획득 하였습니다.", _rewardData.name, _rewardCount), _rewardSData);
        }  
    }
}
