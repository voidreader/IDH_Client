using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class SetFurnitureSetDataMap : CSVParse
{
	internal int groupID;
	internal int itemId;
	internal int itemAttach;  // 0 : None, 1: Attach Floor, 2: Attach Wall
	internal int itemPosX;
	internal int itemPosY;

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out groupID);
		ToParse(_csvData[idx++], out itemId);
		idx++;// 이름 건너뜀
		ToParse(_csvData[idx++], out itemAttach);
		ToParse(_csvData[idx++], out itemPosX);
		ToParse(_csvData[idx++], out itemPosY);

		return id;
	}
}
