using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttendanceItemState
{
    None,
    Taked, 
    Takable,
    Taking,
    Remain,
}

public class AttendanceItemSData
{
    public int id;
    public int rewardItemKey;
    public int dayIndex;
    public AttendanceItemState state; // None, Taked, Takable, Remain
}

public class AttendanceItem : MonoBehaviour
{
    [SerializeField] UISprite spBG;
    [SerializeField] GameObject spClose;
    [SerializeField] GameObject goTake;
    [SerializeField] GameObject goTakeMark;
    [SerializeField] GameObject goTakeEff;
    [SerializeField] GameObject goTakeMarkEff;
    [SerializeField] UILabel lbDayCount;
    [SerializeField] Transform rootItemCard;

    [SerializeField] bool isBig;


    ACheckRewardDataMap data;
    //AttendanceItemSData data;
    AttendanceItemState state;

    CardBase card;

    List<UISprite> sprites = new List<UISprite>();

    Action<int> cbTake;

    internal void Init(ACheckRewardDataMap _data, AttendanceItemState _state, Action<int> _cbTake)
    {
        data = _data;
        cbTake = _cbTake;

        lbDayCount.text = _data.day + "일차";

        if (card != null)
        {
            card.transform.parent = null; // 즉시 삭제되지 않기때문에 sprites에 들어가버린다. 이를 방지하기 위함.
            Destroy(card.gameObject);
        }

        if (_state != AttendanceItemState.None)
        {
            card = isBig ? CardBase.CreateBigCardByKey(_data.reward, rootItemCard, CBOnClick, CBOnPress) :
                            CardBase.CreateSmallCardByKey(_data.reward, rootItemCard, CBOnClick, CBOnPress);
            card.SetStopPressCallback(CBStopPress);
            card.SetCount(_data.rewardValue);

            if (isBig && CardDataMap.IsUnitKey(_data.reward))
            {
                card.transform.localPosition = new Vector3(8f, 2f, 0f);
                card.transform.localScale = Vector3.one * 0.8f;
            }
        }

        sprites.Clear();
        sprites.AddRange(GetComponentsInChildren<UISprite>());

        if (isBig) spBG.spriteName = _data.day % 7 == 0 ? "ITEM_BOX_01_01" : "ITEM_BOX_01";
        else       spBG.spriteName = _data.day % 7 == 0 ? "ITEM_BOX_03_01" : "ITEM_BOX_03";

        SetState(_state, true);
    }


    public void SetState(AttendanceItemState _state, bool _force = false)
    {
        state = _state;

        goTake.SetActive(_state == AttendanceItemState.Taked || (_force && _state == AttendanceItemState.Taking));
        goTakeMark.SetActive(_force && _state == AttendanceItemState.Taking);
        goTakeEff.SetActive(!_force && _state == AttendanceItemState.Taking);
        goTakeMarkEff.SetActive(!_force && _state == AttendanceItemState.Taking);
        spClose.SetActive(_state == AttendanceItemState.None);
        rootItemCard.gameObject.SetActive(_state != AttendanceItemState.None);

        SetGray(_state == AttendanceItemState.None || _state == AttendanceItemState.Taked);
    }



    void SetGray(bool _gray)
    {
        foreach (var sprite in sprites)
        {
            if (sprite.gameObject == goTake)
                continue;
            sprite.GrayScale(_gray);
        }
    }

    

    void CBOnClick(long _key)
    {
        if (state == AttendanceItemState.Takable)
        {
            cbTake(data.day - 1);
            //SetState(AttendanceItemState.Taking);
        }
    }



    void CBOnPress(long _key)
    {
        GameCore.Instance.ShowCardInfoNotHave(data.reward);
    }



    void CBStopPress()
    {
        GameCore.Instance.CloseAlert();
    }
}

