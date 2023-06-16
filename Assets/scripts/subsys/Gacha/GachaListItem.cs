using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class GachaListItem : MonoBehaviour
{
    [SerializeField] UISprite _background;
	[SerializeField] UILabel _nameLabel;
	[SerializeField] UILabel _lockLabel;

	[SerializeField] UILabel _freeTimerLabel;
	[SerializeField] UISprite _freeTimer;
	[SerializeField] GameObject _freeTimeHighlight;
	[SerializeField] UISprite _backAlpha;

	[SerializeField] UIButton _button1;
	[SerializeField] UILabel _button1NameLabel;
	[SerializeField] UISprite _button1Icon;
	[SerializeField] UILabel _button1PriceLabel;

	[SerializeField] UIButton _button2;
	[SerializeField] UILabel _button2NameLabel;
	[SerializeField] UISprite _button2Icon;
	[SerializeField] UILabel _button2PriceLabel;

	GachaDataMap[] datas;
	int itemType;
	DateTime fgcEndTime;
	bool bFGCWaitOver;


    string TitleName; //영웅 장비 인테리어
    string TitleType; //일반 특별

    int costItemType; //골드/펄/하트

    float btnFilterTimer; // 터치 필터링을 위해 사용하는 임시 타이머

    internal static GachaListItem Create(Transform _parant)
	{
		var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Gacha/GachaListItem", _parant);
		var result = go.GetComponent<GachaListItem>();
		result.InitLink();
		return result;
	}

	private void InitLink()
	{
		//_background = GetComponent<UISprite>();
		
		//_nameLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "Name");
		//_lockLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbLock");

		//var goFreeTime = UnityCommonFunc.GetGameObjectByName(gameObject, "freeTime");
		//_freeTimeHighlight = UnityCommonFunc.GetGameObjectByName(goFreeTime, "highlight");
		//_freeTimer = UnityCommonFunc.GetComponentByName<UISprite>(goFreeTime, "freeTime_back");
		//_freeTimerLabel = _freeTimer.GetComponentInChildren<UILabel>();
		//_backAlpha = UnityCommonFunc.GetComponentByName<UISprite>(goFreeTime, "back_alpha");

		//_button1 = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "bt1");
		//_button1NameLabel = UnityCommonFunc.GetComponentByName<UILabel>(_button1.gameObject, "Label");
		//_button1Icon = UnityCommonFunc.GetComponentByName<UISprite>(_button1.gameObject, "textIcon");
		//_button1PriceLabel = UnityCommonFunc.GetComponentByName<UILabel>(_button1.gameObject, "text");
		//_button2 = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "bt10");
		//_button2NameLabel = UnityCommonFunc.GetComponentByName<UILabel>(_button2.gameObject, "Label");
		//_button2Icon = UnityCommonFunc.GetComponentByName<UISprite>(_button2.gameObject, "textIcon");
		//_button2PriceLabel = UnityCommonFunc.GetComponentByName<UILabel>(_button2.gameObject, "text");

		_button1.onClick.Add(new EventDelegate(() => onClickButton(0)));
		_button2.onClick.Add(new EventDelegate(() => onClickButton(1)));

		datas = new GachaDataMap[3];
	}

	internal void Init(int _group)
	{
		if( _group == -1)
		{
			SetLock(true);
			return;
		}
		SetLock(false);

		if (!GetGachaData(_group))
		{
			Debug.LogError(_group + "그룹의 데이터를 찾지 못함.");
			return;
		}
        
		SetColor();

		GameCore.Instance.SetUISprite(_background, datas[1].imgID);
		_nameLabel.text = GameCore.Instance.DataMgr.GetGachaStringData(datas[1].discID);

		SetButton();

		ResetFreeData();
	}

	private void SetLock(bool _lock)
	{
        if (_lock)
        {
            GameCore.Instance.SetUISprite(_background, 5000000);
        }

		_nameLabel.gameObject.SetActive(!_lock);
		_lockLabel.gameObject.SetActive(_lock);
		_button1.gameObject.SetActive(!_lock);
		_button2.gameObject.SetActive(!_lock);
		_freeTimer.gameObject.SetActive(!_lock);
	}

	private void Update()
	{
		if(datas[0] != null)
			UpdateFreeGachaCool();
	}

	private void SetColor()
	{
        int firstBtnIndx = (datas[0] != null && GameCore.nowTime < fgcEndTime) ? 0 : 1;

        _button1.normalSprite = string.Format("BTN_{0:d2}_01_{1:d2}", datas[firstBtnIndx].btnColor, 1);
        _button1.pressedSprite = string.Format("BTN_{0:d2}_01_{1:d2}", datas[firstBtnIndx].btnColor, 2);

        _button2.normalSprite = string.Format("BTN_{0:d2}_01_{1:d2}", datas[2].btnColor, 1);
        _button2.pressedSprite = string.Format("BTN_{0:d2}_01_{1:d2}", datas[2].btnColor, 2);
	}

	private bool GetGachaData(int _group)
	{
		for (int i = 0; i < datas.Length; i++)
			datas[i] = null;

		DataMapCtrl<GachaDataMap> table = (DataMapCtrl<GachaDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.Gacha);
		var it = table.GetEnumerator();
        int idx = 1;
		while (it.MoveNext())
		{
			var data = it.Current.Value;
            if (data.group == _group)
            {
                if(data.countType == 0) datas[0] = data;
                else                    datas[idx++] = data;
            }
		}

		if (datas[1] == null || datas[2] == null)
			return false;

		switch(datas[1].itemType)
		{
			case 1: itemType = 99; break;
			case 2: itemType = 1; break;
			case 3: itemType = 5; break;
		}

		return true;
	}

	internal void ResetFreeData()
	{
		if (datas[0] == null)
		{
			_freeTimer.gameObject.SetActive(false);
			return;
		}

		_freeTimer.gameObject.SetActive(true);
		var fgcData = GameCore.Instance.PlayerDataMgr.GetFreeGachaCool(datas[0].id);
		fgcEndTime = fgcData + new TimeSpan(0, 0, datas[0].waitTime);

        if (GameCore.nowTime < fgcEndTime)
        {
            foreach (var tw in _freeTimer.GetComponentsInChildren<UITweener>())
            {
                tw.GetComponent<UITweener>().enabled = false;
                tw.GetComponent<UITweener>().ResetToBeginning();
            }
            _backAlpha.alpha = 0f;
            _freeTimeHighlight.SetActive(false);
        }
        else
        {
            foreach (var tw in _freeTimer.GetComponentsInChildren<UITweener>())
                tw.GetComponent<UITweener>().enabled = true;
            _freeTimeHighlight.SetActive(true);
        }

        //Debug.Log("[" + itemType + "] " + fgcData + " + " + new TimeSpan(0, 0, datas[0].waitTime)+ " = " +  fgcEndTime);
        bFGCWaitOver = false;
		UpdateFreeGachaCool();
	}

	internal void SetButton()
	{
		_button1NameLabel.text = string.Format("{0}회", datas[1].reward1Count + datas[1].reward2Count);
		SetResourceIcon(_button1Icon, datas[1].costItemID);
		_button1PriceLabel.text = datas[1].lastCostValue.ToString("N0");

		_button2NameLabel.text = string.Format("{0}회", datas[2].reward1Count + datas[2].reward2Count);
		SetResourceIcon(_button2Icon, datas[2].costItemID);
		_button2PriceLabel.text = datas[2].lastCostValue.ToString("N0");
	}

	private void SetResourceIcon(UISprite _sp, int _itemKey)
	{
		switch (_itemKey)
		{
			case (int)ResourceType.Gold:				_sp.spriteName = "ICON_MONEY_02"; break;
			case (int)ResourceType.Cash:				_sp.spriteName = "ICON_MONEY_03"; break;
			case (int)ResourceType.Friendship:	_sp.spriteName = "ICON_MONEY_04"; break;
			default:														_sp.spriteName = "TS_00_NAME"; break;
		}
	}

	private bool UpdateFreeGachaCool()
	{
		if (bFGCWaitOver)
			return true;

		if (GameCore.nowTime < fgcEndTime)
        {
            var gap = fgcEndTime - GameCore.nowTime;
            _freeTimerLabel.text = string.Format("{0:00}:{1:00}:{2:00} 후 무료", (int)gap.TotalHours, (int)gap.Minutes, gap.Seconds);
			_freeTimer.spriteName = "P_BOX_02";
		}
		else
		{
			_freeTimerLabel.text = "무료 뽑기 가능";
			_freeTimer.spriteName = "P_BOX_01";
			_button1PriceLabel.text = "0";
			bFGCWaitOver = true;
        }

		return bFGCWaitOver;
	}
    public static bool GetHeroGachaFreeCool()
    {
        var fgcData = GameCore.Instance.PlayerDataMgr.GetFreeGachaCool(5000001);
        var HeroEndTime = fgcData + new TimeSpan(0, 0, 86400);
        if (GameCore.nowTime > HeroEndTime) return true;
        else return false;
    }
    public static bool GetItemGachaFreeCool()
    {
        var fgcData = GameCore.Instance.PlayerDataMgr.GetFreeGachaCool(5000006);
        var ItemEndTime = fgcData + new TimeSpan(0, 0, 86400);
        if (GameCore.nowTime > ItemEndTime) return true;
        else return false;
    }
    public static bool GetInteriorGachaFreeCool()
    {
        var fgcData = GameCore.Instance.PlayerDataMgr.GetFreeGachaCool(5000011);
        var InteriorEndTime = fgcData + new TimeSpan(0, 0, 86400);
        if (GameCore.nowTime > InteriorEndTime) return true;
        else return false;
    }

 

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_num">UI 상으로 봤을때 윗 버튼이면 0, 아래면 1</param>
    private void onClickButton(int _num)
    {
        if (Time.time < btnFilterTimer)
        {
            Debug.LogError("Filtered");
            return;
        }
        btnFilterTimer = Time.time + 0.8f;

        GachaDataMap data = null;

        if (_num == 0)
        {
            if (GameCore.Instance.lobbyTutorial != null && GameCore.Instance.lobbyTutorial.IsRunning)
            {
                GameCore.Instance.ShowReceiveItem(new CardSData[] { new HeroSData(1000091) });
            }
            else
            {
                if (datas[0] != null && GameCore.nowTime > fgcEndTime) // 무료 뽑기
                {
                    GameCore.Instance.NetMgr.Rew_Gacha_Try(datas[0].id);// itemType, true, true, false);
                    return;
                }
                else
                {
                    data = datas[1];
                }
            }
        }
        else
        {
            data = datas[2];
        }


        if (data != null)
        {
            var gachaName = GameCore.Instance.DataMgr.GetGachaStringData(data.discID);
            GameCore.Instance.ShowAgree(string.Format("{0} 뽑기", gachaName),
                string.Format("다음 재화를 이용하여\n {0} 뽑기를 진행하시겠습니까?", gachaName),
                string.Format("{0:N0}", data.lastCostValue), (MoneyType)(data.costItemID - (int)ResourceType.Gold), 0, () =>
                {
                    GameCore.Instance.CloseMsgWindow();
                    if (CheckMoney(data.costItemID, data.lastCostValue))
                        GameCore.Instance.NetMgr.Rew_Gacha_Try(data.id);// itemType, true, true, false);
                });
        }
    }

	private bool CheckMoney(int _key, int _needCount)
	{
		var count = GameCore.Instance.PlayerDataMgr.GetReousrceCount((ResourceType)_key);

		if(count < _needCount)
		{
			var data = GameCore.Instance.DataMgr.GetItemData(_key);
			if(data == null)
				Debug.LogError("잘못된 재화 데이터 " + _key);
            else
                GameCore.Instance.ShowReduceResource((ResourceType)_key);

            return false;
		}

		return true;
	}

    public Transform GetTransformButton1 { get { return _button1.transform; } }
}
