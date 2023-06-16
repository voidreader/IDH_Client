using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroInfoRatingWindow : MonoBehaviour
{
    public static HeroInfoRatingWindow Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("HeroInfo/RatingWindow", _parent);
        return go.GetComponent<HeroInfoRatingWindow>();
    }

    [SerializeField] UIInput input;
    [SerializeField] UILabel lbTextCount;
    [SerializeField] GameObject goGuideInput;

    [SerializeField] UIButton[] btRatings;
    [SerializeField] UISprite[] spRatingCheckBoxes;

    int maxChar = 140;

    int selectedIndex = 0;
    public int SelectedRate { get { return selectedIndex + 1; } }

    private void Awake()
    {
        for (int i = 0; i < 5; ++i)
        {
            int n = i;
            btRatings[i].onClick.Add(new EventDelegate(()=> OnToggleRate(n)));
        }

        input.onSelect.Add(new EventDelegate(() => goGuideInput.SetActive(false) ));
        input.onDeselect.Add(new EventDelegate(()=> goGuideInput.SetActive(input.value.Length == 0) ));
        ChangeInput();
    }

    public string GetText()
    {
        return input.value;
    }

    public void SetText(string _text)
    {
        input.value = _text;
        goGuideInput.SetActive(_text.Length == 0);
    }

    public void ChangeInput()
    {
        var length = input.value.Length;
        //Debug.Log("ChangeInput. Length:" + length + "   " + input.value);
        //goGuideInput.SetActive(length == 0);
        if (length > maxChar)
        {
            input.value = input.value.Substring(0, maxChar);
            return;
        }

        lbTextCount.text = string.Format("{0}/{1}", length, maxChar);
    }

    public void OnToggleRate(int _idx)
    {
        spRatingCheckBoxes[selectedIndex].color = new Color32(0x89, 0x89, 0x89, 0xFF);
        spRatingCheckBoxes[_idx].color = new Color32(0xF6, 0x00, 0xFF, 0xFF);

        selectedIndex = _idx;
    }
}
