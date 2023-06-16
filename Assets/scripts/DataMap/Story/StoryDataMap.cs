using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class StoryDataMap : CSVParse
{
	internal int chapter;
	internal int stage;
	internal int difficult;
	internal int stageID;
	internal int openCondition;
	internal string name;
	internal int powerRecommand;

	// GoldMin, GlodMax

	internal int clearExp;
	internal int rewardId;
	internal int ovkValue; // 오버킬 기준치
	internal int ovkReward;

	internal int[] missionId;
	internal int[] missionValue1;
    internal int[] missionValue2;

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);
		ToParse(_csvData[idx++], out chapter);
		ToParse(_csvData[idx++], out stage);
		ToParse(_csvData[idx++], out difficult);
		ToParse(_csvData[idx++], out stageID);
		ToParse(_csvData[idx++], out openCondition);
		ToParse(_csvData[idx++], out name);
		ToParse(_csvData[idx++], out powerRecommand);

		idx += 2;// GoldMin, GlodMax

		ToParse(_csvData[idx++], out clearExp);
		ToParse(_csvData[idx++], out rewardId);
		ToParse(_csvData[idx++], out ovkValue);
		ToParse(_csvData[idx++], out ovkReward);

		missionId = new int[3];
		missionValue1 = new int[3];
        missionValue2 = new int[3];
		for(int i = 0; i < 3; ++i)
		{
			ToParse(_csvData[idx++], out missionId[i]);
			ToParse(_csvData[idx++], out missionValue1[i]);
            ToParse(_csvData[idx++], out missionValue2[i]);
		}

		return id;
	}


	internal static int GenerateStoryKey(int _chap, int _diff, int _stageNum)
	{
		// Generate StageInfo Key
		int key = 7000000;
		key += (_chap - 1) * 10;
        key += (_diff - 1) * 100000;
        key += _stageNum;

		return key;
	}

	internal static int GenerateNextStoryKey(int _chap, int _diff, int _stageNum)
	{
		if( _stageNum == 10 )
		{
			if (_chap < GameCore.Instance.DataMgr.GetStoryConstData().maxChapter)
				return GenerateStoryKey(_chap + 1, _diff, 1);
			else
			{
				if (_diff != 3)
					return GenerateStoryKey(1, _diff + 1, 1);
				else
					return -1; // 불가
			}
		}
		else
		{
			return GenerateStoryKey(_chap, _diff, _stageNum+1);
		}
	}

    internal static string GetStrDiffcult(int _diff)
    {
        switch(_diff)
        {
            case 1: return "보통";
            case 2: return "어려움";
            case 3: return "매우 어려움";
            default: return "InValid";
        }
    }

    internal static string GetStageName(int _storyKey)
    {
        var data = GameCore.Instance.DataMgr.GetStoryData(_storyKey);
        if (data == null)
            return string.Empty;

        return string.Format("{0}-{1} {2}({3})", data.chapter, data.stage, data.name, GetStrDiffcult(data.difficult));
    }
}
