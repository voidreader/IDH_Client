using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class TutorialDataMap : CSVParse {

    internal int index;
    internal int type;
    //internal bool isFirstSceneData;
    internal string title;
    internal int titleIndex;
    internal string value;
    internal int characters;
    internal int emotions;
    internal int sizeX;
    internal int sizeY;
    internal int centerX;
    internal int centerY;


    internal override int SetData(string[] _csvData)
    {
        int idx = 0;
        ToParse(_csvData[idx++], out index);
        ToParse(_csvData[idx++], out id);
        //int checkFirstSceneData;
        //ToParse(_csvData[idx++], out checkFirstSceneData);
        //isFirstSceneData = checkFirstSceneData == 1;
        ToParse(_csvData[idx++], out type);
        ToParse(_csvData[idx++], out title);
        ToParse(_csvData[idx++], out titleIndex);
        ToParse(_csvData[idx++], out value);
        ToParse(_csvData[idx++], out characters);
        ToParse(_csvData[idx++], out emotions);
        ToParse(_csvData[idx++], out sizeX);
        ToParse(_csvData[idx++], out sizeY);
        ToParse(_csvData[idx++], out centerX);
        ToParse(_csvData[idx++], out centerY);

        return id;
    }
}
