using System;
using System.Text;
using System.Timers;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour, ISequenceTransform
{
    ChatUI _chatUI;              // 채팅 UI

    // Top Left
    [SerializeField] UISprite _playerIcon;
    [SerializeField] UISprite _playerIconType;      // 대표 캐릭터 속성
    [SerializeField] UIButton _profileIcon;         // 대표 캐릭터 설정
    [SerializeField] UILabel _playerLevel;
    [SerializeField] UILabel _playerName;
    [SerializeField] UILabel _playerInfo;           //exp & 대표팀의 전투력(1번팀)
    
    //[NOTE] 사용되지 않는 변수
    UIButton _btGoogle;                         

    // Left
    [SerializeField] UIButton _attedanceCheck;      // 출석 체크
    [SerializeField] UIButton _invenTab;            // 인벤
    [SerializeField] UIButton _teamTab;             // 팀편집
    [SerializeField] UIButton _storeTab;            // 상점
    [SerializeField] UIButton _hotTimeTab;          // 핫타임

    // Right
    [SerializeField] UIButton _storyTab;            // 스토리
    [SerializeField] UIButton _adventureTab;        // 모험
    [SerializeField] UIButton _pvpTab;              // PVP
    [SerializeField] UIButton _myRoomTab;           // 숙소

    // Bottom Right
    [SerializeField] UIButton _mission;             // 미션
    [SerializeField] UIButton _make;                // 제조
    [SerializeField] UIButton _gacha;               // 뽑기
    [SerializeField] UIButton _farming;             // 파밍

    // Chat
    [SerializeField] UILabel _chatLabel;            // 채팅 내용
    [SerializeField] UIButton _chatButton;          // 채팅 버튼

    // Character
    [SerializeField] UI2DSprite _illust;            // 원화
    [SerializeField] GameObject _speechRoot;        // 말풍선 루트
    [SerializeField] UILabel _speech;               // 대사
    UnitDataMap typicaldata;                        // 대사를 위한 데이터맵 정보
    CharacterDialogueScript characterDialogueScript;

    // Banner
    [SerializeField] BannerScript _banner;

    PlayerInfoScript info;                          // 대표캐릭터 설정 스크립트
    internal long tmpUID { get; private set; }      // 대표 캐릭터 변경시 사요오딜 임시 변수
    internal string tmpComment { get; private set; }// 코멘트 변경시 사용될 임시변수

    //채팅 필터링
    List<string> chatFilterList;

    GachaDataMap[] waitdata;

    internal void Init()
    {
        _chatUI = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Lobby/PanelChatUI", transform).GetComponent<ChatUI>();
        _chatUI.Init();
        _chatUI.gameObject.SetActive(false);


        #region [5Star]에서 외부로부터 데이터를 받아오던 형식 : 현재는 변경된 사항
        // [NOTE] : 변수명 리팩토링 중 외부에서 어떠한 데이터를 참조해야하는지 모를때 참고할 것.

        //// Top Left
        //_playerIcon = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "profileIcon");
        //_playerIconType = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "typeIcon");
        //_profileIcon = _playerIcon.GetComponent<UIButton>();
        //_playerLevel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "playerLevel");
        //_playerName = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "playerName");
        //_playerInfo = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "expBattleValue");
        //_btGoogle = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "googlePlayIcon");

        //// Left
        //_attedanceCheck = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnCheck");
        //_invenTab = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnInven");
        //_teamTab = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnTeam");
        //_storeTab = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnStore");
        //_hotTimeTab = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnHotTime");

        //// Right
        //_storyTab = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnStory");
        //_adventureTab = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnAdventure");
        //_pvpTab = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnPVP");
        //_myRoomTab = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnMyRoom");

        //// Bottom Right
        //_mission = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnMission");
        //_make = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnMake");
        //_gacha = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnGacha");
        //_farming = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnFarming");

        //// Chat
        //_chatLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "chatLabel");
        //_chatButton = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "chatButton");

        //// Character
        //_illust = UnityCommonFunc.GetComponentByName<UI2DSprite>(gameObject, "illust");
        //_speechRoot = UnityCommonFunc.GetGameObjectByName(gameObject, "speechBubble");
        //_speech = UnityCommonFunc.GetComponentByName<UILabel>(_speechRoot, "lbSpeechBubble");


        //// banner
        //_banner = UnityCommonFunc.GetComponentByName<BannerScript>(gameObject, "bannerMask");

        #endregion

        _illust.GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickIllust));
        characterDialogueScript = CharacterDialogueScript.Create(this.transform);

        // Top Left
        var typicalSData = GameCore.Instance.PlayerDataMgr.GetUserMainUnitSData();
        if (typicalSData != null)
        {
            typicaldata = GameCore.Instance.DataMgr.GetUnitData(typicalSData.key);
            GameCore.Instance.SetUISprite(_playerIcon, typicaldata.GetSmallProfileSpriteKey());
            _playerIconType.spriteName = CardDataMap.GetTypeSpriteName(typicaldata.charType);

            // Character
            GameCore.Instance.SetUISprite(_illust, typicaldata.GetIllustSpeiteKey());
            GameCore.Instance.ResourceMgr.GetObject<Sprite>(ABType.AB_Atlas, 
                typicaldata.GetIllustSpeiteKey(), (sprite) => _illust.GetComponent<UIButton>().normalSprite2D = sprite);
            _speechRoot.SetActive(false);
        }
        else
        {
            _playerIcon.atlas = null;
            _playerIconType.spriteName = "";

            // Character
            _illust.sprite2D = null;
            _speechRoot.SetActive(false);
        }
        SetButton(_profileIcon, OnClickPlayerInfo, null, null);
        _playerLevel.text = "LV." + GameCore.Instance.PlayerDataMgr.Level;
        _playerName.text = GameCore.Instance.PlayerDataMgr.Name;
        int power = 0;
        for (int i = 0; i < 6; i++)
        {
            var uid = GameCore.Instance.PlayerDataMgr.GetTeamIds(0, i);
            if (uid != -1)
            {
                var unit = GameCore.Instance.PlayerDataMgr.GetUnitSData(uid);
                if (unit == null)
                    continue;

                power += unit.GetPower();
            }
        }

        if (GameCore.Instance.PlayerDataMgr.MaxExp == 0)
            _playerInfo.text = string.Format(CSTR.playerInfo, '-', '-', power);
        else
            _playerInfo.text = string.Format(CSTR.playerInfo, GameCore.Instance.PlayerDataMgr.Exp, GameCore.Instance.PlayerDataMgr.MaxExp, power);

        // Left
        SetButton(_attedanceCheck, OnClickAttendanceCheck, "출첵",   null);
        SetButton(_invenTab,          OnClickInven,           "가방",   null);
        SetButton(_teamTab,           OnClickTeam,            "팀 편집",null);
        SetButton(_storeTab,          OnClickStore,           "상점",   null);
        SetButton(_hotTimeTab,        OnClickHotTime,         "핫타임", null);

        // Right
        SetButton(_storyTab,          OnClickStory,           "스토리", null);
        SetButton(_adventureTab,      OnClickAdventure,       "모험",   null);
        SetButton(_pvpTab,            OnClickPVP,             "PVP",    null);
        SetButton(_myRoomTab,        OnClickMyRoom,          "숙소",   null);

        // Bottom Right
        SetButton(_mission,        OnClickMission,         "미션", null);
        SetButton(_make,           OnClickMake,            "제조", null);
        SetButton(_gacha,          OnClickGacha,           "뽑기", null);
        SetButton(_farming,        OnClickFarming,         "파밍", null);

        // Chat
        SetButton(_chatButton,           OnClickChat,            "채팅", null);
        _chatLabel.text = "";

        // Banner
        _banner.Init();
        //banner.SetBannerSprite("MAIN_BANNER_01");
        //banner.SetBannerSprite("MAIN_BANNER_02");
        //banner.SetBannerSprite("MAIN_BANNER_03");
        //banner.SetBannerSprite("MAIN_BANNER_04");
        //banner.SetBannerSprite("MAIN_BANNER_05");
        //banner.SetBannerSprite("MAIN_BANNER_06");

        // TopUI
        //GameCore.Instance.CommonSys.UpdateTopUI();
        GameCore.Instance.CommonSys.ResetUI();

        GachaFreeCool();
        MissionRewardStart();

        UpdateHotTime();
    }

    public void UpdateHotTime()
    {
        GameCore.Instance.PlayerDataMgr.UpdateHotTimeData();
        _hotTimeTab.gameObject.SetActive(GameCore.Instance.PlayerDataMgr.GetHotTimeCount() != 0);
    }

    internal void ClearChat()
    {
        _chatLabel.text = "";
        _chatUI.ClearChat();
    }

    IEnumerator MissionReward()
    {
        yield return new WaitForSeconds(0.2f);
        if (GameCore.Instance.MissionMgr.TakeNotificationCheck()) _mission.transform.GetChild(1).gameObject.SetActive(true);
        else _mission.transform.GetChild(1).gameObject.SetActive(false);
    }

    internal void MissionRewardStart()
    {
        StartCoroutine(MissionReward());
    }

    internal void GachaFreeCool()
    {

        var playerData = GameCore.Instance.PlayerDataMgr;
        DataMapCtrl<GachaDataMap> table = (DataMapCtrl<GachaDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.Gacha);
        var it = table.GetEnumerator();
        waitdata = new GachaDataMap[1];
        while (it.MoveNext())
        {
            var data = it.Current.Value;
            if (data.group == 5100001)
            {
                waitdata[0] = data;
                break;
            }
        }
        System.DateTime[] GachaColl = new System.DateTime[3];
        GachaColl[0] = playerData.GetFreeGachaCool(5000001) + new TimeSpan(0,0, waitdata[0].waitTime);
        GachaColl[1] = playerData.GetFreeGachaCool(5000006) + new TimeSpan(0, 0, waitdata[0].waitTime);
        GachaColl[2] = playerData.GetFreeGachaCool(5000011) + new TimeSpan(0, 0, waitdata[0].waitTime);
        for (int i = 0; i < GachaColl.Length; ++i)
        {
            if (GachaColl[i] < GameCore.nowTime) { _gacha.transform.GetChild(1).gameObject.SetActive(true); break; }
        }
        if(GachaColl[0] > GameCore.nowTime && GachaColl[1] > GameCore.nowTime && GachaColl[2] > GameCore.nowTime) _gacha.transform.GetChild(1).gameObject.SetActive(false);

    }

    internal void FarmingReward()
    {
        var count = GameCore.Instance.PlayerDataMgr.GetFarmingCount();
        for(int i = 0; i < count; ++i)
        {
            if (GameCore.Instance.PlayerDataMgr.GetFarmingData(i).endTime < GameCore.nowTime) { _farming.transform.GetChild(2).gameObject.SetActive(true); break; }
            else _farming.transform.GetChild(2).gameObject.SetActive(false);
        }
    }

    internal void MakeReward()
    {
        var it = ((DataMapCtrl<MakingSlotCostDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.MakingSlotCost)).GetEnumerator();
        while (it.MoveNext())
        {
            var data = it.Current.Value;
            var makeDataID = GameCore.Instance.DataMgr.GetMakingSlotData(data.id).id;
            var makeSData = GameCore.Instance.PlayerDataMgr.GetMakeData(makeDataID);
            if (makeSData != null)
            {
                if (makeSData.IsMakeDone()) { _make.transform.GetChild(1).gameObject.SetActive(true); break; }
                else _make.transform.GetChild(1).gameObject.SetActive(false);
            }
        }

    }

    internal void MyRoomReward(int myroomCount)
    {
        if (myroomCount <= 0) _myRoomTab.transform.GetChild(2).gameObject.SetActive(false);
        else _myRoomTab.transform.GetChild(2).gameObject.SetActive(true);
    }

    internal void SetButtonHighlight_Attendance(bool _active)
    {
        SetButtonHighlight(_attedanceCheck, _active);
    }


    private void SetButton(UIButton _btn, EventDelegate.Callback _cb, string _lb1, string _lb2, bool _highLight = false)
	{
		_btn.onClick.Add(new EventDelegate(_cb));
		SetButtonHighlight(_btn, _highLight);

		if (_lb1 == null) return;
		UnityCommonFunc.GetComponentByName<UILabel>(_btn.gameObject, "label1").text = _lb1;
		if (_lb2 == null) return;
		UnityCommonFunc.GetComponentByName<UILabel>(_btn.gameObject, "label2").text = _lb2;
	}

	private void SetButtonHighlight(UIButton _btn , bool _active)
	{
		UnityCommonFunc.GetGameObjectByName(_btn.gameObject, "highlight").SetActive(_active);
	}

	// Top Left Callbacks
	private void OnClickPlayerInfo()
	{
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
		info = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Lobby/playerInfo", transform).GetComponent<PlayerInfoScript>();
		info.Init(CBChangePlayerInfo);
		GameCore.Instance.ShowObject("대표 캐릭터", null, info.gameObject, 0,new MsgAlertBtnData[1] { new MsgAlertBtnData("확인", new EventDelegate(() => {
            info.GetComment();
            info.Destroy();
			info = null;
			GameCore.Instance.CloseMsgWindow();
		}), true, null, SFX.Sfx_UI_Confirm) });
	}

	private void CBChangePlayerInfo(long _uid, string _comment)
	{
        if ((GameCore.Instance.PlayerDataMgr.Comment.CompareTo(_comment) == 0 ||
             _comment.CompareTo(string.Empty) == 0) &&
            GameCore.Instance.PlayerDataMgr.MainCharacterUID == _uid)
            return;

		tmpUID = _uid;
		tmpComment = _comment;

		_comment = JsonTextParse.ToJsonText(_comment);
		GameCore.Instance.NetMgr.Req_Account_Change_ComDele(_uid, _comment);
	}

	internal void InfoReset()
	{
		if (info != null)
			info.ResetData();

		// Lobby의 LT 아이콘과 중앙의 스프라이트 교체
		var typicalSData = GameCore.Instance.PlayerDataMgr.GetUserMainUnitSData();
		if (typicalSData != null)
		{
			typicaldata = GameCore.Instance.DataMgr.GetUnitData(typicalSData.key);
			GameCore.Instance.SetUISprite(_playerIcon, typicaldata.GetSmallProfileSpriteKey());
            GameCore.Instance.SetUISprite(_playerIcon.GetComponent<UIButton>(), typicaldata.GetSmallProfileSpriteKey());

			_playerIconType.spriteName = CardDataMap.GetTypeSpriteName(typicaldata.charType);

            // Character
            GameCore.Instance.SetUISprite(_illust, typicaldata.GetIllustSpeiteKey());
            GameCore.Instance.SetUISprite(_illust.GetComponent<UIButton>().normalSprite2D, typicaldata.GetIllustSpeiteKey());

            _speechRoot.SetActive(false);
		}
		else
		{
			_playerIcon.atlas = null;
			_playerIconType.spriteName = "";

			// Character
			_illust.sprite2D = null;
			_speechRoot.SetActive(false);
		}
	}

	internal void AddChat(string name, string text, int key)
	{
		// 특수문자 파싱
		text = JsonTextParse.FromJsonText(text);

        _chatLabel.color = new Color32(0x89, 0x89, 0x89, 0xFF);
        _chatLabel.supportEncoding = false;
        _chatLabel.text = text;
		_chatUI.AddChat(name, text, key);

    }

    internal void AddChat(int charId, string userName, string itemName, int itemGread)
    {
        var itemGreadStr = "";
        switch (itemGread % 10)
        {
            case 0: itemGreadStr = "SSS"; break;
            case 1: itemGreadStr = "SS"; break;
            case 2: itemGreadStr = "S"; break;
            case 3: itemGreadStr = "A"; break;
            case 4: itemGreadStr = "B"; break;
        }

        _chatLabel.color = Color.white;
        _chatLabel.supportEncoding = true;
        _chatLabel.text = userName + "[898989] 님이 [-] [F600FF]" + itemGreadStr + "등급 " + itemName + "[-]" + "[898989] 를 획득하셨습니다. [-]";
        _chatUI.AddChat(charId, userName, itemName, itemGreadStr);

    }


	private void EnableSpeech(string _str)
	{
		_speechRoot.SetActive(true);
        string textValue = GameCore.Instance.DataMgr.GetProfileStringData(typicaldata.charIdType).dialogues[UnityEngine.Random.Range(0, 3)];
        _speech.text = textValue.Replace("\\n", "\n");
        //lbSpeech.text = _str;
    }
	private void DisableSpeech()
	{
		_speechRoot.SetActive(false);
	}

	private void OnClickIllust()
	{
        //lbSpeech.text = textValue.Replace("\\n", "\n");
        characterDialogueScript.SetDialogue(_illust.transform, typicaldata, DialogueType.Lobby);
        /*
		if (!speechRoot.activeSelf)
			EnableSpeech("말풍선 미구현");
		else
			DisableSpeech();
            */
    }

	//Left Callbacks
	private void OnClickAttendanceCheck()
	{
        //GameCore.Instance.ShowAlert("출석체크 이벤트는 오픈 이후부터 시작됩니다.");
        GameCore.Instance.ChangeSubSystem(SubSysType.Attendance, null);
    }
	private void OnClickInven()
	{
		GameCore.Instance.ChangeSubSystem(SubSysType.Inven, null);
	}
	private void OnClickTeam()
	{
		GameCore.Instance.ChangeSubSystem(SubSysType.EditTeam, null);
	}
	private void OnClickStore()
	{
        GameCore.Instance.ChangeSubSystem(SubSysType.Shop, null);
	}
    private void OnClickHotTime()
    {
        GameCore.Instance.PlayerDataMgr.UpdateHotTimeData();
        if(GameCore.Instance.PlayerDataMgr.GetHotTimeCount() == 0)
        {
            _hotTimeTab.gameObject.SetActive(false);
            return;
        }

        var htAlert = HotTimePopup.Create(GameCore.Instance.Ui_root);
        htAlert.Init(GameCore.Instance.PlayerDataMgr.GetHotTimeDatas());
        GameCore.Instance.ShowObject("핫타임", null, htAlert.gameObject, 1, new MsgAlertBtnData[]{ new MsgAlertBtnData("확인", new EventDelegate(GameCore.Instance.CloseMsgWindow))});
    }

    // Right Callbacks
    private void OnClickStory()
	{
		GameCore.Instance.ChangeSubSystem(SubSysType.Story, null);
	}
	private void OnClickAdventure()
	{
        GameCore.Instance.ShowObject("", null, AdventureUI.Create(transform).gameObject, 4, new MsgAlertBtnData[] { new MsgAlertBtnData("닫기", null) });
    }
	private void OnClickPVP()
	{
        if (SubSysBase.CheckSuddenQuit.SuddenQuitReset() == true)
            return;
    
		if (!GameCore.Instance.PlayerDataMgr.PvPData.placement)    
			GameCore.Instance.ChangeSubSystem(SubSysType.PvPGradeTest, null);
		else
			GameCore.Instance.ChangeSubSystem(SubSysType.PvPReady, null);
	}
    public void OnClickMyRoom()
    {
        GameCore.Instance.ChangeSubSystem(SubSysType.MyRoom, null);
    }

    // Bottom Right Callbacks
    internal void OnClickMission()
	{
        GameCore.Instance.NetMgr.Req_Mission_List();
        GameCore.Instance.MissionMgr.ShowFlag = true;
        //GameCore.Instance.MissionMgr.Mission_Start();
    }

    internal void OnClickMake()
	{
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
		GameCore.Instance.ChangeSubSystem(SubSysType.Make, null);

	}
	internal void OnClickGacha()
	{
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
		GameCore.Instance.ChangeSubSystem(SubSysType.Gacha, null);
	}
	internal void OnClickFarming()
	{
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
		GameCore.Instance.ChangeSubSystem(SubSysType.Farming, null);
	}

	// Chat Callbacks
	internal void OnClickChat()
	{
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
		_chatUI.gameObject.SetActive(true);
        _chatUI.Init();
	}

    public List<ReturnTutorialData> GetTutorialTransformList(int tutorialNum)
    {
        List<ReturnTutorialData> nTutorialList = new List<ReturnTutorialData>();
        switch (tutorialNum)
        {
            case 1:
                nTutorialList.Add(new ReturnTutorialData(_storyTab.transform, 0));
                break;
            case 2:
                nTutorialList.Add(new ReturnTutorialData(_gacha.transform, 0));
                break;
            case 3:
                nTutorialList.Add(new ReturnTutorialData(_teamTab.transform, 0));
                break;
            case 4:
                nTutorialList.Add(new ReturnTutorialData(_storyTab.transform, 0));
                break;
            case 5:
                nTutorialList.Add(new ReturnTutorialData(_storyTab.transform, 0));
                break;
            case 6:
                nTutorialList.Add(new ReturnTutorialData(_invenTab.transform, 0));
                break;
            case 7:
                nTutorialList.Add(new ReturnTutorialData(_invenTab.transform, 0));
                break;
            default:
                break;
        }
        return nTutorialList;
    }
}
