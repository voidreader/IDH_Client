using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateAccountScript : MonoBehaviour
{
    public static CreateAccountScript Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(CSTR.RSC_CreateAccount, _parent);
        return go.GetComponent<CreateAccountScript>();
    }



    [SerializeField] UIInput ipName;
    [SerializeField] UILabel lbResponse;
    [SerializeField] UILabel lbName;


    NCommon.LoginType accountType;
    Action<NCommon.LoginType, string> cbStart;


    private void Awake()
    {
        lbName.multiLine = false;
        ipName.onReturnKey = UIInput.OnReturnKey.Default;
    }

    internal void Init(NCommon.LoginType _accountType, Action<NCommon.LoginType, string> _cbStart)
    {
        accountType = _accountType;
        cbStart = _cbStart;

        OnClickNameInput();
    }

    public void SetResponse(string _text)
    {
        lbResponse.text = _text;
    }

    public void OnClickNameInput()
    {
        ipName.enabled = true;
        ipName.isSelected = true;
    }

    public void OnChangeNameInput()
    {
        SetResponse(string.Empty);
    }

    public void OnSubmitNameInput()
    {
        cbStart(accountType, ipName.value);
    }
   
    public string GetSubmitName()
    {
        return ipName.value;
    }
}
