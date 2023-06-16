using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroInfoPara : ParaBase
{
    public long uid;
    public SubSysType returnSys;
    public int teamIdx;

    public HeroInfoPara(long _charUID, SubSysType _returnSys, int _teamIdx = -1)
    {
        uid = _charUID;
        returnSys = _returnSys;
        teamIdx = _teamIdx;
    }
}

internal class HeroInfoSys : SubSysBase, ISequenceAction
{
    HeroInfoPara para;
    HeroSData sdata;
    HeroInfoUI ui;
    public HeroInfoSys() : base(SubSysType.HeroInfo)
    {
        needInitDataTypes = new InitDataType[] {
            InitDataType.Character,
            InitDataType.Item,
        };
    }

    protected override void EnterSysInner(ParaBase _para)
    {
        base.EnterSysInner(_para);

        Name = "영웅관리";

        para = _para.GetPara<HeroInfoPara>();
        sdata = GameCore.Instance.PlayerDataMgr.GetUnitSData(para.uid);

        ui = HeroInfoUI.Create(GameCore.Instance.ui_root);
        ui.Init(sdata, CBClickHeroUp, CBClickItemUp);
        GameCore.Instance.SetTutorialChild(true, 0, GetTutorialActionList, ui.GetTutorialTransformList);
        GameCore.Instance.NetMgr.Req_Character_Evaluate_Mine(sdata.key);
    }

    protected override void ExitSysInner(ParaBase _para)
    {
        if(ui != null)
        {
            GameObject.Destroy(ui.gameObject);
            ui = null;
        }

        base.ExitSysInner(_para);
    }

    protected override void RegisterHandler()
    {
        handlerMap = new Dictionary<GameEventType, System.Func<ParaBase, bool>>();
        handlerMap.Add(GameEventType.ANS_CHARACTER_COMMENT, ANS_CHARACTER_COMMENT);
        handlerMap.Add(GameEventType.ANS_CHARACTER_COMMENT_EDIT, ANS_CHARACTER_COMMENT);
        handlerMap.Add(GameEventType.ANS_CHARACTER_EVALUATE, ANS_CHARACTER_EVALUATE);
        handlerMap.Add(GameEventType.ANS_CHARACTER_EVALUATE_LIST, ANS_CHARACTER_EVALUATE_LIST);
        handlerMap.Add(GameEventType.ANS_CHARACTER_EVALUATE_MINE, ANS_CHARACTER_EVALUATE_MINE);

        base.RegisterHandler();
    }

    internal override void ClickBackButton()
    {
        // base.ClickBackButton();
        if(ui.bSkinPage)
            ui.OnClickBackFromSkinSelect();
        else
        {
            if(para.returnSys == SubSysType.EditTeam)
            {
                ReturnSysPara returnSysPara = new ReturnSysPara(SubSysType.Lobby, para.teamIdx, null);
                GameCore.Instance.ChangeSubSystem(SubSysType.EditTeam, returnSysPara);
            }
            else
            {
                GameCore.Instance.ChangeSubSystem(para.returnSys, null);
            }
        }
    }

    void CBClickHeroUp()
    {
        GameCore.Instance.ChangeSubSystem(SubSysType.HeroUp, para);
    }

    void CBClickItemUp()
    {
        //GameCore.Instance.ChangeSubSystem(SubSysType.ItemUp, para);
        GameCore.Instance.ChangeSubSystem(SubSysType.EquipItem, para);
    }


    bool ANS_CHARACTER_COMMENT(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var datas = GetEvaluateListByJson(para.GetField("EVAL_LIST"));
                if (datas != null && datas.Count != 0)
                {
                    ui.RateRoot.SetEvaluateMine(datas[0]);
                }
                else
                {
                    Debug.LogError("Invalid Data!");
                }

                float avg = 0f;
                var eval_sum = para.GetField("EVAL_SUMMARY");
                eval_sum[0].GetField(ref avg, "AVERAGE_GRADE");
                ui.RateRoot.SetTotalRate(avg);
                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            case 2: GameCore.Instance.ShowNotice("실패", "SCORE 값 정상 범위 초과", 0); break;
            case 3: GameCore.Instance.ShowNotice("실패", "COMMENT 140자 초과", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    bool ANS_CHARACTER_EVALUATE(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var datas = GetEvaluateListByJson(para.GetField("EVAL_LIST"));
                if (datas != null && datas.Count != 0)
                {
                    ui.RateRoot.UpdateEvaluate(datas[0]);
                }
                else
                {
                    Debug.LogError("Invalid Data!");
                }
                
                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    bool ANS_CHARACTER_EVALUATE_LIST(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var datas = GetEvaluateListByJson(para.GetField("EVAL_LIST"));
                ui.RateRoot.AddEvaluateList(datas);

                float avg = 0f;
                var eval_sum = para.GetField("EVAL_SUMMARY");
                eval_sum[0].GetField(ref avg, "AVERAGE_GRADE");
                ui.RateRoot.SetTotalRate(avg);

                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }

    bool ANS_CHARACTER_EVALUATE_MINE(ParaBase _para)
    {
        var para = _para.GetPara<PacketPara>().data.data;
        int code = -1;
        para.GetField(ref code, "result");

        switch (code)
        {
            case 0:
                var datas = GetEvaluateListByJson(para.GetField("EVAL_LIST"));
                if(datas != null && datas.Count != 0)
                {
                    ui.RateRoot.SetEvaluateMine(datas[0]);
                }
                else
                {
                    ui.RateRoot.SetEvaluateMine(null);
                }
                return true;

            case 1: GameCore.Instance.ShowNotice("실패", "쿼리 오류", 0); break;
            default: GameCore.Instance.ShowNotice("실패", "알 수 없는 에러 : " + code, 0); break;
        }

        return false;
    }


    List<HeroEvaluateSData> GetEvaluateListByJson(JSONObject _list)
    {
        if (_list.type == JSONObject.Type.ARRAY)
        {
            List<HeroEvaluateSData> datas = new List<HeroEvaluateSData>();
            for (int i = 0; i < _list.Count; ++i)
            {
                var data = new HeroEvaluateSData();
                data.SetData(_list[i]);
                datas.Add(data);
            }
            return datas;
        }

        return null;
    }

    public List<Action> GetTutorialActionList(int tutorialNum)
    {
        List<Action> nActionList = new List<Action>();
        switch (tutorialNum)
        {
            case 6:
                //nActionList.Add(() => { });
                break;
            default:
                break;
        }
        return nActionList;
    }
}
