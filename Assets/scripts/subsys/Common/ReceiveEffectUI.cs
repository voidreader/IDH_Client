using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ReceiveEffectUI : MonoBehaviour
{
    GameObject receiveItemListRoot;
    UIGrid receiveItemGrid;

    UIButton closeButton;
    UILabel closeLabel;


    GameObject receiveItemEffectRoot;
    GameObject receiveHeroEffectRoot;
    //Item Variable
    UISprite spIllust_Item;
    UISprite spHeroRank;
    UISprite spHeroType;
    UILabel lbHeroType;
    UILabel lbHeroInfo;
    UILabel lbHeroName;
    UIGrid heroStarRoot;
    GameObject heroResultUI;
    UISprite Item_BG_1;
    UISprite Item_BG_2;

    //Hero Variable
    UI2DSprite spIllust_Char;
    UISprite spItemRank;
    UISprite spItemType;
    UILabel lbItemType;
    UILabel lbItemInfo;
    UILabel lbItemName;
    UIGrid itemStarRoot;
    GameObject itemResultUI;

    private List<GameObject> cardList;

    [Header("HeroVeriable")]
    [SerializeField] GameObject heroClassType;
    [SerializeField] Animator heroEffectAnimator;
    [SerializeField] UI2DSprite spThirdIllust_Char;
    [SerializeField] GameObject uniqueSSSParticle;
    [SerializeField] GameObject uniqueSSParticle;
    [SerializeField] GameObject normalParticle;

    [Header("ItemVeriable")]
    [SerializeField] Animator itemEffectAnimator;
    [SerializeField] GameObject FirstUniqueEffect;
    [SerializeField] GameObject SecondUniqueEffect;
    [SerializeField] GameObject FirstSSEffect;
    [SerializeField] GameObject SecondSSEffect;
    [SerializeField] GameObject FirstNormalEffect;
    [SerializeField] GameObject SecondNormalEffect;
    [SerializeField] GameObject ItemClassType;


    CardSData[] datas;
    int nowIdx;
    bool isFirstAnimation = false;

    public bool nowGachaPlaying = false;


    List<CardBase> cardlist = new List<CardBase>();

    GameObject[] HeroCardListSlotAni = new GameObject[10];
    GameObject[] ItemCardListSLotAni = new GameObject[10];

    Action cbClose;

    private void Awake()
    {

        receiveItemListRoot = UnityCommonFunc.GetGameObjectByName(gameObject, "receiveItemLastRoot");
        receiveItemGrid = UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "grid");
        closeButton = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "button");
        closeButton.onClick.Clear();
        closeButton.onClick.Add(new EventDelegate(() =>
        {
            GameCore.Instance.SoundMgr.RestartBGM();
            GameCore.Instance.TurnOnTutorial(() => { });
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            OnClickClose();
        }));

        closeLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbGuide");

        closeButton.gameObject.SetActive(false);
        closeLabel.gameObject.SetActive(false);

        //ItemEffect Awake
        receiveItemEffectRoot = UnityCommonFunc.GetGameObjectByName(gameObject, "receiveItemEffectRoot");
        UnityCommonFunc.GetComponentByName<UIButton>(receiveItemEffectRoot, "BG").onClick.Add(new EventDelegate(OnClickReceiveEffect));
        UnityCommonFunc.GetComponentByName<UIButton>(receiveItemEffectRoot, "skip").onClick.Add(new EventDelegate(OnClickReceiveSkip));
        spIllust_Item = UnityCommonFunc.GetComponentByName<UISprite>(receiveItemEffectRoot, "Item_Icon");
        spItemRank = UnityCommonFunc.GetComponentByName<UISprite>(receiveItemEffectRoot, "spRank");
        spItemType = UnityCommonFunc.GetComponentByName<UISprite>(receiveItemEffectRoot, "spType");
        lbItemType = UnityCommonFunc.GetComponentByName<UILabel>(receiveItemEffectRoot, "lbType");
        lbItemInfo = UnityCommonFunc.GetComponentByName<UILabel>(receiveItemEffectRoot, "lbInfo");
        lbItemName = UnityCommonFunc.GetComponentByName<UILabel>(receiveItemEffectRoot, "lbName");
        itemStarRoot = UnityCommonFunc.GetComponentByName<UIGrid>(receiveItemEffectRoot, "starRoot");
        itemResultUI = UnityCommonFunc.GetGameObjectByName(receiveItemEffectRoot, "Result_UI");
        Item_BG_1 = UnityCommonFunc.GetComponentByName<UISprite>(ItemClassType.transform.parent.gameObject, "BG_001_1");
        Item_BG_2 = UnityCommonFunc.GetComponentByName<UISprite>(ItemClassType.transform.parent.gameObject, "BG_001_2");

        //HeroEffect Awake
        receiveHeroEffectRoot = UnityCommonFunc.GetGameObjectByName(gameObject, "receiveHeroEffectRoot");
        UnityCommonFunc.GetComponentByName<UIButton>(receiveHeroEffectRoot, "BG").onClick.Add(new EventDelegate(OnClickReceiveEffect));
        UnityCommonFunc.GetComponentByName<UIButton>(receiveHeroEffectRoot, "skip").onClick.Add(new EventDelegate(OnClickReceiveSkip));
        spIllust_Char = UnityCommonFunc.GetComponentByName<UI2DSprite>(receiveHeroEffectRoot, "spCharIllust");
        spHeroRank = UnityCommonFunc.GetComponentByName<UISprite>(receiveHeroEffectRoot, "spRank");
        spHeroType = UnityCommonFunc.GetComponentByName<UISprite>(receiveHeroEffectRoot, "spType");
        lbHeroType = UnityCommonFunc.GetComponentByName<UILabel>(receiveHeroEffectRoot, "lbType");
        lbHeroInfo = UnityCommonFunc.GetComponentByName<UILabel>(receiveHeroEffectRoot, "lbInfo");
        lbHeroName = UnityCommonFunc.GetComponentByName<UILabel>(receiveHeroEffectRoot, "lbName");
        heroStarRoot = UnityCommonFunc.GetComponentByName<UIGrid>(receiveHeroEffectRoot, "starRoot");
        heroResultUI = UnityCommonFunc.GetGameObjectByName(receiveHeroEffectRoot, "Result_UI");

        receiveItemListRoot.SetActive(false);
        receiveItemEffectRoot.SetActive(false);
        receiveHeroEffectRoot.SetActive(false);
        gameObject.SetActive(false);

    }

    internal void ShowReceiveItem(CardSData[] _data, Action _cbClose)
    {
        datas = _data;
        cbClose = _cbClose;
        for (int i = 0; i < datas.Length; ++i)
        {
            //if (datas[i].type == CardType.Character && GameCore.Instance.DataMgr.GetUnitData(datas[i].key).IsExpCard())
            //    Debug.LogError("있다");

            CardDataMap data;
            if (datas[i].type == CardType.Character)
                data = GameCore.Instance.DataMgr.GetUnitData(datas[i].key);
            else
                data = GameCore.Instance.DataMgr.GetItemData(datas[i].key);
            if (datas[i].type == CardType.Character)
            {
                // 여기에다가 데이터의 갯수에 맞게 이팩트를 위한 오브젝트를 생성하고 생성한 오브젝트에다가 바로바로 카드를 넣어주고서 그 목록을 리스트에 담아보자.
                HeroCardListSlotAni[i] = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Gacha/Hero_Eff_Card_Open", receiveItemGrid.transform);
                HeroCardListSlotAni[i].transform.GetChild(2).gameObject.SetActive(false);
                HeroCardListSlotAni[i].transform.GetChild(3).gameObject.SetActive(false);

                //생성될 장소는 오브젝트 안에다가 생성해주고.
                var card = CardBase.CreateCard(datas[i], data, true, UnityCommonFunc.GetGameObjectByName(HeroCardListSlotAni[i], "Card_Slot").transform);
                card.SetCallback(null, (_id) => GameCore.Instance.ShowCardInfo(card.SData));
                HeroCardListSlotAni[i].gameObject.SetActive(false);
            }
            else
            {
                ItemCardListSLotAni[i] = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Gacha/Item_Eff_Card_Open", receiveItemGrid.transform);
                ItemCardListSLotAni[i].transform.GetChild(2).gameObject.SetActive(false);
                ItemCardListSLotAni[i].transform.GetChild(3).gameObject.SetActive(false);

                var card = CardBase.CreateCard(datas[i], data, true, UnityCommonFunc.GetGameObjectByName(ItemCardListSLotAni[i], "Card_Slot").transform);
                card.SetCallback(null, (_id) => GameCore.Instance.ShowCardInfo(card.SData));
                ItemCardListSLotAni[i].gameObject.SetActive(false);
            }
        }

        ResetShowIndex();
        ShowNext();
        //if (PlayerPrefs.GetInt(CommonType.PFS_SKIP_GACHAANIM, 1) == 0)
        //{
        //    ShowLastPage(datas.Length == 1);
        //}
        //else
        //{
        //    ShowEffetByIndex(0);
        //}

        gameObject.SetActive(true);

        receiveItemGrid.enabled = true;
        nowGachaPlaying = true;
    }


    public void ResetShowIndex()
    {
        nowIdx = -1;
    }

    public void ShowNext()
    {
        if (PlayerPrefs.GetInt(CommonType.PFS_SKIP_GACHAANIM, 1) == 0)
        {
            //for (++nowIdx; nowIdx < datas.Length; ++nowIdx)
            //{
            //    var rank = 5;
            //    if (CardDataMap.IsItemKey(datas[nowIdx].key))
            //        rank = GameCore.Instance.DataMgr.GetItemData(datas[nowIdx].key).rank;   // 아이템
            //    else
            //        rank = GameCore.Instance.DataMgr.GetUnitData(datas[nowIdx].key).rank;   // 영웅

            //    if (rank == 0)
            //    {
            //        ShowEffetByIndex(nowIdx);
            //        return;
            //    }
            //}
        }
        else if (++nowIdx < datas.Length)
        {
            ShowEffetByIndex(nowIdx);
            return;
        }

        ShowLastPage(datas.Length == 1);
    }


    private void OnClickClose()
    {
        for (int i = 0; i < receiveItemGrid.transform.childCount; ++i)
            Destroy(receiveItemGrid.transform.GetChild(i).gameObject);

        datas = null;

        closeButton.gameObject.SetActive(false);
        closeLabel.gameObject.SetActive(false);

        gameObject.SetActive(false);

        if (cbClose != null)
        {
            cbClose.Invoke();
            cbClose = null;
        }
    }

    private void OnClickReceiveEffect()
    {
        if (heroEffectAnimator.gameObject.activeSelf == true)
        {
            if (heroEffectAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "HeroNextStep")
            {
                heroEffectAnimator.SetTrigger("OnClick");
                isFirstAnimation = true;
                return;
            }

        }

        if (itemEffectAnimator.gameObject.activeSelf == true)
        {
            if (itemEffectAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "IDH_Gear_M")
            {
                return;
            }
            if (itemEffectAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "ItemLastAnimation")
            {
                itemEffectAnimator.SetTrigger("OnClickEnd");
                isFirstAnimation = true;
                return;
            }
        }
    }

    private void Update()
    {
        if (heroEffectAnimator.gameObject.activeSelf == true)
        {
            if (isFirstAnimation && heroEffectAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "HeroFirstAnimation")
            {
                ShowNext();
                //ShowEffetByIndex(nowIdx + 1);
                isFirstAnimation = false;
            }
        }
        else if (itemEffectAnimator.gameObject.activeSelf == true)
        {
            if (isFirstAnimation && itemEffectAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "ItemResetAnimation")
            {
                ShowNext();
                //ShowEffetByIndex(nowIdx + 1);
                isFirstAnimation = false;
            }
        }
    }

    private void OnClickReceiveSkip()
    {
        if (heroEffectAnimator.gameObject.activeSelf == true)
        {
            if (heroEffectAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "HeroNextStep")
            {
                ShowLastPage(datas.Length == 1);
            }
        }
        else if (itemEffectAnimator.gameObject.activeSelf == true)
        {
            if (itemEffectAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "ItemLastAnimation")
            {
                ShowLastPage(datas.Length == 1);
            }
        }
    }

    internal void SetGachaSound(int _idx, int _rank, bool _hero)
    {
        GameCore.Instance.SoundMgr.PauseBGM();
        if (_hero) GameCore.Instance.SoundMgr.SetCommonBattleSound(_rank < 2 ? SFX.Sfx_Gacha_SSS : SFX.Sfx_Gacha_Normal);
        else GameCore.Instance.SoundMgr.SetCommonBattleSound(_rank < 2 ? SFX.Sfx_Gacha_Equip_SSS : SFX.Sfx_Gacha_Normal);
        //GameCore.Instance.SoundMgr.SetCommonBattleSound(isNormalCard ? SFX.Sfx_Gacha_Normal : SFX.Sfx_Gacha_SSS);
    }

    internal void ShowEffetByIndex(int _idx)
    {
        if (datas.Length <= _idx) { ShowLastPage(datas.Length == 1); return; }

        receiveItemListRoot.gameObject.SetActive(false);
        nowIdx = _idx;

        //Debug.Log("현재 인덱스 넘버 : " + nowIdx);
        if (!CardDataMap.IsItemKey(datas[nowIdx].key)) // 영웅
        {
            var data = GameCore.Instance.DataMgr.GetUnitData(datas[nowIdx].key);
            //if (data.IsExpCard())
            //{
            //    ShowEffetByIndex(nowIdx + 1);
            //    return;
            //}

            heroResultUI.SetActive(false);
            heroResultUI.transform.localScale = Vector3.one * 0.01f;
            FirstStandbyHeroSetting();

            spHeroRank.enabled = true;
            spHeroType.enabled = true;

            //bool isNormalCard = (data.rank > 1);

            int rank = Mathf.Min(2, data.rank);
            SetGachaSound(_idx, rank, true);
            GameCore.Instance.DoWaitCall(() =>
            {
                switch (rank)
                {
                    case 0: heroEffectAnimator.SetTrigger("SSS"); break;
                    case 1: heroEffectAnimator.SetTrigger("SS"); break;
                    default: heroEffectAnimator.SetTrigger("Normal"); break;
                }
            });

            //heroEffectAnimator.SetInteger("CheckUniqueInt", rank);

            normalParticle.SetActive(rank >= 2);
            uniqueSSParticle.SetActive(rank == 1);
            uniqueSSSParticle.SetActive(rank == 0);
            receiveHeroEffectRoot.SetActive(true);

        }
        else
        {
            receiveItemEffectRoot.SetActive(true);

            var data = GameCore.Instance.DataMgr.GetItemData(datas[nowIdx].key);
            itemResultUI.SetActive(false);
            itemResultUI.transform.localScale = Vector3.one * 0.01f;
            if (data.invenType == 1) //Equipment
            {
                var sdata = datas[nowIdx];

                GameCore.Instance.SetUISprite(spIllust_Item, data.GetIliustSpriteKey());
                spItemRank.transform.GetChild(0).gameObject.SetActive(true);
                spItemRank.transform.GetChild(1).gameObject.SetActive(true);

                spItemRank.enabled = true;
                spItemType.enabled = true;

                int rank = data.rank;
                SetGachaSound(_idx, rank, false);
                FirstUniqueEffect.gameObject.SetActive(rank == 0);
                SecondUniqueEffect.gameObject.SetActive(rank == 0);
                FirstSSEffect.gameObject.SetActive(rank == 1);
                SecondSSEffect.gameObject.SetActive(rank == 1);
                FirstNormalEffect.gameObject.SetActive(rank > 1);
                SecondNormalEffect.gameObject.SetActive(rank > 1);

                spItemRank.spriteName = "ICON_LV_" + (data.rank + 1).ToString("00");
                spItemType.spriteName = "ICON_TYPE_01_" + data.equipLimit.ToString("00");
                lbItemName.text = string.Format("{0} {1}(+{2})", ((ItemSData)datas[nowIdx]).GetPrefixNameString(), data.name, ((ItemSData)datas[nowIdx]).enchant);
                lbItemType.text = data.GetSubTypeString() + "     |     " + CardDataMap.GetStrType(data.equipLimit);
                lbItemInfo.text = "전투력     |     [00FF00]" + data.GetDefPower().ToString("N0");
                SetItemStar(5 - data.rank);


                for (int i = 0; i < 5; ++i) { ItemClassType.transform.GetChild(i).gameObject.SetActive(false); }
                ItemClassType.GetComponent<UIWidget>().alpha = 0.01f;
                Item_BG_1.alpha = 0.01f;
                Item_BG_2.alpha = 0.01f;
                //ItemClassType.GetComponent<UITweener>().ResetToBeginning();
                //ItemClassType.GetComponent<UITweener>().PlayForward();
                //Debug.LogError("ResetToBigging()");
                ItemClassType.transform.GetChild(data.equipLimit - 1).gameObject.SetActive(true);


            }
            else if (data.invenType == 4) //interior
            {

                spItemRank.transform.GetChild(0).gameObject.SetActive(false);
                spItemRank.transform.GetChild(1).gameObject.SetActive(false);

                string fileName = data.fileName;

                //spIllust_Item.spriteName = string.Concat("256_", fileName);
                //spIllust_Item.atlas = GameCore.Instance.ResourceMgr.GetLocalObject<UIAtlas>(string.Concat("MyRoom/IconAtlas/", data.atlasName));
                GameCore.Instance.SetUISprite(spIllust_Item, data.GetIliustSpriteKey());

                for (int i = 0; i < 5; ++i) { ItemClassType.transform.GetChild(i).gameObject.SetActive(false); }

                spItemRank.enabled = false;
                spItemType.enabled = false;
                lbItemName.text = string.Format("{0}", data.name);
                lbItemType.text = data.GetSubTypeString() + "     |     " + CardDataMap.GetStrType(data.equipLimit);
                lbItemInfo.text = "만족도     |     [F600FF]" + data.optionValue[0];
                SetItemStar(5 - data.rank);
                int rank = data.rank;
                SetGachaSound(_idx, rank, false);
                FirstUniqueEffect.gameObject.SetActive(rank == 0);
                SecondUniqueEffect.gameObject.SetActive(false);
                FirstSSEffect.gameObject.SetActive(rank == 1);
                SecondSSEffect.gameObject.SetActive(false);
                FirstNormalEffect.gameObject.SetActive(rank > 1);
                SecondNormalEffect.gameObject.SetActive(false);
            }
            else  // etc // 표현 안함.
                ShowEffetByIndex(nowIdx + 1);
        }

    }

    internal void FirstStandbyHeroSetting()
    {
        var sdata = datas[nowIdx] as HeroSData;
        var data = GameCore.Instance.DataMgr.GetUnitData(sdata.key);

        GameCore.Instance.SetUISprite(spIllust_Char, data.GetIllustSpeiteKey());
        GameCore.Instance.SetUISprite(spThirdIllust_Char, data.GetIllustSpeiteKey());

        for (int i = 0; i < 5; ++i) { heroClassType.transform.GetChild(i).gameObject.SetActive(false); }

        if (0 < data.charType)
        {
            heroClassType.transform.GetChild(data.charType - 1).gameObject.SetActive(true);

            spHeroRank.spriteName = "ICON_LV_" + (data.rank + 1).ToString("00");
            spHeroType.spriteName = "ICON_TYPE_01_" + data.charType.ToString("00");
            spHeroType.alpha = 1.0F;
            lbHeroName.text = string.Format(sdata.enchant == 0 ? "{0}" : "{0} (+{1})", data.name, sdata.enchant); 
            lbHeroType.text = "소속     |     " + data.GetBelongString();
            lbHeroInfo.text = "전투력     |     [00FF00]" + sdata.GetPower().ToString("N0");
            SetHeroStar(data.evolLvl);
        }
        else
        {
            spHeroRank.spriteName = "ICON_LV_" + (data.rank + 1).ToString("00");
            spHeroType.spriteName = "";
            spHeroType.alpha = 0.0F;
            lbHeroName.text = data.name;
            lbHeroType.text = "";
            lbHeroInfo.text = "";
            SetHeroStar(0);
        }

    }

    IEnumerator coOpenCardAnimation()
    {
        if (HeroCardListSlotAni[0] != null)
        {
            for (int i = 0; i < HeroCardListSlotAni.Length; ++i)
            {
                if (HeroCardListSlotAni[i] != null)
                {
                    GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Gacha_list, false);
                    yield return new WaitForSeconds(0.08f);
                    HeroCardListSlotAni[i].gameObject.SetActive(true);
                }
            }
        }
        if (ItemCardListSLotAni[0] != null)
        {
            for (int i = 0; i < ItemCardListSLotAni.Length; ++i)
            {
                if (ItemCardListSLotAni[i] != null)
                {
                    GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Gacha_list, false);
                    yield return new WaitForSeconds(0.08f);
                    ItemCardListSLotAni[i].gameObject.SetActive(true);
                }
            }
        }

        StartCoroutine("coOpenUniqueCardAnimation");

        yield return new WaitForSeconds(0.4f);

        closeLabel.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);

    }
    IEnumerator coOpenUniqueCardAnimation()
    {
        if (HeroCardListSlotAni[0] != null)
        {
            for (int i = 0; i < HeroCardListSlotAni.Length; i++)
            {
                if (HeroCardListSlotAni[i] != null)
                {
                    CardBase card = HeroCardListSlotAni[i].transform.GetChild(0).GetChild(0).GetComponent<CardBase>();

                    if (card.Data.rank == 0)
                    {
                        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Gacha_SSS_Get);
                        HeroCardListSlotAni[i].transform.GetChild(2).gameObject.SetActive(true);
                        yield return new WaitForSeconds(0.15f);
                    }
                    else if (card.Data.rank == 1)
                    {
                        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Gacha_SSS_Get);
                        HeroCardListSlotAni[i].transform.GetChild(3).gameObject.SetActive(true);
                        yield return new WaitForSeconds(0.15f);
                    }
                }
            }
        }
        if (ItemCardListSLotAni[0] != null)
        {
            for (int i = 0; i < ItemCardListSLotAni.Length; ++i)
            {
                if (ItemCardListSLotAni[i] != null)
                {
                    CardBase card = ItemCardListSLotAni[i].transform.GetChild(0).GetChild(0).GetComponent<CardBase>();
                    if (card.Data.rank == 0)
                    {
                        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Gacha_SSS_Get);
                        ItemCardListSLotAni[i].transform.GetChild(2).gameObject.SetActive(true);
                        yield return new WaitForSeconds(0.15f);
                    }
                    else if (card.Data.rank == 1)
                    {
                        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Gacha_SSS_Get);
                        ItemCardListSLotAni[i].transform.GetChild(3).gameObject.SetActive(true);
                        yield return new WaitForSeconds(0.15f);
                    }
                }
            }
        }
    }

    //카드 다 보여주는 곳.
    internal void ShowLastPage(bool isOne)
    {
        if (!isOne)
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Gacha_list_Start);
        //GameCore.Instance.SoundMgr.SetCommonBattleSound(isOne ? SFX.Sfx_Item_Get : SFX.Sfx_Gacha_list);
        receiveItemEffectRoot.SetActive(false);
        receiveHeroEffectRoot.SetActive(false);
        receiveItemListRoot.SetActive(true);
        GameCore.Instance.StartCoroutine(coOpenCardAnimation());
        nowGachaPlaying = true;
    }

    private void SetItemStar(int _value)
    {
        if (_value == 0)
        {
            for (int i = 0; i < 5; ++i)
                itemStarRoot.transform.GetChild(i).gameObject.SetActive(false);
            return;
        }

        //Debug.Log("COUNT : " + _value);
        if (_value <= 5)
        {
            for (int i = 0; i < 5; ++i)
            {
                itemStarRoot.transform.GetChild(i).GetComponent<UISprite>().color = Color.white;
                if (_value <= i)
                {
                    itemStarRoot.transform.GetChild(i).gameObject.SetActive(true);
                    itemStarRoot.transform.GetChild(i).GetComponent<UISprite>().spriteName = "ICON_STAR_00";
                }
                else
                {
                    itemStarRoot.transform.GetChild(i).gameObject.SetActive(true);
                    itemStarRoot.transform.GetChild(i).GetComponent<UISprite>().spriteName = "ICON_STAR_01";
                }
            }
        }
        else
        {
            _value -= 5;
            for (int i = 0; i < 5; ++i)
            {
                if (_value <= i)
                {
                    itemStarRoot.transform.GetChild(i).gameObject.SetActive(true);
                    itemStarRoot.transform.GetChild(i).GetComponent<UISprite>().spriteName = "ICON_STAR_01";
                    itemStarRoot.transform.GetChild(i).GetComponent<UISprite>().color = new Color(0.6f, 0.6f, 1f);

                }
                else
                {
                    itemStarRoot.transform.GetChild(i).gameObject.SetActive(true);
                    itemStarRoot.transform.GetChild(i).GetComponent<UISprite>().spriteName = "ICON_STAR_02";
                    itemStarRoot.transform.GetChild(i).GetComponent<UISprite>().color = Color.white;
                }
            }
        }

        itemStarRoot.enabled = true;
    }

    private void SetHeroStar(int _value)
    {
        if (_value == 0)
        {
            for (int i = 0; i < 5; ++i)
                heroStarRoot.transform.GetChild(i).gameObject.SetActive(false);
            return;
        }

        //Debug.Log("COUNT : " + _value);
        if (_value <= 5)
        {
            for (int i = 0; i < 5; ++i)
            {
                heroStarRoot.transform.GetChild(i).GetComponent<UISprite>().color = Color.white;
                if (_value <= i)
                {
                    heroStarRoot.transform.GetChild(i).gameObject.SetActive(true);
                    heroStarRoot.transform.GetChild(i).GetComponent<UISprite>().spriteName = "ICON_STAR_00";
                }
                else
                {
                    heroStarRoot.transform.GetChild(i).gameObject.SetActive(true);
                    heroStarRoot.transform.GetChild(i).GetComponent<UISprite>().spriteName = "ICON_STAR_01";
                }
            }
        }
        else
        {
            _value -= 5;
            for (int i = 0; i < 5; ++i)
            {
                if (_value <= i)
                {
                    heroStarRoot.transform.GetChild(i).gameObject.SetActive(true);
                    heroStarRoot.transform.GetChild(i).GetComponent<UISprite>().spriteName = "ICON_STAR_01";
                    heroStarRoot.transform.GetChild(i).GetComponent<UISprite>().color = new Color(0.6f, 0.6f, 1f);

                }
                else
                {
                    heroStarRoot.transform.GetChild(i).gameObject.SetActive(true);
                    heroStarRoot.transform.GetChild(i).GetComponent<UISprite>().spriteName = "ICON_STAR_02";
                    heroStarRoot.transform.GetChild(i).GetComponent<UISprite>().color = Color.white;
                }
            }
        }

        heroStarRoot.enabled = true;
    }

    public bool GetNowGachaPlaying()
    {
        return nowGachaPlaying;
    }
}