using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class ResultUI : ResultUIBase
{

    [Header("TITLE")]
    [SerializeField] UISprite spClear;
    [SerializeField] GameObject topStarRoot;
    [SerializeField] GameObject topGridRoot;
    [SerializeField] UISprite[] spTopStars;
    [SerializeField] GameObject[] spTopStarsAnimation;
    [SerializeField] GameObject topGuideRoot;

    [Header("TOP BAR")]
    [SerializeField] UILabel lbDifficult;
    [SerializeField] UILabel lbStageName;
    [SerializeField] UILabel lbClearTime;

    [Header("EXP BAR")]
    
    [SerializeField] UISlider sdPrevExp;
    [SerializeField] UISlider sdNowExp;
    [SerializeField] UILabel lbTotalExp;
    [SerializeField] UILabel lbAddedExp;
    [SerializeField] UILabel lbUserName;

    [Header("PLAYER UNITS")]
    [SerializeField] UIGrid grPlayerUnit;

    [Header("CONTENT")]
    [SerializeField] UITable grContents;

    [Header(" - MISSION")]
    [SerializeField] GameObject goMisstionRoot;
    [SerializeField] UISprite[] spMissionStars;
    [SerializeField] UILabel[] lbMissionDisc;

    [Header(" - OVER KILL")]
    [SerializeField] GameObject goOverkillRoot;
    [SerializeField] UILabel lbOKPara;
    [SerializeField] GameObject okFiled;
    [SerializeField] Transform tfOKRoot;

    [Header(" - ADDED ITEM")]
    [SerializeField] GameObject goAddedItemRoot;
    [SerializeField] UIGrid grAddedItem;
    [SerializeField] GameObject goNoItemGuide;

    [Header(" - ADDED MONEY")]
    [SerializeField] GameObject goAddedMoneyRoot;
    [SerializeField] UIGrid grMoney;
    [SerializeField] GameObject goGoldRoot;
    [SerializeField] GameObject goMileageRoot;
    [SerializeField] UILabel lbGold;
    [SerializeField] UILabel lbMileage;

    [Header(" - CHALLENGE")]
    [SerializeField] GameObject goChallengeRoot;
    [SerializeField] UILabel lbChallenge;

    [Header(" - LevelUp")]
    [SerializeField] GameObject goLevelUp;
    [SerializeField] GameObject goLevelUpExp;
    //[SerializeField] UISprite spNowExp;
    //[SerializeField] UISprite spPrevExp;
    [SerializeField] UISprite spLevelUp;
    [SerializeField] UILabel lbLevelUp;
    [SerializeField] GameObject goLevelUpEffect;

    [Header(" - Animator Controller")]                      // 0 - 승리 애니메이터 컨트롤러,   1- 패배 애니메이터 컨트롤러
    [SerializeField] RuntimeAnimatorController[] runtimeAnimatorControllers;


    int prevExp;
	int prevMaxExp;
	int prevLevel;

    internal int cachedGold;
    internal int cachedMileage;



	internal static ResultUI Create(Transform _parent)
	{
		var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Result/PanelResultUI", _parent);
		var result = go.GetComponent<ResultUI>();
		return result; 
	}



    internal override void Init(ResultBasePara _para)
    {
        base.Init(_para);

        cachedGold = GameCore.Instance.PlayerDataMgr.GetReousrceCount(ResourceType.Gold);
        cachedMileage = GameCore.Instance.PlayerDataMgr.GetReousrceCount(ResourceType.Mailage);

        SetClear(_para.clear);
        SetPlayTime(para.playTime);
        SetPlayerUnitByTeamIndex(_para);

        if (_para.type == InGameType.Story)      InitStory();
        else if(_para.type == InGameType.Daily)  InitDaily();

        InitExpBar();
        GameCore.Instance.DoWaitCall(() => {
            GetComponent<Animator>().runtimeAnimatorController = runtimeAnimatorControllers[_para.clear ? 0 : 1];
            GameCore.Instance.SoundMgr.SetCommonBattleSound(_para.clear ? SFX.Sfx_Result_Victory : SFX.Sfx_Result_Defeat);
            float length = GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length;
            StartCoroutine(GameCore.WaitForTime(length, () => { grAddedItem.gameObject.SetActive(true); }));
        });
    }

    public void SetStarSound(int pos)
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Result_Star);
    }

    internal override void UpdateData(params object[] _params)
    {
        UpdateExpBar();

        if(para.type == InGameType.Story)
            SetMission();
        else
            UpdateChallengeCount();
    }


    void InitStory()
    {
        topStarRoot.SetActive(true);
        topGuideRoot.SetActive(false);

        goMisstionRoot.SetActive(true);
        goOverkillRoot.SetActive(true);
        goChallengeRoot.SetActive(false);

        goMileageRoot.SetActive(true);

        goSelectBtn.SetActive(true);
        goRetryBtn.SetActive(true);
        goNextBtn.SetActive(true);
        goCheckOutBtn.SetActive(false);

        goLevelUp.SetActive(false);
        goLevelUpEffect.SetActive(false);

        grContents.enabled = true;
        grMoney.enabled = true;


        //top bar
        var pr = para.GetPara<ResultStoryPara>();
        var data = GameCore.Instance.DataMgr.GetStoryData(pr.storyKey);
        lbDifficult.text = StoryDataMap.GetStrDiffcult(data.difficult);
        lbStageName.text = string.Format("{0}-{1} [ {2} ]", data.chapter, data.stage, data.name);

        SetMission();
        SetOverKill(pr.ovkDmg, data.ovkValue, (int)ResourceType.Gold, data.ovkReward);

    }
    internal override void TurnOffNextButton()
    {
        Color colorGray = new Color(0.2f, 0.2f, 0.2f);
        goNextBtn.transform.GetChild(0).GetComponent<UILabel>().color = colorGray;
        goNextBtn.GetComponent<UISprite>().color = colorGray;
        goNextBtn.GetComponent<UIButton>().enabled = false;
    }
    void InitDaily()
    {
        topStarRoot.SetActive(false);
        topGuideRoot.SetActive(!para.clear);

        goMisstionRoot.SetActive(false);
        goOverkillRoot.SetActive(false);
        goChallengeRoot.SetActive(true);

        goMileageRoot.SetActive(false);

        goSelectBtn.SetActive(false);
        goRetryBtn.SetActive(false);
        goNextBtn.SetActive(false);
        goCheckOutBtn.SetActive(true);

        goLevelUp.SetActive(false);
        goLevelUpEffect.SetActive(false);

        grContents.enabled = true;
        grMoney.enabled = true;

        // Topbar
        var pr = para.GetPara<ResultDailyPara>();
        var data = GameCore.Instance.DataMgr.GetDailyDungeonData(pr.DDungeonKey);
        lbDifficult.text = StoryDataMap.GetStrDiffcult(data.difficult);
        lbStageName.text = string.Format("{0} 던전", data.name);
    }


    internal void SetClear(bool _clear)
	{
		if (_clear)		spClear.spriteName = "RESULT_01";
		else			spClear.spriteName = "RESULT_02";
		spClear.MakePixelPerfect();
	}

    void SetPlayTime(TimeSpan _time)
    {
        lbClearTime.text = string.Format("{0:00}분 {1:00}초", _time.Minutes, _time.Seconds);
    }

    internal void InitExpBar()
	{
		// !! 아직 플레이어 데이터엔 가산되지 않았다고 가정하고 작성됨

		var Player = GameCore.Instance.PlayerDataMgr;

		prevMaxExp = Player.MaxExp;
		prevExp = Player.Exp;
		prevLevel = Player.Level;

		lbTotalExp.text = string.Format("{0} [c]/ {1}", prevExp, prevMaxExp);
		lbUserName.text = string.Format("LV.{0} {1}", Player.Level, Player.Name);
		sdPrevExp.value = prevMaxExp != 0 ? (float)prevExp / prevMaxExp : 1f;
		sdNowExp.value = prevMaxExp != 0 ? (float)prevExp / prevMaxExp : 1f;
	}

    int cachedGap;
    void UpdateExpBar()
	{
		// 경험치 증가 애니메이션과 , 레벨업 처리 필요

		var Player = GameCore.Instance.PlayerDataMgr;

		bool isLevelUp = prevLevel < Player.Level;
        cachedGap = Player.Exp - prevExp;
		if (isLevelUp)
		{
            cachedGap = prevMaxExp - prevExp + Player.Exp;
            //prevExp = 0;
        }

        //StartCoroutine(CoExpAnimation(prevExp, gap + Player.MaxExp, prevMaxExp, Player.MaxExp, true));
        //Debug.Log("prevEXP:" + prevExp + ", Player.Exp:" + Player.Exp + ", Player.MaxExp:" + Player.MaxExp);
        StartCoroutine(CoExpAnimation(0, prevExp, cachedGap, prevMaxExp, Player.MaxExp, false));// isLevelUp));
	}

    public void UpdateChallengeCount()
    {
        lbChallenge.text = GameCore.Instance.PlayerDataMgr.GetItemByKey(CommonType.ITEMKEY_TICKET_DAILY).count.ToString("N0");
    }

	internal IEnumerator CoExpAnimation(float _acc, int _exp, int _add, int _prevMax, int _nowMaxp, bool _levelUp, bool _isFirst = false)
	{
		float now = _exp;				// 현재 경험치
		float acc = 0;          // 추가된 경험치량
		int tg = _exp + _add;   // 최종 경험치량
		int max = _prevMax;     // 현재 최대 경험치
        //Debug.Log("Result Guage Params : " + _acc + " " + _exp + " " + _add + " " + _prevMax + " " + _nowMaxp + " " + _levelUp + " " + _isFirst);

		while (now < tg)
		{
			now += Mathf.Min(max, _add * Time.deltaTime * 0.5f);
			acc += Mathf.Min(max, _add * Time.deltaTime * 0.5f);

            lbTotalExp.text = string.Format("{0} [c]/ {1}", (int)now, max);
            sdNowExp.value = max == 0 ? 1f : (float)now / max;
            lbAddedExp.text = string.Format("+EXP {0:N0}", (int)(acc + _acc));

            if ( max <= now)
			{
                if (_isFirst == false)
                {
                    var Player = GameCore.Instance.PlayerDataMgr;
                    //goLevelUp.SetActive(true);
                    //goLevelUpEffect.SetActive(true);
                    goLevelUpExp.SetActive(true);
                    StartCoroutine(ShowLevelUp((int)acc + _acc, 0, tg - _prevMax, _nowMaxp, 0, true, true));
                    yield break;
                }
                // level up
                max = _nowMaxp;
				now -= _prevMax;
				tg -= _prevMax;
			}

            yield return null;
		}

        if (max == 0)
        {
            lbTotalExp.text = string.Format("{0} [c]/ {1}", '-', '-');
            sdNowExp.value = 1f;
            sdPrevExp.value = 1f;
        }
        else
        {
            lbTotalExp.text = string.Format("{0} [c]/ {1}", (int)tg, max);
            sdNowExp.value = (float)now / max;
        }

        lbAddedExp.text = string.Format("+EXP {0:N0}", (int)cachedGap);
    }
   
    private IEnumerator ShowLevelUp(float _acc, int _exp, int _add, int _prevMax, int _nowMaxp, bool _levelUp, bool _isFirst, ParticleSystem particleSystem = null)
    {
        //TweenAlpha tweenAlphaSprite = spLevelUp.GetComponent<TweenAlpha>();
        //TweenAlpha tweenAlphaLabel = lbLevelUp.GetComponent<TweenAlpha>();
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_LevelUp);
        goLevelUp.SetActive(true);
        goLevelUpEffect.SetActive(true);

        float limitTime = (particleSystem != null) ? particleSystem.duration : 3f;
        //SetUIAlpha(tweenAlphaSprite, 0, 1, 1);
        yield return new WaitForSeconds(limitTime);

        sdPrevExp.value = 0;
        lbUserName.text = string.Format("LV.{0} {1}",
                                    GameCore.Instance.PlayerDataMgr.Level,
                                    GameCore.Instance.PlayerDataMgr.Name);

        lbLevelUp.gameObject.SetActive(true);
        //SetUIAlpha(tweenAlphaSprite, 1, 0, 1);
        //SetUIAlpha(tweenAlphaLabel, 0, 1, 1);
        StartCoroutine(CoExpAnimation(_acc, _exp, _add, _prevMax, _nowMaxp, _levelUp, _isFirst));

    }
    /*
    private void SetUIColor(TweenColor tweenColor, Color from, Color to, float durationTime)
    {
        tweenColor.duration = durationTime;
        tweenColor.from = from;
        tweenColor.to = to;
        tweenColor.ResetToBeginning();
        tweenColor.enabled = true;
    }
    */
    /*
    private void SetUIAlpha(TweenAlpha tweenUIAlpha, float from, float to, float durationTime)
    {
        tweenUIAlpha.from = from;
        tweenUIAlpha.to = to;
        tweenUIAlpha.ResetToBeginning();
        tweenUIAlpha.enabled = true;
    }
    */
    private bool CheckUnDead(ResultBasePara storyPara, HeroSData sdata)
    {
        var _data = GameCore.Instance.DataMgr.GetUnitData(sdata.key);
        for (int i = 0; i < storyPara.isDead.Count; i++)
        {
            int _id = storyPara.isDead[i];
            if (_id == _data.id)
            {
                storyPara.isDead.Remove(_id);
                return true;
            }
        }
        return false;
    }
	internal void SetPlayerUnitByTeamIndex(ResultBasePara _para)
	{
        var storyPara = _para.GetPara<ResultBasePara>();

        for (int i = 0; i < 6; i++)
		{
			var uid = GameCore.Instance.PlayerDataMgr.GetTeamIds(storyPara.playerTeamIdx, i);
			if (uid == -1)
				continue;

			var sdata = GameCore.Instance.PlayerDataMgr.GetUnitSData(uid);
			HeroCardBig heroCardBig = CardBase.CreateBigCard(sdata, grPlayerUnit.transform) as HeroCardBig;
            if (CheckUnDead(storyPara, sdata) == false) heroCardBig.SetDeadSprite();

        }

		grPlayerUnit.enabled = true;
        for(int i = 0; i < grPlayerUnit.transform.childCount; i ++)
        {
            grPlayerUnit.GetChild(i).GetChild(0).localPosition = Vector2.zero;
            grPlayerUnit.GetChild(i).name = "Slot00" + (i + 1).ToString();
        }
	}

	internal override void SetRewardItem(CardSData[] _sdatas)
	{
		int cnt = 0;
		int gold = 0;
		int mailage = 0;
		for(int i = 0; i < _sdatas.Length; ++i)
		{
			var key = _sdatas[i].key;
			if (key == (int)ResourceType.Gold)
			{
				gold += ((ItemSData)_sdatas[i]).count;
			}
			else if (key == (int)ResourceType.Mailage)
			{
				mailage = ((ItemSData)_sdatas[i]).count;
			}
			else
			{
                var reward = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Result/Reward", grAddedItem.transform);
                var sdata = _sdatas[i];
				var card = CardBase.CreateCard(sdata, null, false, reward.transform, null, (_uid) => GameCore.Instance.ShowCardInfo(sdata));
                card.name = "Item";
                card.transform.GetChild(0).localPosition = Vector2.zero;
                grAddedItem.enabled = true;
				++cnt;
			}
		}
        grAddedItem.gameObject.SetActive(false);
        goNoItemGuide.SetActive(cnt == 0);
        SetAddedMoney(gold, mailage);

        if (para.type == InGameType.Daily)
        {
            var dungeonData = GameCore.Instance.DataMgr.GetDailyDungeonData((para as ResultDailyPara).DDungeonKey);
            var rewardData = GameCore.Instance.DataMgr.GetDailyDungeonRewardData(dungeonData.rewardKey);
            if (rewardData.itemKey == (int)ResourceType.Gold)
            {
                goNoItemGuide.GetComponent<UILabel>().text = "골드 러쉬는 아이템과 경험치를\n획득 할 수 없습니다.";
            }
        }

	}

	internal void SetMission()
	{
        if (para.type != InGameType.Story)
            return;

		var player = GameCore.Instance.PlayerDataMgr;
        var pr = para.GetPara<ResultStoryPara>();
		bool[] _clear = player.GetStorySData(pr.storyKey).missionClear;
        var story = GameCore.Instance.DataMgr.GetStoryData(pr.storyKey);
		for ( int i = 0; i < spMissionStars.Length;++i)
		{
			lbMissionDisc[i].text = MissionDefineDataMap.GetMissionDiscString(story.missionId[i], story.missionValue1[i], story.missionValue2[i]);
			if (pr.clear & _clear[i])
			{
				spMissionStars[i].color = new Color32(0xFF, 0xEA, 0x00, 0xFF);
				spTopStars[i].spriteName = "RESULT_STAR_01";
                spTopStarsAnimation[i].SetActive(true);
                

            }
			else
			{
				spMissionStars[i].color = new Color32(0xB0, 0xB0, 0xB0, 0xFF);
				spTopStars[i].spriteName = "RESULT_STAR_02";
                spTopStarsAnimation[i].SetActive(false);
            }
		}
	}

	internal void SetOverKill(int _point, int _needPoint, int _itemKey, int _count)
	{
		var achieve = _point >= _needPoint;
		okFiled.SetActive(!achieve);
		lbOKPara.color = achieve ? new Color32(0x00, 0xF0, 0xFF, 0xFF) : new Color32(0x89, 0x89, 0x89, 0xFF);
		lbOKPara.text = string.Format("{0:N0} [c]/ {1:N0}", _point, _needPoint);

		var card = CardBase.CreateSmallCardByKey(_itemKey, tfOKRoot, null, (key) => GameCore.Instance.ShowCardInfoNotHave((int)key));
		card.SetEnable(achieve);
		if( 1 < _count )
			card.SetCount(_count);
	}

	internal void SetAddedMoney(int _gold, int _mileage)
	{
		lbGold.text = _gold.ToString("N0");
		//lbMileage.text = _mileage.ToString("N0");
		lbMileage.text = _mileage.ToString("N0");
	}


	public void OnClickNext()
	{
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

        // Todo : Get Next Stage ID & Prev Team Index
        var pr = para.GetPara<ResultStoryPara>();
        var data = GameCore.Instance.DataMgr.GetStoryData(pr.storyKey);
        
		var nextKey = StoryDataMap.GenerateNextStoryKey(data.chapter, data.difficult, data.stage);
		var nextData = GameCore.Instance.DataMgr.GetStoryData(nextKey);
		if (nextData != null)
		{
            //GameCore.Instance.ChangeSubSystem(SubSysType.Battle, new BattlePara()
            //{
            //	stageId = nextData.stageID,
            //	playerTeam = playerTeam
            //});
            GameCore.Instance.ChangeSubSystem(SubSysType.Story, new StoryPara(nextKey, true, 0, 1));
		}
		else
		{
			GameCore.Instance.ShowNotice("실패","더 이상 스테이지가 없습니다.", 0);
		}
		//GameCore.Instance.ChangeSubSystem(SubSysType.Battle, null);
	}

	public void OnClickretry()
	{
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

        // Todo : Get Prev Stage ID & Prev Team Index
        //var data = GameCore.Instance.DataMgr.GetStoryData(storyKey);
        //GameCore.Instance.ChangeSubSystem(SubSysType.Battle, new BattlePara(){
        //	stageId = data.stageID,
        //	playerTeam = playerTeam
        //});
        var pr = para.GetPara<ResultStoryPara>();
        GameCore.Instance.ChangeSubSystem(SubSysType.Story, new StoryPara(pr.storyKey, true, pr.playerTeamIdx, 0));
	}

	public void OnClickStageSelect()
	{
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);

		// Todo : Get Prev Stage ID ( Need Info : Chapter, Difficult)

		GameCore.Instance.ChangeSubSystem(SubSysType.Story, null);
	}

	public void OnClickReturnHome()
	{
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
		GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
	}

    public void OnClickCheckOutDailyDungeon()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        GameCore.Instance.ChangeSubSystem(SubSysType.DailyPrepare, null);
    }
}
