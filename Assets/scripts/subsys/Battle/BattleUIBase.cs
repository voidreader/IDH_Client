using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal enum DamageType
{
    HPDamage,
    MTDamage,
    HPPoison,
    MTPoison,
    HPHeal,
    MTHeal,
    State,
    Count		//Never Use
}

/// <summary>
/// 전투시스템 UI Base
/// </summary>
public abstract class BattleUIBase : MonoBehaviour
{
    Dictionary<int, UnitStatusUI> unitstatusUIs;  // 유닛 체력바들
    private ObjectPool<DamagePrint> dmgprintPool; // 데미지 프린트 풀
    private ObjectPool<EffectAlert> eftAlrtPool; // 효과 알리미 풀
    private ObjectPool<Transform> selectablePool; // 선택 하이라이트 풀
    UIFont[] fonts;


    //protected bool animationing;
    protected bool isAnimationing;// { get { return animationing; } set { animationing = value; Debug.LogError(value); } }

    // top
    protected UIButton btAuto;
    protected GameObject autoBt;
    UISprite speedSprite;
    UILabel speedLabel;
    int tmpTimeScale = 1;    // 일시 정지 해제시 되돌아갈 시간배속

    protected UISprite autoSprite;
    protected UISprite autoLockSprite;
    UITweener autoTw;
    protected UILabel autoLabel;

    UISprite menuSprite;

    // bottom
    protected UnitCardUI[] unitCards;                       // 하단 유닛 카드들
    public UnitCardUI[] UnitCards { get { return unitCards; } }

    Action<int> cbClickUnitCard;

    //Team Skill Button
    [SerializeField] protected GameObject teamBt_Root;
    [SerializeField] UISprite team_innerGauge;
    [SerializeField] UISprite team_outterGauge;
    [SerializeField] UISprite team_face;
    [SerializeField] UISprite team_persent;
    [SerializeField] UILabel team_text;
    [SerializeField] UISprite team_thunder;
    [SerializeField] UISprite team_activeCover;
    Coroutine coTeamskill;
    Action cbClickTeamSkill;
    private bool isTeamDeath = false;
    public bool IsTeamDeath { get { return isTeamDeath; } }

    [SerializeField] private GameObject ptcTeamSkill1Obj;
    [SerializeField] private GameObject ptcTeamSkill2Obj;
    //[SerializeField] private ParticleSystem[] ptcTeamSkill1;  //1은 파티클이 없음
    [SerializeField] private ParticleSystem[] ptcTeamSkill2;

    // Friend Skill Button
    [SerializeField] protected GameObject strikeBt_Root;
    [SerializeField] private UISprite strike_Gauge;
    [SerializeField] private UISprite strike_Face;
    [SerializeField] private UISprite strike_activeCover;
    [SerializeField] private GameObject strike_FullGage;
    private Action cbClickStrikeSkill;
    private bool isStrikeDeath = false;
    public bool IsStrikeDeath { get { return isStrikeDeath; } }
    Coroutine coStrikeSkill;

    //Friend Skill Effect
    protected GameObject friendAnimation;
    protected FriendSkillUI friendSkillUI;
    protected float durationTime;

    // Visual Effect (스킬 이펙트)
    GameObject skillVisualEffect;
    UI2DSprite skillVEChar;
    UILabel skillVEName;
    //UITweener[] skillVETweens;

    GameObject skillVisualEffect_R;
    UI2DSprite skillVEChar_R;
    UILabel skillVEName_R;
    //UITweener[] skillVETweens_R;

    BattleTeamSkillEffectAnimation TSVE_Ctrl;

    [SerializeField] protected GameObject roundText;
    [SerializeField] protected GameObject[] roundNumTextSprite;

    protected float timeValue; // 흐른시간

    protected abstract void InitInternal(PvPSData _oppenetData);

    // 현재 진행율 표시
    internal virtual void SetProgress(float _value) { }
    protected virtual void UpdateTime() { }
    internal virtual void ShowRoundText(int _round) { }


    /// <summary>
    /// 초기화
    /// 반드시 사용전에 호출하여야 한다.
    /// </summary>
    /// <param name="_unitData"></param>
    internal void Init(BattleParaBase para, PvPSData _oppenetData = null)
    {
        unitstatusUIs = new Dictionary<int, UnitStatusUI>();

        // Battle Cards
        if (unitCards == null)
        {
            unitCards = new UnitCardUI[5];
            for (int i = 0; i < unitCards.Length; i++)
            {
                unitCards[i] = UnityCommonFunc.GetComponentByName<UnitCardUI>(gameObject, "UnitCard" + (i + 1));
                unitCards[i].AddEventdelegate(new EventDelegate(this, "OnClickCard"), i);
            }
        }

        // top
        speedSprite = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "timeSpeed");
        speedSprite.GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickSpeed));
        speedLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "timeSpeedText");
        menuSprite = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "menu");
        menuSprite.GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickMenu));
        SetSpeedButton(GameCore.Instance.TimeScaleChange);

        autoBt = UnityCommonFunc.GetGameObjectByName(gameObject, "auto");
        btAuto = autoBt.GetComponent<UIButton>();
        btAuto.onClick.Add(new EventDelegate(OnClickAuto));
        if (autoLabel == null)
        {
            autoLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "autoLabel");
            autoSprite = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "autoAnim");
            autoLockSprite = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "autoLock");
            autoTw = autoSprite.GetComponent<UITweener>();
        }
        //GameCore.atuoPlay = GameCore.atuoPlaySave;
        SetAutoButton(GameCore.atuoPlay);

        //Create Object Pool
        dmgprintPool = CreateObjectPool<DamagePrint>("Battle/damagePrint");
        eftAlrtPool = CreateObjectPool<EffectAlert>("Battle/EffectAlert");
        selectablePool = CreateObjectPool<Transform>("Battle/SelectableHighlight");

        //team skill button
        team_activeCover.gameObject.SetActive(false);
        teamBt_Root.GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickTeamSkill));
        isTeamDeath = false;
        /*
        if (team_text == null)
        {
            teamBt_Root = UnityCommonFunc.GetGameObjectByName(gameObject, "TeamSkillBt");
            teamBt_Root.GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickTeamSkill));
            team_text = UnityCommonFunc.GetComponentByName<UILabel>(teamBt_Root, "T_percent");
            team_thunder = UnityCommonFunc.GetComponentByName<UISprite>(teamBt_Root, "T_thunder");
            team_innerGauge = UnityCommonFunc.GetComponentByName<UISprite>(teamBt_Root, "T_gauge_in");
            team_outterGauge = UnityCommonFunc.GetComponentByName<UISprite>(teamBt_Root, "T_gauge_out");
            team_activeCover = UnityCommonFunc.GetComponentByName<UISprite>(teamBt_Root, "T_ActiveCover");
            team_activeCover.gameObject.SetActive(false);

            //ptcTeamSkill2Obj = UnityCommonFunc.GetComponentByName<Transform>(gameObject, "teamskillEffect_1").gameObject;
            //ptcTeamSkill1Obj = UnityCommonFunc.GetComponentByName<Transform>(gameObject, "teamskillEffect_2").gameObject;
            //ptcTeamSkill2 = ptcTeamSkill1Obj.GetComponentsInChildren<ParticleSystem>();
            //ptcTeamSkill1 = ptcTeamSkill2Obj.GetComponentsInChildren< ParticleSystem>();
            //ptcTeamSkill1 = UnityCommonFunc.GetComponentByName<ParticleSystem>(gameObject, "teamskillEffect_1");
            //ptcTeamSkill2 = UnityCommonFunc.GetComponentByName<ParticleSystem>(gameObject, "teamskillEffect_2");
        }
        */
        SetTeamSkillGauge(0f);

        //if (strike_Gauge == null)
        //{
        //    strikeBt_Root = UnityCommonFunc.GetGameObjectByName(gameObject, "FriendBt");
        //    strike_Gauge = UnityCommonFunc.GetComponentByName<UISprite>(strikeBt_Root, "F_gauge");
        //    strike_activeCover = UnityCommonFunc.GetComponentByName<UISprite>(strikeBt_Root, "F_ActiveCover");
        //    strike_activeCover.gameObject.SetActive(false);
        //    strikeBt_Root.GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickFriendSkill));
        //}
        CheckNowBattleSys(para);
        if(isStrikeDeath == false)
        {
            strike_activeCover.gameObject.SetActive(false);
            strikeBt_Root.GetComponent<UIButton>().onClick.Add(new EventDelegate(OnClickFriendSkill));
            SetStrikeSkillGauge(0f);
        }

        if (skillVisualEffect == null)
        {
            skillVisualEffect = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/SkillEffect_L", transform);
            skillVEChar = UnityCommonFunc.GetComponentByName<UI2DSprite>(skillVisualEffect, "Character");
            skillVEName = UnityCommonFunc.GetComponentByName<UILabel>(skillVisualEffect, "SkillName");
            //skillVETweens = skillVisualEffect.GetComponentsInChildren<UITweener>();
            //skillVETweens[0].onFinished.Add(new EventDelegate(() => { skillVisualEffect.SetActive(false); isAnimationing = false; }));
            skillVisualEffect.SetActive(false);
        }

        if (skillVisualEffect_R == null)
        {
            skillVisualEffect_R = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/SkillEffect_R", transform);
            skillVEChar_R = UnityCommonFunc.GetComponentByName<UI2DSprite>(skillVisualEffect_R, "Character");
            skillVEName_R = UnityCommonFunc.GetComponentByName<UILabel>(skillVisualEffect_R, "SkillName");
            //skillVETweens_R = skillVisualEffect_R.GetComponentsInChildren<UITweener>();
            //skillVETweens_R[0].onFinished.Add(new EventDelegate(() => { skillVisualEffect_R.SetActive(false); isAnimationing = false; }));
            skillVisualEffect_R.SetActive(false);
        }

        if (TSVE_Ctrl == null)
        {
            TSVE_Ctrl = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/TeamSkillVE1", transform).GetComponent<BattleTeamSkillEffectAnimation>();
            TSVE_Ctrl.init(() => isAnimationing = false);
        }

        if (fonts == null)
        {
            fonts = new UIFont[(int)DamageType.Count];
            fonts[(int)DamageType.HPDamage] = GameCore.Instance.ResourceMgr.GetLocalObject<GameObject>("Battle/BMF_Orange").GetComponent<UIFont>();
            fonts[(int)DamageType.MTDamage] = GameCore.Instance.ResourceMgr.GetLocalObject<GameObject>("Battle/BMF_Gray").GetComponent<UIFont>();
            fonts[(int)DamageType.HPPoison] = GameCore.Instance.ResourceMgr.GetLocalObject<GameObject>("Battle/BMF_Red").GetComponent<UIFont>();
            fonts[(int)DamageType.MTPoison] = GameCore.Instance.ResourceMgr.GetLocalObject<GameObject>("Battle/BMF_Gray").GetComponent<UIFont>();
            fonts[(int)DamageType.HPHeal] = GameCore.Instance.ResourceMgr.GetLocalObject<GameObject>("Battle/BMF_Green").GetComponent<UIFont>();
            fonts[(int)DamageType.MTHeal] = GameCore.Instance.ResourceMgr.GetLocalObject<GameObject>("Battle/BMF_Green").GetComponent<UIFont>();
            fonts[(int)DamageType.State] = GameCore.Instance.ResourceMgr.GetLocalObject<GameObject>("Battle/BMF_Gray").GetComponent<UIFont>();
        }

        isAnimationing = false;

        InitInternal(_oppenetData);

        StartCoroutine(CoTimer());

    }
    public void TurnOffButtomUI(bool isActive)
    {
        GetComponent<UIPanel>().enabled = isActive;
        ptcTeamSkill1Obj.SetActive(false);
        ptcTeamSkill2Obj.SetActive(false);
        GameCore.Instance.DoWaitCall(() =>
        {
            ptcTeamSkill1Obj.SetActive(false);
            ptcTeamSkill2Obj.SetActive(false);
        });
    }
    public void TurnOffTeamButton()
    {
        Color colorGray = new Color(0.2f, 0.2f, 0.2f);
        teamBt_Root.GetComponent<UIButton>().enabled = false;
        teamBt_Root.GetComponent<UISprite>().color = colorGray;
        team_text.color = colorGray;
        team_face.color = colorGray;
        team_persent.color = colorGray;
        ptcTeamSkill1Obj.SetActive(false);
        ptcTeamSkill2Obj.SetActive(false);
        team_thunder.enabled = false;
        team_text.text = "0";
        team_innerGauge.fillAmount = 0f;
        team_outterGauge.fillAmount = 0f;
        isTeamDeath = true;

        ShowTeamSkillActiveCover(false);
    }
    internal bool CheckNowBattleSys(BattleParaBase para)
    {
        switch(para.type)
        {
            case InGameType.Story:
            case InGameType.Daily:
                return true;
            default:
                isStrikeDeath = true;
                return false;
        }
    }
    public void TurnOffStrikeButton()
    {
        isStrikeDeath = true;
        Color colorGray = new Color(0.2f, 0.2f, 0.2f);
        strikeBt_Root.GetComponent<UISprite>().color = colorGray;
        strike_Face.color = colorGray;
        strikeBt_Root.GetComponent<UIButton>().enabled = false;
        strike_Gauge.enabled = false;
        strike_activeCover.enabled = false;
        strike_FullGage.SetActive(false);
}
    internal void SetSpeedButton(bool isActive)
    {
        speedSprite.GetComponent<UIButton>().enabled = isActive;
        if (isActive)
        {
            SetSpeedButton(GameCore.Instance.TimeScaleChange);
            speedSprite.alpha = 1f;
        }
        else
        {
            speedLabel.color = Color.gray;
            speedSprite.alpha = 0f;
        }

    }
    private ObjectPool<T> CreateObjectPool<T>(string _prefabName) where T : Component
    {
        var obj = GameCore.Instance.ResourceMgr.GetLocalObject<GameObject>(_prefabName);
        return new ObjectPool<T>(obj, transform);
    }

    internal void InitCallback(Action<int> _cbClickUnitCard, Action _cbClickTeamSkill, Action _cbClickStrikeSkill)
    {
        cbClickUnitCard = _cbClickUnitCard;
        cbClickTeamSkill = _cbClickTeamSkill;
        cbClickStrikeSkill = _cbClickStrikeSkill;
    }

    internal UnitCardUI SetUnitCardUI(int _idx, BattleUnitData _unitData)
    {
        unitCards[_idx].SetInfo(_unitData);
        return unitCards[_idx];
    }

    internal UnitStatusUI InstantiateStatusUI(bool _enemy, BattleUnitData _unitData)
    {
        if (unitstatusUIs.ContainsKey(_unitData.FieldId))
        {
            Debug.LogError("해당 필드아이디는 이미 UI가 존재합니다.");
            return null;
        }
        var ui = UnitStatusUI.Create(_enemy, _unitData, transform);
        unitstatusUIs.Add(_unitData.FieldId, ui);
        if (_unitData.FieldId < 0)
            ui.SetAvtive(false);
        return ui;
    }

    internal UnitCardUI GetUnitCardUI(int _fieldId)
    {
        for (int i = 0; i < unitCards.Length; ++i)
            if (unitCards[i].fieldId == _fieldId)
                return unitCards[i];

        //Debug.LogError("존재하지 않는 필드아이디입니다." + _fieldId);
        return null;
    }

    internal void SetUnitStatusUI(int _fieldId, UnitStatusUI _ui)
    {
        RemoveUnitStatusUI(_fieldId);
        unitstatusUIs.Add(_fieldId, _ui);
        if (_fieldId < 0)
            _ui.SetAvtive(false);
    }

    internal UnitStatusUI GetUnitStatusUI(int _fieldId)
    {
        if (unitstatusUIs.ContainsKey(_fieldId))
            return unitstatusUIs[_fieldId];

        //Debug.LogError("존재하지 않는 필드아이디입니다." + _fieldId);
        return null;
    }

    internal void RemoveUnitStatusUI(int _fieldId)
    {
        if (unitstatusUIs.ContainsKey(_fieldId))
        {
            GameObject.Destroy(unitstatusUIs[_fieldId].GetGameObject());
            unitstatusUIs.Remove(_fieldId);
        }
    }

    internal void ShowStatusUI(int _fieldId, bool _show)
    {
        if (unitstatusUIs.ContainsKey(_fieldId))
            unitstatusUIs[_fieldId].SetAvtive((_fieldId < 0) ? false : _show);
    }

    internal void ShowAllStatusBar(bool _show)
    {
        var it = unitstatusUIs.GetEnumerator();
        while (it.MoveNext())
        {
            if (it.Current.Key >= 0)
                it.Current.Value.SetAvtive(_show);
        }
            
    }



    internal void SetTeamSkillGauge(float _value)
    {
        if (isTeamDeath == true)
            return;

        StopTeamSkillGaugeAnimation();
        coTeamskill = StartCoroutine(CoTeamSkillGauge(_value));
    }

    public void StopTeamSkillGaugeAnimation()
    {
        if (coTeamskill != null)
            StopCoroutine(coTeamskill);
    }

    internal bool isTeamSkillAble = false;
    internal void SetTeamSkillEffect(float _value)
    {
        // 팀스킬 게이지 파티클 효과
        if (!team_activeCover.gameObject.activeInHierarchy && _value >= 1f)
        {
            if (_value >= 2f)
            {
                ptcTeamSkill2Obj.gameObject.SetActive(true);
                ptcTeamSkill1Obj.gameObject.SetActive(false);
                for (int i = 0; i < 2; i++)
                {
                    ptcTeamSkill2[i].Play();
                }
            }
            else if (_value >= 1f)
            {
                ptcTeamSkill2Obj.gameObject.SetActive(false);
                ptcTeamSkill1Obj.gameObject.SetActive(true);
            }
        }
        else
        {
            ptcTeamSkill2Obj.gameObject.SetActive(false);
            ptcTeamSkill1Obj.gameObject.SetActive(false);
        }
        
    }
    internal bool isInnerGaugeFull = false;
    internal bool isOuterGaugeFull = false;
    //teamSkillAcc
    internal void SetTeamSkillGaugeBool(float value)
    {
        isInnerGaugeFull = false;
        isOuterGaugeFull = false;
        if (value >= 1)
        {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_TeamSkillGauge_100);
            isInnerGaugeFull = true;
        }
        if(value - 1 >= 1)
        {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_TeamSkillGauge_200);
            isOuterGaugeFull = true;
        }
    }

    IEnumerator CoTeamSkillGauge(float _value)
    {
        float prevValue = team_innerGauge.fillAmount + team_outterGauge.fillAmount;
        float gap = Mathf.Abs(_value - prevValue);
        bool bInc = 0 < _value - prevValue;
        float acc = 0f;
        isTeamSkillAble = _value >= 1f;

        if (bInc)
        {
            while (acc < gap)
            {
                if(isTeamDeath == true) break;

                acc = Mathf.Min(gap, acc + Time.deltaTime * (bInc ? 0.4f : 1f));// * GameCore.timeScale;

                var value = prevValue + (bInc ? acc : -acc);
                team_innerGauge.fillAmount = value;
                if(team_innerGauge.fillAmount == 1 && isInnerGaugeFull == false)
                {
                    isInnerGaugeFull = true;
                    GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_TeamSkillGauge_100);
                }
                team_outterGauge.fillAmount = value - 1f;
                if(team_outterGauge.fillAmount == 1 && isOuterGaugeFull == false)
                {
                    isOuterGaugeFull = true;
                    GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_TeamSkillGauge_200);
                }
                team_text.text = (value * 100).ToString("0");
                team_thunder.gameObject.SetActive(value >= 1f);

                SetTeamSkillEffect(value);
                yield return null;
            }
        }

        if (isTeamDeath == true)
        {
            TurnOffTeamButton();
        }
        else
        {
            team_innerGauge.fillAmount = _value;
            team_outterGauge.fillAmount = _value - 1f;
            team_text.text = (_value * 100).ToString("0");
            team_thunder.gameObject.SetActive(_value >= 1f);
            SetTeamSkillEffect(_value);
        }
    }
    internal void ShowTeamSkillActiveCover(bool _active)
    {
        team_activeCover.gameObject.SetActive(_active);
        if (_active)
        {
            ptcTeamSkill2Obj.gameObject.SetActive(false);
            ptcTeamSkill1Obj.gameObject.SetActive(false);
        }
    }

    internal void SetTeamSkillCover(int _spriteKey)
    {
        GameCore.Instance.SetUISprite(team_activeCover, _spriteKey);
    }
    internal bool isStrikeSkillAble = false;
    internal void SetStrikeSkillGauge(float _value)
    {
        if (isStrikeDeath == true)
            return;
        if (coStrikeSkill != null)
            StopCoroutine(coStrikeSkill);
        coStrikeSkill = StartCoroutine(CoStrikeSkillGauge(_value));

    }
    internal bool isStrikeFull = false;

    IEnumerator CoStrikeSkillGauge(float _value)
    {
        float prevValue = strike_Gauge.fillAmount;
        float gap = Mathf.Abs(_value - prevValue);
        bool bInc = 0 < _value - prevValue;
        float acc = 0f;
        isStrikeSkillAble = _value >= 1f;

        if (bInc)
        {
            while (acc < gap)
            {
                if (isStrikeDeath == true) break;

                acc += Time.deltaTime * (bInc ? 0.4f : 1f);// * GameCore.timeScale;

                var value = prevValue + (bInc ? acc : -acc);
                strike_FullGage.SetActive(value >= 1f);
                strike_Gauge.fillAmount = value;
                yield return null;
            }
        }
        
        strike_FullGage.SetActive(strike_activeCover.gameObject.activeSelf == true ? false : _value >= 1f);
        strike_Gauge.fillAmount = _value;
        if(strike_Gauge.fillAmount == 1 && isStrikeFull == false)
        {
            isStrikeFull = true;
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Strike_Charge);
        }
    }
    internal void ShowStrikeSkillActiveCover(bool _active)
    {
        strike_activeCover.gameObject.SetActive(_active);
        if (_active)
        {
            strike_FullGage.SetActive(false);
        }
    }
    /// <summary>
    /// 데미지 프리터를 활성/출력한다.
    /// </summary>
    /// <param name="_pos">월드 기준 생성좌표(타겟유닛의 기준점)</param>
    /// <param name="_dmg">출력할 데미지값</param>
    /// <param name="_left">출력 이동 방향(true - 왼족으로 움직임)</param>
    internal void ShowDamagePrint(Vector3 _pos, int _dmg, bool _left, DamageType _type, DamagePower _atkDp, DamagePower _grdDp, bool _over)
    {
        var x = UnityEngine.Random.Range(-0.1f, 0.1f);
        var y = UnityEngine.Random.Range(-0.1f, 0.1f) + 0.5f;

        var font = _over ? fonts[(int)DamageType.HPPoison] : fonts[(int)_type];
        dmgprintPool.BringObject().Init(_pos + new Vector3(x, y, 0f), _dmg, _left, font, _atkDp, _grdDp, _over);
    }

    internal void ShowEffectAlert(Vector3 _pos, EffectType _type, int _value)
    {
        var x = UnityEngine.Random.Range(-0.05f, 0.05f);
        var y = UnityEngine.Random.Range(-0.05f, 0.05f) + 0.2f;

        eftAlrtPool.BringObject().Init(_pos + new Vector3(x, y, 0f), _type, _value);
    }

    internal void ShowEffectAlert(Vector3 _pos, UnitStat _type, float _add, int _value)
    {
        var x = UnityEngine.Random.Range(-0.05f, 0.05f);
        var y = UnityEngine.Random.Range(-0.05f, 0.05f) + 0.2f;

        eftAlrtPool.BringObject().Init(_pos + new Vector3(x, y, 0f), _type, _add > 0, _value);
    }

    internal void AddSelectable(BattleUnitData _unit)
    {
        var tf = selectablePool.BringObject();
        var pos = GameCore.Instance.WorldPosToUIPos(_unit.OnHeadTf.position);
        tf.localPosition = pos + new Vector3(0f, 0f, 0f);
    }

    internal void DisableSelectable()
    {
        selectablePool.ReturnObjectAll();
    }

    internal void ShowSkillVisualEffect(BattleUnitData _unit, bool _right = false)
    {
        isAnimationing = true;

#if UNITY_EDITOR
        var timer = new AcumulateTimer();
        timer.Begin(true);
#endif
        if (_right)
        {
            skillVEName_R.text = _unit.SkillData.Data.name;
            GameCore.Instance.ResourceMgr.GetObject<Sprite>(ABType.AB_Atlas, _unit.Data.GetIllustSpeiteKey(), (_sprite) =>
            {
                if (_sprite == null)
                {
                    //Debug.LogError("Not Found Skill Sprite." + _unit.Data.charIdType);
                    isAnimationing = false;
                    return;
                }

                skillVEChar_R.sprite2D = _sprite;
                skillVisualEffect_R.SetActive(true);
                /*
                for (int i = 0; i < skillVETweens_R.Length; ++i)
                {
                    skillVETweens_R[i].ResetToBeginning();
                    skillVETweens_R[i].PlayForward();
                    skillVETweens_R[i].duration = 1.5f / GameCore.timeScale;
                }
                */
                float length = skillVisualEffect_R.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length;
                StartCoroutine(GameCore.WaitForTime(length + 1f, () => {
                    skillVisualEffect_R.SetActive(false); isAnimationing = false;
                }));
            });
        }
        else
        {
            skillVEName.text = _unit.SkillData.Data.name;
            GameCore.Instance.ResourceMgr.GetObject<Sprite>(ABType.AB_Atlas, _unit.Data.GetIllustSpeiteKey(), (_sprite) =>
            {
                if (_sprite == null)
                {
                    isAnimationing = false;
                    return;
                }

                skillVEChar.sprite2D = _sprite;
                skillVisualEffect.SetActive(true);
                /*
                for (int i = 0; i < skillVETweens.Length; ++i)
                {
                    skillVETweens[i].ResetToBeginning();
                    skillVETweens[i].PlayForward();
                    skillVETweens[i].duration = 1.5f / GameCore.timeScale;
                }
                */
                float length = skillVisualEffect.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length;
                StartCoroutine(GameCore.WaitForTime(length + 1f, () => {
                    skillVisualEffect.SetActive(false); isAnimationing = false;
                }));
            });
        }

#if UNITY_EDITOR
        timer.End();
        //Debug.Log("Skill Visual Effect Delay : " + timer.delay);
#endif
    }

    internal void ShowTeamSkillVisualEffect(string _name, int[] _units)
    {
        isAnimationing = true;
        TSVE_Ctrl.Play(_name, _units);
    }

    internal void SetSpeedButton(int _speed)
    {
        string speedText = null;
        switch(_speed)
        {
            case 1:
                speedText = "x1";
                break;
            case 2:
                speedText = "x2";
                break;
            case 3:
                speedText = "x4";
                break;
            default:
                speedText = "x1";
                break;
        }
        speedLabel.text = speedText;
        if (_speed == 1)
        {
            speedLabel.color = Color.white;
            //speedLabel.text = "x1";
            speedSprite.spriteName = "CIRCLE_02_01";
        }
        else
        {
            speedLabel.color = Color.yellow;
            //speedLabel.text = "x" + _speed.ToString("0");
            speedSprite.spriteName = "CIRCLE_02_02";
        }
    }
    internal void SetAutoButton(bool _auto)
    {
        if (!_auto)
        {
            autoLabel.color = Color.white;
            autoSprite.color = Color.white;
            autoTw.enabled = false;
        }
        else
        {
            autoLabel.color = Color.yellow;
            autoSprite.color = Color.yellow;
            //autoTw.ResetToBeginning();
            //autoTw.PlayForward();
            autoTw.enabled = true;
        }
    }

    internal bool IsAnmationing()
    {
        return isAnimationing;
    }
    internal void SetIsAnmationing(bool _setValue)
    {
        isAnimationing = _setValue;
    }
    public virtual void OnClickExistBattle()
    {
        GameCore.Instance.EventMgr.SendEvent(new GameEvent(GameEventType.MsgAlert, new MsgAlertPara(MsgAlertType.Notice, "포기하기",
               new MsgAlertBtnData[] { new MsgAlertBtnData("취소", new EventDelegate(()=>{
                                        GameCore.Instance.CloseMsgWindow();
                                        ReturnSpeed();
                                        OnClickMenu();
                                    }), true, null, SFX.Sfx_UI_Cancel),
                                    new MsgAlertBtnData("확인", new EventDelegate(()=>{
                                        ReturnSpeed();
                                        //GameCore.Instance.StopAllCoroutines();
                                        GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
                                        GameCore.Instance.CloseMsgWindow();
                                    })) },
               0, "전투는 실패 처리됩니다.\n 메인 화면으로 나가시겠습니까?"
              )));
    }
    public void OnClickMenu()
    {
        if (GameCore.Instance.lobbyTutorial != null && GameCore.Instance.lobbyTutorial.IsRunning)
            return;

        BattleSysBase battleSysBase = (BattleSysBase)GameCore.Instance.SubsysMgr.GetNowSubSys();
        BattleSysState state = battleSysBase.State;
        if (state == BattleSysState.Prepare || state == BattleSysState.Story || state == BattleSysState.Chemistry)
            return;
        
        var timeScale = GameCore.Instance.TimeScaleChange;
        tmpTimeScale = timeScale;
        GameCore.Instance.TimeScaleChange = 0;
        menuSprite.spriteName = "ICON_PAUSE_02";

        GameCore.Instance.ShowObjectVertical("일시정지", null, null, 0, new MsgAlertBtnData[] {
                                    new MsgAlertBtnData("계속하기", new EventDelegate(()=>{
                                        GameCore.Instance.CloseMsgWindow();
                                        ReturnSpeed();
                                        }),true,"BTN_03_01_{0:00}"),// new Color(1f/ 255f,158f/ 255f,89f/ 255f)),
                                    new MsgAlertBtnData("사운드", new EventDelegate(()=>{
                                        GameCore.Instance.CloseMsgWindow();
                                        GameCore.Instance.SoundMgr.CreateVolumeOption(this.transform, ()=>{
                                            ReturnSpeed();
                                            OnClickMenu();
                                        });
                                        //아직 기능 없음
                                        })),
                                    new MsgAlertBtnData("나가기",new EventDelegate(()=>{
                                        GameCore.Instance.CloseMsgWindow();
                                        OnClickExistBattle();
                                        }))});
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
    }
    public void ReturnSpeed()
    {
        //tmpTimeScale = 1;
        GameCore.Instance.TimeScaleChange = tmpTimeScale;
        menuSprite.spriteName = "ICON_PAUSE_01";
    }

    public void OnClickAuto()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        GameCore.atuoPlay = !GameCore.atuoPlay;
        GameCore.atuoPlaySave = GameCore.atuoPlay;
        SetAutoButton(GameCore.atuoPlay);
        Debug.Log("GameCore.atuoPlay : " + GameCore.atuoPlay);
    }


    public void OnClickSpeed()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        var prevSpeed = GameCore.Instance.TimeScaleChange;
        var nextSpeed = prevSpeed + 1;


        if (prevSpeed == 0)
            return;

        if (nextSpeed > 3)
        {
            GameCore.Instance.TimeSave = 1;
            //GameCore.Instance.TimeScaleChange = 1;
        }
        else
        {
            GameCore.Instance.TimeSave = nextSpeed;
            //GameCore.Instance.TimeScaleChange = nextSpeed;
        }           

        SetSpeedButton(GameCore.Instance.TimeScaleChange);
        //SetParticleTimeScale();
    }


    public void OnClickFriendSkill()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        cbClickStrikeSkill();
    }

    public void OnClickTeamSkill()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        cbClickTeamSkill();
    }

    public virtual void OnClickCard(int _num)
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        cbClickUnitCard(unitCards[_num].fieldId);
    }

    internal virtual IEnumerator CoTimer()
    {
        timeValue = 0;

        while (true)
        {
            yield return null;

            timeValue += Time.deltaTime;// * GameCore.timeScale;
            UpdateTime();
        }
    }

    internal TimeSpan GetPlayTime()
    {
        return new TimeSpan(0, 0, (int)(timeValue / 60), (int)(timeValue % 60), (int)((timeValue % 1) * 1000));
    }


    internal void CreateFriendSkillUI(BattlePara _para)
    {
        var unitData = GameCore.Instance.DataMgr.GetUnitData(_para.friendIcon);
        //int friendIcon = _para.friendIcon % 1000;
        //friendIcon = friendIcon / 10 + 1 + 1300000;

        friendAnimation = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/friendTeamVE_Root", GameCore.Instance.ui_root);
        friendSkillUI = friendAnimation.GetComponent<FriendSkillUI>();
        TeamSkillDataMap teamSkillData = null;
        if (0 < _para.friendTeamSkill)
            teamSkillData = GameCore.Instance.DataMgr.GetTeamSkillData(_para.friendTeamSkill);

        string skillName;
        if (teamSkillData == null)
        {
            skillName = GameCore.Instance.DataMgr.GetSkillData(unitData.skillId).name;
            var typicaldata = GameCore.Instance.DataMgr.GetUnitData(_para.friendIcon);
            GameCore.Instance.SetUISprite(strike_activeCover, typicaldata.GetSmallProfileSpriteKey());
        }
        else
        {
            skillName = teamSkillData.name;
            int imageID = GameCore.Instance.DataMgr.GetTeamSkillData(_para.friendTeamSkill).imageID;
            GameCore.Instance.SetUISprite(strike_activeCover, imageID);
        }
        friendSkillUI.SetLabel(_para.friendName, skillName);
        friendSkillUI.SetSpriteArray(unitData.GetIllustSpeiteKey());
        durationTime = friendSkillUI.Animator.runtimeAnimatorController.animationClips[0].length + 1;

    }
    internal void TurnOffFriendSkillUI()
    {
        friendAnimation.SetActive(false);
    }
    internal void TurnOnFriendSkillUI()
    {
        friendAnimation.SetActive(true);
        isAnimationing = true;
        StartCoroutine(GameCore.WaitForTime(durationTime, () =>
        {
            friendAnimation.SetActive(false);
            isAnimationing = false;
        }));
    }
}
