using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using IDH.MyRoom;

public class MyRoomHeroBox : MonoBehaviour
{
    protected static Color[] colors = new Color[]
    {
        new Color32(246,  0,255,255), // SSS
		new Color32(  0,240,255,255),	// SS
		new Color32(255,234,  0,255),	// S
		new Color32(255,255,255,255),	// A
		new Color32(139, 88, 46,255), // B
    };

    public UISprite Illust;
    public UIGrid Stars;
    public UISprite Cover;
    public UISprite Selected;

    public MyRoomHeroObject TargetHero { get; private set; }
    private MyRoomSystemRefParameter Parameter { get; set; }
    private Action OnClickBoxCallBack { get; set; }

    public void Initialize(MyRoomSystemRefParameter parameter, MyRoomHeroObject target, Action onClickBoxCallBack)
    {
        TargetHero = target;
        Parameter = parameter;
        OnClickBoxCallBack = onClickBoxCallBack;
        UnitDataMap localData = (target.HeroData.LocalData as UnitDataMap);

        var spriteData = GameCore.Instance.DataMgr.GetSpriteData(localData.GetSmallCardSpriteKey());
        Illust.spriteName = spriteData.sprite_name;
        GameCore.Instance.ResourceMgr.GetObject<GameObject>(ABType.AB_Atlas, spriteData.atlas_id, (go) => { if (go != null) Illust.atlas = go.GetComponent<UIAtlas>(); });

        var cnt = localData.evolLvl;
        var starName = (cnt <= 5) ? "ICON_STAR_01_S" : "ICON_STAR_02_S";
        var emptyStarName = (cnt <= 5) ? "ICON_STAR_00_S" : "ICON_STAR_01_S";
        cnt = ((cnt - 1) % 5) + 1;
        var starCnt = Stars.transform.childCount;
        for (int i = 0; i < starCnt; ++i)
        {
            if (i < cnt)
            {
                Stars.transform.GetChild(i).GetComponent<UISprite>().spriteName = starName;
                Stars.transform.GetChild(i).GetComponent<UISprite>().color = Color.white;
                Stars.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                Stars.transform.GetChild(i).GetComponent<UISprite>().spriteName = emptyStarName;
                Stars.transform.GetChild(i).GetComponent<UISprite>().color = (cnt <= 5) ? new Color(0.6f, 0.6f, 1f) : Color.white;
                Stars.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        Stars.Reposition();

        // 커버 설정
        Cover.color = colors[localData.rank];

        Selected.enabled = false;
    }

    public void OnClickRemoveButton()
    {
        Parameter.Command.CmdMyRoomObjectRetrunToInventory.Invoke(TargetHero, true);
    }

    public void OnClickBox()
    {
        OnClickBoxCallBack.Invoke();
        Parameter.Command.CmdSelectObject(TargetHero);
        //Selected.enabled = true;
    }
}


