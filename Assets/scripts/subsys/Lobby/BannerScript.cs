using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BannerScript : MonoBehaviour
{
	int width;
	int height;
	public float timer = 5f;
	float timeAcc;

	TouchSlidScript slid;
	bool bSlid;

	List<TweenPosition> banner = new List<TweenPosition>();
	public int index;


	private void Awake()
	{
		slid = GetComponent<TouchSlidScript>();
		slid.SetCallbackEndDrag(CBEndDrag);
		var sp = GetComponent<UISprite>();
		width = sp.width;
		height = sp.height;
	}

    internal void Init()
    {
        var iter = GameCore.Instance.DataMgr.GetMainBannerEnumertor();
        while(iter.MoveNext())
        {
            var data = iter.Current.Value;
            SetBannerSprite(data.atlasKey, data.spriteName);
            slid.EnqIndex(data.id);
        }
    }


    internal void SetBannerSprite(int _atlasKey, string _spriteName)
    {
        var sp = GameCore.Instance.ResourceMgr.GetInstanceLocalObject(CSTR.RSC_Banner, transform).GetComponent<UISprite>();
        GameCore.Instance.SetUISprite(sp, _atlasKey, _spriteName);
        Add(sp);
    }


	private void Add(UISprite _sp)
	{
		if (banner.Count != 0)
			_sp.transform.localPosition = new Vector3(width, 0,0);

		_sp.width = width;
		_sp.height = height;
		banner.Add(_sp.GetComponent<TweenPosition>());
	}

	internal void CBEndDrag(Vector2 _vec)
	{
		if (_vec.x < 0)
			MoveNext();
		else
			MovePrev();
	}

	void Update ()
	{
		if (slid.Pressed)
			timeAcc = 0f;

		timeAcc += Time.deltaTime;
		if( timer <= timeAcc )
		{
			MoveNext();
		}

		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			MoveNext();
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			MovePrev();
		}
	}

	internal void MoveNext()
	{
		timeAcc = 0f;
		var tw1 = banner[index];
		tw1.from = tw1.transform.localPosition;//Vector3.zero;
		tw1.to = new Vector3(-width, 0);
		tw1.ResetToBeginning();
		tw1.PlayForward();

		index = (index + 1) % banner.Count;
        slid.urlPos = index;

		var tw2 = banner[index];
		tw2.from = new Vector3(width, 0) + tw1.transform.localPosition;
		tw2.to = Vector3.zero;
		tw2.ResetToBeginning();
		tw2.PlayForward();
	}

	internal void MovePrev()
	{
		timeAcc = 0f;
		var tw1 = banner[index];
		tw1.from = tw1.transform.localPosition;//Vector3.zero;
		tw1.to = new Vector3(width, 0);
		tw1.ResetToBeginning();
		tw1.PlayForward();

		index = (banner.Count + index - 1) % banner.Count;
        slid.urlPos = index;

        var tw2 = banner[index];
		tw2.from = new Vector3(-width, 0) + tw1.transform.localPosition;
		tw2.to = Vector3.zero;
		tw2.ResetToBeginning();
		tw2.PlayForward();
	}
}
