using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using IDH.MyRoom;

public class StainObject : MonoBehaviour
{
    public enum State
    {
        DirtyDust,
        Cleaning,
        Present,
        Removed
    }
    [Header("Ref Object(Header)")]
    public UISprite Sprite;
    public UIButton Button;
    public UISprite TopLabel;

    public UISprite DirtyDustIcon;
    public UISprite CleaningIcon;
    public UISprite PresentBoxIcon;
    public UIProgressBar TimeBar;
    public UISprite TimeBackGroundBar;
    public UISprite TimeForeGroundBar;
    public UILabel TimeLabel;
    public UILabel NameLabel;

    public MyRoomStainData StainData { get; set; }
    public State CurrentState { get; private set; }

    private MyRoomSystemCommand Command { get; set; }
    private bool IsVisit;

    public void Initialize(MyRoomStainData data, MyRoomSystemRefParameter parameter)
    {
        IsVisit = parameter.IsVisit;
        DeActiveAllIcons();
        UpdateStainData(data);
        Command = parameter.Command;
    }

    public void SetLayerDepth(int mainValue)
    {
        Sprite.depth = mainValue;
        DirtyDustIcon.depth = mainValue + 1;
        CleaningIcon.depth = mainValue + 1;
        PresentBoxIcon.depth = mainValue + 1;
        TopLabel.depth = mainValue + 1;
        TimeBackGroundBar.depth = mainValue + 2;
        TimeForeGroundBar.depth = mainValue + 3;
        TimeLabel.depth = mainValue + 4;
        NameLabel.depth = mainValue + 2;
    }

    public void UpdateStainData(MyRoomStainData data)
    {
        DeActiveAllIcons();

        StainData = data;

        if (StainData.CleanEndTime == DateTime.MinValue) CurrentState = State.DirtyDust;
        else if (StainData.CleanEndTime <= GameCore.nowTime) CurrentState = State.Present;
        else CurrentState = State.Cleaning;

        switch (CurrentState)
        {
            case State.DirtyDust:
                SetDirtyDustState();
                break;
            case State.Cleaning:
                SetCleaningState();
                break;
            case State.Present:
                SetPresentState();
                break;
        }
    }

    public void ChangeRemovedState()
    {
        gameObject.SetActive(false);
        CurrentState = State.Removed;
    }

    private void DeActiveAllIcons()
    {
        if (TopLabel != null) TopLabel.gameObject.SetActive(false);
        if (DirtyDustIcon != null) DirtyDustIcon.gameObject.SetActive(false);
        if (CleaningIcon != null) CleaningIcon.gameObject.SetActive(false);
        if (PresentBoxIcon != null) PresentBoxIcon.gameObject.SetActive(false);
        if (TimeBar != null) TimeBar.gameObject.SetActive(false);
        if (TimeLabel != null) TimeLabel.gameObject.SetActive(false);
        if (NameLabel != null) NameLabel.gameObject.SetActive(false);
    }

    private void SetDirtyDustState()
    {
        DirtyDustIcon.gameObject.SetActive(true);
        //if (IsVisit) return;
        Button.onClick.Clear();
        Button.onClick.Add(new EventDelegate(() => 
        {
            Command.CmdCleanDirtyDust(StainData.UniqueId);
            if(IsVisit) ChangeRemovedState();
        }));
    }

    private void SetCleaningState()
    {
        CleaningIcon.gameObject.SetActive(true);
        TopLabel.gameObject.SetActive(true);

        DateTime now = GameCore.nowTime;

        TimeSpan currentTimeDiff = now - StainData.CleanStartTime;
        TimeSpan goalTimeDiff = StainData.CleanEndTime - StainData.CleanStartTime;

        int totalSecond = goalTimeDiff.Minutes * 60 + goalTimeDiff.Seconds;
        int currentSecond = currentTimeDiff.Minutes * 60 + currentTimeDiff.Seconds;

        TimeBar.value = ((float)currentSecond / (float)totalSecond);
        TimeBar.gameObject.SetActive(true);
        
        TimeSpan timeDiff = StainData.CleanEndTime - GameCore.nowTime;
        TimeLabel.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeDiff.Hours, timeDiff.Minutes, timeDiff.Seconds);
        TimeLabel.gameObject.SetActive(true);

        if (gameObject.activeInHierarchy)
            StartCoroutine(CleaningTimerCoroutine(totalSecond, currentSecond));
    }

    private IEnumerator CleaningTimerCoroutine(int totalSecond, int currentSecond)
    {
        //WaitForSecondsRealtime wfsTemp = new WaitForSecondsRealtime(1.0f);
        DateTime calStartTime = GameCore.nowTime;
        float time = currentSecond;
        while (time < totalSecond)
        {
            yield return null;

            if (GameCore.Instance.SubsysMgr.GetNowSysType() != SubSysType.MyRoom)
                yield break;

            time += Time.unscaledDeltaTime;
            calStartTime = calStartTime.AddSeconds(Time.unscaledDeltaTime);
            TimeSpan timeDiff = StainData.CleanEndTime - calStartTime;
            TimeBar.value = (time / (float)totalSecond);
            TimeLabel.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeDiff.Hours, timeDiff.Minutes, timeDiff.Seconds);
        }

        DeActiveAllIcons();
        SetPresentStateOfMine();
    }

    /// <summary>
    /// 스틸 될듯 나중에 막아놔야함
    /// </summary>
    private void SetPresentState()
    {
        PresentBoxIcon.gameObject.SetActive(true);

        if (StainData.HelpUserId > 0 && StainData.HelpUserName != GameCore.Instance.PlayerDataMgr.LocalUserData.Name) 
        {
            NameLabel.text = StainData.HelpUserName;
            NameLabel.gameObject.SetActive(true);
            TopLabel.gameObject.SetActive(true);
        }

        Button.onClick.Clear();
        if (IsVisit) return;

        Button.onClick.Add(new EventDelegate(() => 
        {
            Command.CmdReceiveReward(StainData.UniqueId);
            ChangeRemovedState();
        }));
    }

    private void SetPresentStateOfMine()
    {
        PresentBoxIcon.gameObject.SetActive(true);
        Button.onClick.Clear();
        Button.onClick.Add(new EventDelegate(() =>
        {
            Command.CmdReceiveReward(StainData.UniqueId);
            ChangeRemovedState();
        }));
    }
}
