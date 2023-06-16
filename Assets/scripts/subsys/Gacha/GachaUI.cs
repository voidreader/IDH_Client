using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaUI : MonoBehaviour, ISequenceTransform
{
	[SerializeField] UIButton[] _tabButtons;
    [SerializeField] GameObject[] _listRoot;
    [SerializeField] UILabel _headLabel;
    [SerializeField] private UIButton _linkButton;

    List<GachaListItem> _listItems;     // 내부 수정 변수
    int _tabIndex;

    #region [5Star] 사용흔적을 확인 할 수 없는 변수들
    UILabel[] lbTimer;      // 사용안됨
    DateTime[] timers;      // 사용안됨
    bool[] runningTimer;    // 사용안됨
    #endregion

    public int switchingNum { get; private set; }
    //[SerializeField] private string[] linkAddress;

    internal static GachaUI Create(Transform _parent)
	{
		var result = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Gacha/PanelGachaUI", GameCore.Instance.ui_root).GetComponent<GachaUI>();
		result.InitLink();
		return result;
	}

	private void InitLink()
	{
        #region [5Star]에서 외부로부터 데이터를 받아오던 형식 : 현재는 변경된 사항
        //[NOTE] : 변수명 리팩토링 중 외부에서 어떠한 데이터를 참조해야하는지 모를때 참고할 것.

        _headLabel = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "HeadName");
        _tabButtons = new UIButton[3]  {
            UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "tabBt_Hero"),
            UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "tabBt_Equip"),
            UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "tabBt_Interior")
        };
        _listRoot = new GameObject[3] {
            UnityCommonFunc.GetGameObjectByName(gameObject, "heroListRoot"),
            UnityCommonFunc.GetGameObjectByName(gameObject, "equipListRoot"),
            UnityCommonFunc.GetGameObjectByName(gameObject, "interiorListRoot")
        };
        _linkButton = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "LinkButton");

        #endregion

        lbTimer = new UILabel[3];
		timers = new DateTime[3];
		runningTimer = new bool[3];

		_listItems = new List<GachaListItem>();

        for (int i = 0; i < _tabButtons.Length; ++i)
        {
            var n = i;
            _tabButtons[i].onClick.Add(new EventDelegate(() => {
                GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
                //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
                SwitchingTab(n);
            }));
        }

        _linkButton.onClick.Add(new EventDelegate(() =>
        {
            GoToLinkPage();
        }));
        SwitchingTab(0);
	}

	internal void Init()
	{
        List<int> groups = new List<int>();
		DataMapCtrl<GachaDataMap> table = (DataMapCtrl<GachaDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.Gacha);
		var it = table.GetEnumerator();
		while (it.MoveNext())
		{
			var data = it.Current.Value;
			if (groups.Contains(data.group))
				continue;

			groups.Add(data.group);

			var listItem = GachaListItem.Create(_listRoot[data.itemType-1].transform);
			listItem.Init(data.group);
			_listItems.Add(listItem);
		}

        // Add Lock Dummy
        for (int i = 0; i < 3; ++i)
        {
            while(_listRoot[i].transform.childCount < 4)
			{
				var listItem = GachaListItem.Create(_listRoot[i].transform);
				listItem.Init(-1);
			}
		}
        GachaFreeCheck();

    }

    public void GachaFreeCheck()
    {
        if(GachaListItem.GetHeroGachaFreeCool()) UnityCommonFunc.GetGameObjectByName(gameObject, "tabBt_Hero").transform.GetChild(2).gameObject.SetActive(true);
        else UnityCommonFunc.GetGameObjectByName(gameObject, "tabBt_Hero").transform.GetChild(2).gameObject.SetActive(false);
        if (GachaListItem.GetItemGachaFreeCool())UnityCommonFunc.GetGameObjectByName(gameObject, "tabBt_Equip").transform.GetChild(2).gameObject.SetActive(true);
        else UnityCommonFunc.GetGameObjectByName(gameObject, "tabBt_Equip").transform.GetChild(2).gameObject.SetActive(false);
        if(GachaListItem.GetInteriorGachaFreeCool()) UnityCommonFunc.GetGameObjectByName(gameObject, "tabBt_Interior").transform.GetChild(2).gameObject.SetActive(true);
        else UnityCommonFunc.GetGameObjectByName(gameObject, "tabBt_Interior").transform.GetChild(2).gameObject.SetActive(false);
    }

	// Switching Tab
	public void SwitchingTab(int _n)
	{
        switchingNum = _n;
        _tabButtons[_tabIndex].GetComponent<UISprite>().spriteName = "BTN_06_01_01";
        _tabButtons[_tabIndex].transform.GetChild(0).gameObject.SetActive(false);
        _tabButtons[_tabIndex].transform.localScale = new Vector3(1f, 1f);
		_listRoot[_tabIndex].SetActive(false);

		_tabIndex = _n;
        _tabButtons[_tabIndex].transform.GetChild(0).gameObject.SetActive(true);
		_tabButtons[_tabIndex].GetComponent<UISprite>().spriteName = "BTN_06_01_02";
		_tabButtons[_tabIndex].transform.localScale = new Vector3(1.1f, 1.1f);
        _headLabel.text = _tabButtons[_tabIndex].transform.GetChild(1).GetComponentInChildren<UILabel>().text + " 뽑기";
		_listRoot[_tabIndex].SetActive(true);
    }

	internal void ResetLists()
	{
		for(int i = 0; i < _listItems.Count;++i)
		{
			_listItems[i].SetButton();
			_listItems[i].ResetFreeData();
		}
        GachaFreeCheck();
    }

    public bool GetNowGachaPlaying()
    {
        if (GameObject.Find("ReceiveItemRoot") != null)
        {
            GachaFreeCheck();
            return GameObject.Find("ReceiveItemRoot").GetComponent<ReceiveEffectUI>().GetNowGachaPlaying();
        }
        else
        {
            GachaFreeCheck();
            return false;
        }
    }


    public void GoToLinkPage()
    {
        Application.OpenURL(CSTR.URL_GachaRate);
    }
    

    public List<ReturnTutorialData> GetTutorialTransformList(int tutorialNum)
    {
        List<ReturnTutorialData> nTutorialList = new List<ReturnTutorialData>();
        switch (tutorialNum)
        {
            case 2:
                nTutorialList.Add(new ReturnTutorialData(_listItems[0].GetTransformButton1, 0));
                nTutorialList.Add(new ReturnTutorialData(GameCore.Instance.CommonSys.tbUi.GetBackButtonTransform, 0));
                break;
            default:
                break;
        }
        return nTutorialList;
    }
}
