using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class GachaDataMap : CSVParse
{
	internal int itemType; // 1: 영웅, 2: 장비, 3:가구
	internal int group;
	internal int countType; // 0: 무료, 1: 1회, 2: 10회
	internal string name;
	internal int waitTime; // 무료뽑기에서만 사용되는 사용 대기 시간
	internal int costItemID; // 사용 재화
	internal int costValue; // 원가
	internal float discount; // 할인율
	internal int lastCostValue; // 최종 지불 가격

	internal int reward1ID;
	internal int reward1Count;
	internal int reward2ID;
	internal int reward2Count;

	internal int imgID;
	internal int discID;

    internal int btnColor; // 버튼 컬러

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out itemType);
		ToParse(_csvData[idx++], out group);
		ToParse(_csvData[idx++], out countType);
		ToParse(_csvData[idx++], out name);
		ToParse(_csvData[idx++], out waitTime);
		ToParse(_csvData[idx++], out costItemID);
		ToParse(_csvData[idx++], out costValue);
		ToParse(_csvData[idx++], out discount);
		ToParse(_csvData[idx++], out lastCostValue);
		ToParse(_csvData[idx++], out reward1ID);
		ToParse(_csvData[idx++], out reward1Count);
		ToParse(_csvData[idx++], out reward2ID);
		ToParse(_csvData[idx++], out reward2Count);
		ToParse(_csvData[idx++], out imgID);
		ToParse(_csvData[idx++], out discID);
        ToParse(_csvData[idx++], out btnColor);

        return id;
	}
}
