using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public abstract class CardSData : JsonParse
{
	internal long uid;      // 인스턴스 유니크 아이디
	internal CardType type; // 아이템 타입( 캐릭터 포함 )
	internal int key;       // 테이블 키값
    internal int enchant;   // 강화 레벨
    internal int exp;       // 강화 exp

    internal DateTime createDate; // 생성 시간

    internal bool locked;   // 잠금

    internal static CardSData CreateCardSData(JSONObject _json)
	{
		CardSData result = null;
		int key;
		ToParse(_json, "ITEM_ID", out key); // ITEM_ID라는 필드가 존재할 경우 이 JSONObject를 ItemSData로 간주.(없는 경우 HeroSData로 간주)
		var Data = GameCore.Instance.DataMgr.GetItemData(key);
       
		if (Data == null)
		{
			result = new HeroSData();
			result.type = CardType.Character;
		}
		else
		{
			switch (Data.type)
			{
				case CardType.Equipment:
				case CardType.resource:
				case CardType.Money:
				case CardType.Cash:
				case CardType.GiftBox:
				case CardType.Interior:
				case CardType.Character:
				case CardType.Count:
					result = new ItemSData();
					result.type = Data.type;
					break;
				default:
					break;
			}
		}

		return result;
	}
}

public class HeroSData : CardSData
{
	internal bool dispatch;     // 파견여부
	internal int dormitory;     // 마이룸 배치
    internal int farming_Id;//파견중인 파밍 아이디.

    internal long[] equipItems = new long[5]; // 장착된 아이템 아이디들.(직접 찾아 넣어야 한다.

	internal HeroSData()
	{
	}
	internal HeroSData(int _key)
	{
		key = _key;
		type = CardType.Character;
	}

	internal override bool SetData(JSONObject _json)
	{
		ToParse(_json, "CHA_UID", out uid);
		ToParse(_json, "CHA_ID", out key);
        ToParse(_json, "ENCHANT", out enchant);
        ToParse(_json, "EXP", out exp);
        ToParse(_json, "DISPATCH", out dispatch);
		ToParse(_json, "MYROOM_ID", out dormitory);
        ToParse(_json, "FARMING_ID", out farming_Id);

        ToParse(_json, "LOCK", out locked);

        type = CardType.Character;

		return true;
	}

    internal bool SetRewardData(JSONObject _json)
    {
        //ToParse(_json, "CHA_UID", out uid);
        ToParse(_json, "REWARD_ITEM_ID", out key);

        type = CardType.Character;

        ClearEquip();
        return true;
    }

    internal void ClearEquip()
    {
        for (int i = 0; i < equipItems.Length; ++i)
            equipItems[i] = -1;
    }

    internal int GetEquipCount()
    {
        int cnt = 0;
        for (int i = 0; i < equipItems.Length; ++i)
            if (equipItems[i] > 0)
                ++cnt;

        return cnt;
    }

    internal string GetStatesString()
	{
		string state = "";
		
		bool[] bState = new bool[]{
				GameCore.Instance.PlayerDataMgr.IsArrange(uid),//formation != 0,
				dispatch,
				dormitory != 0
			};
		string[] strState = new string[]
		{
		"팀배치",
		"파밍중",
		"숙소" + dormitory
		};

		for (int i = 0; i < bState.Length; i++)
		{
			if (bState[i])
			{
				if (state != "")
					state += ", ";
				state += strState[i];
			}
		}
		if (state == "")
		{
			state = "배치중인 곳이 없습니다.";
		}

		return state;
	}

    private void AddPredixStat(ItemSData item, ref float[] stats)
    {
        int prefixID = item.prefixIdx;
        float prefixValue = item.prefixValue;
        var pos = ItemSData.GetItemEffectStatType((ItemEffectType)item.prefixIdx);

        if (pos == UnitStat.None) return;
        stats[(int)pos] += stats[(int)pos] * prefixValue / 100;
    }

    internal int GetPower(bool isInclude, ItemSData _sdata, int _enchant = -1)
    {
        var data = GameCore.Instance.DataMgr.GetUnitData(key);

        // 모든 스탯 총합
        float[] charStat = new float[(int)UnitStat.Count];
        data.SetStats(ref charStat, _enchant < 0 ? enchant : _enchant);

        float power = 0;
        float[] itemsStat = new float[(int)UnitStat.Count];
        for (int i = 0; i < equipItems.Length; ++i)
        {
            if (equipItems[i] <= 0)
                continue;

            var item = GameCore.Instance.PlayerDataMgr.GetItemSData(equipItems[i]);
            if (isInclude == false && _sdata == item)
                continue;
            item.SetItemStats(ref itemsStat);
            AddPredixStat(item, ref charStat);
        }
        if(isInclude == true)
        {
            _sdata.SetItemStats(ref itemsStat);
            AddPredixStat(_sdata, ref charStat);
        }

        data.SetStatsPowercal(ref charStat);

        PowercalDataMap powercalDataMap = GameCore.Instance.DataMgr.GetPowercalData(data.charType);
        for (int i = 1; i < itemsStat.Length; i++)
        {
            itemsStat[i] *= powercalDataMap.PowercalStats[i - 1];
        }
        for (int i = 0; i < (int)UnitStat.Count; i++)
        {
            power += (charStat[i] + itemsStat[i]);
        }
        
        return (int)power - GetPower(_enchant);
    }

	internal int GetPower(int _enchant = -1)
	{
		var data = GameCore.Instance.DataMgr.GetUnitData(key);

        if (data.IsExpCard())
            return 0;

        // 모든 스탯 총합
        float[] charStat = new float[(int)UnitStat.Count];

        // 테이블스탯 * 강화스탯 
        data.SetStats(ref charStat, _enchant < 0 ? enchant : _enchant);
        
        float power = 0;
        float[] itemsStat = new float[(int)UnitStat.Count];
        for (int i = 0; i < equipItems.Length; ++i)
        {
            if (equipItems[i] <= 0)
                continue;

            var item = GameCore.Instance.PlayerDataMgr.GetItemSData(equipItems[i]);
            // 장착된 아이템의 옵션 스탯(무조건 상수)
            item.SetItemStats(ref itemsStat);
            // 접미사 효과 유닛에 적용(무조건 배율)
            AddPredixStat(item, ref charStat);
        }
        Stat tmpStat = new Stat();
        //for (int i = 0; i < (int)UnitStat.Count; i++)
        //{
        //    tmpStat.SetStat((UnitStat)i, charStat[i] + itemsStat[i]);
        //    Debug.Log("Stat[" + ((UnitStat)i) + "] :  \t" + tmpStat.GetStat((UnitStat)i) + "\t" + charStat[i] + "\t" + itemsStat[i]);
        //}

        // * 스탯별전투력배율
        data.SetStatsPowercal(ref charStat);

        
        PowercalDataMap powercalDataMap = GameCore.Instance.DataMgr.GetPowercalData(data.charType);
        for (int i = 1; i < itemsStat.Length; i++)
        {
            itemsStat[i] *= powercalDataMap.PowercalStats[i - 1];
        }
        for(int i = 0; i < (int)UnitStat.Count; i++)
        {
            power += (charStat[i] + itemsStat[i]);
        }
        //// 모든 아이템 스탯 종합
        //for (int i = 0; i < equipItems.Length; ++i)
        //{
        //    if (equipItems[i] <= 0)
        //        continue;

        //    var item = GameCore.Instance.PlayerDataMgr.GetItemSData(equipItems[i]);
        //    power += item.GetPower(true, baseStat);
        //}

        return (int)power;
	}

    internal Stat GetStat()
    {
        var data = GameCore.Instance.DataMgr.GetUnitData(key);
        var baseStat = data.GetStat(enchant);
        //var itemStat = GetItemStat(baseStat);

        return baseStat;//baseStat.AddStat(itemStat);
    }

    internal Stat GetBaseStat(int _enchant = -1)
    {
        if (_enchant < 0)
            _enchant = enchant;

        var data = GameCore.Instance.DataMgr.GetUnitData(key);
        return data.GetStat(_enchant);
    }

    internal Stat GetItemStat(Stat _baseStat)
    {
        Stat stat = new Stat();
        Stat statAddtive = new Stat();

        // 모든 아이템 스탯 종합
        for (int i = 0; i < equipItems.Length; ++i)
        {
            if (equipItems[i] <= 0)
                continue;

            var item = GameCore.Instance.PlayerDataMgr.GetItemSData(equipItems[i]);

            stat.AddStat(item.GetStat(false));
            stat.AddStat(item.GetStat(true).MulStat(_baseStat));
        }

        return stat;
    }

    internal int GetFarmingID()
    {
        return farming_Id;
    }

    internal bool IsExpCard()
    {
        return GameCore.Instance.DataMgr.GetUnitData(key).IsExpCard();
    }
}

public class ItemSData : CardSData
{
	internal long equipHeroUID;     // 착용자 UID
	internal int count;
	internal int myRoomCount;

	internal int prefixIdx;
	internal float prefixValue;

	internal int[] optionIdx;      
	internal float[] optionValue;   


	internal ItemSData()
	{

	}
	internal ItemSData(int _key,int _count)
	{
		key = _key;
		count = _count;

        var Data = GameCore.Instance.DataMgr.GetItemData(key);
        if (Data == null)   type = CardType.Character;
        else                type = Data.type;
    }

    internal ItemSData(int _key, int _count, CardType _type)
    {
        key = _key;
        count = _count;
        type = _type;
    }

    internal override bool SetData(JSONObject _json)
	{
		ToParse(_json, "ITEM_UID", out uid);
		ToParse(_json, "ITEM_ID", out key);
        var data = GameCore.Instance.DataMgr.GetItemData(key);
		if( data == null)
		{
			Debug.LogError("잘못된 데이터");
			return false;
		}
		type = data.type;
		ToParse(_json, "CHA_UID", out equipHeroUID);
		ToParse(_json, "ENCHANT", out enchant);
        ToParse(_json, "EXP", out exp);

        float tmpCnt = 0;
        ToParse(_json, "ITEM_COUNT", out tmpCnt);
        count = (int)tmpCnt;

		ToParse(_json, "MYROOM_ITEM_COUNT", out myRoomCount);

		JSONObject prefixJson = _json.GetField("PREFIX");
		if( prefixJson != null)
		{
			ToParse(prefixJson, "ID", out prefixIdx);
			ToParse(prefixJson, "VALUE", out prefixValue);
		}

        JSONObject optionsJson = _json.GetField("OPTIONS");
        if (optionsJson != null && optionsJson.Count > 0)
        {
            optionIdx = new int[optionsJson.Count];
            optionValue = new float[optionsJson.Count];
            for (int i = 0; i < optionsJson.Count; ++i)
            {
                ToParse(optionsJson[i], "ID", out optionIdx[i]);
                ToParse(optionsJson[i], "VALUE", out optionValue[i]);
            }
        }

        ToParse(_json, "CREATE_DATE", out createDate);
        
        ToParse(_json, "LOCK", out locked);

        return true;
	}

    internal ItemSData Clone()
    {
        var sdata = new ItemSData();
        sdata.uid = uid;
        sdata.type = type;
        sdata.key = key;
        sdata.enchant = enchant;
        sdata.exp = exp;
        sdata.equipHeroUID = equipHeroUID;
        sdata.count = count;
        sdata.myRoomCount = myRoomCount;
        sdata.prefixIdx = prefixIdx;
        sdata.prefixValue = prefixValue;
        sdata.optionIdx = new int[optionIdx.Length];
        sdata.optionValue = new float[optionValue.Length];

        for (int i = 0; i < optionIdx.Length; ++i)
            sdata.optionIdx[i] = optionIdx[i];

        for (int i = 0; i < optionValue.Length; ++i)
            sdata.optionValue[i] = optionValue[i];

        return sdata;
    }

    internal bool SetRewardData(JSONObject _json)
    {
        //ToParse(_json, "ITEM_UID", out uid);
        ToParse(_json, "REWARD_ITEM_ID", out key);
        var data = GameCore.Instance.DataMgr.GetItemData(key);
        if (data == null)
        {
            Debug.LogError("잘못된 데이터");
            return false;
        }
        type = data.type;

        float cnt;
        ToParse(_json, "REWARD_ITEM_COUNT", out cnt);
        count = (int)cnt;

        JSONObject prefixJson = _json.GetField("PREFIX");
        if (prefixJson != null)
        {
            ToParse(prefixJson, "ID", out prefixIdx);
            ToParse(prefixJson, "VALUE", out prefixValue);
        }

        JSONObject optionsJson = _json.GetField("OPTIONS");
        if (optionsJson != null && optionsJson.Count > 0)
        {
            optionIdx = new int[optionsJson.Count];
            optionValue = new float[optionsJson.Count];
            for (int i = 0; i < optionsJson.Count; ++i)
            {
                ToParse(optionsJson[i], "ID", out optionIdx[i]);
                ToParse(optionsJson[i], "VALUE", out optionValue[i]);
            }
        }

        return true;
    }


    internal string GetPrefixNameString()
	{
		if (prefixIdx == 0)
			return "";

		var itemEffect = GameCore.Instance.DataMgr.GetItemEffectData(prefixIdx);
		return itemEffect.name + " ";
	}

	internal static string GetPrefixOptionString(int _effectId, float _effectValue)
	{
		if (_effectId == 0)
			return string.Empty;

		var itemEffect = GameCore.Instance.DataMgr.GetItemEffectData(_effectId);
		if (itemEffect == null)
			return string.Empty;

		return string.Format("{0} {1:0.#}{2}", GetItemEffectString(itemEffect.effectType), _effectValue, itemEffect.type == 0 ? "" : "%");
	}

    internal string GetPrefixOptString()
    {
        return GetPrefixOptionString(prefixIdx, prefixValue);
    }


    internal string GetAllOptionString()
	{
        if (optionIdx == null)
            return string.Empty;

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < optionIdx.Length; ++i)
		{
			if (optionIdx[i] <= 0)
				continue;

            var str = GetPrefixOptionString(optionIdx[i], optionValue[i]);
			if (str.Length == 0)
				continue;

            if (sb.Length != 0)
                sb.Append(", ");

            sb.Append(str);
        }

		return sb.ToString();
	}
    internal static void SetItemOption(int _effectId, float _effectValue, ref float[] _value)
    {
        var itemEffect = GameCore.Instance.DataMgr.GetItemEffectData(_effectId);

        int pos = -1;
        switch(itemEffect.effectType)
        {
            case ItemEffectType.HP: pos = 0;break;
            case ItemEffectType.Attack: pos = 1; break;
            case ItemEffectType.Armor: pos = 2; break;
            case ItemEffectType.Vigor: pos = 3; break;
            case ItemEffectType.Agility: pos = 4; break;
            case ItemEffectType.Concent: pos = 5; break;
            case ItemEffectType.Recovery: pos = 6; break;
            case ItemEffectType.Mental: pos = 7; break;
            case ItemEffectType.Aggro: pos = 8; break;
        }
        _value[pos + 1] += _effectValue;
    }

    internal void SetItemStats(ref float[] stats)
    {
        if (optionIdx == null)
            return;

        for (int i = 0; i < optionIdx.Length; ++i)
        {
            if (optionIdx[i] <= 0)
                continue;

            SetItemOption(optionIdx[i], optionValue[i], ref stats);
        }
    }

    internal int GetPower(bool _equiped, Stat _baseStat = null, int _enchant  = -1)
    {
        var data = GameCore.Instance.DataMgr.GetItemData(key);

        if (_equiped)
        {
            Stat stat = _baseStat.MulStat(GetStat(true, _enchant), 3f).AddStat(GetStat(false, _enchant));
            return (int)stat.GetPower(data.equipLimit);
        }
        else
        {
            Stat stat = GetStat(false, _enchant);//GetStat(true, _enchant).AddStat(GetStat(false, _enchant));
            return (int)stat.GetPower(data.equipLimit);
        }
    }

    /// <summary>
    /// 아이템의 스탯을 가져온다.
    /// </summary>
    /// <remarks> 아이템의 스탯은 서버에서 계산된 값이 온다. 고로 인첸트에 따른 값 가공을 할 필요가 없다. </remarks>
    /// <param name="_bPercent">퍼센트 옵션을 가져와야한다면 true, 절대값 옵션을 가져와야한다면 false</param>
    /// <returns></returns>
    internal Stat GetStat(bool _bPercent, int _enchant = -1)
    {
        var strengthen = GameCore.Instance.DataMgr.GetItemStrengthenConstData();
        if (_enchant < 0)
            _enchant = 0;

        Stat stat = new Stat();

        // 접두어 옵션
        if (prefixIdx > 0)
        {
            var peData = GameCore.Instance.DataMgr.GetItemEffectData(prefixIdx);
            if (peData != null && (peData.type == 1) == _bPercent)
            {
                var Idx = GetItemEffectStatType((ItemEffectType)peData.effectType);
                if (Idx != UnitStat.None)
                    stat.SetStat(Idx, prefixValue * (float)Math.Pow(strengthen.prefixScale, _enchant) /** (peData.type == 1 ? 3f : 1f)*/);
            }
        }

        //아이템 옵션
        if (optionIdx != null)
        {
            for (int i = 0; i < optionIdx.Length; ++i)
            {
                if (optionIdx[i] <= 0)
                    continue;

                var eData = GameCore.Instance.DataMgr.GetItemEffectData(optionIdx[i]);
                if (eData != null && (eData.type == 1) == _bPercent)
                {
                    var idx = GetItemEffectStatType((ItemEffectType)eData.effectType);
                    if (idx == UnitStat.None)
                        continue;

                    stat.SetStat(idx, (stat.GetStat(idx) + optionValue[i]) * (float)Math.Pow(strengthen.optionScale, _enchant) /** (eData.type == 1 ? 3f : 1f)*/);
                }
            }
        }

        return stat;
    }


    internal static string GetItemEffectString(ItemEffectType _type)
	{
		switch (_type)
		{
			case ItemEffectType.MoreReward:		    return "모험 보상 증가"; 
			case ItemEffectType.AutoHeal:			return "매턴 회복"; 
			case ItemEffectType.MoreAttack:		    return "공격력"; 
			case ItemEffectType.MoreCritical:	    return "치명타율"; 
			case ItemEffectType.MoreHP:				return "최대 체력"; 
			case ItemEffectType.MoreAggro:		    return "어그로"; 
			case ItemEffectType.MoreArmor:		    return "방어력"; 
			case ItemEffectType.DecCoolTime:	    return "추가쿨타임 감소"; 
			case ItemEffectType.HP:					return "최대 체력"; 
			case ItemEffectType.Attack:				return "공격력"; 
			case ItemEffectType.Armor:				return "방어력"; 
			case ItemEffectType.Vigor:				return "행동력"; 
			case ItemEffectType.Agility:			return "민첩성"; 
			case ItemEffectType.Concent:			return "집중력"; 
			case ItemEffectType.Recovery:			return "회복력"; 
			case ItemEffectType.Mental:				return "정신력"; 
			case ItemEffectType.Aggro:				return "어그로"; 
			default:								return string.Empty;
		}
	}

    internal static UnitStat GetItemEffectStatType(ItemEffectType _type)
	{
		switch (_type)
		{
			case ItemEffectType.MoreReward:		    return      UnitStat.None;
			case ItemEffectType.AutoHeal:			return      UnitStat.None; 
			case ItemEffectType.MoreAttack:		    return UnitStat.Attack;
            case ItemEffectType.MoreCritical:	    return      UnitStat.None;
			case ItemEffectType.MoreHP:				return UnitStat.Hp;
			case ItemEffectType.MoreAggro:		    return UnitStat.Aggro;
			case ItemEffectType.MoreArmor:		    return UnitStat.Armor;
			case ItemEffectType.DecCoolTime:	    return      UnitStat.None;
            case ItemEffectType.HP:					return UnitStat.Hp;
			case ItemEffectType.Attack:				return UnitStat.Attack;
			case ItemEffectType.Armor:				return UnitStat.Armor; 
			case ItemEffectType.Vigor:				return UnitStat.Vigor; 
			case ItemEffectType.Agility:			return UnitStat.Agility;
			case ItemEffectType.Concent:			return UnitStat.Concent;
			case ItemEffectType.Recovery:			return UnitStat.Recovery;
			case ItemEffectType.Mental:				return UnitStat.Mental;
			case ItemEffectType.Aggro:				return UnitStat.Aggro;
			default:								return      UnitStat.None;
		}
	}
}

class TeamSkillSData : CardSData
{
	internal TeamSkillSData(int _key)
	{
		key = _key;
		type = CardType.TeamSkill;
	}

	internal override bool SetData(JSONObject _json)
	{
		return true;
	}
}

public class RoomStain
{
    public int nMyRoomID;
    public int nStainUID;
    public long nHelpUserUID;
    public int nRewardItemID;
    public int nRewardItemCount;

    public string startTime;
    public string endTime;
    //public Vector2 vAnchor;
    //public List<Vector2> posList;

    public RoomStain()
    {
        //vAnchor = new Vector2();
        //posList = new List<Vector2>();
    }
}

public class RoomItem
{
    public int nTableKey;
    public int nMyRoomItemUID;
    public int nMyRoomID;
    public int nItemUID;
    public float fAngle;
    public Vector2 vAnchor;
    public List<Vector2> posList;

    // Old 내용
    // vAnchor 나 posList가 존재하지 않는데
    // attachType 이 wall 이면
    // 벽을 대체하는 오브젝트
    // attachType 이 ground 이면
    // 바닥을 대체하는 오브젝트
    //public BaseObject.eAttachType attachType;
    public int attachType;

    public RoomItem()
    {
        vAnchor = new Vector2();
        posList = new List<Vector2>();
    }
}

internal class MyRoomItemInfo : CardSData
{
    public Dictionary<int, RoomItem> roomBaseItemDic = new Dictionary<int, RoomItem>();
    public List<RoomItem> roomItemList = new List<RoomItem>();
    public List<RoomStain> roomStainList = new List<RoomStain>();

    // 레벨 및 경험치 등
    internal override bool SetData(JSONObject _json)
    {
        //ToParse(_json, "ITEM_UID", out uid);
        //ToParse(_json, "ITEM_ID", out key);

        JSONObject itemLIstJson = _json.GetField("ITEM_LIST");
        if (itemLIstJson != null)
        {
        	for (int i = 0; i < itemLIstJson.Count; ++i)
        	{
        		var optionSubJson = itemLIstJson[i];

                RoomItem roomItem = new RoomItem();
                ToParse(optionSubJson, "ITEM_ID", out roomItem.nTableKey);

                ItemDataMap itemDataMap = GameCore.Instance.PlayerDataMgr.GetItemData((long)roomItem.nTableKey);
                if (itemDataMap == null)
                    itemDataMap = GameCore.Instance.DataMgr.GetItemData(roomItem.nTableKey);

                //if( itemDataMap.attachType == (int)BaseObject.eAttachType.wall)
                {
                    //roomItem.attachType = (BaseObject.eAttachType)itemDataMap.attachType;
                }

                ToParse(optionSubJson, "MYROOM_ITEM_UID", out roomItem.nMyRoomItemUID);
        		ToParse(optionSubJson, "MYROOM_ID", out roomItem.nMyRoomID);
        		ToParse(optionSubJson, "ITEM_UID", out roomItem.nItemUID);

                JSONObject anchorJO = new JSONObject();
                anchorJO = optionSubJson.GetField("ANCHOR");
                roomItem.vAnchor = ToVector2(anchorJO);

                ToParse(optionSubJson, "ANGLE", out roomItem.fAngle);

                JSONObject posListJO = new JSONObject(JSONObject.Type.ARRAY);
                posListJO = optionSubJson.GetField("POSITION");

                if (posListJO != null)
                {
                    if (posListJO.Count > 0)
                    {
                        // j이다 헛갈리지말길
                        for (int j = 0; j < posListJO.Count; j++)
                        {
                            Vector2 vPos = new Vector2();
                            vPos = ToVector2(posListJO[j]);
                            roomItem.posList.Add(vPos);
                        }

                        roomItemList.Add(roomItem);
                    }
                    else
                    {
                        AddRoomBaseItem(roomItem);

                    }
                }
                else
                {
                    AddRoomBaseItem(roomItem);
                }
            }
        }


        JSONObject stainLIstJson = _json.GetField("STAIN_LIST");
        if (stainLIstJson != null)
        {
            for (int i = 0; i < stainLIstJson.Count; ++i)
            {
                JSONObject optionSubJson = stainLIstJson[i];

                RoomStain roomStain = new RoomStain();
                ToParse(optionSubJson, "MYROOM_ID", out roomStain.nMyRoomID);
                ToParse(optionSubJson, "STAIN_UID", out roomStain.nStainUID);
                ToParse(optionSubJson, "HELP_USER_UID", out roomStain.nHelpUserUID);
                ToParse(optionSubJson, "REWARD_ITEM_ID", out roomStain.nRewardItemID);
                ToParse(optionSubJson, "REWARD_ITEM_COUNT", out roomStain.nRewardItemCount);
                ToParse(optionSubJson, "START_TIME", out roomStain.startTime);
                ToParse(optionSubJson, "END_TIME", out roomStain.endTime);

                roomStainList.Add(roomStain);
            }
        }

        /*
        var data = GameCore.Instance.DataMgr.GetItemData(key);
        optionIdx = new int[data.optionValue.Length];
        optionValue = new float[data.optionValue.Length];
        for (int i = 0; i < optionIdx.Length; ++i)
        {
            optionIdx[i] = data.optionValue[i];
            optionValue[i] = data.optionIdx[i];
        }
        */
        return true;
    }

    public void AddRoomBaseItem(RoomItem roomItem)
    {
        RoomItem tmpRoomItem = new RoomItem();

        if( roomBaseItemDic.TryGetValue(roomItem.attachType, out tmpRoomItem) )
        {
            roomBaseItemDic[roomItem.attachType] = roomItem;
        }
        else
        {
            roomBaseItemDic.Add(roomItem.attachType, roomItem);
        }

        if (roomBaseItemDic.Count > 2)
        {
            Debug.LogError("overflow roomBaseItemList.Count = " + roomBaseItemDic.Count);
        }
    }

    public Vector2 ToVector2(JSONObject obj)
    {
        float x = obj["x"] ? obj["x"].f : 0;
        float y = obj["y"] ? obj["y"].f : 0;
        return new Vector2(x, y);
    }

    public Vector3 ToVector3(JSONObject obj)
    {
        float x = obj["x"] ? obj["x"].f : 0;
        float y = obj["y"] ? obj["y"].f : 0;
        float z = obj["z"] ? obj["z"].f : 0;
        return new Vector3(x, y, z);
    }

}

