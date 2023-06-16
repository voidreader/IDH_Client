using System;
using System.Collections.Generic;
internal abstract class CardDataMap : CSVParse
{
	internal string name;
	internal int rank;
	internal CardType type;

	internal int imageID;

	internal static string GetStrRank(int _rank)
	{
		switch(_rank%10)
		{
			case 0: return "SSS";
			case 1: return "SS";
			case 2: return "S";
			case 3: return "A";
			case 4: return "B";
			default: return "";
		}
	}

	internal static string GetStrType(int _type)
	{
		switch(_type%10)
		{
			case 0: return "무속성";
			case 1: return "방어형";
			case 2: return "근접형";
			case 3: return "마법형";
			case 4: return "원거리형";
			case 5: return "지원형";
			default: return "";
		}
	}


	internal static bool IsItemKey(int _key) { return (_key / 1000000) == 3; }
	internal static bool IsUnitKey(int _key) { return (_key / 1000000) == 1; }
	//internal static int GetSmallCardSpriteKey(int _key)	{	return _key + 2000;	}
	//internal static int GetBattleCardSpriteKey(int _key) { return _key + 30000; }

	internal static string GetTypeSpriteName(int _charType)
	{	return "ICON_TYPE_01_" + _charType.ToString("00") + "_S";	}

	internal static string GetRankSpriteName(int _rank)
	{ return "ICON_LV_" + (5-_rank).ToString("00") + "_S"; }
}

