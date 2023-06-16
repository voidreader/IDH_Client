using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditWindow : MonoBehaviour
{
    [SerializeField] float scrollSpeed = 0.001f;
    [SerializeField] UIScrollView sv;
    [SerializeField] UILabel lbCredit;


    [SerializeField] float acc = 0f;
    [SerializeField] float time = 14f;


    public string Spacing = "　　　　";
    public string CorpColor = "[00F0FF]";
    public string HeadColor = "[F600FF]";
    public string NormColor = "[898989]";
    string NameColor = "[FFFFFF]";

    BGMType prevBGMType;

    public static CreditWindow Create()
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("common/CreditWindow", GameCore.Instance.Ui_root);
        var result = go.GetComponent<CreditWindow>();

        result.lbCredit.text = string.Format(result.lbCredit.text, result.Spacing, result.CorpColor, result.HeadColor, result.NormColor);

        result.prevBGMType = GameCore.Instance.SoundMgr.GetNowBGMType();
        GameCore.Instance.SoundMgr.SetBGMSound(BGMType.BMSND_Credit, true, false);

        return result;
    }


    void Update ()
    {
        acc += Time.deltaTime * scrollSpeed;
        sv.Scroll(-Time.deltaTime * scrollSpeed);
        if (acc > time)//lbCredit.height + 600)
        {
            OnClickClose();
        }
    }

    public void OnClickClose()
    {
        GameCore.Instance.SoundMgr.SetBGMSound(prevBGMType, true, false);
        GameCore.Instance.CloseMsgWindow();
    }
}
