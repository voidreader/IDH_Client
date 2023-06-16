using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CardAlignment : MonoBehaviour
{
   
    GameObject alignmentBox;

    private void Awake()
    {
        alignmentBox = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("common/AlignmentBox", transform);

        UnityCommonFunc.GetComponentByName<UIButton>(alignmentBox, "CardRankDown").onClick.Add(new EventDelegate(OnClickSortByRankAscending));
        UnityCommonFunc.GetComponentByName<UIButton>(alignmentBox, "CardRankUp").onClick.Add(new EventDelegate(OnClickSortByRankDescending));
        UnityCommonFunc.GetComponentByName<UIButton>(alignmentBox, "CardNameDown").onClick.Add(new EventDelegate(OnClickSortByNameAscending));
        UnityCommonFunc.GetComponentByName<UIButton>(alignmentBox, "CardNameUp").onClick.Add(new EventDelegate(OnClickSortByNameDescending));
        UnityCommonFunc.GetComponentByName<UIButton>(alignmentBox, "GetCardDown").onClick.Add(new EventDelegate(OnClickSortByGetAscending));
        UnityCommonFunc.GetComponentByName<UIButton>(alignmentBox, "GetCardUp").onClick.Add(new EventDelegate(OnClickSortByGetDescending));
        UnityCommonFunc.GetComponentByName<UIButton>(alignmentBox, "CardDamageDown").onClick.Add(new EventDelegate(OnClickSortByPowerAscending));
        UnityCommonFunc.GetComponentByName<UIButton>(alignmentBox, "CardDamageUp").onClick.Add(new EventDelegate(OnClickSortByPowerDescending));
    }

    //랭크기준 내림차순 정렬
    public void OnClickSortByRankAscending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
    }
    
    //랭크기준 오름차순 정렬
    public void OnClickSortByRankDescending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
    }

    //이름기준 내림차순 정렬
    public void OnClickSortByNameAscending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
    }

    //이름기준 오름차순 정렬
    public void OnClickSortByNameDescending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
    }

    //획득기준 내림차순 정렬
    public void OnClickSortByGetAscending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
    }

    //획득기준 오름차순 정렬
    public void OnClickSortByGetDescending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
    }

    //공격력 내림차순 정렬
    public void OnClickSortByPowerAscending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
    }

    //공격력 오름차순 정렬
    public void OnClickSortByPowerDescending()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
    }

    #region 영웅 정렬방식.
    // 랭크 내림차순 정렬
    private int HeroSortByRankAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        if ((data1.id - 1) % 10 != (data2.id - 1) % 10)
            return (data1.id - 1) % 10 < (data2.id - 1) % 10 ? 1 : -1;

        if (data1.charIdType != data2.charIdType)
            return data1.charIdType < data2.charIdType ? 1 : -1;

        var sdata1 = (HeroSData)c1.SData;
        var sdata2 = (HeroSData)c2.SData;

        return sdata1.GetPower() < sdata2.GetPower() ? -1 : 1;

        // 강화 -- 데이터 테이블이 없어서 패스
    }
    //랭크 오름차순 정렬
    private int HeroSortByRankDecending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        if (data1.rank != data2.rank)
            return data1.rank < data2.rank ? 1 : -1;

        if ((data1.id - 1) % 10 != (data2.id - 1) % 10)
            return (data1.id - 1) % 10 > (data2.id - 1) % 10 ? 1 : -1;

        if (data1.charIdType != data2.charIdType)
            return data1.charIdType > data2.charIdType ? 1 : -1;

        var sdata1 = (HeroSData)c1.SData;
        var sdata2 = (HeroSData)c2.SData;

        return sdata1.GetPower() < sdata2.GetPower() ? -1 : 1;

        // 강화 -- 데이터 테이블이 없어서 패스
    }
    //이름 내림차순 정렬
    private int HeroSortByNameAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        List<string> testNameSort = new List<string>();
        testNameSort.Add(data1.name);
        testNameSort.Add(data2.name);
        testNameSort.Sort();
        testNameSort.Reverse();
        return testNameSort[0] == data1.name ? 1 : -1;

    }
    //이름 오름차순 정렬
    private int HeroSortByNameDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        List<string> testNameSort = new List<string>();
        testNameSort.Add(data1.name);
        testNameSort.Add(data2.name);
        testNameSort.Sort();
        return testNameSort[0] == data1.name ? 1 : -1;

    }
    //획득 내림차순 정렬
    private int HeroSortByGetAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        var sData1 = c1.SData as CardSData;
        var sData2 = c2.SData as CardSData;

        return sData1.uid < sData2.uid ? -1 : 1;

        // 강화 -- 데이터 테이블이 없어서 패스
    }
    //획득 오름차순 정렬
    private int HeroSortByGetDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        var sData1 = c1.SData as CardSData;
        var sData2 = c2.SData as CardSData;

        return sData1.uid > sData2.uid ? -1 : 1;

        // 강화 -- 데이터 테이블이 없어서 패스
    }
    //공격력 내림차순 정렬
    private int HeroSortByPowerAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        var sdata1 = (HeroSData)c1.SData;
        var sdata2 = (HeroSData)c2.SData;

        return sdata1.GetPower() < sdata2.GetPower() ? -1 : 1;

        // 강화 -- 데이터 테이블이 없어서 패스
    }
    //공격력 오름차순 정렬
    private int HeroSortByPowerDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = c1.Data as UnitDataMap;
        var data2 = c2.Data as UnitDataMap;

        if (data1 == null) return 1;
        if (data2 == null) return -1;

        var sdata1 = (HeroSData)c1.SData;
        var sdata2 = (HeroSData)c2.SData;

        return sdata1.GetPower() < sdata2.GetPower() ? -1 : 1;

    }
    #endregion

    #region 아이템 정렬방식.
    //아이템 랭크 내림차순 정렬
    private int ItemSortByRankAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = c1.Data as ItemDataMap;
        var data2 = c2.Data as ItemDataMap;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        var p1 = data1.GetDefPower();
        var p2 = data2.GetDefPower();
        if (p1 != p2)
            return p2.CompareTo(p1);

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;
        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);

    }
    //아이템 랭크 오름차순 정렬
    private int ItemSortByRankDecending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        if (data1.rank != data2.rank)
            return data1.rank < data2.rank ? 1 : -1;

        var p1 = data1.GetDefPower();
        var p2 = data2.GetDefPower();
        if (p1 != p2)
            return p2.CompareTo(p1);

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;
        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);
    }
    //아이템 공격력 내림차순 정렬
    private int ItemSortByPowerAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        if (sdata1.GetPower(false) != sdata2.GetPower(false))
        {
            return sdata1.GetPower(false) < sdata2.GetPower(false) ? 1 : -1;
        }

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        var p1 = data1.GetDefPower();
        var p2 = data2.GetDefPower();
        if (p1 != p2)
            return p2.CompareTo(p1);

        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);
    }
    //아이템 공격력 오름차순 정렬
    private int ItemSortByPowerDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        if (sdata1.GetPower(false) != sdata2.GetPower(false))
        {
            return sdata1.GetPower(false) > sdata2.GetPower(false) ? 1 : -1;
        }

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        var p1 = data1.GetDefPower();
        var p2 = data2.GetDefPower();
        if (p1 != p2)
            return p2.CompareTo(p1);

        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);
    }
    //아이템 이름 내림차순 정렬
    private int ItemSortByNameAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        List<string> testItemNameSort = new List<string>();
        testItemNameSort.Add(data1.name);
        testItemNameSort.Add(data2.name);
        testItemNameSort.Sort();
        if (testItemNameSort[0] != testItemNameSort[1])
            return testItemNameSort[0] == data1.name ? 1 : -1;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);

    }
    //아이템 이름 오름차순 정렬
    private int ItemSortByNameDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = (ItemDataMap)c1.Data;
        var data2 = (ItemDataMap)c2.Data;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        List<string> testItemNameSort = new List<string>();
        testItemNameSort.Add(data1.name);
        testItemNameSort.Add(data2.name);
        testItemNameSort.Sort();
        testItemNameSort.Reverse();

        if (testItemNameSort[0] != testItemNameSort[1])
            return testItemNameSort[0] == data1.name ? 1 : -1;

        if (data1.rank != data2.rank)
            return data1.rank > data2.rank ? 1 : -1;

        if (sdata1.prefixIdx != sdata2.prefixIdx)
            return sdata1.prefixIdx.CompareTo(sdata2.prefixIdx);

        return sdata2.prefixValue.CompareTo(sdata1.prefixValue);
    }
    //아이템 획득 내림차순 정렬
    private int ItemSortByGetAscending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = c1.Data as ItemDataMap;
        var data2 = c2.Data as ItemDataMap;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        return sdata1.uid < sdata2.uid ? 1 : -1;
    }
    //아이템 획득 내림차순 정렬
    private int ItemSortByGetDescending(Transform _1, Transform _2)
    {
        var c1 = _1.GetComponent<CardBase>();
        var c2 = _2.GetComponent<CardBase>();

        if (c1.State != c2.State)
            return c1.State < c2.State ? 1 : -1;

        var data1 = c1.Data as ItemDataMap;
        var data2 = c2.Data as ItemDataMap;

        var sdata1 = (ItemSData)c1.SData;
        var sdata2 = (ItemSData)c2.SData;

        return sdata1.uid > sdata2.uid ? 1 : -1;

    }
    #endregion

    // Todo : Sort callback

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //    {
    //        listRootTable[TypeFlag.Character].SetSortCallBack(SortTest1);
    //        listRootTable[TypeFlag.Character].DoFilter(1);
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha2))
    //    {
    //        listRootTable[TypeFlag.Character].SetSortCallBack(SortTest1);
    //        listRootTable[TypeFlag.Character].DoFilter(2);
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha3))
    //    {
    //        listRootTable[TypeFlag.Character].SetSortCallBack(SortTest1);
    //        listRootTable[TypeFlag.Character].DoFilter(4);
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha4))
    //    {
    //        listRootTable[TypeFlag.Character].SetSortCallBack(SortTest1);
    //        listRootTable[TypeFlag.Character].DoFilter(15);
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha5))
    //    {
    //        listRootTable[TypeFlag.Character].SetSortCallBack(SortTest2);
    //        listRootTable[TypeFlag.Character].DoFilter(15);
    //    }
    //}

}
