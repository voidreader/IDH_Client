using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class ClickSound : MonoBehaviour
{
    //internal SFX sound = SFX.UI_Button;

    void OnClick()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(sound);
    }
}
