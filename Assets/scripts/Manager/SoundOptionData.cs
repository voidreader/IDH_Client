using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOptionData : MonoBehaviour {
    [SerializeField] UISlider[] uiSliders;
    [SerializeField] UIButton[] uiButtonBackGround;

    [SerializeField] UIButton[] uiButtonVolumeMax;
    [SerializeField] UIButton[] uiButtonVolumeMin;
    public UISlider[] GetSoundSliderArray
    {
        get { return uiSliders; }
    }
    public void SetBackGroundButton()
    {
        SetButton(uiButtonBackGround);
    }
    public void SetVolumeButton()
    {
        uiButtonVolumeMax[0].onClick.Add(new EventDelegate(() =>
       {
           SetVolumeMax(0);
       }));
        uiButtonVolumeMax[1].onClick.Add(new EventDelegate(() =>
        {
            SetVolumeMax(1);
        }));
        uiButtonVolumeMax[2].onClick.Add(new EventDelegate(() =>
        {
            SetVolumeMax(2);
        }));
        uiButtonVolumeMin[0].onClick.Add(new EventDelegate(() =>
        {
            SetVolumeMin(0);
        }));
        uiButtonVolumeMin[1].onClick.Add(new EventDelegate(() =>
        {
            SetVolumeMin(1);
        }));
        uiButtonVolumeMin[2].onClick.Add(new EventDelegate(() =>
        {
            SetVolumeMin(2);
        }));

    }
    public void SetVolumeMax(int i)
    {
        uiSliders[i].value = 1f;
    }
    public void SetVolumeMin(int i)
    {
        uiSliders[i].value = 0f;
    }
    private void SetButton(UIButton[] button)
    {
        button[0].onClick.Add(new EventDelegate(() =>
        {
            ChangeVolume(0);
        }));
        button[1].onClick.Add(new EventDelegate(() =>
        {
            ChangeVolume(1);
        }));
        button[2].onClick.Add(new EventDelegate(() =>
        {
            ChangeVolume(2);
        }));
    }
    public void ChangeVolume(int i)
    {
        Vector2 cameraPos = UICamera.lastEventPosition;
        float pos_X = cameraPos.x;
        pos_X -= 435f;
        pos_X /= 540f;
        uiSliders[i].value = pos_X;
    }
}
