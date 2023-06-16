using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroInfoUI : MonoBehaviour, ISequenceTransform
{
    internal static HeroInfoUI Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("HeroInfo/HeroInfoUI", _parent);
        return go.GetComponent<HeroInfoUI>();
    }

    [SerializeField] HeroInfoUIAnimation uiAnim;
    [SerializeField] TabComponent tab;

    [Header("Info")]
    [SerializeField] UI2DSprite spIllust;
    [SerializeField] CardInfoLabel cardInfo;
    [SerializeField] UILabel lbPower;

    [Header("Contents")]
    [SerializeField] UILabel lbHead;

    [SerializeField] HeroInfoStateComponent stateRoot;
    [SerializeField] HeroInfoAbilityComponent abilityRoot;
    [SerializeField] HeroInfoProfileComponent profileRoot;
    [SerializeField] HeroInfoSkillComponenet skillRoot;
    [SerializeField] HeroInfoRateComponent rateRoot;
    [SerializeField] HeroInfoSkinComponent skinRoot;

    [Header("Buttons")]
    [SerializeField] UIButton itemUpBtn;
    [SerializeField] UIButton heroUpBtn;
    [SerializeField] GameObject heroUpBtnHighlight;

    public HeroInfoRateComponent RateRoot { get { return rateRoot; } }

    public bool bSkinPage { get; private set; }
    int cachedIllustKey;
    int illustKey;

    Action cbClickHeroUp;
    Action cbClickItemUp;


    internal void Init(HeroSData _data, Action _cbClickHeroUp, Action _cbClickItemUp)
    {
        cbClickHeroUp = _cbClickHeroUp;
        cbClickItemUp = _cbClickItemUp;

        var unit = GameCore.Instance.PlayerDataMgr.GetUnitData(_data.uid);
        cachedIllustKey = illustKey = unit.GetIllustSpeiteKey();
        GameCore.Instance.SetUISprite(spIllust, illustKey);
        lbPower.text = string.Format("[c]전투력:[/c] {0:N0}", _data.GetPower());

        cardInfo.Init(_data);

        stateRoot.Init(_data, cbClickItemUp);
        abilityRoot.Init(_data);
        profileRoot.Init(_data);
        skillRoot.Init(_data);
        rateRoot.Init(_data);
        
        {
            var data = new HeroInfoSkinListItem.Data[]{
                new HeroInfoSkinListItem.Data(1, 1000001, 25, 1100025, HeroInfoSkinListItem.State.Equipped, 3000001, 10000, "랄랄라", ItemEffectType.Attack, 5, ItemEffectType.Agility, 12),
                new HeroInfoSkinListItem.Data(1, 1000031, 11, 1100011, HeroInfoSkinListItem.State.NotBuy, 3000001, 10000, "랄랄라", ItemEffectType.Attack, 5, ItemEffectType.Agility, 12),
                new HeroInfoSkinListItem.Data(1, 1000031, 5, 1100005, HeroInfoSkinListItem.State.NotBuy, 3000001, 10000, "랄랄라", ItemEffectType.Attack, 5, ItemEffectType.Agility, 12),
            };
            skinRoot.Init(data, CBChangeIllust);
        }
        tab.Init(CBChangeTab);
        tab.OnClickTabButton(0);

        heroUpBtnHighlight.SetActive(HeroCardBig.IsEnchantAlert(_data.uid));
    }


    bool CBChangeTab(int _oldIdx, int _newIdx)
    {
        if (bSkinPage)
            return false;

        SetActiveContent(_oldIdx, false);
        SetActiveContent(_newIdx, true);

        SetHead(_newIdx);

        return true;
    }

    void SetActiveContent(int _idx, bool _active)
    {
        switch (_idx)
        {
            case 0: stateRoot.gameObject.SetActive(_active); return;
            case 1: abilityRoot.gameObject.SetActive(_active); return;
            case 2: profileRoot.gameObject.SetActive(_active); return;
            case 3: skillRoot.gameObject.SetActive(_active); return;
            case 4: rateRoot.gameObject.SetActive(_active); return;
            case 5: skinRoot.gameObject.SetActive(_active); return;
            default: Debug.LogError("INVALID INDEX. " + _idx); return;
        }
    }

    void SetHead(int _idx)
    {
        switch (_idx)
        {
            case 0: lbHead.text = "기본 정보"; return;
            case 1: lbHead.text = "능력치"; return;
            case 2: lbHead.text = "프로필"; return;
            case 3: lbHead.text = ""; return;
            case 4: lbHead.text = ""; return;
            case 5: lbHead.text = "스킨 선택"; return;
            default: lbHead.text = "INVALID INDEX" + _idx; return;
        }
    }

    void CBChangeIllust(int _illustKey)
    {
        if (_illustKey == illustKey || _illustKey <= 0)
            illustKey = cachedIllustKey;
        else
            illustKey = _illustKey;
        GameCore.Instance.SetUISprite(spIllust, illustKey);
    }


    public void OnClickSkinButton()
    {
        bSkinPage = true;

        uiAnim.PlayForward();
        SetActiveContent(tab.SelectIdx, false);
        SetActiveContent(5, true);

        SetHead(5);
    }

    public void OnClickBackFromSkinSelect()
    {
        bSkinPage = false;

        uiAnim.PlayerReverse();
        SetActiveContent(5, false);
        SetActiveContent(tab.SelectIdx, true);

        SetHead(tab.SelectIdx);
    }


    public void OnClickHeroUp()
    {
        cbClickHeroUp();
    }

    public void OnClickItemUp()
    {
        cbClickItemUp();
    }

    public List<ReturnTutorialData> GetTutorialTransformList(int tutorialNum)
    {
        List<ReturnTutorialData> nTutorialList = new List<ReturnTutorialData>();
        switch (tutorialNum)
        {
            case 6:
                nTutorialList.Add(new ReturnTutorialData(itemUpBtn.transform, 0));
                break;
            case 7:
                nTutorialList.Add(new ReturnTutorialData(heroUpBtn.transform, 0));
                break;
            default:
                break;
        }
        return nTutorialList;
    }
}
