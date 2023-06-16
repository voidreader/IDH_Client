using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInfoScript : MonoBehaviour
{
	public bool bWithSdata { get; protected set; }


	internal long id;
	internal CardSData sdata;
	internal bool bHave;
	internal bool bPopup;
	internal CardBase card;

	internal Transform cardRoot;
	internal new UILabel name;
	internal UILabel hlInfo;
	internal UILabel disc;
	internal UIGrid grid;
	internal Transform equipHeroRoot;
	internal GameObject equiperRoot;

	internal UILabel saleValue;
	internal GameObject sliderRoot;
	internal UISlider slider;
	internal UILabel sliderCount;
	internal int maxCount;
	internal int sellValue;
	internal int nowCount;

	private void InitLink()
	{
		if (cardRoot == null)
		{
			cardRoot = UnityCommonFunc.GetComponentByName<Transform>(gameObject, "cardRoot");
			name = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "name");
			hlInfo = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "state");
			disc = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "disc");
			grid = UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "equipGrid");
			equipHeroRoot = UnityCommonFunc.GetComponentByName<Transform>(gameObject, "equipHeroCardRoot");
			equiperRoot = UnityCommonFunc.GetGameObjectByName(gameObject, "equiperRoot");
			saleValue = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "sale");
			sliderRoot = UnityCommonFunc.GetGameObjectByName(gameObject, "sliderRoot");
			slider = UnityCommonFunc.GetComponentByName<UISlider>(gameObject, "Slider");
			sliderCount = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "Count");
		}
		if(saleValue!= null)
			saleValue.gameObject.SetActive(false);
		if (sliderRoot != null)
			sliderRoot.SetActive(false);
		if (equiperRoot != null)
			equiperRoot.SetActive(false);
	}


    internal void Init(CardSData _sdata, bool _have, bool _popup = false, int texture =-1)
	{
		id = _sdata.uid;
		sdata = _sdata;
		bHave = _have;
		bPopup = _popup;
		InitLink();

		if( grid != null )
			for(int i = grid.transform.childCount - 1; i >= 0; --i)
				Destroy(grid.transform.GetChild(i).gameObject);

		for (int i = cardRoot.childCount - 1; i >= 0; --i)
			Destroy(cardRoot.GetChild(0).gameObject);

        if (card != null)
            Destroy(card.gameObject);

        switch (_sdata.type)
		{
			case CardType.Interior:		InitInterior(texture);	break;
			case CardType.Character:	InitCharter();		    break;
			case CardType.TeamSkill:	InitTeamSkill(texture);	break;
			default:					InitItem(texture);		break;
		}
	}

    internal void InitByStore(ShopPackageSData _sdata)
    {
        InitLink();

        if (grid != null)
            for (int i = grid.transform.childCount - 1; i >= 0; --i)
                Destroy(grid.transform.GetChild(i).gameObject);

        for (int i = cardRoot.childCount - 1; i >= 0; --i)
            Destroy(cardRoot.GetChild(0).gameObject);

        name.text = GameCore.Instance.DataMgr.GetShopPackageInfoData(_sdata.id).name.Replace('\n', ' ');
		if (_sdata.stringIndex > 1)
		{
			Debug.Log(_sdata.stringIndex);
			hlInfo.text = GameCore.Instance.DataMgr.GetShopItemStringData(_sdata.stringIndex).str;
		}

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _sdata.rewardSData.Count; ++i)
        {
            if (i != 0)
                sb.Append(", ");

			// get form card type
			Debug.Log(_sdata.rewardSData[i].id);
            if (CardDataMap.IsItemKey(_sdata.rewardSData[i].id))
                sb.Append(GameCore.Instance.DataMgr.GetItemData(_sdata.rewardSData[i].id).name);
            else
                sb.Append(GameCore.Instance.DataMgr.GetUnitData(_sdata.rewardSData[i].id).name);

            sb.Append(" x");
            sb.Append(_sdata.rewardSData[i].value);
        }

        disc.text = sb.ToString();

        card = CardBase.CreateBigCardByKey((int)ResourceType.Gold, cardRoot.transform, null, null) as ItemCardBase;
        card.SetPressCallback(null);
        ((ItemCardBase)card).GetCountLabel().text = "";
        if (0 < _sdata.texture)
            GameCore.Instance.SetUISprite(((ItemCardBase)card).GetSprite(), _sdata.texture);
    }

    internal void InitCharter()
	{
		UnitDataMap data = GameCore.Instance.DataMgr.GetUnitData(sdata.key);
		card = CardBase.CreateCard(sdata, data, true, cardRoot);
		card.transform.localScale = new Vector3(0.75f, 0.75f);

		name.text = data.name.Replace('\n', ' ');

		if (bPopup)
		{
			hlInfo.text = CardDataMap.GetStrRank(data.rank) + "등급 " + CardDataMap.GetStrType(data.charType) + " 영웅";
			disc.text = GameCore.Instance.DataMgr.GetCharacterStringData(data.discId);
		}
		else
		{
			hlInfo.text = ((HeroSData)sdata).GetStatesString();
			disc.text = "";
		}

		if (bHave && !bPopup)
		{
			int n = 0;
			// 장착아이템 정보
			for (int i = 0; i < ((HeroSData)sdata).equipItems.Length; ++i)
			{
                if (((HeroSData)sdata).equipItems[i] <= 0)
                    continue;

                ++n;
				var itemSData = GameCore.Instance.PlayerDataMgr.GetItemSData(((HeroSData)sdata).equipItems[i]);
				var item = CardBase.CreateSmallCard(itemSData, grid.transform, null, (_id) => GameCore.Instance.ShowCardInfo(GameCore.Instance.PlayerDataMgr.GetItemSData(_id)));
			}
			// 장착아이템 빈슬롯 추가
			for (; n < 5; ++n)
			{
				var itemObj = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("commonRsc/prefab/card_small_empty", grid.transform);
			}
		}


		grid.enabled = true;
	}

	internal void InitItem(int _texture)
	{
		ItemDataMap data = GameCore.Instance.DataMgr.GetItemData(sdata.key);

		// 카드 설정
        if (bHave) card = CardBase.CreateCard(sdata, data, true, cardRoot);
        else       card = CardBase.CreateCard(null, data, true, cardRoot);
        if (0 < _texture)
            GameCore.Instance.SetUISprite(((ItemCardBase)card).GetSprite(), _texture);

        name.text = data.name.Replace('\n',' ');
		hlInfo.text = "";

		switch (data.type)
		{
			case CardType.Equipment:
                if (bHave)
                {
                    var itemSdata = ((ItemSData)sdata);
                    if (itemSdata.prefixIdx != 0)
                    {
                        hlInfo.text = ItemSData.GetPrefixOptionString(itemSdata.prefixIdx, itemSdata.prefixValue);
                        name.text = string.Format("{0}{1}", itemSdata.GetPrefixNameString(), data.name.Replace('\n', ' '));
                    }

                    disc.text = itemSdata.GetAllOptionString();
                }
                else
                {
                    hlInfo.text = "랜덤 능력치";
                    disc.text = GameCore.Instance.DataMgr.GetItemStringData(data.discID); // ((ItemSData)sdata).GetAllOptionString();
                }


                // 착용자 정보
                if (bHave && ((ItemSData)sdata).equipHeroUID != 0 && equiperRoot != null)
				{
					var equiperData = GameCore.Instance.PlayerDataMgr.GetUnitSData(((ItemSData)sdata).equipHeroUID);
					var equipCard = CardBase.CreateSmallCard(equiperData, equipHeroRoot, null, (_id) => GameCore.Instance.ShowCardInfo(GameCore.Instance.PlayerDataMgr.GetUnitSData(_id)));
					equiperRoot.SetActive(equipCard != null);
				}
				break;

			case CardType.GiftBox:
				disc.text = GameCore.Instance.DataMgr.GetItemStringData(data.discID);
				break;
				
			case CardType.resource:
			default:
				disc.text = GameCore.Instance.DataMgr.GetItemStringData(data.discID);
				break;
		}
	}


    internal void InitItemByStore(int _texture, string _name, string _desc)
    {
        card = CardBase.CreateBigCardByKey((int)ResourceType.Gold, cardRoot) as ItemCardBase;
        GameCore.Instance.SetUISprite(((ItemCardBase)card).GetSprite(), _texture);

        name.text = _name.Replace('\n', ' ');
        hlInfo.text = "";
        disc.text = _desc;
    }


	internal void InitTeamSkill(int _texture = -1)
	{
		var data = GameCore.Instance.DataMgr.GetTeamSkillData(sdata.key);

		// 카드 설정
		card = CardBase.CreateCard( null, data, true, cardRoot);
		name.text = data.name.Replace('\n', ' ');
		disc.text = data.disc;
        if (0 < _texture)
            GameCore.Instance.SetUISprite(((ItemCardBase)card).GetSprite(), _texture);

        StringBuilder sb = new StringBuilder();
		for (int i = 0; i < data.needChar.Length;)
		{
			var unitData = GameCore.Instance.DataMgr.GetUnitDataByCharID(data.needChar[i]);
			if (unitData != null)
				sb.Append(unitData.name);
			else
				sb.Append(data.needChar[i]);

			++i;

			if (i < data.needChar.Length)
				sb.Append(", ");
		}
		hlInfo.text = sb.ToString();
	}

	internal void InitInterior(int _texture)
	{
		ItemDataMap data = GameCore.Instance.DataMgr.GetItemData(sdata.key);

		// 카드 설정
		card = CardBase.CreateCard(sdata, data, true, cardRoot);
        if (0 < _texture)
            GameCore.Instance.SetUISprite(((ItemCardBase)card).GetSprite(), _texture);

        if (saleValue != null)	saleValue.gameObject.SetActive(true);
		if (sliderRoot != null)	sliderRoot.SetActive(true);

		name.text = data.name.Replace('\n', ' ');
		hlInfo.text = "만족도 " + data.optionValue[0];
		switch (data.subType)
		{
			case ItemSubType.Furniture: disc.text = "가구"; break;
			case ItemSubType.Prop:			disc.text = "소품"; break;
			case ItemSubType.Wall:			disc.text = "벽지"; break;
			case ItemSubType.Floor:			disc.text = "바닥"; break;
		}
		// Todo : 세트명가져오기
		disc.text += "\n" + "";

		if (slider != null)
		{
			sellValue = data.sellValue;
			maxCount = ((ItemSData)sdata).count - ((ItemSData)sdata).myRoomCount;
			slider.numberOfSteps = maxCount + 1;
			SetCount(Mathf.Min(1, maxCount));

			slider.onChange.Add(new EventDelegate(ChangeSlider));
			//UnityCommonFunc.GetComponentByName<UIButton>(sliderCount.gameObject, "Inc").onClick.Add(new EventDelegate(IncCount));
			var inc = UnityCommonFunc.GetComponentByName<ButtonRapper>(sliderCount.gameObject, "Inc");
			inc.SetClickCallback(IncCount);
			inc.SetPressCallback(() => StartCoroutine(CoCount_Loop(true)));
			inc.SetStopPressCallback(() => StopAllCoroutines());
			var dec = UnityCommonFunc.GetComponentByName<ButtonRapper>(sliderCount.gameObject, "Dec");
			dec.SetClickCallback(DecCount);
			dec.SetPressCallback(() => StartCoroutine(CoCount_Loop(false)));
			dec.SetStopPressCallback(() => StopAllCoroutines());
		}
	}

	internal void ChangeSlider()
	{
        if (maxCount == 0)
            SetCount(0);
        else
        {
            var count = Mathf.Clamp((slider.value * maxCount), 1, maxCount);
            SetCount(Mathf.RoundToInt(count));
        }
	}

	internal void IncCount()
	{
        if (maxCount == 0)
            SetCount(0);
        else
        {
            var count = Mathf.Clamp((slider.value * maxCount) + 1, 1, maxCount);
            SetCount(Mathf.RoundToInt(count));
        }
	}

	IEnumerator CoCount_Loop(bool _inc)
	{
		float interval = 0.3f;
		float cnt = 1;
		while (true)
		{
			var count = Mathf.Clamp((slider.value * maxCount) + (int)((_inc) ? cnt : (-cnt)), 1, maxCount);
			SetCount(Mathf.RoundToInt(count));

			if(count == 29)
				Debug.Log(interval + "  " + cnt);

			if (interval > 0.05f)
				yield return new WaitForSeconds(interval);
			else
				yield return null;
			
			interval *= 0.6f;
			cnt *= 1.005f;
		}
	}

	internal void DecCount()
	{
        if (maxCount == 0)
            SetCount(0);
        else
        {
            var count = Mathf.Clamp((slider.value * maxCount) - 1, 1, maxCount);
            SetCount((int)count);
        }
	}

	internal void SetCount(int _count)
	{
        Debug.Log("Count : " + _count);
		nowCount = _count;
		sliderCount.text = nowCount.ToString();
		saleValue.text = (sellValue * nowCount) + " G";
        if (maxCount == 0)  slider.Set(0, false);
        else                slider.Set((float)nowCount / maxCount, false);
	}

	internal int GetCount()
	{
		return (int)(slider.value * maxCount);
	}

	internal void SetName(string _name)
	{
		name.text = _name.Replace('\n', ' ');
	}

	internal void SetHLInfo(string _str)
	{
		hlInfo.text = _str;
	}

	internal void SetDisc(string _str)
	{
		disc.text = _str;
	}

	internal void AddEquipItem(Transform _tf)
	{
		_tf.parent = grid.transform;
		grid.enabled = true;
	}

	internal void ClearEquipItem()
	{
		while(grid.transform.childCount != 0)
			Destroy(grid.transform.GetChild(0).gameObject);
	}

	internal void SetEquiper()
	{

	}
}
