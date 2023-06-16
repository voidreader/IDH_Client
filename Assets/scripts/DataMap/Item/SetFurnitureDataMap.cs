using System;
using System.Collections.Generic;
class SetFurnitureDataMap : CSVParse
{
	internal string name;
	internal int discID;
	internal int imgID;

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out name);
		ToParse(_csvData[idx++], out discID);
		ToParse(_csvData[idx++], out imgID);

		return id;
	}
}
