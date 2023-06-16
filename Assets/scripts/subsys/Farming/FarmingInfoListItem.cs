using System;
using System.Collections.Generic;
using UnityEngine;


internal class FarmingInfoListItem : MonoBehaviour
{
	internal enum State
	{
		Unlock,
		None,
		Active_NotUseQuick,
		Active_UsedQuick,
		Done,
	}

	GameObject runningLabel;
	UILabel lbName;
	UILabel lbCondition;
	UILabel lbTime;
	UILabel lbPower;
	UISprite spPowerIcon;

	GameObject btnGo1;
	GameObject btnGo2;

	UIButton btn1;
	UIButton btn2;

	UISprite btnSp1;
	UISprite btnSp2;

	UILabel btnLb1;
	UILabel btnLb2;

	UIGrid grid;

	FarmingDataMap data;

	Action<int> cbTeam;
	Action<int> cbDone;
	Action<int> cbCansel;

	State state;

	GameObject blind;

    GameObject fliper;

    DateTime endTime; // 파밍하고 있을때만 사용됨. 종료되는 시간.

	internal FarmingDataMap Data
	{
		get { return data; }
	}

    internal Transform GetTransformButton1()
    {
        return btn1.transform;
    }

	internal static FarmingInfoListItem Create(Transform _parent)
	{
		var result = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Farming/farmingListItem", _parent).GetComponent<FarmingInfoListItem>();
		result.InitLink();
		return result;
	}

	private void InitLink()
	{
		runningLabel = UnityCommonFunc.GetGameObjectByName(gameObject, "spRunning");
		lbName = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "name");
		lbCondition = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "condition");
		lbTime = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "time");
		lbPower = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "power");
		spPowerIcon = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "power_icon");
		grid = UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "grid");

		btn1 = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btn1");
		btnSp1 = btn1.GetComponent<UISprite>();
		btnLb1 = btn1.GetComponentInChildren<UILabel>();
		btnGo1 = btn1.gameObject;
		btn2 = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btn2");
		btnSp2 = btn2.GetComponent<UISprite>();
		btnLb2 = btn2.GetComponentInChildren<UILabel>();
		btnGo2 = btn2.gameObject;

		blind = UnityCommonFunc.GetGameObjectByName(gameObject, "blind");

        fliper = UnityCommonFunc.GetGameObjectByName(gameObject, "fliper");
        fliper.SetActive(false);
    }

	internal void Init(FarmingDataMap _data, State _state, DateTime _time, Action<int> _cbClickTeam, Action<int> _cbClickDone, Action<int> _cbClickCansel)
	{
		data = _data;
		cbTeam = _cbClickTeam;
		cbDone = _cbClickDone;
		cbCansel = _cbClickCansel;

		SetState(_state);

		endTime = _time;
		lbName.text = string.Format("{0}.{1}", data.stage, data.name);

		lbCondition.text = FarmingDefineMap.GetFarmingDescString(data);// data.condStringID;

		UpdateTime();

		SetPowerCondition(data.powerCondition);
		SetRewardItem(_data.rewardID);

		btn1.onClick.Clear();
		btn2.onClick.Clear();
		btn1.onClick.Add(new EventDelegate(() => OnClickBtn1(data.id)));
		btn2.onClick.Add(new EventDelegate(() => OnClickBtn2(data.id)));

		var tw = GetComponent<UITweener>();
		tw.ResetToBeginning();
		tw.PlayForward();
	}

	internal void SetPowerCondition(int _power)
	{
		if (_power != 0)
		{
			lbPower.text = _power.ToString("N0");
			spPowerIcon.enabled = true;
		}
		else
		{
			lbPower.text = "";
			spPowerIcon.enabled = false;
		}
	}

	internal void SetRewardItem(int _key)
	{
		while( 0 < grid.transform.childCount )
		{
			var tf = grid.transform.GetChild(0).transform;
			tf.parent = null;
			Destroy(tf.gameObject);
		}

		var reward = GameCore.Instance.DataMgr.GetFarmingRewardData(_key);
		for (int i = 0; i < reward.coin.Length; i++)
		{
			if (reward.coin[i] == 0)
				continue;

			var card = CardBase.CreateSmallCardByKey((int)ResourceType.Coin1 + i, grid.transform, null, (key) => GameCore.Instance.ShowCardInfoNotHave((int)key));
			card.SetCount(reward.coin[i]);
		}

		grid.enabled = true;
	}

	internal void SetState(State _state)
	{
		state = _state;

        fliper.SetActive(state == State.None);

        switch (state) // b 02 p 01 g 03 gr 05
		{
			case State.Unlock:
				btnGo2.SetActive(false);
				blind.SetActive(true);
				runningLabel.SetActive(false);

				btn1.normalSprite = "BTN_01_01_01";
				btn1.pressedSprite = "BTN_01_01_02";
				btnLb1.text = "팀 선택";

				lbTime.color = new Color32(0x2F, 0x32, 0x35, 0xFF);

				btn1.enabled = false;
				btnSp1.alpha = 0.2f;
				btn2.enabled = false;
				btnSp2.alpha = 0.2f;
				break;

			case State.None:
				btnGo2.SetActive(false);
				blind.SetActive(false);
				runningLabel.SetActive(false);

				btn1.normalSprite = "BTN_01_01_01";
				btn1.pressedSprite = "BTN_01_01_02";
				btnLb1.text = "팀 선택";

				lbTime.color = new Color32(0x2F, 0x32, 0x35, 0xFF);

				btn1.enabled = true;
				btnSp1.alpha = 1f;
				btn2.enabled = false;
				btnSp2.alpha = 1f;
				break;

			case State.Active_NotUseQuick:
				btnGo2.SetActive(true);
				blind.SetActive(false);
				runningLabel.SetActive(true);

                btn1.normalSprite = CommonType.BTN_5_NORMAL; // "BTN_05_01_01";
				btn1.pressedSprite = CommonType.BTN_5_ACTIVE; // "BTN_05_01_02";
				btnLb1.text = "취소";

                btn2.normalSprite = CommonType.BTN_3_NORMAL;
                btn2.pressedSprite = CommonType.BTN_3_ACTIVE;
                btnLb2.text = "팀 보기";

                lbTime.color = new Color32(0x00, 0x76, 0xC0, 0xFF);

				btn1.enabled = true;
				btnSp1.alpha = 1f;
                btn2.enabled = true;
                btnSp2.alpha = 1f;
                break;

			case State.Active_UsedQuick:
				btnGo2.SetActive(true);
				blind.SetActive(false);
				runningLabel.SetActive(true);
                btn1.normalSprite = CommonType.BTN_5_NORMAL; // "BTN_05_01_01";
                btn1.pressedSprite = CommonType.BTN_5_ACTIVE; // "BTN_05_01_02";
                btnLb1.text = "취소";

                btn2.normalSprite = CommonType.BTN_3_NORMAL;
                btn2.pressedSprite = CommonType.BTN_3_ACTIVE;
                btnLb2.text = "팀 보기";

                lbTime.color = new Color32(0x00, 0x76, 0xC0, 0xFF);

				btn1.enabled = true;
				btnSp1.alpha = 1f;
                btn2.enabled = true;
                btnSp2.alpha = 1f;
                break;

			case State.Done:
				btnGo2.SetActive(false);
				blind.SetActive(false);
				runningLabel.SetActive(false);

				btn1.normalSprite = "BTN_02_01_01";
				btn1.pressedSprite = "BTN_02_01_02";
				btnLb1.text = "자원 받기";

				lbTime.color = new Color32(0x00,0x76,0xC0,0xFF);

				btn1.enabled = true;
				btnSp1.alpha = 1f;
				break;
		}
	}

	internal void UpdateTime()
	{
		TimeSpan time;
		if (state == State.Active_NotUseQuick || state == State.Active_UsedQuick)
		{
			time = endTime - GameCore.nowTime;
			if (time <= TimeSpan.Zero)
				SetState(State.Done);
		}
		else if (state == State.Done)
			time = new TimeSpan();
		else
			time = data.time;
		
		lbTime.text = string.Format("{0:00}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);
	}

	float btnTimer = 0f;
	internal void OnClickBtn1(int _id)
	{
		if (Time.time < btnTimer)
		{
			Debug.LogError("Filtered");
			return;
		}
		btnTimer = Time.time + 0.8f;

        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
		switch (state) // b 02 p 01 g 03 gr 05
		{
			case State.Unlock:
				return;
			case State.None:
				// Show Team
				if (cbTeam != null)
					cbTeam(data.id);
				return;
			case State.Active_UsedQuick:
			case State.Active_NotUseQuick:
				// cansel
				if (cbCansel != null)
				cbCansel(data.id);
				return;
			case State.Done:
				// Take Resource
				if (cbDone != null)
					cbDone(data.id);
				return;
		}
	}

	float btnTimer2;
	internal void OnClickBtn2(int _id)
	{
		if (Time.time < btnTimer2)
		{
			Debug.LogError("Filtered");
			return;
		}
		btnTimer2 = Time.time + 0.8f;

        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
		if (State.Active_NotUseQuick == state)
		{
            //// 시간 단축
            GameCore.Instance.ShowAlert("시간 단축 미구현");
            SetState(State.Active_UsedQuick);
        }	
		else
		{
			//GameCore.Instance.ShowAlert("더이상 시간 단축을 할 수 없습니다.");

            // Show FarmingTeamList
            var heroList = GameCore.Instance.PlayerDataMgr.GetUnitsByFarmingKey(Data.id);
            List<PvPOppUnitSData> list = new List<PvPOppUnitSData>();
            foreach (var hero in heroList)
            {
                var data = new PvPOppUnitSData();
                data.charID = hero.key;
                data.skill = 0;
                data.enchant = hero.enchant;
                list.Add(data);
            }

            var teamInfo = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(CSTR.RSC_TeamInfo, transform);
            teamInfo.GetComponent<PvPTeamInfoComponent>().SetData(list, GameCore.Instance.PlayerDataMgr.Name);
            GameCore.Instance.ShowObject(string.Empty, null, teamInfo, 4, new MsgAlertBtnData[] {
                    new MsgAlertBtnData(CSTR.TEXT_Accept, new EventDelegate(() => GameCore.Instance.CloseMsgWindow()))
                });
            var widget = teamInfo.GetComponent<UIWidget>();
            if (widget != null) widget.alpha = 0f;
        }
	}
}
