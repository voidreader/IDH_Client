using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System.Security.Cryptography;
using System.Text;
using IDH.MyRoom;



internal class PacketPara : ParaBase
{
	internal SocketIOEvent data;

	internal PacketPara(SocketIOEvent _data)
	{
		data = _data;
	}
}

internal class GameNetworkMgr : IEventHandler
{

	#region ####################암호화 관련############################



	////////암호화////////
	public static byte[] preSharedKey;//74fafe28";
    public static byte[] preSharedIv;
    private static byte[] finalKey; // 사용할 비밀키


	private int ck;         // 1 ~ 8의 랜덤키 제너레이트
	private int passwd;  // 서버에 전송할 공개키
	private int fk;      // 비밀키


	public static string MD5Hash(string str)
	{
		MD5 md5 = new MD5CryptoServiceProvider();
		byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(str));
		StringBuilder stringBuilder = new StringBuilder();
		foreach (byte b in hash)
		{
			stringBuilder.AppendFormat("{0:x2}", b);
		}
		Debug.Log(stringBuilder.ToString());
		return stringBuilder.ToString();
	}


	public static byte[] ToByte(string _str)
	{
		return Encoding.ASCII.GetBytes(_str);
	}


    internal static string CustomDecrypt(string _text)
	{
		using (var rijndaelManaged =
						new RijndaelManaged { Key = preSharedKey, IV = preSharedIv, Mode = CipherMode.CBC })
		{
			rijndaelManaged.BlockSize = 128;
			rijndaelManaged.KeySize = 256;
			using (var memoryStream = new MemoryStream(Convert.FromBase64String(_text)))
			using (var cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(preSharedKey, preSharedIv), CryptoStreamMode.Read))
				return new StreamReader(cryptoStream).ReadToEnd();
		}
	}


    private static string CustomEncrypt(string _text)
	{
		using (var rijndaelManaged =
						new RijndaelManaged { Key = preSharedKey, IV = preSharedIv, Mode = CipherMode.CBC })
		{

			rijndaelManaged.BlockSize = 128;
			rijndaelManaged.KeySize = 256;

            //finalKey = ToByte("01234567890123456789012345678901");

			using (var memoryStream = new MemoryStream())
			using (var cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateEncryptor(preSharedKey, preSharedIv), CryptoStreamMode.Write))
			{
				var bytes = Encoding.UTF8.GetBytes(_text); //Convert.FromBase64String(_text);
				cryptoStream.Write(bytes, 0, bytes.Length);
				cryptoStream.FlushFinalBlock();

				return Convert.ToBase64String(memoryStream.ToArray());
			}
		}
	}



    #endregion



    #region EventHandle (ACCOUNT_LOGIN, ACCOUNT_RECONNECT, CURRENT_TIME, NOTIFY_INSPECTION)



    public bool HandleMessage(GameEvent _evt)
    {
        switch (_evt.EvtType)
        {
            case GameEventType.ANS_ACCOUNT_LOGIN:       ANS_ACCONT_LOGIN(_evt.Para.GetPara<PacketPara>().data.data);      break;
            case GameEventType.ANS_UPDATE_DEVICE:       ANS_UPDATE_DEVICE(_evt.Para.GetPara<PacketPara>().data.data);     break;
            case GameEventType.ANS_CURRENT_TIME:        ANS_CURRENT_TIME(_evt.Para.GetPara<PacketPara>().data.data);      break;
            case GameEventType.ANS_ACCOUNT_LOAD:        ANS_ACCOUNT_LOAD(_evt.Para.GetPara<PacketPara>().data.data);      break;
            case GameEventType.ANS_NOTIFY_INSPECTION:   ANS_NOTIFY_INSPECTION(_evt.Para.GetPara<PacketPara>().data.data); break;
            case GameEventType.ANS_NOTIFY_HOTTIME:      ANS_NOTIFY_HOTTIME(_evt.Para.GetPara<PacketPara>().data.data);    break;
            case GameEventType.ANS_ACCOUNT_KICK:        ANS_ACCOUNT_KICK(_evt.Para.GetPara<PacketPara>().data.data);      break;

            case GameEventType.ANS_ACCOUNT_RECONNECT:
                if (bReconnetSession)
                    GameCore.Instance.ReconnectedSession(true);
                Debug.Log("Reconnect Recive");
                ResetReqCount();
                break;
        }

        return true;
    }


    void ANS_ACCONT_LOGIN(JSONObject _data)
    {
        string rule = null;
        _data.GetField(ref rule, "rule");
        //Debug.Log("Receive RULE : " + rule);
        if (rule == null || rule == "")
            return;

        // rule Decrypt
        string strJson = CustomDecrypt(rule);
        Debug.Log("rule : " + strJson);


        #region 비대칭키 암호화 사용 코드 (현재 비사용)


        // Generate Key
        //JSONObject keyGen = new JSONObject(strJson);

        //ck = UnityEngine.Random.Range(1, 9); // 랜덤 생성값
        //int d = 0, h = 0, sr = 0;
        //keyGen.GetField(ref d, "d");
        //keyGen.GetField(ref h, "h");
        //keyGen.GetField(ref sr, "sr");

        //passwd = (int)Mathf.Pow(d, ck) % h; // 전송할 공개키
        //fk = (int)Mathf.Pow(sr, ck) % h;    // 사용할 비밀키

        //// final Key Padding
        //StringBuilder sk_str = new StringBuilder();
        //sk_str.Append(fk.ToString());

        //for (int i = sk_str.Length; i < 32; ++i)
        //    sk_str.Append("0");
        //finalKey = ToByte(sk_str.ToString());
        //Debug.Log("Final Key : " + sk_str.ToString());


        // 전송할 데이터 생성
        //			JSONObject data = new JSONObject();
        //			int money = 5000;

        //byte[] intBytes = BitConverter.GetBytes(money);
        //if (BitConverter.IsLittleEndian)
        //	Array.Reverse(intBytes);
        //SHA1 sha = new SHA1CryptoServiceProvider();
        //string sha_str = Convert.ToBase64String(sha.ComputeHash(intBytes));

        //			string sha_str = MD5Hash(money.ToString());

        //			data.AddField("money", money);
        //			data.AddField("enc", sha_str);

        //// 전송할 데이터 암호화 및 키값 전달
        //			Debug.Log(data.ToString());
        //			var send_str = CustomEncrypt(data.ToString());
        //			Debug.Log(send_str);

        //			JSONObject send = new JSONObject();
        //			send.AddField("data", send_str);
        //			send.AddField("p", passwd);

        //송신
        //	socket.Emit("message", send);


        //aesMgr = new RijndaelManaged();
        ////Debug.Log(Encoding.UTF8.GetBytes(preSharedKey).Length);
        //aesMgr.Key = Encoding.UTF8.GetBytes(preSharedKey);
        //aesMgr.IV = Encoding.UTF8.GetBytes(preSharedKey);
        //var decyptor = aesMgr.CreateDecryptor(aesMgr.Key, aesMgr.IV);

        //byte[] outText = new byte[400];
        //decyptor.TransformBlock(Encoding.UTF8.GetBytes(rule), 0, 0, outText, 0);

        //Debug.Log("Public Key" + outText);


        #endregion 비대칭키 암호화 사용 코드 (현재 비사용)

    }

    void ANS_UPDATE_DEVICE(JSONObject _data)
    {
        int result = 1;
        _data.GetField(ref result, "result");
        if (result != 0)
            Debug.LogError("Fail ANS_UPDATE_DEVICE");
    }


    void ANS_CURRENT_TIME(JSONObject _data)
    {
        GameCore.Instance.PlayerDataMgr.SetCardSData(_data.GetField("ITEM_LIST"));

        string str = null;
        _data.GetField(ref str, "TIME");
        if (str == null)
            return;

        DateTime time = DateTime.ParseExact(str, "yyyy-MM-dd HH:mm:ss", null);
        GameCore.timeGap = time - DateTime.Now;
        GameCore.prevTime = time;

        if (bSessionTest)
        {
            bSessionTest = false;
            GameCore.Instance.ReconnectedSession(false);
        }
    }

    
    void ANS_ACCOUNT_LOAD(JSONObject _data)
    {
        int result = 1;
        _data.GetField(ref result, "result");
        if (result == 0)
            GameCore.Instance.PlayerDataMgr.LoadedDataToServer = true;
        else
        {
            Debug.LogError("Fail ANS_ACCOUNT_LOAD");
        }
    }

    void ANS_NOTIFY_INSPECTION(JSONObject _data)
    {
        if (_data != null)
        {
#if !NO_INSPECTION
            var INSPECTION = _data.GetField("INSPECTION");

            if (INSPECTION != null)
            {
                inspectionSData = new InspectionSData();
                inspectionSData.SetData(INSPECTION);
                inspectionNotify = false;
            }
#endif
            var deh = _data.GetField("DEH");
            if (deh != null) preSharedIv = ToByte(deh.str);
            var dkh = _data.GetField("DKH");
            if (dkh != null) preSharedKey = ToByte(dkh.str);

            ANS_CURRENT_TIME(_data);
        }
    }

    void ANS_NOTIFY_HOTTIME(JSONObject _data)
    {
        if (_data != null)
        {
            GameCore.Instance.PlayerDataMgr.SetHotTimeSData(_data.GetField("HOTTIME"));
            if (GameCore.Instance.SubsysMgr.GetNowSysType() == SubSysType.Lobby)
            {
                (GameCore.Instance.SubsysMgr.GetNowSubSys() as LobbySys).UpdateHotTime();
            }
        }
    }

    void ANS_ACCOUNT_KICK(JSONObject _data)
    {
        GameCore.Instance.ShowAlert(CSTR.MSG_KICK);
        GameCore.Instance.LogOut();
    }



#endregion EventHandle (ACCOUNT_LOGIN, ACCOUNT_RECONNECT, CURRENT_TIME, NOTIFY_INSPECTION)



	private SocketIOComponent socket;       // 소켓

    // 점검 데이터 관련
    InspectionSData inspectionSData = null; // 점검데이터를 보관하는 데이터

    bool inspectionNotify = false;          // 10분 후 점검으로 종료한다는 메시지를 띄웠다면 true
    bool inspectionQuit = false;            // 점검으로 종료한다는 메시지를 띄웠다면 true
        
    bool bLogoutDisconnect;                 // 로그아웃으로인한 연결 종료를 위한 플래그
    bool bLogoutReconnect;                  // 로그아웃으로인한 재연결을 위한 플래그

    bool bSessionTest = false;              // 세션 검사를 위한 플래그

    int waitRecvCnt = 0;                    // 서버에게 응답 대기 중인 메시지 카운트
    float waitReponse = 0f;                 // 모든 응답을 받기까지의 시간을 재기위한 변수( = nowTime + 10 second, if over ? DoDistconnect )

    //bool bReconnect { get { return bb; } set { bb = value; Debug.Log(">>>>>>>bReconnect : " + value); } }
    bool bReconnect;                        // 소켓 에러 및 연결이 끊길 경우 재연결을 윈한 변수
    float disconnectTimer = 0f;             // 리커넥트 시 시간을 재기위한 변수
    float disconnectTimeOut = 10f;          // 리커넥트 시 타임아웃 시간. 타임 아웃시 연결이 종료되고, 연결실패 메시지와 함께 로딩화면으로 돌아간다.

    bool bReconnetSession = false;          // 세션 연결을 위한 리커넥스시 true.

    // 서버 데이터 델리게이트를 위한 매핑 테이블 <MESSAGE, <ATYPE, EventType>>
    Dictionary<string, Dictionary<string, GameEventType>> packetEventMap = new Dictionary<string, Dictionary<string, GameEventType>>();

    public bool WaitReceive { get { return 0 < waitRecvCnt; } }


    internal GameNetworkMgr(SocketIOComponent _socket)
	{
		AddPacketEnventMap("ANS_CURRENT_TIME", "999", GameEventType.ANS_CURRENT_TIME);
		AddPacketEnventMap("ANS_ACCOUNT", "000", GameEventType.ANS_ACCOUNT_CREATE);
		AddPacketEnventMap("ANS_ACCOUNT", "100", GameEventType.ANS_ACCOUNT_LOGIN);
		AddPacketEnventMap("ANS_ACCOUNT", "101", GameEventType.ANS_ACCOUNT_LOGOUT);
        AddPacketEnventMap("EVENT_ACCOUNT_KICK", "999", GameEventType.ANS_ACCOUNT_KICK);
        AddPacketEnventMap("ANS_ACCOUNT", "102", GameEventType.ANS_ACCOUNT_RECONNECT);
        AddPacketEnventMap("ANS_ACCOUNT", "200", GameEventType.ANS_ACCOUNT_CHARSLOT);
		AddPacketEnventMap("ANS_ACCOUNT", "201", GameEventType.ANS_ACCOUNT_SETCOMDELE);
        AddPacketEnventMap("ANS_ACCOUNT", "202", GameEventType.ANS_ACCOUNT_SETMAINMYROOM);
        AddPacketEnventMap("ANS_ACCOUNT", "203", GameEventType.ANS_ACCOUNT_EQUIPSLOT);
        AddPacketEnventMap("ANS_ACCOUNT", "204", GameEventType.ANS_TUTORIAL);
        AddPacketEnventMap("ANS_ACCOUNT", "205", GameEventType.ANS_ACCOUNT_DELETE);
        AddPacketEnventMap("ANS_ACCOUNT", "103", GameEventType.ANS_ACCOUNT_LOAD);

        AddPacketEnventMap("ANS_UPDATE_DEVICE", "999", GameEventType.ANS_UPDATE_DEVICE);

        AddPacketEnventMap("ANS_LEVEL", "999", GameEventType.ANS_LEVEL_UP);
        //AddPacketEnventMap("ANS_CHARACTER", "000", GameEventType.ANS_CHARACTER_CREATE);
        AddPacketEnventMap("ANS_CHARACTER", "000", GameEventType.ANS_CHARACTER_COMMENT);
        AddPacketEnventMap("ANS_CHARACTER", "100", GameEventType.ANS_CHARACTER_EVALUATE_LIST);
        AddPacketEnventMap("ANS_CHARACTER", "101", GameEventType.ANS_CHARACTER_EVALUATE_MINE);
        AddPacketEnventMap("ANS_CHARACTER", "200", GameEventType.ANS_UPDATE_TEAM);
		AddPacketEnventMap("ANS_CHARACTER", "201", GameEventType.ANS_CHARACTER_ALLOCATE); 
        AddPacketEnventMap("ANS_CHARACTER", "202", GameEventType.ANS_CHARACTER_COMMENT_EDIT);
        AddPacketEnventMap("ANS_CHARACTER", "203", GameEventType.ANS_CHARACTER_EVALUATE);
        AddPacketEnventMap("ANS_CHARACTER", "204", GameEventType.ANS_CHARACTER_UNALLOCATE);
        AddPacketEnventMap("ANS_CHARACTER", "205", GameEventType.ANS_CHARACTER_POWER);
        AddPacketEnventMap("ANS_CHARACTER", "206", GameEventType.ANS_CHARACTER_STRENGTHEN_EXP);
        AddPacketEnventMap("ANS_CHARACTER", "207", GameEventType.ANS_CHARACTER_STRENGTHEN);
        AddPacketEnventMap("ANS_CHARACTER", "208", GameEventType.ANS_CHARACTER_EVOLUTION);
        AddPacketEnventMap("ANS_CHARACTER", "300", GameEventType.ANS_CHARACTER_DELETE);
		AddPacketEnventMap("ANS_CHARACTER", "301", GameEventType.ANS_CHARACTER_SALE);

        AddPacketEnventMap("ANS_ITEM", "200", GameEventType.ANS_ITEM_GEN_CHECK);
        AddPacketEnventMap("ANS_ITEM", "201", GameEventType.ANS_ITEM_EQUIP);
        AddPacketEnventMap("ANS_ITEM", "202", GameEventType.ANS_ITEM_EQUIP_CHANGE);
        AddPacketEnventMap("ANS_ITEM", "203", GameEventType.ANS_ITEM_UNEQUIP);
        AddPacketEnventMap("ANS_ITEM", "204", GameEventType.ANS_ITEM_STRENGTHEN_EXP);
        AddPacketEnventMap("ANS_ITEM", "205", GameEventType.ANS_ITEM_STRENGTHEN);
        AddPacketEnventMap("ANS_ITEM", "300", GameEventType.ANS_ITEM_SALE); 

        AddPacketEnventMap("ANS_FARMING", "000", GameEventType.ANS_FARMING_CREATE);
		AddPacketEnventMap("ANS_FARMING", "100", GameEventType.ANS_FARMING_GETLIST);
		AddPacketEnventMap("ANS_FARMING", "200", GameEventType.ANS_FARMING_DONE);
		AddPacketEnventMap("ANS_FARMING", "300", GameEventType.ANS_FARMING_CANCEL);
        AddPacketEnventMap("ANS_CHATTING", "000", GameEventType.ANS_CHAT);
        AddPacketEnventMap("ANS_CHATTING", "001", GameEventType.ANS_CHAT_NOTIFICATION);
        AddPacketEnventMap("ANS_CHATTING", "100", GameEventType.ANS_CHAT_FILTER);
		AddPacketEnventMap("ANS_GACHA", "000", GameEventType.ANS_GACHA);
		AddPacketEnventMap("ANS_MAKING", "000", GameEventType.ANS_MAKE_DONE);
		AddPacketEnventMap("ANS_MAKING", "001", GameEventType.ANS_MAKE_QUICK);
        AddPacketEnventMap("ANS_MAKING", "002", GameEventType.ANS_MAKE_TAKEALL);
        AddPacketEnventMap("ANS_MAKING", "100", GameEventType.ANS_MAKE_GETLIST);
		AddPacketEnventMap("ANS_MAKING", "200", GameEventType.ANS_MAKE_UNLOCK);
		AddPacketEnventMap("ANS_MAKING", "201", GameEventType.ANS_MAKE_START);
		AddPacketEnventMap("ANS_STORY", "100", GameEventType.ANS_STORY_GETLIST);
		AddPacketEnventMap("ANS_STORY", "101", GameEventType.ANS_STORY_START);
		AddPacketEnventMap("ANS_STORY", "102", GameEventType.ANS_STORY_DONE);
		AddPacketEnventMap("ANS_STORY", "103", GameEventType.ANS_STORY_REWARD);

        AddPacketEnventMap("ANS_MYROOM", "000", GameEventType.ANS_MYROOM_Buy);
        AddPacketEnventMap("ANS_MYROOM", "100", GameEventType.ANS_MYROOM_GetInfo);
        AddPacketEnventMap("ANS_MYROOM", "101", GameEventType.ANS_MYROOM_FriendList);
        AddPacketEnventMap("ANS_MYROOM", "102", GameEventType.ANS_MYROOM_FriendRoomDataList);
        AddPacketEnventMap("ANS_MYROOM", "103", GameEventType.ANS_MYROOM_HISTORY);
        AddPacketEnventMap("ANS_MYROOM", "104", GameEventType.ANS_MYROON_REVENGE);
		AddPacketEnventMap("ANS_MYROOM", "200", GameEventType.ANS_MYROOM_BUILD);
		AddPacketEnventMap("ANS_MYROOM", "201", GameEventType.ANS_MYROOM_START_CLEAN);
		AddPacketEnventMap("ANS_MYROOM", "202", GameEventType.ANS_MYROOM_FINISH_REVENGE);
		AddPacketEnventMap("ANS_MYROOM", "300", GameEventType.ANS_MYROOM_ReturnToInventoryObject);
        AddPacketEnventMap("ANS_MYROOM", "301", GameEventType.ANS_MYROOM_END_CLEAN);
        AddPacketEnventMap("ANS_MYROOM", "302", GameEventType.ANS_MYROOM_END_CLEAN_ALL);
        AddPacketEnventMap("ANS_MYROOM", "105", GameEventType.ANS_MYROOM_GETBUFFLIST);

        AddPacketEnventMap("ANS_PVP", "000", GameEventType.ANS_PVP_QUITGRADETEST);
        AddPacketEnventMap("ANS_PVP", "100", GameEventType.ANS_PVP_RANKLIST); 
        AddPacketEnventMap("ANS_PVP", "101", GameEventType.ANS_PVP_CONFIRMLASTSEASON);		
        AddPacketEnventMap("ANS_PVP", "103", GameEventType.ANS_PVP_TEAMINFO); 
        AddPacketEnventMap("ANS_PVP", "200", GameEventType.ANS_PVP_MATCHLIST);
        AddPacketEnventMap("ANS_PVP", "201", GameEventType.ANS_PVP_MATCHLIST);
        AddPacketEnventMap("ANS_PVP", "202", GameEventType.ANS_PVP_FINISHPLACEMENT);
        AddPacketEnventMap("ANS_PVP", "203", GameEventType.ANS_PVP_STARTBATTLE);
        AddPacketEnventMap("ANS_PVP", "204", GameEventType.ANS_PVP_FINISHBATTLE);

        AddPacketEnventMap("ANS_FRIEND", "000", GameEventType.ANS_FRIEND_REQUEST);
        AddPacketEnventMap("ANS_FRIEND", "100", GameEventType.ANS_FRIEND_LIST);
        AddPacketEnventMap("ANS_FRIEND", "101", GameEventType.ANS_FRIEND_SEARCH);
        AddPacketEnventMap("ANS_FRIEND", "102", GameEventType.ANS_FRIEND_RECOMMENDLIST);
        AddPacketEnventMap("ANS_FRIEND", "103", GameEventType.ANS_FRIEND_REQUESTEDLIST);
        AddPacketEnventMap("ANS_FRIEND", "104", GameEventType.ANS_FRIEND_ACCEPTABLELIST);
        AddPacketEnventMap("ANS_FRIEND", "105", GameEventType.ANS_FRIEND_TEAMINFO);
        AddPacketEnventMap("ANS_FRIEND", "106", GameEventType.ANS_FRIEND_STRIKER);
        AddPacketEnventMap("ANS_FRIEND", "200", GameEventType.ANS_FRIEND_ACCEPT_OK);
        AddPacketEnventMap("ANS_FRIEND", "201", GameEventType.ANS_FRIEND_ACCEPT_NO);
        AddPacketEnventMap("ANS_FRIEND", "301", GameEventType.ANS_FRIEND_REMOVE);
        AddPacketEnventMap("ANS_FRIEND", "302", GameEventType.ANS_FRIEND_SEND);
        AddPacketEnventMap("ANS_FRIEND", "303", GameEventType.ANS_FRIEND_RECEIVE);

        AddPacketEnventMap("ANS_DUNGEON", "100", GameEventType.ANS_DUNGEON_DAILY_START);
        AddPacketEnventMap("ANS_DUNGEON", "200", GameEventType.ANS_DUNGEON_DAILY_END);
        AddPacketEnventMap("ANS_DUNGEON", "201", GameEventType.ANS_DUNGEON_DAILY_BUY_TICKET);

        AddPacketEnventMap("ANS_RAID", "100", GameEventType.ANS_RAID_PREPARE);
        AddPacketEnventMap("ANS_RAID", "101", GameEventType.ANS_RAID_START);
        AddPacketEnventMap("ANS_RAID", "102", GameEventType.ANS_RAID_MYRANK);
        AddPacketEnventMap("ANS_RAID", "103", GameEventType.ANS_RAID_RANK50);
        AddPacketEnventMap("ANS_RAID", "104", GameEventType.ANS_RAID_TEAM_INFO);
        AddPacketEnventMap("ANS_RAID", "200", GameEventType.ANS_RAID_END);

        AddPacketEnventMap("ANS_MAIL", "000", GameEventType.ANS_MAIL_GET);
        AddPacketEnventMap("ANS_MAIL", "100", GameEventType.ANS_MAIL_LIST);

        AddPacketEnventMap("ANS_MISSION", "000", GameEventType.ANS_MISSION_REWARD);
        AddPacketEnventMap("ANS_MISSION", "001", GameEventType.ANS_MISSION_REWARD_TOP);
        AddPacketEnventMap("ANS_MISSION", "100", GameEventType.ANS_MISSION_LIST);

        AddPacketEnventMap("ANS_NOTIFY", "100", GameEventType.ANS_NOTIFY_FRIEND_MAIL_COUNT);
        AddPacketEnventMap("ANS_NOTIFY", "101", GameEventType.ANS_NOTIFY_MYROOM_MISSION_COUNT);
        AddPacketEnventMap("ANS_NOTIFY", "102", GameEventType.ANS_NOTIFY_FRIEND_COUNT);
        AddPacketEnventMap("ANS_NOTIFY", "103", GameEventType.ANS_NOTIFY_MAIL_COUNT);
        AddPacketEnventMap("ANS_NOTIFY", "104", GameEventType.ANS_NOTIFY_INSPECTION);
        AddPacketEnventMap("ANS_NOTIFY", "105", GameEventType.ANS_NOTIFY_HOTTIME);
        AddPacketEnventMap("ANS_NOTIFY", "106", GameEventType.ANS_NOTIFY_NOTICE);

        AddPacketEnventMap("NOTIFY_FRIEND", "999", GameEventType.NOTIFY_FRIEND);
        AddPacketEnventMap("NOTIFY_MAIL", "999", GameEventType.NOTIFY_MAIL);

        AddPacketEnventMap("ANS_SHOP", "000", GameEventType.ANS_SHOP_BUY);
        AddPacketEnventMap("ANS_SHOP", "100", GameEventType.ANS_SHOP_INQUIRY);
        AddPacketEnventMap("ANS_SHOP", "101", GameEventType.ANS_SHOP_INQUIRY_ITEM_SKIN);
        AddPacketEnventMap("ANS_SHOP", "001", GameEventType.ANS_SHOP_TAKE_ITEM);

        //AddPacketEnventMap("ANS_PURCHASE", "000", GameEventType.ANS_PURCHASE);
        //AddPacketEnventMap("ANS_PURCHASE", "001", GameEventType.ANS_PURCHASE_COUPON);

        AddPacketEnventMap("ANS_ATTENDANCE", "000", GameEventType.ANS_ATTENDANCE_RECEIVE);
        AddPacketEnventMap("ANS_ATTENDANCE", "100", GameEventType.ANS_ATTENDANCE_LOOKUP);

        AddPacketEnventMap("ANS_PUSH", "200", GameEventType.ANS_PUSH_SAVE);

        AddPacketEnventMap("ANS_INITDATA", "100", GameEventType.ANS_INITDATA);
        

        //Todo : 수신 패킷 추가시
        

        // 이벤트 핸들러 등록
        GameCore.Instance.EventMgr.RegisterHandler(this, GameEventType.ANS_ACCOUNT_LOGIN);
        GameCore.Instance.EventMgr.RegisterHandler(this, GameEventType.REQ_ACCOUNT_RECONNECT);
        GameCore.Instance.EventMgr.RegisterHandler(this, GameEventType.ANS_ACCOUNT_RECONNECT);
        GameCore.Instance.EventMgr.RegisterHandler(this, GameEventType.ANS_CURRENT_TIME);
        GameCore.Instance.EventMgr.RegisterHandler(this, GameEventType.ANS_UPDATE_DEVICE);
        GameCore.Instance.EventMgr.RegisterHandler(this, GameEventType.ANS_ACCOUNT_LOAD);
        GameCore.Instance.EventMgr.RegisterHandler(this, GameEventType.ANS_NOTIFY_INSPECTION);
        GameCore.Instance.EventMgr.RegisterHandler(this, GameEventType.ANS_NOTIFY_HOTTIME);
        GameCore.Instance.EventMgr.RegisterHandler(this, GameEventType.ANS_ACCOUNT_KICK);

        Debug.Log("Socket Connect.... << ");

        // 소켓 설정 및 연결
        socket = _socket;
        socket.SetMasterHandler(CBFromServerDataDelegate);
        socket.RegistWebSocket(socket.url);
        socket.Connect();

    }


    /// <summary>
    /// 이벤트 패킷 매핑
    /// </summary>
    /// <param name="_name"> 메시지 명 </param>
    /// <param name="_code"> 메시지 AType </param>
    /// <param name="_type"> 매핑할 이벤트 타입 </param>
    private void AddPacketEnventMap(string _name, string _code, GameEventType _type)
	{
		if (!packetEventMap.ContainsKey(_name))
			packetEventMap.Add(_name, new Dictionary<string, GameEventType>());

		if(packetEventMap[_name].ContainsKey(_code))
		{
			Debug.LogError("Exist same Packet Event Type. [" + _name + "][" + _code + "]");
			return;
		}

		packetEventMap[_name].Add(_code, _type);
	}


    /// <summary>
    /// 수신 대기 업카운트
    /// </summary>
    /// <param name="_text"> 요청하는 MASSEGE 이름 </param>
    public void IncReqCount(string _text)
    {
        // 업카운트 제외 메시지
        switch (_text)
        {
            case "REQ_NOTIFY":
            case "REQ_LEVEL":
            case "REQ_CHATTING":
            case "REQ_CURRENT_TIME":
                return;
        }

        //Debug.Log(_text + " nReqCount : " + nReqCount + " + 1+++++");

        ++waitRecvCnt;
        GameCore.Instance.SetActiveBlockPanel(true); // 업데이트에서 1초 지연 후 호출한다.

        // 대기 타이머 세팅
        if (waitRecvCnt == 1)
            waitReponse = Time.unscaledTime + 30f;
    }



    /// <summary>
    /// 수신 대기 다운카운트
    /// </summary>
    /// <param name="_text"></param>
    public void DecReqCount(string _text)
    {
        // 다운 카운트 제외 메시지
        switch (_text)
        {
            case "ANS_NOTIFY":
            case "NOTIFY_FRIEND":
            case "NOTIFY_MAIL":
            case "ANS_LEVEL":
            case "ANS_CHATTING":
            case "ANS_CURRENT_TIME":
            case "EVENT_ACCOUNT_KICK":
                return;
        }

        if (waitRecvCnt == 0)
            Debug.LogError("Invalid Exception. Invalid ReqCount down. Check please " + _text);

        // Debug.Log(_text + " nReqCount : " + nReqCount + " - 1-----");

        waitRecvCnt = Mathf.Max(0, waitRecvCnt - 1);
        GameCore.Instance.SetActiveBlockPanel(waitRecvCnt != 0);

        // 대기 타이머 리셋
        if (waitRecvCnt == 0)
            waitReponse = 0f;
    }

    /// <summary>
    /// 수신 대기 카운트 리셋
    /// </summary>
    public void ResetReqCount()
    {
        waitRecvCnt = 0;
        waitReponse = 0f;
        GameCore.Instance.SetActiveBlockPanel(false);
    }


    /// <summary>
    /// 수신 - 소켓 이벤트 마스터 핸들러 (콜백)
    /// </summary>
    /// <param name="_data"></param>
	internal void CBFromServerDataDelegate(SocketIOEvent _data)
	{
        //Debug.Log("PACKET NAME : " + _data.name + "   \t" + DateTime.Now.Minute+":"+ DateTime.Now.Second +"."+DateTime.Now.Millisecond);

        //Debug.LogWarning(_data.name.Split(' ')[0].ToLower());
        switch (_data.name.Split(' ')[0].ToLower())
		{
			case "connect":
			case "open":
			case "close":
                if (bLogoutDisconnect)
                {
                    bLogoutDisconnect = false;
                    bLogoutReconnect = true;
                }
                return;

            case "disconnect":
                break;
			
			case "error":
                bReconnect = true;
				break;

            case "event_lost_session":
                if (bSessionTest)
                {
                    bSessionTest = false;
                    Req_Account_Reconnect();
                }
                break;
        }

		if (_data.data == null)
			return;



        string atype = "999";	// If Not Used AType, Use This String ( ANS_CURRENT_TIME, ... )
		_data.data.GetField(ref atype, "ATYPE");


        if (_data.name != "ANS_ACCOUNT" || atype != "103") // 예외처리....
        {
            // 수신 대기 디스카운트
            DecReqCount(_data.name);
        }

        // 알맞는 이벤트 타임을 찾아 이벤트 매니저에서 넘긴다.
        if (packetEventMap.ContainsKey(_data.name))
		{
			if(packetEventMap[_data.name].ContainsKey(atype))
			{
				GameCore.Instance.EventMgr.SendEvent(new GameEvent(packetEventMap[_data.name][atype], new PacketPara(_data)));
                GameCore.Instance.CommonSys.ShowLoadingIcon(false);
				return;
			}
		}

		Debug.Log("처리할 수 없는 패킷 : " + _data.name + " " + atype); return;
	}


    /// <summary>
    /// 송신 - 서버로 메시지를 보낸다.
    /// </summary>
    /// <param name="_req"> 메시지 이름 </param>
    /// <param name="_atype"> ATYPE </param>
    /// <param name="_data"> 보낼 데이터 </param>
    void Send(string _req, string _atype, JSONObject _data)
    {
        // 특정 패킷만 암호화
        switch (_req)
        {
            case "REQ_ACCOUNT": if (_atype == "000" || _atype == "100") _data = ConvertSendData(_data.ToString()); break;
            case "REQ_STORY":   if (_atype == "102")                    _data = ConvertSendData(_data.ToString()); break;
        }

        // 송신
        _data.AddField("ATYPE", _atype);

        EmitRapper(_req, _data);

        // 수신대기 업카운트
        IncReqCount(_req);
    }

    private void EmitRapper(string _req, JSONObject _data)
    {
        if (socket.IsConnected)
            socket.Emit(_req, _data);
        else
            Debug.LogWarning("Not Connected.");
    }


    /// <summary>
    /// 메시지를 암호화된 메시지로 변환한다.
    /// </summary>
    /// <param name="dataStr">암호화할 메시지</param>
    /// <returns> 암호화된 메시지 </returns>
    internal JSONObject ConvertSendData(string dataStr)
    {
        string encryptData = CustomEncrypt(dataStr);
        JSONObject send = new JSONObject();
        send.AddField("data", encryptData);
        return send;
    }

    /// <summary>
    /// Update from GameCore
    /// </summary>
    internal void Update()
	{
        // 점검 데이터가 있고, 활성화되어있다면
        if (inspectionSData != null && inspectionSData.ACTIVE != 0)
            UpdateInspection(); // 점검 데이터 처리

        // 리커넥트 시도 상태 시 (에러나 세션을 잃었을때)
        if (bReconnect)
            UpdateReconnect(); // 리커넥트 시도 및 실패 처리

        // 로그아웃으로 인한 리커넥트 시
        if (bLogoutReconnect)
        {
            bLogoutReconnect = false;
            GameCore.Instance.DoWaitCall(socket.Connect);
        }

        // 서버 응답 30초 지연 시 disconnect 한다.
        CheckUpdateResponse();
    }


    /// <summary>
    /// 연결 종료 후, 재연결을 하기위한 플래그를 설정한다.(close 패킷이 정상적으로 와야 재연결을 시도한다.)
    /// </summary>
    public void DoReconnect()
    {
        bLogoutDisconnect = true;
        bLogoutReconnect = false;
        socket.Close();
    }

    /// <summary>
    /// 디스커넥트한다.
    /// </summary>
    public void DoDisconnect()
    {
        bLogoutDisconnect = false;
        bLogoutReconnect = false;
        socket.Close();
    }

    
    internal void SessionTest()
    {
        bSessionTest = true;
        Req_Current_Time();
    }

    /// <summary>
    /// 점검 메시지를 처리한다.
    /// </summary>
    /// <returns> 
    ///     false : 활성화 대기 상태
    ///     true  : 점검 중인 상태
    /// </returns>
    bool UpdateInspection()
    {
        // 점검 시간이 지났다면 
        if (inspectionSData.END_DATE < GameCore.nowTime)
        {
            //inspectionSData = null;
            inspectionNotify = false;
        }

        // 점검 시간 중이라면
        else if (inspectionSData.START_DATE < GameCore.nowTime)
        {
            if (inspectionQuit == false)
            {
                inspectionQuit = true;

                string msg = string.Format("{0}\n\n종료 시간 : {1}",
                                            inspectionSData.MSG,
                                            inspectionSData.END_DATE.ToString("yyyy/MM/dd HH:mm"));
                GameCore.Instance.ShowNotice(inspectionSData.TITLE, msg, () =>
                {
                    GameCore.Instance.QuitApplication();
                }, 0, false);
            }

            return true;
        }

        // 점검 사전 통보 여부
        else if (inspectionNotify == false)
        {
            if (inspectionSData.START_DATE.AddMinutes(-10) < GameCore.nowTime)
            {
                inspectionNotify = true;
                string msg = string.Format("{0}\n\n점검 시간 : {1}\n종료 시간 : {2}", 
                                            inspectionSData.MSG, 
                                            inspectionSData.START_DATE.ToString("yyyy/MM/dd HH:mm"), 
                                            inspectionSData.END_DATE.ToString("yyyy/MM/dd HH:mm"));
                GameCore.Instance.ShowNotice(inspectionSData.TITLE, msg, 0);
            }
        }



        return false;
    }


    /// <summary>
    /// 리커넥팅 시도 상태일 때 리커넥트를 시도한다.
    /// </summary>
    void UpdateReconnect()
    {
        // Reconnect Success
        if (socket.socket.IsConnected)
        {
            bReconnect = false;
            Req_Account_Reconnect();

            disconnectTimer = 0f;
            GameCore.Instance.CommonSys.ShowLoadingIcon(false);
        }
        // Reconnecting...
        else
        {
            if (disconnectTimer >= 0f)
                disconnectTimer += Time.unscaledDeltaTime;
            GameCore.Instance.CommonSys.ShowLoadingIcon(true);

            // Reconnect fail
            if (disconnectTimeOut <= disconnectTimer)
            {
                GameCore.Instance.LogOut();
                disconnectTimer = -1f;

                GameCore.Instance.ShowNotice("연결 실패", "서버에 연결 할 수 없습니다. 잠시 후 다시 시도해주세요.", () =>
                {
                    GameCore.Instance.CloseMsgWindow();
                    disconnectTimer = 0;
                    socket.Connect(); // 한번도 연결이 안되었을경우을 대비해 확인버튼 클릭시 커넥트 시도
                }, 0, false);
            }
        }
    }


    /// <summary>
    /// 수신 대기중인 시간이 일정 시간 지나면 연결 종료한다.
    /// </summary>
    /// <returns> 연결이 종료된다면 false, 유지라면 true </returns>
    bool CheckUpdateResponse()
    {
        if (0 < waitRecvCnt && waitReponse < Time.unscaledTime)
        {
            GameCore.Instance.LogOut();
            GameCore.Instance.ShowNotice("연결 끊김", "서버로부터 응답이 없습니다.", GameCore.Instance.CloseMsgWindow, 0, false);
            ResetReqCount();
            return false;
        }

        return true;
    }



#region 송신 메시지

    internal void req_Logging(string _msg)
    {
        JSONObject data = new JSONObject();
        data.AddField("LOG", _msg);
        EmitRapper("LOGGING", data);            // Send를 사용치 않음(1/3)
    }

    internal void Req_Current_Time()
    {
        // 데이터 생성
        JSONObject data = new JSONObject();
        data.Add("CURRENT_TIME");
        socket.Emit("REQ_CURRENT_TIME", data); // Send를 사용치 않음(2/3)
    }

    internal void Req_Update_Device()
    {
        // 데이터 생성
        JSONObject data = new JSONObject();
        data.AddField("PLATFORM", Application.platform.ToString());
        data.AddField("REGID", GameCore.GetGCMRegID());
        data.AddField("MODEL", SystemInfo.deviceModel);
        data.AddField("VERSION", SystemInfo.operatingSystem);
        //송신
        socket.Emit("REQ_UPDATE_DEVICE", data); // Send를 사용치 않음 (3/3)
        IncReqCount("REQ_UPDATE_DEVICE");
    }


    internal void Req_Account_Create(string _userName, NCommon.LoginType _type)
	{
		// 송신 제약 조건
		// Nothing

		// 데이터 생성
		JSONObject data = new JSONObject();
        data.AddField("UDID", SystemInfo.deviceUniqueIdentifier + GameCore.Instance.UserID);
        //string userIDStr = GameCore.Instance.userIDStr;
        //if (userIDStr == "") data.AddField("UDID", SystemInfo.deviceUniqueIdentifier + GameCore.Instance.UserID);
        //else data.AddField("UDID", userIDStr);
        data.AddField("USER_NAME", _userName);

        // 게임팟 로그인 관련
#if !UNITY_EDITOR
        var typeNum = GamePotManager.GetLoginTypeNumber(_type);
        data.AddField("TYPE", typeNum);
        data.AddField("LTID", typeNum == 0 ? "" : GameCore.Instance.GamePotMgr.UserInfo.memberid);
#else
        data.AddField("TYPE", 0);
        data.AddField("LTID", "");
#endif
        //송신
        Send("REQ_ACCOUNT", "000", data);
    }

    internal void ReConnectSession()
    {
        bReconnetSession = true;
        Req_Account_Reconnect();
    }

    internal bool CheckSession()
    {
        return socket.socket.ReadyState == WebSocketSharp.WebSocketState.Open;
    }

    internal void Req_Account_Login(NCommon.LoginType _type)
	{
        Debug.Log(">> Req_Account_Login ::  " + _type);

        // Req_Current_Time();
        // 송신 제약 조건
        // Nothing

        // 데이터 생성
        JSONObject data = new JSONObject();
        data.AddField("UDID", SystemInfo.deviceUniqueIdentifier + GameCore.Instance.UserID);
        //string userIDStr = GameCore.Instance.userIDStr;
        //if (userIDStr == "") data.AddField("UDID", SystemInfo.deviceUniqueIdentifier + GameCore.Instance.UserID);
        //else data.AddField("UDID", userIDStr);
        //      data.AddField("PLATFORM", Application.platform.ToString());
        //data.AddField("REGID", GameCore.GetGCMRegID());
        //data.AddField("MODEL", SystemInfo.deviceModel);
        //data.AddField("VERSION", SystemInfo.operatingSystem);

        // 게임팟 로그인 관련
#if !UNITY_EDITOR
        var typeNum = GamePotManager.GetLoginTypeNumber(_type);
        data.AddField("TYPE", typeNum);
        data.AddField("LTID", typeNum == 0 ? "" : GameCore.Instance.GamePotMgr.UserInfo.memberid);
#else
        data.AddField("TYPE", 0);
        data.AddField("LTID", "");
#endif

		//송신
        Send("REQ_ACCOUNT", "100", data);
    }


	internal void Req_Account_Logout()
	{
		JSONObject data = new JSONObject();
        Send("REQ_ACCOUNT", "101", data);
    }


	internal void Req_Account_Reconnect()
	{
		JSONObject data = new JSONObject();
        data.AddField("UDID", SystemInfo.deviceUniqueIdentifier + GameCore.Instance.UserID);
        //string userIDStr = GameCore.Instance.userIDStr;
        //if (userIDStr == "") data.AddField("UDID", SystemInfo.deviceUniqueIdentifier + GameCore.Instance.UserID);
        //else data.AddField("UDID", userIDStr);
        data.AddField("PLATFORM", Application.platform.ToString());
		data.AddField("REGID", GameCore.GetGCMRegID());
		data.AddField("MODEL", SystemInfo.deviceModel);
		data.AddField("VERSION", SystemInfo.operatingSystem);

        // 게임팟 로그인 관련
#if !UNITY_EDITOR
        var typeNum = 0;
        if (GameCore.Instance.GamePotMgr.UserInfo != null)
            typeNum = GamePotManager.GetLoginTypeNumber(GameCore.Instance.GamePotMgr.LoginType);
        data.AddField("TYPE", typeNum);
        data.AddField("LTID", typeNum == 0 ? "" : GameCore.Instance.GamePotMgr.UserInfo.memberid);
#else
        data.AddField("TYPE", 0);
        data.AddField("LTID", "");
#endif

        Send("REQ_ACCOUNT", "102", data);
    }


	internal void Req_Account_Change_ComDele(long _typicalUID, string _str)
	{
		JSONObject data = new JSONObject();
		data.AddField("DELEGATE_ICON", _typicalUID);
		data.AddField("COMMENT", _str);

        Send("REQ_ACCOUNT", "201", data);
    }


    internal void Req_Account_Change_Tutorial(Tutorial _tutorialData)
    {
        JSONObject realData = new JSONObject();
        realData.AddField("main", _tutorialData.main);
        realData.AddField("dungeon", _tutorialData.dungeon);
        realData.AddField("raid", _tutorialData.raid);
        realData.AddField("pvp", _tutorialData.pvp);
        realData.AddField("myroom", _tutorialData.myRoom);
        realData.AddField("manufact", _tutorialData.manufact);
        realData.AddField("farming", _tutorialData.farming);
        realData.AddField("mission", _tutorialData.mission);
        realData.AddField("mail", _tutorialData.mail);
        JSONObject data = new JSONObject();
        data.AddField("TUTORIAL", realData);

        Send("REQ_ACCOUNT", "204", data);
    }


	internal void Req_Update_Character_Slot(int _cash, int _cnt)
	{
		JSONObject data = new JSONObject();
		//data.AddField("CASH_AMOUNT", _cash);
		//data.AddField("CHARACTER_AMOUNT", _cnt);

		//송신
        Send("REQ_ACCOUNT", "200", data);
    }

	internal void Req_Update_Item_Slot()
	{
		JSONObject data = new JSONObject();
        Send("REQ_ACCOUNT", "203", data);
    }


    internal void Req_Notify_Friend_Mail_Count()
    {
        JSONObject data = new JSONObject();
        Send("REQ_NOTIFY", "100", data);
    }


    internal void Req_Notify_MyRoom_Count()
    {
        JSONObject data = new JSONObject();
        Send("REQ_NOTIFY", "101", data);
    }


    internal void Req_Delete_Character(long[] _ids)
	{
		// 송신 제약 조건
		// Nothing

		// 데이터 생성
		JSONObject data = new JSONObject();

		JSONObject idlist = new JSONObject(JSONObject.Type.ARRAY);
		for (int i = 0; i < _ids.Length; ++i)
		{
			idlist.AddField("CHA_UID", _ids[i].ToString("0"));
			//idlist.AddField("CNT", 1);
		}
		data.AddField("IDLIST",idlist);

		//송신
        Send("REQ_CHARACTER", "300", data);
    }


	internal void Req_Sale_Character(long[] _ids)
	{
		// 송신 제약 조건
		// Nothing

		// 데이터 생성
		JSONObject data = new JSONObject();

		JSONObject idlist = new JSONObject(JSONObject.Type.ARRAY);
		for (int i = 0; i < _ids.Length; ++i)
		{
			idlist.Add(_ids[i]);
		}
		data.AddField("IDLIST", idlist);

		//송신
        Send("REQ_CHARACTER", "301", data);
    }


	// 한 번에 여러개를 보내는 대신 각 개수는 1개로 한다.
	internal void Req_Sell_Item(long[] _ids)
	{
		// 송신 제약 조건
		// Nothing

		// 데이터 생성
		JSONObject data = new JSONObject();

		JSONObject idlist = new JSONObject(JSONObject.Type.ARRAY);
		for (int i = 0; i < _ids.Length; ++i)
		{
			JSONObject item = new JSONObject();
			item.AddField("ITEM_UID", _ids[i].ToString("0"));
			item.AddField("ITEM_COUNT", 1);
			idlist.Add(item);
		}
		data.AddField("SALELIST", idlist);

		//송신
        Send("REQ_ITEM", "300", data);
    }


	// 한개만 보낸다. 대신 카운트를 설정할 수 있따.
	internal void Req_Sell_Item(long _id, int _count)
	{
		// 송신 제약 조건
		// Nothing

		// 데이터 생성
		JSONObject data = new JSONObject();

		JSONObject idlist = new JSONObject(JSONObject.Type.ARRAY);
		JSONObject item = new JSONObject();
		item.AddField("ITEM_UID", _id.ToString("0"));
		item.AddField("ITEM_COUNT", _count);
		idlist.Add(item);
		data.AddField("SALELIST", idlist);

		//송신
        Send("REQ_ITEM", "300", data);
    }


	internal void Req_Update_Team(List<int> _team, List<int> _idx, List<long> _unit, List<int> _ts)
	{
		if (_unit.Count == 0)
			return;

		JSONObject data = new JSONObject();

		JSONObject idlist = new JSONObject(JSONObject.Type.ARRAY);
		for ( int i = 0; i < _idx.Count; ++i)
		{
			JSONObject unit = new JSONObject();
			unit.AddField("TEAM", _team[i]);
			unit.AddField("CHA_UID", _unit[i]);
			unit.AddField("POSITION", _idx[i]);
			unit.AddField("SKILL", _ts[i]);
			idlist.Add(unit);
		}
		data.AddField("IDLIST",idlist);

		//송신
        Send("REQ_CHARACTER", "200", data);
    }


	internal void Req_Chat(string _text)
	{
		JSONObject data = new JSONObject();
		data.AddField("MESSAGE", _text);
        Send("REQ_CHATTING", "000", data);
    }


    internal void Req_Chat_Filter()
    {
        JSONObject data = new JSONObject();
        Send("REQ_CHATTING", "100", data);
    }


    public void Req_MyRoom_GetInfo()
    {
        JSONObject data = new JSONObject();
        Send("REQ_MYROOM", "100", data);
    }

    //public void Req_MyRoomBuild(BuildableObject buildableObject)
    //{
    //    TileConditionMgr parentConMgr = null;
    //    if (buildableObject.transform.parent)
    //        parentConMgr = buildableObject.transform.parent.GetComponent<TileConditionMgr>();

    //    //.SceneMyRoom.Inst.nCurrTownIdx.ToString()
    //    JSONObject data = new JSONObject();
    //    data.AddField("MYROOM_ID", TouchMgr.Inst.UIMain.nCurrRoomIdx);
    //    data.AddField("ITEM_ID", buildableObject.ObjectInfos.tableKey);
    //    if(buildableObject.ObjectInfos.nObjectSID != 0)
    //        data.AddField("ITEM_UID", buildableObject.ObjectInfos.nObjectSID.ToString());
    //    else
    //        data.AddField("ITEM_UID", 0);

    //    // edit?
    //    data.AddField("MYROOM_ITEM_UID", buildableObject.ObjectInfos.nMyRoomItemUID);
        
    //    JSONObject anchor = new JSONObject();
    //    Vector3 anchorPos = new Vector3();
    //    anchorPos = parentConMgr.centerObject.GetComponent<TileConditionUnit>().rayCastTile.tileInfo.tilePos;
    //    anchor.AddField("x", anchorPos.x);
    //    anchor.AddField("y", anchorPos.z);

    //    //anchor.Add(parentConMgr.centerObject.GetComponent<TileConditionUnit>().rayCastTile.tileInfo.tilePos.x);
    //    //anchor.Add(parentConMgr.centerObject.GetComponent<TileConditionUnit>().rayCastTile.tileInfo.tilePos.z);
    //    data.AddField("ANCHOR", anchor);

    //    data.AddField("ANGLE", buildableObject.Angle.ToString());



    //    // 기획테이블 연결 키
    //    //obj.object_key = buildableObject.TableKey;
    //    // 오브젝트 타입
    //    //obj.object_type = (int)buildableObject.TownObjType;
    //    //obj.object_level = buildableObject.ObjectInfos.nObjectLevel;
    //    //obj.ground_x = (int)parentConMgr.centerObject.GetComponent<TileConditionUnit>().rayCastTile.tileInfo.tilePos.x;
    //    //obj.ground_y = (int)parentConMgr.centerObject.GetComponent<TileConditionUnit>().rayCastTile.tileInfo.tilePos.z;
    //    //obj.object_angle = (int)buildableObject.Angle;
    //    //cReq.town_obj = obj;


    //    JSONObject posList = new JSONObject(JSONObject.Type.ARRAY);

    //    for ( int i=0; i < parentConMgr.BuildableTileList.Count; i++ )
    //    {
    //        if (parentConMgr)
    //        {   /*
    //            detail.object_x = (int)parentConMgr.BuildableTileList[i].x;
    //            detail.object_y = (int)parentConMgr.BuildableTileList[i].z;

                
    //            if (buildableObject.AreaMatchedList != null && 
    //                buildableObject.AreaMatchedList.Count == parentConMgr.BuildableTileList.Count)
    //            {
    //                detail.object_before_x = (int)buildableObject.AreaMatchedList[i].x;
    //                detail.object_before_y = (int)buildableObject.AreaMatchedList[i].z;
    //            }
    //            */
    //            JSONObject pos = new JSONObject();
    //            //pos.Add(parentConMgr.BuildableTileList[i].x);
    //            //pos.Add(parentConMgr.BuildableTileList[i].z);
    //            pos.AddField("x", parentConMgr.BuildableTileList[i].x);
    //            pos.AddField("y", parentConMgr.BuildableTileList[i].z);
    //            posList.Add(pos);
    //        }
    //        else
    //        {
    //            Debug.LogError("Error In RequestTownUpdate; object xy ");
    //        }

    //    }

    //    data.AddField("POSITION", posList);

    //    Send("REQ_MYROOM", "200", data);

    //}

    public void Req_MyRoomBuildBase(MyRoomObjectData myRoomObjectData)
    {
        JSONObject data = new JSONObject();

        data.AddField("MYROOM_ID", myRoomObjectData.UsedRoomId);
        data.AddField("ITEM_UID", myRoomObjectData.ItemUniqueid);
        data.AddField("MYROOM_ITEM_UID", myRoomObjectData.MyRoomUniqueid);

        JSONObject anchor = new JSONObject();
        anchor.AddField("x", 0);
        anchor.AddField("y", 0);

        data.AddField("ANCHOR", anchor);
        data.AddField("ANGLE", 0);

        JSONObject positionList = new JSONObject(JSONObject.Type.ARRAY);

        JSONObject vector = new JSONObject();
        vector.AddField("x", (int)myRoomObjectData.vectorList[0].x);
        vector.AddField("y", (int)myRoomObjectData.vectorList[0].y);
        positionList.Add(vector);

        JSONObject flip = new JSONObject();
        flip.AddField("x", (int)myRoomObjectData.vectorList[1].x);
        flip.AddField("y", (int)myRoomObjectData.vectorList[1].y);

        positionList.Add(flip);

        data.AddField("POSITION", positionList);

        //Debug.Log(data.ToString());

        Send("REQ_MYROOM", "200", data);
    }

    public void Req_MyRoomBuildBase(List<MyRoomObjectData> list)
    {
        JSONObject dataList = new JSONObject(JSONObject.Type.ARRAY);

        foreach(var objData in list)
        {
            JSONObject data = new JSONObject();

            data.AddField("MYROOM_ID", objData.UsedRoomId);
            data.AddField("ITEM_UID", objData.ItemUniqueid);
            data.AddField("MYROOM_ITEM_UID", 0);

            JSONObject anchor = new JSONObject();
            anchor.AddField("x", 0);
            anchor.AddField("y", 0);

            data.AddField("ANCHOR", anchor);
            data.AddField("ANGLE", 0);

            JSONObject positionList = new JSONObject(JSONObject.Type.ARRAY);

            JSONObject vector = new JSONObject();
            vector.AddField("x", (int)objData.vectorList[0].x);
            vector.AddField("y", (int)objData.vectorList[0].y);
            positionList.Add(vector);

            JSONObject flip = new JSONObject();
            vector.AddField("x", (int)objData.vectorList[1].x);
            vector.AddField("y", (int)objData.vectorList[1].y);
            positionList.Add(flip);

            data.AddField("POSITION", positionList);
            dataList.Add(data);
        }

        //Debug.Log(data.ToString());

        Send("REQ_MYROOM", "200", dataList);
    }

    public void Req_MyRoomItemRemove(MyRoomObjectData myRoomObjectData)
    {
        JSONObject data = new JSONObject();
        data.AddField("MYROOM_ID", myRoomObjectData.UsedRoomId);
        data.AddField("MYROOM_ITEM_UID", myRoomObjectData.MyRoomUniqueid);

        Send("REQ_MYROOM", "300", data);
    }

    public void Req_MyRoomFriendList()
    {
        Send("REQ_MYROOM", "101", new JSONObject());
    }

    public void Req_MyRoomFriendRoomDataList(long friendUID)
    {
        JSONObject json = new JSONObject();
        json.AddField("FRIEND_UID", friendUID);
        Send("REQ_MYROOM", "102", json);
    }

    public void Req_MyRoomCharArrangement(int roomId, List<int> charUIDList)
    {
        JSONObject data = new JSONObject();
        data.AddField("MYROOM_ID", roomId);

        JSONObject idList = new JSONObject(JSONObject.Type.ARRAY);
        for (int i = 0; i < charUIDList.Count; ++i)
        {
            idList.Add(charUIDList[i]);
        }
        data.AddField("IDLIST", idList);

        Send("REQ_CHARACTER", "201", data);
    }

    public void Req_MyRoomCharUnArrangement(int roomId, List<int> charUIDList)
    {
        JSONObject data = new JSONObject();
        data.AddField("MYROOM_ID", roomId);

        JSONObject idList = new JSONObject(JSONObject.Type.ARRAY);
        for (int i = 0; i < charUIDList.Count; ++i)
        {
            idList.Add(charUIDList[i]);
        }
        data.AddField("IDLIST", idList);

        Send("REQ_CHARACTER", "204", data);
    }

    public void Req_MyRoomStainCleanStart(MyRoomStainData stainData)
    {
        JSONObject data = new JSONObject();
        data.AddField("MYROOM_ID", stainData.PlacedRoomId);
        data.AddField("STAIN_UID", stainData.UniqueId);
        data.AddField("HELP_USER_UID", stainData.HelpUserId);

        Send("REQ_MYROOM", "201", data);
    }

    public void Req_MyRoomTakePresent(MyRoomStainData stainInfo)
    {
        JSONObject data = new JSONObject();
        data.AddField("MYROOM_ID", stainInfo.PlacedRoomId);
        data.AddField("STAIN_UID", stainInfo.UniqueId);

        Send("REQ_MYROOM", "301", data);
    }
    public void Req_MyRoomTakePresentAll(int _myroomID)
    {
        JSONObject data = new JSONObject();
        data.AddField("MYROOM_ID", _myroomID);

        Send("REQ_MYROOM", "302", data);
    }

    public void Req_FriendRoomCleanStain(MyRoomStainData stainData, long userUID)
    {
        JSONObject data = new JSONObject();
        data.AddField("MYROOM_ID", stainData.PlacedRoomId);
        data.AddField("STAIN_UID", stainData.UniqueId);
        data.AddField("HELP_USER_UID", userUID);

        Send("REQ_MYROOM", "201", data);
    }

    public void Req_MyRoomStartCleanStain(RoomStain stainInfo)
    {
        JSONObject data = new JSONObject();
        data.AddField("MYROOM_ID", stainInfo.nMyRoomID);
        data.AddField("STAIN_UID", stainInfo.nStainUID);
        data.AddField("HELP_USER_UID", stainInfo.nHelpUserUID);
        //data.AddField("CHA_UID", charUID);

        Send("REQ_MYROOM", "201", data);
    }

    public void Req_MyRoomEndCleanStain(RoomStain stainInfo)
    {
        JSONObject data = new JSONObject();
        data.AddField("MYROOM_ID", stainInfo.nMyRoomID);
        data.AddField("STAIN_UID", stainInfo.nStainUID);
        data.AddField("HELP_USER_UID", stainInfo.nHelpUserUID);
        //data.AddField("CHA_UID", charUID);

        Send("REQ_MYROOM", "301", data);
    }

    public void Req_MyRoomSelectMainRoom(int mainRoomIndex)
    {
        JSONObject data = new JSONObject();
        data.AddField("DELEGATE_MYROOM", mainRoomIndex);
        Send("REQ_ACCOUNT", "202", data);
    }

	public void Req_MyRoomHistory()
	{
		JSONObject data = new JSONObject();
        Send("REQ_MYROOM", "103", data);
    }

	public void Req_MyRoomRevenge(int _historyUID)
	{
		JSONObject data = new JSONObject();
		data.AddField("HISTORY_UID", _historyUID);

        Send("REQ_MYROOM", "104", data);
    }

	public void Req_MyRoomFinishRevenge(bool _win, long _historyUID)
	{
		JSONObject data = new JSONObject();
		data.AddField("HISTORY_UID", _historyUID);
		data.AddField("VICTORY", _win ? 1: 0);

        Send("REQ_MYROOM", "202", data);
    }

    public void Req_MyRoom_GetBuff()
    {
        JSONObject data = new JSONObject();
        Send("REQ_MYROOM", "105", data);
    }


	/// <summary>
	/// 가챠 시도
	/// </summary>
	/// <param name="_type">영웅은 99. 다머지는 ItemType의 정수값</param>
	/// <param name="_isSpecial"></param>
	/// <param name="_isTen"></param>
	/// <param name="_isFree"></param>
	internal void Rew_Gacha_Try(int _key)//int _type, bool _isSpecial, bool _isTen, bool _isFree)
	{
		JSONObject data = new JSONObject();
		//data.SetField("ITEM_TYPE", _type);
		//data.SetField("PICK_TYPE", _isSpecial ? 1 : 0);
		//data.SetField("NUMBER", _isTen ? 10 : 1);
		//data.SetField("FREE", _isFree ? 1 : 0);
		data.SetField("GACHA_ID", _key);

        Send("REQ_GACHA", "000", data);
    }

	internal void Req_Make_GetList()
	{
		JSONObject data = new JSONObject();
        Send("REQ_MAKING", "100", data);
    }

	internal void Req_Make(int _slotID, long[] _rscUID, int[] _rscCounts)
	{
		JSONObject data = new JSONObject();
		data.SetField("MAKING_ID", _slotID);

		JSONObject money = new JSONObject(JSONObject.Type.ARRAY);
		for(int i= 0; i < _rscCounts.Length; ++i)
		{
			JSONObject rsc = new JSONObject();
			rsc.AddField("ITEM_UID", _rscUID[i]);
			rsc.AddField("ITEM_COUNT", _rscCounts[i]);
			money.Add(rsc);
		}
		data.AddField("MONEY", money);

        Send("REQ_MAKING", "201", data);
    }

	internal void Req_Make_Done(int _slotID)
	{
		JSONObject data = new JSONObject();
		data.SetField("MAKING_ID", _slotID);

        Send("REQ_MAKING", "000", data);
    }

	internal void Req_Make_Quick(int _slotID)
	{
		JSONObject data = new JSONObject();
		data.SetField("MAKING_ID", _slotID);

        Send("REQ_MAKING", "001", data);
    }

    internal void Req_Make_Take_All(int _type)
    {
        JSONObject data = new JSONObject();
        data.SetField("TYPE", _type); // 1: 영웅, 2: 장비, 3: 가구

        Send("REQ_MAKING", "002", data);
    }

	internal void Req_Make_Unlock(int _slotID, bool _rental)
	{
		JSONObject data = new JSONObject();
		data.SetField("MAKING_ID", _slotID);
		data.SetField("TYPE", _rental ? 0 : 1);

        Send("REQ_MAKING", "200", data);
    }

	internal void Req_Farming_GetList()
	{
		JSONObject data = new JSONObject();
        Send("REQ_FARMING", "100", data);
    }

	// Todo : 송신 패킷 추가시
	internal void Req_Farming_Create(int _key, long[] _uids)
	{
		JSONObject data = new JSONObject();
		data.SetField("FARMING_ID", _key);
		JSONObject cha_uid = new JSONObject(JSONObject.Type.ARRAY);
		for (int i = 0; i < _uids.Length; ++i)
			cha_uid.Add(_uids[i]);
		data.SetField("IDLIST", cha_uid);

        Send("REQ_FARMING", "000", data);
    }

	internal void Req_Farming_Done(int[] _key)
	{
		JSONObject data = new JSONObject();

		JSONObject list = new JSONObject(JSONObject.Type.ARRAY);
		for (int i = 0; i < _key.Length; i++)
			list.Add(_key[i]);
		data.SetField("IDLIST", list);

        Send("REQ_FARMING", "200", data);
    }

	internal void Req_Farming_Cansel(int _key)
	{
		JSONObject data = new JSONObject();
		data.SetField("FARMING_ID", _key);
        Send("REQ_FARMING", "300", data);
    }

	internal void Req_Story_Start(int _storyKey, int _TeamIndex, long _strikerUID)
	{
		JSONObject data = new JSONObject();
		data.SetField("STORY_ID", _storyKey);
		data.SetField("TEAM", _TeamIndex);
        data.SetField("FRIEND_UID", _strikerUID);

        Send("REQ_STORY", "101", data);
    }

	internal void req_Story_Done(bool _clear, int _storyKey, bool[] _mission, int _ovkDmg, int _teamIdx)
	{
		JSONObject data = new JSONObject();
		data.SetField("CLEAR", _clear ? 1 : 0);
		data.SetField("STORY_ID", _storyKey);
		data.SetField("MISSION1", _clear & _mission[0] ? 1 : 0);
		data.SetField("MISSION2", _clear & _mission[1] ? 1 : 0);
		data.SetField("MISSION3", _clear & _mission[2] ? 1 : 0);
		data.SetField("OVERKILL", _ovkDmg);
        data.SetField("TEAM", _teamIdx);
        Debug.Log(data);
        Send("REQ_STORY", "102", data);
    }

	internal void Req_Story_Reward(int _chapterKey, int _level)
	{
		JSONObject data = new JSONObject();
		data.SetField("CHAPTER_ID", _chapterKey);
		data.SetField("LEVEL", _level);

        Send("REQ_STORY", "103", data);
    }

    public void Req_PvPQuitGradeTest()
    {
        //{ ATYPE: "000",GRADE: }

        JSONObject data = new JSONObject();
        data.AddField("GRADE", 0);

        Send("REQ_PVP", "000", data);
    }

    // Todo : 송신 패킷 추가시
    public void Req_PvPRankList()
    {
        JSONObject data = new JSONObject();
        //data.AddField("CHA_UID", charUID);

        Send("REQ_PVP", "100", data);
    }

	public void Req_PvPConfirmLastSeason()
	{
		JSONObject data = new JSONObject();
        Send("REQ_PVP", "101", data);
    }

	public void Req_PvPTeamInfo(long _userUID)
    {
        JSONObject data = new JSONObject();
        data.AddField("USER_UID", _userUID);

        Send("REQ_PVP", "103", data);
    }

	public void Req_PvPGradeTestMatchList()
    {
        //{ ATYPE: "000",GRADE: }

        JSONObject data = new JSONObject();
        data.AddField("COUNT", GameCore.Instance.PlayerDataMgr.ResearchCount);

        Send("REQ_PVP", "200", data);
    }

    public void Req_PvPMatchList()
    {
        //{ ATYPE: "000",GRADE: }

        JSONObject data = new JSONObject();
        Send("REQ_PVP", "200", data);
    }

	public void Req_PvPReDiscover()
	{
		JSONObject data = new JSONObject();
        Send("REQ_PVP", "201", data);
    }

	public void Req_PvPFinishPlacement(bool win)
	{
		JSONObject data = new JSONObject();
		data.AddField("VICTORY", win ? 1 : 0);

        Send("REQ_PVP", "202", data);
    }

	public void Req_PvPStartBattle()
	{
		JSONObject data = new JSONObject();
        Send("REQ_PVP", "203", data);
    }

	public void Req_PvPFinishBattle(int plyPower, int oppPower, bool win)
	{
		JSONObject data = new JSONObject();
		data.AddField("MY_COMBAT", plyPower);
		data.AddField("ENEMY_COMBAT", oppPower);
		data.AddField("VICTORY", win ? 1 : 0);

        Send("REQ_PVP", "204", data);
    }


    internal void Req_Friend_Request(long _uid)
    {
        JSONObject data = new JSONObject();
        data.AddField("FRIEND_UID", _uid);
        Send("REQ_FRIEND", "000", data);
    }

    internal void Req_Friend_List()
    {
        JSONObject data = new JSONObject();
        Send("REQ_FRIEND", "100", data);
    }

    internal void Req_Friend_Search(string _name)
    {
        JSONObject data = new JSONObject();
        data.AddField("USER_NAME", _name);
        Send("REQ_FRIEND", "101", data);
    }

    internal void Req_Friend_RecommendList(int _page)
    {
        JSONObject data = new JSONObject();
        data.AddField("PAGE", _page);
        Send("REQ_FRIEND", "102", data);
    }

    internal void Req_Friend_RequestedList()
    {
        JSONObject data = new JSONObject();
        Send("REQ_FRIEND", "103", data);
    }

    internal void Req_Friend_AcceptableList()
    {
        JSONObject data = new JSONObject();
        Send("REQ_FRIEND", "104", data);
    }

    internal void Req_Friend_Accept_OK(long _uid)
    {
        JSONObject data = new JSONObject();
        data.AddField("FRIEND_UID", _uid);
        Send("REQ_FRIEND", "200", data);
    }

    internal void Req_Friend_Accept_NO(long _uid)
    {
        JSONObject data = new JSONObject();
        data.AddField("FRIEND_UID", _uid);
        Send("REQ_FRIEND", "201", data);
    }

    // Not Use
    internal void Req_Friend_Accept_Cancel(long _uid)
    {
        JSONObject data = new JSONObject();
        data.AddField("FRIEND_UID", _uid);
        Send("REQ_FRIEND", "300", data);
    }

    internal void Req_Friend_Remove(long _uid)
    {
        JSONObject data = new JSONObject();
        data.AddField("FRIEND_UID", _uid);
        Send("REQ_FRIEND", "301", data);
    }

    internal void Req_Friend_Send(long _uid)
    {
        JSONObject data = new JSONObject();
        data.AddField("FRIEND_UID", _uid);
        Send("REQ_FRIEND", "302", data);
    }

    internal void Req_Friend_Receive(long _uid)
    {
        JSONObject data = new JSONObject();
        data.AddField("FRIEND_UID", _uid);
        Send("REQ_FRIEND", "303", data);
    }

    internal void Req_Friend_TeamInfo(long _uid, int _teamNum)
    {
        JSONObject data = new JSONObject();
        data.AddField("FRIEND_UID", _uid);
        data.AddField("TEAM_NO", _teamNum);
        Send("REQ_FRIEND", "105", data);
    }

    internal void Req_Friend_Striker()
    {
        JSONObject data = new JSONObject();
        Send("REQ_FRIEND", "106", data);
    }

    internal void Req_Dungeon_Daily_Start(int _key, int _team, long _friendUID )
    {
        JSONObject data = new JSONObject();
        data.AddField("DUNGEON_ID", _key);
        data.AddField("TEAM", _team+1);
        data.AddField("FRIEND_UID", _friendUID);
        Send("REQ_DUNGEON", "100", data);
    }

    internal void Req_Dungeon_Daily_End(int _key, bool _win)
    {
        JSONObject data = new JSONObject();
        data.AddField("DUNGEON_ID", _key);
        data.AddField("VICTORY", _win ? 1 : 0);
        Send("REQ_DUNGEON", "200", data);
    }

    internal void Req_Dungeon_Daily_Buy_Ticket()
    {
        JSONObject data = new JSONObject();
        Send("REQ_DUNGEON", "201", data);
    }

    internal void Req_Raid_Prepare()
    {
        JSONObject data = new JSONObject();
        Send("REQ_RAID", "100", data);
    }

    internal void Req_Raid_Start(int _key, int _teamIdx, long _friendUID)
    {
        JSONObject data = new JSONObject();
        data.AddField("RAID_ID", _key);
        data.AddField("TEAM", _teamIdx+1);
        data.AddField("FRIEND_UID", _friendUID);
        Send("REQ_RAID", "101", data);
    }

    internal void Req_Raid_End(int _key, int _addedDmg, float _time, int _teamIdx)// pr.raidKey, pr.addedDmg, pr.playTime, pr.playerTeamIdx)
    {
        JSONObject data = new JSONObject();
        data.AddField("RAID_ID", _key);
        data.AddField("DAMAGE", _addedDmg);
        data.AddField("TAKE_TIME", _time);
        data.AddField("POWER", GameCore.Instance.PlayerDataMgr.GetTeamPower(_teamIdx));
        data.AddField("TEAM", _teamIdx + 1);
        Send("REQ_RAID", "200", data);
    }

    internal void Req_Raid_MyRank(int _key)
    {
        JSONObject data = new JSONObject();
        data.AddField("RAID_ID", _key);
        Send("REQ_RAID", "102", data);
    }

    internal void Req_Raid_Rank50(int _key)
    {
        JSONObject data = new JSONObject();
        data.AddField("RAID_ID", _key);
        Send("REQ_RAID", "103", data);
    }

    internal void Req_Raid_TeamInfo(long _userUID)
    {
        JSONObject data = new JSONObject();
        data.AddField("FRIEND_UID", _userUID);
        Send("REQ_RAID", "104", data);
    }


    internal void Req_Mail_List()
    {
        JSONObject data = new JSONObject();
        Send("REQ_MAIL", "100", data);
    }

    internal void Req_Mail_Get(long _mailUID)
    {
        JSONObject data = new JSONObject();
        data.AddField("MAIL_UID", _mailUID);
        Send("REQ_MAIL", "000", data);
    }

    internal void Req_Check_GenItem(int _key)
    {
        JSONObject data = new JSONObject();
        data.AddField("ITEM_ID", _key);
        Send("REQ_ITEM", "200", data);
    }

    internal void Req_Mission_List()
    {
        JSONObject data = new JSONObject();
        Send("REQ_MISSION", "100", data);
    }

    internal void Req_Character_Power(int _power)
    {
        JSONObject data = new JSONObject();
        data.AddField("P", _power);
        Send("REQ_CHARACTER", "205", data);
    }

    internal void Req_Mission_Reward(MissionType _type, int _uid)
    {
        JSONObject data = new JSONObject();
        data.AddField("T", (int)_type);
        data.AddField("UID", _uid);
        Send("REQ_MISSION", "000", data);
    }

    internal void Req_Mission_Reward_Top(MissionType _type, int _uid)
    {
        JSONObject data = new JSONObject();
        data.AddField("T", (int)_type);
        data.AddField("UID", _uid);
        Send("REQ_MISSION", "001", data);
    }


    internal void Req_Character_Comment_New(int _charKey, int _score, string _comment)
    {
        JSONObject data = new JSONObject();
        data.AddField("CHA_ID", _charKey);
        data.AddField("SCORE", _score);
        data.AddField("COMMENT", _comment);
        Send("REQ_CHARACTER", "000", data);
    }

    internal void Req_Character_Comment_Edit(int _charKey, int _score, string _comment)
    {
        JSONObject data = new JSONObject();
        data.AddField("CHA_ID", _charKey);
        data.AddField("SCORE", _score);
        data.AddField("COMMENT", _comment);
        Send("REQ_CHARACTER", "202", data);
    }

    internal void Req_Character_Evaluate(long _userUID, int _charKey, bool _like)
    {
        JSONObject data = new JSONObject();
        data.AddField("USER_UID", _userUID);
        data.AddField("CHA_ID", _charKey);
        data.AddField("RECOMMAND", _like ? 1 : 0);
        Send("REQ_CHARACTER", "203", data);
    }

    internal void Req_Character_Evaluate_List(long _charKey, bool _sortOrderNew, int _startIdx)
    {
        JSONObject data = new JSONObject();
        data.AddField("CHA_ID", _charKey);
        data.AddField("ORDERTYPE", _sortOrderNew ? 1 : 0);
        data.AddField("START", _startIdx);
        Send("REQ_CHARACTER", "100", data);
    }
    internal void Req_Character_Evaluate_Mine(long _charKey)
    {
        JSONObject data = new JSONObject();
        data.AddField("CHA_ID", _charKey);
        Send("REQ_CHARACTER", "101", data);
    }

    internal void Req_Enchant_Strengthen_Exp(long _charUID, CardBase[] _stuffCharCards)
    {
        JSONObject data = new JSONObject();

        var target = new JSONObject(JSONObject.Type.ARRAY);
        target.Add(_charUID);
        data.AddField("T", target);

        var stuff = new JSONObject(JSONObject.Type.ARRAY);
        if (_stuffCharCards != null)
            for (int i = 0; i < _stuffCharCards.Length; ++i)
                stuff.Add(_stuffCharCards[i].SData.uid);
        data.AddField("UL", stuff);

        Send("REQ_CHARACTER", "206", data);
    }

    internal void Req_Enchant_Strengthen(long _charUID)
    {
        JSONObject data = new JSONObject();

        var target = new JSONObject(JSONObject.Type.ARRAY);
        target.Add(_charUID);
        data.AddField("T", target);

        Send("REQ_CHARACTER", "207", data);
    }

    internal void Req_Enchant_Evolution(long _charUID)
    {
        JSONObject data = new JSONObject();

        var target = new JSONObject(JSONObject.Type.ARRAY);
        target.Add(_charUID);
        data.AddField("T", target);

        Send("REQ_CHARACTER", "208", data);
    }

    internal void Req_Item_Equip(long _charUID, params long[] _itemUIDs)
    {
        JSONObject data = new JSONObject();

        var t = new JSONObject(JSONObject.Type.ARRAY);
        t.Add(_charUID);
        data.AddField("T", t);

        var il = new JSONObject(JSONObject.Type.ARRAY);
        for(int i = 0; i < _itemUIDs.Length; ++i)
            il.Add(_itemUIDs[i]);
        data.AddField("IL", il);

        Send("REQ_ITEM", "201", data);
    }

    internal void Req_Item_Equip_Change(long _charUID, params long[] _itemUIDs)
    {
        JSONObject data = new JSONObject();

        var t = new JSONObject(JSONObject.Type.ARRAY);
        t.Add(_charUID);
        data.AddField("T", t);

        var il = new JSONObject(JSONObject.Type.ARRAY);
        for (int i = 0; i < _itemUIDs.Length; ++i)
            il.Add(_itemUIDs[i]);
        data.AddField("IL", il);

        Send("REQ_ITEM", "202", data);
    }

    internal void Req_Item_Unequip(long _charUID, params long[] _itemUIDs)
    {
        JSONObject data = new JSONObject();

        var t = new JSONObject(JSONObject.Type.ARRAY);
        t.Add(_charUID);
        data.AddField("T", t);

        var il = new JSONObject(JSONObject.Type.ARRAY);
        for (int i = 0; i < _itemUIDs.Length; ++i)
            il.Add(_itemUIDs[i]);
        data.AddField("IL", il);

        Send("REQ_ITEM", "203", data);
    }

    internal void Req_Item_Strengthen_Exp(long _itemUID, CardBase[] _stuffitemCards)
    {
        JSONObject data = new JSONObject();

        var target = new JSONObject(JSONObject.Type.ARRAY);
        target.Add(_itemUID);
        data.AddField("T", target);

        var stuff = new JSONObject(JSONObject.Type.ARRAY);
        if (_stuffitemCards != null)
            for (int i = 0; i < _stuffitemCards.Length; ++i)
                stuff.Add(_stuffitemCards[i].SData.uid);
        data.AddField("IL", stuff);

        Send("REQ_ITEM", "204", data);
    }

    internal void Req_Item_Strengthen(long _itemUID)
    {
        JSONObject data = new JSONObject();

        var buy = new JSONObject(JSONObject.Type.ARRAY);
        buy.Add(_itemUID);
        data.AddField("T", buy);

        Send("REQ_ITEM", "205", data);
    }

    internal void Req_Shop_Buy(int index)
    {
        JSONObject data = new JSONObject();

        data.AddField("ID", index);

        Send("REQ_SHOP", "000", data);
    }

    internal void Req_Shop_Take_Item(int index, int condition)
    {
        JSONObject data = new JSONObject();

        data.AddField("ID", index);
        data.AddField("CON", condition);

        Send("REQ_SHOP","001", data);
    }

    internal void Req_Shop_Inquiry()
    {
        JSONObject data = new JSONObject();
        Send("REQ_SHOP", "100", data);
    }

    internal void Req_Shop_Inquiry_Item_Skin()
    {
        JSONObject data = new JSONObject();
        Send("REQ_SHOP", "101", data);
    }


    internal void Req_MyRoom_Buy(int _index)
    {
        JSONObject data = new JSONObject();
        data.AddField("MYROOM_ID", _index);
        Send("REQ_MYROOM", "000", data);
    }


    internal void Req_Attendance_Receive(int _aCheckKey)
    {
        JSONObject data = new JSONObject();
        data.AddField("AT", _aCheckKey);
        Send("REQ_ATTENDANCE", "000", data);
    }

    internal void Req_Attendance_Lookup()
    {
        JSONObject data = new JSONObject();
        Send("REQ_ATTENDANCE", "100", data);
    }


    internal void Req_Push_Save(PushSettingSData pushSetting)
    {

        JSONObject config = new JSONObject();
        config.AddField("behavior", pushSetting.vigor ? 1 : 0);
        config.AddField("raid", pushSetting.raid ? 1 : 0);
        config.AddField("pvp", pushSetting.pvp ? 1 : 0);
        config.AddField("event", pushSetting.evnt ? 1 : 0);
        config.AddField("myroom", pushSetting.clean ? 1 : 0);
        config.AddField("night", pushSetting.night ? 1 : 0);

        JSONObject data = new JSONObject();
        data.AddField("CONFIG", config);
        Send("REQ_PUSH", "200", data);
    }


    internal void Req_InitData(InitDataType _type)
    {
        JSONObject data = new JSONObject();
        data.AddField("TYPE", (int)_type);
        Send("REQ_INITDATA", "100", data);
    }


    internal void Req_Account_Delete()
    {
        JSONObject data = new JSONObject();
        Send("REQ_ACCOUNT", "205", data);
    }

    #endregion
}

