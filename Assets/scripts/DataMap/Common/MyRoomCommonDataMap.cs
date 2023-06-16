using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


internal class MyRoomCommonDataMap : CSVParse
{
	internal float reduceVigor; // 스토리모드에서 소모한 행동력이 마이룸 행동력으로 바뀔때 곱해지는 값
	internal float floorVCell;  // 바닥 세로 셀 수
	internal float floorHCell;  // 바닥 가로 셀 수
	internal float costVigor;   // 청소시 소모되는 코스트
	internal float dustInterval;// 더러움 발생 가능한 간격(분단위)
	internal float dustPercent; // 더러움 발생 확률
	internal float CleaningDelay; // 청소에 소모되는 시간(초)


	internal override int SetData(string[] _csvData)
	{
		id = 1;

		int idx = 0;
		ToParse(_csvData[idx++], out reduceVigor); 
		ToParse(_csvData[idx++], out floorVCell);  
		ToParse(_csvData[idx++], out floorHCell);  
		ToParse(_csvData[idx++], out costVigor);   
		ToParse(_csvData[idx++], out dustInterval);
		ToParse(_csvData[idx++], out dustPercent); 
		ToParse(_csvData[idx++], out CleaningDelay); 

		return id;
	}
}
