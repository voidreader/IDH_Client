using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStoryUI : MonoBehaviour {

    private List<BattleChapterDataMap> listBattleChapter;   //현 챕터의 대사 리스트 전체
    private List<BattleChapterDataMap> listFirst;           //앞 대사 리스트
    private List<BattleChapterDataMap> listEnd;             //뒷 대사 리스트
    private List<BattleChapterDataMap> listCheck;           //현재 진행중인 대사 리스트
    private int listPos;            //현재 읽고있는 대화 리스트 순번
    private int readTextPos = 0;    //현재 읽고있는 대화 리스트의 대사의 읽고있는 위치

    BattlePara battlePara;
    bool isBackground;
    private Dictionary<string, Texture> dicTexture = new Dictionary<string, Texture>(); //texture들을 저장하기 위한 딕셔너리
    [SerializeField] private UILabel uiLabalValue;   //대사를 입력받는 라벨
    [SerializeField] private UILabel uiTitle;        //타이틀(제목)을 입력받는 라벨
    [SerializeField] private List<UITexture> listCharacter = new List<UITexture>();  //캐릭터들의 texture를 보여주기 위한 묶음 리스트 (3개)
    [SerializeField] private UIButton buttonPage;                                    //전체 페이지를 클릭시 동작하는 버튼
    [SerializeField] private UIButton buttonSkip;                                    //스토리 스킵용 버튼
    [SerializeField] private GameObject objTextBoxPage;                              //웹툰이 아닌 대사화면
    [SerializeField] private GameObject objBackgroundPage;                           //웹툰 화면용
    [SerializeField] private UITexture backgroundTexture;                            //웹툰 화면용 texture
    [SerializeField] private UISprite arrowSetAllText;
    [SerializeField] private UISprite TouchSetAllText;
    [SerializeField] private Transform parentTouchSetAll;
    private int touchSpriteWidth;
    private int touchSpriteHeight;
    private float runningTime = 0f;
    private float yPos = 0f;


    private float waitTime = 0f;
                                            //웹툰 화면용인지 아닌지 체크

    [SerializeField]private bool isEnd = false;
    private bool isStop = true;
    private bool isFirstDel = false;
    private EventDelegate eventDelSetData;
    private EventDelegate eventDelSetAllValue;
    private EventDelegate eventDelSetEnd;
    private EventDelegate delCheck;
    [SerializeField] bool isEndAuto;
    StoryState storyState;
    StoryState prevState;

    //스토리 Auto와 관련된 정보들
    [SerializeField] private UISprite autoSprite;
    [SerializeField] private UITweener autoTw;
    [SerializeField] private UILabel autoLabel;

    Dictionary<string, Sprite> spriteDictionary;
    Dictionary<string, AudioClip> audioClipDictionary;
    [SerializeField] private float audioClipDistance;
    [SerializeField] private float presentTime = 0f;

    public static bool CanCreate(ParaBase _para)
    {
        BattlePara battlePara = _para.GetPara<BattlePara>();
        if (battlePara.stageId / 1000000 != 7) return false;
        StoryDataMap storyData = GameCore.Instance.DataMgr.GetStoryData(battlePara.stageId);
        if (storyData.id >= 7100000) return false;
        
        //if(GameCore.Instance.PlayerDataMgr.GetStorySData(storyData.id).count > 0) return false;
        return true;
    }

    public static BattleStoryUI Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Story/PanalBattleStoryUI", _parent);
        return go.GetComponent<BattleStoryUI>();
    }

    private void SetAssetBundleResource(StoryDataMap _storyData, bool isChapterValue)
    {
        int id = (isChapterValue) ? 7000000 + (_storyData.chapter - 1) * 10 + _storyData.stage : 7000000;

        if (GameCore.Instance.DataMgr.GetAssetBundleData(id) == null)
        {
            SetTutorialAudioClipDic(isChapterValue, _storyData);
            return;
        }

        GameCore.Instance.ResourceMgr.GetObjectList<Sprite>(ABType.AB_BattleStory, id, (_spriteList) =>
        {
            if (_spriteList == null)
                return;

            for (int i = 0; i < _spriteList.Length; i++)
            {
                Sprite sprite = _spriteList[i];
                if (sprite != null)
                {
                    spriteDictionary.Add(sprite.name, sprite);
                }
                else
                {
                    Debug.LogError("AB_BattleStory." + id + " load Fail");
                }
                
                
            }
        });

        SetTutorialAudioClipDic(isChapterValue, _storyData);
    }

    private void SetTutorialAudioClipDic(bool _isChapterValue, StoryDataMap _storyData)
    {
        if (_isChapterValue == true)
        {
            int soundID = 7100000 + (_storyData.chapter - 1) * 10 + _storyData.stage;
            if (GameCore.Instance.DataMgr.GetAssetBundleData(soundID) == null)
                return;
            GameCore.Instance.ResourceMgr.GetObjectList<AudioClip>(ABType.AB_Audio, soundID, (_audioClipList) =>
            {
                if (_audioClipList == null)
                    return;

                for (int i = 0; i < _audioClipList.Length; i++)
                {
                    AudioClip nAudioClip = _audioClipList[i];
                    audioClipDictionary.Add(nAudioClip.name, nAudioClip);
                }
            });
        }
    }

    internal void RemoveResource()
    {
        spriteDictionary.Clear();
        audioClipDictionary.Clear();
        StoryDataMap storyData = GameCore.Instance.DataMgr.GetStoryData(battlePara.stageId);

        ////GameCore.Instance.ResourceMgr.RemoveLocalList("Story/PanalBattleStoryUI");
        //GameCore.Instance.ResourceMgr.RemoveAssetBundle(7000000 + (storyData.chapter - 1) * 10 + storyData.stage);
        //GameCore.Instance.ResourceMgr.RemoveAssetBundle(7000000);
        //GameCore.Instance.ResourceMgr.RemoveAssetBundle(7100000 + (storyData.chapter - 1) * 10 + storyData.stage);
    }

    public void SetStory(ParaBase _para, EventDelegate eventDelSkip)
    {
        battlePara = _para.GetPara<BattlePara>();
        StoryDataMap storyData = GameCore.Instance.DataMgr.GetStoryData(battlePara.stageId);

        spriteDictionary = new Dictionary<string, Sprite>();
        audioClipDictionary = new Dictionary<string, AudioClip>();
        SetAssetBundleResource(storyData, false);
        SetAssetBundleResource(storyData, true);

        DataMapListCtrl<BattleChapterDataMap> table = (DataMapListCtrl<BattleChapterDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.BattleChapter);
        listFirst = new List<BattleChapterDataMap>();
        listEnd = new List<BattleChapterDataMap>();
        var it = table.GetEnumerator();
        while (it.MoveNext())
        {
            var data = it.Current.Value;
            if (it.Current.Key == storyData.id)
            {
                listBattleChapter = data;
                break;
            }
        }
        if (listBattleChapter == null) return;

        for (int i = 0; i < listBattleChapter.Count; i++)
        {
            if (listBattleChapter[i].front == 0)
            {
                listFirst.Add(listBattleChapter[i]);
            }
            else
            {
                listEnd.Add(listBattleChapter[i]);
            }
        }
        eventDelSetData = new EventDelegate(SetData);
        eventDelSetAllValue = new EventDelegate(GotoSetAll);
        eventDelSetEnd = new EventDelegate(SetEnd);
        buttonSkip.onClick.Clear();
        buttonSkip.onClick.Add(eventDelSkip);
        buttonSkip.onClick.Add(new EventDelegate(()=> {
            GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
            StopState(); }));

        parentTouchSetAll = TouchSetAllText.transform.parent;
        touchSpriteWidth = TouchSetAllText.width;
        touchSpriteHeight = TouchSetAllText.height;
        prevState = StoryState.None;
        return;
    }

    public bool CheckStory(bool isFront)
    {
        listCheck = isFront ? listFirst : listEnd;
        return listCheck.Count != 0;
    }

    private void GotoSetAll()
    {
        SetAllValue();
        if (isBackground == true)
            StartCoroutine(MoveByValue(800f, 485f, 10f));
        else
            arrowSetAllText.enabled = true;
        storyState = StoryState.NextPrepare;
        //storyState = StoryState.AllSet;
    }

    public void OnClickStoryAuto()
    {
        GameCore.Instance.SoundMgr.SetCommonBattleSound(SFX.Sfx_UI_Button);
        //GameCore.Instance.SndMgr.PlaySFX(SFX.UI_Button);
        GameCore.atuoStory = !GameCore.atuoStory;
        GameCore.atuoStorySave = GameCore.atuoStory;
        SetStoryAutoButton(GameCore.atuoStory);
    }

    internal void SetStoryAutoButton(bool _auto)
    {
        if (!_auto)
        {
            autoLabel.color = Color.white;
            autoSprite.color = Color.white;
            autoTw.enabled = false;
        }
        else
        {
            autoLabel.color = Color.yellow;
            autoSprite.color = Color.yellow;
            //autoTw.ResetToBeginning();
            //autoTw.PlayForward();
            autoTw.enabled = true;
        }
    }

    public bool CheckList()
    {
        //끝나는 조건
        if (listCheck.Count == 0)
            return true;
        // eventDelegate를 이용한 터치 조건 바꾸기
        if (isFirstDel == true)
        {
            isFirstDel = false;
            buttonPage.onClick.Clear();
            buttonPage.onClick.Add(delCheck);
        }
        //오토를 켰을 시 끝나는 조건
        if (isEnd == true) return true;
        if (listCheck.Count <= listPos && isEndAuto == false)
        {
            
            if (GameCore.atuoStory == true)
            {
                isEndAuto = true;
                waitTime = 0f;
                storyState = StoryState.Wait;
                return false;
            }
            
            isFirstDel = true;
            delCheck = eventDelSetEnd;
            return false;
        }

        switch(storyState)
        {
            case StoryState.Setting:
                storyState = (isBackground == true) ? StoryState.Cartoon : StoryState.Story;
                break;
            case StoryState.Story:
                ReadList();
                break;
            case StoryState.Cartoon:
                CheckSound();
                break;
            case StoryState.AllSet:
                SetAllValue();
                waitTime = 0f;
                storyState = StoryState.Wait;
                break;
            case StoryState.Wait:
                WaitFunc();
                break;
            case StoryState.NextPrepare:
                
                if (GameCore.atuoStory == true)
                {
                    if (isEndAuto == true)
                    {
                        isEnd = true;
                    }
                    else
                        SetData();
                }
               
                ReadyMotion();
                break;
            case StoryState.Stop:
                break;
            default:
                break;
        }
        return false;
    }

    public void StartStory(bool isFront)
    {
        SetStoryAutoButton(GameCore.atuoStory);
        isEnd = false;
        listPos = 0;
        readTextPos = 0;
        SetData();
        isEndAuto = false;
    }

    public void SetData()
    {
        //중단조건 -> 필요한 채팅을 다 읽었을 경우
        if (listCheck.Count <= listPos) return;
        isStop = true;

        //백그라운드 이미지(카툰형 음성 스토리)가 있을시, 우선적으로 틀어준다 -> 없을경우 -1
        string backgroundPath = listCheck[listPos].spriteCartoonPath;
        isBackground = (backgroundPath != "-1") ? true : false;
        objBackgroundPage.SetActive(isBackground);
        objTextBoxPage.SetActive(!isBackground);
        if(isBackground)
        {
            backgroundTexture.mainTexture = GetTexture(backgroundPath);
            //GameCore.Instance.SndMgr.PlayCahracterVoice(CHARACTER_VOICE.CV_NAGA, CHARACTER_ACTION.CA_SKILL);
        }

        //title setting
        string titleText = listCheck[listPos].TitleCharacter;
        uiTitle.text = (titleText == "-1") ? null : titleText;
        uiLabalValue.text = null;

        //texture setting
        string mainTexture = listCheck[listPos].mainCharacterTexture;
        SetTexture(mainTexture, listCheck[listPos].spritePresentCharacterPath[0], listCharacter[0]);
        SetTexture(mainTexture, listCheck[listPos].spritePresentCharacterPath[1], listCharacter[1]);
        SetTexture(mainTexture, listCheck[listPos].spritePresentCharacterPath[2], listCharacter[2]);
        arrowSetAllText.enabled = false;

        delCheck = eventDelSetAllValue;
        isFirstDel = true;
        storyState = StoryState.Setting;
        parentTouchSetAll.gameObject.SetActive(false);
        
        audioClipDistance = 0f;
        presentTime = 0f;

        string audioClipName = string.Concat("VRSND_", listCheck[listPos].index);
        AudioSource nAudioSource = GameCore.Instance.SoundMgr.BattleStorySFX;
        nAudioSource.Stop();

        if (audioClipDictionary.ContainsKey(audioClipName))
        {
            nAudioSource.clip = audioClipDictionary[audioClipName];
            audioClipDistance = nAudioSource.clip.length;
            nAudioSource.Play();
        }
    }

    private Texture GetTexture(string key)
    {
        /*
        Texture texture;
        if (dicTexture.ContainsKey(key) == false)
        {
            texture = (Texture)Resources.Load("UI/InGameStory/" + key);
            dicTexture.Add(key, texture);
        }
        else
            texture = dicTexture[key];
            
        return texture;
        */
        if (spriteDictionary.ContainsKey(key))
            return spriteDictionary[key].texture;
        else
        {
            Debug.LogError("Not found Contains StoryTexture " + key);
            return null;
        }

    }

    public void SetTexture(string mainTexture, string key, UITexture texture)
    {
        // 테이블 값이 -1일 경우 이미지 끄기
        bool enableTexture = (key == "-1") ? false : true;
        texture.enabled = enableTexture;
        if (enableTexture == false) return;

        float textureSize = 0f;
        float colorValue = 1f;
        switch (mainTexture)
        {
            case "1":
                textureSize = 1024f;
                colorValue = 1f;
                break;
            case "0":
                textureSize = 1024f * 0.9f;
                colorValue = 60f / 255f;
                break;
            default:
                textureSize = (key == mainTexture) ? 1024f : 1024f * 0.9f;
                colorValue = (key == mainTexture) ?  1f    : 60f / 255f;

                break;
        }

        //texture 사이즈 조절
        texture.width = (int)textureSize;
        texture.height = (int)textureSize;
        

        //texture 칼라 조정
        texture.color = new Color(colorValue, colorValue, colorValue);
        texture.mainTexture = GetTexture(key);

        // 뎁스 조절
        texture.depth = (key == mainTexture) ? 0 : -1;
    }

    public void SetEnd()
    {
        isEnd = true;
    }

    public void SetAllValue()
    {
        if (listCheck.Count <= listPos) return;

        uiLabalValue.text = listCheck[listPos].Dialogue;
        listPos++;
        readTextPos = 0;
        isStop = false;

        delCheck = eventDelSetData;
        isFirstDel = true;


        if (storyState == StoryState.AllSet)
            storyState = StoryState.Wait;
        else
            storyState = StoryState.NextPrepare;
    }

    public void CheckSound()
    {
        if (audioClipDistance <= presentTime)
        {
            storyState = StoryState.AllSet;
        }
        else
        {
            presentTime += Time.deltaTime;
        }
    }

    private IEnumerator MoveByValue(float startPos, float destination, float speed)
    {
        parentTouchSetAll.gameObject.SetActive(true);
        Vector2 localPos = parentTouchSetAll.localPosition;
        localPos.x = startPos;
        parentTouchSetAll.localPosition = localPos;
        float value = 0f;
        while (true)
        {
            if (parentTouchSetAll.localPosition.x - destination <= 0.1f)
                break;
            value = (startPos >= destination) ? -1f : 1f;
            localPos.x += value * Time.fixedDeltaTime * 100f * speed;
            parentTouchSetAll.localPosition = localPos;
            yield return new WaitForFixedUpdate();
        }

    }

    public void ReadList()
    {
        string _dialogue = listCheck[listPos].Dialogue;
        
        if (readTextPos < _dialogue.Length)
            readTextPos++;
        string text = _dialogue.Substring(0, readTextPos);
        uiLabalValue.text = text;
        if (text.Length == _dialogue.Length && audioClipDistance <= presentTime)
        {
            storyState = StoryState.AllSet;
        }
        presentTime += Time.deltaTime;
    }

    private void WaitFunc()
    {
        if(waitTime < 2f)
        {
            waitTime += Time.deltaTime;
            return;
        }
        if(isBackground == true)
            StartCoroutine(MoveByValue(800f, 485f, 10f));
        else
            arrowSetAllText.enabled = true;
        storyState = StoryState.NextPrepare;
    }

    private void ReadyMotion()
    {
        if (runningTime >= 2 * 3.14) runningTime = 0f;
        runningTime += Time.deltaTime * 5f;
        yPos = Mathf.Sin(runningTime) * 5f;
        if (isBackground == false)
        { 
            arrowSetAllText.transform.localPosition = new Vector2(490, -60 + yPos);
        }
        else
        {
            TouchSetAllText.width = (int)(touchSpriteWidth + yPos);
            TouchSetAllText.height = (int)(touchSpriteHeight + yPos);
        }
    }

    public void StopState()
    {
        if (prevState == StoryState.None)
        {
            prevState = storyState;
            storyState = StoryState.Stop;
        }
    }

    public void ReStartState()
    {
        if (prevState != StoryState.None)
        {
            storyState = prevState;
            prevState = StoryState.None;
        }
    }
}

public enum StoryState
{
    None = 0,
    Setting = 1,
    Story = 2,
    Cartoon = 3,
    Wait = 4,
    AllSet = 5,
    NextPrepare = 6,
    Stop = 7,
}
