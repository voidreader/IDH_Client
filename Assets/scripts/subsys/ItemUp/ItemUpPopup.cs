using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemUpPopup : MonoBehaviour 
{
    internal static ItemUpPopup Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("ItemUp/ItemUpPopup", _parent);
        var result = go.GetComponent<ItemUpPopup>();
        return result;
    }

    [SerializeField] Transform tfCardRoot;
    [SerializeField] UISprite spOptionBG;
    [SerializeField] UILabel[] lbNames;
    [SerializeField] UILabel[] lbValues;
    [SerializeField] UILabel[] lbAdds;

    [SerializeField] UISprite spCardGlow;
    [SerializeField] UISprite spCardFrame;
    [SerializeField] GameObject goBG;


    bool[] types; // 옵션이 비율이라면 true, 절대값이라면 false, 0 : 접두어옵션
    float[] prevValue;
    float[] addValue;

    internal void Init(ItemSData _itemSData, ItemSData _prevItemSData)
    {
        CardBase.CreateBigCard(_itemSData, tfCardRoot);
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Strengthen_Popup);

        var prevPercentStat = _prevItemSData.GetStat(true);
        var nowPercentStat = _itemSData.GetStat(true);
        var prevConstStat = _prevItemSData.GetStat(false);
        var nowConstStat = _itemSData.GetStat(false);

        int idx = 0;

        types = new bool[1 + _itemSData.optionIdx.Length];
        prevValue = new float[types.Length];
        addValue = new float[types.Length];

        // 접두어 옵션
        var peData = GameCore.Instance.DataMgr.GetItemEffectData(_itemSData.prefixIdx);
        if (peData != null)
        {
            types[idx] = peData.type == 1;
            prevValue[idx] = _prevItemSData.prefixValue;
            addValue[idx] = _itemSData.prefixValue - _prevItemSData.prefixValue;

            lbNames[idx].text = ItemSData.GetItemEffectString((ItemEffectType)_itemSData.prefixIdx);
            if (types[idx])
            {
                lbValues[idx].text = string.Format("{0:0.#}{1}", prevValue[idx], "%");
                lbAdds[idx].text = string.Format("{0:+0.#; -#.#}{1}", addValue[idx], "%");
            }
            else
            {
                lbValues[idx].text = string.Format("{0:0}{1}", Mathf.RoundToInt(prevValue[idx]), "  ");
                lbAdds[idx].text = string.Format("{0:+0; -#}{1}", Mathf.RoundToInt(addValue[idx]), "  ");
            }

            ++idx;
        }

        // 옵션
        for (int i = 0; i < _itemSData.optionIdx.Length; ++i)
        {
            if (_itemSData.optionIdx[i] <= 0)
                continue;

            var eData = GameCore.Instance.DataMgr.GetItemEffectData(_itemSData.optionIdx[i]);
            if (eData == null)
                continue;

            types[idx] = eData.type == 1;
            prevValue[idx] = _prevItemSData.optionValue[i];
            addValue[idx] = _itemSData.optionValue[i] - _prevItemSData.optionValue[i];

            lbNames[idx].text = ItemSData.GetItemEffectString((ItemEffectType)eData.effectType);
            lbValues[idx].text = string.Format("{0:0}{1}", Mathf.RoundToInt(prevValue[idx]), types[idx] ? "%" : "  ");
            lbAdds[idx].text = string.Format("{0:+0; -#}{1}", Mathf.RoundToInt(addValue[idx]), types[idx] ? "%" : "  ");

            ++idx;
        }

        spOptionBG.height = idx * 30 + 10;

        for(; idx < lbNames.Length; ++idx)
        {
            lbNames[idx].text = string.Empty;
            lbValues[idx].text = string.Empty;
            lbAdds[idx].text = string.Empty;

            lbNames[idx].gameObject.SetActive(false);
        }

        StartCoroutine(CoIncStatValue(1f));
    }

    public void StopParticle()
    {
        goBG.transform.parent = GameCore.Instance.Ui_root;
        var particles = goBG.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
            ps.Stop();

        var go = goBG;
        GameCore.Instance.DoWaitCall(5f, () => { Destroy(goBG); });
    }

    IEnumerator CoIncStatValue(float _time)
    {
        float acc = 0f;

        yield return new WaitForSeconds(3f);

        while (acc < _time)
        {
            yield return null;

            acc += Time.unscaledDeltaTime;
            for (int i = 0; i < lbAdds.Length; ++i)
            {
                var bytes = System.Text.Encoding.ASCII.GetBytes(lbValues[i].text);
                for (int j = 0; j < bytes.Length; ++j)
                {
                    if ('0' <= bytes[j] && bytes[j] <= '9')
                        bytes[j] = (byte)(((bytes[j] + 1) % 10) + 48);
                }

                lbValues[i].text = System.Text.Encoding.Default.GetString(bytes);
                lbAdds[i].text = "";
            }
        }

        for (int i = 0; i < lbAdds.Length; ++i)
        {
            if (types[i])
            {
                lbValues[i].text = string.Format("{0:0.#}{1}", prevValue[i] + addValue[i], "%");
            }
            else
            {
                lbValues[i].text = string.Format("{0:0}{1}", Mathf.RoundToInt(prevValue[i] + addValue[i]), "  ");
            }
        }
    }
}
