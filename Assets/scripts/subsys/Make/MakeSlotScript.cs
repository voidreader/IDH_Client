using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MakeSlotScript : MonoBehaviour
{
	internal enum MakeState
	{
		Block,		// 언락도 불가능한 상태
		Lock,			// 언락 가능한 상태
		None,			// 대기상태
		Making,		// 제조중
		Done			// 제조 완료
	}

	UISprite[] bgs;
	SliderScript[] sliders;
	GameObject sliderRoot;

	// 중앙 아이콘 관련
	UISprite spIconRoot;
	UITweener twIcon;
	UISprite spIconGauge;

	// Button 관련
	UIButton bt1;
	GameObject evinceBt1;
	UISprite spBtnIcon;
	UILabel lbBt1;
	UILabel lbEvinceBt1;

	UIButton bt2;
	GameObject evinceBt2;
    UILabel lbBt2;

    GameObject bt3; // 자원 최대 넣기

	// 제조중 알람 관련
	GameObject makingEvince;
	UILabel makingTimer;

	GameObject blockGuide;

    // 이펙트 관련
    [SerializeField] GameObject[] goCompleteEffects;
    [SerializeField] GameObject[] goMakingEffects;
    [SerializeField] GameObject[] goOpenEffects;


    MakeState state;
	int idx;			//고유 식별자 번호
	int itemType; // 아이템 타입( 1 영웅, 2 장비, 3 가구)
	internal bool isRental { get; private set; }

	DateTime MakeStartDate;
	TimeSpan makeDealy;
	DateTime rentalEndDate;

	Action<int, int> cbUnlock;
	Action<int, int> cbRental;
	Action<int, int, int[]> cbMake;
	Action<int> cbInstant;
	Action<int> cbDone;

	Func<int, int, MakeState, MakeState> cbChangeState;

	int[] cachedValues;

	internal static MakeSlotScript Create(Transform _parent)
	{
		var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Make/MakeSlotRoot", _parent);
		var result = go.GetComponent<MakeSlotScript>();
		result.InitLink();
		return result;
	}


	private void InitLink()
	{
		bgs = new UISprite[2] {
			UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "bg1"),
			UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "bg2")
		};

		sliderRoot = UnityCommonFunc.GetGameObjectByName(gameObject, "sliderRoot");
		sliders = new SliderScript[5];
		for (int i = 0; i < sliders.Length; i++)
		{
			sliders[i] = UnityCommonFunc.GetComponentByName<SliderScript>(sliderRoot, "type" + (i + 1));
			sliders[i].Init(i, CBCheckSliderValue);
		}

		spIconRoot = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "iconRoot");
		twIcon = UnityCommonFunc.GetComponentByName<UITweener>(gameObject, "iconAnim");
		spIconGauge = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "gauge");

		bt1 = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "bt1");
		evinceBt1 = UnityCommonFunc.GetGameObjectByName(gameObject, "bt1Alert");
		spBtnIcon = UnityCommonFunc.GetComponentByName<UISprite>(bt1.gameObject, "icon");
		lbBt1 = UnityCommonFunc.GetComponentByName<UILabel>(bt1.gameObject, "text");
		lbEvinceBt1 = UnityCommonFunc.GetComponentByName<UILabel>(evinceBt1.gameObject, "Label");

		bt2 = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "bt2");
		evinceBt2 = UnityCommonFunc.GetGameObjectByName(gameObject, "bt2Alert");
        lbBt2 = UnityCommonFunc.GetComponentByName<UILabel>(bt2.gameObject, "text");

        bt3 = UnityCommonFunc.GetGameObjectByName(gameObject, "bt3");

        makingEvince = UnityCommonFunc.GetGameObjectByName(gameObject,"makingBoard");
		makingTimer = UnityCommonFunc.GetComponentByName<UILabel>(makingEvince, "lbTimer");

		blockGuide = UnityCommonFunc.GetGameObjectByName(gameObject, "lbBlockGuide");
	}

	internal void Init(int _key, int[] _values = null)
	{
		idx = _key;
		cachedValues = _values;

		var sdata = GameCore.Instance.PlayerDataMgr.GetMakeData(idx);
		var data = GameCore.Instance.DataMgr.GetMakingSlotData(idx);
		if(data ==null)
		{
			Debug.LogError("Invalid Data");
			return;
		}

		itemType = data.type;

		for (int i = 0; i < sliders.Length; ++i)
			sliders[i].SetCount(cachedValues[i], true);

		if (sdata != null)
		{
			MakeStartDate = sdata.makeSTime;
			makeDealy = sdata.GetDelay();
			isRental = sdata.IsRental();
			if (isRental)
				rentalEndDate = sdata.period;

			SetState(GetState(sdata));
		}
		else
		{
			SetState(MakeState.Block);
		}
	}

	internal int GetDataKey()
	{
		return idx;
	}
	internal void SetCallbacks(Action<int, int> _cbUnlock, Action<int, int> _cbRental, Action<int, int, int[]> _cbMake, Action<int> _cbInstant, Action<int> _cbDone, Func<int, int, MakeState, MakeState> _cbChangeState)
	{
		cbUnlock = _cbUnlock;
		cbRental = _cbRental;
		cbMake = _cbMake;
		cbInstant = _cbInstant;
		cbDone = _cbDone;
		cbChangeState = _cbChangeState;
	}

	internal void CBCheckSliderValue(int _idx)
	{
		cachedValues[_idx] = sliders[_idx].count;
		var sdata = GameCore.Instance.PlayerDataMgr.GetItemByKey((int)ResourceType.Coin1 + _idx);
		if (sdata == null || sdata.count < sliders[_idx].count)
			sliders[_idx].label.color = Color.red;
		else
			sliders[_idx].label.color = Color.white;

	}

	internal void CheckSliderValueAll()
	{
		for (int i = 0; i < sliders.Length; ++i)
			CBCheckSliderValue(i);
	}

	private void Update()
	{
		if( state == MakeState.Making )
		{
			if (GameCore.nowTime < MakeStartDate + makeDealy)
			{
				var time = MakeStartDate + makeDealy - GameCore.nowTime;
				int hour = time.Hours + (time.Days * 24);
				makingTimer.text = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", hour, time.Minutes, time.Seconds, time.Milliseconds);
				spIconGauge.fillAmount = (1f - ((float)time.Ticks / makeDealy.Ticks));
			}
			else
			{
				SetState(MakeState.Done);
			}
		}
		
		if(state == MakeState.None)
		{
			if (isRental)
			{
				if (GameCore.nowTime < rentalEndDate)
				{
					var time = rentalEndDate - GameCore.nowTime;
					if (time.Days != 0)				lbEvinceBt1.text = string.Format("{0}일 남음", time.Days);
					else if(time.Hours != 0)	lbEvinceBt1.text = string.Format("{0:00}시{1:00}분{2:00}초 남음", time.Hours, time.Minutes, time.Seconds);
					else											lbEvinceBt1.text = string.Format("{0:00}분{1:00}초 남음", time.Minutes, time.Seconds, time.Milliseconds);
				}
				else
				{
					isRental = false;
					SetState(MakeState.Lock);
				}
			}
		}
	}


	internal MakeState GetState()	{	return state;	}
	internal int GetIndex()	{	return idx;	}
	internal int GetItemType()	{	return itemType;	}

	internal MakeState GetState(MakeSData _sdata)
	{
		MakeState nextState = MakeState.Block;
		if (_sdata != null)
		{
			switch (_sdata.state)
			{
				case 0: // Lock
					nextState = MakeState.Lock;
					break;
				case 1: // rentaled or making or making done
				case 2: // buyed or making or making done
					if (_sdata.IsMakeDone())
						nextState = MakeState.Done;
					else if (_sdata.IsMaking())
						nextState = MakeState.Making;
					else
						nextState = MakeState.None;
					break;
			}
		}
		return nextState;
	} 

	internal void SetState(MakeState _state, bool _force = false)
	{
        var prevState = state;
		//if (!_force && cbChangeState != null)
		//	state = cbChangeState(itemType, idx, _state);
		//else
			state = _state;

		switch (state)
		{
			case MakeState.Block:   SetStateBlock();	break;
			case MakeState.Lock:	SetStateLock();		break;
			case MakeState.None:	SetStateMakable();	break;
			case MakeState.Making:  SetStateMaking();	break;
			case MakeState.Done:	SetStateMakeDone();	break;
		}

        for(int i = 0; i < goCompleteEffects.Length; ++i)
            goCompleteEffects[i].SetActive(state == MakeState.Done);
        for (int i = 0; i < goMakingEffects.Length; ++i)
            goMakingEffects[i].SetActive(state == MakeState.Making);

        for (int i = 0; i < goOpenEffects.Length; ++i)
            goOpenEffects[i].SetActive(prevState == MakeState.Lock && state == MakeState.None);
    }


	internal  void SetStateBlock()
	{
		for(int i = 0; i < bgs.Length; ++i)
		{
			bgs[i].spriteName = "BOX";
			bgs[i].color = new Color32(0x00, 0x0F, 0x1F, 0x60);
		}

		sliderRoot.SetActive(false);
		makingEvince.SetActive(false);
		blockGuide.SetActive(true);

		spIconRoot.spriteName = "MAKING_ICON_00_01";
		spIconRoot.color = new Color(1f, 1f, 1f, 0.2f);
		twIcon.gameObject.SetActive(false);
		spIconGauge.gameObject.SetActive(false);

        bt3.SetActive(false);
		bt2.gameObject.SetActive(false);
		evinceBt2.SetActive(false);
		bt1.gameObject.SetActive(false);
		evinceBt1.SetActive(false);
	}
	internal void SetStateLock()
	{
		for (int i = 0; i < bgs.Length; ++i)
		{
			bgs[i].spriteName = "BOX";
			bgs[i].color = new Color32(0x00, 0x0F, 0x1F, 0x60);
		}

		sliderRoot.SetActive(false);
		makingEvince.SetActive(false);
		blockGuide.SetActive(false);

		spIconRoot.spriteName = "MAKING_ICON_00_01";
		spIconRoot.color = new Color(1f, 1f, 1f, 1f);
		twIcon.gameObject.SetActive(false);
		spIconGauge.gameObject.SetActive(false);

        bt3.SetActive(false);
        bt2.gameObject.SetActive(true);
		bt2.onClick.Clear();
		bt2.onClick.Add(new EventDelegate(() => { 
            if (!ClickFilter()) return;
            string popupText = "다음 재화를 사용하여 슬롯을 오픈 하시겠습니까?";
            GameCore.Instance.ShowAgree("슬롯 열기", popupText, lbBt2.text, MoneyType.Gold, 0, () => {
                GameCore.Instance.CloseMsgWindow();
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
                //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button); 
                cbRental(itemType, idx); 
            });
        }));
		evinceBt2.SetActive(true);
		bt2.GetComponentInChildren<UILabel>().text = GameCore.Instance.DataMgr.GetMakingSlotData(idx).rentalCost.ToString("N0");

		spBtnIcon.gameObject.SetActive(true);
		spBtnIcon.spriteName = "ICON_MONEY_03";
		spBtnIcon.width = 26;
		spBtnIcon.height = 26;
		evinceBt1.SetActive(true);
		lbEvinceBt1.text = "완전 개방";

		bt1.gameObject.SetActive(true);
		bt1.onClick.Clear();
		bt1.onClick.Add(new EventDelegate(() => { 
            if (!ClickFilter()) return; 
            string popupText = "다음 재화를 사용하여 슬롯을 오픈 하시겠습니까?";
            GameCore.Instance.ShowAgree("슬롯 열기", popupText, lbBt1.text, MoneyType.Pearl, 0, () => {
                GameCore.Instance.CloseMsgWindow();
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
                //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button); 
                cbUnlock(itemType, idx); 
            });
        }));
		bt1.normalSprite = "BTN_02_01_01";
		bt1.pressedSprite = "BTN_02_01_02";
		lbBt1.alignment = NGUIText.Alignment.Right;
		var value = GameCore.Instance.DataMgr.GetMakingSlotData(idx).unlockCost;
		lbBt1.text = string.Format("{0:N0}", value );
	}

	internal void SetStateMakable()
	{
		for (int i = 0; i < bgs.Length; ++i)
		{
			bgs[i].spriteName = "BOX";
			bgs[i].color = new Color32(0x00, 0x0F, 0x1F, 0xC0);
		}

		sliderRoot.SetActive(true);
		CheckSliderValueAll();

		makingEvince.SetActive(false);
		blockGuide.SetActive(false);

		spIconRoot.spriteName = "MAKING_ICON_00_02";
		spIconRoot.color = new Color(1f, 1f, 1f, 1f);
		twIcon.gameObject.SetActive(false);
		spIconGauge.gameObject.SetActive(false);

        bt3.SetActive(true);

        bt2.gameObject.SetActive(false);
		evinceBt2.SetActive(false);

		evinceBt1.SetActive(isRental);

		bt1.gameObject.SetActive(true);
		bt1.onClick.Clear();
		bt1.onClick.Add(new EventDelegate(() => { if (!ClickFilter()) return;
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            cbMake(itemType, idx, GetValues()); }));
		spBtnIcon.gameObject.SetActive(false);
		bt1.normalSprite = "BTN_01_01_01";
		bt1.pressedSprite = "BTN_01_01_02";
		lbBt1.alignment = NGUIText.Alignment.Center;
		lbBt1.text = "제조";
	}

	internal void SetStateMaking()
	{
		for (int i = 0; i < bgs.Length; ++i)
		{
			bgs[i].spriteName = "BOX_GREEN";
			bgs[i].color = Color.white;
		}

		sliderRoot.SetActive(false);
		makingEvince.SetActive(true);
		blockGuide.SetActive(false);

		spIconRoot.spriteName = "MAKING_ICON_00_00";
		spIconRoot.color = new Color(1f, 1f, 1f, 1f);
		twIcon.gameObject.SetActive(true);
		twIcon.enabled = true;
		twIcon.ResetToBeginning();
		twIcon.PlayForward();
		spIconGauge.gameObject.SetActive(true);
		spIconGauge.fillAmount = 0.5635f;

        bt3.SetActive(false);

        bt2.gameObject.SetActive(false);
		evinceBt2.SetActive(false);

		spBtnIcon.gameObject.SetActive(true);
		spBtnIcon.spriteName = (itemType == 1) ? "TICKET_01" : "TICKET_02"; // 영웅:01, 장비 및 가구:02
		spBtnIcon.width = 44;
		spBtnIcon.height = 24;
		evinceBt1.SetActive(false);

		bt1.gameObject.SetActive(true);
		bt1.onClick.Clear();
		bt1.onClick.Add(new EventDelegate(() => {
            if (!ClickFilter()) return;
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
            string popupText = "다음 재화를 사용하여 제조를 완료 하시겠습니까?";
            if (itemType == 1)
            {
                GameCore.Instance.ShowAgree("빠른 제조", popupText, " 1 ", MoneyType.HeroTicket, 0, () => { GameCore.Instance.CloseMsgWindow(); cbInstant(idx); });
            }
            else
            {
                GameCore.Instance.ShowAgree("빠른 제조", popupText, " 1 ", MoneyType.ItemTicket, 0, () => { GameCore.Instance.CloseMsgWindow(); cbInstant(idx); });
            }
        }));
		bt1.normalSprite = "BTN_04_01_01";
		bt1.pressedSprite = "BTN_04_01_02";
		lbBt1.alignment = NGUIText.Alignment.Right;
		lbBt1.text = "즉시완료";
	}


	internal void SetStateMakeDone()
	{
		for (int i = 0; i < bgs.Length; ++i)
		{
			bgs[i].spriteName = "BOX_BLUE";
			bgs[i].color = Color.white;
		}

		sliderRoot.SetActive(false);
		makingEvince.SetActive(false);
		blockGuide.SetActive(false);

		spIconRoot.spriteName = "MAKING_ICON_00_00";
		spIconRoot.color = new Color(1f, 1f, 1f, 1f);
		twIcon.gameObject.SetActive(true);
		twIcon.enabled = false;
		twIcon.transform.localRotation = Quaternion.identity;
		spIconGauge.gameObject.SetActive(true);
		spIconGauge.fillAmount = 1f;

        bt3.SetActive(false);

        bt2.gameObject.SetActive(false);
		evinceBt2.SetActive(false);
		evinceBt1.SetActive(false);

		bt1.gameObject.SetActive(true);
		spBtnIcon.gameObject.SetActive(false);
		bt1.onClick.Clear();
		bt1.onClick.Add(new EventDelegate(() => { if (!ClickFilter()) return;
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button); 
            cbDone(idx); 
        }));
		bt1.normalSprite = "BTN_03_01_01";
		bt1.pressedSprite = "BTN_03_01_02";
		lbBt1.alignment = NGUIText.Alignment.Center;
		lbBt1.text = "완료";
	}


	float btnTimer = 0f;
	bool ClickFilter()
	{
		if (Time.time < btnTimer)
		{
#if UNITY_EDITOR
            Debug.LogError("Filtered");
#endif
            return false;
		}
		btnTimer = Time.time + 0.8f;

		return true;
	}


	int[] GetValues()
	{
		int[] values = new int[5];
		for (int i = 0; i < values.Length; ++i)
			values[i] = sliders[i].GetCount();

		return values;
	}


    public void OnClickMaxCoins()
    {
        for(int i = 0; i< sliders.Length; ++i)
        {
            var sdata = GameCore.Instance.PlayerDataMgr.GetItemByKey((int)ResourceType.Coin1 + i);
            if (sdata != null)
                sliders[i].SetCount(sdata.count);
            else
                sliders[i].SetCount(0);
        }
    }
}
