using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankEffectManager
{
    public static void CreatePVP(int grade, Transform parent)
    {
        if (grade < 7000005) return;
        var obj = (Resources.Load("UI/Components/RankEffect") as GameObject);
        //RankEffect rankEffect = GameObject.Instantiate(obj).GetComponent<RankEffect>();
        RankEffect rankEffect = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("UI/Components/RankEffect",parent).GetComponent< RankEffect>();
        rankEffect.Create(grade, parent, false);
    }
    public static void CreateGradeTest(int grade, Transform parent)
    {
        if (grade < 4) return;
        var obj = (Resources.Load("UI/Components/RankEffect") as GameObject);
        //RankEffect rankEffect = GameObject.Instantiate(obj).GetComponent<RankEffect>();
        RankEffect rankEffect = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("UI/Components/RankEffect", parent).GetComponent<RankEffect>();
        rankEffect.Create(grade, parent, true);
    }
}

public class RankEffect : MonoBehaviour {

    public UITexture rankGold;
    public List<UITexture> rankPlatinum;
    public GameObject rankMaster;
    public GameObject rankChamp;

    public void Create(int grade, Transform parent, bool isGradeTest)
    {
        if (isGradeTest == true)
            grade += 7000001;

        UISprite parentSprite = parent.GetComponent<UISprite>();

        Debug.Log(((float)parentSprite.width / 150f) + "(int)((float)parentSprite.width / 150f) : " + (int)((float)parentSprite.width / 150f));

        if(grade >= 7000005 && grade <= 7000006)
        {
            rankGold.width = (int)(rankGold.width * ((float)parentSprite.width / 150f));
            rankGold.height = (int)(rankGold.height * ((float)parentSprite.width / 150f));
            rankGold.gameObject.SetActive(true);
        }
        else if(grade <= 7000007)
        {
            for(int i = 0; i < rankPlatinum.Count; i++)
            {
                UITexture platinumTexture = rankPlatinum[i];
                platinumTexture.width = (int)(platinumTexture.width * ((float)parentSprite.width / 150f));
                platinumTexture.height = (int)(platinumTexture.height * ((float)parentSprite.width / 150f));
                platinumTexture.gameObject.SetActive(true);
            }
        }
        else if (grade <= 7000008)
        {
            rankMaster.transform.localScale = Vector3.one * (float)parentSprite.width / 150f;
            rankMaster.SetActive(true);
        }
        else if(grade == 7000009)
        {
            rankChamp.transform.localScale = Vector3.one * (float)parentSprite.width / 150f;
            rankChamp.SetActive(true);
        }
    }
}
