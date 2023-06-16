using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TabComponent : MonoBehaviour
{
    [SerializeField] UIGrid grid;
    [SerializeField] List<UIButton> tabButtons;

    public int SelectIdx { get; protected set; }

    Func<int, int, bool> cbChangeTab; // cbChangeTab(prevTabIdx, newTabIdx);

    private void Awake()
    {
        for (int i = 0; i < tabButtons.Count; ++i)
        {
            var n = i;
            tabButtons[i].onClick.Add(new EventDelegate(() =>
            {
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
                //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
                OnClickTabButton(n);
            }));
        }
    }

    public void Init(Func<int, int, bool> _cbChangeTab)
    {
        cbChangeTab = _cbChangeTab;
    }

    public void AddTab(UIButton _btTab)
    {
        var n = tabButtons.Count;
        _btTab.onClick.Add(new EventDelegate(() =>
        {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            OnClickTabButton(n);
        }));

        tabButtons.Add(_btTab);

        EnableGrid();
    }

    public void EnableGrid()
    {
        grid.enabled = true;
    }

    public GameObject GetTabGameObject(int _index)
    {
        if (_index < 0 || tabButtons.Count <= _index)
        {
            Debug.LogError("Invalid Index." + _index);
            return null;
        }

        return tabButtons[_index].gameObject;
    }

    public  virtual void OnClickTabButton(int _index)
    {
        //if (SelectIdx == _index)
        //    return;

        if(_index < 0 || tabButtons.Count <= _index)
        {
            Debug.LogError("Invalid Index." + _index);
            return;
        }

        tabButtons[SelectIdx].GetComponent<UISprite>().spriteName = "BTN_06_01_01";
        tabButtons[SelectIdx].transform.localScale = new Vector3(1f, 1f);
        GameObject tabEffectOff = tabButtons[SelectIdx].transform.GetChild(1).gameObject;
        if (tabEffectOff != null) tabEffectOff.SetActive(false);

        tabButtons[_index].GetComponent<UISprite>().spriteName = "BTN_06_01_02";
        tabButtons[_index].transform.localScale = new Vector3(1.1f, 1.1f);
        GameObject tabEffectOn = tabButtons[_index].transform.GetChild(1).gameObject;
        if (tabEffectOn != null) tabEffectOn.SetActive(true);

        if (cbChangeTab!= null)
            if (cbChangeTab(SelectIdx, _index))
                SelectIdx = _index;
    }
}
