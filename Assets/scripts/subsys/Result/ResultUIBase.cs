using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal abstract class ResultUIBase : MonoBehaviour, ISequenceTransform
{
    [Header("BOTTOM BUTTONS")]
    [SerializeField] protected GameObject goSelectBtn;
    [SerializeField] protected GameObject goRetryBtn;
    [SerializeField] protected GameObject goNextBtn;
    [SerializeField] protected GameObject goCheckOutBtn;
    [SerializeField] protected GameObject goLobbyBtn;

    public ResultBasePara para { get; protected set; }


    internal static ResultUIBase Create(Transform _parent, InGameType _type)
    {
        switch (_type)
        {
            case InGameType.Story:
            case InGameType.Daily:  return ResultUI.Create(_parent);
            case InGameType.PvP:    return ResultPvPUI.Create(_parent);
            case InGameType.Raid:
            default:                return ResultRaidUI.Create(_parent);
        }
    }
    internal abstract void TurnOffNextButton();
    internal virtual void Init(ResultBasePara _para)
    {
        para = _para;
    }

    // 데이터의 변경이 있을 시 표현 (UI 변경 및 애니메이션)
    internal abstract void UpdateData(params object[] _params);

    internal abstract void SetRewardItem(CardSData[] _sdatas);

    public List<ReturnTutorialData> GetTutorialTransformList(int tutorialNum)
    {
        List<ReturnTutorialData> nTutorialList = new List<ReturnTutorialData>();
        switch(tutorialNum)
        {
            case 1:
                nTutorialList.Add(new ReturnTutorialData(goLobbyBtn.transform,0));
                break;
            case 4:
                nTutorialList.Add(new ReturnTutorialData(goLobbyBtn.transform, 0));
                break;
            case 5:
                nTutorialList.Add(new ReturnTutorialData(goLobbyBtn.transform, 0));
                break;
            default:
                break;
        }
        return nTutorialList;
    }
}
