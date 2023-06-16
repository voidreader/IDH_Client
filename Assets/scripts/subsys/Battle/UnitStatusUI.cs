using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


//public struct Gauge
//{
//    //UISprite follow;
//    UISprite gauge;
//    float percent;

//    float cacheMax;

//    public Gauge(GameObject _go, float _cacheMax = -1f, float _percent = 1f)
//    {
//        //follow = UnityCommonFunc.GetComponentByName<UISprite>(_go, "follow");
//        gauge = UnityCommonFunc.GetComponentByName<UISprite>(_go, "gauge");
//        percent = _percent;
//        cacheMax = _cacheMax;

//        gauge.fillAmount = percent;
//    }

//    public void SetPercent(float _percent)
//    {
//        percent = _percent;
//        gauge.fillAmount = percent;
//    }

//    public void SetMaxValue(float _max)
//    {
//        cacheMax = _max;
//    }

//    public void SetValue(float _value)
//    {
//        percent = _value / cacheMax;
//        gauge.fillAmount = percent;
//    }


//    public float GetMaxValue()
//    {
//        return cacheMax;
//    }

//    public float GetValue()
//    {
//        return cacheMax * percent;
//    }

//    public float GetPercent()
//	{
//		return percent;
//	}

//	internal void SetColor(Color _clr)
//	{
//		gauge.color = _clr;
//	}
//}
internal struct BuffIconInfo
{
    internal int childPosInfo;
    internal int BuffCount;
    internal string iconName;
}
internal class BuffIcon
{
    internal int childPosInfo;
	internal UISprite sprite;
	internal UILabel text;

	internal int buffCnt; //버프 중첩 개수
	internal string iconName; // 버프 아이콘 스프라이트 아이디
}

/// <summary>
/// 배틀 중 캐릭터의 상태를 보여주는 UI  (이름, HP 등 )
/// </summary>
internal class UnitStatusUI :MonoBehaviour
{
	Transform cachedTf;
	UISlider HpBar;             // 캐릭터 HP 게이지바
    UISlider MentalBar;			// 캐릭터 HP 게이지바
	Transform buffRoot;         // 버프 아이콘 생성 기준 위치
    GameObject excessBuff;      // 특정 개수 이상 초과했을시 나오는 BuffIcon
    UIGrid buffGrid;			// 버프 그리드 정렬용

    UILabel lbTotalDmg;         // 총 데미지(레이드 보스 전용)
    List<BuffIcon> buffIcons;   // 버프 아이콘 데이터 배열
    List<BuffIconInfo> buffIconInfos;

    [SerializeField] BattleUnitData data;

    [SerializeField] float maxHp;
    [SerializeField] float maxMental;

    public bool isBoss { get; private set; }

	internal GameObject GetGameObject() { return gameObject; }

	internal static  UnitStatusUI Create(bool _enemy, BattleUnitData _data, Transform _parent = null)
	{
        GameObject go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/charStatus", _parent);
        
        var result = go.GetComponent<UnitStatusUI>();
        result.data = _data;
        result.isBoss = false;
        result.buffIcons= new List<BuffIcon>();
        result.buffIconInfos = new List<BuffIconInfo>();
        result.cachedTf = result.transform;

        result.buffRoot	= UnityCommonFunc.GetComponentByName<Transform>(go, "origineBuff");
        result.buffGrid = result.buffRoot.GetComponent<UIGrid>();

        result.excessBuff = UnityCommonFunc.GetGameObjectByName(go, "excessBuff");
        result.excessBuff.SetActive(false);
        //result.excessBuff = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/buffIcon", result.transform);
        //result.excessBuff.transform.localPosition = result.buffRoot.localPosition;
        //result.excessBuff.GetComponent<UISprite>().spriteName = "BUFF_04_00";

        result.HpBar = UnityCommonFunc.GetComponentByName<UISlider>(go, "hp");
        result.MentalBar = UnityCommonFunc.GetComponentByName<UISlider>(go, "mental");
        result.maxHp = _data.GetStat(UnitStat.Hp);
        result.maxMental = 100;
        //result.maxMental = _data.GetStat(UnitStat.Mental);
        result.SetHp(result.maxHp);
        result.SetMental(result.maxMental);

        if (_enemy)
		{
            UnityCommonFunc.GetComponentByName<UISprite>(result.HpBar.gameObject, "Foreground").color = Color.red;
			UnityCommonFunc.GetGameObjectByName(go, "mental").SetActive(false);
		}

        return result;
	}

    internal void InitForBoss(BattleUnitData _data, float _damage)
    {
        buffIcons = new List<BuffIcon>();
        cachedTf = transform;
        isBoss = true;

        buffRoot = UnityCommonFunc.GetComponentByName<Transform>(gameObject, "origineBuff");
        lbTotalDmg = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "lbTotalDmg");
        buffGrid = buffRoot.GetComponent<UIGrid>();

        HpBar = UnityCommonFunc.GetComponentByName<UISlider>(gameObject, "hp");
        MentalBar = UnityCommonFunc.GetComponentByName<UISlider>(gameObject, "mental");
        maxHp = _data.GetStat(UnitStat.Hp);
        maxMental = 100f;
        //maxMental = _data.GetStat(UnitStat.Mental);

        SetHp(maxHp - _damage);
        SetMental(maxMental);
    }

	internal void SetPosition(Vector3 _pos)//, Vector3 _height)
	{
		cachedTf.localPosition = _pos;//     +_height;
		//cachedTf.localPosition += _height;
	}

	internal void SetAvtive(bool _show)
	{
        if (!isBoss)
        {
            gameObject.SetActive(_show);
        }
        else
        {
            if(GameCore.Instance.TimeScaleChange < 3 || _show == true)
            //if(GameCore.in < 3 || _show == true)
                gameObject.SetActive(_show);
        }
	}

	internal void SetHp(float _nowHp)
	{
        var value = _nowHp / maxHp;

        if (isBoss)
        {
            var value1 = Mathf.Max(0, (value * 2) - 1);
            HpBar.Set(value1);
            MentalBar.Set((value * 2) - value1);

            lbTotalDmg.text = (maxHp - _nowHp).ToString("N0");
        }
        else
        {
            HpBar.Set(value);
        }
    }

	internal void ResetHp(float _now, float _max)
	{
        maxHp = _max;
        SetHp(_now);
	}

	internal void SetMental( float _nowMental)
	{
        if (!isBoss)
        {
            MentalBar.Set(_nowMental / maxMental);
        }
	}
	
	internal void ResetMantal( float _now, float _max)
	{
        if (!isBoss)
        {
            maxMental = _max;
            SetMental(_now);
        }
	}

	internal void SetDepth(int _depth)
	{
		gameObject.GetComponent<UIWidget>().depth = _depth;
	}


	internal void AddStatBuff(UnitStat _type, int value)
	{
        AddBuff(GetIconName(_type), value, (int)_type, true);

	}

	internal void SetStateBuff(EffectType _type, int value)
	{
        AddBuff(GetIconName(_type), value, (int)_type, false);
	}
    private void CheckBuffCount()
    {
        int limitExcess = 5;
        bool isExcess = (buffIcons.Count >= limitExcess);

        for (int i = 0; i < buffIcons.Count; ++i)
        {
            bool limitBoolCheck = isExcess ? (i < (limitExcess - 2)) : true;
            buffIcons[i].sprite.enabled = limitBoolCheck;
            buffIcons[i].text.enabled = limitBoolCheck;
        }
        Vector3 excessBuffPos = buffRoot.localPosition;
        if(isExcess == true)
        {
            excessBuffPos.x += 26 * (limitExcess - 2);
            excessBuff.transform.localPosition = excessBuffPos;
        }
        excessBuff.SetActive(isExcess);
    }
	internal void AddBuff(string _buffName, int value, int childPosInfo, bool _added = true)
	{
		for (int i = 0; i < buffIcons.Count; ++i)
		{
			if (buffIcons[i].iconName == _buffName)
			{
				if (_added)		buffIcons[i].buffCnt += value;
				else			buffIcons[i].buffCnt = value;

				if (buffIcons[i].buffCnt > 0)
				{
					buffIcons[i].sprite.spriteName = _buffName + "_1";
					buffIcons[i].text.text = Mathf.Abs(buffIcons[i].buffCnt).ToString("0");
                    buffIcons[i].childPosInfo = childPosInfo + (int)UnitStat.Count;

                }
				else if (buffIcons[i].buffCnt < 0)
				{
					buffIcons[i].sprite.spriteName = _buffName + "_2";
					buffIcons[i].text.text = Mathf.Abs(buffIcons[i].buffCnt).ToString("0");
                    buffIcons[i].childPosInfo = childPosInfo;
                }
				else
				{
					GameObject.Destroy(buffIcons[i].sprite.gameObject);
					buffIcons.RemoveAt(i);
                    buffGrid.enabled = true;
				}
                SortBuffInfo();
                CheckBuffCount();
                return;
			}
		}

		var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Battle/buffIcon", buffRoot);
		BuffIcon icon = new BuffIcon();
		icon.sprite = go.GetComponent<UISprite>();
		icon.text = go.GetComponentInChildren<UILabel>();
		icon.buffCnt = value;
		icon.iconName = _buffName;
        icon.childPosInfo = childPosInfo;

        buffIcons.Add(icon);

        //var spriteData = GameCore.Instance.DataMgr.GetSpriteData(_buffName);
        //icon.sprite.atlas = spriteData.atlas_id;
        icon.sprite.spriteName = _buffName + ((icon.buffCnt > 0) ? "_1" : "_2");
		icon.text.text = Mathf.Abs(icon.buffCnt).ToString("0");

        buffGrid.enabled = true;
        SortBuffInfo();
        CheckBuffCount();

    }
    internal void SortBuffInfo()
    {
        //List<BuffIconInfo> buffIconInfos = new List<BuffIconInfo>();
        for (int i = 0; i < buffIcons.Count; i ++)
        {
            BuffIconInfo buffIconInfo = new BuffIconInfo();
            buffIconInfo.iconName = buffIcons[i].iconName;
            buffIconInfo.BuffCount = buffIcons[i].buffCnt;
            buffIconInfo.childPosInfo = buffIcons[i].childPosInfo;
            buffIconInfos.Add(buffIconInfo);
        }

        var sortList = from sortData in buffIconInfos
                       orderby sortData.childPosInfo
                       select sortData;
        int j = 0;
        foreach(var sortData in sortList)
        {
            string IconName = sortData.iconName;
            int buffCount = sortData.BuffCount;
            buffIcons[j].childPosInfo = sortData.childPosInfo;
            buffIcons[j].iconName = IconName;
            buffIcons[j].buffCnt = buffCount;
            buffIcons[j].sprite.spriteName = IconName + ((buffCount > 0)? "_1" : "_2");
            buffIcons[j].text.text = Mathf.Abs(buffCount).ToString("0");
            j++;
        }
        buffIconInfos.Clear();
        
    }

	internal string GetIconName(UnitStat _type)
	{
		return _type.ToString().ToLower();
	}

	internal string GetIconName(EffectType _type)
	{
		return _type.ToString().ToLower();
	}
}
