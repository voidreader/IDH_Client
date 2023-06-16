using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class BattleTSVEPanel
{
    internal GameObject objTeamSkill;
	UI2DSprite sprite;
    UILabel label;
    internal Animator animator;
	//UITweener[] rootUiTw;
	//UITweener[] childUiTw;

	internal BattleTSVEPanel(UI2DSprite _sprite, GameObject _objTeamSkill)
	{
		sprite = _sprite;
        objTeamSkill = _objTeamSkill;
        label = _objTeamSkill.transform.GetChild(0).GetChild(3).GetComponent<UILabel>();
        //rootUiTw = sprite.gameObject.GetComponents<UITweener>();
        //childUiTw = sprite.transform.GetChild(0).GetComponentsInChildren<UITweener>();

        //rootUiTw[0].onFinished.Add(new EventDelegate(() => sprite.gameObject.SetActive(false)));
        sprite.gameObject.SetActive(false);
        animator = objTeamSkill.GetComponent<Animator>();

    }

	internal void Play(int _spriteKey, float _delay, float _posX)
	{
		var unit = GameCore.Instance.DataMgr.GetUnitData(_spriteKey);
		GameCore.Instance.SetUISprite(sprite, unit.GetSkillSpriteKey());
		//var label = sprite.gameObject.GetComponentInChildren<UILabel>();
		if(label != null)		label.text = unit.name;
        sprite.gameObject.SetActive(true);
        objTeamSkill.transform.localPosition = Vector2.right * _posX;
        objTeamSkill.SetActive(true);
        //animator.gameObject.SetActive(true);

       
        
        /*
		for (int i = 0; i < rootUiTw.Length; i++)
		{
			if(rootUiTw[i] is TweenPosition)
			{
				((TweenPosition)rootUiTw[i]).from.x = _posX;
				((TweenPosition)rootUiTw[i]).to.x = _posX;
			}
			rootUiTw[i].delay = _delay / GameCore.timeScale;
			rootUiTw[i].duration = 2.5f / GameCore.timeScale;
			rootUiTw[i].ResetToBeginning();
			rootUiTw[i].PlayForward();
		}

		for (int i = 0; i < childUiTw.Length; i++)
		{
			childUiTw[i].delay = (_delay*1.5f + 0.3f) / GameCore.timeScale;
			childUiTw[i].duration = 0.8f / GameCore.timeScale;
			childUiTw[i].ResetToBeginning();
			childUiTw[i].PlayForward();
		}
        */
    }
}

public class BattleTeamSkillEffectAnimation : MonoBehaviour
{
	List<BattleTSVEPanel> panels;
    [SerializeField] UI2DSprite[] spritesInfo;
    [SerializeField] GameObject[] teamSkillObjects;
    //UITweener[] labelTw;
    [SerializeField] GameObject labelGo;
	[SerializeField] UILabel skillName;
    public bool IsDoneSkillEff;
    Action cb;


    internal void init(Action _cb)
	{
        cb = _cb;
		panels = new List<BattleTSVEPanel>();
		for (int i = 0; i < 5; i++)
		{
			panels.Add(new BattleTSVEPanel(spritesInfo[i], teamSkillObjects[i]));
            //spritesInfo[i].gameObject.SetActive(false);
            teamSkillObjects[i].SetActive(false);
        }
		labelGo = UnityCommonFunc.GetGameObjectByName(gameObject, "TSNameLabel");
		skillName = UnityCommonFunc.GetComponentByName<UILabel>(labelGo, "TSName");
        labelGo.SetActive(false);
        //labelTw = labelGo.GetComponents<UITweener>();
        //labelTw[0].onFinished.Add(new EventDelegate(() => { labelTw[0].gameObject.SetActive(false);  }));
        //labelTw[0].gameObject.SetActive(false);
    }
	
	internal void Play(string _TSName, int[] _activeUnitCharIDs)
	{
		skillName.text = _TSName;

		var cnt = -_activeUnitCharIDs.Length / 2f;
        labelGo.SetActive(true);
        for ( int i = 0; i < _activeUnitCharIDs.Length; ++i, cnt+=1)
			panels[i].Play(_activeUnitCharIDs[i], i * 0.1f, cnt * 220f + 110f);

        /*labelTw[0].gameObject.SetActive(true);
		for ( int i = 0; i < labelTw.Length; ++i )
		{
			labelTw[i].duration = 3f / GameCore.timeScale;
			labelTw[i].ResetToBeginning();
			labelTw[i].PlayForward();
		}
        */
        float length = panels[0].animator.runtimeAnimatorController.animationClips[0].length + 1f;
        StartCoroutine(GameCore.WaitForTime(length , () =>
        {
            for (int i = 0; i < _activeUnitCharIDs.Length; ++i, cnt += 1)
                panels[i].objTeamSkill.SetActive(false);
            labelGo.SetActive(false);
            cb();
        }));
        //StartCoroutine(EnforceSkillEffect(arrStr[nRnd]));
	}
    public void SetIsAnimationingFalse(out bool _isAnimationing)
    {
        _isAnimationing = false;
    }

   
	//internal void Update()
	//{
	//	if (Input.GetKeyDown(KeyCode.Space))
	//		Play("알흠다운 쪼합", new int[] { 1100001, 1100002, 1100004, 1100005 });
	//}
}
