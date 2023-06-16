using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 단일데이터이기 때문에 고정
internal class MyRoomRandomBoxDataMap : CSVParse
{
	internal int[] itemId;
	internal int[] valueMin, valueMax;
	internal int[] probabillity;

	int totalProbabillty;
	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		id = 1;
		var count = 5;
		itemId = new int[count];
		valueMin = new int[count];
		valueMax = new int[count];
		probabillity = new int[count];
		totalProbabillty = 0;
		for (int i = 0; i < count; ++i)
		{
			ToParse(_csvData[idx++], out itemId[i]);
			ToParse(_csvData[idx++], out valueMin[i]);
			ToParse(_csvData[idx++], out valueMax[i]);
			ToParse(_csvData[idx++], out probabillity[i]);
			totalProbabillty += probabillity[i];
		}

		return id;
	}
}
