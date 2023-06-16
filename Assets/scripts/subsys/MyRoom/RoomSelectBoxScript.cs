using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSelectBoxScript : MonoBehaviour
{

	public UISprite spButton;
	public GameObject roomIndexRoot;
	public UILodgingComponent[] roomIndexList;

	bool bOpened;

	Collider2D[] colliders;
	Collider2D colButton;

	private void Start()
	{
        //var count = GameCore.Instance.PlayerDataMgr.MyRoomItemDic.Count;
        int count = 10;
        colliders = new Collider2D[roomIndexList.Length];

		for (int i = 0; i < roomIndexList.Length; ++i)
		{
			roomIndexList[i].Init(i < count);
			colliders[i] = roomIndexList[i].GetComponent<Collider2D>();
		}
		colButton = spButton.GetComponent<Collider2D>();
	}

	public void Show(bool _show)
	{
		if (gameObject.activeSelf == _show)
			return;

		bOpened = _show;
		if (bOpened)
		{
			spButton.spriteName = "BTN_03_01_02";
            gameObject.SetActive(true);
		}
		else
		{
			spButton.spriteName = "BTN_03_01_01";
            gameObject.SetActive(false);
		}
	}
}
