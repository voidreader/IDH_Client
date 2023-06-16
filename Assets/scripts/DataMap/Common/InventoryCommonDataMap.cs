using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


internal class InventoryCommonDataMap : CSVParse
{
	internal int defSlot;               // 최초 슬롯수
	internal int[] expendSlotCost;      // 슬롯확장시 드는 비용( 1~10)																								// 10
	internal int[] expendSlotCount;     // 슬롯 확장시 추가되는 슬롯 개수(1~10)																				// 10

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

		return id;
	}
}
