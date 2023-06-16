using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class BattleChapterDataMap : CSVParse {

                                            
    internal string index;                     //index (대사 순번 정보를 id값으로 입력)
    internal int front;                     //전후 구분 0-앞, 1-뒤
    internal string TitleCharacter;         //대사 타이틀 캐릭터
    internal string mainCharacterTexture;             //sprite 파일 경로
    internal string Dialogue;               //대사
    internal string backGroundPath;         //뒷배경 파일 경로
    internal string[] spritePresentCharacterPath; //현 화면에 있어야 할 이미지 경로
    internal string spriteCartoonPath;            //카툰식 이미지가 있을경우


    internal override int SetData(string[] _csvData)
    {

        int idx = 0;
        ToParse(_csvData[idx++], out id);
        ToParse(_csvData[idx++], out index);
        ToParse(_csvData[idx++], out front);
        ToParse(_csvData[idx++], out TitleCharacter);
        ToParse(_csvData[idx++], out mainCharacterTexture);
        ToParse(_csvData[idx++], out Dialogue);
        ToParse(_csvData[idx++], out backGroundPath);

        var count = 3;
        spritePresentCharacterPath = new string[count];
        ToParse(_csvData[idx++], out spritePresentCharacterPath[0]);
        ToParse(_csvData[idx++], out spritePresentCharacterPath[1]);
        ToParse(_csvData[idx++], out spritePresentCharacterPath[2]);

        ToParse(_csvData[idx++], out spriteCartoonPath);
        return id;
    }
}
