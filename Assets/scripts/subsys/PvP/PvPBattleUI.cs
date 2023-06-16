using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 스토리모드 전투시스템 UI
/// </summary>
public class PvPBattleUI : BattleUIBase
{
    public static PvPBattleUI Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/PanelPvPBattleUI", _parent);
        return go.GetComponent<PvPBattleUI>();
    }

    PvPBattleInfo pvpBattleinfo;
    public override void OnClickExistBattle()
    {
        GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, "포기하기",
               new MsgAlertBtnData[] { new MsgAlertBtnData("취소", new EventDelegate(()=>{
                                        GameCore.Instance.CloseMsgWindow();
                                        ReturnSpeed();
                                        OnClickMenu();
                                    }), true, null, SFX.Sfx_UI_Cancel),
                                    new MsgAlertBtnData("확인", new EventDelegate(()=>{
                                        ReturnSpeed();
                                        PvPBattleSys pvpBattleSys = (PvPBattleSys) GameCore.Instance.SubsysMgr.GetNowSubSys();
                                        pvpBattleSys.BuildedRoom.Destroy();
                                        //GameCore.Instance.StopAllCoroutines();
                                        GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
                                        GameCore.Instance.CloseMsgWindow();
                                    })) },
               0, "전투는 실패 처리됩니다.\n 메인 화면으로 나가시겠습니까?"
              )));
    }
    protected override void InitInternal(PvPSData _oppenetData)
    {
        pvpBattleinfo = UnityCommonFunc.GetComponentByName<PvPBattleInfo>(gameObject, "pvpBattleInfo");
        pvpBattleinfo.gameObject.SetActive(false);
        pvpBattleinfo.SetInfo(_oppenetData);

        SetAutoButton(false);
        autoSprite.enabled = false;
        autoLockSprite.enabled = true;
        autoLabel.alpha = 0.6f;
        autoBt.GetComponent<UISprite>().enabled = false;

        //autoBt.SetActive(true);
        autoBt.GetComponent<Collider2D>().enabled = false;
        //if(strikeBt_Root != null) strikeBt_Root.SetActive(false);

        for (int i = 0; i < unitCards.Length; i++)
            unitCards[i].SetPvPMode(true);

        StartCoroutine(CoCamWorking());
    }


    IEnumerator CoCamWorking()
    {
        isAnimationing = true;

        // Set Cam Working
        var camTf = GameCore.Instance.GetWorldCam().transform;
        Vector3 sPos = new Vector3(0f, 16.14f, -19.21f);
        Vector3 ePos = new Vector3(0f, 11.8f, -19f);
        Quaternion sRot = Quaternion.Euler(new Vector3(32.5f/*25f*/, 0f, 0f));
        Quaternion eRot = Quaternion.Euler(new Vector3(32.5f, 0f, 0f));

        camTf.position = sPos;
        camTf.rotation = sRot;

        yield return new WaitForSeconds(0.5f);

        float time = 1f;
        float acc = Time.deltaTime;
        while (acc < time)
        {
            var v = acc / time;
            camTf.position = Vector3.Lerp(sPos, ePos, v);
            camTf.rotation = Quaternion.Lerp(sRot, eRot, v);

            yield return null;
            acc += Time.deltaTime;
        }

        camTf.position = ePos;
        camTf.rotation = eRot;

        // Set BattleInfo Viewing
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_PVP_VS);
        pvpBattleinfo.gameObject.SetActive(true);
        //var panel = pvpBattleinfo.GetPanel();
        //var region = panel.baseClipRegion;

        //time = 0.5f;
        //acc = Time.deltaTime;
        //while (acc < time)
        //{
        //    region.z = Mathf.Lerp(1, 1800, acc / time);
        //    panel.baseClipRegion = region;

        //    yield return null;
        //    acc += Time.deltaTime;
        //}

        yield return new WaitForSeconds(pvpBattleinfo.GetAnimatorLimitTime());

        //time = 0.5f;
        //acc = Time.deltaTime;
        //while (acc < time)
        //{
        //    region.z = Mathf.Lerp(1800, 1, acc / time);
        //    panel.baseClipRegion = region;

        //    yield return null;
        //    acc += Time.deltaTime;
        //}
        pvpBattleinfo.gameObject.SetActive(false);


        isAnimationing = false;
    }


    public override void OnClickCard(int _num)
    {
        // do nothing
    }
}
