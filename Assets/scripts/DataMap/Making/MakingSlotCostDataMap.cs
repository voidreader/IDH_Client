using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class MakingSlotCostDataMap : CSVParse
{
	internal int type;					// 1 영웅, 2 장비, 3 가구
	internal int number;				// 1 ~ 10. (11 - 이벤트 슬롯)
	internal int rentalCostID;	// 대여 재화 종류
	internal int rentalCost;		// 대여 비용
	internal int rentalTime;		// 대여 기간 (초)
	internal int unlockCostID;	// 구매 재화 종류
	internal int unlockCost;    // 구매 비용

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out type);
		ToParse(_csvData[idx++], out number);
		ToParse(_csvData[idx++], out rentalCostID);
		ToParse(_csvData[idx++], out rentalCost);
		ToParse(_csvData[idx++], out rentalTime);
		ToParse(_csvData[idx++], out unlockCostID);
		ToParse(_csvData[idx++], out unlockCost);

		return id;
	}
}
