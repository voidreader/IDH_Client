using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MsgAlertWindow : MonoBehaviour
{
    // NOTE : Prefeb 내부에서 확인할 수 없는 데이터 : EnvelopContent
    // TODO : 존재이유 확인해야함 2020-02-24
    [SerializeField] EnvelopContent _resizer;

    [SerializeField] UITable _contentRoot;
    [SerializeField] UITable _contentInner;
    [SerializeField] UILabel _title;
    [SerializeField] UIGrid _btnRoot;
    [SerializeField] GameObject _goEff;
    [SerializeField] Animator _popupAnimator;

    // NOTE : 사용되지 않는 변수
    UILabel lbMsg;

    

    bool bInit;
    public bool canBack;

    private void Awake()
    {
        bInit = false;
        _resizer = GetComponent<EnvelopContent>();

        #region [5Star]에서 외부로부터 데이터를 받아오던 형식 : 현재는 변경된 사항
        // [NOTE] : 변수명 리팩토링 중 외부에서 어떠한 데이터를 참조해야하는지 모를때 참고할 것.
        //_contentRoot = UnityCommonFunc.GetComponentByName<UITable>(gameObject, "Contents_root");
        //_contentInner = UnityCommonFunc.GetComponentByName<UITable>(gameObject, "Contents");
        //_title = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "Title");
        //_btnRoot = UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "Buttons");
        //_goEff = UnityCommonFunc.GetGameObjectByName(gameObject, "Container");
        //_popupAnimator = UnityCommonFunc.GetComponentByName<Animator>(gameObject, "ConfirmWindow");
        #endregion

        RemoveContents();
        RemoveButtons();
        gameObject.SetActive(false);
    }

    internal void ShowMsgAlert(MsgAlertPara _para)
    {
        gameObject.SetActive(true);
        canBack = _para.isCanBack;

        if (GameCore.Instance.lobbyTutorial != null && GameCore.Instance.lobbyTutorial.IsRunning)
            _popupAnimator.enabled = false;

        // Set Order
        if (!canBack)
        {
            gameObject.GetComponent<UIPanel>().sortingOrder = 200;
            gameObject.GetComponent<UIPanel>().depth = 10000;
        }

        // Set Title
        _title.text = _para.title;

        // Set Msg
        if (_para.message != null && _para.message != "")
        {
            var msg = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Common/Msg", _contentInner.transform);
            msg.GetComponent<UILabel>().text = "\n" + _para.message;
            msg.GetComponent<UILabel>().fontSize = _para.fontSize;
        }
        if (_para.message2 != null && _para.message2 != "")
        {
            var msg = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Common/Msg", _title.transform);
            msg.GetComponent<UILabel>().text = _para.message2;
            msg.GetComponent<UILabel>().fontSize = _para.fontSize2;
        }
        // Set Contents
        if (_para.obj != null)
        {
            _para.obj.transform.parent = _contentInner.transform;
            _para.obj.transform.localPosition = Vector3.zero;
            _para.obj.SetActive(true);
        }

        // Set Highlight Msg
        if (_para.highlight != null && _para.highlight != "")
        {
            // Add Padding
            if (_para.size < 0 || 4 < _para.size)
            {
                var pad = new GameObject("pad");
                pad.transform.parent = _contentInner.transform;
                pad.transform.localScale = Vector3.one;
                pad.AddComponent<UIWidget>().height = 30;
            }
            var highLightText = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Common/HL", _contentInner.transform);
            highLightText.transform.GetChild(0).GetComponent<UILabel>().text = _para.highlight;
            switch(_para.moneyType)
            {
                case MoneyType.Gold:
                    highLightText.transform.GetChild(1).GetComponent<UISprite>().spriteName = "ICON_MONEY_02";
                    break;
                case MoneyType.Pearl:
                    highLightText.transform.GetChild(1).GetComponent<UISprite>().spriteName = "ICON_MONEY_03";
                    break;
                case MoneyType.FriendshipPoint:
                    highLightText.transform.GetChild(1).GetComponent<UISprite>().spriteName = "ICON_MONEY_04";
                    break;
                case MoneyType.HeroTicket:
                    highLightText.transform.GetChild(1).GetComponent<UISprite>().spriteName = "TICKET_01";
                    break;
                case MoneyType.ItemTicket:
                    highLightText.transform.GetChild(1).GetComponent<UISprite>().spriteName = "TICKET_02";
                    break;
                case MoneyType.Cash:
                    highLightText.transform.GetChild(1).GetComponent<UISprite>().spriteName = "W_75";
                    break;
                case MoneyType.None:
                    highLightText.transform.GetChild(1).GetComponent<UISprite>().spriteName = "ICON_MONEY_02";
                    break;
            }
            highLightText.transform.GetChild(1).GetComponent<UISprite>().MakePixelPerfect();
        }

        // Set Button
        if (_para.size == 0)
        {
            if (3 <= _para.btns.Length) _btnRoot.cellWidth = 140;
            else _btnRoot.cellWidth = 280;
        }
        for (int i = 0; i < _para.btns.Length; ++i)
        {
            if (_para.isVertical == false)
                AddButton(_para.btns[i].text, _para.btns[i].ed, _para.btns[i].enable, _para.size <= 0, _para.btns.Length >= 3, _para.btns[i].btnName, _para.btns[i].clickSound);
            else
                AddButton(_para.btns[i].text, _para.btns[i].ed, _para.btns[i].enable, false, false, _para.btns[i].btnName, _para.btns[i].clickSound);
        }

        if (UIInput.selection != null)
            UIInput.selection.isSelected = false;

        Vector2 size;
        switch (_para.size)
        {
            case 0: size = new Vector2(656, 350); break;    // 최소
            case 1: size = new Vector2(656, 510); break;    // 소
            case 2: size = new Vector2(936, 510); break;    // 중
            case 3: size = new Vector2(1200, 650); break;   // 대
            case 4: size = new Vector2(1280, 650); break;   // Not Define	// 최대

            default:
                StartCoroutine(CoResize());
                return;
        }

        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Popup);

        // Sorting
        var window = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "ConfirmWindow");
        window.width = (int)size.x;
        window.height = (int)size.y;
        if (_para.size == 4)
        {
            window.enabled = false;
            _goEff.SetActive(false);
        }

        _title.transform.localPosition = new Vector3(0, -34);
        _contentInner.transform.localPosition = new Vector3(0, -68);
        _btnRoot.transform.localPosition = new Vector3(0, -size.y + 60);

        var contentSize = new Vector2(size.x - 60, size.y - 68 - 60 - ((_para.size > 0) ? 34 : 24));
        var contentCnt = _contentInner.transform.childCount;
        float totlaContentHeight = 0;
        List<Bounds> contentHeights = new List<Bounds>();
        for (int i = 0; i < contentCnt; ++i)
        {
            Bounds b = NGUIMath.CalculateRelativeWidgetBounds(transform.parent, _contentInner.transform.GetChild(i), false);
            contentHeights.Add(b);
            totlaContentHeight += b.max.y - b.min.y;
        }
        var interval = (contentSize.y - totlaContentHeight) / (contentCnt + 1);
        float heightAcc = 0;
        for (int i = 0; i < contentCnt; ++i)
        {
            var height = contentHeights[i].max.y - contentHeights[i].min.y;

            var centerPosY = heightAcc + height * 0.5f + interval;
            heightAcc += height + interval;
            _contentInner.transform.GetChild(i).localPosition = new Vector3(0, -centerPosY);
        }

        //		Debug.Log("contentSize:" + contentSize);
        //		Debug.Log("totlaContentHeight:" + totlaContentHeight);
        //		Debug.Log("interval:" + interval);
        if (_para.isVertical == true)
        {
            _btnRoot.arrangement = UIGrid.Arrangement.Vertical;
            _btnRoot.cellHeight = 80f;
            _btnRoot.enabled = true;
            int parentHeight = _contentRoot.transform.parent.GetComponent<UISprite>().height;
            int BtnHeight = _btnRoot.transform.GetChild(0).GetComponent<UISprite>().height;
            int rootPos = (parentHeight - BtnHeight) / 2 + BtnHeight;
            rootPos *= -1;
            rootPos += (int)_btnRoot.cellHeight / 2 * (_btnRoot.transform.childCount - 1);
            _btnRoot.transform.localPosition = new Vector3(0, rootPos, 0);
        }

        if (GameCore.Instance.SubsysMgr.GetNowSysType() != GameCore.Instance.StartSystem)
            GameCore.Instance.SetDynamicTutorial(GetDynamicReturnTutorialDataList);
    }

    internal List<ReturnTutorialData> GetDynamicReturnTutorialDataList(int nTutorialPos)
    {
        List<ReturnTutorialData> returnTutorialList = new List<ReturnTutorialData>();
        switch (nTutorialPos)
        {
            case 3:
                Transform obj = _contentInner.transform.GetChild(0);
                returnTutorialList.Add(new ReturnTutorialData(obj.GetChild(0).GetChild(0).GetChild(0).GetComponent<TeamSkillListItem>().ButtonTransform, 0));
                break;
            case 6:
                //returnTutorialList.Add(new ReturnTutorialData(btnRoot.GetChild(1), 0));
                //break;
            case 7:
                if(GameCore.Instance.SubsysMgr.GetNowSysType() == SubSysType.Inven)
                {
                    _btnRoot.enabled = false;
                    returnTutorialList.Add(new ReturnTutorialData(_btnRoot.GetChild(1).transform, 0));
                    if (_btnRoot.transform.GetChild(0) != null) _btnRoot.transform.GetChild(0).localPosition = Vector3.right * 150f;
                    //if (btnRoot.transform.GetChild(2) != null) btnRoot.transform.GetChild(1).localPosition = Vector3.up * -62.5f;
                    if (_btnRoot.transform.GetChild(1) != null) _btnRoot.transform.GetChild(2).localPosition = Vector3.right * -150f;
                }
                break;
        }
        return returnTutorialList;
    }

    internal void ShowMsgAlert()
    {
        gameObject.SetActive(true);
    }

    internal void HideMsgAlert()
    {
        //RemoveContents();
        //RemoveButtons();
        gameObject.SetActive(false);
    }

    internal IEnumerator CoResize()
    {
        if (_contentInner != null) _contentInner.Reposition();
        if (_contentRoot != null) _contentRoot.Reposition();

        yield return new WaitForEndOfFrame();

        if (_resizer != null) _resizer.Execute();

        var anc = _contentRoot.GetComponent<UIAnchor>();
        if (anc != null) anc.Update();

        transform.localPosition = new Vector3(0, 0, -50);
    }

    internal void RemoveContents()
    {
        while (_contentInner.transform.childCount != 0)
            Destroy(_contentInner.transform.GetChild(0));
    }

    internal void RemoveButtons()
    {
        while (_btnRoot.transform.childCount != 0)
            Destroy(_btnRoot.GetChild(0));
    }

    internal void AddButton(string _text, EventDelegate _evtDlgt, bool _enable, bool _small, bool _small2, string _btnName, SFX _clickSound)
    {
        UIButton btn = null;
        if (_small && _small2) btn = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Common/AlertButton_ss", _btnRoot.transform).GetComponent<UIButton>();
        else if (_small || _small2) btn = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Common/AlertButton_s", _btnRoot.transform).GetComponent<UIButton>();
        else btn = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Common/AlertButton", _btnRoot.transform).GetComponent<UIButton>();
        btn.state = !_enable ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal;
        btn.GetComponent<Collider2D>().enabled = _enable;
        //if (_color.a == 0)
        //    _color = new Color(126f / 255f, 0f, 1f);
        //btn.defaultColor = _color;
        //btn.disabledColor = _color;
        //btn.hover = _color;
        //btn.pressed = _color;
        if (_btnName == null)
            _btnName = "BTN_01_01_{0:d2}";
        btn.normalSprite = string.Format(_btnName, 1);
        btn.hoverSprite = string.Format(_btnName, 2);
        btn.pressedSprite = string.Format(_btnName, 2);
        btn.disabledSprite = string.Format(_btnName, 1);
        

        btn.GetComponentInChildren<UILabel>().text = _text;
        /*btn.onClick.Add(new EventDelegate(() =>
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button)
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button)
        ));*/
        btn.GetComponent<ButtonRapper>().ClickSound = _clickSound;
        btn.onClick.Add(_evtDlgt);
    }
}
