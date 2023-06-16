using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum AutonomyTutoType
{
    Daily,
    Raid,
    PvP,
    MyRoom,
    Manufact,
    Farming,
    Mission,
    Mail
}

public class AutonomyTutorial : MonoBehaviour, IEventHandler
{
    [SerializeField] GameObject goClose;
    [SerializeField] UI2DSprite spImage;

    [SerializeField] Sprite[] txDaily;   // 0
    [SerializeField] Sprite[] txRaid;    // 1
    [SerializeField] Sprite[] txPvP;     // 2
    [SerializeField] Sprite[] txMyRoom;  // 3
    [SerializeField] Sprite[] txManufact;// 4
    [SerializeField] Sprite[] txFarming; // 5
    [SerializeField] Sprite[] txMission; // 6
    [SerializeField] Sprite[] txMail;    // 7
    

    Sprite[][] txLists = new Sprite[8][];
    AutonomyTutoType type; // 현재 진행중인 튜토리얼 종류
    int idx; // 현재 진행중인 튜토리얼 인덱스
    int cnt; // 현재 진행중인 튜토리얼의 남은 인덱스 개수

    float showTime;

    public static bool IsRunning;

    public static AutonomyTutorial Create(Transform _parent)
    {
        return GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Tutorial/PanelAutonomyTutorial", _parent).GetComponent<AutonomyTutorial>();
    }

    private void Awake()
    {
        txLists[0] = txDaily;
        txLists[1] = txRaid;
        txLists[2] = txPvP;
        txLists[3] = txMyRoom;
        txLists[4] = txManufact;
        txLists[5] = txFarming;
        txLists[6] = txMission;
        txLists[7] = txMail;

        // ㅆ니전환시 게임 오브젝트를 삭제하기위해 이벤트 핸들러에 등록한다.
        GameCore.Instance.EventMgr.RegisterHandler(this, GameEventType.ChangeSys);
    }

    /// <summary>
    /// 씬 전환시 자율 튜토리얼 게임오브젝트를 삭제한다.
    /// </summary>
    /// <param name="_evt"></param>
    /// <returns></returns>
    public bool HandleMessage(GameEvent _evt)
    {
        if (IsRunning)
        {
            IsRunning = false;
            Destroy(gameObject);
        }
        return true;
    }

    /// <summary>
    /// 튜토리얼을 실행해야한다면 알아서 동작한다.
    /// 튜토리얼이 시작되는 곳에서 호출만 해주면 된다.
    /// </summary>
    /// <param name="_type"> 진행하고자하는 튜토리얼의 종류 </param>
    /// <param name="_idx"> 진행하고자하는 튜토리얼의 인덱스 </param>
    /// <param name="_cnt"> 연속으로 진행할 튜토리얼이미지 개수. 음수일 경우 전부 출력 </param>
    public static void AutoRunTutorial(AutonomyTutoType _type, int _idx, int _cnt = -1)
    {
        if (!CheckSubTutorial(_type, _idx))
        {
            var at = Create(GameCore.Instance.Ui_root);
            at.ShowSubTutorial(_type, _idx, _cnt);
        }
    }


    /// <summary>
    /// 자율 튜토리얼의 진행 여부를 반환
    /// </summary>
    /// <param name="_type"> 확인하고자하는 튜토리얼 넘버(자율만 0~7) </param>
    /// <param name="_idx"> 확인하고자하는 튜토리얼의 인덱스(0~N) </param>
    /// <returns> 완료 되었다면 true, 아직 진행하지 않았다면 false </returns>
    public static bool CheckSubTutorial(AutonomyTutoType _type, int _idx)
    {
        switch (_type)
        {
            case AutonomyTutoType.Daily:    return (GameCore.Instance.PlayerDataMgr.TutorialData.dungeon & (1 << _idx)) != 0;
            case AutonomyTutoType.Raid:     return (GameCore.Instance.PlayerDataMgr.TutorialData.raid    & (1 << _idx)) != 0;
            case AutonomyTutoType.PvP:      return (GameCore.Instance.PlayerDataMgr.TutorialData.pvp     & (1 << _idx)) != 0;
            case AutonomyTutoType.MyRoom:   return (GameCore.Instance.PlayerDataMgr.TutorialData.myRoom  & (1 << _idx)) != 0;
            case AutonomyTutoType.Manufact: return (GameCore.Instance.PlayerDataMgr.TutorialData.manufact& (1 << _idx)) != 0;
            case AutonomyTutoType.Farming:  return (GameCore.Instance.PlayerDataMgr.TutorialData.farming & (1 << _idx)) != 0;
            case AutonomyTutoType.Mission:  return (GameCore.Instance.PlayerDataMgr.TutorialData.mission & (1 << _idx)) != 0;
            case AutonomyTutoType.Mail:     return (GameCore.Instance.PlayerDataMgr.TutorialData.mail    & (1 << _idx)) != 0;
            default:                        return true;
        }
    }


    /// <summary>
    /// 자율튜토리얼 시작
    /// </summary>
    /// <param name="_type"> 진행하고자하는 튜토리얼종류 </param>
    /// <param name="_idx"> 시작하고자하는 튜토리얼 인덱스 </param>
    /// <param name="_cnt"> 연속으로 진행할 튜토리얼이미지 개수. 음수일 경우 전부 출력 </param>
    public void ShowSubTutorial(AutonomyTutoType _type, int _idx, int _cnt = -1)
    {
        if (_type < 0 || txLists.GetLength(0) <= (int)_type ||
            _idx < 0  || txLists[(int)_type].Length <= _idx ||
            (_cnt < 0 && txLists[(int)_type].Length <= _idx + _cnt-1))
        {
            Debug.LogError("Can ShowTutorial. [" + _type + "] " + _idx + " ~ " + (_idx + _cnt - 1));
            Destroy(gameObject.gameObject);
            return;
        }

        if (_cnt == -1)
            _cnt = txLists[(int)_type].Length;

        type = _type;
        idx = _idx;
        cnt = _cnt;

        ShowSprite(type, idx);

        GameCore.Instance.PlayerDataMgr.SetSubTutorial(_type, _idx); // 시작 인덱스 위치로 갱신
    }

    void ShowSprite(AutonomyTutoType _type, int _idx)
    {
        IsRunning = true;
        spImage.sprite2D = txLists[(int)_type][_idx];
        showTime = Time.realtimeSinceStartup;
    }

    public void OnClickTouch() // == OnclickNext
    {
        if (Time.realtimeSinceStartup < showTime + 2f)
            return;

        OnClickClose();
    }

    public void OnClickClose()// == OnclickNext
    {
        ++idx;
        --cnt;

        if (cnt == 0) // 튜토리얼 종료
        {
            IsRunning = false;
            Destroy(gameObject);
        }
        else // 다음 장 출력
        {
            ShowSprite(type, idx);
        }
    }

    private void OnDestroy()
    {
        GameCore.Instance.EventMgr.UnregisterHandler(this);
    }
}
