using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


internal class CharacterCommonDataMap : CSVParse
{
	internal int defSlot;				// 최초 슬롯수
	internal int[] expendSlotCost;		// 슬롯확장시 드는 비용( 1~10)																			// 10
	internal int[] expendSlotCount;		// 슬롯 확장시 추가되는 슬롯 개수(1~10)																	// 10

	internal int[] maxEnchant;			// 진화단계별 최대 강화	레벨																		    // 5
	internal float[] evolEnchantValue;  // 진화단계별 경험치 환산																				// 5

	internal float[] enchantLvValue;    // 강화 단계별 경험치 배율( enhactValue += rankEnchantValue * enchantLvValue )// !maxEnchant
	internal float[] needValue;         // 각 강화단계별 필요 경험치 ( [0] : +0 -> +1 )															// !maxEnchant
	internal int[] needGold;			// 각 강화단계별 필요 골드	 ( [0] : +0 -> +1 )													        // !maxEnchant

	internal int[] evolUpCost_Gold;		// 진화시(별개수 증가) 필요 골드																		// 5-1
	internal int[] evolUpCost_Coin;		// 진화시(별개수 증가) 필요 코인																		// 5-1
	internal float[] evolUpChance;      // 진화시(별개수 증가) 성공확률(백분율)																	// 5-1
	internal int[] evolUpCost_Cash;     // 캐시 진화시(별개수 증가) 필요 캐시																	// 5-1
	internal float[] evolUpChance_Cash; // 캐시 진화시(별개수 증가) 성공확률(백분율)															// 5-1

	internal float[] enchantCompensate; // 강화단계별 보정값 (stat = defStat * enchantCompensate[enchantLvl]; )				                    // !maxEnchant + 1

	internal override int SetData(string[] _csvData)
	{
		id = 1;

		int idx = 0;
		ToParse(_csvData[idx++], out defSlot);

		var maxExpendCount = 10;	

		expendSlotCost = new int[maxExpendCount];
		expendSlotCount = new int[maxExpendCount];
		for (int i = 0; i < maxExpendCount; i++)
		{
			ToParse(_csvData[idx++], out expendSlotCost[i]);
			ToParse(_csvData[idx++], out expendSlotCount[i]);
		}

		var maxEvolLv = 5;
		int maxEnchantLv = 0;

		maxEnchant = new int[maxEvolLv];
		for (int i = 0; i < maxEvolLv; i++)
		{
			ToParse(_csvData[idx++], out maxEnchant[i]);
			maxEnchantLv += maxEnchant[i];
		}
		Debug.Log("maxEnchantLv : " + maxEnchantLv);

		evolEnchantValue = new float[maxEvolLv];
		for (int i = 0; i < maxEvolLv; i++)
			ToParse(_csvData[idx++], out evolEnchantValue[i]);

		enchantLvValue = new float[maxEnchantLv];
		for (int i = 0; i < maxEnchantLv; i++)
			ToParse(_csvData[idx++], out enchantLvValue[i]);

		needValue = new float[maxEnchantLv];
		needGold = new int[maxEnchantLv];
		for (int i = 0; i < maxEnchantLv; i++)
		{
			ToParse(_csvData[idx++], out needValue[i]);
			ToParse(_csvData[idx++], out needGold[i]);
		}

		evolUpCost_Gold = new int[maxEvolLv-1];
		evolUpCost_Coin = new int[maxEvolLv-1];
		evolUpChance = new float[maxEvolLv-1];
		evolUpCost_Cash = new int[maxEvolLv-1];
		evolUpChance_Cash = new float[maxEvolLv-1];
		for (int i = 0; i < maxEvolLv-1; i++)	// 진화할때 이기때문에
		{
			ToParse(_csvData[idx++], out evolUpCost_Gold[i]);
			ToParse(_csvData[idx++], out evolUpCost_Coin[i]);
			ToParse(_csvData[idx++], out evolUpChance[i]);
			ToParse(_csvData[idx++], out evolUpCost_Cash[i]);
			ToParse(_csvData[idx++], out evolUpChance_Cash[i]);
		}

		enchantCompensate = new float[maxEnchantLv+1]; // 0도 포함하기 때문에
		for (int i = 0; i < maxEnchantLv+1; i++)
			ToParse(_csvData[idx++], out enchantCompensate[i]);

		return id;
	}
}
