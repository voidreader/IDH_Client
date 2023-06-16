//using System;
//using System.Collections.Generic;
//using UnityEngine;

//// 프로토용 임시
//enum BGM
//{
//    MainTheme,
//	Lobby,
//    Lobby2,
//	SelectStage,
//    PVP_Battle,
//    Story_Battle,
//	Battle_Boss,
//    Result_Win,
//    Result_Lose,
//    Story,

//    //SceneType
//    StoryScene,
//    DungeonScene,
//    MyRoomScene,
//    MakeScene,
//    GachaScene,
//    FarmingScene,
//    InventoryScene,
//    EditTeamScene,
//}


//enum CHARACTER_VOICE
//{
//	CV_NAGA				= 1100001,
//	CV_HYENA			= 1100002,
//	CV_GUINUENG		= 1100003,
//	CV_DANA				= 1100004,
//	CV_LADY				= 1100005,
//	CV_EUNBIDAN		= 1100006,
//	CV_SASA				= 1100007,
//	CV_REDRUM			= 1100008,
//	CV_MEDUSA			= 1100009,
//	CV_Count
//}

//enum CHARACTER_ACTION
//{
//	CA_ATTACK =0,
//	CA_SKILL =1,
//	CA_DEATH=2
//}


//class SoundMgr
//{
//	GameObject root;
//	AudioSource bgm;
//	AudioSource sfx;
//	AudioSource uiSfx;

//	float bgmVolume = 0f;
//	float sfxVolum = 0f;

//    public bool BGMStop = false;

//    Dictionary<BGM, string> bgmLoadMap;
//	Dictionary<SFX, string> sfxLoadMap;
//	Dictionary<CHARACTER_VOICE, string[]> voiceLoadMap;

//	internal SoundMgr()
//	{
//		root = new GameObject("SoundMgr");
//		root.transform.parent = GameCore.Instance.gameObject.transform;

//		var go = new GameObject("bgm");
//		go.transform.parent = root.transform;
//		bgm = go.AddComponent<AudioSource>();
//		bgm.playOnAwake = false;
//		bgm.volume = bgmVolume;
//		bgm.loop = true;

//		go = new GameObject("sfx");
//		go.transform.parent = root.transform;
//		sfx = go.AddComponent<AudioSource>();
//		sfx.playOnAwake = false;
//		sfx.volume = sfxVolum;
//		sfx.loop = false;

//		go = new GameObject("uiSfx");
//		go.transform.parent = root.transform;
//		uiSfx = go.AddComponent<AudioSource>();
//		uiSfx.playOnAwake = false;
//		uiSfx.volume = sfxVolum;
//		uiSfx.loop = false;


///* 
//		// 프로토타입용
//		bgmLoadMap = new Dictionary<BGM, string>();
//		bgmLoadMap.Add(BGM.Lobby, "commonRsc/sound/BGM_Scene_Main_A-Play(0806)");
//        bgmLoadMap.Add(BGM.Lobby2, "commonRsc/sound/BGM_Scene_Main_B-Loop");
//        bgmLoadMap.Add(BGM.SelectStage, "commonRsc/sound/BGM_Scene_Stage");
//		bgmLoadMap.Add(BGM.PVP_Battle, "commonRsc/sound/05 BGM_Scene_Battle");
//        bgmLoadMap.Add(BGM.Story_Battle, "commonRsc/sound/05 BGM_Scene_Battle");
//        bgmLoadMap.Add(BGM.Battle_Boss, "commonRsc/sound/BGM_Scene_Battle_Boss");
//        bgmLoadMap.Add(BGM.Result_Win, "commonRsc/sound/08 BGM_Result_Suscess");
//        bgmLoadMap.Add(BGM.Result_Lose, "commonRsc/sound/09 BGM_Result_Defeat");
//        bgmLoadMap.Add(BGM.Story, "commonRsc/sound/04 BGM_Scene_Story");
//        bgmLoadMap.Add(BGM.MainTheme, "commonRsc/sound/BGSND_01_01");


//        //각종 시작 씬에서 나올 사운드
//        bgmLoadMap.Add(BGM.StoryScene, "commonRsc/sound/BGM_Scene_Stage");
//        bgmLoadMap.Add(BGM.DungeonScene, "commonRsc/sound/BGM_Scene_Stage");
//        bgmLoadMap.Add(BGM.MyRoomScene, "commonRsc/sound/BGM_Scene_Main2");
//        bgmLoadMap.Add(BGM.MakeScene, "commonRsc/sound/BGM_Scene_Main2");
//        bgmLoadMap.Add(BGM.GachaScene, "commonRsc/sound/BGM_Scene_Main2");
//        bgmLoadMap.Add(BGM.FarmingScene, "commonRsc/sound/BGM_Scene_Main2");
//        bgmLoadMap.Add(BGM.InventoryScene, "commonRsc/sound/BGM_Scene_Stage");
//        bgmLoadMap.Add(BGM.EditTeamScene, "commonRsc/sound/BGM_Scene_Stage");

//*/








//        sfxLoadMap = new Dictionary<SFX, string>();
//		sfxLoadMap.Add(SFX.UI_Back, "commonRsc/sound/SFX_Back");
//		sfxLoadMap.Add(SFX.UI_Button, "commonRsc/sound/SFX_Button");
//		sfxLoadMap.Add(SFX.Sfx_Charge, "commonRsc/sound/SFX_Charge");
//		sfxLoadMap.Add(SFX.Sfx_Skill, "commonRsc/sound/SFX_Skill");
//		sfxLoadMap.Add(SFX.Sfx_Man_Skill_Voice, "commonRsc/sound/SFX_Man_Skill_Voice");
//		sfxLoadMap.Add(SFX.Sfx_Woman_Skill_Voice, "commonRsc/sound/SFX_Woman_Skill_Voice");
//		sfxLoadMap.Add(SFX.Sfx_Hit, "commonRsc/sound/SFX_Hit");
//		sfxLoadMap.Add(SFX.Sfx_Hit_Max, "commonRsc/sound/SFX_Hit_Max");

//        sfxLoadMap.Add(SFX.Sfx_Attack_Normal, "commonRsc/sound/SFX_Attack_Normal");
//		sfxLoadMap.Add(SFX.Sfx_Attack_Critical, "commonRsc/sound/SFX_Attack_Critical");
//        sfxLoadMap.Add(SFX.Sfx_Magic_Normal, "commonRsc/sound/SFX_Magic_Normal");
//        sfxLoadMap.Add(SFX.Sfx_Magic_Critical, "commonRsc/sound/SFX_Magic_Critical");
//        sfxLoadMap.Add(SFX.Sfx_Pistol_Normal, "commonRsc/sound/SFX_Pistol_Normal");
//        sfxLoadMap.Add(SFX.Sfx_Pistol_Critical, "commonRsc/sound/SFX_Pistol_Critical");
//        sfxLoadMap.Add(SFX.Sfx_Sword_Normal, "commonRsc/sound/SFX_Sword_Normal");
//        sfxLoadMap.Add(SFX.Sfx_Sword_Critical, "commonRsc/sound/SFX_Sword_Critical");

//        sfxLoadMap.Add(SFX.Sfx_TeamSkill_Start, "commonRsc/sound/SFX_Teamskill_Start");

//        sfxLoadMap.Add(SFX.Sfx_StageStart, "commonRsc/sound/SFX_Stage_Start");
//		sfxLoadMap.Add(SFX.Sfx_StageClear, "commonRsc/sound/SFX_Stage_Clear");

//		voiceLoadMap = new Dictionary<CHARACTER_VOICE, string[]>();
//		voiceLoadMap.Add(CHARACTER_VOICE.CV_NAGA, new string[] {
//		"commonRsc/sound/naga/1100001_Attack",
//		"commonRsc/sound/naga/1100001_Skill",
//		"commonRsc/sound/naga/1100001_Death"
//		});
//		voiceLoadMap.Add(CHARACTER_VOICE.CV_HYENA, new string[] {
//		"commonRsc/sound/hyena/1100002_Attack",
//		"commonRsc/sound/hyena/1100002_Skill",
//		"commonRsc/sound/hyena/1100002_Death"
//		});
//		voiceLoadMap.Add(CHARACTER_VOICE.CV_GUINUENG, new string[] {
//		"commonRsc/sound/guinueng/1100003_Attack",
//		"commonRsc/sound/guinueng/1100003_Skill",
//		"commonRsc/sound/guinueng/1100003_Death"
//		});
//		voiceLoadMap.Add(CHARACTER_VOICE.CV_DANA, new string[] {
//		"commonRsc/sound/dana/1100004_Attack",
//		"commonRsc/sound/dana/1100004_Skill",
//		"commonRsc/sound/dana/1100004_Death"
//		});
//		voiceLoadMap.Add(CHARACTER_VOICE.CV_LADY, new string[] {
//		"commonRsc/sound/lady/1100005_Attack",
//		"commonRsc/sound/lady/1100005_Skill",
//		"commonRsc/sound/lady/1100005_Death"
//		});
//		voiceLoadMap.Add(CHARACTER_VOICE.CV_EUNBIDAN, new string[] {
//		"commonRsc/sound/eunbidan/1100006_Attack",
//		"commonRsc/sound/eunbidan/1100006_Skill",
//		"commonRsc/sound/eunbidan/1100006_Death"
//		});
//		voiceLoadMap.Add(CHARACTER_VOICE.CV_SASA, new string[] {
//		"commonRsc/sound/sasa/1100007_Attack",
//		"commonRsc/sound/sasa/1100007_Skill",
//		"commonRsc/sound/sasa/1100007_Death"
//		});
//		voiceLoadMap.Add(CHARACTER_VOICE.CV_REDRUM, new string[] {
//		"commonRsc/sound/redrum/1100008_Attack",
//		"commonRsc/sound/redrum/1100008_Skill",
//		"commonRsc/sound/redrum/1100008_Death"
//		});
//		voiceLoadMap.Add(CHARACTER_VOICE.CV_MEDUSA, new string[] { // 음성이 없어서 레드럼것으로 대체
//		"commonRsc/sound/redrum/1100008_Attack",
//		"commonRsc/sound/redrum/1100008_Skill",
//		"commonRsc/sound/redrum/1100008_Death"
//		});
//	}
//    internal float GetBgmPlayTime()
//    {
//        return bgm.clip.length;
//    }
    
//    internal void PlayBGM(BGM _type)
//	{
//        // Play BGM

//        bgm.loop = true;

//        var clip = GameCore.Instance.ResourceMgr.GetLocalObject<AudioClip>(bgmLoadMap[_type]);
//        if (bgm.clip != clip)
//        {
//            bgm.clip = clip;
//            bgm.Play();
//        }
      
//	}
    
//    internal void PlayBGM(BGM _type, bool isLoop)
//    {
//        // Play BGM
//        var clip = GameCore.Instance.ResourceMgr.GetLocalObject<AudioClip>(bgmLoadMap[_type]);
//        if (bgm.clip != clip)
//        {
//            bgm.clip = clip;
//            bgm.Play();
//            bgm.loop = isLoop;
//        }
        
//    }
//    internal void CoStopBGM(bool isStop)
//    {
//        BGMStop = isStop;
//    }
//	internal void StopBGM()
//	{
//		// Stop BGM
//		bgm.Stop();
//	}

//	internal void PlayCahracterVoice(CHARACTER_VOICE _key, CHARACTER_ACTION _act)
//	{
//		//if(GameCore.timeScale == 1)
//			if(voiceLoadMap.ContainsKey(_key))
//				uiSfx.PlayOneShot(GameCore.Instance.ResourceMgr.GetLocalObject<AudioClip>(voiceLoadMap[_key][(int)_act]));
//	}

//	internal void PlaySFX(SFX _type)
//	{
//		if( _type < SFX.UI_Count)
//		{
//			PlayUISFX(_type);
//			return;
//		}

//		// Play Sound Effect
//        if(GameCore.Instance.TimeScaleChange == 1)
//		//if (GameCore.timeScale == 1f)
//			sfx.PlayOneShot(GameCore.Instance.ResourceMgr.GetLocalObject<AudioClip>(sfxLoadMap[_type]));
//	}
//    internal void PlaySFX(string _path)
//    {
//        if (_path == "-1") return;

//        // Play Sound Effect
//        if (GameCore.Instance.TimeScaleChange == 1)
//            //if (GameCore.timeScale == 1f)
//            sfx.PlayOneShot(GameCore.Instance.ResourceMgr.GetLocalObject<AudioClip>("commonRsc/sound/" + _path));
//    }

//    internal void StopAllSFX()
//	{
//		// Stop All Sound Effect
//		sfx.Stop();
//	}

//    internal bool CheckSFXPlay()
//    {
//        //return sfx.isPlaying;
//        return uiSfx.isPlaying;
//    }
//    internal void StopUISFX()
//    {
//        uiSfx.Stop();
//    }


//    private void PlayUISFX(SFX _type)
//	{
//		uiSfx.PlayOneShot(GameCore.Instance.ResourceMgr.GetLocalObject<AudioClip>(sfxLoadMap[_type]));
//	}
//}
