using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;
using UnityEngine.U2D;
using IDH.MyRoom;

internal class PvPBattlePara : BattlePara
{
    // PvP Datas
    public ResultPvPUI.From from;
    public int plyPower;
    public int oppPower;
    public long historyUID; // Revenge일경우 UID. 아닐경우 -1
    internal PvPSData opponentData;
    internal List<PvPOppUnitSData> charList;
    internal List<MyRoomObjectData> interierList;

    internal static PvPBattlePara CreatePvPPara(ResultPvPUI.From _from, PvPSData _oppSData, List<PvPOppUnitSData> _oppUnitList, List<MyRoomObjectData> _InteriorList, int _plyPower, int _oppPower, long _revengeUID, List<StatInfos> _unitStatInfo)
    {
        return new PvPBattlePara()
        {
            type = InGameType.PvP,
            playerTeam = CommonType.TEAM_PVP_IDX,
            from = _from,
            plyPower = _plyPower,
            oppPower = _oppPower,
            historyUID = _revengeUID,
            opponentData = _oppSData,
            charList = _oppUnitList,
            interierList = _InteriorList,
            unitStatInfosList = _unitStatInfo
        };
    }
}

internal class PvPBattleSys : BattleSysBase
{
    private MyRoom buildedRoom;
    public MyRoom BuildedRoom { get { return buildedRoom; } }
    public PvPBattleSys() : base(SubSysType.PvPBattle)
    {
        preloadingBundleKeys = new int[]
        {
            CommonType.DEF_KEY_BGM_PVP,
            CommonType.DEF_KEY_SFX_BATTLE,
            CommonType.DEF_KEY_SFX_SKILL_VOICE_P,
            CommonType.DEF_KEY_SFX_SKILL_VOICE_T,
            CommonType.DEF_KEY_SFX_SKILL_P,
            CommonType.DEF_KEY_SFX_SKILL_T,
        };
    }

    protected override void Init()
    {
        var battlePara = para.GetPara<PvPBattlePara>();
        GameCore.Instance.SoundMgr.SetBGMSound(BGMType.BMSND_PVP, true, false);
        //GameCore.Instance.SndMgr.PlayBGM(BGM.PVP_Battle);
        var bg_Ids = new int[3]
        {
                99001,
                99501,// defualt wall
                99502 // defualt Ground
        };
        targetCount = 1;
        // Init Unit
        unitPool.Init(battlePara, battlePara.charList);
        buildedRoom = MyRoomSys.BuildMyRoom(ref battlePara.interierList);
        buildedRoom.Root.transform.position = new Vector3(0, 0, -2.5f);
        buildedRoom.Root.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

        List<MyRoomObject> listObject = buildedRoom.MyRoomObjectList;
        for(int i = 0; i < listObject.Count; i ++)
        {
            string objectTypeName = listObject[i].ObjectTypeName;

            switch (objectTypeName)
            {
                case MyRoomObject.TYPE_FURNITURE:
                    listObject[i].Sprite_Renderer.sortingOrder = -1;

                    //buildedRoom.WallTileMap.SetPlace(listObject[i].)
                    //listObject[i].Place(buildedRoom.WallTileMap.TileMap[,])

                    //var tile = buildedRoom.WallTileMap.GetTileOrNull(listObject[i].PlacedTile.X, buildedRoom.WallTileMap.LengthY - 1);
                    //var tile = buildedRoom.FloorTileMap.GetTileOrNull(listObject[i].PlacedTile.X, buildedRoom.WallTileMap.LengthY - 1);
                    //listObject[i].AttachedObject.transform.position = tile.Trans.position;
                    Vector3 objectPos = listObject[i].AttachedObject.transform.position;
                    objectPos.z = 2.8f - buildedRoom.WallTileMap.LengthY * 0.1f;
                    objectPos.y = 0;
                    listObject[i].AttachedObject.transform.position = objectPos;
                    break;
                case MyRoomObject.TYPE_WALL:
                    listObject[i].Sprite_Renderer.sortingOrder = -1000;
                    break;

                case MyRoomObject.TYPE_FLOOR:
                    listObject[i].Sprite_Renderer.sortingOrder = -999;
                    break;
                default:
                    listObject[i].Sprite_Renderer.sortingOrder = -2;
                    break;
            }
        }

        //GameCore.atuoPlay = true;

        //buildedRoom.Destroy();
        /*
        for (int i = 0; i < battlePara.interierList.Count; i++)
        {
            // 0하늘, 1벽, 2바닥, 3프론트
            // 23벽, 24바닥
            PvPRoomSData baseItem = battlePara.interierList[i];

            if (baseItem.TYPE == 23)
                bg_Ids[1] = GameCore.Instance.DataMgr.GetItemData(baseItem.ITEM_ID).inGameImageID;
            else if (baseItem.TYPE == 24)
                bg_Ids[2] = GameCore.Instance.DataMgr.GetItemData(baseItem.ITEM_ID).inGameImageID;
        }
        */
        battleField = PvPBattleField.Create(GameCore.Instance.world_root);
        battleField.Init(bg_Ids, true);
        ui = PvPBattleUI.Create(GameCore.Instance.ui_root);
        if(battlePara.opponentData.userName == null) battlePara.opponentData.userName = "악의 조직 나이프";
        ui.Init(battlePara, battlePara.opponentData);
        GameCore.Instance.TimeSave = PlayerPrefs.GetInt("TimeScaleValue", 1);
        ui.SetSpeedButton(GameCore.Instance.TimeScaleChange);

        //Instantiate Unit
        for (int i = 0; i <= targetCount; ++i)
        {
            var tf = InstantiateTeam(i);
            if (i == 0)
                battleField.SetMoveTeam(tf);
            else
                battleField.SetEnemyTeam(tf);
        }
        ui.ShowAllStatusBar(false);

        canUseSkillEnmey = true;

        SubSysBase.CheckSuddenQuit.SetSuddenQuitData(CheckSuddenQuit.SuddenQuitSysType.PvpBattle, () =>
        {
            ui.ReturnSpeed();
            BuildedRoom.Destroy();
        });
        //CreateInterier(battlePara.interierList);
    }

    protected override void ExitSysInner(ParaBase _para)
    {
        buildedRoom.Destroy();
        base.ExitSysInner(_para);
    }


    protected override void EndBattle()
    {
        GameCore.Instance.CloseMsgWindow();
        var battlePara = para.GetPara<PvPBattlePara>();

        var resultPara = new ResultPvPPara()
        {
            type = para.type,
            clear = playerWin,
            myCombat = battlePara.plyPower,
            oppCombet = battlePara.oppPower,
            playerTeamIdx = CommonType.TEAM_PVP_IDX,
            playTime = ui.GetPlayTime(),
            historyUID = battlePara.historyUID,
            from = battlePara.from
        };
        GameCore.atuoPlay = false;
        GameCore.Instance.ChangeSubSystem(SubSysType.Result, resultPara);
    }
    internal override void UpdateUI()
    {
        base.UpdateUI();
        SubSysBase.CheckSuddenQuit.SuddenQuit();
    }
    void CreateInterier(List<PvPRoomSData> _list)
    {
        
    }
}