using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopAttentionComponent : MonoBehaviour {

    public static ShopAttentionComponent Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Shop/ShopAttention", _parent);
        var result = go.GetComponent<ShopAttentionComponent>();
        return result;
    }

    [SerializeField]    UILabel text;


    internal void Init(string Text)
    {
        text.text = Text;
    }
}
