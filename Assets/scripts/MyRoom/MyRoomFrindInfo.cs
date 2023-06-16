using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using IDH.MyRoom;

public class MyRoomFrindInfo : MonoBehaviour
{
    public class ColorLabel
    {
        public enum Type
        {
            Satisfaction,
            Dirty,
            OpenRoomCount
        }

        public const string SatisfactionFormat = "[F600FFFF]{0}[-] / 400";
        public const string DirtyFormat = "[24FF00FF]{0}[-] / {1}";
        public const string OpenRoomCountFormat = "[FF8800FF]{0}[-] / 10";

        public UILabel Label { get; set; }
        public Type FormatType { get; set; }

        public ColorLabel(UILabel label, Type type)
        {
            Label = label;
            FormatType = type;
        }

        public void SetColorLabel(string value, string maxValue)
        {
            switch (FormatType)
            {
                case Type.Satisfaction:
                    Label.text = string.Format(SatisfactionFormat, value);
                    break;
                case Type.Dirty:
                    Label.text = string.Format(DirtyFormat, value, maxValue);
                    break;
                case Type.OpenRoomCount:
                    Label.text = string.Format(OpenRoomCountFormat, value);
                    break;
            }
        }
    }

    public UILabel UserLevel;
    public UILabel UserName;
    public UILabel UserComment;
    public UISprite UserProfileIcon;

    public UILabel UserSatisfactionValue;
    public UILabel UserDirtyValue;
    public UILabel UserOpenRoomCount;
    public UILabel UserLastLoginTime;

    public UIButton VisitButton;

    private ColorLabel SatisfactionLabel;
    private ColorLabel DirtyLabel;
    private ColorLabel OpenRoomLabel;
    private long targetUserUID;

    public void Initialize(MyRoomFriendData data, Action<long> buttonCallBack)
    {
        targetUserUID = data.FriendUID;

        UserLevel.text = string.Concat("LV ", data.UserLevel.ToString());
        UserName.text = data.UserName;
        UserComment.text = GameCore.Instance.PlayerDataMgr.GetFriendOrNull(data.FriendUID).COMM;

        int delegateIconID = GameCore.Instance.PlayerDataMgr.GetFriendOrNull(data.FriendUID).DELEGATE_ICON;

        var unit = GameCore.Instance.DataMgr.GetUnitData(delegateIconID);
        GameCore.Instance.SetUISprite(UserProfileIcon, unit.GetBigProfileSpriteKey());

        SatisfactionLabel = new ColorLabel(UserSatisfactionValue, ColorLabel.Type.Satisfaction);
        DirtyLabel = new ColorLabel(UserDirtyValue, ColorLabel.Type.Dirty);
        OpenRoomLabel = new ColorLabel(UserOpenRoomCount, ColorLabel.Type.OpenRoomCount);

        //SatisfactionLabel.SetColorLabel(data.)
        SatisfactionLabel.SetColorLabel(data.SatisfactionCount.ToString(), "");
        DirtyLabel.SetColorLabel(data.StainCount.ToString(), "10" /*(data.OpenRoomCount * 10).ToString()*/);
        OpenRoomLabel.SetColorLabel(data.OpenRoomCount.ToString(), "");

        int h = (int)((GameCore.nowTime - data.LastLoginTime).TotalHours);
        h = Mathf.Abs(h);
        string timeString = "";
        if (h == 0) timeString = "방금 전";
        else if (h < 24) timeString = string.Format(" {0} 시간 전", h);
        else timeString = string.Format("{0} 일 전 ", h / 24);

        UserLastLoginTime.text = timeString;

        VisitButton.onClick.Add(new EventDelegate(() => { buttonCallBack.Invoke(targetUserUID); }));
    }
}
