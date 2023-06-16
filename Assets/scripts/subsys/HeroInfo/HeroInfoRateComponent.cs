using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroInfoRateComponent : MonoBehaviour
{
    [SerializeField] UISprite spLatest;
    [SerializeField] UISprite spMostRate;

    [SerializeField] Transform listRoot;

    [SerializeField] Transform tfExpendSpeach;
    [SerializeField] UILabel lbExpendSpeach;

    [SerializeField] UILabel lbTotalRate;

    [SerializeField] UILabel lbTating;

    UIGrid grid;
    Transform tfScrollView;
    float viewHeight;
    float contentItemHeight;

    HeroSData sdata;

    int sortOrder; // 0 ~ 1. 0 - 추천순. 1 - 최신순
    Dictionary<long, HeroInfoRateListItem>[] items = new Dictionary<long, HeroInfoRateListItem>[2] 
        {
            new Dictionary<long, HeroInfoRateListItem>(),
            new Dictionary<long, HeroInfoRateListItem>()
        };
    int[] calledNeedItemCount = new int[2];

    HeroEvaluateSData mine;

    string cachedText = "";
    int cachedRate = 4;
    HeroInfoRateListItem cachedItem;


    internal void Init(HeroSData _sdata)
    {
        sdata = _sdata;
        grid = listRoot.GetComponent<UIGrid>();
        grid.hideInactive = true;
        grid.sorting = UIGrid.Sorting.Custom;
        tfScrollView = listRoot.parent;
        var panel = tfScrollView.GetComponent<UIPanel>();
        viewHeight = panel.GetViewSize().y;
        contentItemHeight = grid.cellHeight;
        SetEvaluateMine(null);

        SetTotalRate(0f);
        OnClickExpendSpeachBlock();
        OnClickSortLatest();
    }

    internal void SetTotalRate(float _totalRate)
    {
        lbTotalRate.text = _totalRate.ToString("F1");
    }

    /// <summary>
    /// 플레이어의 평가를 업데이트 한다.
    /// </summary>
    /// <param name="_mine"></param>
    internal void SetEvaluateMine(HeroEvaluateSData _mine)
    {
        mine = _mine;
        if (_mine == null)
        {
            lbTating.text = "평가 하기";
        }
        else
        {
            lbTating.text = "평가 수정";

            for (int i = 0; i < items.Length; ++i)
            {
                HeroInfoRateListItem item = null;
                if (items[i].ContainsKey(_mine.UID))
                {
                    item = items[i][_mine.UID];
                }
                else
                {
                    item = HeroInfoRateListItem.Create(listRoot);
                    items[i].Add(_mine.UID, item);
                    //calledNeedItemCount[i]++;
                }

                item.Init(_mine, CBClickSpeachBlock, CBClickDown, CBClickUp);
                item.gameObject.SetActive(i == sortOrder);
            }
            grid.enabled = true;
        }
    }

    /// <summary>
    /// 현재 정렬의 데이터 리스트에 데이터를 추가/수정 한다.
    /// </summary>
    /// <param name="_datas"></param>
    internal void AddEvaluateList(List<HeroEvaluateSData> _datas)
    {
        if (_datas == null)
            return;

        for (int i = 0; i < _datas.Count; ++i)
        {
            HeroInfoRateListItem item = null;
            if (items[sortOrder].ContainsKey(_datas[i].UID))
            {
                item = items[sortOrder][_datas[i].UID];
            }
            else
            {
                item = HeroInfoRateListItem.Create(listRoot);
                items[sortOrder].Add(_datas[i].UID, item);
            }

            item.Init(_datas[i], CBClickSpeachBlock, CBClickDown, CBClickUp);
        }

        grid.enabled = true;
    }

    /// <summary>
    /// 정렬 관계없이 모든 아이템 중에서 해당 데이터를 찾아 업데이트 한다.없다면 아무것도 하지 않는다.
    /// </summary>
    /// <param name="_data"></param>
    internal void UpdateEvaluate(HeroEvaluateSData _data)
    {
        for (int i = 0; i < items.Length; ++i)
        {
            if (items[i].ContainsKey(_data.UID))
            {
                HeroInfoRateListItem item = items[i][_data.UID];
                item.Init(_data, CBClickSpeachBlock, CBClickDown, CBClickUp);
            }
        }
    }


    /// <summary>
    /// 현재 정렬 리스트의 하단부에 다달았다면 리스트를 추가로 로딩하도록한다.(만약 추가 로드할 것이 없다면 아무 것도 하지 않는다.)
    /// </summary>
    void Update()
    {
        if (calledNeedItemCount[sortOrder] <= items[sortOrder].Count)
        {
            if ((items[sortOrder].Count - 2) * contentItemHeight - viewHeight < tfScrollView.localPosition.y)
            {
                GameCore.Instance.NetMgr.Req_Character_Evaluate_List(sdata.key, sortOrder == 1, calledNeedItemCount[sortOrder]);
                calledNeedItemCount[sortOrder] += 10;
            }
        }
    }

    void CBClickSpeachBlock(HeroInfoRateListItem _data)
    {
        tfExpendSpeach.localPosition = _data.transform.localPosition;
        lbExpendSpeach.text = _data.data.comment;
        tfExpendSpeach.gameObject.SetActive(true);
    }

    void CBClickDown(long _UID)
    {
        GameCore.Instance.NetMgr.Req_Character_Evaluate(_UID, sdata.key, false);

        //for(int i = 0; i < items[sortOrder].Count; ++i)
        //    if(items[sortOrder][i].data.UID == _UID)
        //    {
        //        items[sortOrder][i].IncDown();
        //        break;
        //    }
    }

    void CBClickUp(long _UID)
    {
        GameCore.Instance.NetMgr.Req_Character_Evaluate(_UID, sdata.key, true);
        //for (int i = 0; i < items[sortOrder].Count; ++i)
        //    if (items[sortOrder][i].data.UID == _UID)
        //    {
        //        items[sortOrder][i].IncUp();
        //        break;
        //    }
    }

    public void OnClickExpendSpeachBlock()
    {
        tfExpendSpeach.gameObject.SetActive(false);
    }

    public void OnClickSortLatest()
    {
        if (sortOrder == 1)
            return;

        sortOrder = 1;
        spLatest.spriteName = CommonType.BTN_5_ACTIVE;
        spMostRate.spriteName = CommonType.BTN_5_NORMAL;

        grid.onCustomSort = CBSortLatest;
        var panel = tfScrollView.GetComponent<UIPanel>();
        panel.clipOffset = Vector2.zero;
        tfScrollView.localPosition = Vector3.zero;
        var sp = tfScrollView.GetComponent<SpringPanel>();
        if (sp != null) sp.enabled = false;
        ShowItems(sortOrder);
        OnClickExpendSpeachBlock();
    }

    public void OnClickSortMostRate()
    {
        if (sortOrder == 0)
            return;

        sortOrder = 0;
        spLatest.spriteName = CommonType.BTN_5_NORMAL;
        spMostRate.spriteName = CommonType.BTN_5_ACTIVE;

        grid.onCustomSort = CBSortMostRate;
        var panel = tfScrollView.GetComponent<UIPanel>();
        panel.clipOffset = Vector2.zero;
        tfScrollView.localPosition = Vector3.zero;
        ShowItems(sortOrder);
        OnClickExpendSpeachBlock();
    }

    void ShowItems(int _sortOrder)
    {
        for (int i = 0; i < items.Length; ++i)
        {
            var it = items[i].GetEnumerator();
            while(it.MoveNext())
            {
                it.Current.Value.gameObject.SetActive(i == sortOrder);
            }
        }
        grid.enabled = true;
    }
    private bool TextNumberCheck()
    {
        int blankTextCount = 0;
        for(int i = 0; i < cachedText.Length; ++i)
        {
            string blankTest = " ";
            if (cachedText[i].ToString() != blankTest) blankTextCount++;
        }
        if (blankTextCount < 2) return false;
        else return true;
    }


    public void OnClickRating()
    {
        var window = HeroInfoRatingWindow.Create(GameCore.Instance.ui_root);
        if (mine != null)
        {
            window.OnToggleRate((int)mine.score - 1);
            window.SetText(mine.comment);
        }
        else
        {
            window.OnToggleRate(cachedRate);
            window.SetText(cachedText);
        }

        GameCore.Instance.ShowObject("평가하기", null, window.gameObject, 1, () => {
            cachedText = window.GetText();
            cachedRate = window.SelectedRate - 1;
            if(!TextNumberCheck())
            {
                string richText = "평가는 [8C1CFFFF]2자 이상[B0B0B0FF] 남겨야 합니다.";
                GameCore.Instance.ShowAlert(richText);
                return;
            }

            /*
            string newtext = cachedText;

            for (int i = 0; i < chatFilterList.Count; ++i)
            {
                if (cachedText.Contains(chatFilterList[i]))
                {
                    newtext = newtext.Replace(chatFilterList[i], "***");
                }
            }
            */

            if (mine == null)
            {
                GameCore.Instance.NetMgr.Req_Character_Comment_New(sdata.key, window.SelectedRate, cachedText);
            }
            else
            {
                GameCore.Instance.NetMgr.Req_Character_Comment_Edit(sdata.key, window.SelectedRate, cachedText);
            }
            GameCore.Instance.CloseMsgWindow();
        });
    }

    int CBSortLatest(Transform _1, Transform _2)
    {
        var a = _1.GetComponent<HeroInfoRateListItem>();
        var b = _2.GetComponent<HeroInfoRateListItem>();

        return b.data.createTime.CompareTo(a.data.createTime);
    }

    int CBSortMostRate(Transform _1, Transform _2)
    {
        var a = _1.GetComponent<HeroInfoRateListItem>();
        var b = _2.GetComponent<HeroInfoRateListItem>();

        return b.data.upCount.CompareTo(a.data.upCount);
    }
}
