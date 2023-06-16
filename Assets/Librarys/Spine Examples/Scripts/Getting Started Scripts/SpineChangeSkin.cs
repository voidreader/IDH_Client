using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using Spine.Unity.Modules.AttachmentTools;

public class SpineChangeSkin : MonoBehaviour {

	public TextAsset skeletonJson;
	public TextAsset atlasText1;
	public Texture2D[] textures1;

	public TextAsset atlasText2;
	public Texture2D[] textures2;
	public Material materialPropertySource;

	[Space]
	public Material customMaterial;

	Material nowMt;
	bool bMt;

	SkeletonAnimation newSkeletonAnimation;

	AtlasAsset runtimeAtlasAsset1;
	AtlasAsset runtimeAtlasAsset2;
	SkeletonDataAsset runtimeSkeletonDataAsset1;
	SkeletonDataAsset runtimeSkeletonDataAsset2;

	MeshRenderer mr;

	public void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			CreateSpineCharacter();
		}
		if (Input.GetKeyDown(KeyCode.A))
		{
			newSkeletonAnimation.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f, 0f);
		}
		if ( Input.GetKeyDown(KeyCode.B))
		{
			//var time = newSkeletonAnimation.AnimationState.GetCurrent(0).trackTime;
			if ( !bMt )
			{
				bMt = !bMt;
				
				newSkeletonAnimation.GetComponent<MeshRenderer>().material = customMaterial;
				//newSkeletonAnimation.skeletonDataAsset.atlasAssets = new AtlasAsset[] { runtimeAtlasAsset1 }; //notworking
				//newSkeletonAnimation.skeletonDataAsset = runtimeSkeletonDataAsset1;
				//newSkeletonAnimation.Initialize(true);// = SkeletonAnimation.NewSkeletonAnimationGameObject(runtimeSkeletonDataAsset);
				//newSkeletonAnimation.AnimationState.SetAnimation(0, "idle", true).trackTime = time;
			}
			else
			{
				bMt = !bMt;
				newSkeletonAnimation.GetComponent<MeshRenderer>().material = materialPropertySource;
				//newSkeletonAnimation.skeletonDataAsset.atlasAssets = new AtlasAsset[] { runtimeAtlasAsset2 }; //notworking
				//newSkeletonAnimation.skeletonDataAsset = runtimeSkeletonDataAsset2;
				//newSkeletonAnimation.Initialize(true);// = SkeletonAnimation.NewSkeletonAnimationGameObject(runtimeSkeletonDataAsset);
				//var entry = newSkeletonAnimation.AnimationState.SetAnimation(0, "idle", true).trackTime = time;
			}

			Debug.Log(newSkeletonAnimation.initialSkinName);
			//ChangeMaterial();
		}
	}


	public void CreateSpineCharacter()
	{
		//AcumulateTimer timer = new AcumulateTimer();
		if (newSkeletonAnimation == null)
		{
		//	timer.Begin();
			runtimeAtlasAsset1 = AtlasAsset.CreateRuntimeInstance(atlasText1, textures1, materialPropertySource, true);
			runtimeAtlasAsset2 = AtlasAsset.CreateRuntimeInstance(atlasText2, textures2, customMaterial, true);
		//	timer.End(); Debug.Log("AtlasAsset.CreateRuntimeInstance : " + timer.Delay);

		//	timer.Begin(true);
			runtimeSkeletonDataAsset1 = SkeletonDataAsset.CreateRuntimeInstance(skeletonJson, runtimeAtlasAsset1, true);
			runtimeSkeletonDataAsset2 = SkeletonDataAsset.CreateRuntimeInstance(skeletonJson, runtimeAtlasAsset2, true);
		//	timer.End(); Debug.Log("SkeletonDataAsset.CreateRuntimeInstance : " + timer.Delay);
		}
	//	timer.Begin(true);
		newSkeletonAnimation = SkeletonAnimation.NewSkeletonAnimationGameObject(runtimeSkeletonDataAsset2);
	//	timer.End(); Debug.Log("SkeletonAnimation.NewSkeletonAnimationGameObject : " + timer.Delay);
		//timer.Begin(true);
		//var newSkeletonGraphic = SkeletonGraphic.NewSkeletonGraphicGameObject(runtimeSkeletonDataAsset, transform);
		//timer.End(); Debug.Log("SkeletonGraphic.NewSkeletonGraphicGameObject : " + timer.Delay);
		
		newSkeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
		newSkeletonAnimation.transform.position = new Vector3(0f, -3f, 0f);
		newSkeletonAnimation.transform.parent = transform;
		mr = newSkeletonAnimation.GetComponent<MeshRenderer>();
	}

	public void ChangeMaterial()
	{
		//AcumulateTimer timer = new AcumulateTimer();
		//var slots = runtimeSkeletonDataAsset1.GetSkeletonData(true).Slots;
		//var atlasPaga = nowMt.ToSpineAtlasPage(); // Spine.Unity.Modules.AttachmentTools의 확장 프로그램
		//for (int i = 0; i< slots.Count; ++i )
		//{
		//	Attachment myAttachment = FindAttachment(runtimeSkeletonDataAsset1, "default", slots.Items[i].name, slots.Items[i].name);
		//	SetAttachmentRegionMaterial(myAttachment, atlasPaga);
		//}


		//Skin combined = new Skin("combined");
		//foreach (var equip in EquipList)
		//{
		//	Skin skin = newSkeletonAnimation.skeleton.data.FindSkin(equip);

		//	if (skin != null)
		//	{
		//		combined.AddFromSkin(skin);
		//	}
		//}

		//newSkeletonAnimation.skeleton.Skin = null;
		//newSkeletonAnimation.skeleton.SetSkin(combined);
	}


	static public Attachment FindAttachment(SkeletonDataAsset skeletonDataAsset, string skinName, string slotName, string attachmentName)
	{
		if (skeletonDataAsset == null) throw new System.ArgumentNullException("skeletonDataAsset");

		var skeletonData = skeletonDataAsset.GetSkeletonData(true);
		//if (skeletonData == null) return null;
		var skin = skeletonData.FindSkin(skinName);
		//if (skin == null) return null;
		int slotIndex = skeletonData.FindSlotIndex(slotName);
		//if (slotIndex <= 0) return null;
		var attachment = skin.GetAttachment(slotIndex, attachmentName);

		return attachment;
	}

	/// <summary>
	/// 지정된 Material로부터 새로운 AtlasPage를 생성 해, 그것을 사용해, Attachment의 AtlasRegion를 변경합니다. 이 방법은이 첨부 파일을 사용하는 모든 스켈레톤에 영향을 미칩니다.
	/// </summary>
	static public void SetAttachmentRegionMaterial(Attachment attachment, AtlasPage atlasPage)
	{
		if (attachment == null) return;

		var atlasRegion = GetAttachmentAtlasRegion(attachment);
		if (atlasRegion == null) return;

		atlasRegion.page = atlasPage;
	}


	/// <summary>
	/// Attachment의 AtlasRegion에 대한 참조를 가져옵니다. 첨부 파일을 렌더링 할 수없는 경우 null을 반환합니다.
	/// </summary>
	static public AtlasRegion GetAttachmentAtlasRegion(Attachment attachment)
	{
		AtlasRegion atlasRegion = null;

		var regionAttachment = attachment as RegionAttachment;
		if (regionAttachment != null)
		{
			atlasRegion = (AtlasRegion)regionAttachment.RendererObject;
		}
		else
		{
			var meshAttachment = attachment as MeshAttachment;
			if (meshAttachment != null)
				atlasRegion = (AtlasRegion)meshAttachment.RendererObject;
		}

		return atlasRegion;
	}

}
