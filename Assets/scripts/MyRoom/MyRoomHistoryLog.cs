using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using IDH.MyRoom;

public class MyRoomHistoryLog : MonoBehaviour
{
    // type 1
    //  복수 전
    public const string DefensiveFailureTitle = "[FF0000FF]방어 실패"; // 빨강
    public const string DefensiveSuccessTitle = "[00F0FFFF]방어 성공"; // 하늘색

    //  복수 후
    public const string RevengeFailureTitle = "[FF0000FF]복수 실패"; // 빨강
    public const string RevengeSuccessTitle = "[00F0FFFF]복수 성공"; // 하늘색

    // type 2
    public const string CleaningHelpTitle = "[24FF00FF]도움 받음";     // 초록


    public const string RevengeDescription = "[898989FF]당신이 [/b][FFFFFFFF]{0} 님[/b][898989FF]에게 복수했습니다.";
    public const string BattleDescription = "[FFFFFFFF]{0} 님[/b][898989FF]이 당신을 공격했습니다.";
    public const string CleaningHelpDescription = "[FFFFFFFF]{0} 님[/b][898989FF]이 숙소 청소를 도와줬습니다.";

    public enum HistoryLogType
    {
        RevengeFailure,
        RevengeSuccess,
        DefensiveFailure,
        DefensiveSuccess,
        CleaningHelp,
    }

    public HistoryLogType LogType { get; set; }


    [Header("Ref Object")]
    public UILabel Title;
    public UILabel Description;
    public UILabel Time;
    public UIButton Button;
    public UILabel ButtonLabel;
    private MyRoomHistoryLogData LogData { get; set; }
    private MyRoomSystemRefParameter RefParameter { get; set; }

    public void Initialize(MyRoomSystemRefParameter parameter, MyRoomHistoryLogData data)
    {
        LogData = data;
        RefParameter = parameter;

        switch (LogData.HistoryType)
        {
            case 1:
                switch (data.REVENGE)
                {
                    case 0:
                    default:
                        if (data.SUCCESS == 1)
                        {
                            LogType = HistoryLogType.DefensiveFailure;
                            Button.onClick.Add(new EventDelegate(() => { RefParameter.Command.CmdMoveToRevengeMatch(LogData.HistoryUID); }));
                        }
                        else
                            LogType = HistoryLogType.DefensiveSuccess;
                        break;

                    case 1: LogType = HistoryLogType.RevengeFailure; break;
                    case 2: LogType = HistoryLogType.RevengeSuccess; break;
                }
                break;
            case 2:
                LogType = HistoryLogType.CleaningHelp;
                Button.onClick.Add(new EventDelegate(() => 
                {
                    var targetData = RefParameter.FriendList.Find(target => target.UserName == LogData.AttackUserName);
                    if (targetData == null) return;
                    RefParameter.Command.CmdVisitFriendRoom(targetData.FriendUID);
                }));
                break;

            default:
                return;
        }

        
        SetHistoryLog();
    }

    private void SetHistoryLog()
    {
        Color targetColor = default(Color);
        switch (LogType)
        {
            case HistoryLogType.RevengeFailure:
                Title.text = RevengeFailureTitle;
                Description.text = string.Format(RevengeDescription, LogData.AttackUserName);
                Button.gameObject.SetActive(false);
                break;

            case HistoryLogType.RevengeSuccess:
                Title.text = RevengeSuccessTitle;
                Description.text = string.Format(RevengeDescription, LogData.AttackUserName);
                Button.gameObject.SetActive(false);
                break;

            case HistoryLogType.DefensiveFailure:
                Title.text = DefensiveFailureTitle;
                Description.text = string.Format(BattleDescription, LogData.AttackUserName);
                targetColor = UIColorPalette.Color08;
                targetColor.a = 1f;
                Button.gameObject.SetActive(true);
                ButtonLabel.text = "전투";
                break;

            case HistoryLogType.DefensiveSuccess:
                Title.text = DefensiveSuccessTitle;
                Description.text = string.Format(BattleDescription, LogData.AttackUserName);
                Button.gameObject.SetActive(false);
                break;

            case HistoryLogType.CleaningHelp:
                Title.text = CleaningHelpTitle;
                Description.text = string.Format(CleaningHelpDescription, LogData.AttackUserName);
                targetColor = UIColorPalette.Color11;
                Button.gameObject.SetActive(true);
                ButtonLabel.text = "방문";
                break;
        }

        if (Button.gameObject.activeSelf)
        {
            Button.defaultColor = targetColor;
            Button.hover = targetColor;
            Button.pressed = targetColor;
        }

        Time.text = string.Format("[898989FF]{0}.{1}.{2}. {3:00}:{4:00}", LogData.CreateTime.Year,
                                                          LogData.CreateTime.Month,
                                                          LogData.CreateTime.Day,
                                                          LogData.CreateTime.Hour,
                                                          LogData.CreateTime.Minute);

        Button.UpdateColor(true);
    }
}
