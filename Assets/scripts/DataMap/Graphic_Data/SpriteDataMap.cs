using System;
using System.Collections.Generic;

internal class SpriteDataMap : CSVParse
{
	internal int atlas_id;        // AssetBundleID of atlas 
	internal string sprite_name;  // Srptie Name

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out atlas_id);
		ToParse(_csvData[idx++], out sprite_name);

		return id;
	}

}
