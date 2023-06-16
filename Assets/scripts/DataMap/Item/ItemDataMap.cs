using System;
using System.Collections.Generic;
using UnityEngine;

enum CardType
{
	Equipment,  // 장비
	resource,   // 재료 및 소모품
	Money,		// 게임 머니
	Cash,       // 게임 캐시
	GiftBox,    // 선물상자
	Interior,   // 인테리어
	Character,  // 캐릭터
	TeamSkill,  // 팀스킬
    Immediate,  // 즉시 사용 아이템( 우편함에서만 유효 )
    Count,      // Never Used
    Dummy,
}

enum ItemSubType
{
	None,
	Money,
	Cash,
	Char_Rand_Box,
	Item_Rand_Box,
    Interior_Rand_Box,

	EquipItem = 10,
	Head,
	Body,
	Weapon,
	shoes,
	Accessories,

	Interior = 20,
	Furniture,
	Prop,
	Wall,
	Floor,

	Count		// Never Use
}

class ItemDataMap : CardDataMap
{
	internal int		lifeType;		// 0 : 수량제, 1:기간제, 2: 기본지급 무제한
	internal int		maxCount;
	internal int		invenType;		// 0 : 기타, 1: 장비, 2:재료, 3:상자, 4:가구
    internal int        originalRank;   // 테이블상에 기입된 랭크값
	internal ItemSubType subType;		// 0 : None, 1:머니, 2:캐시, 3:캐릭랜덤상자, 4:아이템랜덤상자, 5:가구랜덤상자, 11~15: 장비 각 부위, 21~24: 가구 각종류

	internal int		lvlLimit;		// 레벨 제한
	internal int		equipLimit;		// 장착 타입
	internal int		prefixType;     // 접두사 타입. 0:None, 1:Group, 2:ID
	internal int		prefixIdx;		// 접두사 타입에 따라 의미가 다름. (BRuleItemEffect의 인덱스 또는 그룹 아이디로 사용)
	internal int[]	optionIdx;			// 옵션타입. 0 : None, 1:Group, 2:ID, 3:인테리어(만족도)
	internal int[]	optionValue;        // 옵션타입에 따라 의미가 다름. (BRuleItemEffect의 인덱스 또는 그룹 아이디로 사용)(인테리어만 직접 사용)

	
	internal int		inGameImageID;
	internal int		discID;

	internal bool		sellable;		// 팔기 가능 여부
	internal int		sellValue;		// 팔기 가격

	internal int		sizeX;			// 가구의 X 크기
	internal int		sizeY;			// 가구의 Y 크기

    internal string     attachConditionType;        // 조건변수(n = Normal, u = Up, d = Down, l = Left, r = Right)
	internal int        attachConditionValue;		// 조건변수값
    internal int        exp;            // 재료로 사용시 사용되는 값

    internal string     fileName;       //마이룸 파일네임
    internal string     typeName;       //마이룸 배치 타입 네임
    internal int        setNumber;      //세트 넘버
    internal string     atlasName;      //아틀라스 네임
    internal Vector3    scaleValue;


    internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out name);
		ToParse(_csvData[idx++], out originalRank);
        rank = originalRank;
		if (rank != 99) rank %= 10;
		ToParse(_csvData[idx++], out type);
		ToParse(_csvData[idx++], out lifeType);
		ToParse(_csvData[idx++], out maxCount);
		ToParse(_csvData[idx++], out invenType);
		ToParse(_csvData[idx++], out subType);
		ToParse(_csvData[idx++], out lvlLimit);
		ToParse(_csvData[idx++], out equipLimit);
		ToParse(_csvData[idx++], out prefixType);
		ToParse(_csvData[idx++], out prefixIdx);

		optionIdx = new int[4];
		optionValue = new int[4];
		for (int i = 0; i < optionIdx.Length; i++)
		{
			ToParse(_csvData[idx++], out optionIdx[i]);
			ToParse(_csvData[idx++], out optionValue[i]);
		}

		ToParse(_csvData[idx++], out imageID);
		ToParse(_csvData[idx++], out inGameImageID);
		ToParse(_csvData[idx++], out discID);
		ToParse(_csvData[idx++], out sellable);
		ToParse(_csvData[idx++], out sellValue);

		// 가구 전용 데이터
		ToParse(_csvData[idx++], out sizeX);
		ToParse(_csvData[idx++], out sizeY);

        string attachCondition;
		ToParse(_csvData[idx++], out attachCondition);

        if(attachCondition.Split('_').Length > 1)
        {
            string[] attachConditionTemp = attachCondition.Split('_');
            attachConditionType = attachConditionTemp[0];
            attachConditionValue = int.Parse(attachConditionTemp[1]);
        }
        else
        {
            attachConditionType = "n";
            attachConditionValue = 0;
        }

        ToParse(_csvData[idx++], out exp);

        if (type != CardType.Interior) return id;

        ToParse(_csvData[idx++], out fileName);
        ToParse(_csvData[idx++], out typeName);

        fileName = fileName.Trim();
        typeName = typeName.Trim();

        int layerValue = -1;
        if (fileName == "") return id;
        string value = fileName.Split('_')[2];
        if (int.TryParse(value, out layerValue))
        {
            optionValue[1] = (optionValue[1] == 0) ? layerValue : optionValue[1];
        }

        setNumber = (id % 1000) / 100;

        switch(setNumber)
        {
            case 0 :
                atlasName = "MyRoom_Base";
                break;
            case 1:
                atlasName = "MyRoom_PartyRoom";
                break;
            case 2:
                atlasName = "MyRoom_Glamping";
                break;
            case 3:
                atlasName = "MyRoom_Cafe";
                break;
        }

        string scaleText;
        ToParse(_csvData[idx++], out scaleText);
        scaleText = scaleText.Trim();
        string[] scaleTextSplit = scaleText.Split('_');
        if (scaleText == "" || scaleTextSplit.Length != 3)
        {
            scaleValue = new Vector3(1, 1, 1);
        }
        else
        {
            scaleValue.x = float.Parse(scaleTextSplit[0]);
            scaleValue.y = float.Parse(scaleTextSplit[1]);
            scaleValue.z = float.Parse(scaleTextSplit[2]);
        }

        return id;
	}

	internal int GetIliustSpriteKey() { return imageID; }
	internal int GetCardSpriteKey() { return imageID + 10000; }

    internal bool IsRandomBox() { return (subType == ItemSubType.Char_Rand_Box ||
                                          subType == ItemSubType.Item_Rand_Box ||
                                          subType == ItemSubType.Interior_Rand_Box); }

	internal string GetSubTypeString()
	{
		switch (subType)
		{
			case ItemSubType.None:
			case ItemSubType.Money:					return "골드";
			case ItemSubType.Cash:					return "캐시";
			case ItemSubType.Char_Rand_Box:		    return "캐릭터랜덤뽑기";
			case ItemSubType.Item_Rand_Box:		    return "아아템랜덤뽑기";
            case ItemSubType.Interior_Rand_Box:		return "가구랜덤뽑기";
			case ItemSubType.EquipItem:				return "장비아이템";
			case ItemSubType.Head:					return "머리";
			case ItemSubType.Body:					return "몸";
			case ItemSubType.Weapon:				return "무기";
			case ItemSubType.shoes:					return "신발";
			case ItemSubType.Accessories:			return "엑세서리";
			case ItemSubType.Interior:				return "인테리어";
			case ItemSubType.Furniture:				return "가구";
			case ItemSubType.Prop:					return "소품";
			case ItemSubType.Wall:					return "벽지";
			case ItemSubType.Floor:					return "바닥재";
			default:								return "NONE";
		}
	}

	internal int GetDefPower()
	{
		float result = 0;
		for (int i = 0; i < optionValue.Length; ++i)
		{
			if (optionValue[i] <= 0)
				break;
			result += optionValue[i];
		}

		return (int)(result*3);
	}

    internal string GetCharTypeString()
    {
        return GetStrType(equipLimit);
    }
}
