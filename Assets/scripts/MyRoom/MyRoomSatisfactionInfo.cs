using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IDH.MyRoom;
using System;

public class MyRoomSatisfactionInfo : MonoBehaviour
{
    private const string SatisfactionValueTextFormat = "[F600FFFF]{0}[-] / 400";
    private const int SatisfactionMaxValue = 400;
    [System.Serializable]
    public class SatisfactionLevelObject
    {
        public UISlider MainSlider;
        public UISlider SubSlider;
        public UISprite Mark;
        public UILabel EffectText;
        public Color effectTextColor;

        public SatisfactionLevelObject(UISlider slider)
        {
            MainSlider = slider;
            //Mark = mark;
        }

        public void SetDefault()
        {
            MainSlider.value = SubSlider.value = 0.0f;
            if(Mark != null) Mark.color = UIColorPalette.Color06;
            EffectText.color = UIColorPalette.Color06;
        }

        public void SetValue(float value)
        {
            SubSlider.value = value;
            if (Mark == null)
                return;
            if (Mark.color == UIColorPalette.Color02) return;
            if (value >= 1.0f)
            {
                Mark.color = UIColorPalette.Color03;
            }
            else
            {
                Mark.color = UIColorPalette.Color06;
            }
        }

        public void Apply()
        {
            MainSlider.value = SubSlider.value;

            if (MainSlider.value >= 1.0f)
            {
                //Mark.color = UIColorPalette.Color02;
                EffectText.color = effectTextColor;
            }
            if (Mark == null)
                return;
            if (MainSlider.value >= 1.0f)
            {
                Mark.color = UIColorPalette.Color02; 
            }
            else
            {
                Mark.color = UIColorPalette.Color06;
            }
        }

    }
    [Header("Ref Object")]
    public GameObject Detail;
    public SatisfactionLevelObject[] SatisfactionLevelObjectList;

    public UILabel SatisfactionValueLabel;
    public UILabel DustEffectValueLabel;

    public int CurrentValue { get; private set; }

    private MyRoomSystemRefParameter Parameter { get; set;}

    public void Initialize(MyRoomSystemRefParameter parameter)
    {
        Parameter = parameter;

        var data = GameCore.Instance.DataMgr.GetMyRoomData(Parameter.CurrentRoomID);

        for (int i = 0; i < SatisfactionLevelObjectList.Length; ++i)
        {
            SatisfactionLevelObjectList[i].SetDefault();
            SatisfactionLevelObjectList[i].EffectText.text = MyRoomDataMap.GetStrMyRoomEffect(data.satisfactionEffectID[i], data.satisfactionEffectValue[i]);
        }

        parameter.Observer.OnStartMyRoomObjectEditMode += AddSatisFactionValue;
        parameter.Observer.OnEndMyRoomObjectEditMode += ApplyValue;

        Parameter.Observer.OnPlacedMyRoomObject += ApplyValue;

        parameter.Observer.OnInitializedMyRoomSystem += () =>
        {
            ApplyValue(parameter.SatisfactionValue);
        };

        parameter.Observer.OnClearRoom += () =>
        {
            ApplyValue(parameter.SatisfactionValue);
        };
    }

    private void AddSatisFactionValue(IPlaceAbleObject target)
    {
        if (target == null || target.IsSameClass(typeof(MyRoomObject)) == false)
            return;

        MyRoomObject myRoomObject = target as MyRoomObject;
        int satisfactionValue = Parameter.SatisfactionValue - myRoomObject.LocalData.optionValue[0];
        ApplyValue(satisfactionValue);
        var dustValue = GameCore.Instance.PlayerDataMgr.UserMyRoom.MyRoomDataList.Find(x => x.ID == Parameter.CurrentRoomID).StainDataList.Count * 10;
        SetSatisFactionValue(satisfactionValue + myRoomObject.LocalData.optionValue[0] - dustValue);
    }

    private void SetSatisFactionValue(int satisfactionValue)
    {
        satisfactionValue = Mathf.Clamp(satisfactionValue, 0, SatisfactionMaxValue);

        SatisfactionValueLabel.text = string.Format(SatisfactionValueTextFormat, satisfactionValue);

        if (satisfactionValue == SatisfactionMaxValue)
        {
            for (int i = 0; i < SatisfactionLevelObjectList.Length; ++i)
            {
                SatisfactionLevelObjectList[i].SetValue(1.0f);
            }
        }
        else
        {
            int maxValueIndex = ((satisfactionValue + 100) / 100) - 1;
            int remainValue = (satisfactionValue - (100 * maxValueIndex));
            float remainPercent = ((float)remainValue) / 100.0f;

            for (int i = SatisfactionLevelObjectList.Length - 1; i >= 0; --i)
            {
                if (i == maxValueIndex) SatisfactionLevelObjectList[i].SetValue(remainPercent);
                else if(i > maxValueIndex) SatisfactionLevelObjectList[i].SetValue(0.0f);
                else SatisfactionLevelObjectList[i].SetValue(1.0f);
            }
        }
    }

    private void ApplyValue(int satisfactionValue)
    {
        var dustValue = GameCore.Instance.PlayerDataMgr.UserMyRoom.MyRoomDataList.Find(x=> x.ID == Parameter.CurrentRoomID).StainDataList.Count * 10;
        //var dustValue = GameCore.Instance.PlayerDataMgr.UserMyRoom.MyRoomDataList[Parameter.CurrentRoomID - 1].StainDataList.Count * 10;
        if (dustValue == 0) DustEffectValueLabel.text = "";
        else                DustEffectValueLabel.text = string.Format("먼지로 인해 만족도 [c][FE1111]{0}[-][/c] 하락", dustValue);
        SetSatisFactionValue(satisfactionValue - dustValue);
        ApplyValue();
    }

    private void ApplyValue(IPlaceAbleObject placedObject)
    {
        ApplyValue(Parameter.SatisfactionValue);
    }

    private void ApplyValue()
    {
        for (int i = 0; i < SatisfactionLevelObjectList.Length; ++i)
        {
            SatisfactionLevelObjectList[i].Apply();
        }
    }

    public void OnClickSatisfactionPanelButton()
    {
        Detail.gameObject.SetActive(!Detail.activeInHierarchy);
    }
    public void TurnOffSatisfactionPanelButton()
    {
        Detail.gameObject.SetActive(false);
    }

    internal void UpdateValue()
    {
        ApplyValue(Parameter.SatisfactionValue);
    }
}
