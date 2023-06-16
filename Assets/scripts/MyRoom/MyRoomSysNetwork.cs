using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDH.MyRoom
{
    internal partial class MyRoomSys
    {
        public void Send_PlaceMyRoomObjectData(MyRoomObjectData obj)
        {
            GameCore.Instance.NetMgr.Req_MyRoomBuildBase(obj);
        }

        public void Send_ReturnToInventoryObject(MyRoomObject obj)
        {
            GameCore.Instance.NetMgr.Req_MyRoomItemRemove(obj.MyRoomServerData);

        }

        public void Send_PlaceMyRoomHero(List<MyRoomHeroObject> heroList)
        {
            List<int> heroIdList = new List<int>();

            for (int i = 0; i < heroList.Count; ++i)
            {
                heroIdList.Add((int)heroList[i].HeroData.ServerData.uid);
            }

            GameCore.Instance.NetMgr.Req_MyRoomCharArrangement(MyRoomCursor.ID, heroIdList);
        }

        public void Send_PlaceMyRoomHero(int uid)
        {
            List<int> heroIdList = new List<int>();

            heroIdList.Add(uid);

            GameCore.Instance.NetMgr.Req_MyRoomCharArrangement(MyRoomCursor.ID, heroIdList);
        }

        public void Send_ReturnToInventoryHero(List<MyRoomHeroObject> heroList)
        {
            if (heroList == null || heroList.Count == 0) return;

            List<int> heroIdList = new List<int>();

            for (int i = 0; i < heroList.Count; ++i)
            {
                heroIdList.Add((int)heroList[i].HeroData.ServerData.uid);
            }

            GameCore.Instance.NetMgr.Req_MyRoomCharUnArrangement(MyRoomCursor.ID, heroIdList);
        }

        public void Send_ReturnToInventoryHero(List<HeroSData> heroSdataList)
        {
            List<int> heroIdList = new List<int>();

            for (int i = 0; i < heroSdataList.Count; ++i)
            {
                heroIdList.Add((int)heroSdataList[i].uid);
            }

            GameCore.Instance.NetMgr.Req_MyRoomCharUnArrangement(heroSdataList[0].dormitory, heroIdList);
        }

        public void Send_CleanStain(MyRoomStainData data)
        {
            GameCore.Instance.NetMgr.Req_MyRoomStainCleanStart(data);
        }

        public void Send_TakePresent(MyRoomStainData data)
        {
            GameCore.Instance.NetMgr.Req_MyRoomTakePresent(data);
        }

        public void Send_SelectMainRoom(int index)
        {
            GameCore.Instance.NetMgr.Req_MyRoomSelectMainRoom(index);
        }

        internal bool ANS_MYROOM_BUILD(ParaBase _para)
        {
            var json = _para.GetPara<PacketPara>().data.data;
            Debug.Log(json);

            int result = 0;
            json.GetField(ref result, "result");

            switch (result)
            {
                case 0:
                    MyRoomObjectData placedItemData = new MyRoomObjectData();
                    GameCore.Instance.PlayerDataMgr.SetCardSData(json.GetField("ITEM"));
                    MyRoomUI.UpdateInventory();

                    var jsonItemData = json.GetField("ITEM_LIST");
                    jsonItemData.GetField(ref placedItemData.ItemId, "ITEM_ID");
                    jsonItemData.GetField(ref placedItemData.ItemUniqueid, "ITEM_UID");
                    jsonItemData.GetField(ref placedItemData.UsedRoomId, "MYROOM_ID");
                    jsonItemData.GetField(ref placedItemData.MyRoomUniqueid, "MYROOM_ITEM_UID");

                    JSONObject positionDataList = jsonItemData.GetField("POSITION");

                    Vector2 position = Vector2.zero;
                    positionDataList[0].GetField(ref position.x, "x"); //x
                    positionDataList[0].GetField(ref position.y, "y"); //y
                    placedItemData.vectorList.Add(position);

                    Vector2 flip = Vector2.zero;
                    positionDataList[1].GetField(ref flip.x, "x"); //x
                    positionDataList[1].GetField(ref flip.y, "y"); //y
                    placedItemData.vectorList.Add(flip);

                    placedItemData.SeverData = GameCore.Instance.PlayerDataMgr.GetItemSData((long)placedItemData.ItemUniqueid);
                    placedItemData.LocalData = GameCore.Instance.PlayerDataMgr.GetItemData((long)placedItemData.ItemUniqueid);

                    if (position.x == 999)
                    {
                        Vector2 center = SelectMapCursor(placedItemData.LocalData.typeName).Center;
                        placedItemData.vectorList[0] = center;
                        MyRoomCursor.PlacedObjectList.Add(placedItemData);
                        var spawnedObject = SpawnObject(placedItemData);

                        if (Observer.OnEndMyRoomObjectEditMode != null) Observer.OnEndMyRoomObjectEditMode.Invoke(SelectedObject);
                        if (Observer.OnPlacedMyRoomObject != null) Observer.OnPlacedMyRoomObject.Invoke(spawnedObject);

                        if (spawnedObject.ObjectTypeName == MyRoomObject.TYPE_WALL ||
                            spawnedObject.ObjectTypeName == MyRoomObject.TYPE_FLOOR)
                        {
                            ChangeControlMode(MyRoomControlMode.SelectMyRoomObject);
                            return true;
                        }

                        //if (Observer.OnPlacedMyRoomObject != null) Observer.OnPlacedMyRoomObject.Invoke(spawnedObject);

                        if (spawnedObject.ObjectTypeName == MyRoomObject.TYPE_FLOORBOARD ||
                            spawnedObject.ObjectTypeName == MyRoomObject.TYPE_CEILING)
                        {
                            ChangeControlMode(MyRoomControlMode.SelectMyRoomObject);
                            return true;
                        }

                        SelectObject(PlacedObjectList.Find(target => target.MyRoomServerData.MyRoomUniqueid == placedItemData.MyRoomUniqueid));
                    }
                    else
                    {
                        UpdateObjectData(placedItemData);
                    }

                    return true;

                case 1:
                    string richText1 = "오류";
                    GameCore.Instance.ShowAlert(richText1);
                    return false;
                case 2:
                    string richText2 = "해당 숙소 배치 한도 초과 (최대 20개)";
                    GameCore.Instance.ShowAlert(richText2);
                    return false;
                case 3:
                    string richText3 = "숙소에 배치되어 남은 수량이 없습니다.";
                    GameCore.Instance.ShowAlert(richText3);
                    ChangeControlMode(MyRoomControlMode.SelectMyRoomObject);
                    return false;
                case 4:
                    string richText4 = "이미 배치된 아이템입니다.";
                    GameCore.Instance.ShowAlert(richText4);
                    return false;
                default:
                    return false;
            }
        }

        // 마이룸 -> 인벤토리로 전환
        internal bool ANS_MYROOM_ReturnToInventoryObject(ParaBase _para)
        {
            var json = _para.GetPara<PacketPara>().data.data;
            Debug.Log(json);

            int result = 0;
            json.GetField(ref result, "result");
            switch (result)
            {
                case 0:
                    GameCore.Instance.PlayerDataMgr.SetCardSData(json.GetField("ITEM"));
                    MyRoomUI.UpdateAllInventory();
                    break;
                case 1:
                    string richText1 = "통신오류 :: 연결상태가 좋지 않습니다";
                    GameCore.Instance.ShowAlert(richText1);
                    break;
            }

            return true;
        }



        internal bool ANS_CHARACTER_ALLOCATE(ParaBase _para)
        {
            var json = _para.GetPara<PacketPara>().data.data;
            Debug.Log(json);

            int result = -1;
            json.GetField(ref result, "result");

            switch (result)
            {
                case 0:
                    JSONObject list = json.GetField("CHA_LIST");

                    for (int i = 0; i < list.Count; ++i)
                    {
                        MyRoomHeroData heroData = new MyRoomHeroData();

                        long uid = -1;
                        int key = -1;
                        int dispatch = -1;
                        int farmingID = -1;
                        int teamID = 1;
                        int myRoomid = 1;

                        list[i].GetField(ref uid, "CHA_UID");
                        list[i].GetField(ref key, "CHA_ID");
                        list[i].GetField(ref dispatch, "DISPATCH");
                        list[i].GetField(ref farmingID, "FARMING_ID");
                        list[i].GetField(ref teamID, "TEAM");
                        list[i].GetField(ref myRoomid, "MYROOM_ID");

                        HeroSData targetHeroSdata;
                        if (ServerInventory.HeroSdataDic.TryGetValue(uid, out targetHeroSdata) == false) continue;
                        targetHeroSdata.dormitory = myRoomid;

                        heroData.ServerData = targetHeroSdata;
                        heroData.LocalData = GameCore.Instance.PlayerDataMgr.GetUnitData(uid);

                        SpawnHero(heroData, false);
                        MyRoomCursor.PlacedHeroList.Add(heroData);
                    }

                    return true;
                case 1:
                    string richText1 = "서버 데이터 오류";
                    GameCore.Instance.ShowAlert(richText1);
                    break;
                case 2:
                    string richText2 = "존재하지 않는 ID";
                    GameCore.Instance.ShowAlert(richText2);
                    break;
                case 3:
                    string richText3 = "해당 숙소 배치 한도 초과 (최대 5명)";
                    GameCore.Instance.ShowAlert(richText3);
                    break;
                case 4:
                    string richText4 = "해당 영웅은 다른방에 배치중입니다.";
                    GameCore.Instance.ShowAlert(richText4);
                    break;
            }
            StartEditMode();
            return true;
        }


        internal bool ANS_CHARACTER_UNALLOCATE(ParaBase _para)
        {
            var json = _para.GetPara<PacketPara>().data.data;
            Debug.Log(json);

            int result = -1;
            json.GetField(ref result, "result");

            switch (result)
            {
                case 0:
                    JSONObject list = json.GetField("CHA_LIST");

                    for (int i = 0; i < list.Count; ++i)
                    {
                        long uid = -1;
                        int key = -1;
                        int dispatch = -1;
                        int farmingID = -1;
                        int teamID = 1;
                        int myRoomid = 1;

                        list[i].GetField(ref uid, "CHA_UID");
                        list[i].GetField(ref key, "CHA_ID");
                        list[i].GetField(ref dispatch, "DISPATCH");
                        list[i].GetField(ref farmingID, "FARMING_ID");
                        list[i].GetField(ref teamID, "TEAM");
                        list[i].GetField(ref myRoomid, "MYROOM_ID");

                        HeroSData targetHeroSdata;
                        if (ServerInventory.HeroSdataDic.TryGetValue(uid, out targetHeroSdata) == false) continue;
                        targetHeroSdata.dormitory = myRoomid;
                        var serchData = MyRoomCursor.PlacedHeroList.Find(d => d.ServerData.uid == targetHeroSdata.uid);
                        if (serchData != null) MyRoomCursor.PlacedHeroList.Remove(serchData);
                    }

                    return true;
                case 1:
                    string richText1 = "서버 데이터 오류";
                    GameCore.Instance.ShowAlert(richText1);
                    break;
                case 2:
                    string richText2 = "존재하지 않는 ID";
                    GameCore.Instance.ShowAlert(richText2);
                    break;
            }
            return false;
        }


        internal bool ANS_MYROOM_START_CLEAN(ParaBase _para)
        {
            var json = _para.GetPara<PacketPara>().data.data;

            int result = 99;
            json.GetField(ref result, "result");

            switch (result)
            {
                case 0:
                    break;
                case 1:
                    string richText1 = "실패 ANS_MYROOM_START_CLEAN_ result 1";
                    GameCore.Instance.ShowAlert(richText1);
                    return true;
                case 2:
                    string richText2 = "마일리지 부족";
                    GameCore.Instance.ShowAlert(richText2);
                    return true;
                case 3:
                    string richText3 = "청소 진행중";
                    GameCore.Instance.ShowAlert(richText3);
                    return true;
                case 4:

                    ItemSData temp = new ItemSData();

                    var reward_ItemList = json.GetField("REWARD_ITEM_LIST");
                    //GameCore.Instance.PlayerDataMgr.SetRewardItems()

                    for (int i = 0; i < reward_ItemList.Count; ++i)
                    {
                        ItemSData rewardItem = new ItemSData();
                        reward_ItemList[i].GetField(ref rewardItem.key, "REWARD_ITEM_ID");
                        reward_ItemList[i].GetField(ref rewardItem.count, "REWARD_ITEM_COUNT");
                        int type = 0;
                        reward_ItemList[i].GetField(ref type, "ITEM_TYPE");
                        rewardItem.type = (CardType)type;

                        if (cleanAllQ.Count != 0)
                        {
                            rewardsByCleaning.Add(rewardItem);
                        }
                        else
                        {
                            GameCore.Instance.ShowReceiveItemPopup("청소 보상", "", new CardSData[] { rewardItem });
                        }
                    }
                    break;
            }

            JSONObject reward = json.GetField("REWARD");

            int myRoomID = 0;
            reward.GetField(ref myRoomID, "MYROOM_ID");
            int stainUID = 0;
            reward.GetField(ref stainUID, "STAIN_UID");
            DateTime startTime;
            JsonParse.ToParse(reward, "START_TIME", out startTime);
            DateTime endTime;
            JsonParse.ToParse(reward, "END_TIME", out endTime);

            StainObject target = StainObjectList.Find(stain => stain.StainData.UniqueId.Equals(stainUID));
            if (result == 4)
            {
                target.StainData.HelpUserId = -1;// "-1I";
                target.StainData.HelpUserName = GameCore.Instance.PlayerDataMgr.LocalUserData.Name;
            }

            target.StainData.CleanStartTime = startTime;
            target.StainData.CleanEndTime = endTime;
            if (target.StainData.HelpUserName == "" || target.StainData.HelpUserName == null)
            {
                if (InnerParameter.TargetUserID == -1) target.StainData.HelpUserName = GameCore.Instance.PlayerDataMgr.LocalUserData.Name;
                else target.StainData.HelpUserName = UserFriendDataList.Find(friend => friend.FriendUID == InnerParameter.TargetUserID).UserName;
            }

            if (InnerParameter.IsVisit)
            {
                UserFriendDataList.Find(friend => friend.FriendUID == InnerParameter.TargetUserID).StainCount--;
            }

            target.UpdateStainData(target.StainData);
            if (RefParameter.IsVisit)
                target.ChangeRemovedState();

            GameCore.Instance.PlayerDataMgr.SetCardSData(json.GetField("ITEM_LIST"));


            if (cleanAllQ.Count != 0)
            {
                cleanAllQ.Dequeue();
                if (!SendNextStartCleanStain())
                {
                    MergeRewardItemSdata(ref rewardsByCleaning);
                    GameCore.Instance.ShowReceiveItemPopup("청소 보상", "", rewardsByCleaning.ToArray());
                    rewardsByCleaning.Clear();
                }
            }


            GameCore.Instance.CommonSys.UpdateMoney();

            return true;
        }


        internal bool ANS_MYROOM_END_CLEAN(ParaBase _para)
        {
            var json = _para.GetPara<PacketPara>().data.data;
            Debug.Log(json);
            int result = 0;
            json.GetField(ref result, "result");
            if (result != 0) return false;

            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Item_Get);

            JSONObject rewardArrayList = json.GetField("REWARD");

            JSONObject rewardList = rewardArrayList.GetField("REWARD");

            for (int i = 0; i < rewardList.Count; ++i)
            {
                ItemSData temp = new ItemSData();

                rewardList[i].GetField(ref temp.key, "REWARD_ITEM_ID");
                rewardList[i].GetField(ref temp.count, "REWARD_ITEM_COUNT");
                int type = 99;
                rewardList[i].GetField(ref type, "ITEM_TYPE");
                temp.type = (CardType)type;

                GameCore.Instance.ShowReceiveItemPopup
                    ("청소 보상", "",
                    new CardSData[] { temp });

                long stainUID = -1;
                rewardList[i].GetField(ref stainUID, "STAIN_UID");
                var target = MyRoomCursor.StainDataList.Find(data => data.UniqueId.Equals(stainUID));
                if (target != null)
                {
                    MyRoomCursor.StainDataList.Remove(target);
                    target = GameCore.Instance.PlayerDataMgr.UserMyRoom.MyRoomDataList.Find(x => x.ID == MyRoomCursor.ID).StainDataList.Find(data => data.UniqueId.Equals(stainUID));
                    GameCore.Instance.PlayerDataMgr.UserMyRoom.MyRoomDataList.Find(x => x.ID == MyRoomCursor.ID).StainDataList.Remove(target);
                }
            }

            JSONObject itemList = rewardArrayList.GetField("ITEM_LIST");

            for (int i = 0; i < itemList.Count; ++i)
            {
                ItemSData temp = new ItemSData();

                itemList[i].GetField(ref temp.uid, "ITEM_UID");
                itemList[i].GetField(ref temp.equipHeroUID, "CHA_UID");
                itemList[i].GetField(ref temp.key, "ITEM_ID");
                itemList[i].GetField(ref temp.exp, "EXP");
                itemList[i].GetField(ref temp.enchant, "ENCHANT");
                float tmpCnt = 0;
                itemList[i].GetField(ref tmpCnt, "ITEM_COUNT");
                temp.count = (int)tmpCnt;
                //itemList[i].GetField(ref temp.prefixValue, "PREFIX");
                //itemList[i].GetField(ref temp.optionValue[0], "OPTIONS");
                DateTime createTime;
                JsonParse.ToParse(itemList[i], "CREATE_DATE", out createTime);
                temp.createDate = createTime;
                itemList[i].GetField(ref temp.myRoomCount, "MYROOM_ITEM_COUNT");


                ItemSData playerItem = GameCore.Instance.PlayerDataMgr.GetItemSData(temp.uid);
                playerItem.count = temp.count;
                playerItem.myRoomCount = temp.myRoomCount;
            }

            GameCore.Instance.CommonSys.UpdateMoney();
            MyRoomUI.SatisfactionInfo.UpdateValue();

            return true;
        }


        internal bool ANS_MYROOM_END_CLEAN_ALL(ParaBase _para)
        {
            var json = _para.GetPara<PacketPara>().data.data;
            Debug.Log(json);
            int result = 0;
            json.GetField(ref result, "result");
            switch (result)
            {
                case 0:
                    GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Item_Get);
                    var rewardList = GameCore.Instance.PlayerDataMgr.SetRewardItems(json.GetField("REWARD"));
                    var list = new List<CardSData>(rewardList);
                    MergeRewardItemSdata(ref list);

                    GameCore.Instance.ShowReceiveItemPopup ("청소 보상", "", list.ToArray());

                    var jsonStainUIDs = json.GetField("SUL");
                    for (int i = 0; i < jsonStainUIDs.Count; ++i)
                    {
                        int stainUID = jsonStainUIDs[i].custom_n;
                        var target = MyRoomCursor.StainDataList.Find(data => data.UniqueId.Equals(stainUID));
                        if (target != null)
                        {
                            var so = StainObjectList.Find(stainObejct => stainObejct.StainData.UniqueId.Equals(target.UniqueId));
                            so.ChangeRemovedState();
                            MyRoomCursor.StainDataList.Remove(target);
                            target = GameCore.Instance.PlayerDataMgr.UserMyRoom.MyRoomDataList.Find(x => x.ID == MyRoomCursor.ID).StainDataList.Find(data => data.UniqueId.Equals(stainUID));
                            GameCore.Instance.PlayerDataMgr.UserMyRoom.MyRoomDataList.Find(x => x.ID == MyRoomCursor.ID).StainDataList.Remove(target);
                        }
                    }
                    GameCore.Instance.CommonSys.UpdateMoney();
                    MyRoomUI.SatisfactionInfo.UpdateValue();
                    return true;

                
                case 2: GameCore.Instance.ShowNotice("실패", "완료된 청소가 없습니다.", 0); break;
                default: GameCore.Instance.ShowNotice("실패", "쿼리오류" + result, 0); break;
            }

            return true;
        }

        internal bool ANS_RequestFreindList(ParaBase _para)
        {
            var json = _para.GetPara<PacketPara>().data.data;
            Debug.Log(json);
            int result = 0;
            json.GetField(ref result, "result");
            if (result != 0)
            {
                Debug.Log(string.Format("MyRoomSys.ANS_RequestFreindList ::: result code = {0}", result));
                return false;
            }

            var friendList = json.GetField("VISIT_LIST");
            if (friendList.type != JSONObject.Type.ARRAY)
            {
                Debug.Log(string.Format("MyRoomSys.ANS_RequestFreindList ::: VISIT_LIST is not Array"));
                return false;
            }

            for (int i = 0; i < friendList.Count; ++i)
            {
                MyRoomFriendData data = new MyRoomFriendData();
                friendList[i].GetField(ref data.FriendUID, "FRIEND_UID");
                friendList[i].GetField(ref data.DelegateIconID, "DELEGATE_ICON");
                friendList[i].GetField(ref data.UserName, "USER_NAME");
                friendList[i].GetField(ref data.UserLevel, "USER_LEVEL");
                JsonParse.ToParse(friendList[i], "LOGIN_DATE", out data.LastLoginTime);
                friendList[i].GetField(ref data.OpenRoomCount, "MYROOM_CNT");
                friendList[i].GetField(ref data.MyRoomItemCount, "MYROOM_ITEM_CNT");
                friendList[i].GetField(ref data.StainCount, "STAIN_CNT");
                friendList[i].GetField(ref data.SatisfactionCount, "SATISFACTION_CNT");

                UserFriendDataList.Add(data);
            }

            return true;
        }

        internal bool ANS_RequestFreindRoomDataList(ParaBase _para)
        {
            var json = _para.GetPara<PacketPara>().data.data;
            Debug.Log(json);
            int result = 0;
            json.GetField(ref result, "result");
            if (result != 0)
            {
                GameCore.Instance.ShowNotice("마이룸", "알 수 없는 에러. " + result, 0);
                Debug.Log(string.Format("MyRoomSys.ANS_RequestFreindRoomDataList ::: result code = {0}", result));
                return false;
            }

            var myRoomList = json.GetField("MYROOM");
            if (myRoomList.type != JSONObject.Type.ARRAY)
            {
                Debug.Log(string.Format("MyRoomSys.ANS_RequestFreindRoomDataList ::: MYROOM is not Array"));
                return false;
            }

            MyRoomInfo friendMyRoom = new MyRoomInfo();
            friendMyRoom.MyRoomDataList = new List<MyRoomData>();

            for (int roomIndex = 0; roomIndex < myRoomList.Count; ++roomIndex)
            {
                MyRoomData roomData = new MyRoomData();
                roomData.PlacedObjectList = new List<MyRoomObjectData>();
                roomData.StainDataList = new List<MyRoomStainData>();
                roomData.PlacedHeroList = new List<MyRoomHeroData>();

                myRoomList[roomIndex].GetField(ref roomData.ID, "MYROOM_ID");
                myRoomList[roomIndex].GetField(ref roomData.Delegate, "DELEGATE");

                JSONObject jsonItemList = myRoomList[roomIndex].GetField("ITEM_LIST");
                if (jsonItemList == null)
                {
                    Debug.Log("MyRoomSys ::: jsonItemList ITEM_LIST is null");
                    return false;
                }

                for (int i = 0; i < jsonItemList.Count; ++i)
                {
                    MyRoomObjectData placedItemData = new MyRoomObjectData();

                    jsonItemList[i].GetField(ref placedItemData.ItemId, "ITEM_ID");
                    jsonItemList[i].GetField(ref placedItemData.ItemUniqueid, "ITEM_UID");
                    jsonItemList[i].GetField(ref placedItemData.UsedRoomId, "MYROOM_ID");
                    jsonItemList[i].GetField(ref placedItemData.MyRoomUniqueid, "MYROOM_ITEM_UID");

                    JSONObject positionDataList = jsonItemList[i].GetField("POSITION");

                    if (positionDataList.IsArray == false) Debug.Log(string.Format("MyRoomSys ::: JsonData ITEM_LIST_[{0}] POSITION data is null", i));

                    for (int k = 0; k < positionDataList.Count; ++k)
                    {
                        Vector2 temp = Vector2.zero;
                        positionDataList[k].GetField(ref temp.x, "x"); //x
                        positionDataList[k].GetField(ref temp.y, "y"); //y
                        placedItemData.vectorList.Add(temp);
                    }

                    while (placedItemData.vectorList.Count < 2)
                    {
                        placedItemData.vectorList.Add(Vector2.one);
                    }

                    placedItemData.LocalData = GameCore.Instance.DataMgr.GetItemData(placedItemData.ItemId);

                    roomData.PlacedObjectList.Add(placedItemData);
                }

                JSONObject heroDataList = myRoomList[roomIndex].GetField("CHA_LIST");

                if(heroDataList != null){
                    if (heroDataList.IsArray == false) Debug.Log(string.Format("MyRoomSys ::: JsonData CHA_LIST data is null"));

                    for (int i = 0; i < heroDataList.Count; ++i)
                    {
                        int key = 0;
                        heroDataList[i].GetField(ref key, "CHA_ID");
                        MyRoomHeroData data = new MyRoomHeroData();
                        data.LocalData = GameCore.Instance.DataMgr.GetUnitData(key);
                        data.ServerData = new HeroSData();

                        roomData.PlacedHeroList.Add(data);
                    }

                }

                JSONObject jsonStainLIst = myRoomList[roomIndex].GetField("STAIN_LIST");

                for (int i = 0; i < jsonStainLIst.Count; ++i)
                {
                    JSONObject jsonStainData = jsonStainLIst[i];

                    MyRoomStainData roomStain = new MyRoomStainData();

                    jsonStainLIst[i].GetField(ref roomStain.PlacedRoomId, "MYROOM_ID");
                    jsonStainLIst[i].GetField(ref roomStain.UniqueId, "STAIN_UID");
                    jsonStainLIst[i].GetField(ref roomStain.HelpUserId, "HELP_USER_UID");
                    jsonStainLIst[i].GetField(ref roomStain.HelpUserName, "HELP_USER_NAME");
                    jsonStainLIst[i].GetField(ref roomStain.RewardItemId, "REWARD_ITEM_ID");
                    jsonStainLIst[i].GetField(ref roomStain.RewardItemCount, "REWARD_ITEM_COUNT");
                    JsonParse.ToParse(jsonStainLIst[i], "START_TIME", out roomStain.CleanStartTime);
                    JsonParse.ToParse(jsonStainLIst[i], "END_TIME", out roomStain.CleanEndTime);

                    roomData.StainDataList.Add(roomStain);
                }

                friendMyRoom.MyRoomDataList.Add(roomData);
            }

            //friendMyRoom.TestPrintLog();

            InnerParameter.IsVisit = true;
            InnerParameter.TargetUserMyRoomInfo = friendMyRoom;
            InnerParameter.TargetMyRoom = friendMyRoom.MainRoom;

            GameCore.Instance.ChangeSubSystem(SubSysType.MyRoom, InnerParameter);

            return true;
        }

        internal bool ANS_MyroomHistoryList(ParaBase _para)
        {
            //HISTORY_UID: 1, TYPE: 1, ATTACK_USER_NAME: '금광적화', SUCCESS: 0,
            //REVENGE: 2, CREATE_TIME: '2018-09-18 11:26:03'

            var json = _para.GetPara<PacketPara>().data.data;
            Debug.Log(json);

            int result = 0;
            json.GetField(ref result, "result");

            if (result != 0) return false;

            var jsonHistoryList = json.GetField("MYROOM_HISTORY");
            if (jsonHistoryList == null || jsonHistoryList.type != JSONObject.Type.ARRAY) return false;

            for (int i = 0; i < jsonHistoryList.Count; ++i)
            {
                MyRoomHistoryLogData data = new MyRoomHistoryLogData();
                jsonHistoryList[i].GetField(ref data.HistoryUID, "HISTORY_UID");
                jsonHistoryList[i].GetField(ref data.HistoryType, "TYPE");
                jsonHistoryList[i].GetField(ref data.TargetUserUID, "ATTACK_USER_UID");
                jsonHistoryList[i].GetField(ref data.AttackUserName, "ATTACK_USER_NAME");
                jsonHistoryList[i].GetField(ref data.SUCCESS, "SUCCESS");
                jsonHistoryList[i].GetField(ref data.REVENGE, "REVENGE");
                JsonParse.ToParse(jsonHistoryList[i], "CREATE_TIME", out data.CreateTime);

                HistoryLogDataList.Add(data);
            }

            if (Observer.OnUpdatedHistoryData != null) Observer.OnUpdatedHistoryData.Invoke();

            return true;
        }

        internal bool ANS_SelectMainRoom(ParaBase _para)
        {
            var json = _para.GetPara<PacketPara>().data.data;

            int result = -1;
            json.GetField(ref result, "result");

            if (result == 0)
            {
                var findData = UserMyRoomInfo.MyRoomDataList.Find(room => room.ID == MyRoomCursor.ID);
                if (findData == null) return false;
                foreach (var room in UserMyRoomInfo.MyRoomDataList) room.Delegate = 0;
                findData.Delegate = 1;
                RefParameter.UserMainRoomID = MyRoomCursor.ID;
                string richText = string.Format("[8C1CFFFF]{0}번 숙소[B0B0B0FF]가[7E00FFFF] 대표룸[B0B0B0FF]으로 설정되었습니다.", findData.ID);
                GameCore.Instance.ShowAlert(richText);
                if (Observer.OnChangedMainRoom != null) Observer.OnChangedMainRoom.Invoke();
                return true;
            }
            else
            {
                string richText = string.Format("통신 상태 오류");
                GameCore.Instance.ShowAlert(richText);
                return true;
            }
        }

        internal bool ANS_MYROOM_Buy(ParaBase _para)
        {
            var json = _para.GetPara<PacketPara>().data.data;
            Debug.Log(json);

            int result = 0;
            json.GetField(ref result, "result");
            switch (result)
            {
                case 0:
                    GameCore.Instance.PlayerDataMgr.SetRewardItems(json.GetField("REWARD"));
                    GameCore.Instance.PlayerDataMgr.SetMyRoomData(json.GetField("MYROOM"));
                    UserMyRoomInfo = GameCore.Instance.PlayerDataMgr.UserMyRoom;
                    MyRoomUI.ActiveRoomButton(MyRoomUIController.ChangeMyRoomButton.lastBuyRoomIndex);
                    break;
                case 1:
                    string richText1 = "통신오류 :: 연결상태가 좋지 않습니다";
                    GameCore.Instance.ShowAlert(richText1);
                    break;

                case 2:
                    string richText2 = "MYROOM_ID 중복";
                    GameCore.Instance.ShowAlert(richText2);
                    break;

                case 3:
                    if (MyRoomBuyScript.needCostType == 1)
                    {
                        GameCore.Instance.ShowNotice("구매 실패", "펄이 부족합니다.", 0);
                    }
                    else
                    {
                        GameCore.Instance.ShowAgree("구매 실패", "골드가 부족합니다.\n상점으로 이동하시겠습니까?",0,  () => {
                            GameCore.Instance.CloseMsgWindow();
                            GameCore.Instance.ChangeSubSystem(SubSysType.Shop, null);
                        });
                    }
                    break;
            }

            return true;
        }
    }
}

