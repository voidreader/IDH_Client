using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum AudioType
{
    BGM,
    SFX,
    UISFX,
}
public enum BGMType
{
    BMSND_None = 0,
    BMSND_Main_01 = 1,
    BMSND_Main_02 = 2,
    BMSND_Story = 3,
    BMSND_Battle_Normal = 4,
    BMSND_Battle_Boss = 5,      // 임시 파일
    BMSND_Mission_Success = 6,
    BMSND_Mission_Fail = 7,
    BMSND_Scene = 8,            // 임시
    //BMSND_Scene_2 = 9,
    BMSND_PVP = 10,
    BMSND_MyRoom = 11,
    BMSND_Credit = 12, 
}
enum UISFX
{
    UI_Back = 200,
    UI_Button = 201,
    UI_Count = 202,
}
enum SFX
{
    None,
    //UI
    UI_Back,
    UI_Button,

    UI_Count,

    //Battle
    Sfx_Hit,
    Sfx_Hit_Max,
    Sfx_Man_Skill_Voice,
    Sfx_Woman_Skill_Voice,
    Sfx_Skill,
    Sfx_StageStart,
    Sfx_StageClear,
    

    //Skill Start Sound
    Sfx_Skill_Start = 100,
    Sfx_TeamSkill_Start = 101,
    Sfx_StrikeSkill_Start = 128,    //나중에 스트라이크 시스템 사운드 넘어올때 재작업 필요

    //Attack Hit
    Sfx_Attack_Normal = 102,
    Sfx_Attack_Critical = 103,
    Sfx_Pistol_Normal = 104,
    Sfx_Pistol_Critical = 105,
    Sfx_Magic_Normal = 106,
    Sfx_Magic_Critical = 107,
    Sfx_Sword_Normal = 108,
    Sfx_Sword_Critical = 109,
    Sfx_Attack_Fail = 110,
    Sfx_Pistol_Fail = 111,
    Sfx_Magic_Fail = 112,
    Sfx_Sword_Fail = 108,           //사운드 없어서 normal값으로 입력

    Sfx_Death = 113,
    Sfx_Critical_Start = 114, // 일반 공격 크리티컬. 현재 이것을 단일로만 사용한다.
    Sfx_Charge = 115,
    Sfx_Buff = 116,
    Sfx_Concentration = 117,
    Sfx_Debuff = 118,
    Sfx_DamageDown = 119,
    Sfx_Purification = 120,
    Sfx_Heal = 121,
    Sfx_MaxGuard = 122,
    Sfx_Poison = 123,
    Sfx_Paralysis = 124,
    Sfx_Sleep = 125,
    Sfx_Stun = 126,
    Sfx_Strike_Charge = 127,
    Sfx_Strike = 128,
    Sfx_TeamSkillGauge_100 = 129,
    Sfx_TeamSkillGauge_200 = 130,


    Sfx_Enemy_Info = 200,
    Sfx_PVP_Result = 201,
    Sfx_PVP_Rank_UP = 202,
    Sfx_PVP_Season_End = 203,
    Sfx_PVP_Ready = 204,
    Sfx_Chapter_Release = 205,
    Sfx_Result_Victory = 206,
    Sfx_Result_Defeat = 207,
    Sfx_LevelUp = 208,
    Sfx_Result_Star = 209,
    Sfx_Item_Get = 212,
    Sfx_UI_Button = 213,
    Sfx_UI_Cancel = 214,
    Sfx_UI_Confirm = 215,
    Sfx_UI_Page_Next = 216,
    Sfx_UI_Pre = 217,
    Sfx_UI_Popup = 218,
    Sfx_Gacha_Equip_Normal = 219,
    Sfx_Gacha_Equip_SSS = 220,
    Sfx_Gacha_Normal = 221,
    Sfx_Gacha_SSS = 222,
    Sfx_Gacha_list = 223, // 나열 개수만큼 재생됨
    Sfx_Gacha_SSS_Get = 224,
    Sfx_Gacha_list_Start = 225, 


    Sfx_PVP_VS, // PVP VS 표시할때
    Sfx_Strengthen, // 영웅 및 아이템 강화시
    Sfx_Evolution,  // 영웅 및 아이템 진화시
    Sfx_EquipItem,      // 장비 장착
    Sfx_Strengthen_Popup, // 영웅 및 아이템 강화 및 진화 팝업시
    Sfx_Attendance,     // 출석 체크 시
    Sfx_Popup,          // 팝업시
    Sfx_Make_Start,     // 제조 시작시
    Sfx_Make_Open,      // 제조 슬롯 열림
    Sfx_InGame_Round,   // 인게임 라운드 표시
    Sfx_TeamSkill_Equip,// 팀스킬 장착
}


public class SoundManager
{
    protected struct SFXStruct
    {
        internal AudioType audioType;
        internal AudioSource audioSource;

        public SFXStruct(AudioType _bgmType, AudioSource _audioSource)
        {
            audioType = _bgmType;
            audioSource = _audioSource;
        }
    }
    GameObject root;

    Action cbEndBGM;

    private AudioSource bgm;
    private AudioSource battleStorySFX;
    public AudioSource BattleStorySFX
    {
        get
        {
            if (battleStorySFX == null)
            {
                GameObject nBattleStorySFX = new GameObject();
                nBattleStorySFX.transform.parent = root.transform;
                battleStorySFX = nBattleStorySFX.AddComponent<AudioSource>();
            }
            battleStorySFX.volume = volumeValueArray[1];
            return battleStorySFX;
        }
    }
    private List<SFXStruct> listSFX;

    private BGMType prevBGMType;

    //볼륨 관련 변수들
    private float[] volumeValueArray;
    public UISlider[] volumeSliders;

    bool bPuasedBGM = false;



    internal SoundManager()
    {
        root = new GameObject("SoundManager");
        root.transform.parent = GameCore.Instance.gameObject.transform;

        bgm = root.AddComponent<AudioSource>();
        bgm.loop = true;
        listSFX = new List<SFXStruct>();

        //볼륨정보(BGM, SFX, UISFX)초기화 후 세팅
        volumeValueArray = new float[3];
        for (int i = 0; i < volumeValueArray.Length; i++)
        {
            volumeValueArray[i] = PlayerPrefs.GetFloat("VolumeValue" + i, 1f);
        }
        SetVolume();

        prevBGMType = BGMType.BMSND_None;

        //사운드 파일을 미리 생성하여 멈춤현상 방지
        //GameCore.Instance.ResourceMgr.GetObjectListAsync<AudioClip>(ABType.AB_Audio, 6000004, null);
        //GameCore.Instance.ResourceMgr.GetObjectListAsync<AudioClip>(ABType.AB_Audio, 6000005, null);
    }

    public void RestartBGM()
    {
        bPuasedBGM = false;
        bgm.UnPause();
    }

    public void PauseBGM()
    {
        bPuasedBGM = true;
        bgm.Pause();
    }

    internal float GetBgmPlayTime()
    {
        return bgm.clip.length;
    }
    private bool CheckChangeBGM(BGMType bgmType)
    {
        switch (bgmType)
        {
            case BGMType.BMSND_Main_01:
                if (prevBGMType == BGMType.BMSND_Main_02)
                    return false;
                return (bgmType == prevBGMType) ? false : true;
            //case BGMType.BMSND_Main_02:
            //    if (prevBGMType == BGMType.BMSND_Main_01)
            //        return false;
            //    return (bgmType == prevBGMType) ? false : true;
            default:
                return (bgmType == prevBGMType) ? false : true;
        }
    }
    internal void SetBGMSound(BGMType bgmType, bool isLoop, bool mustChange, Action _cbEndBGM = null)
    {
        if (CheckChangeBGM(bgmType) == false && mustChange == false)
            return;
        prevBGMType = bgmType;
        SetAudioSource((int)bgmType, AudioType.BGM, isLoop, true);
        cbEndBGM = _cbEndBGM;
    }

    public void SetMainBGMSound()
    {
        SetBGMSound(BGMType.BMSND_Main_01, false, false, ()=>
            SetBGMSound(BGMType.BMSND_Main_02, true, false) );
    }

    public BGMType GetNowBGMType()
    {
        return prevBGMType;
    }

    internal void SetCommonBattleSound(ParticleType particleType, int attackType)
    {
        SFX sfxType = SFX.None;
        switch (particleType)
        {
            case ParticleType.NearSkill:
                if (attackType == 3) sfxType = SFX.Sfx_Attack_Critical;
                else if (attackType == 2) sfxType = SFX.Sfx_Attack_Normal;
                else if (attackType == 1) sfxType = SFX.Sfx_Attack_Normal;
                break;
            case ParticleType.SwordSkill:
                if (attackType == 3) sfxType = SFX.Sfx_Sword_Critical;
                else if (attackType == 2) sfxType = SFX.Sfx_Sword_Normal;
                else if (attackType == 1) sfxType = SFX.Sfx_Sword_Fail;
                break;
            case ParticleType.MagicSkill:
                if (attackType == 3) sfxType = SFX.Sfx_Magic_Critical;
                else if (attackType == 2) sfxType = SFX.Sfx_Magic_Normal;
                else if (attackType == 1) sfxType = SFX.Sfx_Magic_Fail;
                break;
            case ParticleType.GunSkill:
                if (attackType == 3) sfxType = SFX.Sfx_Pistol_Critical;
                else if (attackType == 2) sfxType = SFX.Sfx_Pistol_Normal;
                else if (attackType == 1) sfxType = SFX.Sfx_Pistol_Fail;
                break;
            case ParticleType.Death: sfxType = SFX.Sfx_Death; break;
            case ParticleType.Charge: sfxType = SFX.Sfx_Charge; break;
            case ParticleType.Buff: sfxType = SFX.Sfx_Buff; break;
            case ParticleType.Concentration: sfxType = SFX.Sfx_Concentration; break;
            case ParticleType.Debuff: sfxType = SFX.Sfx_Debuff; break;
            case ParticleType.DamageDown: sfxType = SFX.Sfx_DamageDown; break;
            case ParticleType.Purification: sfxType = SFX.Sfx_Purification; break;
            case ParticleType.Heal: sfxType = SFX.Sfx_Heal; break;
            case ParticleType.MaxGuard: sfxType = SFX.Sfx_MaxGuard; break;
            case ParticleType.Poison: sfxType = SFX.Sfx_Poison; break;
            case ParticleType.Paralysis: sfxType = SFX.Sfx_Paralysis; break;
            case ParticleType.Sleep: sfxType = SFX.Sfx_Sleep; break;
            case ParticleType.Stun: sfxType = SFX.Sfx_Stun; break;
        }
        if (sfxType == SFX.None) return;
        SetCommonBattleSound(sfxType, false);
    }

    internal void SetCommonBattleSound(SFX sfxType, bool isOriginalSound = true)
    {
        float volume = 1f;
        switch (sfxType)
        {
            case SFX.Sfx_Skill_Start:
                volume = 0.2f;
                break;
            default:
                break;
        }
        SetAudioSource((int)sfxType, AudioType.UISFX, false, isOriginalSound);
    }
    internal void SetCharacterSkillSound(BattleUnitData unitData, bool isVoice, bool isTeamSkill)
    {
        if ((!isTeamSkill && unitData.SkillData.Data.soundPath == -1) ||
            (isTeamSkill && unitData.TeamSkillData.Data.soundPath == -1) )
            return;
        
        int charID = (isTeamSkill) ? unitData.TeamSkillData.Data.id : unitData.SkillData.Data.id;
        charID += isVoice ? 1000 : 0;
        SetAudioSource(charID, isVoice ? AudioType.SFX : AudioType.UISFX, false, isVoice);
    }
    internal void SetCharacterVoiceSound(UnitDataMap unitData, int randomValue)
    {
        int charID = unitData.charIdType;
        charID += 10000 * (randomValue + 1);
        SetAudioSource(charID, AudioType.SFX, false, true);
    }
    internal float GetCharacterVoiceClipLength(UnitDataMap unitData, int randomValue)
    {
        int charID = unitData.charIdType;
        charID += 10000 * (randomValue + 1);
        return GetAudioClipLength(charID);
    }
    private float GetAudioClipLength(int assetBundleID)
    {
        SoundCommonDataMap soundCommonData = GameCore.Instance.DataMgr.GetSoundCommonData(assetBundleID);
        for (int i = 0; i < listSFX.Count; i++)
        {
            if (listSFX[i].audioSource.clip.name == soundCommonData.fileName)
                return listSFX[i].audioSource.clip.length;
        }
        return 5f;
    }
    internal void SetUISound(UISFX uiSFX)
    {
        SetAudioSource((int)uiSFX, AudioType.UISFX, false, true);
    }
    private AudioSource GetAudioSource(string fileName, AudioType _bgmType)
    {
        SetVolume();
        for (int i = 0; i < listSFX.Count; i++)
        {
            if (listSFX[i].audioType != _bgmType)
                continue;
            if (listSFX[i].audioSource.clip.name == fileName)
                return listSFX[i].audioSource;
        }
        for (int i = 0; i < listSFX.Count; i++)
        {
            if (listSFX[i].audioType != _bgmType)
                continue;
            if (listSFX[i].audioSource.isPlaying == false)
                return listSFX[i].audioSource;
        }
        GameObject nSFXAudioSource = new GameObject("SFX_AudioSource");
        nSFXAudioSource.transform.parent = root.transform;
        AudioSource nAudioSource = nSFXAudioSource.AddComponent<AudioSource>();
        SFXStruct sfxStruct = new SFXStruct(_bgmType, nAudioSource);
        listSFX.Add(sfxStruct);
        SetVolume();
        return nAudioSource;
    }
    private void SetAudioSource(int assetBundleID, AudioType audioType, bool isLoop, bool isOriginalSpeed)
    {
        if (GameCore.Instance.DataMgr == null)
            return;

        SoundCommonDataMap soundCommonData = GameCore.Instance.DataMgr.GetSoundCommonData(assetBundleID);
        if (soundCommonData == null || soundCommonData.fileName == "-1")
            return;


#if UNITY_EDITOR
        if (GameCore.Instance.bLogSound)
            Debug.Log(soundCommonData.fileName + " : " + soundCommonData.id + " : " + soundCommonData.assetBundleID + "------------------------------" +
                      DateTime.Now.Minute + ":" + DateTime.Now.Second + "." + DateTime.Now.Millisecond);
#endif
        if (soundCommonData.assetBundleID == -1)
        {
            var audioSource = GameCore.Instance.ResourceMgr.GetLocalObject<AudioClip>(System.IO.Path.Combine("CommonRsc/Sound", soundCommonData.fileName));
            PlaySound(audioSource, audioType, isLoop, soundCommonData.fileName, isOriginalSpeed);
        }
        else
        {
            GameCore.Instance.ResourceMgr.GetObjectByName<AudioClip>(ABType.AB_Audio,
                                                                     soundCommonData.id,
                                                                     soundCommonData.assetBundleID,
                                                                     soundCommonData.fileName,
                                                                     (audioSource) => {
                                                                         PlaySound(audioSource, audioType, isLoop, soundCommonData.fileName, isOriginalSpeed);
                                                                     });
        }
    }


    void PlaySound(AudioClip audioClip, AudioType audioType, bool isLoop, string fileName, bool isOriginalSpeed)
    {
        //Debug.Log("Success AssetBundle Build : " +  soundCommonData.fileName);
        if (audioClip == null)
        {
            Debug.LogError("There is no Resource about AudioClip");
            return;
        }

        switch (audioType)
        {
            case AudioType.BGM:
                
                bgm.Stop();
                bgm.loop = isLoop;
                bgm.clip = audioClip;
                bgm.Play();
                break;

            case AudioType.SFX:

            case AudioType.UISFX:
                if (audioType == AudioType.UISFX || (GameCore.Instance.TimeScaleChange == 1 || GameCore.Instance.TimeScaleChange == 4))
                {
#if UNITY_EDITOR
                    if (GameCore.Instance.bLogSound)
                        Debug.Log("Played Sound : " + fileName + "  " + DateTime.Now.Minute + ":" + DateTime.Now.Second + "." + DateTime.Now.Millisecond + (isOriginalSpeed ? 1.0f : GameCore.timeScale)); 
#endif
                    AudioSource _audioSource = GetAudioSource(fileName, audioType);
                    _audioSource.clip = audioClip;
                    _audioSource.pitch = isOriginalSpeed ? 1.0f : GameCore.timeScale;
                    _audioSource.PlayOneShot(audioClip);
                }
                break;
            default:
                Debug.LogError("There is Error About AudioSource in SoundManager");
                break;
        }
    }

    private void SetVolume()
    {
        SetVolume(AudioType.BGM, volumeValueArray[0]);
        SetVolume(AudioType.SFX, volumeValueArray[1]);
        SetVolume(AudioType.UISFX, volumeValueArray[2]);
    }
    //사운드 볼륨 저장
    public void SaveVolume()
    {
        for (int i = 0; i < volumeValueArray.Length; i++)
        {
            volumeValueArray[i] = volumeSliders[i].value;
            PlayerPrefs.SetFloat("VolumeValue" + i, volumeValueArray[i]);
        }
        SetVolume();
    }
    //사운드 되돌리기(취소 누를시)
    private void ReturnVolume()
    {
        for (int i = 0; i < volumeSliders.Length; i++)
        {
            SetSliderValue(volumeSliders[i], volumeValueArray[i]);
        }
        SetVolume();
    }
    //슬라이더 값 초기화
    private void SetSliderValue(UISlider _uiSlider, float _value)
    {
        _uiSlider.value = _value;
    }
    public void SetVolumeSlider(ref GameObject volumeOption)
    {

        SoundOptionData soundOptionData = volumeOption.GetComponent<SoundOptionData>();
        volumeSliders = soundOptionData.GetSoundSliderArray;
        SetVolumeSlider();
        for (int i = 0; i < volumeSliders.Length; i++)
        {
            SetSliderValue(volumeSliders[i], volumeValueArray[i]);
        }
        soundOptionData.SetBackGroundButton();
        soundOptionData.SetVolumeButton();
    }

    //볼륨 옵션 생성
    public void CreateVolumeOption(Transform _parent, Action _action)
    {
        var volumeOption = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Option/SoundOptionUI", _parent);
        SetVolumeSlider(ref volumeOption);
        //var volumeOption = SetVolumeSlider(_parent,);

        GameCore.Instance.ShowObject("사운드", null, volumeOption, 2, new MsgAlertBtnData[] {
            new MsgAlertBtnData("취소", new EventDelegate(()=> {
                GameCore.Instance.CloseMsgWindow();
                ReturnVolume();
                _action();
            }), true, null, SFX.Sfx_UI_Cancel),
            new MsgAlertBtnData("저장", new EventDelegate(() => {
                 GameCore.Instance.CloseMsgWindow();
                SaveVolume();
                _action();
            }), true, null, SFX.Sfx_UI_Confirm) });
    }

    //볼륨 슬라이더의 값이 변할 시, 나오는 eventDelegate 세팅 함수
    private void SetVolumeSlider()
    {
        volumeSliders[0].onChange.Add(new EventDelegate(() =>
        {
            SetVolume(AudioType.BGM, volumeSliders[0].value);
        }));
        volumeSliders[1].onChange.Add(new EventDelegate(() =>
        {
            SetVolume(AudioType.SFX, volumeSliders[1].value);
        }));
        volumeSliders[2].onChange.Add(new EventDelegate(() =>
        {
            SetVolume(AudioType.UISFX, volumeSliders[2].value);
        }));
    }
    //Audio 타입에 따른 볼륨값 조정 함수
    private void SetVolume(AudioType audioType, float _value)
    {
        switch (audioType)
        {
            case AudioType.BGM:
                bgm.volume = _value * 0.3f;
                break;

            case AudioType.SFX:

            case AudioType.UISFX:
                var sfxList = from sfxData in listSFX
                              where sfxData.audioType == audioType
                              select sfxData;
                foreach (var sfxData in sfxList)
                {
                    sfxData.audioSource.volume = (audioType == AudioType.UISFX) ? _value * 0.5f : _value;
                }
                break;
            default:
                Debug.LogError("There is Error About AudioSource in SoundManager");
                break;
        }
    }
    public void PlayMainBackgroundSound()
    {
        prevBGMType = BGMType.BMSND_None;
        AudioClip audio = Resources.Load<AudioClip>("commonRsc/sound/BGSND_01_01");
        bgm.Stop();
        bgm.loop = true;
        bgm.clip = audio;
        bgm.Play();
    }

    public void Update()
    {
        if (!GameCore.cachedPause && bgm != null && cbEndBGM != null && !bPuasedBGM && !bgm.isPlaying)
        {
            cbEndBGM.Invoke();
            cbEndBGM = null;
        }
    }
   
    public void StopAllAudioSource()
    {
        for (int i = 0; i < listSFX.Count; i++)
        {
            if (listSFX[i].audioSource.isPlaying)
                listSFX[i].audioSource.Stop();
        }
        if (BattleStorySFX.isPlaying == true)
            BattleStorySFX.Stop();
    }
}