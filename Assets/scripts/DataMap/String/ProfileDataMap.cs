using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class ProfileDataMap : CSVParse
{
    internal string belong;
    internal string name;
    internal string @class;
    internal string ability;
    internal string character;
    internal string feature;
    internal string desc;
    internal string[] dialogues;

    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out belong);
        ToParse(_csvData[idx++], out name);
        ToParse(_csvData[idx++], out @class);
        ToParse(_csvData[idx++], out ability);
        ToParse(_csvData[idx++], out character);
        ToParse(_csvData[idx++], out feature);
        ToParse(_csvData[idx++], out desc);
        dialogues = new string[3];
        for(int i = 0; i < 3; i++)
        {
            ToParse(_csvData[idx++], out dialogues[i]);
        }
        return id;
    }
}