using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class HeroUpPopup : MonoBehaviour
{
    internal static HeroUpPopup Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("HeroUp/HeroUpPopup", _parent);
        var result = go.GetComponent<HeroUpPopup>();
        return result;
    }

    [SerializeField] Transform tfCardRoot;
    [SerializeField] UILabel[] lbNames;
    [SerializeField] UILabel[] lbValues;
    [SerializeField] UILabel[] lbAdds;


    [SerializeField] UISprite spCardGlow;
    [SerializeField] UISprite spCardFrame;
    [SerializeField] GameObject goStarEffect;
    [SerializeField] UISprite spStarEffect;
    [SerializeField] GameObject goBG;

    bool isEvol = false;

    internal void Init(HeroSData _unitSData, HeroSData _prevSData)
    {
        var card = CardBase.CreateBigCard(_unitSData, tfCardRoot) as HeroCardBig;

        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_Strengthen_Popup);

        if (_unitSData.key != _prevSData.key) // 진화일 때
        {
            isEvol = true;
            var data = GameCore.Instance.DataMgr.GetUnitData(_unitSData.key);

            var pos = (data.evolLvl - 1) % 5;
            spStarEffect.spriteName = string.Format("ICON_STAR_{0:00}_S", data.evolLvl <= 5 ? 1 : 2);
            goStarEffect.transform.localPosition = new Vector3(19 + (pos * 25), -155, 0);

            card.SetStar(data.evolLvl - 1);
        }


        var prevBaseStat = _prevSData.GetBaseStat(_prevSData.enchant);
        var nowBaseStat = _unitSData.GetBaseStat(_unitSData.enchant);

        var gapStat = nowBaseStat.SubStat(prevBaseStat);

        //var prevBaseStat = _unitSData.GetBaseStat(_unitSData.enchant - 1);
        //var nowBaseStat = _unitSData.GetBaseStat();

        for (int i = 0; i < lbNames.Length; ++i)
        {
            var st = (UnitStat)(i + 1);
            lbNames[i].text = Stat.GetStatName(st);
            lbValues[i].text = Mathf.RoundToInt(prevBaseStat.GetStat(st)).ToString("N0");
            lbAdds[i].text = Mathf.RoundToInt(gapStat.GetStat(st)).ToString("+0;-#"); // string.Format("{ 0:+0;-#}", nowBaseStat.GetStat(st) - prevBaseStat.GetStat(st));
        }

        StartCoroutine(CoIncStatValue(1f, prevBaseStat, gapStat));
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

    IEnumerator CoIncStatValue(float _time, Stat _prevBaseStat, Stat _addedStat)
    {
        float acc = 0f;

        yield return new WaitForSeconds(3f);

        while(acc < _time)
        {
            yield return null;

            acc += Time.unscaledDeltaTime;
            for (int i = 0; i < lbAdds.Length; ++i)
            {
                var bytes = System.Text.Encoding.ASCII.GetBytes(lbValues[i].text);
                for (int j = 0; j < bytes.Length; ++j)
                    bytes[j] = (byte)(((bytes[j]+1) % 10) + 48);

                lbValues[i].text = System.Text.Encoding.Default.GetString(bytes);
                lbAdds[i].text = "";
            }
        }

        for (int i = 0; i < lbAdds.Length; ++i)
        {
            var value = _addedStat.GetStat((UnitStat)i + 1);
            var now = Mathf.RoundToInt(_prevBaseStat.GetStat((UnitStat)i + 1) + value);

            lbValues[i].text = now.ToString("N0");
        }

        if (isEvol)
            goStarEffect.SetActive(true);
    }

}
