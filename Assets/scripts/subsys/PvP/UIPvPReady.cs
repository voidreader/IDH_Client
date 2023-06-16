using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SeasonEndData
{
	public DayOfWeek doyOfWeek;
	public int Hour;
}

internal class UIPvPReady : /*MonoBehaviour*/UIBase
{
	[SerializeField]
	PvPPlayerInfoScript playerInfo;

	[SerializeField]
	PvPPlayerRankInfo playerRankInfo;

	[SerializeField]
	UISprite[] listToggleBtns;
	[SerializeField]
	GameObject[] toggleLists;
	[SerializeField]
	UIGrid[] grids;
	[SerializeField]
	UIDragScrollView dragScroll;
	int nowPageIdx;

	[SerializeField]
	GameObject pfRankListItem;
	[SerializeField]
	GameObject pfGradeInfoListItem;

	[SerializeField]
    UIGrid rankGrid;

    [SerializeField]
    GameObject rankComponent;

    [SerializeField]
    GameObject userRankComponent;

	[SerializeField]
	UILabel userRankLbl;

	[SerializeField]
	UILabel seasonTimerLbl;

	DateTime seasonEndTime;

	//SeasonEndData[] seasonEndData = new SeasonEndData[]
	//{
	//	new SeasonEndData() { doyOfWeek = DayOfWeek.Sunday, Hour = 2 },
	//	new SeasonEndData() { doyOfWeek = DayOfWeek.Thursday, Hour = 2 },
	//};

	private void Start()
	{
        seasonEndTime = PvPReadySys.GetRemainPvPSeasonEnd();
        Debug.Log("Season End Time : " + seasonEndTime);
	}

	internal void Init( EventDelegate.Callback _cbSelectStage, EventDelegate.Callback _cbBack, stRank userRank, List<stRank> rankList)
	{
	}


	public void UpdatePlayerRankInfo(int _grade, int _groupRank)
	{
		playerRankInfo.SetData(_grade, _groupRank);
	}

	public void UpdatePlayerInfo(PvPSData _data, int _rank, int _groupRank)
	{
		playerInfo.SetData(UIPvPMatch.PVPInfoTarget.Player, _data, _rank, _groupRank);
	}

	public void CreateGroupRank(List<PvPGroupRankSData> _list)
	{
		var playerUID = GameCore.Instance.PlayerDataMgr.PvPData.userUID;
		for (int i = 0; i < _list.Count; i++)
		{
			var go = Instantiate(pfRankListItem, grids[0].transform);
			go.GetComponent<UIRankComponent>().Init(_list[i]);

            if (_list[i].USER_UID.Equals(playerUID))
            {
                userRankComponent.GetComponent<UIRankComponent>().Init(_list[i]);
                GameCore.Instance.PlayerDataMgr.PvPData.power = _list[i].POWER;
            }
		}
		grids[0].Reposition();
	}

	public void CreateTop50Rank(List<PvPGroupRankSData> _list)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			var go = Instantiate(pfRankListItem, grids[1].transform);
			go.GetComponent<UIRankComponent>().Init(_list[i]);
		}
		grids[1].Reposition();
	}

	public void CreateRankInfo(Dictionary<int, float> _pvpGradeAvg)
	{
		for (int i = 7000009; i > 7000000; --i)
		{
			var go = Instantiate(pfGradeInfoListItem, grids[2].transform);
			if (_pvpGradeAvg.ContainsKey(i))
				go.GetComponent<PvPGradeInfoItem>().SetData(i, _pvpGradeAvg[i]);
			else
				go.GetComponent<PvPGradeInfoItem>().SetData(i, -1);
		}
		grids[2].Reposition();
	}


	public void UpdateNowPage()
	{
		switch (nowPageIdx)
		{
			case 1:
				OnClickTop50Rank();
				break;
			case 2:
				OnClickGradeInfo();
				break;
			default:
				OnClickGroupRank();
				break;
		}
	}

	public void OnClickGroupRank()
	{
		nowPageIdx = 0;
		listToggleBtns[0].spriteName = "BTN_05_01_02";
		listToggleBtns[1].spriteName = "BTN_05_01_01";
		listToggleBtns[2].spriteName = "BTN_05_01_01";

		toggleLists[0].SetActive(true);
		toggleLists[1].SetActive(false);
		toggleLists[2].SetActive(false);

		userRankComponent.SetActive(true);
		userRankLbl.text = string.Format("{0:N0}위", GameCore.Instance.PlayerDataMgr.PvPGroupRank);
		dragScroll.scrollView = toggleLists[0].GetComponentInChildren<UIScrollView>();
	}

	public void OnClickTop50Rank()
	{
		nowPageIdx = 1;
		listToggleBtns[0].spriteName = "BTN_05_01_01";
		listToggleBtns[1].spriteName = "BTN_05_01_02";
		listToggleBtns[2].spriteName = "BTN_05_01_01";

		toggleLists[0].SetActive(false);
		toggleLists[1].SetActive(true);
		toggleLists[2].SetActive(false);

		userRankComponent.SetActive(true);
		userRankLbl.text = string.Format("{0:N0}위", GameCore.Instance.PlayerDataMgr.PvPRank);
		dragScroll.scrollView = toggleLists[1].GetComponentInChildren<UIScrollView>();
	}

	public void OnClickGradeInfo()
	{
		nowPageIdx = 2;
		listToggleBtns[0].spriteName = "BTN_05_01_01";
		listToggleBtns[1].spriteName = "BTN_05_01_01";
		listToggleBtns[2].spriteName = "BTN_05_01_02";

		toggleLists[0].SetActive(false);
		toggleLists[1].SetActive(false);
		toggleLists[2].SetActive(true);

		userRankComponent.SetActive(false);
		dragScroll.scrollView = toggleLists[2].GetComponentInChildren<UIScrollView>();
	}


	public void OnClickRankReadyBtn()
	{
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        GameCore.Instance.ChangeSubSystem(SubSysType.PvPMatch, null);
	}


	private void Update()
	{
		TimeSpan timeGap = seasonEndTime - GameCore.nowTime;
		if( 0 < timeGap.TotalDays )
		{
			seasonTimerLbl.text = string.Format("시즌 종료까지 [00F0FFFF]{0}일{1}시간[-] 남음", timeGap.Days, timeGap.Hours);
		}
		else
			seasonTimerLbl.text = string.Format("시즌 종료까지 [00F0FFFF]{0}:{1}:{2}[-] 남음", timeGap.Hours, timeGap.Minutes, timeGap.Seconds);
        /*
		if( timeGap.TotalSeconds < 0)
		{
			GameCore.Instance.ShowNotice("시즌 종료", "시즌이 종료 되었습니다.", 0);
			GameCore.Instance.ChangeSubSystem(SubSysType.Lobby, null);
		}
        */
	}
}
