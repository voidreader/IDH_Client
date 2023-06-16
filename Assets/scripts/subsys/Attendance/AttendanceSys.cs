using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class AttendanceSys : SubSysBase
{

    AttendanceUI ui;



    public AttendanceSys() : base(SubSysType.Attendance) { }



    protected override void EnterSysInner(ParaBase _para)
    {
        Name = "출석체크";
        ui = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Attendance/PanelAttendanceUI", GameCore.Instance.ui_root).GetComponent<AttendanceUI>();


        if (!GameCore.Instance.PlayerDataMgr.InitedAttendanceData)
        {
            GameCore.Instance.NetMgr.Req_Attendance_Lookup();
            ui.Init(true);
        }
        else
        {
            ui.Init();
        }

        GameCore.Instance.CommonSys.tbUi.CloseBottomBR();
        base.EnterSysInner(_para);
    }


    protected override void ExitSysInner(ParaBase _para)
    {
        if (ui != null) GameObject.Destroy(ui.gameObject);
        GameCore.Instance.CommonSys.tbUi.UncloseBottomBR();
        base.ExitSysInner(_para);
    }




    public override bool HandleMessage(GameEvent _evt)
    {
        return base.HandleMessage(_evt);
    }

    protected override void RegisterHandler()
    {
        handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();
        handlerMap.Add(GameEventType.ANS_ATTENDANCE_RECEIVE, ANS_ATTENDANCE_RECEIVE);
        handlerMap.Add(GameEventType.ANS_ATTENDANCE_LOOKUP, ANS_ATTENDANCE_LOOKUP);
        base.RegisterHandler();
    }

    internal override void ClickBackButton()
    {
        base.ClickBackButton();
    }



    bool ANS_ATTENDANCE_RECEIVE(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                int key = 0;
                para.GetField("ATTENDANCE").GetField(ref key, "AT");
                GameCore.Instance.PlayerDataMgr.SetAttendanceData(para.GetField("ATTENDANCE"));
                ui.ShowTakedEffect();
                ui.OffHighLightByKey(key);
                break;

            case 2:     GameCore.Instance.ShowNotice("실패", "요청 데이터 누락", 0); break;
            case 3:     /*GameCore.Instance.ShowNotice("실패", "오늘의 출석 체크 보상을 이미 받으셨습니다.", 0);*/ break;
            case 4:     /*GameCore.Instance.ShowNotice("실패", "이미 출석 체크 보상을 모두 받으셨습니다.", 0);*/ break;
            case 5:     GameCore.Instance.ShowNotice("실패", "오늘 일자 해당하는 출석체크가 없습니다.", 0); break;
            default:    GameCore.Instance.ShowNotice("실패", "출석 첵크 보상 받기 실패. " + code, 0); break;
        }
        return true;
    }

    bool ANS_ATTENDANCE_LOOKUP(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                GameCore.Instance.PlayerDataMgr.SetAttendanceData(para.GetField("ATTENDANCE"));
                ui.Init();
                break;

            case 2: GameCore.Instance.ShowNotice("실패", "오늘 일자 해당하는 출석체크가 없습니다.", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "출석 체크 정보받기 실패. " + code, 0); break;
        }
        return true;
    }
}
