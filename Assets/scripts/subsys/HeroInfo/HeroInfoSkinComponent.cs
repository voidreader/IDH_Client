using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroInfoSkinComponent : MonoBehaviour
{
    [SerializeField] GameObject goDraggableCameraScrollView;
    [SerializeField] UIDraggableCamera draggableCamera;
    [SerializeField] UIGrid ListRoot;
    [SerializeField] Transform tfFixedItemRoot;

    List<HeroInfoSkinListItem> items = new List<HeroInfoSkinListItem>();
    HeroInfoSkinListItem fixedItem;
    System.Action<int> cbChangeIllust;

    HeroInfoSkinListItem cachedPreviewItem;

    private void OnEnable()
    {
        goDraggableCameraScrollView.SetActive(true);
    }
    private void OnDisable()
    {
        goDraggableCameraScrollView.SetActive(false);
    }

    internal void Init(HeroInfoSkinListItem.Data[] _items, System.Action<int> _cbChangeIllust)
    {
        cbChangeIllust = _cbChangeIllust;
        for (int i = 0; i < _items.Length; ++i)
        {
            var item = HeroInfoSkinListItem.Create(ListRoot.transform);
            item.Init(_items[i], draggableCamera, CBPreview, CBPrice);
            items.Add(item);
        }

        tfFixedItemRoot.gameObject.SetActive(false);
    }

    internal void Init(HeroInfoSkinListItem.Data[] _items, HeroInfoSkinListItem.Data _fixedItem, System.DateTime _time, System.Action<int> _cbChangeIllust)
    {
        cbChangeIllust = _cbChangeIllust;
        var dummy = HeroInfoSkinListItem.Create(ListRoot.transform);
        dummy.gameObject.SetActive(false);
        items.Add(dummy);

        for (int i = 0; i < _items.Length; ++i)
        {
            var item = HeroInfoSkinListItem.Create(ListRoot.transform);
            item.Init(_items[i], draggableCamera, CBPreview, CBPrice);
            items.Add(item);
        }

        tfFixedItemRoot.gameObject.SetActive(true);

        var fItem = HeroInfoSkinListItem.Create(tfFixedItemRoot);
        fItem.Init(_fixedItem, draggableCamera, CBPreview, CBPrice, _time, CBTimeOut);
        fixedItem = fItem;
    }

    void CBTimeOut()
    {
        if (fixedItem != null)
            Destroy(fixedItem.gameObject);
        tfFixedItemRoot.gameObject.SetActive(false);

        Destroy(items[0].gameObject);
        items.RemoveAt(0);
    }

    void CBPreview(HeroInfoSkinListItem _item)
    {
        if (cachedPreviewItem != null)
        {
            cachedPreviewItem.bPreview = false;
            cachedPreviewItem.UpdatePreviewButton();
        }
        if (cachedPreviewItem != _item)
        {
            _item.bPreview = true;
            cbChangeIllust(_item.GetData().illustKey);
            cachedPreviewItem = _item;
        }
        else
        {
            cbChangeIllust(-1);
            cachedPreviewItem = null;
        }
    }

    void CBPrice(HeroInfoSkinListItem _item)
    {

    }

    void CBSetSkin(HeroInfoSkinListItem _item)
    {
        for(int i = 0; i < items.Count; ++i)
        {
            if (items[i].GetState() == HeroInfoSkinListItem.State.Equipped)
            {
                items[i].SetState(HeroInfoSkinListItem.State.bought);
                break;
            }
        }
        _item.SetState(HeroInfoSkinListItem.State.Equipped);
    }
}
