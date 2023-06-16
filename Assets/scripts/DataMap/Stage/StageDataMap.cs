using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ID			bgIds		Team Count		 enmeyIds1			 enmeyIds2						enmeyIds3					...
/// 1			101,102				3					1,2,3,4,5,6	 11,12,-1,-1,11,12		-1,-1,-1,100,-1,-1		...
/// ...
///			위와 다르게 사용함!(,를 모두 셀별로 분리해서 사용. teamCount는 무조건 3으로 고정.(이전 버전과 호환 되도록 유지만 함)
/// </summary>
internal class StageDataMap : CSVParse
{
	public int[]	bg_Ids;  // sprite IDs(차래대로 생성되어 루핑된다.)
	public int		teamCnt;                // 적팀 개수
	public int[,] monster_Ids;		// monster IDs(그대로 배치됨)

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);

		// Background Ids
		//var bgIds = _csvData[idx++].Split(new char[] { ',' });
		bg_Ids = new int[4];
		for (int i = 0; i < bg_Ids.Length; ++i)
			ToParse(_csvData[idx++], out bg_Ids[i]);

		// Team Count
		//ToParse(_csvData[idx++], out teamCnt);
		teamCnt = 3;

		// Enemy Ids
		monster_Ids = new int[teamCnt, 6];
		for( int i = 0; i < monster_Ids.GetLength(0); ++i )
		{
			//var monIds = _csvData[idx++].Split(new char[] { ',' });
			for (int j = 0; j < monster_Ids.GetLength(1); j++)
			{
				//ToParse(monIds[j], out monster_Ids[i, j]);
				ToParse(_csvData[idx++], out monster_Ids[i, j]);

				if(monster_Ids[i,j] == 1010516)
				{
					Debug.Log(" >>>>> " + id);
				}
			}
				
		}

		return id;
	}
}
