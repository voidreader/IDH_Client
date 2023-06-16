using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class StaminaCommonDataMap : CSVParse
{
	internal int defMaxVigor;
	internal int defVigor; // Never Use (for Server)
	internal int addMaxVigor;
	internal int createVigorTime; // Never Use (for Server)


    internal override int SetData(string[] _csvData)
	{
		id = 1;

		int idx = 0;
		ToParse(_csvData[idx++], out defMaxVigor);
		ToParse(_csvData[idx++], out defVigor);
		ToParse(_csvData[idx++], out addMaxVigor);
		ToParse(_csvData[idx++], out createVigorTime);

        return id;
	}
}
