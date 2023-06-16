using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using Spine;
using UnityEngine;

/// <summary>
/// 배틀 시스템 상태
/// </summary>
internal enum BattleSysState
{
    Prepare,                // 준비
    Chemistry,          // 케미 버프 설정
    Battle_Before,  // 전투 직전 ( 버프 처리 )
    Battle,         // 전투
    Forward,                // 전진
    Retreat,                // 후퇴
    Pause,                  // 일시 정지
    Result,                 // 보상
    End,						// 종료
    Story,             //스토리 진행
}

internal enum DamagePower
{
    Critical,   // 치명
    Normal,     // 일반
    Fail,			// 실수
    Absolute    //절대값
}

public enum InGameType
{
    Story,
    Daily,
    PvP,
    Raid
}

internal enum BattleEvent
{
    Move,
    MoveStraight,
    MoveReturn,
    MoveReturnStraight,
    Turn,
    TurnForward,
    TurnBack,
    HideUI,
    ShowUI,
    Attack,
    Skill_Start,
    Skill,
    TeamSkill,
    Death,
    Start_Zoom,
    Dealyed_Zoom,
    Start_Zoom_Skill,
    Dealyed_Zoom_Skill,
    EndZoom,
    Skill_Sound_Personal,
    Skill_Sound_Team,
    ChargeAttackEffect,
    MaxGuardEffect,
}
[Flags]
internal enum BattleStoryFlag
{
    None = 0,
    First = 1,
    End = 2,
}

internal class BattleParaBase : ParaBase
{
    public InGameType type;
    public int playerTeam;
    public List<StatInfos> unitStatInfosList;
    public List<StatInfos> enemyStatInfosList;
}

internal abstract class BattleSysBase : SubSysBase
{
    public const float MoveDelay = 0.3f;

    // have
    protected BattleFieldBase battleField;          // 필드와 모든 유닛 애니메이션 제어
    protected BattleUIBase ui;                      // 전투 UI 제어
    protected BattleUnitPool unitPool;              // 유닛 풀
    public BattleUnitPool UnitPool { get { return unitPool; } }
    protected BattleBuffCtrl buffCtrl;
    protected BattleAttackCtrl atkCtrl;

    protected BattlePara para;
    
    // 상태 및 상태제어 성향의 변수들
    protected InGameType gameType;                  // 게임 타입. 
    protected BattleSysState state;                 // 시스템 현재 상태(준비, 진행, 전투, 후퇴, 일시정지, 보상, 종료)

    // 데이터, 정보 성향의 변수들
    private BattleCommonDataMap consts;             // 공식에 사용된는 상수들
    protected int stageId;                          // 스테이지 ID
    protected int playerTeam;                       // 플레이어 팀 번호
    protected int targetCount;                      // 목표 전투 횟수
    protected int battleCount;                      // 현재 전투 횟수(전투 완료시 카운팅)

    private float fwdSpeed = 0.4f;                  // 전진 속도( deltaTime * fwdSpeed. Norm )
    private float fwdAcc = 0f;                      // 전진 누적 변수    


    // mission용 데이터들
    protected bool playerWin;           // 클리어
    protected float totalAttack;        // 총 준 데미지
    protected float totalDamage;        // 총 받은 데미지
    protected int useTeamSkillCount;    // 팀 스킬 사용 횟수
    protected int playerTeamCount;      // 원래의 플레이어팀의 유닛 수
    protected int playerTeamDeadCount;  // 플레이어팀의 죽은 유닛 수


    // 임시 , cache 성향의 변수들
    private List<int> fIdInMovePanel = new List<int>();         // StatusUI 위치 갱신을 위한 MovePanel에 속한 유닛의 필드아이디 배열
    private List<int> removeUnits;                              // 삭제될 유닛의 필드아이디들
    private float endWaitTime = 1f;                             // 전투 종료 후 기다리는 시간
    private float endWaitAcc = 0f;                              // 전투 종료 후 기다리는 시간을재는 변수

    private int skillActiveUnitFid = -1;                        // 스킬 준비시 선택 중인 유닛
    private int[] skillTargetList = null;                       // 스킬 준비시 선택할 수 있는 유닛 리스트


    public BattlePara Para { get { return para; } }

    public int coSpecialBuff { get; protected set; }        // 동작중인 스페셜 버프 개수

    public bool canUseSkillEnmey { get; protected set; }    // 적이 스킬을 쓸 수 있는지 여부

    public float overKillDmg { get; protected set; }

    protected List<int> killedMonsterID; // 잡은 몬스터 아이디 리스트

    protected BattleStoryUI battleStoryUI;

    protected BattleSysState stateBefore;   //스토리 모드를 들어가기 전 상태정보
    protected BattleStoryFlag storyFlag;

    protected SkillSpineMgr skillSpineMgr;
    public SkillSpineMgr SkilSpineMgr { get { return skillSpineMgr; } }

    internal BattleSysState State
    {
        get
        {
            return state;
        }
        set
        {
            if (state != value)
            {
                //Debug.Log("SetState: " + value + "\t\t" + Time.time);

                state = value;
                switch (value)
                {
                    case BattleSysState.Pause:
                        //ui.OnClickMenu();
                        break;
                    case BattleSysState.Prepare: return;
                    case BattleSysState.Forward:
                        for (int i = 0; i < 6; ++i)
                            unitPool.SetNormalAnimation(i, SpineAnimation.Run);
                        if (gameType == InGameType.PvP)
                            for (int i = 0; i < 6; ++i)
                                unitPool.SetNormalAnimation((1 << 6) + i, SpineAnimation.Run);
                        break;

                    case BattleSysState.Battle:
                        ui.ShowRoundText(battleCount);

                        for (int i = 0; i < 6; ++i)
                            unitPool.SetNormalAnimation(i, SpineAnimation.Idle);
                        if (gameType == InGameType.PvP)
                            for (int i = 0; i < 6; ++i)
                                unitPool.SetNormalAnimation((1 << 6) + i, SpineAnimation.Idle);
                        break;

                    case BattleSysState.Retreat:
                        // End after Escape
                        break;

                    case BattleSysState.Result:
                        //endWiatTime = 3f;
                        endWaitAcc = 0f;
                        break;

                    case BattleSysState.End:
                        // Exit Sys
                        break;
                }

                ChangedBattleState(value);
            }
        }
    }
    protected bool CheckFriendAble(BattlePara _para)
    {
        int friendIcon = _para.friendIcon;
        if (friendIcon <= 0)
        {
            ui.TurnOffStrikeButton();
            return false;
        }
        ui.CreateFriendSkillUI(_para);
        ui.TurnOffFriendSkillUI();
        //friendAnimation.SetActive(false);
        return true;
    }

    // UI와 BattleField를 생성 및 초기화해야 한다.
    // targetCount를 설정해야 한다.
    // UnitPool에 모든 유닛을 생성해야 한다.
    protected abstract void Init();

    // Result에 데이터를 보낸다.
    // ResultSys로 변경한다.
    protected abstract void EndBattle();

    internal BattleSysBase(SubSysType _subSys) : base(_subSys)
	{
        unitPool = new BattleUnitPool();
        removeUnits = new List<int>();
        needInitDataTypes = new InitDataType[] {
            InitDataType.Character,
            InitDataType.Team,
        };

        preloadingBundleKeys = new int[]
        {
            CommonType.DEF_KEY_BATTLE_BG,
            CommonType.DEF_KEY_BGM_INGAME,
            CommonType.DEF_KEY_BGM_STORY,
            CommonType.DEF_KEY_SFX_BATTLE,
            CommonType.DEF_KEY_SFX_SKILL_VOICE_P,
            CommonType.DEF_KEY_SFX_SKILL_VOICE_T,
            CommonType.DEF_KEY_SFX_SKILL_P,
            CommonType.DEF_KEY_SFX_SKILL_T,
        };
    }

    public override int PreLoading()
    {
        GameCore.Instance.NetMgr.Req_MyRoom_GetBuff();
        return base.PreLoading() + 1;
    }


    protected override void RegisterHandler()
    {
        base.RegisterHandler();
    }
    protected bool IsStoryComplete(BattleStoryFlag flag)
    {
        if (battleStoryUI == null)
            return true;
        if ((storyFlag & flag) == flag)
            return true;
        storyFlag |= flag;
        return false;
    }
    public void SkipFunc()
    {
        GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, "대화 스킵",
             new MsgAlertBtnData[] { new MsgAlertBtnData("취소", new EventDelegate(()=>{
                                        GameCore.Instance.CloseMsgWindow();
                                        battleStoryUI.ReStartState();
                                        ui.ReturnSpeed();
                                    }), true, null, SFX.Sfx_UI_Cancel),
                                    new MsgAlertBtnData("확인", new EventDelegate(()=>{
                                        GameCore.Instance.CloseMsgWindow();
                                        battleStoryUI.ReStartState();
                                        EndStory();
                                    })) },
             0, "대화 장면을 스킵하시겠습니까?"
            )));
    }
    protected void StartStory(bool isFirst, BattleSysState returnState)
    {
        if (GameCore.Instance.PlayerDataMgr.GetStorySData(para.stageId).clear && PlayerPrefs.GetInt(CommonType.PFS_SKIP_STORY, 1) == 0)
        {
            return;
        }

        bool checkStory = battleStoryUI.CheckStory(isFirst);
        if (checkStory == true)
        {
            GameCore.Instance.SoundMgr.SetBGMSound(BGMType.BMSND_Story, true, false);
            //GameCore.Instance.SndMgr.PlayBGM(BGM.Story);
            stateBefore = returnState;
            battleStoryUI.gameObject.SetActive(true);
            state = BattleSysState.Story;
            battleField.SetActiveTeam(false);
            ui.TurnOffButtomUI(false);
            //ui.GetComponent<UIPanel>().enabled = false;
            battleStoryUI.StartStory(isFirst);
        }
    }
    protected void EndStory()
    {
        GameCore.Instance.SoundMgr.StopAllAudioSource();
        //GameCore.Instance.SndMgr.StopBGM();
        battleStoryUI.gameObject.SetActive(false);
        battleField.SetActiveTeam(true);
        ui.TurnOffButtomUI(true);
        //ui.GetComponent<UIPanel>().enabled = true;
        state = stateBefore;
    }
    protected override void EnterSysInner(ParaBase _para)
    {
        base.EnterSysInner(_para);

        para = _para.GetPara<BattlePara>();
        gameType = para.type;
        Reset();


        //InGame Story모드에서의 이야기 진행을 위한 부분 추가
        if (BattleStoryUI.CanCreate(para))
        {
            battleStoryUI = BattleStoryUI.Create(GameCore.Instance.ui_root);
            battleStoryUI.SetStory(para, new EventDelegate(SkipFunc));
            battleStoryUI.gameObject.SetActive(false);
            storyFlag = BattleStoryFlag.None;
        }

        //GameAuto 확인
        GameCore.atuoPlay = GameCore.Instance.SubsysMgr.NowSysType == SubSysType.PvPBattle? true : GameCore.atuoPlaySave;

        // GameType 별 데이터
        Init();

        battleField.SetAdvance(0f);
        ui.InitCallback(CBClickUnitCard, CBClickTeamSkill, CBClickStrikeSkill);
        //PlayerPrefs.DeleteKey("TimeScaleValue");
        //GameCore.Instance.TimeSave = PlayerPrefs.GetInt("TimeScaleValue", 1);
        //GameCore.Instance.TimeScaleChange = GameCore.Instance.TimeSave;
        //GameCore.timeScale = Mathf.Clamp(PlayerPrefs.GetFloat("TimeScale", 1f), 1f, 4f);
        ui.SetSpeedButton(GameCore.Instance.TimeScaleChange);

        buffCtrl = new BattleBuffCtrl();
        atkCtrl = new BattleAttackCtrl();
        buffCtrl.Init(unitPool, ui, atkCtrl);
        atkCtrl.Init(unitPool, ui, buffCtrl, battleField, para);
        var tsKey = GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(para.playerTeam);
        if (tsKey != 0)
        {
            var tsData = GameCore.Instance.DataMgr.GetTeamSkillData(tsKey);
            if (tsData != null)
                ui.SetTeamSkillCover(tsData.imageID);
        }
        else
        {
            ui.TurnOffTeamButton();
        }

        skillSpineMgr = SkillSpineMgr.Create(ui.transform);
        skillSpineMgr.Init();

        SetCamViewportBySreen();
    }

    void Reset()
    {
        state = BattleSysState.Prepare;

        canUseSkillEnmey = true;
        fwdAcc = 0f;
        overKillDmg = 0f;
        coSpecialBuff = 0;

        battleCount = 0;

        if (killedMonsterID == null)
            killedMonsterID = new List<int>();
        killedMonsterID.Clear();

        // Load Dmg Const
        consts = GameCore.Instance.PlayerDataMgr.GetBattleCommonData();

        // MissionDatas
        playerWin = false;
        totalAttack = 0f;
        totalDamage = 0f;
        useTeamSkillCount = 0;
        playerTeamCount = 0;
        playerTeamDeadCount = 0;
    }

   
    protected override void ExitSysInner(ParaBase _para)
    {
        base.ExitSysInner(_para);
        if (ui != null)
        {
            //ui.ReturnSpeed();
            GameObject.Destroy(ui.gameObject);
            ui = null;
        }

        if (battleField != null)
        {
            battleField.StopAllCoroutines();
            GameObject.Destroy(battleField.gameObject);
            battleField = null;
        }

        if (battleStoryUI != null)
        {
            battleStoryUI.RemoveResource();
            GameObject.Destroy(battleStoryUI.gameObject);
            battleStoryUI = null;
        }

        buffCtrl.Release();
        atkCtrl.Release();
        buffCtrl = null;
        atkCtrl = null;

        
        GameCore.Instance.TimeScaleChange = 1;
        //GameCore.timeScale = 1f;

        Debug.Log("Exit Battle");
        state = BattleSysState.Prepare;
        unitPool.StopAllCoroutineInCombat();
        unitPool.StopAllAnimation();
        unitPool.Clear();
        removeUnits.Clear();
        coSpecialBuff = 0;
    }


    public static void SetCamViewportBySreen()
    {
        var sw = Screen.width;
        var sh = Screen.height;
        var sr = sw / (float)sh;
        var ratio = 1280 / 720f;

        var cam = GameCore.Instance.GetWorldCam();

        if (sr < ratio)
        {
            var height = sr / ratio;
            cam.rect = new Rect(0, (1 - height)/2, 1, height);
        }
        else
        {
            cam.rect = new Rect(0, 0, 1, 1);
        }
    }


    internal override void ClickBackButton()
    {
        
        ui.OnClickMenu();

        /*
        if (GameCore.Instance.TimeScaleChange != 0)
            ui.OnClickMenu();
            
        GameCore.Instance.ShowAgree("도망", "전투는 실패 처리됩니다.\n 그래도 나가시겟습니까?", 0, () =>
        {
            GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
            GameCore.Instance.CloseMsgWindow();
        });
        */
    }

    internal void OffTeamSkillCover()
    {
        ui.ShowTeamSkillActiveCover(false);
    }

    GameObject obj, sp;
    internal override void UpdateUI()
    {
        UpdateSkillCoolTime();
        UpdateSetSkillTarget();

        // UI 연출중에는 업데이트 하지 않는다.
        if (ui.IsAnmationing())
            return;

        // 특수 버프 코루틴 중에는 업데이트를 하지 않음
        if (coSpecialBuff != 0)
            return;

        // 애니메이션이 동작 중인지 검사
        if (unitPool.CheckRunning())
            return;

        // 죽은 유닛 제거
        for (int i = 0; i < removeUnits.Count; ++i)
        {
            unitPool.RemoveUnit(removeUnits[i]);
            ui.RemoveUnitStatusUI(removeUnits[i]);
        }
        removeUnits.Clear();

        battleField.ShowBlander(false);

        switch (state)
        {
            case BattleSysState.Story:
                if(battleStoryUI.CheckList()) EndStory();
                return;
            case BattleSysState.Pause: return;
            case BattleSysState.Prepare:
                if (GameCore.Instance.IsDoneResourceLoad())
                {
                    //GameCore.Instance.SoundMgr.SetBGMSound(BGMType.BMSND_Battle_Normal);
                    //GameCore.Instance.SndMgr.PlayBGM(BGM.Story_Battle);
                    State = BattleSysState.Chemistry;
                }
                //State = BattleSysState.Chemistry;
                return;

            case BattleSysState.Chemistry: // Allways Chemistry Buff

                if (IsStoryComplete(BattleStoryFlag.First) == false)
                {
                    
                    StartStory(true, BattleSysState.Prepare);
                    return;
                }
                GameCore.Instance.TimeSave = PlayerPrefs.GetInt("TimeScaleValue", 1);
                ui.SetSpeedButton(GameCore.Instance.TimeScaleChange);

                if (GameCore.Instance.SubsysMgr.GetNowSysType() != SubSysType.PvPBattle)
                    GameCore.Instance.SoundMgr.SetBGMSound(BGMType.BMSND_Battle_Normal, true, false);
                
                unitPool.AddCombatTeam(0, buffCtrl.CBSetChemiBuff);
                
                State = BattleSysState.Forward;
                break;

            case BattleSysState.Forward:
                if (fwdAcc < 1f)
                {
                    fwdAcc += Time.deltaTime * GameCore.timeScale * fwdSpeed;
                    battleField.SetAdvance(battleCount + fwdAcc);
                    ui.SetProgress((battleCount + fwdAcc) / targetCount);
                }
                else
                {
                    fwdAcc = 0f;                                            // 전진 누적 변수 초기화
                    ++battleCount;                                          // 배틀 카운트 추가
                    atkCtrl.battleCount = battleCount;
                    unitPool.AddCombatTeam(battleCount, buffCtrl.CBSetChemiBuff);    // 새로운 상대를 유닛 리스트에 추가
                    battleField.SetAdvance(battleCount);                    // 이동 애니메이션을 멈추고, 배경 및 적 위치를 정확히 맞춘다.
                    ui.SetProgress((float)battleCount / targetCount);       // 전투 진행도 UI 업데이트

                    for (int i = 0; i < fIdInMovePanel.Count; ++i)          // 캐릭터 dstPos 업데이트
                        if (unitPool.GetUnitByFieldID(fIdInMovePanel[i]) != null)
                            unitPool.GetUnitByFieldID(fIdInMovePanel[i]).UpdateWorldPos();

                    if (skillActiveUnitFid != -1)
                        SetTargetAbleList(skillActiveUnitFid);

                    State = BattleSysState.Battle_Before;                   // 상태 변환 -> 배틀직전
                }
                break;

            case BattleSysState.Battle_Before: // 각 페이지 시작직전 한번만 동작한다.
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_InGame_Round);
                ui.ShowAllStatusBar(true);
                if (battleCount == targetCount && GameCore.Instance.SubsysMgr.GetNowSysType() != SubSysType.PvPBattle)
                    GameCore.Instance.SoundMgr.SetBGMSound(BGMType.BMSND_Battle_Boss, true, false);
                //GameCore.Instance.SndMgr.PlayBGM(BGM.Battle_Boss);
                atkCtrl.SetNextRound();
                if (battleCount == 1 && GameCore.Instance.lobbyTutorial != null)
                {
                    GameCore.Instance.TurnOnTutorial(() => { State = BattleSysState.Pause; GameCore.Instance.StopTime(); }, 1, 4, 5);
                    return;
                }
                State = BattleSysState.Battle;
                break;

            case BattleSysState.Battle:
                UpdateBattle();
                break;

            case BattleSysState.Retreat:
            // End after Escape
            case BattleSysState.Result:

                ui.StopTeamSkillGaugeAnimation();

                // wait for second
                endWaitAcc += Time.deltaTime;
                if (endWaitAcc < endWaitTime)
                    break;
                
                playerWin = !unitPool.CheckTeamDead(0);
                if (IsStoryComplete(BattleStoryFlag.End) == false && playerWin == true)
                {

                    StartStory(false, BattleSysState.End);
                    return;
                }
                State = BattleSysState.End;
                break;
            // Show Result by BattleUI
            case BattleSysState.End:
                // Exit Sys
                EndBattle();
                State = BattleSysState.Pause;
                break;
        }
    }

    protected virtual void ChangedBattleState(BattleSysState _state)
    {

    }

    protected virtual bool IsDefeat()
    {
        // 플레이어의 유닛이 모두 죽었는지 검사
        return unitPool.CheckTeamDead(0);
    }

    protected virtual bool IsClear()
    {
        // 적의 현재 팀이 모두 죽었는지 검사
        return unitPool.CheckTeamDead(battleCount);
    }

    /// <summary>
    /// 전투 처리
    /// </summary>
    private void UpdateBattle()
    {
        // 게임 플로우 결정 부분 가상화 필요.
        if (IsDefeat())
        {
            Debug.Log("던전 클리어 실패");
            State = BattleSysState.Result;
            return;
        }
        else if (IsClear())
        {
            if (targetCount == battleCount)
            {
                //ui.ShowRoundText("CLEAR!!");
                Debug.Log("던전 클리어");
                State = BattleSysState.Result;

                // 승리 애니메이션
                var count = unitPool.GetCombatUnitCount();
                for (int i = 0; i < count; ++i)
                {
                    var unit = unitPool.GetCombatUnit(i);
                    unit.SpineCtrl.SetAnimation(SpineAnimation.Victory);
                }

            }
            else
            {
                //Debug.Log(battleCount + "페이즈 클리어");
                // 모든 코루틴 제거
                unitPool.StopAllCoroutineInCombat();
                State = BattleSysState.Forward;
            }
            return;
        }

        atkCtrl.UpdateBattle();
    }

    private void UpdateSkillCoolTime()
    {
        int cnt = unitPool.GetCombatUnitCount();
        for (int i = 0; i < cnt; i++)
        {
            var unit = unitPool.GetCombatUnit(i);
            if (unit == null || unit.GetCoolTimeAcc() <= 0)
                continue;
            unit.SubSkillCoolAcc(Time.deltaTime * GameCore.timeScale);

            var cardUI = ui.GetUnitCardUI(unit.FieldId);
            if (cardUI != null)
                cardUI.SetCoolTime(unit.GetCoolTimeAcc(), unit.GetCoolTime());
        }
    }
    public BattleUnitData GetUnitData(int _atkUnit)
    {
        BattleUnitData unitData = unitPool.GetUnitByFieldID(_atkUnit);
        if (unitData == null) unitData = unitPool.GetFriendUnitByFieldID(_atkUnit);
        return unitData;
    }
    /// <summary>
    ///  공격시 발생하는 애니메이션 이벤트 처리
    /// </summary>
    /// <param name="_atkUnit"></param>
    /// <param name="_targetId"></param>
    /// <param name="_evt"></param>
    /// <returns></returns>
    private float CBEvent(int _atkUnit, int _targetId, TrackEntry _te, BattleEvent _evt)
    {
        if (GameCore.Instance.bLogSpineAnmation)
        {
            if (_te != null) Debug.Log("[" + _atkUnit + "] " + _te.animation.Name + "  " + _evt);
            else             Debug.Log("[" + _atkUnit + "]   " + _evt);
        }

        BattleUnitData unit = GetUnitData(_atkUnit);
        if (unit == null)
            return -1;

        switch (_evt)
        {
            case BattleEvent.Move: MoveToFace(_atkUnit, _targetId, false, false); return 0;
            case BattleEvent.MoveStraight: MoveToFace(_atkUnit, _targetId, false, true); return 0;
            case BattleEvent.MoveReturn: MoveToUnit(_atkUnit, GetUnitData(_atkUnit).RootTf.position, false, false, ()=> { }, false); return 0;
            case BattleEvent.MoveReturnStraight: MoveToUnit(_atkUnit, GetUnitData(_atkUnit).RootTf.position, false, false, () => { }, true); return 0;
            case BattleEvent.Turn: GetUnitData(_atkUnit).ToggleTurn(); return 0;

            case BattleEvent.TurnForward: GetUnitData(_atkUnit).SetTurn(false); return 0;
            case BattleEvent.TurnBack: GetUnitData(_atkUnit).SetTurn(true); return 0;

            case BattleEvent.HideUI: 
                ui.ShowStatusUI(_atkUnit, false);
                return 0;

            case BattleEvent.ShowUI:
                ui.ShowStatusUI(_atkUnit, true);
                return 0;

            case BattleEvent.Attack:
                DoAttackApply(_atkUnit, _targetId);
                //GameCore.Instance.SndMgr.PlayCahracterVoice((CHARACTER_VOICE)unit.Data.charIdType, CHARACTER_ACTION.CA_ATTACK);
                return 0;

            case BattleEvent.Skill_Start:
                //GameCore.Instance.SndMgr.PlayCahracterVoice((CHARACTER_VOICE)unit.Data.charIdType, CHARACTER_ACTION.CA_SKILL);
                //GameCore.Instance.SndMgr.PlaySFX(SFX.Sfx_Skill);
                //battleField.ShowParticle(ParticleType.Charge, unit);
                return 0;

            case BattleEvent.Skill:
                DoSkillApply(_atkUnit, _targetId, (_atkUnit < 0 ? false : true), false, 1f);
                return 0;

            case BattleEvent.TeamSkill:
                DoSkillApply(_atkUnit, _targetId, false, true, atkCtrl.teamSkillDmgScale);
                return 0;

            case BattleEvent.Death:
                CBDeath(_atkUnit);
                battleField.ShowParticle(ParticleType.Death, unit);
                //GameCore.Instance.SndMgr.PlayCahracterVoice((CHARACTER_VOICE)unit.Data.charIdType, CHARACTER_ACTION.CA_DEATH);
                return 0;


            case BattleEvent.Start_Zoom:
                {
                    if (unit.Data.atkType == AttackType.Near || unit.Data.atkType == AttackType.Sword)
                    {
                        var tgUnit = unitPool.GetUnitByFieldID(_targetId);
                        var pos = tgUnit.Transform.position + new Vector3(0f, tgUnit.Height * 0.5f, 0f);
                        var delay = 0.0f;
                        var time = 0.2f;
                        battleField.SetZoomIn(pos, 15f, UpdateWorldPositionUI, delay, time);
                    }
                    return 0;
                }

            case BattleEvent.Dealyed_Zoom:
                {
                    if (unit.Data.atkType == AttackType.Near || unit.Data.atkType == AttackType.Sword)
                    {
                        var tgUnit = unitPool.GetUnitByFieldID(_targetId);
                        var pos = tgUnit.Transform.position + new Vector3(0f, tgUnit.Height * 0.5f, 0f);
                        var delay = 0.3f;
                        var time = 0.2f;
                        battleField.SetZoomIn(pos, 15f, UpdateWorldPositionUI, delay, time);
                    }
                    return 0;
                }

            case BattleEvent.Start_Zoom_Skill:
                {
                    if (atkCtrl.CheckMoveType(unit.SkillData))
                    {
                        EffectDataMap[] effectData = new EffectDataMap[5];
                        unit.SkillData.PeekNowEffects(ref effectData);
                        for (int i = 0; i < effectData.Length; ++i)
                        {
                            if (effectData[i] == null)
                                continue;
                            if (effectData[i].targetType == 0)
                            {
                                var tgUnit = unitPool.GetUnitByFieldID(_targetId);
                                var pos = tgUnit.Transform.position + new Vector3(0f, tgUnit.Height * 0.5f, 0f);
                                var delay = 0.0f;
                                var time = 0.2f;
                                battleField.SetZoomIn(pos, 15f, UpdateWorldPositionUI, delay, time);
                                return 0;
                            }
                        }
                    }

                    return 0;
                }

            case BattleEvent.Dealyed_Zoom_Skill:
                {
                    if (atkCtrl.CheckMoveType(unit.SkillData))
                    {
                        EffectDataMap[] effectData = new EffectDataMap[5];
                        unit.SkillData.PeekNowEffects(ref effectData);
                        for (int i = 0; i < effectData.Length; ++i)
                        {
                            if (effectData[i] == null)
                                continue;
                            if (effectData[i].targetType == 0)
                            {
                                var tgUnit = unitPool.GetUnitByFieldID(_targetId);
                                var pos = tgUnit.Transform.position + new Vector3(0f, tgUnit.Height * 0.5f, 0f);
                                var delay = 0.6f;
                                var time = 0.2f;
                                battleField.SetZoomIn(pos, 15f, UpdateWorldPositionUI, delay, time);
                                return 0;
                            }
                        }
                    }
                    return 0;
                }

            case BattleEvent.EndZoom:
                {
                    var delay = 0.5f;
                    var time = 0.2f;
                    battleField.SetZoomOut(UpdateWorldPositionUI, delay, time);
                    return 0;
                }
            case BattleEvent.Skill_Sound_Personal:
                {
                    GameCore.Instance.SoundMgr.SetCharacterSkillSound(unit, false, false);
                    return 0;
                }
            case BattleEvent.Skill_Sound_Team:
                {
                    GameCore.Instance.SoundMgr.SetCharacterSkillSound(unit, false, true);
                    return 0;
                }
            case BattleEvent.ChargeAttackEffect:
                {
                    battleField.ShowParticle(ParticleType.Charge, unit);
                    return 0;
                }
            case BattleEvent.MaxGuardEffect:
                {
                    battleField.ShowParticle(ParticleType.MaxGuard, unit);
                    return 0;
                }
            default: Debug.LogWarning("Not Find Hnadle : " + _evt); return 0;
        }
    }

    public void UpdateWorldPositionUI()
    {
        unitPool.UpdateUnitWorldPosInCombat();
        // UI.DemagePrint Update - Resolve
        // UI.Special Effect Update - Resolve
    }

    public IEnumerator WaitForUIAnimation(Action _cb)
    {
        yield return null;
        while (ui != null && ui.IsAnmationing())
            yield return null;
        if (ui != null)
            _cb();
    }

    public void ShowTeamUnit(BattleUnitData unit, bool _show, Action _action)
    {
        if (unit != null)
        {
            MoveToUnit(unit.FieldId, unit.GetWorldPosition() + new Vector3(_show ? 7 : -7, _show ? -6 : 6, 0), false, true, _action, false);
            if (!_show)
                unit.SpineCtrl.skelAnim.AnimationState.SetAnimation(0, "Jump_start", false);
            else
            {
                unit.SpineCtrl.skelAnim.AnimationState.SetAnimation(0, "Jump_end", false);
                unit.SpineCtrl.skelAnim.AnimationState.AddAnimation(0, "Idle", true, 0);
            }
        }
    }

    public void DoAirborne(BattleUnitData unit, bool airborne)
    {
        if (unit != null)
        {
            if (!airborne)
            {
                unit.SpineCtrl.StartAirborn(()=> {
                    unit.SpineCtrl.skelAnim.timeScale = 0f;
                });
            }
            else
            {
                unit.SpineCtrl.skelAnim.timeScale = 1f;
                var isDead = IsDead(unit);
                if (isDead)
                {
                    killedMonsterID.Add(unit.Data.id);
                }
                else
                {
                    unit.SpineCtrl.EndAirbron();
                    
                }
            }
        }
    }

    /// <summary>
    /// 피격 처리를 한다.
    /// </summary>
    /// <param name="_tgUnit">데미지를 받을 유닛</param>
    /// <param name="_dmg">데미지</param>
    /// <returns>사망시 true</returns>
    public bool DoDamage(BattleUnitData _atkUnit, BattleUnitData _tgUnit, float _dmg, DamageType _type, bool _gauge = true, DamagePower _atkDp = DamagePower.Normal, DamagePower _grdDp = DamagePower.Normal, bool _finish = true)
    {
        float tgHp = 0;
        bool alreadyDead = _tgUnit.NowHp == 0;

        switch (_type)
        {
            case DamageType.HPDamage:
                if (0 < BattleUnitPool.GetTeamByFieldId(_tgUnit.FieldId))
                    totalAttack += _dmg;
                else
                    totalDamage += _dmg;

                tgHp = AddUnitHP(_tgUnit.FieldId, -_dmg);
                // 수면 제거
                _tgUnit.RemoveStateBuff(EffectType.Sleep, buffCtrl.CBRemoveBuffIcon);
                //특수 방어를 위한 타겟의 grd 타입 변경
                _tgUnit.grd = _grdDp;
                // 게이지 계산
                if (_gauge /*&& 0 <= _atkUnit.FieldId*/) // 스트라이크로 개인스킬사용시 strike 증가되지 않도록
                {
                    CalcTeamSkillGauge(_atkUnit, _tgUnit, _dmg);
                    CalcStrikeSkillGauge(_atkUnit, _tgUnit, _dmg);
                }
                battleField.CamShake(_dmg);
                break;

            case DamageType.MTDamage: AddUnitMT(_tgUnit.FieldId, -_dmg); break;

            case DamageType.HPHeal: tgHp = AddUnitHP(_tgUnit.FieldId, _dmg); break;
            case DamageType.MTHeal: AddUnitMT(_tgUnit.FieldId, _dmg); break;

            case DamageType.HPPoison:
                tgHp = AddUnitHP(_tgUnit.FieldId, -_dmg);
                if (0 < BattleUnitPool.GetTeamByFieldId(_tgUnit.FieldId))
                    totalAttack += _dmg;
                else
                    totalDamage += _dmg;

                // 게이지 계산
                if (_gauge)
                {
                    CalcTeamSkillGauge(_atkUnit, _tgUnit, _dmg);
                    CalcStrikeSkillGauge(_atkUnit, _tgUnit, _dmg);
                }
                break;
            case DamageType.MTPoison: AddUnitMT(_tgUnit.FieldId, -_dmg); break;

            case DamageType.State:
                return false;
        }

        var pos = _tgUnit.GetWorldPosition();
        var team = BattleUnitPool.GetTeamByFieldId(_tgUnit.FieldId) == 0;


        // OverKill
        if (_atkUnit != null && _atkUnit != _tgUnit && (alreadyDead || tgHp < 0) && 
            (BattleUnitPool.GetTeamByFieldId(_atkUnit.FieldId) == 0 || BattleUnitPool.GetTeamByFieldId(_atkUnit.FieldId) == -1))
        {
            overKillDmg += (alreadyDead) ? _dmg : -tgHp;
            //ui.StartCoroutine(GameCore.CoWaitCall( 0.2f, ()=> 
            ui.ShowDamagePrint(pos + new Vector3(0.1f, 0.65f, 0f), (int)_dmg, team, _type, _atkDp, _grdDp, true);//));
        }
        else
        {
            ui.ShowDamagePrint(pos, (int)_dmg, team, _type, _atkDp, _grdDp, alreadyDead);
        }
        /*
        // OverKill
        if (!alreadyDead && tgHp < 0 && BattleUnitPool.GetTeamByFieldId(_atkUnit.FieldId) == 0)
        {
            overKillDmg -= tgHp;
            //ui.StartCoroutine(GameCore.CoWaitCall( 0.2f, ()=> 
            ui.ShowDamagePrint(pos + new Vector3(0.1f, 0.65f, 0f), (int)-tgHp, team, _type, _atkDp, _grdDp, true);//));
        }
        */

        if (_type == DamageType.HPDamage ||
                _type == DamageType.MTDamage ||
                _type == DamageType.HPPoison ||
                _type == DamageType.MTPoison)
        {
            if (_finish)
            {
                var isDead = IsDead(_tgUnit);
                if (isDead) killedMonsterID.Add(_tgUnit.Data.id);
                return isDead;
            }
            else
            {
                if (!_tgUnit.IsDead())
                    unitPool.SetNormalAnimation(_tgUnit.FieldId, _tgUnit.grd == DamagePower.Critical ? SpineAnimation.HitMaxGuard : SpineAnimation.Hit);
                return false;
            }
        }
        else
            return false;
    }



    private void CalcTeamSkillGauge(BattleUnitData _atkUnit, BattleUnitData _tgUnit, float _dmg)
    {
        var tsKey = GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(para.playerTeam);
        if (tsKey == 0)
            return;
        if (ui.IsTeamDeath == true)
            return;

        var teamSkillAcc = atkCtrl.teamSkillAcc;
#if UNITY_EDITOR
        if (GameCore.Instance.bTestTeamSkill)
            teamSkillAcc = 2f;
#endif
        if (_atkUnit == null) // 독데미지일경우
        {
            if (BattleUnitPool.GetTeamByFieldId(_tgUnit.FieldId) == 0) // 플레이어의 캐릭터가 데미지 입는 경우
                teamSkillAcc += _dmg * 0.001f * consts.TeamGrdGaugeConst;
            else
                teamSkillAcc += _dmg * 0.001f * consts.TeamDmgGaugeConst;
        }
        else
        {
            if (BattleUnitPool.GetTeamByFieldId(_atkUnit.FieldId) == 0) // 플레이어가 공격할 경우
            {
                if (_atkUnit.ChemiBuffCount != 0) // 캐미 버프가 있을 경우
                    teamSkillAcc += _dmg * 0.001f * consts.TeamDmgChemiConst;
                else
                    teamSkillAcc += _dmg * 0.001f * consts.TeamDmgGaugeConst;
            }
            else // 플레이어가 맞는 경우
            {
                if (_tgUnit.ChemiBuffCount != 0) // 캐미 버프가 있을 경우
                    teamSkillAcc += _dmg * 0.001f * consts.TeamGrdChemiConst;
                else
                    teamSkillAcc += _dmg * 0.001f * consts.TeamGrdGaugeConst;
            }
        }

        atkCtrl.teamSkillAcc = Mathf.Min(teamSkillAcc, 2f);
        ui.SetTeamSkillGauge(atkCtrl.teamSkillAcc);
    }

    private void CalcStrikeSkillGauge(BattleUnitData _atkUnit, BattleUnitData _tgUnit, float _dmg)
    {
        if (ui.IsStrikeDeath == true)
            return;
        var StrikeSkillAcc = atkCtrl.StrikeSkillAcc;
#if UNITY_EDITOR
        if (GameCore.Instance.bTestStrikeSkill)
            StrikeSkillAcc = 1f;
#endif
        if (_atkUnit == null) // 독데미지일경우
        {
            if (BattleUnitPool.GetTeamByFieldId(_tgUnit.FieldId) == 0) // 플레이어의 캐릭터가 데미지 입는 경우
                StrikeSkillAcc += _dmg * 0.001f * consts.StlikeGrdConst;
            else
                StrikeSkillAcc += _dmg * 0.001f * consts.StlikeDmgConst;
        }
        else
        {
            if (BattleUnitPool.GetTeamByFieldId(_atkUnit.FieldId) == 0) // 플레이어가 공격할 경우
                StrikeSkillAcc += _dmg * 0.001f * consts.StlikeDmgConst;
            else // 플레이어가 맞는 경우
                StrikeSkillAcc += _dmg * 0.001f * consts.StlikeGrdConst;
        }

        atkCtrl.StrikeSkillAcc = Mathf.Min(StrikeSkillAcc, 1f);
        ui.SetStrikeSkillGauge(atkCtrl.StrikeSkillAcc);
    }
    private bool CheckCreateSpine(BattleUnitData atkUnit, BattleUnitData tgUnit, bool _critical)
    {
        switch (atkUnit.Data.atkType)
        {
            case AttackType.SpineFixed:
            case AttackType.SpineMove:
                if (atkUnit.AttackFirst == false)
                    return false;
                atkUnit.AttackFirst = false;
                SkilSpineMgr.SetAttackSpine(atkUnit, tgUnit, atkUnit.IsPlayerTeam, TargetType.SingleEnemy, true, -1, _critical);
                break;
            default:
                return false;
        }
        return true;
    }
    /// <summary>
    /// 현재 공격력으로 대상에게 데미지를 준다.
    /// </summary>
    /// <param name="_atkUnit"></param>
    /// <param name="_targetID"></param>
    /// <returns>대상 유닛 사망시 true</returns>
    private bool DoAttackApply(int _atkUnit, int _targetID)
    {
        var atkUnit = unitPool.GetUnitByFieldID(_atkUnit);
        var tgUnit = unitPool.GetUnitByFieldID(_targetID);

        if (tgUnit == null)
        {
            Debug.LogWarning("[" + _atkUnit + "] " + _targetID + "타겟이 존재하지 않습니다.");
            return false;
        }

        // calc Demage
        DamagePower atkDp, grdDp;
        var damage = atkUnit.attackDamage;
        atkDp = atkUnit.atk;
        grdDp = atkUnit.grd;

        if (CheckCreateSpine(atkUnit, tgUnit, atkDp == DamagePower.Critical) == true) return true;
        //var damage = CalcDamage(atkUnit, tgUnit, out atkDp, out grdDp); //atkUnit.GetStat(UnitStat.Attack) / (1 + (tgUnit.GetStat(UnitStat.Armor) / 10));

        // 체력 비례 데미지
        var buff = atkUnit.GetSpecialChemiEffect(EffectType.ProportionDmg, _targetID);
        if (buff != null)
            damage = tgUnit.GetStat(UnitStat.Hp) * buff.effect.buffData.value1 * 0.01f;

        SfxHitSound(atkUnit, atkDp, false, false);
        /*
        if (atkDp == DamagePower.Critical)
            GameCore.Instance.SndMgr.PlaySFX(SFX.Sfx_Hit_Max);
        else
            GameCore.Instance.SndMgr.PlaySFX(SFX.Sfx_Hit);
            */

        bool dead = DoDamage(atkUnit, tgUnit, damage, DamageType.HPDamage, true, atkDp, grdDp);

        var atkType = GetExceptionAttackType(atkUnit, false, false);
        switch (atkType)
        {
            case AttackType.Near:
            case AttackType.Sword:
                battleField.ShowParticle(ParticleType.NearSkill, tgUnit, atkDp, grdDp);
                break;
            case AttackType.Gun:
                battleField.ShowParticle(ParticleType.GunSkill, tgUnit);
                break;
            case AttackType.Magic:
                battleField.ShowParticle(ParticleType.MagicSkill, tgUnit);
                break;
            case AttackType.SpineFixed:
            case AttackType.SpineMove:
                battleField.ShowParticle(ParticleType.NearSkill, tgUnit);
                break;
            default:
                Debug.Log(atkUnit.Data.name + " is " + atkUnit.Data.atkType);
                Debug.LogWarning("잘못된 데이터!");
                break;
        }

        // 카운터 공격일 때 버프 해제
        buffCtrl.DepplySpecialBuff(EffectType.CounterAtk, _atkUnit);

        // 반격
        buffCtrl.ApplySpecialBuff(EffectType.CounterAtk, _targetID, _atkUnit, false);
        // RSA
        buffCtrl.ApplySpecialBuff(EffectType.RandStateAtk, _atkUnit, _targetID);
        //// 협공
        //ApplySpecialBuff(EffectType.FollowAtk, atkUnit.FieldId, tgUnit.FieldId);

        return dead;
    }
    private void SfxHitSound(BattleUnitData atkUnit, DamagePower atkDp, bool _skill, bool _teamSkill)
    {
        SFX sfxType = SFX.Sfx_Attack_Normal;
        var atkType = GetExceptionAttackTypeOnlySfxSound(atkUnit, _skill, _teamSkill);
        switch (atkType)
        {
            default:
            case AttackType.Near:  sfxType = (atkDp == DamagePower.Critical) ? SFX.Sfx_Attack_Critical : SFX.Sfx_Attack_Normal; break;
            case AttackType.Sword: sfxType = (atkDp == DamagePower.Critical) ? SFX.Sfx_Sword_Critical : SFX.Sfx_Sword_Normal; break;
            case AttackType.Magic: sfxType = (atkDp == DamagePower.Critical) ? SFX.Sfx_Magic_Critical : SFX.Sfx_Magic_Normal; break;
            case AttackType.Gun:   sfxType = (atkDp == DamagePower.Critical) ? SFX.Sfx_Pistol_Critical : SFX.Sfx_Pistol_Normal; break;
            case AttackType.None:  return;
        }

        GameCore.Instance.SoundMgr.SetCommonBattleSound(sfxType);
        //GameCore.Instance.SndMgr.PlaySFX(sfxType);
    }
    public float CalcDamage(BattleUnitData atkUnit, BattleUnitData tgUnit, out DamagePower _atkDp, out DamagePower _grdDp)
    {
        var dmg = CalcAttackValue(atkUnit, out _atkDp);
        if (_atkDp == DamagePower.Absolute)
        {
            _grdDp = DamagePower.Absolute;
            return CalcDamage(dmg, 0);
        }
        var grd = CalcGuardValue(tgUnit, _atkDp, out _grdDp);
        return CalcDamage(dmg, grd);
    }

    private float CalcDamage(float _dmg, BattleUnitData tgUnit, DamagePower _atkDp, out DamagePower _grdDp, int valueType, float _value)
    {
        if (_atkDp == DamagePower.Absolute)
        {
            _grdDp = DamagePower.Absolute;
            return CalcDamage(_dmg, 0);
        }
        var grd = CalcGuardValue(tgUnit, _atkDp, out _grdDp);
        return Mathf.Max((valueType == 0)? CalcDamage(_dmg, grd) * (_value * 0.01f) : CalcDamage(_dmg, grd), 1f);
    }

    private float CalcDamage(float _dmg, float _grd)
    {
        return Mathf.Max(_dmg - _grd, 1f);
    }

    private float CalcHealValue(BattleUnitData atkUnit, BattleUnitData tgUnit, int valueType = -1, float _value = 0f)
    {
        if (valueType == 1) return _value;
        return tgUnit.Data.stats.GetStat(UnitStat.Hp) * (_value * 0.01f);
    }

    /// <summary>
    /// 공격 데미지 계산
    /// </summary>
    /// <param name="_unit">공격하는 유닛</param>
    /// <param name="valueType">데미지 증감 스킬시 사용. 0 = 배율, 1 = 고정</param>
    /// <param name="_vlaue">데미지 증감 스킬시 사용하는 값</param>
    /// <returns></returns>
    private float CalcAttackValue(BattleUnitData _unit, out DamagePower _dp)
    {
        var atk = _unit.GetStat(UnitStat.Attack);
        
        var val = atk * consts.DmgAtkCrt_C + _unit.GetStat(UnitStat.Concent) * consts.DmgCctCrt_C + GameCore.Instance.PlayerDataMgr.Level * consts.DmgLvlCrt_C;
        // 아이템미구현
        //var dmgItem = atkUnit.GetStat(UnitStat.Attack) * consts.DmgAtkCrt_I +
        //							atkUnit.GetStat(UnitStat.Concent) * consts.DmgCctCrt_I + 
        //							GameCore.Instance.PlayerDataMgr.GetUserLevel() * consts.DmgLvlCrt_I;
        val = val * consts.DmgCrt_C; // + dmgItem * consts.DmgCrt_I;

        var seed = UnityEngine.Random.Range(consts.DmgCriConstMin, consts.DmgCriConstMax) * 0.01f;
        var criRate = (_unit.GetStat(UnitStat.Concent) * consts.DmgCriCctcrt + _unit.GetStat(UnitStat.Agility) * consts.DmgCriAglcrt) * seed;
        var norRate = (90f - criRate) * seed;
        var rlvRate = 100f - norRate - criRate;

        var rand = UnityEngine.Random.value * 100;

        // 마이룸 크리티컬 확률 증가
        if (_unit.myroomEffect.ContainsKey(MyRoomEffectType.MoreCritical))
            criRate *= _unit.myroomEffect[MyRoomEffectType.MoreCritical];

        // 아이템 크리티컬 확률 증가
        criRate += _unit.addedCritical;

        if (rand < criRate)
        {
            val *= consts.DmgCriCriScale; _dp = DamagePower.Critical;
        }// 크리티컬
        else if (rand < 1 - rlvRate)
        {
            val *= consts.DmgCriNorScale; _dp = DamagePower.Normal;
        }// 일반
        else
        {
            val *= consts.DmgCriFilScale; _dp = DamagePower.Fail;
        }// 경감

        // 데미지 증감 스킬 적용
        //if (valueType == 0) val *= (_value * 0.01f);
        return val;
    }

    private float CalcGuardValue(BattleUnitData _unit, DamagePower _atkDp, out DamagePower _dp)
    {
        var val = _unit.GetStat(UnitStat.Armor) * consts.GrdAmrCrt_C + _unit.GetStat(UnitStat.Agility) * consts.GrdAglCrt_C + GameCore.Instance.PlayerDataMgr.Level * consts.GrdLvlCrt_C;
        // 아이템미구현
        //var dmgItem = atkUnit.GetStat(UnitStat.Armor) * consts.GrdAmrCrt_I + atkUnit.GetStat(UnitStat.Concent) * consts.GrdCctCrt_I + GameCore.Instance.PlayerDataMgr.GetUserLevel() * consts.GrdLvlCrt_I;
        val = val * consts.GrdCrt_C; // + dmgItem * consts.GrdCrt_I;

        var seed = UnityEngine.Random.Range(consts.GrdCriConstMin, consts.GrdCriConstMax) * 0.01f;
        var criRate = _unit.GetStat(UnitStat.Concent) * consts.GrdCriCctcrt + _unit.GetStat(UnitStat.Agility) * consts.GrdCriAglcrt * seed;
        var norRate = (90f - criRate) * seed;
        var rlvRate = 100f - norRate - criRate;


        var rand = UnityEngine.Random.value * 100f;
        if (_atkDp != DamagePower.Critical && rand < criRate)   { val *= consts.GrdCriCriScale; _dp = DamagePower.Critical; }// 크리티컬
        else if (rand < 1 - rlvRate)                            { val *= consts.GrdCriNorScale; _dp = DamagePower.Normal; }// 일반
        else                                                    { val *= consts.GrdCriFilScale; _dp = DamagePower.Fail; }// 경감

        return val;
    }
    /// <summary>
    /// 스킬공격중 히트 이벤트 발생시 동작하는 스킬 적용 루틴
    /// </summary>
    /// <param name="_atkUnit"></param>
    /// <param name="_targetID"></param>
    /// <returns></returns>
    private bool DoSkillApply(int _atkUnit, int _targetID, bool _gauge, bool _isTeamSkill, float _dmgScale = 1f)
    {
        var atkUnit = GetUnitData(_atkUnit);
         var tgUnit = unitPool.GetUnitByFieldID(_targetID);

        EffectDataMap[] bes = new EffectDataMap[5];
        var cnt = (!_isTeamSkill) ? atkUnit.SkillData.GetNowEffects(ref bes, true) : atkUnit.TeamSkillData.GetNowEffects(ref bes, true);
        //if (!_gauge)
        //{
        //    atkUnit.TeamSkillData. == 
        //}
        if(cnt == 999)
        {
            unitPool.SetGetBackAnimation(atkUnit.FieldId);
            return true;
        }
        List<int> applyUnits = new List<int>();
        DamagePower atkDp, grdDp;
        for (int i = 0; i < cnt; ++i)
        {
            var effect = bes[i];

            //for(int j = 0; j < applyUnits.Count; j ++)
            //{
            //    var tg = unitPool.GetUnitByFieldID(applyUnits[j]);
            //    tg.SpineCtrl.SetDefSortingOrder();
            //}
            // 적용 타겟 결정
            applyUnits.Clear();
            GetTarget(effect.targetType, _atkUnit, _targetID, ref applyUnits);
            //for (int j = 0; j < applyUnits.Count; j++)
            //{
            //    var tg = GetUnitData(applyUnits[j]);
            //    tg.SpineCtrl.SetFrontSortingOrder();
            //}
            int teamNumber;
            float damage = 0f;
            switch (effect.type)
            {
                case EffectType.AirBorn:
                    bool checkAirborne = tgUnit.Airborne;
                    DoAirborne(tgUnit, checkAirborne);
                    break;
                case EffectType.spineAnimationEnemy:
                    int teamNumberEnemy = tgUnit.TeamNumber;
                    for (int j = 0; j < 6; j++)
                    {
                        var unit = unitPool.GetUnitByTeam(teamNumberEnemy, j);
                        if (unit == null)
                            continue;
                        skillSpineMgr.SetSkillSpine(atkUnit, unit, atkUnit.IsPlayerTeam, effect.targetType, _isTeamSkill, j);
                    }
                    
                    break;

                case EffectType.spineAnimationTeam:
                    for(int j = 0; j < 6; j ++)
                    {
                        teamNumber = atkUnit.TeamNumber == -1 ? 0: atkUnit.TeamNumber;
                        var unit = unitPool.GetUnitByTeam(teamNumber, j);
                        if (unit == null)
                            continue;
                        skillSpineMgr.SetSkillSpine(atkUnit, unit, atkUnit.IsPlayerTeam, effect.targetType, _isTeamSkill, j);
                    }
                   
                    break;

                case EffectType.spineAnimation:
                case EffectType.spineAnimationWide:
                    skillSpineMgr.SetSkillSpine(atkUnit, tgUnit, atkUnit.IsPlayerTeam, effect.targetType, _isTeamSkill, -1);
                    
                    //atkUnit.SkillData.Data
                    break;

                case EffectType.HPAttack:
                    if ((effect.buffData.type == 1))
                    {
                        damage =  effect.buffData.value1;
                        atkDp = DamagePower.Absolute;
                    }
                    else
                    {
                        damage = CalcAttackValue(atkUnit, out atkDp) * _dmgScale;
                    }

                    //Debug.Log("damage : " + damage);
                    for (int j = 0; j < applyUnits.Count; ++j)
                    {
                        var tg = unitPool.GetUnitByFieldID(applyUnits[j]);
                        var dmg = CalcDamage(damage, tg, atkDp, out grdDp, effect.buffData.type, effect.buffData.value1);

                        // 체력 비례 데미지
                        var buff = atkUnit.GetSpecialChemiEffect(EffectType.ProportionDmg, tg.FieldId);
                        if (buff != null)
                            dmg = tgUnit.GetStat(UnitStat.Hp) * buff.effect.buffData.value1;

                        else
                        {
                            var atkType = GetExceptionAttackType(atkUnit, true, _isTeamSkill);
                            switch (atkType)
                            {
                                case AttackType.Near: battleField.ShowParticle(ParticleType.NearSkill, tg); break;
                                case AttackType.Sword: battleField.ShowParticle(ParticleType.NearSkill, tg); break;
                                case AttackType.Gun: battleField.ShowParticle(ParticleType.GunSkill, tg); break;
                                case AttackType.Magic: battleField.ShowParticle(ParticleType.MagicSkill, tg); break;
                                case AttackType.SpineFixed: battleField.ShowParticle(ParticleType.NearSkill, tg); break;
                                case AttackType.SpineMove: battleField.ShowParticle(ParticleType.GunSkill, tg); break;
                                default: Debug.LogWarning("잘못된 데이터!"); break;
                            }
                        }

                        if (j == 0) // 소리 중첩시 크기가 너무 커짐
                            SfxHitSound(atkUnit, atkDp, true, _isTeamSkill);

                        //Debug.Log("[" + atkUnit + "] Skill Finish ? " + ((!_isTeamSkill) ? atkUnit.SkillData.finishNow : atkUnit.TeamSkillData.finishNow));
                        DoDamage(atkUnit, tg, dmg, DamageType.HPDamage, _gauge, atkDp, grdDp, (!_isTeamSkill) ? atkUnit.SkillData.finishNow : atkUnit.TeamSkillData.finishNow);
                        buffCtrl.ApplySpecialBuff(EffectType.CounterAtk, _targetID, _atkUnit, false);
                    }
                    break;

                case EffectType.MTAttack:
                    damage = effect.buffData.value1;
                    for (int j = 0; j < applyUnits.Count; ++j)
                    {
                        var tg = unitPool.GetUnitByFieldID(applyUnits[j]);
                        var dmg = damage / (1 + (tg.GetStat(UnitStat.Armor) / 10));

                        DoDamage(atkUnit, tg, dmg, DamageType.MTDamage, _gauge);
                    }
                    break;

                case EffectType.HPHeal:
                    for (int j = 0; j < applyUnits.Count; ++j)
                    {
                        var tg = unitPool.GetUnitByFieldID(applyUnits[j]);
                        var dmg = CalcHealValue(atkUnit, tg, effect.buffData.type, effect.buffData.value1) * _dmgScale;
                        DoDamage(atkUnit, tg, dmg, DamageType.HPHeal, _gauge);
                        battleField.ShowParticle(ParticleType.Heal, tg);
                    }
                    //for (int j = 0; j < applyUnits.Count; ++j)
                    //{
                    //    var tg = unitPool.GetUnitByFieldID(applyUnits[j]);
                    //    DoDamage(atkUnit, tg, damage, DamageType.HPHeal, _gauge);
                    //    battleField.ShowParticle(ParticleType.Heal, tg);
                    //}
                    break;

                case EffectType.MTHeal:
                    damage = effect.buffData.value1;
                    for (int j = 0; j < applyUnits.Count; ++j)
                    {
                        var tg = unitPool.GetUnitByFieldID(applyUnits[j]);
                        DoDamage(atkUnit, tg, damage, DamageType.MTHeal, _gauge);
                        battleField.ShowParticle(ParticleType.Heal, tg);
                    }
                    break;

                case EffectType.Buff:
                    for (int j = 0; j < applyUnits.Count; ++j)
                    {
                        BattleEffect be = new BattleEffect(effect);
                        var tg = unitPool.GetUnitByFieldID(applyUnits[j]);
                        if (tg == null) tg = unitPool.GetFriendUnitByFieldID(applyUnits[j]);
                        buffCtrl.AddBuff(applyUnits[j], -1, be);
                        ui.ShowEffectAlert(tg.GetWorldPosition(), effect.buffData.tgStat, effect.buffData.value1, effect.count);
                        if (effect.buffData.value1 < 0)
                            battleField.ShowParticle(effect.buffData.tgStat == UnitStat.Attack ? ParticleType.DamageDown : ParticleType.Debuff, tg);
                        else
                            battleField.ShowParticle(effect.buffData.tgStat == UnitStat.Concent ? ParticleType.Concentration : ParticleType.Buff, tg);
                    }
                    break;
                case EffectType.HPPoison:
                    
                case EffectType.MTPoison:
                case EffectType.paralyze:
                case EffectType.Stun:
                case EffectType.Sleep:
                    for (int j = 0; j < applyUnits.Count; ++j)
                    {
                        BattleEffect be = new BattleEffect(effect);
                        var tg = unitPool.GetUnitByFieldID(applyUnits[j]);
                        buffCtrl.AddBuff(applyUnits[j], -1, be);
                        ui.ShowEffectAlert(tg.GetWorldPosition(), effect.buffData.tgStat, -effect.buffData.value1, effect.count);
                        battleField.ShowParticle(GetParticleType(effect.type), tg);
                    }
                    break;
                //for (int j = 0; j < applyUnits.Count; ++j)
                //{
                //    BattleEffect be = new BattleEffect(effect);
                //    var tg = unitPool.GetUnitByFieldID(applyUnits[j]);
                //    buffCtrl.AddBuff(applyUnits[j], -1, be);
                //    ui.ShowEffectAlert(tg.GetWorldPosition(), effect.type, effect.count);
                //    battleField.ShowParticle(ParticleType.Debuff, tg);
                //}
                //break;
                case EffectType.RandomStateBuff:
                    for (int j = 0; j < applyUnits.Count; ++j)
                    {
                        int[] effectArray = { 41,42,43, 44};
                        int randNum = UnityEngine.Random.Range(0, effectArray.Length);
                        EffectDataMap effectData = GameCore.Instance.DataMgr.GetEffectData(effectArray[randNum]);
                        BattleEffect be = new BattleEffect(effectData);
                        var tg = unitPool.GetUnitByFieldID(applyUnits[j]);
                        buffCtrl.AddBuff(applyUnits[j], -1, be);
                        ui.ShowEffectAlert(tg.GetWorldPosition(), effectData.buffData.tgStat, -effectData.buffData.value1, effectData.count);
                        battleField.ShowParticle(GetParticleType(effectData.type), tg);
                    }
                    
                    break;
                case EffectType.Purification:
                    teamNumber = atkUnit.TeamNumber == -1 ? 0 : atkUnit.TeamNumber;
                    for (int j = 0; j < 6; j++)
                    {
                        var unit = unitPool.GetUnitByTeam(teamNumber, j);
                        if (unit == null)
                            continue;
                        battleField.ShowParticle(ParticleType.Purification, unit);
                        unit.Purification((int)effect.buffData.value1, buffCtrl.CBRemoveBuffIcon);
                    }
                    break;
            }

        }
        //if (tgUnit != null)
            //tgUnit.SpineCtrl.SetDefSortingOrder();
        return true;
    }
    private ParticleType GetParticleType(EffectType _effectType)
    {
       switch(_effectType)
        {
            case EffectType.HPHeal:return ParticleType.Heal;
            case EffectType.SetGuard: return ParticleType.MaxGuard;
            case EffectType.Purification: return ParticleType.Purification;
            case EffectType.Sleep: return ParticleType.Sleep;
            case EffectType.paralyze: return ParticleType.Paralysis;
            case EffectType.Stun: return ParticleType.Stun;
            case EffectType.HPPoison: return ParticleType.Poison;
            default: return ParticleType.Debuff;
        }
    }
    public void GetTargetForColor(TargetType _tgType, int _atvUnit, int _tgUnit, ref List<int> _tgUnitsColor)
    {
        bool singleEnemy = false;
        bool singleTeam = false;
        var atkUnit = GetUnitData(_atvUnit);
        var tgUnit = unitPool.GetUnitByFieldID(_tgUnit);

        var team = BattleUnitPool.GetTeamByFieldId(_atvUnit);
        team = (team == -1) ? 0 : team;
        if ((_tgType & TargetType.SingleEnemy) == TargetType.SingleEnemy)
        {
            if (tgUnit == null)
                return;
            singleEnemy = true;
            _tgUnitsColor.Add(tgUnit.FieldId);
        }
        if ((_tgType & TargetType.AllEnemy) == TargetType.AllEnemy || (_tgType & TargetType.AllEnemyEach) == TargetType.AllEnemyEach)
        {
            if (tgUnit == null)
                return;
            if (singleEnemy == true)
                _tgUnitsColor.Remove(tgUnit.FieldId);
            for (int i = 0; i < unitPool.GetCombatUnitCount(); i++)
            {
                var unit = unitPool.GetCombatUnit(i);
                if (BattleUnitPool.GetTeamByFieldId(unit.FieldId) != team) _tgUnitsColor.Add(unit.FieldId);
            }
        }
        if ((_tgType & TargetType.Self) == TargetType.Self)
        {
            singleTeam = true;
            if (atkUnit.IsDead() == false)
                _tgUnitsColor.Add(atkUnit.FieldId);
        }
        if ((_tgType & TargetType.TeamAll) == TargetType.TeamAll || (_tgType & TargetType.TeamAllCenter) == TargetType.TeamAllCenter)
        {
            if (singleTeam == true)
                _tgUnitsColor.Remove(atkUnit.FieldId);
            for (int i = 0; i < unitPool.GetCombatUnitCount(); i++)
            {
                var unit = unitPool.GetCombatUnit(i);
                if (BattleUnitPool.GetTeamByFieldId(unit.FieldId) == team)//&&
                                                                          //unit.IsDead() == false)
                    _tgUnitsColor.Add(unit.FieldId);
            }
        }
        if ((_tgType & TargetType.All) == TargetType.All)
        {
            _tgUnitsColor.Clear();
            for (int i = 0; i < unitPool.GetCombatUnitCount(); i++)
            {
                var unit = unitPool.GetCombatUnit(i);
                //if (unit.IsDead() == false)
                _tgUnitsColor.Add(unit.FieldId);
            }
        }
    }
    /// <summary>
    /// 스킬 사용시 대상 타겟를 리스트에 담아 반환한다.
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="_tgUnits"></param>
    public void GetTarget(TargetType _tgType, int _atvUnit, int _tgUnit, ref List<int> _tgUnits)
    {
        var atkUnit = GetUnitData(_atvUnit);
        var tgUnit = unitPool.GetUnitByFieldID(_tgUnit);

        var team = BattleUnitPool.GetTeamByFieldId(_atvUnit);
        team = (team == -1) ? 0 : team;
        switch (_tgType)
        {
            case TargetType.SingleEnemy: // 적 단일
                if (tgUnit == null)
                    return;
                _tgUnits.Add(tgUnit.FieldId);
                break;

            case TargetType.AllEnemyEach:
            case TargetType.AllEnemy: // 적 광역
                if (tgUnit == null)
                    return;
                for (int i = 0; i < unitPool.GetCombatUnitCount(); i++)
                {
                    var unit = unitPool.GetCombatUnit(i);
                    if (BattleUnitPool.GetTeamByFieldId(unit.FieldId) != team)//&&
                                                                              //unit.IsDead() == false)
                        _tgUnits.Add(unit.FieldId);
                }
                break;
            case TargetType.Self: // 캐릭터 자신
                if (atkUnit.IsDead() == false)
                    _tgUnits.Add(atkUnit.FieldId);
                break;

            case TargetType.TeamAllCenter:
            case TargetType.TeamAll: // 캐릭터의 팀전체
                for (int i = 0; i < unitPool.GetCombatUnitCount(); i++)
                {
                    var unit = unitPool.GetCombatUnit(i);
                    if (BattleUnitPool.GetTeamByFieldId(unit.FieldId) == team)//&&
                                                                              //unit.IsDead() == false)
                        _tgUnits.Add(unit.FieldId);
                }
                break;

            case TargetType.All: // 전체
                for (int i = 0; i < unitPool.GetCombatUnitCount(); i++)
                {
                    var unit = unitPool.GetCombatUnit(i);
                    //if (unit.IsDead() == false)
                    _tgUnits.Add(unit.FieldId);
                }
                break;
        }
    }


    /// <summary>
    /// 유닛이 죽었는지 검사하고 사망 또는 히트 시 애니메이션 처리를 한다.
    /// </summary>
    /// <param name="_unit">확인할 유닛</param>
    /// <returns>죽었다면 true</returns>
    internal bool IsDead(BattleUnitData _unit)
    {
        if (!_unit.IsDead())
        {
            unitPool.SetNormalAnimation(_unit.FieldId, _unit.grd == DamagePower.Critical ? SpineAnimation.HitMaxGuard : SpineAnimation.Hit);
            return false;
        }
        else
        {
            if (skillActiveUnitFid != -1)
            {
                // 타게팅중인 유닛이 죽을경우
                if (skillActiveUnitFid == _unit.FieldId)
                    ClearTargetData();
                else
                {
                    //Update Targeting
                    SetTargetAbleList(skillActiveUnitFid);
                }
            }

            unitPool.SetNormalAnimation(_unit.FieldId, SpineAnimation.Death);
            //unitPool.RemoveCombatUnit(_unit.FieldId);
            buffCtrl.RemoveChemiBuffs(_unit.FieldId);

            //ui.RemoveUnitStatusUI(_unit.FieldId);
            fIdInMovePanel.Remove(_unit.FieldId);
            atkCtrl.attackQueue.GetList.Remove(_unit);
            return true;
        }
    }

    /// <summary>
    /// 해당 유닛의 HP에 값을 더하고, 관련 UI를 수정 한다.
    /// </summary>
    /// <param name="_tgFieldId">타겟 유닛의 필드 아이디</param>
    /// <param name="_value">HP에 더할 값</param>
    /// <returns> 남은 유닛 HP (초과하면 음의 값으로 반환)</returns>
    internal virtual float AddUnitHP(int _tgFieldId, float _value)
    {
        var tgUnit = unitPool.GetUnitByFieldID(_tgFieldId);

        var result = tgUnit.NowHp + _value;
        tgUnit.AddHp(_value);

        var hp = tgUnit.NowHp;

        var stausUI = ui.GetUnitStatusUI(_tgFieldId);
        if (stausUI != null) stausUI.SetHp(hp);

        var cardUI = ui.GetUnitCardUI(_tgFieldId);
        if (cardUI != null) cardUI.SetHp(hp);

        return result;
    }

    /// <summary>
    /// 해당 유닛의 멘탈에 값을 더하고, 관련 UI를 수정 한다.
    /// </summary>
    /// <param name="_tgFieldId">타겟 유닛의 필드 아이디</param>
    /// <param name="_value">HP에 더할 값</param>
    /// <returns> hp > 0 (* 죽으면 false)</returns>
    internal bool AddUnitMT(int _tgFieldId, float _value)
    {
        var tgUnit = unitPool.GetUnitByFieldID(_tgFieldId);
        tgUnit.AddHp(_value);

        var mental = tgUnit.NowMental;
        var stausUI = ui.GetUnitStatusUI(_tgFieldId);
        if (stausUI != null)
            stausUI.SetMental(mental);
        //if (BattleUnitPool.GetTeamByFieldId(_tgFieldId) == 0)
        //	ui.GetUnitCardUI(_tgFieldId).SetHp(hp);

        return mental > 0;
    }


    private float MoveToUnit(int _fieldId, Vector3 _tgPos, bool _turn, bool _skillMove, Action _action, bool isStraight)
    {
        var unit = GetUnitData(_fieldId);
        atkCtrl.SetUnitsSortingOrder(_fieldId, _turn, unit);
        //unit.SpineCtrl.StartCoroutine(CoMoveUnit(unit, _tgPos, _turn, _skillMove, _action));
        unit.SpineCtrl.StartCoroutine((!isStraight) ? CoMoveUnit(unit, _tgPos, _turn, _skillMove, _action) : CoMoveUnitStraight(unit, _tgPos, _turn, _skillMove));
        return Vector3.Distance(unit.GetWorldPosition(), _tgPos);
    }

    private float MoveToFace(int _fieldId, int _targetId, bool _turn , bool isStraight)
    {
        var unit = GetUnitData(_fieldId);
        var tgUnit = unitPool.GetUnitByFieldID(_targetId);
        if (tgUnit == null)
        {
            Debug.LogWarning("[" + _fieldId + "] " + _targetId + "타겟이 존재하지 않습니다.");
            return 0;
        }
        atkCtrl.SetUnitsSortingOrder(_targetId, !_turn, unit);
        Vector3 tgPos = SetAtkPosition(unit, tgUnit) + Vector3.back * 0.1f;
        //Vector3 tgPos = tgUnit.dstPos;// + tgUnit.TeamRootTf.position;
        //tgPos += new Vector3((unit.SpineCtrl.GetBoundWidth() + tgUnit.SpineCtrl.GetBoundWidth()) * 0.5f, 0f, 0f) * ((tgUnit.IsLookLeft()) ? -1f : 1f);

        unit.SpineCtrl.StartCoroutine((!isStraight)?CoMoveUnit(unit, tgPos, _turn, false,()=> { }) : CoMoveUnitStraight(unit, tgPos, _turn, false));

        return Vector3.Distance(unit.GetLocalPosition(), tgPos);
    }
    private Vector3 GetBackAndForthPos(BattleUnitData unit, BattleUnitData tgUnit)
    {
        Vector3 backAndForthPos;
        switch (unit.SpineCtrl.IsTeamSkill ? unit.TeamSkillData.Data.type : unit.SkillData.Data.type)
        {
            case PositionType.TeamFront:
                backAndForthPos = new Vector3((unit.SpineCtrl.GetBoundWidth() + tgUnit.SpineCtrl.GetBoundWidth()) * 0.25f, 0f, 0f) * ((tgUnit.IsLookLeft()) ? 1f : -1f);
                break;
            case PositionType.TeamBack:
                backAndForthPos = new Vector3((unit.SpineCtrl.GetBoundWidth() + tgUnit.SpineCtrl.GetBoundWidth()) * 0.25f, 0f, 0f) * ((tgUnit.IsLookLeft()) ? -1f : 1f);
                break;
            default:
                backAndForthPos = new Vector3((unit.SpineCtrl.GetBoundWidth() + tgUnit.SpineCtrl.GetBoundWidth()) * 0.5f, 0f, 0f) * ((tgUnit.IsLookLeft()) ? -1f : 1f);
                break;
        }
        return backAndForthPos;
    }

    Vector3 atkPosition = Vector3.zero;
    private Vector3 SetAtkPosition(BattleUnitData unit, BattleUnitData tgUnit)
    {
        BattleUnitData partner;
        switch (unit.SpineCtrl.IsTeamSkill ? unit.TeamSkillData.Data.type : unit.SkillData.Data.type)
        {
            case PositionType.EnemyFront:
                atkPosition = tgUnit.TeamRootTf.GetChild(1).position;
                break;
            case PositionType.EnemyCenter:
                atkPosition = tgUnit.TeamRootTf.GetChild(4).position;
                break;
            case PositionType.TeamFront:
            case PositionType.TeamBack:
                partner = unitPool.GetPartner(unit);
                if (partner == null)
                {
                    atkPosition = tgUnit.dstPos;
                    Debug.LogWarning("You Don't have TeamSkill Partner");
                }
                else
                    atkPosition = partner.dstPos;
                break;
            default:
                atkPosition = tgUnit.dstPos;
                break;
        }
        atkPosition += GetBackAndForthPos(unit, tgUnit);
        return atkPosition;
    }

    /// <summary>
    ///  유닛을 현재 위치에서 _tgPos로 이동한다.
    ///  @-> 시간을 0.9배만 사용한다.(0.1가량의 애니메이션은 도작지점에서 제자리 동작을한다.
    /// </summary>
    /// <param name="_unit"> 이동할 유닛</param>
    /// <param name="_tgPos">도착 좌표(NGUI좌표계)</param>
    /// <param name="_time">소요 시간</param>
    /// <param name="_turn">FlipX 여부</param>
    /// <returns></returns>
        IEnumerator CoMoveUnit(BattleUnitData _unit, Vector3 _tgPos, bool _turn, bool _skillMove, Action _action)
    {
        _unit.dstPos = _tgPos;
        var time = MoveDelay;
        float acc = 0f;
        //if (_turn) _unit.ToggleTurn();
        Vector3 startPos = _unit.GetWorldPosition();
        while (acc < time)
        {
            acc = Mathf.Min(acc + ((_skillMove) ? Time.deltaTime : Time.deltaTime * GameCore.timeScale), time);
            var value = acc / time;

            // 포물선Y 계산
            var v = value * 2;

            if (v < 1f) v = v - 1;
            else        v = 1 - v;

            var posY = v * v * v + 1;

            _unit.SetWorldPosition(Vector3.Lerp(startPos, _tgPos/* - _unit.Transform.parent.localPosition*/, value), posY);// * 50);

            yield return null;
        }
        if (_action != null) _action();
        //	if (_turn) _unit.ToggleTurn();
        _unit.SetWorldPosition(_tgPos/* - _unit.Transform.parent.localPosition*/, 0f);
        _unit.SetTurn(false);
    }

    IEnumerator CoMoveUnitStraight(BattleUnitData _unit, Vector3 _tgPos, bool _turn, bool _skillMove)
    {
        _unit.dstPos = _tgPos;
        var skillDataMap = _unit.SpineCtrl.IsTeamSkill == true ? _unit.TeamSkillData.Data : _unit.SkillData.Data;
        var time = /*MoveDelay * */skillDataMap.frameValue - 0.1f;
        float acc = 0f;
        _turn = IsLeftPos(_unit.GetWorldPosition(), _tgPos) ^ _unit.TeamNumber > 0;
        _unit.SetTurn(_turn);
        //if (_turn) _unit.ToggleTurn();
        Vector3 startPos = _unit.GetWorldPosition();
        while (acc < time)
        {
            acc = Mathf.Min(acc + ((_skillMove) ? Time.deltaTime : Time.deltaTime * GameCore.timeScale), time);
            var value = acc / time;
            _unit.SetWorldPosition(Vector3.Lerp(startPos, _tgPos/* - _unit.Transform.parent.localPosition*/, value), 0f);// * 50);
            yield return null;
        }

        //	if (_turn) _unit.ToggleTurn();
        _unit.SetWorldPosition(_tgPos/* - _unit.Transform.parent.localPosition*/, 0f);
        //_unit.SetTurn(false);
    }

    private bool IsLeftPos(Vector3 _originPos, Vector3 _tgPos)
    {
        return _tgPos.x < _originPos.x;
    }



    /// <summary>
    /// 캐릭터 사망시 캐릭터를 바로 지운다.
    /// </summary>
    /// <param name="_fieldId"></param>
    /// <returns></returns>
    private bool CBDeath(int _fieldId)
    {
        var tsKey = GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(para.playerTeam);
        if (tsKey != 0)
        {
            var tsData = GameCore.Instance.DataMgr.GetTeamSkillData(tsKey);
            if (tsData != null)
            {
                int[] teamCharacters = tsData.needChar;
                var atkUnit = unitPool.GetUnitByFieldID(_fieldId);
                int deathCharacterID = atkUnit.Data.charIdType;
                for (int i = 0; i < teamCharacters.Length; i++)
                {
                    if (teamCharacters[i] == deathCharacterID && atkUnit.TeamNumber == 0)
                    {
                        ui.TurnOffTeamButton();
                        break;
                    }
                }
            }
        }

        //RemoveBuffByDeathUnit(_fieldId);
        //var unit = unitPool.GetUnitByFieldID(_fieldId);
        removeUnits.Add(_fieldId);
        //ui.RemoveUnitStatusUI(_fieldId);
        //fIdInMovePanel.Remove(_fieldId);

        if (BattleUnitPool.GetTeamByFieldId(_fieldId) == 0)
            playerTeamDeadCount++;

        return true;
    }

    protected Transform InstantiateFriendTeam(int _team, Vector3 _offset = default(Vector3), bool _raidBoss = false)
    {
        Transform positionRoot = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/LeftTeamPos", battleField.transform).transform;
        UnityCommonFunc.ResetTransform(positionRoot);
        if (_team <= 0) positionRoot.localPosition = new Vector3((_team * BattleFieldBase.EnemyDistance), 0f, 0f) + _offset;
        int cardIdx = 0;
        for (int i = 0; i < positionRoot.childCount; ++i)
        {
            var unit = unitPool.GetFriendUnitByTeam(_team, i);
            if (unit != null)
            {
                var statusUI = ui.InstantiateStatusUI(_team != 0, unit);
                unit.Instantiate(_team != -1, statusUI, positionRoot.GetChild(i), CBEvent);

                if (_team == 0)
                {
                    ui.SetUnitCardUI(cardIdx++, unit);
                }
                fIdInMovePanel.Add(unit.FieldId);
            }

        }
        return positionRoot;
    }
    /// <summary>
    /// 팀단위로 캐릭터를 생성한다.
    /// 팀의 ID로 생성 위치가 결정된다.( 0,1,2,.... * battleField.EnemyDistance )
    /// </summary>
    /// <param name="_team">생성할 팀 ID</param>
    /// <param name="_offset">이격 미세 조정</param>
    /// <returns>팀 루트 트랜스폼 반환</returns>
    protected Transform InstantiateTeam(int _team, Vector3 _offset = default(Vector3), bool _raidBoss = false)
    {
        Transform positionRoot;
        if (_team <= 0)     positionRoot = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/LeftTeamPos", battleField.transform).transform;
        else if (_raidBoss) positionRoot = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/RaidBossPos", battleField.transform).transform;
        else                positionRoot = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/RightTeamPos", battleField.transform).transform;

        UnityCommonFunc.ResetTransform(positionRoot);
        if (_team <= 0) positionRoot.localPosition = new Vector3((_team * BattleFieldBase.EnemyDistance), 0f, 0f) + _offset;
        else            positionRoot.localPosition = new Vector3((_team * BattleFieldBase.EnemyDistance), 0f, 0f) + _offset;


        int cardIdx = 0;
        for (int i = 0; i < positionRoot.childCount; ++i)
        {
            var unit = unitPool.GetUnitByTeam(_team, i);
            if (unit != null)
            {
                var statusUI = ui.InstantiateStatusUI(_team != 0, unit);
                unit.Instantiate(_team != 0, statusUI, positionRoot.GetChild(i), CBEvent);

                if (_team == 0)
                {
                    ui.SetUnitCardUI(cardIdx++, unit);
                    playerTeamCount++;
                }
                fIdInMovePanel.Add(unit.FieldId);
            }

        }

        return positionRoot;
    }

    private void UpdateSetSkillTarget()
    {
        if (skillActiveUnitFid == -1)
            return;

//#if UNITY_EDITOR || UNITY_STANDALONE
        if (!Input.GetMouseButtonDown(0))
//#elif UNITY_ANDROID
//		if (!(Input.GetTouch(0).phase == TouchPhase.Began))
//#endif
            return;

        RaycastHit hitInfo;
        //var ray = GameCore.Instance.GetWorldCam().ScreenToWorldPoint(Input.mousePosition);
        var ray = GameCore.Instance.GetWorldCam().ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitInfo, 99999f, LayerMask.GetMask("World")))
        {
            var ctrl = hitInfo.collider.gameObject.GetComponentInChildren<SpineCharacterCtrl>();
            if (ctrl != null)
            {
                for (int i = 0; i < skillTargetList.Length; i++)
                {
                    if (ctrl.FieldId == skillTargetList[i])
                    {
                        atkCtrl.EnqSkill(skillActiveUnitFid, ctrl.FieldId);
                        ClearTargetData();
                        break;
                    }
                }
            }
        }
        //Debug.Log("ray:" + ray + ",  hit: " + hitInfo);
        //Debug.DrawRay(ray.origin, ray.direction * 99999f, Color.red, 5f);

        return;
    }

    internal void ClearTargetData()
    {
        skillActiveUnitFid = -1;
        skillTargetList = null;
        ui.DisableSelectable();

        battleField.DisableSkillHightlight();
    }

    internal void SetTargetAbleList(int _fieldId)
    {
        ClearTargetData();

        int[] list;
        if (BattleUnitPool.GetTeamByFieldId(_fieldId) == 0)
            list = unitPool.AttackAbleList(battleCount);
        else
            list = unitPool.AttackAbleList(0);

        if (GameCore.atuoPlay)
        {
            atkCtrl.EnqSkill(_fieldId, list[UnityEngine.Random.Range(0, list.Length)]);
        }
        else
        {
            // 선택 강조
            for (int j = 0; j < list.Length; ++j)
            {
                var tgUnit = unitPool.GetUnitByFieldID(list[j]);
                ui.AddSelectable(tgUnit);
                battleField.ShowSkillHighlight(tgUnit.RootTf, false);
            }

            //시전자 강조
            var unit = unitPool.GetUnitByFieldID(_fieldId);
            battleField.ShowSkillHighlight(unit.RootTf, true);

            // 데이터 저장
            skillActiveUnitFid = _fieldId;
            skillTargetList = list;
        }
    }

    protected void CBClickUnitCard(int _fieldId)
    {
        if (skillActiveUnitFid != -1)
        {
            var tmp = skillActiveUnitFid;
            ClearTargetData();

            if (tmp == _fieldId)
                return;
        }

        var unit = unitPool.GetUnitByFieldID(_fieldId);
        if (unit == null)
            return;

        if (atkCtrl.SkillQueue.Contains(_fieldId))
            return;

        // skill able?
        if (unit.Skillable)
        {
            var effects = unit.SkillData.Data.effects;

            //// 타겟팅
            //if (unit.SkillData.bSetTarget)
            //{
            //	SetTargetAbleList(_fieldId);
            //	return;
            //}
            //// 논타겟팅
            //else
            //{
            //	// EnQ Skill
            //	EnqSkill(unit.FieldId, -1);
            //}

            // 항상 오토 타게팅
            atkCtrl.EnqSkill(unit.FieldId, -1);
        }

        // disable UnitCard
    }

    public bool bBlockTeamSkil;
    protected void CBClickTeamSkill()
    {
        if (bBlockTeamSkil)
            return;

        var tsKey = GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(para.playerTeam);
        if (tsKey == 0)
            return;

        if (skillActiveUnitFid != -1)
            ClearTargetData();

        if (atkCtrl.teamSkillAcc >= 1f)
        {
            atkCtrl.QueueingTeamSkill = true;
            Debug.Log("Click TeamSkill");
            ui.ShowTeamSkillActiveCover(true);
            useTeamSkillCount++;
        }
        else
        {
            //Debug.Log("No Team Skill!");
        }
    }


    public bool bBlockStrikeSkil;
    protected void CBClickStrikeSkill()
    {
        if (bBlockStrikeSkil)
            return;

        if (ui.IsStrikeDeath == true)
            return;

        if (atkCtrl.QueueingStrikeSkill)
            return;

        if (skillActiveUnitFid != -1)
            ClearTargetData();

        if (atkCtrl.StrikeSkillAcc >= 1f)
        {
            atkCtrl.QueueingStrikeSkill = true;
            ui.ShowStrikeSkillActiveCover(true);
        }
        else
        {
            Debug.Log("No Strike Skill");
        }
    }
    public IEnumerator CoFriendSkill(Action _cb)
    {
        while (true)
        {
            yield return null;
            if (ui == null)
                break;
            // UI 연출중에는 업데이트 하지 않는다.
            if (ui.IsAnmationing())
                continue;

            //// 특수 버프 코루틴 중에는 업데이트를 하지 않음
            //if (coSpecialBuff > 1)
            //	continue;

            // 애니메이션이 동작 중인지 검사
            if (unitPool.CheckFriendRunning())
                continue;

            break;
        }
        if (ui != null)
        {
            _cb();
        }
    }
    public IEnumerator CoRunningWait(Action _cb)
    {
        ++coSpecialBuff;

        while (true)
        {
            yield return null;
            if (ui == null)
                break;
            // UI 연출중에는 업데이트 하지 않는다.
            if (ui.IsAnmationing())
                continue;

            //// 특수 버프 코루틴 중에는 업데이트를 하지 않음
            //if (coSpecialBuff > 1)
            //	continue;

            // 애니메이션이 동작 중인지 검사
            if (unitPool.CheckRunning())
                continue;

            break;
        }
        if (ui != null)
        {
            --coSpecialBuff;
            _cb();
        }
    }
    public IEnumerator CoFriendRunningWait(Action _cb)
    {
        ++coSpecialBuff;

        while (true)
        {
            yield return null;
            if (ui == null)
                break;
            // UI 연출중에는 업데이트 하지 않는다.
            if (ui.IsAnmationing())
                continue;

            //// 특수 버프 코루틴 중에는 업데이트를 하지 않음
            //if (coSpecialBuff > 1)
            //	continue;

            // 애니메이션이 동작 중인지 검사
            if (unitPool.CheckFriendRunning())
                continue;

            break;
        }
        if (ui != null)
        {
            --coSpecialBuff;
            _cb();
        }
    }

    public IEnumerator CoSpecialBuff(Func<IEnumerator> _cb)
    {
        ++coSpecialBuff;

        while (true)
        {
            yield return null;

            // UI 연출중에는 업데이트 하지 않는다.
            if (ui.IsAnmationing())
                continue;

            //// 특수 버프 코루틴 중에는 업데이트를 하지 않음
            //if (coSpecialBuff > 1)
            //	continue;

            // 애니메이션이 동작 중인지 검사
            if (unitPool.CheckRunning())
                continue;

            break;
        }

        yield return _cb();

        --coSpecialBuff;
    }

    // 플레이어의 캐릭터들의 사망여부를 마지막에 체크하기 위한 함수
    protected List<int> CheckDead()
    {
        List<int> unDeadList = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            var unit = unitPool.GetUnitByTeam(0, i);
            if (unit != null)
            {
                unDeadList.Add(unit.Data.id);
            }
        }
        return unDeadList;
    }

    AttackType GetExceptionAttackTypeOnlySfxSound(BattleUnitData _unit, bool _skill, bool _teamSkill)
    {
        if (_skill)
        {
            if (_teamSkill)
            {
                if (_unit.Data.charIdType == 1300041) // 마고 특별 예외
                {
                    int teamSkillNameNum = _unit.GetTeamSkillSkillNameNum();
                    if (teamSkillNameNum == 21)
                    {
                        return AttackType.None;
                    }
                }

                if (_unit.Data.charIdType == 1300048) // 영정 특별 예외
                {
                    int teamSkillNameNum = _unit.GetTeamSkillSkillNameNum();
                    if (teamSkillNameNum == 21)
                    {
                        return AttackType.None;
                    }
                }
            }
        }

        return GetExceptionAttackType(_unit, _skill, _teamSkill);
    }

    AttackType GetExceptionAttackType(BattleUnitData _unit, bool _skill, bool _teamSkill)
    {
        if (_skill)
        {
            if (_teamSkill)
            {
                if (_unit.Data.charIdType == 1300035) // 레이디 특별 예외
                {
                    int teamSkillNameNum = _unit.GetTeamSkillSkillNameNum();
                    if (teamSkillNameNum == 19)
                    {
                        return AttackType.Near;
                    }
                }
                if (_unit.Data.charIdType == 1300041) // 마고 특별 예외
                {
                    int teamSkillNameNum = _unit.GetTeamSkillSkillNameNum();
                    if (teamSkillNameNum == 21)
                    {
                        return AttackType.Near;
                    }
                }

                if (_unit.Data.charIdType == 1300048) // 영정 특별 예외
                {
                    int teamSkillNameNum = _unit.GetTeamSkillSkillNameNum();
                    if (teamSkillNameNum == 21)
                    {
                        return AttackType.Near;
                    }
                }
            }
            else
            {
                if (_unit.Data.charId == 1110035 || // 복면강도_강아지 특별 예외
                    _unit.Data.charIdType == 1300018) // 퇴마사 헤이즈 특별 예외
                {
                    return AttackType.Near;
                }

                if (_unit.Data.charIdType == 1300018) // 염호 특별 예외
                {
                    return AttackType.Sword;
                }
            }
        }

        return _unit.Data.atkType;
    }
}
