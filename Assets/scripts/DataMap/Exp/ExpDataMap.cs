using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class ExpDataMap :CSVParse
{
	internal int accExp;
	internal int gold;
	internal int cash;
	internal int itemID;
	internal int itemValue;
	internal bool refeel; // 행동력 풀충전

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id); // == USER LEVEL
		ToParse(_csvData[idx++], out accExp);
		ToParse(_csvData[idx++], out gold);
		ToParse(_csvData[idx++], out cash);
		ToParse(_csvData[idx++], out itemID);
		ToParse(_csvData[idx++], out itemValue);
		ToParse(_csvData[idx++], out refeel);

		return id;
	}
}
