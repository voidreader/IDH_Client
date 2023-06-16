using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class StoryListItem : MonoBehaviour
{
	UISprite spBlind;

	GameObject clearLabel;

	// Head
	UILabel lbName;
	Transform tfStarRoot;
	UILabel lbPower;

	// Body
	UIGrid rewardGrid;
	CardBase[] rewards;

	Transform tfoverkillRoot;
	CardBase overkillReward;

	UIButton btPrepare;
	GameObject btnFliper;
    GameObject effectLine;

	int key;		// 데이터 테이블 키값
	int index; // 리스트에서 관리된느 번호

	internal static StoryListItem Create(Transform _parent)
	{
		var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Story/StoryListItem", _parent);
		var result = go.GetComponent<StoryListItem>();
		result.InitLink();
		return result;
	}

	private void InitLink()
	{
		spBlind = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "blind");

		clearLabel = UnityCommonFunc.GetGameObjectByName(gameObject, "spClear");

		// Head
		lbName = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "name");
		tfStarRoot = UnityCommonFunc.GetComponentByName<Transform>(gameObject, "starRoot");
		lbPower = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "power");

		// Body
		tfoverkillRoot = UnityCommonFunc.GetComponentByName<Transform>(gameObject, "okItemRoot");
		rewardGrid = UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "grid");

		btPrepare = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btn");
		btnFliper = UnityCommonFunc.GetGameObjectByName(gameObject, "fliper");
        effectLine = UnityCommonFunc.GetGameObjectByName(gameObject, "Eff_Line");
	}
	
	internal void Init(int _key, bool _active, bool _cleared, int _starFlag = 0, EventDelegate.Callback _cbPrepare = null)
	{
		// Todo : GetstageData & Set Data
		//    @$#% 반드시 리셋하고 만들 수 있게 만들 것.(Init만 호출하면 깔끔하게 바뀌도록)

		var data = GameCore.Instance.DataMgr.GetStoryData(_key);

		key = _key;
		index = data.stage;

		lbName.text = string.Format("{0}-{1}.{2}", data.chapter,index, data.name);
		lbPower.text = string.Format("{0:N0}", data.powerRecommand);
		SetStarCount(_starFlag);
		clearLabel.SetActive(_cleared);
        btnFliper.SetActive(_active && !_cleared);
        effectLine.SetActive(_active && !_cleared);
        //btnFliper.SetActive(_active);
        //effectLine.SetActive(_active);


        if (data.rewardId == 90)
			Debug.Log("!!!!");

		var rewardData = GameCore.Instance.DataMgr.GetStoryRewardData(data.rewardId);
		SetRewardItem(rewardData.rewardID , rewardData.rewardValue);
		SetOverKillRewardItem((int)ResourceType.Gold, data.ovkReward);

		SetActive(_active);
		btPrepare.onClick.Clear();
		if (_active && _cbPrepare != null)
			btPrepare.onClick.Add(new EventDelegate(()=> {
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
                //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
                GameCore.Instance.CommonSys.ShowLoadingPage(true,
                    () => { _cbPrepare(); });
                 }));

		var tw = GetComponent<UITweener>();
		tw.ResetToBeginning();
		tw.PlayForward();
	}

	private void SetStarCount(int _flag)
	{
		for(int i = 0; i < 3; ++i)
		{
			var star = tfStarRoot.GetChild(i).GetComponent<UISprite>();
			bool active = (_flag & (1 << i)) != 0; 
			if (active) star.color = new Color32(0xFF, 0xEA, 0x00, 0xFF); 
			else				star.color = new Color32(0x2F, 0x32, 0x35, 0xFF);
		}
	}
    public Transform GetPrepareButton()
    {
        return btPrepare.transform;
    }
    public Transform GetRewardTransform(int pos)
    {
        return rewards[pos].transform;
    }
	private CardBase CreateCard( int _key, Transform _parent)
	{
		CardBase card = CardBase.CreateSmallCardByKey(_key, _parent, null, (key) => GameCore.Instance.ShowCardInfoNotHave((int)key));
		return card;
	}

	private void ClearRewardItem()
	{
        for (int i = 0; i < rewards.Length; i++)
            if (rewards[i] != null)
            {
                rewards[i].transform.parent = GameCore.Instance.Ui_root;
                GameObject.Destroy(rewards[i].gameObject);
            }
		rewards = null;
	}

	private void SetRewardItem(int[] _keys, int[] _counts)
	{
		if (rewards != null)
			ClearRewardItem();

		rewards = new CardBase[_keys.Length];
		for (int i = 0; i < _keys.Length; ++i)
		{
            if(_keys[i] != -1){
                rewards[i] = CreateCard(_keys[i], rewardGrid.transform);
                if(_counts !=null)	rewards[i].SetCount(_counts[i]);
            }
		}

		rewardGrid.enabled = true;
	}

	private void SetOverKillRewardItem(int _key, int _count)
	{
		if(overkillReward != null)
		{
			Destroy(overkillReward.gameObject);
			overkillReward = null;
		}

		overkillReward = CreateCard(_key, tfoverkillRoot);
		overkillReward.SetCount(_count);
	}

	private void SetActive(bool _active)
	{
		spBlind.gameObject.SetActive(!_active);
		btPrepare.enabled = _active;
	}
}
