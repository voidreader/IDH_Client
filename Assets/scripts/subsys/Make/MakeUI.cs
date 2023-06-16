using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class MakeUI : MonoBehaviour {

	UIButton[] tabBtns;
	UIGrid[] listRoots;
	Dictionary<int, List<MakeSlotScript>> makeSlots;

	UILabel lbHead;
	int tabIdx;

	BottomRscScript rsc;

	static Dictionary<int, int[]> useResourceCache;
    UIButton linkButton;
    public int switchingNum { get; private set; }
    //[SerializeField] private string[] linkAddress;

    internal static MakeUI CreateMakeUI()
	{
		var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Make/PanelMakeUI", GameCore.Instance.ui_root);
		var result = go.GetComponent<MakeUI>();
		result.InitLink();
		return result;
	}

	private void InitLink()
	{
		tabBtns = new UIButton[3] {
			UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "tabBt_Hero"),
			UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "tabBt_Equip"),
			UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "tabBt_Interior")
		};

		listRoots = new UIGrid[3] {
			UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "heroListRoot"),
			UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "equipListRoot"),
			UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "interiorListRoot")
		};
		listRoots[0].onCustomSort = CBSortSlot;
		listRoots[1].onCustomSort = CBSortSlot;
		listRoots[2].onCustomSort = CBSortSlot;


		for (int i = 0; i < tabBtns.Length; ++i)
		{
			var n = i;
			tabBtns[i].onClick.Add(new EventDelegate(() => {
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
                //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
                SwitchingTab(n); }));
		}

		lbHead = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "HeadName");

        rsc = BottomRscScript.Create(transform);
		rsc.UpdateCount();

		if (useResourceCache == null)
		{
			useResourceCache = new Dictionary<int, int[]>();
			var it = ((DataMapCtrl<MakingSlotCostDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.MakingSlotCost)).GetEnumerator();
			while (it.MoveNext())
			{
				var data = it.Current.Value;
				var list = new int[5];
				for (int i = 0; i < 5; i++)
					list[i] = 100;

				useResourceCache.Add(data.id, list);
			}
		}

        linkButton = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "LinkButton");
        linkButton.onClick.Add(new EventDelegate(() =>
        {
            GoToLinkPage();
        }));
    }

	internal void Init()
	{
		makeSlots = new Dictionary<int, List<MakeSlotScript>>();
		var it = ((DataMapCtrl<MakingSlotCostDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.MakingSlotCost)).GetEnumerator();
		while(it.MoveNext())
		{
			var data = it.Current.Value;

			if (data.rentalCostID == -1)
				continue;

			if (!makeSlots.ContainsKey(data.type) )
				makeSlots.Add(data.type, new List<MakeSlotScript>());

			var script = MakeSlotScript.Create(listRoots[data.type-1].transform);
			script.SetCallbacks(OnClickUnlock, OnClickRental, OnClickMake, OnClickInstant, OnClickDone, OnChangeStateSlot);
			script.Init(data.id, useResourceCache[data.id]);
			makeSlots[data.type].Add(script);
		}

		SwitchingTab(0);


        MakeDoneCheck();
    }

	internal void UpdateSlotsNowTab()
	{
		//Debug.Log("tabIdx:" + tabIdx);
		var list = makeSlots[tabIdx+1];
		for (int i = 0; i < list.Count; ++i)
		{
			var key = list[i].GetDataKey();
			var data = GameCore.Instance.DataMgr.GetMakingSlotData(key);
			makeSlots[data.type][data.number - 1].Init(key, useResourceCache[data.id]);
		}
		rsc.UpdateCount();
        MakeDoneCheck();

    }

	internal void UpdateSlot(int _key)
	{
		var data = GameCore.Instance.DataMgr.GetMakingSlotData(_key);
		makeSlots[data.type][data.number-1].Init(_key, useResourceCache[data.id]);

		rsc.UpdateCount();
	}

    internal void MakeDoneCheck()
    {
        for(int i = 0; i < tabBtns.Length; ++i)
        {
            UnityCommonFunc.GetGameObjectByName(tabBtns[i].gameObject, "highlight").SetActive(false);
        }
        //일단 여기서 전부 비활성화를 먼저 시키면 기본상태가 false상태로 시작해서 있다면 활성화 됨.
        for(int i = 0; i < makeSlots[1].Count; ++i)
        {
            if(makeSlots[1][i].GetState() == MakeSlotScript.MakeState.Done)
            {
                //알림을 활성화
                UnityCommonFunc.GetGameObjectByName(tabBtns[0].gameObject, "highlight").SetActive(true);
                break;
            }
        }
        for(int i = 0; i < makeSlots[2].Count; ++i)
        {
            if(makeSlots[2][i].GetState() == MakeSlotScript.MakeState.Done)
            {
                //알림을 활성화
                UnityCommonFunc.GetGameObjectByName(tabBtns[1].gameObject, "highlight").SetActive(true);
                break;
            }
        }
        for(int i = 0; i < makeSlots[3].Count; ++i)
        {
            if(makeSlots[3][i].GetState() == MakeSlotScript.MakeState.Done)
            {
                //알림을 활성화
                UnityCommonFunc.GetGameObjectByName(tabBtns[2].gameObject, "highlight").SetActive(true);
                break;
            }
        }
    }

	public void SwitchingTab(int _n)
	{
        switchingNum = _n;
        tabBtns[tabIdx].GetComponent<UISprite>().spriteName = "BTN_06_01_01";
		tabBtns[tabIdx].transform.localScale = new Vector3(1f, 1f);
        tabBtns[tabIdx].transform.GetChild(0).gameObject.SetActive(false);
        listRoots[tabIdx].gameObject.SetActive(false);

		tabIdx = _n;

		tabBtns[tabIdx].GetComponent<UISprite>().spriteName = "BTN_06_01_02";
		tabBtns[tabIdx].transform.localScale = new Vector3(1.1f, 1.1f);
        tabBtns[tabIdx].transform.GetChild(0).gameObject.SetActive(true);
        lbHead.text = tabBtns[tabIdx].GetComponentInChildren<UILabel>().text + " 제조";
		listRoots[tabIdx].gameObject.SetActive(true);
	}


	private MakeSlotScript.MakeState OnChangeStateSlot(int _itemType, int idx, MakeSlotScript.MakeState _state)
	{
		if (_state != MakeSlotScript.MakeState.Lock && _state != MakeSlotScript.MakeState.Block)
			return _state;

		// Get ItemType Index
		int itemIdx = 0;
		int[] itemTypes = new int[] { 99, 0, 5 };
		for (; itemIdx < itemTypes.Length; ++itemIdx)
			if (itemTypes[itemIdx] == _itemType)
				break;

		// 블럭 또는 락 중 첫번째만 락 상태. 나머지는모두 블럭 상태로 만든다.
		var slots = makeSlots[itemIdx+1];
		bool findFirstLock = false;
		MakeSlotScript.MakeState result = MakeSlotScript.MakeState.Block;
		for ( int i = 0; i < slots.Count; ++i)
		{
			if (i == idx - 1 && !findFirstLock)
			{
				findFirstLock = true;
				result = MakeSlotScript.MakeState.Lock;
			}

			switch (slots[i].GetState())
			{
				case MakeSlotScript.MakeState.Block:
					if (!findFirstLock)
					{
						findFirstLock = true;
						slots[i].SetState(MakeSlotScript.MakeState.Lock, true);
					}
					break;
				case MakeSlotScript.MakeState.Lock:
					if (!findFirstLock)
						findFirstLock = true;
					else
						slots[i].SetState(MakeSlotScript.MakeState.Block, true);
					break;
			}
		}

		listRoots[itemIdx].enabled = true;
		return result;
	}


	private int CBSortSlot(Transform _1, Transform _2)
	{
		var slot1 = _1.GetComponent<MakeSlotScript>();
		var slot2 = _2.GetComponent<MakeSlotScript>();

		int state1 = Mathf.Min((int)MakeSlotScript.MakeState.None, (int)slot1.GetState());
		int state2 = Mathf.Min((int)MakeSlotScript.MakeState.None, (int)slot2.GetState());

		if (state1 != state2)
			return state1 < state2 ? 1 : -1;

		if (slot1.isRental != slot2.isRental)
			return slot1.isRental ? 1 : -1;
		
		return slot1.GetIndex() < slot2.GetIndex() ? -1 : 1;
			
	}

	private void OnClickRental(int _itemType, int _idx)
	{
        // 재화 검사 루틴
        var data = GameCore.Instance.DataMgr.GetMakingSlotData(_idx);
        var type = (ResourceType)data.rentalCostID;
        var cost = data.rentalCost;

        if (cost <= GameCore.Instance.PlayerDataMgr.GetReousrceCount(type))
            GameCore.Instance.NetMgr.Req_Make_Unlock(_idx, true);
        else
            GameCore.Instance.ShowReduceResource(type);
	}

	private void OnClickUnlock(int _itemType, int _idx)
	{
        // 재화 검사 루틴
        var data = GameCore.Instance.DataMgr.GetMakingSlotData(_idx);
        var type = (ResourceType)data.unlockCostID;
        var cost = data.unlockCost;

        if (cost <= GameCore.Instance.PlayerDataMgr.GetReousrceCount(type))
            GameCore.Instance.NetMgr.Req_Make_Unlock(_idx, false);
        else
            GameCore.Instance.ShowReduceResource(type);
	}

	private void OnClickMake(int _itemType, int _idx, int[] _values)
	{
		long[] rscUIDs = new long[5];
		for (int i = 0; i < rscUIDs.Length; ++i)
		{
			var data = GameCore.Instance.PlayerDataMgr.GetItemByKey((int)ResourceType.Coin1 + i);
			if (data == null || _values[i] > data.count)
			{
				GameCore.Instance.ShowNotice("제조 실패", "자원이 부족합니다.", 0);
				return;
			}
			rscUIDs[i] = data.uid;
		}
		useResourceCache[_idx] = _values;

		GameCore.Instance.NetMgr.Req_Make(_idx, rscUIDs, _values);
	}

	private void OnClickInstant(int _idx)
	{
        // 재화 검사
        var data = GameCore.Instance.DataMgr.GetMakingSlotData(_idx);

        ResourceType type = ResourceType.Ticket1;
        if (data.type != 1)
            type = ResourceType.Ticket2;

        if (GameCore.Instance.PlayerDataMgr.GetReousrceCount(type) <= 0)
            GameCore.Instance.ShowReduceResource(type);
        else
		    GameCore.Instance.NetMgr.Req_Make_Quick(_idx);
	}

	private void OnClickDone(int _idx)
	{
		GameCore.Instance.NetMgr.Req_Make_Done(_idx);
        MakeDoneCheck();
    }

    public bool GetNowGachaPlaying()
    {
        if (GameObject.Find("ReceiveItemRoot") != null)
        {
            return GameObject.Find("ReceiveItemRoot").GetComponent<ReceiveEffectUI>().GetNowGachaPlaying();
        }
        else return false;
    }


    public void GoToLinkPage()
    {
        Application.OpenURL(CSTR.URL_MakeRate);
    }


    public void OnclickTakeAll()
    {
        foreach(var item in makeSlots[switchingNum+1])
        {
            if (item.GetState() == MakeSlotScript.MakeState.Done)
            {
                GameCore.Instance.NetMgr.Req_Make_Take_All(switchingNum + 1);
                return;
            }
        }

        GameCore.Instance.ShowNotice("제조", "완료 가능한 제조가 없습니다.", 0);
    }
}
