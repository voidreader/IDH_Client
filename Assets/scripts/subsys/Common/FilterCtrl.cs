using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterCtrl : MonoBehaviour
{
	GameObject[] type_root;
	FilterItem[] spToggles;

	int prevFilterFalg;
	int filterFlag;

	private void Awake()
	{
		type_root = new GameObject[4];
		spToggles = new FilterItem[20];
		prevFilterFalg = 0;
		filterFlag = 0;
		int idx = 0;
		type_root[0] = UnityCommonFunc.GetGameObjectByName(gameObject, "rank_root");
		type_root[1] = UnityCommonFunc.GetGameObjectByName(gameObject, "type_root");
		type_root[2] = UnityCommonFunc.GetGameObjectByName(gameObject, "rare_root1");
		type_root[3] = UnityCommonFunc.GetGameObjectByName(gameObject, "rare_root2");
		for (int i = 0; i < type_root.Length; ++i)
		{
			if (type_root[i] == null)
				continue;

			if (i == 1)
			{
				for (int j = 0; j < 5; ++j)
				{
					spToggles[idx] = UnityCommonFunc.GetComponentByName<FilterItem>(type_root[i], "item" + (j+1));
					spToggles[idx].Init(idx, CBToggleFilter);
					spToggles[idx++].SetToggle(false);
				}
			}
			else
			{
				for (int j = 5; j > 0; --j)
				{
					spToggles[idx] = UnityCommonFunc.GetComponentByName<FilterItem>(type_root[i], "item" + j);
					spToggles[idx].Init(idx, CBToggleFilter);
					spToggles[idx++].SetToggle(false);
				}
			}
		}
	}

    internal void Reset()
    {
        for (int i = 0; i < spToggles.Length; ++i)
            if(spToggles[i] != null)
                spToggles[i].SetToggle(false);
        prevFilterFalg = filterFlag = 0;

    }

    internal void ChangeReverse()
	{
		filterFlag = prevFilterFalg;

		for (int i = 0; i < 20; ++i)
		{
			var flag = 1 << i;
			spToggles[i].SetToggle((filterFlag & flag) != 0);
		}
	}

    // 1을 _flag만큼 시프트 연산한걸 대입연산자로 filterFlag를 대입하고 _flag번째의 spTogles의 토클 여부를 filterFlag와 1을 _flag만큼 시프트 연산한 걸 And 연산 후 이게 0이 아니라면 true를 만들어라.
    private void CBToggleFilter(int _flag)
	{
		filterFlag ^= 1 << _flag;
		spToggles[_flag].SetToggle((filterFlag & (1 << _flag)) != 0);
	}


	internal void SetPrevFilter()
	{
		prevFilterFalg = filterFlag;
	}

	internal int GetFilter()
	{
		return filterFlag;
	}

	internal int GetFilterRank() {	return (filterFlag & 0x1F << 0) >> 0;	}
	internal static int GetFilterRank(int _filter) { return (_filter & 0x1F << 0) >> 0; }

	internal int GetFilterType()	{	return (filterFlag & 0x1F << 5) >> 5;	}
	internal static int GetFilterType(int _filter) { return (_filter & 0x1F << 5) >> 5; }

	internal int GetFilterRare()	{	return (filterFlag & 0x2FF << 10) >> 10;	}
	internal static int GetFilterRare(int _filter) { return (_filter & 0x2FF << 10) >> 10; }

}