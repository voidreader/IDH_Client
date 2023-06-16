using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class HeroCardSmall : CardBase
{

	[SerializeField] UISprite _illust;
	[SerializeField] UIGrid _stars;
	[SerializeField] UISprite _cover;
	[SerializeField] UISprite _blind;
	[SerializeField] UISprite _selected;
	[SerializeField] UILabel _count;

	private void InitLink()
	{
		if (_illust != null)
			return;

		#region [5Star]에서 외부로부터 데이터를 받아오던 형식 : 현재는 변경된 사항
		// [NOTE] : 변수명 리팩토링 중 외부에서 어떠한 데이터를 참조해야하는지 모를때 참고할 것.

		//_illust = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "Illust");
		//_stars = UnityCommonFunc.GetComponentByName<UIGrid>(gameObject, "Stars");
		//_cover = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "Cover");
		//_blind = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "Blind");
		//_count = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "count");
		//_selected = UnityCommonFunc.GetComponentByName<UISprite>(gameObject, "Selected");
        #endregion

    }

    internal override void Init(CardSData _sdata, CardDataMap _data, Action<long> _cbClick, Action<long> _cbPress)
	{
		InitLink();
		base.Init(_sdata, _data, _cbClick, _cbPress);

		var data = (UnitDataMap)Data;
		if (data == null)
			return;

		// 일러스트 및 이름 설정
		var spData = GameCore.Instance.DataMgr.GetSpriteData(data.GetSmallCardSpriteKey());
		_illust.spriteName = spData.sprite_name;
		GameCore.Instance.ResourceMgr.GetObject<GameObject>(ABType.AB_Atlas, spData.atlas_id, (go) => { if (go != null) _illust.atlas = go.GetComponent<UIAtlas>(); });

        // 별 설정 // 각성막음
        ///////////////////////////////////////////////////////////////////////////
        /// Open spec : 각성 불가이기 때문에 등급만 5성으로 고정                ///
        ///////////////////////////////////////////////////////////////////////////
        //var cnt = data.evolLvl;                                               ///
        var cnt = Mathf.Min(data.evolLvl, 5);                                   ///
        ///////////////////////////////////////////////////////////////////////////

        if (data.IsExpCard())
        {
            _stars.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            _stars.transform.parent.gameObject.SetActive(true);

            var starName = (cnt <= 5) ? "ICON_STAR_01_S" : "ICON_STAR_02_S";
            var emptyStarName = (cnt <= 5) ? "ICON_STAR_00_S" : "ICON_STAR_01_S";
            cnt = ((cnt - 1) % 5) + 1;
            var starCnt = _stars.transform.childCount;
            for (int i = 0; i < starCnt; ++i)
            {
                if (i < cnt)
                {
                    _stars.transform.GetChild(i).GetComponent<UISprite>().spriteName = starName;
                    _stars.transform.GetChild(i).GetComponent<UISprite>().color = Color.white;
                    _stars.transform.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    _stars.transform.GetChild(i).GetComponent<UISprite>().spriteName = emptyStarName;
                    _stars.transform.GetChild(i).GetComponent<UISprite>().color = (cnt <= 5) ? new Color(0.6f, 0.6f, 1f) : Color.white;
                    _stars.transform.GetChild(i).gameObject.SetActive(true);
                }
            }
        }
		_stars.Reposition();

		// 커버 설정
		_cover.color = colors[data.rank];


		// 기본 상태 설정
		_blind.enabled = false;
		_selected.enabled = false;
	}

	//internal override void Init(long _idx, bool _have, ItemType _type, Action<long> _cbClick, Action<long> _cbPress)
	//{
	//	InitLink();
	//	base.Init(_idx, _have, _type, _cbClick, _cbPress);

	//	var data = (UnitDataMap)Data;

	//	// 일러스트 및 이름 설정
	//	var spData = GameCore.Instance.DataMgr.GetSpriteData(data.GetSmallCardSpriteKey());
	//	spIllust.spriteName = spData.sprite_name;
	//	GameCore.Instance.ResourceMgr.GetObjectAsync<GameObject>(ABType.AB_Atlas, spData.atlas_id, (go) => spIllust.atlas = go.GetComponent<UIAtlas>());

	//	// 별 설정
	//	var cnt = data.evolLvl;
	//	var starName = (cnt < 5) ? "ICON_STAR_01_S" : "ICON_STAR_02_S";
	//	cnt = ((cnt - 1) % 5) + 1;
	//	var starCnt = tbStars.transform.childCount;
	//	for (int i = 0; i < starCnt; ++i)
	//	{
	//		if (i < cnt)
	//		{
	//			tbStars.transform.GetChild(i).GetComponent<UISprite>().spriteName = starName;
	//			tbStars.transform.GetChild(i).gameObject.SetActive(true);
	//		}
	//		else
	//		{
	//			tbStars.transform.GetChild(i).gameObject.SetActive(false);
	//		}
	//	}
	//	tbStars.Reposition();

	//	// 커버 설정
	//	spCover.color = colors[data.rank];


	//	// 기본 상태 설정
	//	spBlind.enabled = false;
	//	spSelected.enabled = false;
	//}

	protected override void UpdateEnable(bool _active)
	{
		_blind.enabled = !_active;
	}

	protected override void UpdateHighLight(SelectState _state)
	{
		if (_state == SelectState.Highlight)
		{
			_selected.enabled = true;
			_selected.spriteName = "SELECT_02_01_01";
			_blind.enabled = true;
		}
		else if (_state == SelectState.Select)
		{
			_selected.enabled = true;
			_selected.spriteName = "SELECT_01_01_01";
			_blind.enabled = true;
		}
		else
		{
			_selected.enabled = false;
			UpdateEnable(State == States.Normal);
		}
	}

	protected override void UpdateInfo(UnitInfo _info)
	{
        // none
    }

    protected override void UpdateButton(ActiveButton _active)
	{
        // none
    }

    protected override void UpdateState(States _state)
	{
        // none
    }

    protected override void UpdateCount(int _count)
	{
        // none
        if (_count <= 1)
            this._count.text = string.Empty;
        else
            this._count.text = _count.ToString("N0");
    }

    protected override void UpdateEnchant(int _value)
    {
        // none
    }

    protected override void UpdateCompare(CardSData _target)
    {
        // None
    }

    protected override void UpdateLock(bool _lock)
    {
        // Todo : UpdateLock HeroCardSmall
    }
}

