using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MissionListItemScript : MonoBehaviour
{
    public static MissionListItemScript Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Lobby/MissionListItem", _parent);
        return go.GetComponent<MissionListItemScript>();
    }

    [SerializeField] UILabel lbName;
    [SerializeField] UILabel lbLevel;
    [SerializeField] UILabel lbDesc;
    [SerializeField] UISlider slider;

    [SerializeField] UILabel lbCounter;

    [SerializeField] Transform cardRoot;
    [SerializeField] GameObject goObtain;
    [SerializeField] GameObject goTakableEffect;

    [SerializeField] UILabel lbState;
    [SerializeField] UIButton button;

    [SerializeField] GameObject goLock;
    [SerializeField] GameObject goBlind;


    MissionSData sdata;
    Action cbTake;
    int idx;
    CardBase card;
    MissionDataMap data;

    public void Init(MissionType _type, int _idx, MissionSData _sdata, Action _cbTake)
    {
        sdata = _sdata;
        cbTake = _cbTake;
        idx = _idx;

        switch (_type)
        {
            case MissionType.Daily:    data = GameCore.Instance.DataMgr.GetMissionDailyData(_sdata.UID); break;
            case MissionType.Weekly:   data = GameCore.Instance.DataMgr.GetMissionWeeklyData(_sdata.UID); break;
            case MissionType.Achieve:  data = GameCore.Instance.DataMgr.GetMissionAchieveData(_sdata.UID);
                                       lbLevel.text = string.Format("LV.{0}", ((AchieveDataMap)data).level); break;
            case MissionType.Quest:    data = GameCore.Instance.DataMgr.GetMissionQuestData(_sdata.UID); break;
        }

        var missionDefine = GameCore.Instance.DataMgr.GetMissionDefineData(data.defineKey);

        lbName.text = string.Format("{0}.{1}", _idx+1, missionDefine.name);
        lbDesc.text = MissionDefineDataMap.GetMissionDiscString(data.defineKey, data.value1, data.value2);

        SetCount(_sdata.VALUE, data.value1);
        SetRewardItem( new ItemSData(data.rewardKey, data.rewardValue));
        SetState(_sdata.state);
    }

    public MissionState GetState()
    {
        return sdata.state;
    }

    public int GetID()
    {
        return data.id;
    }
    public int GetDefineKey()
    {
        return data.defineKey;
    }
    public int GetIndex()
    {
        return idx;
    }

    void SetCount(int _now, int _target)
    {
        slider.value = (float)_now / _target;

        if (sdata.state == MissionState.Complete ||
           sdata.state == MissionState.Takable)
#if UNITY_EDITOR
            lbCounter.text = string.Format("{0:N0}[c] / {1:N0}", _now, _target);
#else
            lbCounter.text = string.Format("{0:N0}[c] / {1:N0}", _target, _target);
#endif
        else
            lbCounter.text = string.Format("[c]{0:N0} / {1:N0}", _now, _target);
    }

    void SetRewardItem(ItemSData _data)
    {
        if (card != null)
            Destroy(card.gameObject);
        card = CardBase.CreateSmallCard(_data, cardRoot);
    }

    public void SetState(MissionState _state)
    {
        sdata.state = _state;
        goLock.SetActive(_state == MissionState.Lock);
        button.gameObject.SetActive(_state == MissionState.Running || _state == MissionState.Takable);

        switch (_state)
        {
            case MissionState.Lock:
                lbState.text = "미완료";
                goObtain.SetActive(false);
                card.SetEnable(true);
                goBlind.SetActive(true);
                goTakableEffect.SetActive(false);
                break;

            case MissionState.Running:
                lbState.text = "이동";
                goObtain.SetActive(false);
                card.SetEnable(true);
                goBlind.SetActive(false);
                goTakableEffect.SetActive(false);
                button.normalSprite = CommonType.BTN_3_NORMAL;
                button.pressedSprite = CommonType.BTN_3_ACTIVE;

                button.onClick.Clear();
                button.onClick.Add(new EventDelegate(CBForwardLocation));
                break;

            case MissionState.Takable:
                lbState.text = "받기";
                goObtain.SetActive(false);
                card.SetEnable(true);
                goBlind.SetActive(false);
                goTakableEffect.SetActive(true);
                button.normalSprite = CommonType.BTN_2_NORMAL;
                button.pressedSprite = CommonType.BTN_2_ACTIVE;

                button.onClick.Clear();
                button.onClick.Add(new EventDelegate(OnClickTake));
                break;

            case MissionState.Complete:
                lbState.text = "완료";
                goObtain.SetActive(true);
                card.SetEnable(false);
                goBlind.SetActive(true);
                goTakableEffect.SetActive(false);
                break;
        }
    }


    public void OnClickTake()
    {
        sdata.state = MissionState.Complete;
        SetState(MissionState.Complete);
        GameCore.Instance.NetMgr.Req_Mission_Reward(sdata.type, sdata.UID);
        if(cbTake != null) cbTake();
    }

    void CBForwardLocation()
    {
        MissionDefineDataMap.MoveRedirect(data.defineKey, data.value1, data.value2);
        GameCore.Instance.MissionMgr.Close();
    }
}
