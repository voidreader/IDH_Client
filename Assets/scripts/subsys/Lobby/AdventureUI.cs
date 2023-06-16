using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdventureUI : MonoBehaviour
{
    public static AdventureUI Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(CSTR.RSC_AdventureUI, _parent);
        var result = go.GetComponent<AdventureUI>();
        result.Init();
        return result;
    }


    [SerializeField] UILabel lbDailyChance;


    public void Init()
    {
        lbDailyChance.text = string.Format(CSTR.DailyChallengeCount, GameCore.Instance.PlayerDataMgr.GetReousrceCount(ResourceType.Ticket_daily));
    }

    public void OnClickDailyDungeon()
    {
        GameCore.Instance.CloseMsgWindow();
        GameCore.Instance.ChangeSubSystem(SubSysType.DailyPrepare, null);
    }

    public void OnClickRaid()
    {
        GameCore.Instance.CloseMsgWindow();
        GameCore.Instance.ChangeSubSystem(SubSysType.RaidPrepare, null);
    }

}
