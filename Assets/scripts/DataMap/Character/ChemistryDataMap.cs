using System;
using System.Collections.Generic;


class ChemistryDataMap : CSVParse
{
	internal int tgId;							// 버프를 받는 유닛의 아이디
	internal int needId;						// 버프를 받는데 필요한 유닛 아이디
	internal int[] teamBuffIds;			// 필요한 유닛이 한 팀일때 받는 버프들
	internal int[] oppositeBuffIds; // 필요한 유닛이 적 팀일때 받는 버프들

    internal string nameID;
    internal string buffStringID;
    internal string debuffStringID;

    internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out tgId);
		ToParse(_csvData[idx++], out needId);

		//var buffIds = _csvData[idx++].Split(new char[] { ',' });
		teamBuffIds = new int[3];
		for (int i = 0; i < teamBuffIds.Length; ++i)
			ToParse(_csvData[idx++], out teamBuffIds[i]);

		//buffIds = _csvData[idx++].Split(new char[] { ',' });
		oppositeBuffIds = new int[3];
		for (int i = 0; i < oppositeBuffIds.Length; ++i)
			ToParse(_csvData[idx++], out oppositeBuffIds[i]);

        ToParse(_csvData[idx++], out nameID);
        //ToParse(_csvData[idx++], out buffStringID);
        //ToParse(_csvData[idx++], out debuffStringID);

        return id;
	}
}


