using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;
using System;

public class GameManager : MonoBehaviour {
	public TextMesh text_fx_name;
	public GameObject[] fx_prefabs;
	public int index_fx = 0;
	private Ray ray;
	private RaycastHit2D ray_cast_hit;

	public SkeletonAnimation ctrl;
	bool bHit;
	void Start () 
	{
		text_fx_name.text = "[" + (index_fx + 1) + "] " + fx_prefabs[ index_fx ].name;
	}

	void Update () 
	{
		if (Input.GetMouseButtonDown (0)) 
		{
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			ray_cast_hit = Physics2D.Raycast(new Vector2(ray.origin.x, ray.origin.y), new Vector2(0,0));
			if (ray_cast_hit) 
			{
				switch(ray_cast_hit.transform.name){
				case "BG":
					Instantiate(fx_prefabs[ index_fx ], new Vector3(ray.origin.x, ray.origin.y, 0), Quaternion.identity);
					break;
				case "UI-arrow-right":
					ray_cast_hit.transform.SendMessage("Go");
					index_fx++;
					if(index_fx >= fx_prefabs.Length)
						index_fx = 0;
					text_fx_name.text = "[" + (index_fx + 1) + "] " + fx_prefabs[ index_fx ].name;
					break;
				case "UI-arrow-left":
					ray_cast_hit.transform.SendMessage("Go");
					index_fx--;
					if(index_fx <= -1)
						index_fx = fx_prefabs.Length - 1;
					text_fx_name.text = "[" + (index_fx + 1) + "] " + fx_prefabs[ index_fx ].name;
					break;
				case "Instructions":
					Destroy(ray_cast_hit.transform.gameObject);
					break;
				}
			}
		}
		//Change-FX keyboard..	
		if ( Input.GetKeyDown("z") || Input.GetKeyDown("left") ){
			GameObject.Find("UI-arrow-left").SendMessage("Go");
			index_fx--;
			if(index_fx <= -1)
				index_fx = fx_prefabs.Length - 1;
			text_fx_name.text = "[" + (index_fx + 1) + "] " + fx_prefabs[ index_fx ].name;	
		}

		if ( Input.GetKeyDown("x") || Input.GetKeyDown("right")){
			GameObject.Find("UI-arrow-right").SendMessage("Go");
			index_fx++;
			if(index_fx >= fx_prefabs.Length)
				index_fx = 0;
			text_fx_name.text = "[" + (index_fx + 1) + "] " + fx_prefabs[ index_fx ].name;
		}

		if ( Input.GetKeyDown("space") ){
			AnimationAttack();
			//Instantiate(fx_prefabs[ index_fx ], new Vector3(0, 0, -5), Quaternion.identity);	
		}

		if( Input.GetKeyDown(KeyCode.C))
		{
			bHit = !bHit;
		}
	}

	public void AnimationAttack()
	{
		Spine.TrackEntry te;
		if (bHit)
		{
			ctrl.AnimationState.SetAnimation(0, "shot", false);
			var go = Instantiate(fx_prefabs[index_fx], new Vector3(0, 0, -5), Quaternion.identity);
			go.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		else
		{
			te = ctrl.AnimationState.SetAnimation(0, "attack", false);
			te.Event += CBEvent;
		}
		ctrl.AnimationState.AddAnimation(0, "Idle", true, 0f);
	}

	private void CBEvent(TrackEntry trackEntry, Spine.Event e)
	{
		var go = Instantiate(fx_prefabs[index_fx], new Vector3(0, 0, -5), Quaternion.identity);
		go.transform.localScale = new Vector3(1f, 1f, 1f);
		trackEntry.Event -= CBEvent;
	}
}
