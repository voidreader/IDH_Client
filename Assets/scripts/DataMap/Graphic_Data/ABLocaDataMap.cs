using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 에셋번들 관리용 데이터들(저장경로 및 타입, 생명주기)
/// </summary>
internal class ABLocaDataMap : CSVParse
{
	internal string file;         // 에셋번들의 파일 이름
	internal ABType type;         // 에셋번들의 데이터 종류
	internal string name;         // 에셋번들의 오브젝트 이름
	internal int LifeType;				// 에셋번들의 생면주기.(0 : 수동, -1 : 영구)

	internal override int SetData(string[] _csvData)
	{
		int idx = 0;
		ToParse(_csvData[idx++], out id);

		switch(_csvData[idx++].ToLower())
		{
			case "prefab": type = ABType.AB_Prefab; break;
			case "atlas" : type = ABType.AB_Atlas; break;
			case "texture": type = ABType.AB_Texture; break;
			case "audio": type = ABType.AB_Audio; break;
            case "unityatlas": type = ABType.AB_UnityAtlas; break;
            case "battlestory": type = ABType.AB_BattleStory; break;
            case "text": type = ABType.AB_CSV; break;
            // Todo : 에셋번들 종류 추가 (타입파싱)

            default: Debug.LogError("에셋번들 타입 파싱 실패."); type = ABType.AB_None; return -1; // failed;
		}

		ToParse(_csvData[idx++], out file);
		ToParse(_csvData[idx++], out name);
		ToParse(_csvData[idx++], out LifeType);

		return id;
	}
}
