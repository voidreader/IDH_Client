using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System.Threading;
using GamePotUnity;

public class GameCore : MonoSingleton<GameCore>
{
	[Space]
	public SubSysType StartSystem = SubSysType.Loading;
	[HideInInspector] public Transform Ui_root = null;
	[HideInInspector] public Transform World_root = null;

	private static SubSysMgr subsysMgr;
	
	private GameDataMgr dataMgr;
	private PlayerDataMgr playerMgr;
	private ResourceMgr rscMgr;
	//private SoundMgr sndMgr;
	private GameNetworkMgr netMgr;
    private GameMissionMgr missionMgr;

    private ChatMgr chatMgr;

    private CommonSys commonSys;

    private Camera worldCam, uiCam;

    private GameEventMgr evtMgr;

    private SoundManager soundMgr;

    //#if UNITY_EDITOR
    public bool bTestTeamSkill;
    public bool bTestStrikeSkill; 
    [Space(order = -1)]
    public bool bLogLoadTable;
    public bool BLogNetworkData;
    public bool bOffline; // 서버통신으로 동작하지 않을 경우
    public bool bLogSound;
    public bool bLogSpineAnmation;
    public int UserID = 0;// 계정 변경용 식별자
    public string userIDStr;

    public bool enableTutorial = false; // 튜토리얼 활성화여부
    public int fixedTutorialStartIndex = 1;// 고정 튜토리얼 시작 인덱스 설정(실사용 -1)

    public GameObject goBlockPanel;
    public GameObject goBlockPanelInvisable;

    //public string userIDStr;
    //#endif

#if UNITY_ANDROID
    public static AndroidJavaObject jo;
#endif

    public static DateTime prevTime;

    public static TimeSpan timeGap;        // 서버와 시간 차이
    public static DateTime nowTime { get { return DateTime.Now + timeGap; } } // 서버와 동기화된 현재 시간
    public static float timeScale = 1f;     // 인게임용 타임 스케일 값
    private int timeScaleChange = 1;
    private int timeScaleSave = 1;
    private int saveTimeValue;
    float[] timeScaleValue = { 0f, 1.3f, 2f, 3.5f, 1f };

    [SerializeField] GameObject touchEffect;
    public void StopTime()
    {
        saveTimeValue = TimeScaleChange;
        TimeScaleChange = 0;
    }
    public void SaveTime()
    {
        saveTimeValue = TimeScaleChange;
        TimeScaleChange = 1;
    }
    public void RevertTime()
    {
        TimeScaleChange = saveTimeValue;
        saveTimeValue = 0;
    }
    public int TimeSave
    {
        get
        {
            return timeScaleSave;
        }
        set
        {
            TimeScaleChange = value;
            //PlayerPrefs.SetFloat("TimeScale", GameCore.timeScale);
            if (value > 0)
                PlayerPrefs.SetInt("TimeScaleValue", value);
        }
    }
    public int TimeScaleChange
    {
        get
        {
            return timeScaleChange;
        }
        set
        {
            timeScaleChange = value;
            timeScale = timeScaleValue[timeScaleChange];
        }
    }

    public static bool cachedPause = false;     // 앱 퍼즈 여부 (되어도 한프레임 update가 돌아서 이를 구분하기위해 사용)

    //public static float timeScale { get { return TimeScale; } set { TimeScale = value; Debug.Log("TimeScale : " + TimeScale); } }
    public static bool atuoPlay;                // 인게임용 오토여부
    public static bool atuoPlaySave;            // 인게임용 오토 저장을 위한 변수

    public static bool atuoStory = false;               // 인게임용 스토리 오토여부
    public static bool atuoStorySave = false;          // 인게임용 스토리 오토 저장을 위한 변수

    public static int chapterSave = -1;
    public static int stageSave = -1;
    public static int difficult = -1;
    public static void SetStageInfo(int _difficult, int _chapter, int _stage)
    {
        difficult = _difficult;
        chapterSave = _chapter;
        stageSave = _stage;
    }

    //public static TimeSpan STimeInterval;		// 서버와의 시간차
    //public static DateTime NowTime { get { return DateTime.Now + STimeInterval; } }

    internal SubSysMgr SubsysMgr { get { return subsysMgr; } }
    internal GameEventMgr EventMgr { get { return evtMgr; } }
	internal PlayerDataMgr PlayerDataMgr { get { return playerMgr; } }
	internal GameDataMgr DataMgr { get { return dataMgr; } }
	internal ResourceMgr ResourceMgr { get { return rscMgr; } }
	//internal SoundMgr SndMgr { get { return sndMgr; } }
    internal SoundManager SoundMgr { get { return soundMgr; } }
	internal GameNetworkMgr NetMgr { get { return netMgr; } }
    internal GameMissionMgr MissionMgr { get { return missionMgr; } }
    internal ChatMgr ChatMgr { get { return chatMgr; } }

#if !UNITY_EDITOR
    private GamePotManager gamePotMgr;
    internal GamePotManager GamePotMgr { get { return gamePotMgr; } }
#endif

    internal CommonSys CommonSys { get { return commonSys; } }
    internal ObjectPool<Transform> TouchEffectPool;
    public UIPanel TouchEffectPanel;

    private Action actionAfterLoading;
    
    public LobbyTutorial lobbyTutorial;    //튜토리얼을 위한 변수

    public bool bLogined { get { return PlayerPrefs.GetInt("logined", 0) != 0; } set { PlayerPrefs.SetInt("logined", value ? 1 : 0); } }


    public void SetActionAfterLoading(Action _action)
    {
        actionAfterLoading += _action;
    }
    public void ActionAfterLoading() {
        if (actionAfterLoading == null)
            return;
        actionAfterLoading();
        actionAfterLoading = null;
    }

    public static IEnumerator WaitForTime(float limitTime, Action _cb)
    {
        float presentTime = 0f;
        while (limitTime > presentTime)
        {
            presentTime += Time.deltaTime * GameCore.timeScale;
            yield return null;
        }
        _cb();
    }



    private void Awake()
    {
        //nowTime = DateTime.Now;
        #if UNITY_ANDROID || UNITY_IOS
        GamePot.initPlugin();
        #endif
    }
    private void Start()
	{
#if UNITY_EDITOR
        //gameObject.AddComponent<ShowFPS>();
#else
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		StartSystem = SubSysType.Loading;
#if UNITY_ANDROID || UNITY_IOS
 //           AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
  //          jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        gamePotMgr = new GamePotManager();
        GamePot.setListener(gamePotMgr);
#endif
//             Debug.Log(jo.ToString());
 //            jo.Call("toastTest");
  //           jo.Call("SetVibrate", 500);
#endif
        Init();
    }



    internal override void Init()
	{
		if (Ui_root != null)
			return;

#if !UNITY_EDITOR
        // 에러 로그 서버로 전송
        Application.logMessageReceived += CBLoggerToServer;
#endif
        Ui_root = GameObject.Find("UI Root").transform;
		World_root = GameObject.Find("World Root").transform;
		uiCam = GameObject.Find("Camera_UI").GetComponent<Camera>();
		worldCam = GameObject.Find("Camera_World").GetComponent<Camera>();
		Application.targetFrameRate = 300;

		if (subsysMgr != null)
		{
			GameCore.Instance.Reload();
			Destroy(gameObject);
			return;
		}
		Debug.Log("GameCore Init Start");

		DontDestroyOnLoad(gameObject);

		GetComponent<SocketIOComponent>().enabled = true;

		FilePath.Init();                // 파일 경로 데이터 초기화

		evtMgr = new GameEventMgr();    // 이벤트 핸들링 관리

//		dataMgr = new GameDataMgr();    // 데이터 관리(고정 데이터 관리)

		netMgr = new GameNetworkMgr(GetComponent<SocketIO.SocketIOComponent>());

		//playerMgr = new PlayerDataMgr();// 플레이어 데이터 관리

		rscMgr = new ResourceMgr();     // 리소스 관리

        //sceneMgr = new SceneMgr();		// 씬 관리

        ///sndMgr = new SoundMgr();				// 사운드 관리
        soundMgr = new SoundManager();

        subsysMgr = new SubSysMgr();    // 서브시스템 매니저

        missionMgr = new GameMissionMgr();

        chatMgr = gameObject.AddComponent<ChatMgr>();// new ChatMgr();
        chatMgr.Init();

        commonSys = subsysMgr.GetSubSys(SubSysType.Common) as CommonSys;

		//clientMgr = new ClientMgr();	// 서버와의 통신 담당
		//rscMgr = new ResourceMgr();		// 리소스 관리

		//alertMgr = new AlertMgr();		// 알림창 등 현재 UI와 별개로 사용되는 알람용 UI 매니저

		//dataMgr = new DataMgr(); // 퀘스트, 인벤 등 플레이어 정보와 게임에 필요한 데이터를 관리

		ChangeSubSystem(StartSystem, null);

        //var socketThread = new Thread(TestThreadRoutine);
        //socketThread.Start();
        TouchEffectPool = new ObjectPool<Transform>(touchEffect, TouchEffectPanel.transform);
    }

    public void NewPlayerData()
    {
        playerMgr = new PlayerDataMgr();
    }

    public void InitDataMgr()
    {
        if (dataMgr == null)
        {
            dataMgr = new GameDataMgr();    // 데이터 관리(고정 데이터 관리)
            missionMgr.InitBundleData();
        }
    }

    void CBLoggerToServer(string condition, string stackTrace, LogType type)
    {
        char ch;
        switch(type)
        {
            case LogType.Assert:    ch = 'A'; break;
            case LogType.Exception: ch = 'X'; break;
          //case LogType.Error:     ch = 'E'; break;
            default: return;
        }

        if (NetMgr == null || SubsysMgr == null)
            return;

        string msg = string.Format("[{0}][{1}][{2}] {3}\n{4}", ch, DateTime.Now, SubsysMgr.GetNowSysType(), condition, stackTrace);
        msg = msg.Replace("\n", "\\n");
        NetMgr.req_Logging(msg);
    }

    //----------------------------튜토리얼----------------------------------------------
    //튜토리얼 생성
    private void CreateTutorial()
    {
#if UNITY_EDITOR
        Debug.Log("Create Tutorial");
#endif
        if (lobbyTutorial != null) return;
       
        lobbyTutorial = LobbyTutorial.Create(GameCore.Instance.ui_root);
        lobbyTutorial.Init();
    }

    public void ResetTutorial()
    {
        if (lobbyTutorial == null) return;
        lobbyTutorial.gameObject.SetActive(true);
        lobbyTutorial.ResetTutorial();
    }

    public bool CheckTutorialAble(bool isFixed, int pos, Tutorial nTutorialData)
    {
        return LobbyTutorial.CheckCreateAble(nTutorialData, isFixed, pos);
    }
    private bool isFixedTutorial = true;
    private int tutorialPos = -1;
    public virtual void SetTutorialChild(bool _isFixed, int _pos, Func<int, List<Action>> getTutorialActionFunc,params Func<int, List<ReturnTutorialData>>[] getTutorialTransformFunc)
    {
#if UNITY_EDITOR
        // 튜토리얼 테스트용
        if (!enableTutorial)
            return;
#elif SKIP_TUTORIAL
        return;
#endif

        // 개별 튜토리얼 스킵
        if (!_isFixed)
            return;

        isFixedTutorial = _isFixed;
        tutorialPos = _pos;
        
        if (subsysMgr.GetNowSysType() == SubSysType.Lobby)
            ResetTutorial();
        Tutorial nTutorialData = GameCore.Instance.PlayerDataMgr.TutorialData;
        if (CheckTutorialAble(_isFixed, _pos, nTutorialData))
        {
            GameCore.Instance.DoWaitCall(() => {
                CreateTutorial();
                int tutorialPos = _isFixed == true ? nTutorialData.main + 1 : _pos;
                //lobbyTutorial.SetTutorialList(tutorialPos);
                //lobbyTutorial.AddTutorialChildData(getTutorialTransformFunc(tutorialPos));
                lobbyTutorial.SetTutorialList(_isFixed, tutorialPos);
                for (int i = 0; i < getTutorialTransformFunc.Length; i++)
                {
                    lobbyTutorial.AddTutorialChildData(getTutorialTransformFunc[i](tutorialPos));
                }
                if (getTutorialActionFunc != null)
                    lobbyTutorial.AddTutorialActionData(getTutorialActionFunc(tutorialPos));
                lobbyTutorial.TutorialStateData = LobbyTutorial.TutorialState.Prepare;
            });
        }
    }
    public virtual void SetTutorialChild(bool _isFixed, int _pos, Func<int, List<Action>> getTutorialActionFunc, List<ReturnTutorialData> returnTutorialDatas)
    {
        isFixedTutorial = _isFixed;
        tutorialPos = _pos;

        if (subsysMgr.GetNowSysType() == SubSysType.Lobby)
            ResetTutorial();
        Tutorial nTutorialData = GameCore.Instance.PlayerDataMgr.TutorialData;
        if (CheckTutorialAble(_isFixed, _pos, nTutorialData))
        {
            CreateTutorial();
            int tutorialPos = _isFixed == true ? nTutorialData.main + 1 : _pos;
            //lobbyTutorial.SetTutorialList(tutorialPos);
            //lobbyTutorial.AddTutorialChildData(getTutorialTransformFunc(tutorialPos));
            lobbyTutorial.SetTutorialList(_isFixed, tutorialPos);
            for (int i = 0; i < returnTutorialDatas.Count; i++)
            {
                lobbyTutorial.InsertTutorialChildData(returnTutorialDatas);
            }
            if (getTutorialActionFunc != null)
                lobbyTutorial.InsertTutorialActionData(getTutorialActionFunc(tutorialPos));
            if (lobbyTutorial.TutorialStateData == LobbyTutorial.TutorialState.Pause)
                StartCoroutine(CoWaitCall(20, () => { lobbyTutorial.TutorialStateData = LobbyTutorial.TutorialState.Prepare; }));

        }
    }
    public bool CheckTutorialBeforeTurnOn(params int[] pos)
    {
        for(int i = 0; i < pos.Length; i++)
        {
            if (lobbyTutorial.CheckTutorialBeforeTurnOn(pos[i]) == true)
                return true;
        }
        return pos.Length == 0 ? true : false;
    }

    public void TurnOnTutorial(Action action, params int[] pos)
    {
        if (lobbyTutorial == null || CheckTutorialBeforeTurnOn(pos) == false) return;
        lobbyTutorial.TurnOnTutorial(action);

    }
    //----------------------------튜토리얼 끝----------------------------------------------
    private void Reload()
	{
		Ui_root =       GameObject.Find("UI Root").transform;
		World_root =    GameObject.Find("World Root").transform;
		uiCam =         GameObject.Find("Camera_UI").GetComponent<Camera>();
		worldCam =      GameObject.Find("Camera_World").GetComponent<Camera>();

		commonSys.EnterSys(new CommonSysPara(false));
		ChangeSubSystem(subsysMgr.GetNextSubSysType(), subsysMgr.GetNextSubSysPara());
	}

    [SerializeField] bool timeReset = false;
    Touch tempTouchs;
    bool touchOn = false;
	private void Update()
	{
        if(timeReset == true)
        {
            timeReset = false;
            TimeScaleChange = 1;
        }

        if (netMgr != null)
			netMgr.Update();

        if( evtMgr != null)
			evtMgr.Update();

		if (subsysMgr != null )
			subsysMgr.Update();

        if (commonSys != null)
            commonSys.Update();

        if (SoundMgr != null)
            SoundMgr.Update();

        // 날짜가 바뀌면 요일던전 티켓을 리셋한다.
        if (OnMidnight())
            PlayerDataMgr.ResetDailyDungeonTicket();


#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) 
        { 
            StartTouchEffect(Input.mousePosition);
        }
#endif
#if UNITY_ANDROID || UNITY_IOS
        if(Input.touchCount > 0){
            for (int i = 0; i < Input.touchCount; i++){
                tempTouchs = Input.GetTouch(i);
                if(tempTouchs.phase == TouchPhase.Began) {
                    StartTouchEffect(tempTouchs.position);
                    touchOn = true;
                    break;
                }
            }
        }
#endif
    }


    internal void StartTouchEffect(Vector3 targetVec){
        Vector3 vec = UICamera.mainCamera.ScreenToWorldPoint(targetVec);
        Vector3 targetPosition = new Vector3(vec.x, vec.y, -0.5f);
        var tf = TouchEffectPool.BringObject();
        tf.position = targetPosition;
        tf.rotation = Quaternion.identity;
        Invoke("ReturnTouchEffectObject", 1f);
    }

    internal void ReturnTouchEffectObject(){
        TouchEffectPool.ReturnObjectAll();
    }

#region UtilMethod

    /// <summary>
    /// 비동기로 토딩중인 리소스가 있는지 여부를 반환
    /// </summary>
    /// <returns></returns>
    internal bool IsDoneResourceLoad()
    {
        return ResourceMgr.LoadingCount() == 0;
    }

    /// <summary>
    /// 테이블을 참고하여nGui Sprite의 이미지를 설정한다.
    /// </summary>
    /// <param name="_sp"></param>
    /// <param name="_spriteID"></param>
    internal void SetUISprite(UISprite _sp, int _spriteID)
    {
        var data = DataMgr.GetSpriteData(_spriteID);
        if (data == null)
        {
            Debug.LogError("Not Found Sprite Data. " + _spriteID);
            return;
        }

        _sp.spriteName = data.sprite_name;
        if (data.atlas_id != -1)
            ResourceMgr.GetObject<GameObject>(ABType.AB_Atlas, data.atlas_id, (go) => { if (go != null) _sp.atlas = go.GetComponent<UIAtlas>(); });
    }

    internal void SetUISprite(UIButton _button, int _spriteID)
    {
        var data = DataMgr.GetSpriteData(_spriteID);
        if(data == null)
        {
            Debug.LogError("Not Found Sprite Data " + _spriteID);
            return;
        }

        _button.normalSprite = data.sprite_name;
    }

    internal void SetUISprite(UISprite _sp, int _atlasID, string _spriteName)
    {
        _sp.spriteName = _spriteName;
        ResourceMgr.GetObject<GameObject>(ABType.AB_Atlas, _atlasID, (go) => { if (go != null) _sp.atlas = go.GetComponent<UIAtlas>(); });
    }

    internal void SetUISprite(UI2DSprite _sp, int _spriteID)
    {
        if (_sp != null && _spriteID > 0)
            ResourceMgr.GetObject<Sprite>(ABType.AB_Atlas, _spriteID, (sprite) => _sp.sprite2D = sprite);
        else
        {
            Debug.LogError("Data Invalid!" + _spriteID);
        }
    }

    internal void SetUISprite(Sprite _sp, int _spriteID)
    {
        if (_sp != null && _spriteID > 0)
        {
            ResourceMgr.GetObject<Sprite>(ABType.AB_Atlas, _spriteID, (sprite) => _sp = sprite);
        }
        else
        {
            Debug.LogError("Data Invalid!" + _spriteID);
        }
    }

    /// <summary>
    /// 한프레임 뒤 콜백을 호출한다.
    /// </summary>
    /// <param name="_cb"></param>
    internal void DoWaitCall(Action _cb)
    {
        StartCoroutine(CoWaitCall(_cb));
    }

    internal IEnumerator CoWaitCall(Action _cb)
    {
        yield return null;
        _cb();
    }

    /// <summary>
    /// 원하는 시간만큼 지연한 뒤 콜백을 호출한다.
    /// </summary>
    /// <param name="_time"></param>
    /// <param name="_cb"></param>
    internal void DoWaitCall(float _time, Action _cb)
    {
        StartCoroutine(CoWaitCall(_time, _cb));
    }
    internal static IEnumerator CoWaitCall(float _time, Action _cb)
    {
        yield return new WaitForSeconds(_time);
        _cb();
    }

    public void SetActiveBlockPanel(bool _show)
    {
        goBlockPanel.SetActive(_show);
    }

    public void SetActiveBlockPanelInvisable(bool _show)
    {
        goBlockPanelInvisable.SetActive(_show);
    }

    /// <summary>
    /// 원하는 프레임만큼 지연한 뒤 콜백을 호출한다.
    /// </summary>
    /// <param name="_time"></param>
    /// <param name="_cb"></param>
    internal void DoWaitCall(int _frame, Action _cb)
    {
        StartCoroutine(CoWaitCall(_frame, _cb));
    }
    internal static IEnumerator CoWaitCall(int _frame, Action _cb)
    {
        for (int i = 0; i < _frame; ++i)
            yield return null;
        _cb();
    }

    internal Vector3 WorldPositionToUiPosition(Vector3 worldPosition)
    {
        Vector3 reVal = worldPosition;
        reVal = GetWorldCam().WorldToScreenPoint(reVal);
        reVal.z = 0.0f;
        reVal = GetUICam().ScreenToWorldPoint(reVal);
        return reVal;
    }

    internal Vector3 UiPositionToWorldPosition(Vector3 uiPosition)
    {
        Vector3 reVal = uiPosition;
        reVal = GetUICam().WorldToScreenPoint(reVal);
        reVal = GetWorldCam().ScreenToWorldPoint(reVal);
        return reVal;
    }

    internal Vector3 WorldPosToUIPos(Vector3 _pos)
    {
        var p = GetWorldCam().WorldToViewportPoint(_pos);
        p -= new Vector3(0.5f, 0.5f, 0f);
        p.x *= GetWorldCam().aspect / (1280f / 720f);
        p.x *= 1280;
        p.y *= 720;
        p.z = 0f;

        return p;
    }

#endregion

#region GetMethod

    internal Camera GetWorldCam()
    {
        if (worldCam == null)
            worldCam = GameObject.Find("Camera_World").GetComponent<Camera>();
        return worldCam;
    }
    internal Camera GetUICam()
    {
        if (uiCam == null)
            uiCam = GameObject.Find("Camera_UI").GetComponent<Camera>();
        return uiCam;
    }

    internal Transform ui_root
    {
        get
        {
            if (Ui_root == null)
                Ui_root = GameObject.Find("UI Root").transform;
            return Ui_root;
        }
    }

    internal Transform world_root
    {
        get
        {
            if (World_root == null)
                World_root = GameObject.Find("World Root").transform;
            return World_root;
        }
    }


    internal string GetVersion()
    {
        return string.Format("{0}", Application.version);
    }
    public static string GetGCMRegID()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        //return jo.CallStatic<string>("GetGCMRegID");
		return null;
#else
        return null;
#endif
    }

    #endregion

    /// <summary>
    /// 날짜가 바뀌는 순간 true를 반환한다.
    /// </summary>
    /// <returns></returns>
    bool OnMidnight()
    {
        if (playerMgr == null || SubsysMgr == null || SubsysMgr.NowSysType == SubSysType.Loading)
            return false;


        bool result = prevTime.Day != nowTime.Day;
        prevTime = nowTime;

        return result;
    }


    /////////////////////Send Evnet Rapper/////////////////////
    internal void ChangeSubSystem(SubSysType _type, ParaBase _para)
    {
        //if (SubsysMgr.GetNowSubSys() != null && SubsysMgr.GetNowSysType() != SubSysType.Loading && _type != SubSysType.Loading)
        //    GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);

        ResourceMgr.UnloadAllAsset();
        //StartCoroutine(CoWaitCall(0.25f, () => { evtMgr.SendEvent(new GameEvent(GameEventType.ChangeSys, new ChangeSysPara(_type, _para))); }));
        evtMgr.SendEvent(new GameEvent(GameEventType.ChangeSys, new ChangeSysPara(_type, _para)));
	}

	internal void SetCBToReturnBtn(Action _cb)
	{
		EventMgr.SendEvent(new GameEvent(GameEventType.SetReturnCB, new SetReturnCBPara(_cb)));
	}
	internal void ShowAlert(string _str)
	{
		EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, MsgAlertPara.CreateAlertMsg(_str)));
	}

    internal void ShowNotice(string _title, string _str,  int _size, bool _canBack = true)
	{
		EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, MsgAlertPara.CreateNoticeMsg(_title, _str, _size, _canBack)));
	}

	internal void ShowNotice(string _title, string _str, EventDelegate.Callback _cbCorrect, int _size, bool _canBack = true)
	{
		EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, MsgAlertPara.CreateNoticeMsg(_title, _str, _cbCorrect, _size, _canBack)));
	}

    internal void ShowNotice(string _title, string _str, string _highlight, int _size, bool _canBack = true)
    {
        EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, MsgAlertPara.CreateNoticeMsg(_title, _str, _highlight, _size, _canBack)));
    }

    internal void ShowAgree(string _title, string _str, int _size, string _strCorrectBtn, EventDelegate.Callback _cbCorrect, bool _canBack = true)
    {
        EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, MsgAlertPara.CreateAgreeMsg(_title, _str, null, MoneyType.None, _strCorrectBtn, _cbCorrect, _size, _canBack)));
    }
    internal void ShowAgree(string _title, string _str, int _size, EventDelegate.Callback _cbCorrect, bool _canBack = true)
	{
        EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, MsgAlertPara.CreateAgreeMsg(_title, _str, null, MoneyType.None, _cbCorrect, _size, _canBack)));
	}
    internal void ShowAgree(string _title, string _str, int _size, EventDelegate.Callback _cbOKCorrect, EventDelegate.Callback _cbCancelCorrect, bool _canBack = true)
	{
        EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, MsgAlertPara.CreateAgreeMsg(_title, _str, null, MoneyType.None, _cbOKCorrect, _cbCancelCorrect, _size, _canBack)));
	}

	internal void ShowAgree(string _title, string _str, string _highlight, MoneyType moneyType, int _size, EventDelegate.Callback _cbCorrect)
	{
		EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, MsgAlertPara.CreateAgreeMsg(_title, _str, _highlight, moneyType, _cbCorrect, _size)));
	}

	internal void ShowObject(string _title, string _str, GameObject _object, int _size, EventDelegate.Callback _cbCorrect)
	{
		EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, _title,
					new MsgAlertBtnData[] { new MsgAlertBtnData("확인", new EventDelegate(()=> {
                        _cbCorrect(); }), true, null, SFX.Sfx_UI_Confirm), new MsgAlertBtnData("취소", null) }, _size, null, null, _object)));
	}
    internal void ShowObject(string _title, string _str, GameObject _object, int _size, MsgAlertBtnData[] _btns)
	{
        EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, _title, _btns, _size, _str, null, _object)));
	}

    internal void SetDynamicTutorial(Func<int, List< ReturnTutorialData>> returnTutorialDatas)
    {
        Tutorial nTutorialData = GameCore.Instance.PlayerDataMgr.TutorialData;
        if (CheckTutorialAble(isFixedTutorial, tutorialPos, nTutorialData) && tutorialPos != -1)
        {
            int nTutorialPos = isFixedTutorial == true ? nTutorialData.main + 1 : tutorialPos;
            SetTutorialChild(true, 0, null, /*returnTutorialDatas);//*/ returnTutorialDatas(nTutorialPos));
        }
    }

    internal void ShowObjectVertical(string _title, string _str, GameObject _object, int _size, MsgAlertBtnData[] _btns)
    {
        //GameCore.Instance.soundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, _title, _btns, _size, null, null, _object, true)));
    }

    internal void ShowCardInfoByStore(ShopPackageSData _sdata)
    {
        EventMgr.SendEvent(new GameEvent(GameEventType.InfoAlert, new InfoAlertPara(_sdata)));
    }


    internal void ShowCardInfoNotHave(int _key, int _texture = -1)
	{
		CardDataMap data;
		CardSData sdata;
		if (CardDataMap.IsItemKey(_key))
		{
			data = DataMgr.GetItemData(_key);
			sdata = new ItemSData(_key, 1);
		}
		else if (CardDataMap.IsUnitKey(_key))
		{
			sdata = new HeroSData(_key);
		}
		else
		{
			sdata = new TeamSkillSData(_key);
		}
		EventMgr.SendEvent(new GameEvent(GameEventType.InfoAlert, new InfoAlertPara(sdata, false) { texture = _texture }));
	}

	internal void ShowCardInfo(CardSData _sdata)
	{
		EventMgr.SendEvent(new GameEvent(GameEventType.InfoAlert, new InfoAlertPara(_sdata, true)));
	}

	internal void ShowReceiveItem(CardSData[] _data, Action _cbClose = null)
	{
		commonSys.ShowReceiveItem(_data, _cbClose);
	}


    internal void ShowReceiveItemPopup(string _title, CardSData[] _list, EventDelegate confirmCallback = null)
    {
        GameCore.Instance.CommonSys.tbUi.SetReturnBtnCB(null);

        if (_list == null || _list.Length == 0)
            return;

        var scrollview = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Farming/content_receives", ui_root);
        var grid = scrollview.GetComponentInChildren<UIGrid>();

        for (int i = 0; i < _list.Length; ++i)
        {
            var sdata = _list[i];
            if (!(sdata is ItemSData) || ((ItemSData)sdata).count > 0)
                CardBase.CreateCard(sdata, null, false, grid.transform, null, (_key) => GameCore.Instance.ShowCardInfo(sdata));
        }

        if (8 < _list.Length)
            SpringPanel.Begin(scrollview, new Vector3((_list.Length - 8) * 37.5f + 16, 0), 16);
        else
            SpringPanel.Begin(scrollview, new Vector3(0, 0), 16);
        GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, _title,
            new MsgAlertBtnData[] { new MsgAlertBtnData("확인", confirmCallback) }, 0, "\n\n\n\n\n가방에서 확인할 수 있습니다.", null, scrollview)));
        //grid.transform.localPosition = new Vector3(-34, -68);
    }


    internal void ShowReceiveItemPopup(string _title, string _description, CardSData[] _list)
    {
        if (_list == null || _list.Length == 0)
            return;

        var scrollview = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Farming/content_receives", ui_root);
        var grid = scrollview.GetComponentInChildren<UIGrid>();

        for (int i = 0; i < _list.Length; ++i)
        {
            var sdata = _list[i];
            if (!(sdata is ItemSData) || ((ItemSData)sdata).count > 0)
                CardBase.CreateCard(sdata, null, false, grid.transform, null, (_key) => GameCore.Instance.ShowCardInfo(sdata));
        }


        if (8 < _list.Length)
            SpringPanel.Begin(scrollview, new Vector3((_list.Length - 8) * 37.5f + 16, 0), 16);
        else
            SpringPanel.Begin(scrollview, new Vector3(0, 0), 16);

        GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, _title,
            new MsgAlertBtnData[] { new MsgAlertBtnData("확인", null) }, 0, string.Concat("\n\n\n\n\n", _description), null, scrollview)));
        //grid.transform.localPosition = new Vector3(-32, -68);
    }


    internal void ShowReceiveHeroPopup(string title, string str, int fontSize, string str2, int fontSize2, CardSData data, EventDelegate.Callback _cbCorrect)
    {
        if (data == null)
            return;
        
        var grid = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Farming/content", ui_root);

        var sdata = data;
            if ((sdata is HeroSData))
                CardBase.CreateCard(sdata, null, true, grid.transform, null, (_key) => GameCore.Instance.ShowCardInfo(sdata));
            
        GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, title,
            new MsgAlertBtnData[] { new MsgAlertBtnData("취소", null), new MsgAlertBtnData("확인", new EventDelegate(_cbCorrect)) }, 
            1, str, null, grid.gameObject, false, fontSize, str2, fontSize2)));

        grid.transform.localPosition = new Vector3(-52, -74);
        grid.transform.localScale = new Vector3(0.8f, 0.8f);
    }




    internal void ShowReduceResource(ResourceType _type)
    {
        string str = string.Empty;
        bool goShop = false;
        switch (_type)
        {
            case ResourceType.Gold:       str = "골드가";             goShop = true; break;
            case ResourceType.Cash:       str = "펄이";               goShop = true; break;
            case ResourceType.Friendship: str = "우정의 증표가";      goShop = false; break;
            case ResourceType.Ticket1:    str = "빠른 영웅 제조권이"; goShop = false; break;
            case ResourceType.Ticket2:    str = "빠른 장비 제조권이"; goShop = false; break;
            default:                      str = "필요 재화가";        goShop = false; break;
        }

        if (goShop)
        {

            GameCore.Instance.ShowAgree("실패", string.Format("{0} 부족합니다.\n상점으로 이동하시겠습니까?", str), 0, () =>
            {
                GameCore.Instance.CloseMsgWindow();
                int pos = 0;
                switch (_type)
                {
                    case ResourceType.Gold: pos = 1; break;
                    case ResourceType.Cash: pos = 0; break;
                }

                GameCore.Instance.ChangeSubSystem(SubSysType.Shop, new ShopPara() { openPos = pos });
            });
        }
        else
        {
            GameCore.Instance.ShowNotice("실패", string.Format("{0} 부족합니다.", str), 0);
        }
    }


    internal void RestartGame()
    {
        if (lobbyTutorial != null)
        {
            Destroy(lobbyTutorial.gameObject);
            lobbyTutorial = null;
        }
        StopAllCoroutines();
        commonSys.ShowLoadingPage(false);
        PlayerDataMgr.ResetPlayerData();

        ChangeSubSystem(StartSystem, null);
        CloseMsgWindow();
        CloseMsgWindow();

        commonSys.ClearNotices();
        commonSys.tbUi.SetActiveFriendUI(false);

        if (missionMgr.missionUI != null)
            missionMgr.missionUI.Show(false);

#if UNITY_ANDROID
        // AbBrix제거
        //AdBrixRmAOS.AdBrixRm.logout();
#endif
    }

    internal void CloseMsgWindow()
	{
		EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, null));
	}

	internal void CloseAlert()
	{
		EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, MsgAlertPara.CreateAlertMsg(null)));
	}

	internal void ChangeBack()
	{
		SubsysMgr.ReturnBackSubSys();
	}

	internal GameObject CreateButton(string _str, int _fontsize, string _spriteName, EventDelegate.Callback _cbClick)
	{
		var obj = ResourceMgr.GetInstanceLocalObject("Common/DefButton", ui_root);
		var lb = obj.GetComponentInChildren<UILabel>();
		lb.text = _str;
		lb.fontSize = _fontsize;
		var btn = obj.GetComponentInChildren<UIButton>();
		btn.normalSprite = _spriteName + "1";
		btn.pressedSprite = _spriteName + "2";
		btn.onClick.Add(new EventDelegate(_cbClick));

		return obj;
	}
    ///////////////////////////////////////////////////////////

    public static int GetDayNum(DateTime _date)
    {
        var span = _date - default(DateTime);
        return (int)span.TotalDays;
    }


    public void LogOut()
    {
        bLogined = false;
#if !UNITY_EDITOR
        GamePotMgr.Reset();
#endif
        // Todo Clear PlayerData

        NetMgr.DoReconnect();
        SetStageInfo(-1, -1, -1);
        if (subsysMgr.GetNowSysType() != SubSysType.Loading)
            GameCore.Instance.RestartGame();
    }

    internal void ReconnectedSession(bool _isreconnected)
    {
        if (_isreconnected)
            netMgr.Req_Current_Time();
        NetMgr.Req_Notify_Friend_Mail_Count();
    }



    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnApplicationPause(bool pause)
    {
        cachedPause = pause;
        if (bLogined)
        {
            if (netMgr != null && SubsysMgr != null && SubsysMgr.GetNowSysType() != SubSysType.Loading)
            {
                if (!pause)
                {
                    netMgr.SessionTest();
#if !UNITY_EDITOR
            gamePotMgr.CancelLocalPush();
#endif
                }
                else
                {
#if !UNITY_EDITOR

            gamePotMgr.SetLocalPush();
#endif
                }
            }
        }


        // 애드브릭스 로그인/로그아웃 (사용시간)
#if UNITY_ANDROID
        if (subsysMgr != null && subsysMgr.GetNowSysType() != SubSysType.Loading)
        {
            if (pause)
            {
                // ADBrix제거
                //AdBrixRmAOS.AdBrixRm.logout();
            }
            else
            {
                // ADBrix제거
                //AdBrixRmAOS.AdBrixRm.login(PlayerDataMgr.Name);
            }
        }
#endif
    }

    private void OnApplicationQuit()
    {
        //ResourceMgr.AbMgr.AbortAllThread();
#if !UNITY_EDITOR
        gamePotMgr.SetLocalPush();
#endif
    }
}
