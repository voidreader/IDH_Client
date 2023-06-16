using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroInfoUIAnimation : MonoBehaviour
{
    [SerializeField] TweenPosition twTab;
    [SerializeField] TweenPosition twIllust;

    bool bInit;

    public void Init()
    {
        if (!bInit)
        {
            if(twTab.enabled || twTab.enabled)
            {
                Debug.LogError("Enabled tweener even if not init");
                return;
            }

            twTab.from = twTab.transform.localPosition;
            twTab.to = twTab.transform.localPosition + new Vector3(-150, 0, 0);

            twIllust.from = twIllust.transform.localPosition;
            twIllust.to = twIllust.transform.localPosition + new Vector3(-30, 0, 0);

            twTab.ResetToBeginning();
            twIllust.ResetToBeginning();

            bInit = true;
        }
    }

    public void PlayForward()
    {
        if(!bInit) Init();

        twTab.PlayForward();
        twIllust.PlayForward();
    }

    public void PlayerReverse()
    {
        if (!bInit) Init();

        twTab.PlayReverse();
        twIllust.PlayReverse();
    }
}
