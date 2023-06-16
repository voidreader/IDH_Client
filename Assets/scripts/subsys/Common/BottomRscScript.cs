using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomRscScript : MonoBehaviour
{
	UISprite btnSprite;
	UITweener[] tws;

	UILabel[] lbCounts;
	int[] countCache;

	bool bShow;

	internal static BottomRscScript Create(Transform _parent)
	{
		var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("CommonRsc/Prefab/resourceRoot", _parent);
		var result = go.GetComponent<BottomRscScript>();
        go.GetComponent<UIAnchor>().container = GameCore.Instance.ui_root.gameObject;
        result.InitLink();
		return result;
	}

	internal void InitLink()
	{
		var btn = UnityCommonFunc.GetComponentByName<UIButton>(gameObject, "btnRsc");
		btnSprite = btn.GetComponent<UISprite>();
		btn.onClick.Add(new EventDelegate(onClickButton));
		var rscRoot = UnityCommonFunc.GetGameObjectByName(gameObject, "Rsc_Root");
		tws = new UITweener[] {
			btn.GetComponent<UITweener>(),
			rscRoot.GetComponent<UITweener>(),
            UnityCommonFunc.GetComponentByName<UITweener>(btn.gameObject, "Label")
        };

		countCache = new int[7];
		lbCounts = new UILabel[7];
		for(int i = 0; i < lbCounts.Length; ++i)
			lbCounts[i] = UnityCommonFunc.GetComponentByName<UILabel>(gameObject, "rsc"+(i+1)+"lb");


        bShow = true;

        for (int i = 0; i < tws.Length; ++i)
        {
            tws[i].ResetToBeginning();
            tws[i].PlayForward();
        }
	}

	internal void onClickButton()
	{
		bShow = !bShow;

		if (!bShow)
		{
			btnSprite.spriteName = "BTN_08_01_02";
			for (int i = 0; i < tws.Length; ++i)
				tws[i].PlayReverse();
		}
		else
		{
			btnSprite.spriteName = "BTN_08_01_01";
			for (int i = 0; i < tws.Length; ++i)
				tws[i].PlayForward();
		}
	}

	internal void UpdateCount()
	{
		for(int i = 0; i < lbCounts.Length; ++i)
		{
			var count = GameCore.Instance.PlayerDataMgr.GetReousrceCount(ResourceType.Coin1 + i);
			lbCounts[i].text = "x " + count.ToString("N0");
			countCache[i] = count;
		}
	}
}
