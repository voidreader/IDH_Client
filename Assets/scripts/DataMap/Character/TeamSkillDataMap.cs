using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class TeamSkillDataMap : CardDataMap
{
    internal int skillNameNum;
    internal string disc;
	internal int[] needChar;
    internal int[] charSkill;
    internal string info;

    internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out name);
		ToParse(_csvData[idx++], out rank);
		if (rank != 99) rank %= 10;
        ToParse(_csvData[idx++], out skillNameNum);
        type = (CardType)7;
		ToParse(_csvData[idx++], out imageID);
		ToParse(_csvData[idx++], out disc);

		List<int> needCharTmp = new List<int>();
        List<int> skillTmp = new List<int>();
        int tmp;
        int tmpSkill;
		for (int i = 0; i < 5; ++i)
		{
			ToParse(_csvData[idx++], out tmp);
            ToParse(_csvData[idx++], out tmpSkill);
            if (0 < tmp)
                needCharTmp.Add(tmp);
            if (0 < tmpSkill)
                skillTmp.Add(tmpSkill);

        }
        charSkill = skillTmp.ToArray();
        needChar = needCharTmp.ToArray();

        if (_csvData.Length > idx)
            ToParse(_csvData[idx++], out info);


        return id;
	}
}
