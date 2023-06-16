using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrepareUI : MonoBehaviour, ISequenceTransform
{
	[SerializeField] GameObject _touchGuard;

	// left 
	TeamSlotMgr slot;
	
	// NOTE : 외부 수정이 잠겨있는 변수 : 인스팩터창 수정 불가능
	UISprite[] _team;

	[SerializeField] UISprite _teamSkillIcon;
	[SerializeField] UILabel _teamSkillNameLabel;
	[SerializeField] UILabel _teamSkillDiscLabel;
	[SerializeField] GameObject _teamSkillEffectLoop;

	[SerializeField] UILabel _recmdPowerLabel;
	[SerializeField] UILabel _teamPowerLabel;

	// right
	[SerializeField] UISprite[] _missionStars;
	[SerializeField] UILabel[] _missionDiscLabel;
	[SerializeField] UILabel _difficultLabel;

	[SerializeField] UIGrid _rewardGrid;
	
	[SerializeField] Transform _okItemRoot;
	[SerializeField] UILabel _okValueLabel;
	CardBase[] _rewardItems;
	CardBase _okItem;
	
	[SerializeField] FriendSelectComponent _friendList;

	// bottom
	[SerializeField] UIButton _startButton;
	[SerializeField] UILabel _needVigorLabel;
	[SerializeField] UIButton _editTeamButton;
	[SerializeField] UILabel _deductLabel;
	
	// NOTE : 코드 진행중 형변환이 존재하여 인스팩터 참조할 시 에러가 날 가능성이있음.
	UITweener[] _deductsTweener;

	internal int storyKey;
	internal static int selectedTeamIndex = 0;
	//long selectedFriedUID = -1;

	internal static PrepareUI Create()
	{
		var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Story/PanelPrepareUI", GameCore.Instance.ui_root);
		var result = go.GetComponent<PrepareUI>();
		result.InitLink();
		return result;
	}

	private void InitLink()
	{		
		// left
		var slotRoot = UnityCommonFunc.GetGameObjectByName(gameObject, "slot_Root");
		slot = new TeamSlotMgr(slotRoot, null, null, null, null, null);

		_team = new UISprite[5];
		for (int i = 0; i < _team.Length; i++)
		{
			_team[i] = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "teamBtn" + (i + 1));
			int n = i;
			_team[i].GetComponent<UIButton>().onClick.Add(new EventDelegate(() => {
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
                //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
                OnChangeTeam(n); }));
		}

        #region [5Star]에서 외부로부터 데이터를 받아오던 형식 : 현재는 변경된 사항
        // [NOTE] : 변수명 리팩토링 중 외부에서 어떠한 데이터를 참조해야하는지 모를때 참고할 것.

        //_touchGuard = UnityCommonFunc.GetGameObjectByName(gameObject, "TouchGuard");
        //_teamSkillIcon = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "TeamSkill_Icon");
        //_teamSkillNameLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "teamskill_Name");
        //_teamSkillDiscLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "teamSkill_Disc");

        //_recmdPowerLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbRecmdPower");
        //_teamPowerLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbTeamPower");

        //_teamSkillEffectLoop = UnityCommonFunc.GetGameObjectByName(_teamSkillIcon.gameObject, "Eff_Equip_Loop");

        //// right

        //_difficultLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbDiffcult");
        //_missionStars = new UISprite[3];
        //_missionDiscLabel = new UILabel[3];
        //for (int i = 0; i < 3; ++i)
        //{
        //	_missionStars[i] = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "spStar" + (i + 1));
        //	_missionDiscLabel[i] = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbMission" + (i + 1));
        //}

        //_rewardGrid = UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "gridRewardList");
        //_okItemRoot = UnityCommonFunc.GetComponentByName<Transform>(gameObject, "okItemRoot");
        //_okValueLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbOvkValue");

        //_friendList = UnityCommonFunc.GetComponentByName<FriendSelectComponent>(gameObject, "friendRoot");

        //// bottom
        //_startButton = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnStart");
        //_needVigorLabel = UnityCommonFunc.GetComponentByName<UILabel>(_startButton.gameObject, "lbNeedPoint");
        //_editTeamButton = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnTeamEdit");
        //_deductLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbDeduct");

        #endregion

        _deductsTweener = _deductLabel.GetComponents<UITweener>();

		_startButton.onClick.Add(new EventDelegate(() => {
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            OnClickStart(); }));
		_editTeamButton.onClick.Add(new EventDelegate(()=> {
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button); 
            OnClickEditTeam(); }));

		SetActiveTouchGuard(false);
	}

	internal void Init(int _storyKey)
	{
        storyKey = _storyKey;
		var data = GameCore.Instance.DataMgr.GetStoryData(storyKey);
		var sdata = GameCore.Instance.PlayerDataMgr.GetStorySData(storyKey);
		var chapterData = GameCore.Instance.DataMgr.GetStoryChapterData(StoryChapterDataMap.GenerateChapterKey(data.chapter, data.difficult));
		var rewardData = GameCore.Instance.DataMgr.GetStoryRewardData(data.rewardId);

		_recmdPowerLabel.text = data.powerRecommand.ToString("N0");
		SetReward(rewardData.rewardID, rewardData.rewardValue, (int)ResourceType.Gold, data.ovkReward);
		_okValueLabel.text = string.Format("목표 {0:N0}", data.ovkValue);
		_difficultLabel.text = string.Format("({0})", StoryDataMap.GetStrDiffcult(data.difficult));
		for (int i = 0; i < 3; ++i)
			SetMissionInfo(i, sdata.missionClear[i], storyKey);


		//FriendData[] frdData = new FriendData[] {
		//	new FriendData() { uid = 1, name = "하하하1", lv = 21, typicalKey = 1100001, power= 230, skillKey = 4300001 },
		//	new FriendData() { uid = 2, name = "하하하3", lv = 21, typicalKey = 1100002, power= 2230, skillKey = 4300002 },
		//	new FriendData() { uid = 3, name = "하하하4", lv = 21, typicalKey = 1100003, power= 3230, skillKey = 4300003 },
		//	new FriendData() { uid = 4, name = "하하하5", lv = 21, typicalKey = 1100004, power= 2430, skillKey = 4300004 },
		//	new FriendData() { uid = 5, name = "하하하6", lv = 21, typicalKey = 1100005, power= 28730, skillKey = 4300005 },
		//};

		//SetFriend(frdData, null);

		_needVigorLabel.text = chapterData.stamina.ToString(); //GetNeedVigor().ToString();


        OnChangeTeam(selectedTeamIndex);
	}

	internal void Destroy()
	{
		//camFriendList.enabled = false;
		//camFriendList.GetComponent<UICamera>().enabled = false;
		GameObject.Destroy(gameObject);
	}

	internal void OnChangeTeam(int _idx)
	{
		for (int i = 0; i < 6; i++)
			slot.RemoveCharacter(i);
		
		selectedTeamIndex = _idx;
		for (int i = 0; i < 6; i++)
		{
			var uid = GameCore.Instance.PlayerDataMgr.GetTeamIds(_idx, i);
			if (uid != -1)
				slot.SetCharacter(i, uid, true);
		}

		for(int i = 0; i < _team.Length; ++i)
			_team[i].spriteName = (_idx != i) ? "BTN_05_01_01" : "BTN_05_01_02";

		if (slot.Count == 0)
		{
			OnChangeTeam(0);
			return;
		}

		// 전투력 및 팀 스킬
		var tsKey = GameCore.Instance.PlayerDataMgr.GetTeamSkillKey(_idx);
		ChangeTeamSkill(tsKey);
		CalcPower();
	}

	internal void SetMissionInfo(int _num, bool _isClear, int _storyKey)
	{
		if( _num < 0  || 3 <= _num )
		{
			Debug.LogError("Out of Range Mission Number. " + _num);
			return;
		}

		if (!_isClear)		_missionStars[_num].color = new Color32(0x89, 0x89, 0x89, 0xFF);
		else				_missionStars[_num].color = new Color32(0xFF, 0xEA, 0x00, 0xFF);


        var story = GameCore.Instance.DataMgr.GetStoryData(_storyKey);
        _missionDiscLabel[_num].text = MissionDefineDataMap.GetMissionDiscString(story.missionId[_num], story.missionValue1[_num], story.missionValue2[_num]);
    }

	internal void SetReward(int[] _itemKeys, int[] _itemCounts, int _ovItemKey, int _ovkCount)
	{
        // Delete Prev card
        if (_rewardItems != null)
            for (int i = 0; i < _rewardItems.Length; ++i)
                if (_rewardItems[i] != null)
                {
                    _rewardItems[i].transform.parent = GameCore.Instance.Ui_root;
                    Destroy(_rewardItems[i].gameObject);
                }

		if (_okItem != null)
			Destroy(_okItem.gameObject);

		// Set Reward card
		_rewardItems = new CardBase[_itemKeys.Length];
		for (int i = 0; i < _itemKeys.Length; i++)
		{
            if(_itemKeys[i] != -1){
                _rewardItems[i] = CardBase.CreateSmallCardByKey(_itemKeys[i], _rewardGrid.transform, null, OnPressRewardItem);
                if (_itemCounts != null) _rewardItems[i].SetCount(_itemCounts[i]);
            }
		}
		_rewardGrid.enabled = true;

		// Set over kill card
		_okItem = CardBase.CreateSmallCardByKey(_ovItemKey, _okItemRoot, null, OnPressRewardItem);
		_okItem.SetCount(_ovkCount);
	}


	internal void OnPressRewardItem(long _uid)
	{
		GameCore.Instance.ShowCardInfoNotHave((int)_uid);
	}

	internal void SetActiveTouchGuard(bool _active)
	{
		_touchGuard.SetActive(_active);
	}

	internal void OnClickStart()
	{
		Debug.Log("Hero : " + GameCore.Instance.PlayerDataMgr.HeroCount + " / " + GameCore.Instance.PlayerDataMgr.HeroSlotLimitCount);
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        if (GameCore.Instance.PlayerDataMgr.HeroCount >= GameCore.Instance.PlayerDataMgr.HeroSlotLimitCount)
        {
            GameCore.Instance.ShowAgree("영웅 슬롯 부족", "영웅 슬롯이 가득 찼습니다.\n영웅을 획득할 수 없습니다.\n 계속 하시겠습니까 ?", 0, () => CheckEquipItem());
            //GameCore.Instance.ShowNotice("영웅 슬롯 부족", "영웅 슬롯이 가득 찼습니다.\n영웅을 획득할 수 없습니다.\n 계속 하시겠습니까 ?", 0);
			return;
		}
        CheckEquipItem();
    }

    private void CheckEquipItem()
    {
        GameCore.Instance.CloseMsgWindow();
        Debug.Log("Equip Item : " + GameCore.Instance.PlayerDataMgr.GetEquipItemCount() + " / " + GameCore.Instance.PlayerDataMgr.EquipItemSlotLimitCount);
        if (GameCore.Instance.PlayerDataMgr.GetEquipItemCount() >= GameCore.Instance.PlayerDataMgr.EquipItemSlotLimitCount)
        {
            GameCore.Instance.ShowAgree("장비 슬롯 부족", "장비 슬롯이 가득 찼습니다.\n장비를 획득할 수 없습니다.\n 계속 하시겠습니까 ?", 0, () => StartStoryChapter());
            return;
        }
        StartStoryChapter();
    }

    private void StartStoryChapter()
    {
        for (int i = 0; i < 6; ++i)
            if (GameCore.Instance.PlayerDataMgr.GetTeamIds(selectedTeamIndex, i) != -1)
            {
                var data = GameCore.Instance.DataMgr.GetStoryData(storyKey);
                //GameCore에 chapter와 stage정보를 저장하여 게임을 끄기 전까진 해당 chapter, stage로 이동하도록 하기 위한 함수
                GameCore.SetStageInfo(data.difficult, data.chapter, data.stage);
                var chapterData = GameCore.Instance.DataMgr.GetStoryChapterData(StoryChapterDataMap.GenerateChapterKey(data.chapter, data.difficult));
                PlayDeduct(chapterData.stamina);

                StartCoroutine(CoDelayStart(storyKey, selectedTeamIndex + 1));

                //GameCore.Instance.ChangeSubSystem(SubSysType.Battle, new BattlePara() { playerTeam = selectedTeamIndex, stageId = stageKey });
                SetActiveTouchGuard(true);
                GameCore.Instance.CloseMsgWindow();
                return;
            }

        GameCore.Instance.ShowAlert("팀이 비어 있습니다.");
    }

	bool bWaitStart = false;
	IEnumerator CoDelayStart(int _storyKey, int _selectTeam)
	{
		if (bWaitStart)
			yield break;

		bWaitStart = true;

		yield return new WaitForSeconds(0.8f);
        //var tutoOpenFriend = GameCore.Instance.lobbyTutorial != null && GameCore.Instance.lobbyTutorial.IsRunning && GameCore.Instance.lobbyTutorial.tutorialPos == 5;

        if (_friendList.SelectFriendItem == null)
            GameCore.Instance.NetMgr.Req_Story_Start(_storyKey, _selectTeam, -1);
        else
            GameCore.Instance.NetMgr.Req_Story_Start(_storyKey, _selectTeam, _friendList.SelectFriendItem.GetUID());
		bWaitStart = false;
	}

	private void PlayDeduct(int _value)
	{
        // 핫타임 버프 적용
        var datas = GameCore.Instance.PlayerDataMgr.GetHotTimeDatas();
        int value = 0;
        foreach (var data in datas)
        {
            if (data.type == 3)
            {
                value = data.value;
                break;
            }
        }
        _value -= (int)(_value * (value / 100f));

		_deductLabel.text = "-" + _value; // "-" + GetNeedVigor();
        for (int i = 0; i < _deductsTweener.Length; i++)
		{
			var tw = _deductsTweener[i];
			tw.ResetToBeginning();
			tw.PlayForward();
		}
	}

    public int GetNeedVigor()
    {
        // 핫타임 버프 적용
        var hot_datas = GameCore.Instance.PlayerDataMgr.GetHotTimeDatas();
        int value = 0;
        foreach (var hot_data in hot_datas)
        {
            if (hot_data.type == 3)
            {
                value = hot_data.value;
                break;
            }
        }

        var data = GameCore.Instance.DataMgr.GetStoryData(storyKey);
        var chapterData = GameCore.Instance.DataMgr.GetStoryChapterData(StoryChapterDataMap.GenerateChapterKey(data.chapter, data.difficult));
        var vigor = chapterData.stamina;

        return vigor - (int)(vigor * (value / 100f));
    }

    internal void OnClickEditTeam()
    {
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        GameCore.Instance.ChangeSubSystem(SubSysType.EditTeam, new ReturnSysPara(SubSysType.Story, selectedTeamIndex, new StoryPara(storyKey, true, selectedTeamIndex)));
	}


	internal void SetFriend(FriendSData[] _myFrdData, FriendSData[] _etcFrdData)
    {
        _friendList.SetFriend(_myFrdData, _etcFrdData);
	}

	internal void ChangeTeamSkill(int _key)
    {
        slot.SetAllTeamSkillIcon(false);

		TeamSkillDataMap data = null;
		if( _key > 0)
			data = GameCore.Instance.DataMgr.GetTeamSkillData(_key);
		
		if(data == null)
		{
			GameCore.Instance.SetUISprite(_teamSkillIcon, 4370001);
			_teamSkillNameLabel.text = "스킬없음";
			_teamSkillDiscLabel.text = "";
            _teamSkillEffectLoop.SetActive(false);

        }
		else
		{
			GameCore.Instance.SetUISprite(_teamSkillIcon, data.imageID);
			_teamSkillNameLabel.text = data.name;
			_teamSkillDiscLabel.text = data.disc;


			for (int i = 0; i < data.needChar.Length; i++)
			{
				var slotIdx = slot.GetSlotIndexByCharIDType(data.needChar[i]);
				if (slotIdx < 0)
				{
					Debug.LogError("장착 불가능한 팀스킬!!");
					slot.SetAllTeamSkillIcon(false);
					return;
				}
				slot.SetTeamSkillIcon(slotIdx, true);
                _teamSkillEffectLoop.SetActive(true);

            }
		}
	}

	internal void CalcPower()
	{
		int power = 0;
		for(int i = 0; i < slot.Count; ++i)
		{
			var uid = slot.GetID(i);
			var unit = GameCore.Instance.PlayerDataMgr.GetUnitSData(uid);
			if (unit == null)
				continue;

			power += unit.GetPower();
		}

		_teamPowerLabel.text = power.ToString("N0");
        if (GameCore.Instance.DataMgr.GetStoryData(storyKey).powerRecommand < power)
            _teamPowerLabel.color = new Color32(0x24, 0xFF, 0x00, 0xFF);
        else
            _teamPowerLabel.color = Color.red;
	}
    public List<ReturnTutorialData> GetTutorialTransformList(int tutorialNum)
    {
        List<ReturnTutorialData> nTutorialList = new List<ReturnTutorialData>();
        switch (tutorialNum)
        {
            case 1:
                nTutorialList.Add(new ReturnTutorialData(_startButton.transform, 0));
                break;
            case 4:
                nTutorialList.Add(new ReturnTutorialData(_startButton.transform, 0));
                break;
            case 5:
                nTutorialList.Add(new ReturnTutorialData(_startButton.transform, 0));
                break;
            default:
                break;
        }
        return nTutorialList;
    }
}
