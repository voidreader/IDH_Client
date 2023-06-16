using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendSkillUI : MonoBehaviour {
    [SerializeField] UILabel label;
    [SerializeField] UI2DSprite[] spriteArray;
    [SerializeField] Animator animator;
    public Animator Animator { get { return animator; } }
    public void SetSpriteArray(int iconNumber)
    {
        GameCore.Instance.SetUISprite(spriteArray[0], iconNumber);
        GameCore.Instance.SetUISprite(spriteArray[1], iconNumber);
    }
    public void SetLabel(string userName, string userSkill)
    {
        label.text = "[b][F600FF]" + userName + "[-]의 팀 / [24FF00]" + userSkill + "[-][/b]";
    }
}
