using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;
using System;

internal enum SpineAnimation
{
	None,			// 아무것도 안함
	Idle,			// 대기
	Run,			// 이동
	//AttackEntry,   // 공격
	Attack,			// 공격
    ChargeAttack,   //차지 공격
	//AttackEnd,	// 공격 종료
	Skill,		    // 스킬
	TeamSkill,      // 팀스킬
	Hit,			// 피격
    HitMaxGuard,    //피격 (맥스가드)
	Death,		    // 사망

	Victory,	    // 승리시 애니메이션
	MyRoom,		    // 마이룸에서 하는 액션

	Move,			// 자리 이동
	GuardStart,     // 특수 버프 행동
	GuardEnd,  

    Behind,
}


public class SpineCharacterCtrl : MonoBehaviour
{
    enum IDHAnimations
    {
        Idle,
        Move,
        Jump_Start,
        Jump_End,
        Attack,
        ChargeAttack,
        Skill,
        TeamSkill,
        Hit,
        Victory,
        MyRoom,
        Death,
        Win01,
        Win02,
        Win03,
        Count
    }

    private int DefaultLayerValue = 1;

    int fieldId;		// 현재 유닛의 필드 아이디
	int targetFlag;     // 공격대상에대한 플래그.(0~5의 비트를 사용) ( 6 ~의 비트는 팀 인덱스 값)

	float width, height;// 캐릭터 높이
	float moveDist;		// 이동 거리

	bool bMove;         // 일반 공격시 이동 여부
	bool bTurn;         // 이동시 턴 여부(일반 공격 , 스킬시 복귀 애니메이션에서 반대로 적용된다.)
	bool bDead;         // 사망 여부

    Spine.Animation[] Animations;  // 애니메이션 캐시 리스트
    float moveDelayTime;
    internal SkeletonAnimation skelAnim;
    
	internal SpineAnimation lastAnim { get; private set; }    // 마지막 애니메이션

	//private bool running;
	//internal bool isRunning { get { return running; } set { running = value;Debug.Log("[" + fieldId + "] running Set : " + running); } }		
	internal bool isRunning	{ get; set; }// 현재 애니메이션 중인지 여부(흐름을 멈추는 애니메이션만 해당)
	internal bool bSkill { get; set; }      // 현재 스킬중인지, 일반 공격 중인지 구분용도

	internal SkeletonAnimation SkelAnim { get { return skelAnim; } }

	Func<int, int, TrackEntry, BattleEvent, float> cbEvent;

    public bool skillStraight = false;


	internal int FieldId { get { return fieldId; } }
	internal int TargetFid { get { return targetFlag; } }

    private bool isFirst;
    Color colorGray = new Color(0.2f, 0.2f, 0.2f);

    //현재 진행되는 스킬이 팀스킬인지 아닌지 판별
    private bool isTeamSkill;
    public bool IsTeamSkill { get { return isTeamSkill; } }

    internal void Init(bool _flipX, int _fieldId, Func<int, int, TrackEntry, BattleEvent, float> _cbEvent, bool _world = true)
	{
		fieldId = _fieldId;
		cbEvent = _cbEvent;

		skelAnim = GetComponent<SkeletonAnimation>();

        //if(_world)
        //	skelAnim.skeletonDataAsset = SkeletonDataAsset.CreateRuntimeInstance(skelAnim.SkeletonDataAsset.skeletonJSON, skelAnim.SkeletonDataAsset.atlasAssets, true, 0.009f);
        //else //  default
        //	skelAnim.skeletonDataAsset = SkeletonDataAsset.CreateRuntimeInstance(skelAnim.SkeletonDataAsset.skeletonJSON, skelAnim.SkeletonDataAsset.atlasAssets, true, 0.65f);
        if (!_world)
            skelAnim.skeletonDataAsset = SkeletonDataAsset.CreateRuntimeInstance(skelAnim.SkeletonDataAsset.skeletonJSON, skelAnim.SkeletonDataAsset.atlasAssets, true, 0.65f);

        //skelAnim.AnimationState.Data.DefaultMix = 0.2f;
        skelAnim.initialFlipX = _flipX;
		skelAnim.Initialize(true);
		SetDefSortingOrder();
		bDead = false;

        var skelData = skelAnim.SkeletonDataAsset.GetSkeletonData(false);
        Animations = new Spine.Animation[(int)IDHAnimations.Count];
        Animations[(int)IDHAnimations.Idle] =       skelData.FindAnimation("Idle");
        Animations[(int)IDHAnimations.Move] =       skelData.FindAnimation("Move");
        Animations[(int)IDHAnimations.Jump_Start] = skelData.FindAnimation("Jump_start");
        Animations[(int)IDHAnimations.Jump_End] =   skelData.FindAnimation("Jump_end");
        Animations[(int)IDHAnimations.Attack] =     skelData.FindAnimation("Attack");
        Animations[(int)IDHAnimations.ChargeAttack] = skelData.FindAnimation("Charge_attack");
        Animations[(int)IDHAnimations.Skill] =      skelData.FindAnimation("Skill_attack");
        Animations[(int)IDHAnimations.TeamSkill] =  skelData.FindAnimation("Charge_attack");
        Animations[(int)IDHAnimations.Hit] =        skelData.FindAnimation("Shot");
        Animations[(int)IDHAnimations.Victory] =    skelData.FindAnimation("Win_sp");
        Animations[(int)IDHAnimations.MyRoom] =     skelData.FindAnimation("Emotion");
        Animations[(int)IDHAnimations.Death] =      skelData.FindAnimation("Dead");
        Animations[(int)IDHAnimations.Victory] =    skelData.FindAnimation("Win_sp");
        Animations[(int)IDHAnimations.Win01] =      skelData.FindAnimation("Win_01");
        Animations[(int)IDHAnimations.Win02] =      skelData.FindAnimation("Win_02");
        Animations[(int)IDHAnimations.Win03] =      skelData.FindAnimation("Win_03");
        moveDelayTime = BattleSys.MoveDelay + Animations[(int)IDHAnimations.Jump_Start].duration;
        SetAnimation(SpineAnimation.Idle);
	}

    public void SetDefaultOrderLayerValue(int value)
    {
        DefaultLayerValue = value;
    }

	internal void ToggleFlipX()
	{
		skelAnim.skeleton.flipX = !skelAnim.skeleton.flipX;
	}

	internal void FlipX(bool _flip)
	{
		skelAnim.skeleton.flipX = _flip;
	}

	internal void Settarget(int _targetFlag)
	{
		//Debug.Log("[" + fieldId + "] Set Target : " + _targetFlag);
		targetFlag = _targetFlag;
	}

	internal void SetDefSortingOrder()
	{
		//skelAnim.GetComponent<MeshRenderer>().sortingOrder = BattleUnitPool.GetLineByFieldId(fieldId) + 1;
		skelAnim.GetComponent<MeshRenderer>().sortingOrder = DefaultLayerValue;
	}
	//internal void SetTargetSortingOrder()
	//{
	//	//skelAnim.GetComponent<MeshRenderer>().sortingOrder = BattleUnitPool.GetLineByFieldId(targetFlag) + 1; 
	//	skelAnim.GetComponent<MeshRenderer>().sortingOrder = 1;
	//}
    internal void SetSpineCharacterColor(bool isDark)
    { skelAnim.skeleton.SetColor(isDark ? colorGray : Color.white);  }
    //internal void SetAttackerFrontSortingOrder()
    //{ skelAnim.GetComponent<MeshRenderer>().sortingOrder = 3; }
    internal void SetFrontSortingOrder()
    { skelAnim.GetComponent<MeshRenderer>().sortingOrder = 2; }
    internal void SetSortingOrder(int sortingOrderNum)
    {
        skelAnim.GetComponent<MeshRenderer>().sortingOrder = sortingOrderNum;
    }
    internal int GetSortingOrder()
    {
        return skelAnim.GetComponent<MeshRenderer>().sortingOrder;
    }

    private void CBMove(TrackEntry _trackEntry)
	{
		if (cbEvent == null)
			return;

		if (bMove)
			moveDist = cbEvent(fieldId, targetFlag, _trackEntry, BattleEvent.Move);
		else
            moveDist = cbEvent(fieldId, targetFlag, _trackEntry, BattleEvent.MoveReturn);

        bMove = false;//!bMove;
	}

	private void CBTurn(TrackEntry _te)
	{
		if (!bMove)
			cbEvent(fieldId, targetFlag, _te, BattleEvent.TurnBack);
		else
			cbEvent(fieldId, targetFlag, _te, BattleEvent.TurnForward);
	}

	private void CBAddDelay(TrackEntry _te)
	{
		_te.Delay = moveDist* 20000;
	}

	/// <summary>
	/// 애니메이션 중 이벤트 발생시 핸들링하는 루틴 래퍼
	/// </summary>
	/// <param name="_te"></param>
	private void CBEvent(TrackEntry _te)
	{
		CBEvent(_te, null);
	}
	/// <summary>
	/// 애니메이션 중 이벤트 발생시 핸들링하는 루틴
	/// </summary>
	/// <param name="_trackEntry"></param>
	/// <param name="_e"></param>
	private void CBEvent(TrackEntry _trackEntry, Spine.Event _e)
	{
        //if (_e != null) Debug.Log("Event [" + fieldId + "] " + _e.Data.Name + "  " + lastAnim);
        //else            Debug.Log("Event [" + fieldId + "] Death  " + lastAnim);
        if (cbEvent == null)
			return;

        string[] eventName = new string[] { "attack" };
        if (_e != null)
            eventName = _e.Data.Name.Replace(" ", "").Split(',');


        foreach (var name in eventName)
        {            switch (name)
            {
                case "move":
                    switch (lastAnim)
                    {
                        case SpineAnimation.Skill:
                            isTeamSkill = false;
                            cbEvent(fieldId, targetFlag, _trackEntry, BattleEvent.MoveStraight);
                            break;
                        case SpineAnimation.TeamSkill:
                            isTeamSkill = true;
                            cbEvent(fieldId, targetFlag, _trackEntry, BattleEvent.MoveStraight);
                            break;
                        default:
                            isTeamSkill = false;
                            break;
                    }
                    break;
                case "moveback":
                    {
                        GetBackStraightAnimation();
                        break;
                    }

                case "front":
                    cbEvent(fieldId, targetFlag, _trackEntry, BattleEvent.TurnForward);
                    break;

                case "back":
                case "zback":
                    cbEvent(fieldId, targetFlag, _trackEntry, BattleEvent.TurnBack);
                    break;
                case "attack":
                    switch (lastAnim)
                    {
                        case SpineAnimation.ChargeAttack:
                        case SpineAnimation.Attack:
                            isTeamSkill = false;
                            cbEvent(fieldId, targetFlag, _trackEntry, BattleEvent.Attack);
                            break;
                        case SpineAnimation.Skill:
                            isTeamSkill = false;

                            cbEvent(fieldId, targetFlag, _trackEntry, BattleEvent.Skill);
                            break;
                        case SpineAnimation.TeamSkill:
                            isTeamSkill = true;
                            //if (isFirst)
                            //{
                            //    isFirst = false;
                            //    cbEvent(fieldId, targetFlag, _trackEntry, BattleEvent.MoveStraight);
                            //}
                            //else
                            cbEvent(fieldId, targetFlag, _trackEntry, BattleEvent.TeamSkill);
                            break;

                        case SpineAnimation.Death:
                            isTeamSkill = false;
                            isRunning = false;
                            cbEvent(fieldId, 0, null, BattleEvent.Death);
                            break;
                    }
                    break;

            }
        }
	}

	


	/// <summary>
	/// Idle, Run, Death 이외의 모든 애니메이션 이 완료되면 이 루틴을 실행한다.
	/// </summary>
	/// <param name="_te"></param>
	private void CBEndAnimation(TrackEntry _te)
	{
		_te.Complete -= CBEndAnimation;
		if (bDead)
			return;

		//Debug.Log("_te :" + _te.animation.name);
		cbEvent(fieldId, 0, null, BattleEvent.ShowUI);
		cbEvent(fieldId, targetFlag, _te, BattleEvent.TurnForward);
		SetAnimation(SpineAnimation.Idle);
	}

	/// <summary>
	/// 스파인 애니메이션이 동작한다.
	/// </summary>
	/// <param name="_state">동작할 애니메이션</param>
	/// <param name="_bMove">공격/스킬시에만 유효하며, true일 경우 상대의 앞으로 이동한다.</param>
	/// <returns>애니메이션 동작 성공 여부</returns>
	internal bool SetAnimation(	SpineAnimation _state, bool _bMove = false)
	{
		if ((_state == SpineAnimation.Hit ||
            _state == SpineAnimation.HitMaxGuard) &&
                (lastAnim == SpineAnimation.ChargeAttack ||
                lastAnim == SpineAnimation.Attack ||
				 lastAnim == SpineAnimation.Skill))
			return false;

        //Debug.Log("[" + fieldId + "] set Animation : " + _state + "  " + Time.time);
        SpineAnimation checkMyRoomState = lastAnim;
        lastAnim = _state;
		bMove = _bMove;
		float duation;
		TrackEntry te = null;
        int randomValue;
        isTeamSkill = false;

        switch (_state)
		{
			case SpineAnimation.Idle:
				//SetDefSortingOrder();
				te = skelAnim.AnimationState.SetAnimation(0, Animations[(int)IDHAnimations.Idle], true);
				te.mixDuration = 0.2f;
				isRunning = false;
				return true;

			case SpineAnimation.Run:
				te = skelAnim.AnimationState.SetAnimation(0, Animations[(int)IDHAnimations.Move], true);
				te.mixDuration = 0.2f;
				isRunning = false;
				return true;

			case SpineAnimation.Death:
                if (bDead)
                    return true;
                te = skelAnim.AnimationState.SetAnimation(0, Animations[(int)IDHAnimations.Death], false);
				te.Complete += CBEvent;
				bDead = true;
				isRunning = true;
				return true;

			case SpineAnimation.Hit:
				te = skelAnim.AnimationState.SetAnimation(0, Animations[(int)IDHAnimations.Hit], false);
                isRunning = true;
				break;
            case SpineAnimation.HitMaxGuard:
                te = skelAnim.AnimationState.SetAnimation(0, Animations[(int)IDHAnimations.Hit], false);
                cbEvent(fieldId, targetFlag, null, BattleEvent.MaxGuardEffect);
                isRunning = true;
                break;
            case SpineAnimation.ChargeAttack:
                cbEvent(fieldId, 0, null, BattleEvent.HideUI);
                if (_bMove)
                {
                    te = SetJumpAnmation(true, false); //SetJump(true, false);

                    // 근거리 일반 공격
                    te.Start += (_te) => cbEvent(fieldId, targetFlag, null, BattleEvent.Start_Zoom);
                    
                    te = skelAnim.AnimationState.AddAnimation(0, Animations[(int)IDHAnimations.ChargeAttack], false, 0f);
                    te.Event += CBEvent;
                    te.Start += (_te) => cbEvent(fieldId, targetFlag, null, BattleEvent.ChargeAttackEffect);
                    te.Start += (_te) =>
                    {
                        if (GameCore.timeScale <= 2)
                            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Critical_Start);
                    };
                    te.Event += (_te, _e) => cbEvent(fieldId, targetFlag, null, BattleEvent.EndZoom);

                    te = SetJumpAnmation(false, true);
                }
                else
                {
                    te = skelAnim.AnimationState.SetAnimation(0, Animations[(int)IDHAnimations.ChargeAttack], false);
                    // 원거리 일반 공격
                    //te.Start += (_te) => cbEvent(fieldId, targetFlag, null, "start_zoom");
                    cbEvent(fieldId, targetFlag, null, BattleEvent.Dealyed_Zoom);
                    if (GameCore.timeScale <= 2)
                        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Critical_Start);

                    te.Event += CBEvent;
                    cbEvent(fieldId, targetFlag, null, BattleEvent.ChargeAttackEffect);
                    te.Event += (_te, _e) => cbEvent(fieldId, targetFlag, null, BattleEvent.EndZoom);
                }
                //SetFrontSortingOrder();
                //SetTargetSortingOrder();
                isRunning = true;
                break;
            case SpineAnimation.Attack:
				cbEvent(fieldId, 0, null, BattleEvent.HideUI);
				if (_bMove)
				{
                    SetJumpAnmation(true, false);

                    te = skelAnim.AnimationState.AddAnimation(0, Animations[(int)IDHAnimations.Attack], false, 0f);
					// 근거리 일반 공격
					te.Start += (_te) => cbEvent(fieldId, targetFlag, null, BattleEvent.Start_Zoom);
                    te.Event += CBEvent;
                    te.Event += (_te, _e) => cbEvent(fieldId, targetFlag, null, BattleEvent.EndZoom);

                    te = SetJumpAnmation(false, true);
                }
				else
				{
					te = skelAnim.AnimationState.SetAnimation(0, Animations[(int)IDHAnimations.Attack], false);
					// 원거리 일반 공격
					//te.Start += (_te) => cbEvent(fieldId, targetFlag, null, "start_zoom");
					cbEvent(fieldId, targetFlag, null, BattleEvent.Dealyed_Zoom);

                    te.Event += CBEvent;
                    te.Event += (_te, _e) => cbEvent(fieldId, targetFlag, null, BattleEvent.EndZoom);
				}
				//SetFrontSortingOrder();
				//SetTargetSortingOrder();
				isRunning = true;
				break;

			case SpineAnimation.Skill:
				cbEvent(fieldId, 0, null, BattleEvent.HideUI);
				//SetFrontSortingOrder();
				if (_bMove)
				{
                    if (!skillStraight)
                    {
                        te = SetJumpAnmation(true, false);
                        te.Start += (_te) => cbEvent(fieldId, targetFlag, null, BattleEvent.Skill_Sound_Personal);
                        te = skelAnim.AnimationState.AddAnimation(0, Animations[(int)IDHAnimations.Skill], false, 0f);
                    }
                    else
                    {
                        te = skelAnim.AnimationState.SetAnimation(0, Animations[(int)IDHAnimations.Skill], false);
                        bMove = false;
                    }

                    //te.Start += (_te) => cbEvent(fieldId, targetFlag, null, BattleEvent.Skill_Sound_Personal);
                    //te.Start += (_te) => cbEvent(fieldId, targetFlag, null, BattleEvent.Skill_Start);
					te.Start += (_te) => cbEvent(fieldId, targetFlag, null, BattleEvent.Start_Zoom_Skill);
					te.Event += CBEvent;
					te.Complete += (_te) => cbEvent(fieldId, targetFlag, null, BattleEvent.EndZoom);
                    
                    te = SetJumpAnmation(false, true);
                }
				else
				{
					//cbEvent(fieldId, targetFlag, null, BattleEvent.Skill_Start);
					te = skelAnim.AnimationState.SetAnimation(0, Animations[(int)IDHAnimations.Skill], false);
                    cbEvent(fieldId, targetFlag, null, BattleEvent.Dealyed_Zoom_Skill);
                    cbEvent(fieldId, targetFlag, null, BattleEvent.Skill_Sound_Personal);
                    //te.Start += (_te) => cbEvent(fieldId, targetFlag, null, BattleEvent.SoundStart);
                    te.Event += CBEvent;
					te.Event += (_te, _e) => cbEvent(fieldId, targetFlag, null, BattleEvent.EndZoom);
				}
				isRunning = true;
				break;

			case SpineAnimation.TeamSkill:
				
				break;
			
			case SpineAnimation.Move: // 특수 버프 행동 코루틴에서 사용하기 위해 Move 애니메이션만 하는 분기
				cbEvent(fieldId, 0, null, BattleEvent.HideUI);
				//CBTurn(null);
                te = SetJumpAnmation(true, false);
                //te = skelAnim.AnimationState.SetAnimation(0, "Jump_start", false);
				//te.Complete += CBMove;
				//duation = te.Animation.duration;
				//te = skelAnim.AnimationState.AddEmptyAnimation(0, 0f, BattleSys.MoveDelay + duation);
				//te = skelAnim.AnimationState.AddAnimation(0, "Jump_end", false, 0f);
				te.Complete += CBTurn;
				isRunning = true;
				break;

				// 승리시 애니메이션
			case SpineAnimation.Victory:
                randomValue = UnityEngine.Random.Range(0, 4);
                Spine.Animation victoryAnimation;
                if (randomValue == 0)
                {
                    victoryAnimation = Animations[(int)IDHAnimations.Victory];
                    if (victoryAnimation == null) victoryAnimation = Animations[UnityEngine.Random.Range(12, 15)];
                }
                else
                {
                    victoryAnimation = Animations[randomValue + 11];
                }

				te = skelAnim.AnimationState.SetAnimation(0, victoryAnimation, false);
				isRunning = true;
				break;

			case SpineAnimation.MyRoom:
                if (checkMyRoomState != SpineAnimation.Run && checkMyRoomState != SpineAnimation.Idle)
                    return false;
                randomValue = UnityEngine.Random.Range(0, 4);
                Spine.Animation myRoomAnimation;
                if (randomValue == 0)
                {
                    myRoomAnimation = Animations[(int)IDHAnimations.MyRoom];
                    if (myRoomAnimation == null) myRoomAnimation = Animations[UnityEngine.Random.Range(12, 15)];
                }
                else
                {
                    myRoomAnimation = Animations[randomValue + 11];
                }

                te = skelAnim.AnimationState.AddAnimation(0, myRoomAnimation, false, 0);
                te.Complete += (e) => AddAnimation(SpineAnimation.Idle);
                isRunning = true;
                return true;

			default: return false;
		}

		if( te != null )
			te.Complete += CBEndAnimation;

		return true;
	}
    private bool isJumpAble = true;
    internal bool GetBackStraightAnimation()
    {
        isJumpAble = false;
        cbEvent(fieldId, 0, null, BattleEvent.MoveReturnStraight);
        AddAnimation(SpineAnimation.Idle, false);
        return true;
    }
    internal bool GetBackAnimation()
    {
        cbEvent(fieldId, 0, null, BattleEvent.MoveReturn);
        AddAnimation(SpineAnimation.Idle, false);
        return true;
    }
    internal bool SetAnimationTeam(SpineAnimation _state, string _animationName, bool _bMove = false, bool _straight = false)
    {
        if ((_state == SpineAnimation.Hit ||
            _state == SpineAnimation.HitMaxGuard) &&
                (lastAnim == SpineAnimation.ChargeAttack ||
                lastAnim == SpineAnimation.Attack ||
                 lastAnim == SpineAnimation.Skill))
            return false;

        //Debug.Log("[" + fieldId + "] set Animation : " + _state + "  " + Time.time);
        var skelData = skelAnim.SkeletonDataAsset.GetSkeletonData(false);
        Spine.Animation nAnimation = skelData.FindAnimation(_animationName);
        lastAnim = _state;
        bMove = _bMove;
        TrackEntry te = null;
        cbEvent(fieldId, 0, null, BattleEvent.HideUI);
        isTeamSkill = false;
        //SetFrontSortingOrder();
        if (_bMove)
        {
            isFirst = true;
            bMove = false;
            te = skelAnim.AnimationState.SetAnimation(0, nAnimation, false);
            //te.Event += (_te, _e) => { bMove = false; };
            //te.Event += (_te, _e) => cbEvent(fieldId, targetFlag, null, BattleEvent.Start_Zoom_Skill);
            te.Event += CBEvent;
            /*te.Start += (_te) => */cbEvent(fieldId, targetFlag, null, BattleEvent.Skill_Sound_Team);
            te.Complete += (_te) => cbEvent(fieldId, targetFlag, null, BattleEvent.EndZoom);
            //te = SetJumpAnmation(false, true);
            if (_straight == false)
                te = SetJumpAnmation(false, true);
            //te.Complete += (_te) =>SetJump(false, true);
            //te = SetJump(false, true);
        }
        else
        {
            //cbEvent(fieldId, targetFlag, null, BattleEvent.Skill_Start);
            te = skelAnim.AnimationState.SetAnimation(0, nAnimation, false);
            cbEvent(fieldId, targetFlag, null, BattleEvent.Skill_Sound_Team);
            //cbEvent(fieldId, targetFlag, null, BattleEvent.Dealyed_Zoom_Skill);
            te.Event += CBEvent;
            //te.Event += (_te, _e) => cbEvent(fieldId, targetFlag, null, BattleEvent.EndZoom);
        }
        isRunning = true;

        if (te != null)
            te.Complete += CBEndAnimation;

        return true;
    }

    internal bool AddAnimation(SpineAnimation _state, bool _bMove = false)
    {
        lastAnim = _state;
        isTeamSkill = false;
        bMove = _bMove;
        TrackEntry te = null;

        switch (_state)
        {
            case SpineAnimation.Idle:
                //SetDefSortingOrder();
                te = skelAnim.AnimationState.AddAnimation(0, Animations[(int)IDHAnimations.Idle], true, 0);
                te.mixDuration = 0.2f;
                isRunning = false;
                return true;

            case SpineAnimation.Run:
                te = skelAnim.AnimationState.AddAnimation(0, Animations[(int)IDHAnimations.Move], true, 0);
                te.mixDuration = 0.2f;
                isRunning = false;
                return true;

            case SpineAnimation.Death:
                //if (bDead)
                //    return true;
                te = skelAnim.AnimationState.AddAnimation(0, Animations[(int)IDHAnimations.Death], false, 0);
                te.Complete += CBEvent;
                bDead = true;
                isRunning = true;
                return true;

            case SpineAnimation.Hit:
                te = skelAnim.AnimationState.AddAnimation(0, Animations[(int)IDHAnimations.Hit], false, 0);
                isRunning = true;
                break;
            case SpineAnimation.HitMaxGuard:
                te = skelAnim.AnimationState.SetAnimation(0, Animations[(int)IDHAnimations.Hit], false);
                cbEvent(fieldId, targetFlag, null, BattleEvent.MaxGuardEffect);
                isRunning = true;
                break;
            case SpineAnimation.Move:
                cbEvent(fieldId, 0, null, BattleEvent.HideUI);
                te = SetJumpAnmation(true, false);
                te.Complete += CBTurn;
                isRunning = true;
                break;

            // 승리시 애니메이션
            case SpineAnimation.Victory:
                Spine.Animation victoryAnimation = Animations[(int)IDHAnimations.Victory];
                if (victoryAnimation == null) victoryAnimation = Animations[UnityEngine.Random.Range(12, 15)];
                te = skelAnim.AnimationState.SetAnimation(0, victoryAnimation, false);
                isRunning = true;
                break;

            case SpineAnimation.MyRoom:
                Spine.Animation myRoomAnimation = Animations[(int)IDHAnimations.MyRoom];

                if (myRoomAnimation == null)
                {
                    myRoomAnimation = Animations[UnityEngine.Random.Range(12,15)];
                }

                te = skelAnim.AnimationState.AddAnimation(0, myRoomAnimation, false, 0);
                te.Complete += (e) => AddAnimation(SpineAnimation.Idle);
                isRunning = true;
                return true;

            default: return false;
        }

        if (te != null)
            te.Complete += CBEndAnimation;

        return true;
    }
    private void SetJump(bool _isFirst, bool _turn)
    {
        if (isJumpAble == false) return;
        GetBackAnimation();
        //isJumpAble = true;
        return;
        //return SetJumpAnmation(_isFirst, _turn);
    }
    TrackEntry SetJumpAnmation(bool _isFirst, bool _turn)
    {
        TrackEntry te;// = skelAnim.AnimationState.GetCurrent(0);
        if (_isFirst)   te = skelAnim.AnimationState.SetAnimation(0, Animations[(int)IDHAnimations.Jump_Start], false);
        else            te = skelAnim.AnimationState.AddAnimation(0, Animations[(int)IDHAnimations.Jump_Start], false, 0f);
        //if (_turn) te.Start += CBTurn;
        te.Complete += CBMove;

        te = skelAnim.AnimationState.AddEmptyAnimation(0, 0f, moveDelayTime);

        te = skelAnim.AnimationState.AddAnimation(0, Animations[(int)IDHAnimations.Jump_End], false, 0f);
        if (_turn) te.Complete += CBTurn;

        return te;
    }


    internal bool GetFlipX()
	{
		return skelAnim.skeleton.flipX;
	}

	internal float GetboundHeight()
	{
		if (height == 0)
		{
			float x, y;
			float[] vertexBuf = null;
			skelAnim.skeleton.GetBounds(out x, out y, out width, out height, ref vertexBuf);
		}
		return height;
	}

	internal float GetBoundWidth()
	{
		if (height == -1)
		{
			float x, y;
			float[] vertexBuf = null;
			skelAnim.skeleton.GetBounds(out x, out y, out width, out height, ref vertexBuf);
		}
		return width;
	}

    public void StartAirborn(Action _cbAction)
    {
        var te = skelAnim.AnimationState.SetAnimation(0, Animations[(int)IDHAnimations.Hit], false);
        float duration = Animations[(int)IDHAnimations.Hit].duration;
        StartCoroutine(GameCore.WaitForTime(duration / 3, _cbAction));
    }

    public void EndAirbron()
    {
        var te = skelAnim.AnimationState.SetAnimation(0, Animations[(int)IDHAnimations.Jump_End], false);
        te.Complete += CBEndAnimation;
    }
}
