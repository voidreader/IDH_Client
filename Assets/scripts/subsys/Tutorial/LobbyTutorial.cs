using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;
using System;


public class LobbyTutorial : MonoBehaviour {
    private List<TutorialDataMap> listTutorial;   //현 튜토리얼 전체
    [SerializeField] SkeletonAnimation[] spineAnimation = new SkeletonAnimation[2];
    [SerializeField] SkeletonDataAsset[] skeletonDataAssets = new SkeletonDataAsset[2];
    [SerializeField] GameObject[] objectTextBox = new GameObject[2];
    [SerializeField] UILabel[] labelTitleText = new UILabel[2];
    [SerializeField] UILabel[] labelTextBox = new UILabel[2];
    [SerializeField] UISprite[] spriteTextBox = new UISprite[2];
    [SerializeField] Transform[] arrowTransform = new Transform[2];
    [SerializeField] UIButton buttonTouch;
    [SerializeField] private TutorialBackGroundScript tutorialBackGround;
    [SerializeField] GameObject parentPos;
    private bool isDelChange = false;
    private bool isSetup = false;
    int[] beforeDataAssetPos = { -1, -1 };
    string[] TutorialAniType =
    {
        "Idle", "Emotion", "Win_01", "Win_02", "Win_03", "Win_sp"
    };

    private EventDelegate eventDelSetData;
    private EventDelegate eventDelSetAllData;
    private EventDelegate resultDelList;
    private EventDelegate delCheck;
    private TutorialActive tutorialActiveFlag = TutorialActive.None;
    private int listPos = 0;
    private int textPos = 0;
    private int presentTextPos;
    private bool isEnd = false;
    private TutorialState tutorialStateData;
    internal TutorialState TutorialStateData { set { tutorialStateData = value; /*Debug.LogError(tutorialStateData);*/ } get { return tutorialStateData; } }

    private int prevTutorialType;
    private float runningTime;
    private float yPos;

    public List<ReturnTutorialData> tutorialTransformQueue = new List<ReturnTutorialData>();
    public List<Action> tutorialActionQueue = new List<Action>();
    List<DequeueData> dequeueDataList = new List<DequeueData>();

    private bool isTutorialFixed;
    internal int tutorialPos;
    private bool isTwice = false;
    public bool IsRunning { get; private set; }

    private SkeletonAnimation prevSkeletonAnimation;

    public static bool CheckCreateAble(Tutorial nTutorialData, bool isFixed, int pos)
    {
        if (isFixed == true)
            return nTutorialData.main < 8;

        switch(pos)
        {
            case 0: return nTutorialData.dungeon == 0;
            case 1: return nTutorialData.raid == 0;
            case 2: return nTutorialData.pvp == 0;
            case 3: return nTutorialData.myRoom == 0;
            case 4: return nTutorialData.manufact == 0;
            case 12: return nTutorialData.farming == 0;
            case 6: return nTutorialData.mission == 0;
            case 7: return nTutorialData.mail == 0;
        }
        Debug.LogError("Not Correct Tutorial Value");
        return false;
    }

    public static LobbyTutorial Create(Transform _parent)
    {
        var go = GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Tutorial/PanelTutorial", _parent);
        var component = go.GetComponent<LobbyTutorial>();
        foreach (var spine in component.spineAnimation)
            spine.IsUnscaledTime = true;

        return component;
    }

    protected void ChangeSpineAnimation(int pos, int skeletonDataAssetPos, int emotionPos)
    {
        SkeletonAnimation skeletonAni;
        for (int i = 0; i < spineAnimation.Length; i++)
        {
            spineAnimation[i].gameObject.SetActive(i == pos);
            if (i == pos)
            {
                skeletonAni = spineAnimation[i];
                if (beforeDataAssetPos[pos] != skeletonDataAssetPos)
                {
                    beforeDataAssetPos[pos] = skeletonDataAssetPos;
                    SkeletonDataAsset skeletonDataAsset = skeletonDataAssets[skeletonDataAssetPos];
                    skeletonAni.skeletonDataAsset = skeletonDataAsset;
                    skeletonAni.skeletonDataAsset = SkeletonDataAsset.CreateRuntimeInstance(
                                                                        skeletonDataAsset.skeletonJSON,
                                                                        skeletonDataAsset.atlasAssets, true, 1.2f);

                    skeletonAni.Initialize(true);
                }
                spineAnimation[i].AnimationState.SetAnimation(0, TutorialAniType[emotionPos], true);
            }
        }

    }

    protected void ChangeTextBox(int pos, string value)
    {
        for(int i = 0; i < objectTextBox.Length;i++)
        {
            objectTextBox[i].SetActive(i == pos);
            if (i == pos)
            {
                labelTextBox[i].text = value;
            }
        }
        //objectTextBox[pos].SetActive((character != -1));
        //spriteTextBox[pos].color = ((tutorialActiveFlag & flagValue) == flagValue) ? Color.white : Color.gray;
        //if(presentTextPos == character) labelTextBox[pos].text = value;
    }

    public void Init()
    {
        eventDelSetData = new EventDelegate(SetData);
        eventDelSetAllData = new EventDelegate(SetAllData);
        isDelChange = false;
        for (int i = 0; i < 2; i ++)
        {
            spineAnimation[i].GetComponent<MeshRenderer>().sortingOrder = 103;
        }
        //결과 함수 리스트 초기화
        resultDelList = new EventDelegate(SetEnd);
        listPos = 0;
        TutorialStateData = TutorialState.Prepare;
        IsRunning = true;
    }

    public void ResetTutorial()
    {
        tutorialTransformQueue.Clear();
        tutorialActionQueue.Clear();
        dequeueDataList.Clear();

        listPos = 0;
        TutorialStateData = TutorialState.Prepare;
        isSetup = false;
        IsRunning = false;
    }

    private void SetEnd()
    {
        isEnd = true;
    }

    public void SetTutorialList(bool _isFixed, int key)
    {
        DataMapListCtrl<TutorialDataMap> table = (DataMapListCtrl<TutorialDataMap>)GameCore.Instance.DataMgr.GetDataList(DataMapType.Tutorial);
        var it = table.GetEnumerator();
        while (it.MoveNext())
        {
            var data = it.Current.Value;
            if (it.Current.Key == key)
            {
                isTutorialFixed = _isFixed;
                tutorialPos = key;
                listTutorial = data;
                break;
            }
        }
        //tutorialState = TutorialState.Prepare;
        //SetData();
    }

    public bool checkList()
    {
        if (isEnd == true)
        {
            if (isTutorialFixed == true)
                GameCore.Instance.PlayerDataMgr.SetMainTutorial();
            //else
            //    GameCore.Instance.PlayerDataMgr.SetSubTutorial(tutorialPos);
            return true;
        }

        if (listPos >= listTutorial.Count)
        {
            int resultPos = listTutorial[0].id - 1;
            buttonTouch.onClick.Clear();
            buttonTouch.onClick.Add(resultDelList);
            ReadyMotion();
            return false;
        }
            
        if(isDelChange == true)
        {
            isDelChange = false;
            buttonTouch.onClick.Clear();
            buttonTouch.onClick.Add(delCheck);
            return false;
        }
        switch(TutorialStateData)
        {
            case TutorialState.Pause:
                break;
            case TutorialState.Prepare:
                if (GameCore.Instance.IsDoneResourceLoad())
                {
                    SetData();
                    TutorialStateData = TutorialState.Setup;
                }
                break;
            case TutorialState.Setup:
                TutorialStateData = TutorialState.Read;
                break;
            case TutorialState.Read:
                ReadText();
                break;
            case TutorialState.ClickButton:
                ReadText();
                break;
            case TutorialState.SetAll:
                SetAllData();
                break;
            case TutorialState.Wait:
                break;
            case TutorialState.NextPrepare:
                ReadyMotion();
                break;
            default:
                break;
        }
        
        return false;
    }
    private void CheckFirstScene()
    {
        if (listPos >= listTutorial.Count)
            return;
        TutorialDataMap nTutorialDataMap = listTutorial[listPos];
        if (GameCore.Instance.SubsysMgr.IsChanging)
        {
            TutorialStateData = TutorialState.Pause;
        }
        //if (nTutorialDataMap.isFirstSceneData == true)
        //{
        //    TutorialStateData = TutorialState.Pause;
        //}
    }
    private void SetAllData()
    {
        if (isSetup == false)
            return;
        isSetup = false;
        TutorialDataMap nTutorialDataMap = listTutorial[listPos];
        ChangeTextBox(nTutorialDataMap.characters, nTutorialDataMap.value);
        listPos++;
        textPos = 0;
        TutorialStateData = TutorialState.NextPrepare;
        delCheck = eventDelSetData;
        isDelChange = true;
        for(int i = 0; i < arrowTransform.Length; i ++)
        {
            arrowTransform[0].gameObject.SetActive(nTutorialDataMap.characters == i);
        }
        CheckFirstScene();
    }

    private void ReadText()
    {
        TutorialDataMap nTutorialDataMap = listTutorial[listPos];
        string _dialogue = nTutorialDataMap.value;
        int dialogueLength = _dialogue.Length;
        textPos = dialogueLength <= textPos ? dialogueLength : (textPos + 1);
        string text = _dialogue.Substring(0, textPos);
        ChangeTextBox(nTutorialDataMap.characters, text);
        if (text.Length == _dialogue.Length)
        {
            TutorialStateData = TutorialState.SetAll;
        }
    }

    private void ChangeTitle(int pos, string title)
    {
        labelTitleText[pos].text = title;
        //if (presentTextPos == character)
        //{
        //    labelTitleText[pos].text = title;
        //}
        //labelTitleText[pos].color = ((tutorialActiveFlag & flagValue) == flagValue) ? Color.white : Color.gray;
    }

    public void SetData()
    {
        if (TutorialStateData == TutorialState.Pause) return;
        TutorialStateData = TutorialState.Setup;
        if (isSetup == true)
            return;
        if (listTutorial.Count <= listPos)
            return;
        textPos = 0;
        isSetup = true;
        TutorialDataMap nTutorialDataMap = listTutorial[listPos];
        int selectChar = nTutorialDataMap.titleIndex;
        
        switch (nTutorialDataMap.title)
        {
            case "귀능": presentTextPos = 0; break;
            case "혜나": presentTextPos = 1; break;
            default:break;
        }

        int tutorialType = nTutorialDataMap.type;
        prevTutorialType = tutorialType;
        switch (tutorialType)
        {
            case 0:
                tutorialBackGround.ResetDraw(false, 0, 0, 0, 0);break;
            case 1:
                GameCore.Instance.DoWaitCall(()=>ResetTutorialData(nTutorialDataMap)); break;
            case 2:
                tutorialBackGround.ResetDraw(true, nTutorialDataMap.centerX, nTutorialDataMap.centerY, nTutorialDataMap.sizeX, nTutorialDataMap.sizeY); break;
            case 3:
                GameCore.Instance.DoWaitCall(() => ResetTutorialData(nTutorialDataMap)); break;
            case 4:
                DequeueTutorialActionData()();
                gameObject.SetActive(false);
                isTwice = false;
                return;
            case 5:
                GameCore.Instance.DoWaitCall(() => ResetTutorialData(nTutorialDataMap));
                DequeueTutorialActionData()();
                break;

            case 6:
                DequeueTutorialActionData()();
                return;
            default:break;
        }

        ChangeTitle(nTutorialDataMap.characters, nTutorialDataMap.title);
        ChangeSpineAnimation(nTutorialDataMap.characters, nTutorialDataMap.titleIndex, nTutorialDataMap.emotions);
        arrowTransform[0].gameObject.SetActive(false);
        arrowTransform[1].gameObject.SetActive(false);
        delCheck = eventDelSetAllData;
        isDelChange = true;
        TutorialStateData = TutorialState.Setup;
    }

    private void ResetTutorialData(TutorialDataMap nTutorialDataMap)
    {
        var deq = DequeueTutorialTransformData();
        Transform dequeueTutorialTransform = null;
        if (deq != null)
        {
            dequeueTutorialTransform = deq.transform;

            buttonTouch.enabled = false;
            tutorialBackGround.ResetDraw(true, nTutorialDataMap.centerX, nTutorialDataMap.centerY, nTutorialDataMap.sizeX, nTutorialDataMap.sizeY);
            dequeueTutorialTransform.parent = parentPos.transform;
            parentPos.SetActive(false);
            parentPos.SetActive(true);
        }
    }

    public bool CheckTutorialBeforeTurnOn(int pos)
    {
        return pos == tutorialPos;
    }

    public void TurnOnTutorial(Action action)
    {
        if (isTwice == true) return;
        isTwice = true;
        gameObject.SetActive(true);
        if (isSetup == true)
        {
            if (listPos + 1 >= listTutorial.Count) return;
            listPos++;
            isSetup = false;
        }

        TutorialStateData = LobbyTutorial.TutorialState.Prepare;
        action();
        SetData();
    }

    private void ReadyMotion()
    {
        if (runningTime >= 2 * 3.14) runningTime = 0f;
        runningTime += Time.deltaTime * 5f;
        yPos = Mathf.Sin(runningTime) * 5f;
        arrowTransform[0].localPosition = new Vector2(150, -50 + yPos);
        arrowTransform[1].localPosition = new Vector2(150, -50 + yPos);
    }

    public enum TutorialState
    {
        None = 0,
        Setup = 1,
        Read = 2,
        SetAll = 3,
        Wait = 4,
        NextPrepare = 5,
        Prepare = 6,
        ClickButton,
        Pause,
    }

    public enum TutorialActive
    {
        None = 0,
        Left = 1,
        Right = 2,
    }

    private void Update()
    {
        if (/*!GameCore.Instance.SubsysMgr.IsChanging &&*/ checkList())
        {
            GameObject.Destroy(gameObject);
        }
    }
    public void AddTutorialAction(ReturnTutorialData tutorialChild)
    {
        switch (tutorialChild.type)
        {
            case 0:

                UIButton uiButton = tutorialChild.transform.GetComponent<UIButton>();
                uiButton.onClick.Add(new EventDelegate(() => { ReturnTutorialData(uiButton); }));
                break;
            case 1:
                {
                    ButtonRapper buttonRapper = tutorialChild.transform.GetComponent<ButtonRapper>();
                    buttonRapper.SetPressAddCallback(() => { ReturnTutorialData(); });
                }
                break;
            case 2:
                {
                    EditTeamUI editTeamUI = tutorialChild.transform.GetComponent<EditTeamUI>();
                    editTeamUI.SetOnPressAction(() => { ReturnTutorialData(); });
                    tutorialChild.transform = editTeamUI.GetCardTransform();
                    break;
                }
            case 3:
                {
                    SlotBase<SpineCharacterCtrl> slotBase = tutorialChild.transform.GetComponent<SlotBase<SpineCharacterCtrl>>();
                    if (slotBase.Item != null)
                    {
                        prevSkeletonAnimation = slotBase.Item.skelAnim;
                        prevSkeletonAnimation.GetComponent<MeshRenderer>().sortingOrder = 105;
                        prevSkeletonAnimation.GetComponent<MeshRenderer>().sortingLayerName = "UI";
                    }
                    slotBase.saveAction.onClickAction = () => {
                        if (prevSkeletonAnimation != null)
                        {
                            prevSkeletonAnimation.GetComponent<MeshRenderer>().sortingOrder = 1;
                            prevSkeletonAnimation.GetComponent<MeshRenderer>().sortingLayerName = "Defualt";
                        }
                        slotBase.Item.skelAnim.GetComponent<MeshRenderer>().sortingOrder = 1;
                        
                        ReturnTutorialData(); };
                    //tutorialChild.transform = editTeamUI.GetCardTransform();
                    break;
                }
            case 4:
                {
                    InvenUI editTeamUI = tutorialChild.transform.GetComponent<InvenUI>();
                    editTeamUI.SetOnClickAction(() => { ReturnTutorialData(); });
                    tutorialChild.transform = editTeamUI.GetCardTransform();
                    break;
                }
            case 5:
                {
                    ButtonRapper buttonRapper = tutorialChild.transform.GetComponent<ButtonRapper>();
                    buttonRapper.SetClickAddCallback(() => { ReturnTutorialData(); });
                }
                break;
            case 6:
                {
                    EquipItemSlot buttonRapper = tutorialChild.transform.GetComponent<EquipItemSlot>();
                    buttonRapper.saveAction.onClickAction = (() => { ReturnTutorialData(); });
                }
                break;

            default: break;
        }
    }
    public void InsertTutorialChildData(List<ReturnTutorialData> tutorialChildList)
    {
        for (int i = 0; i < tutorialChildList.Count; i++)
        {
            tutorialTransformQueue.Insert(0 + i, tutorialChildList[i]);
        }
    }
    public void AddTutorialChildData(List<ReturnTutorialData> tutorialChildList)
    {
        for (int i = 0; i < tutorialChildList.Count; i++)
        {
            tutorialTransformQueue.Add(tutorialChildList[i]);
        }
        /*
        for (int i = 0; i < tutorialChildList.Count; i++)
        {
            UIButton uiButton = tutorialChildList[i].transform.GetComponent<UIButton>();
            switch(tutorialChildList[i].type)
            {
                case 0:
                    uiButton.onClick.Add(new EventDelegate(() => { ReturnTutorialData(uiButton); }));
                    tutorialTransformQueue.Enqueue(tutorialChildList[i]);
                    break;
                case 1:
                    {
                        ButtonRapper buttonRapper = tutorialChildList[i].transform.GetComponent<ButtonRapper>();
                        buttonRapper.SetPressAddCallback(() => { ReturnTutorialData(); });
                        buttonRapper.SetStopPressAddCallback(() => { ReturnTutorialData(); });
                        tutorialTransformQueue.Enqueue(tutorialChildList[i]);
                    }
                    break;
                case 2:
                    EditTeamUI editTeamUI = tutorialChildList[i].transform.GetComponent<EditTeamUI>();
                    editTeamUI.SetTutorialAction(() => { ReturnTutorialData(); });
                    tutorialChildList[i].transform = editTeamUI.GetCardTransform();
                    tutorialTransformQueue.Enqueue(tutorialChildList[i]);
                    break;
                default:break;
            }
            */
        //if(uiButton != null)
        //{
        //    uiButton.onClick.Add(new EventDelegate(() => { ReturnTutorialData(uiButton); }));
        //}
        //else
        //{
        //    ButtonRapper buttonRapper = tutorialChildList[i].transform.GetComponent<ButtonRapper>();
        //    buttonRapper.SetPressAddCallback(() => { ReturnTutorialData(); });
        //}

    }
    private ReturnTutorialData DequeueTutorialTransformData()
    {
        if (tutorialTransformQueue.Count <= 0) return null;

        ReturnTutorialData dequeueTutorialData = tutorialTransformQueue[0];
        tutorialTransformQueue.Remove(dequeueTutorialData);
        AddTutorialAction(dequeueTutorialData);
        DequeueData nDequeueData = new DequeueData(dequeueTutorialData, dequeueTutorialData.transform.parent);
        dequeueDataList.Add(nDequeueData);
        //childPrevParent.GetComponent<UISprite>().depth = 10;
        return dequeueTutorialData;
    }
    public void InsertTutorialActionData(List<Action> _tutorialActionList)
    {
        for (int i = 0; i < _tutorialActionList.Count; i++)
        {
            tutorialActionQueue.Insert(0 + i, _tutorialActionList[i]);
        }
    }
    public void AddTutorialActionData(List<Action> _tutorialActionList)
    {
        for (int i = 0; i < _tutorialActionList.Count; i++)
            tutorialActionQueue.Add(_tutorialActionList[i]);
    }

    private Action DequeueTutorialActionData()
    {
        if (tutorialActionQueue.Count <= 0) return null;
        Action action = tutorialActionQueue[0];
        tutorialActionQueue.Remove(action);
        return action;
    }

    public void ReturnTutorialData(UIButton uiButton)
    {
        ReturnTutorialData();
        uiButton.onClick.Remove(new EventDelegate(() => { ReturnTutorialData(uiButton); }));
    }

    public void ReturnTutorialData()
    {
        if(isSetup)
        {
            isSetup = false;
            listPos++;
        }
        if (listPos >= listTutorial.Count)
        {
            isEnd = true;
        }
        else
        {
            buttonTouch.enabled = true;
            if (isSetup == true)
            {
                listPos++;
                isSetup = false;
            }
            CheckFirstScene();

            SetData();
        }
        if (prevTutorialType == 3)
            return;

        for (int i = 0; i < dequeueDataList.Count; i++)
        {
            dequeueDataList[0].ReturnPrevPos();
            dequeueDataList.RemoveAt(0);
        }

    }
}

public interface ISequenceTransform
{
    List<ReturnTutorialData> GetTutorialTransformList(int tutorialNum);
}

public interface ISequenceAction
{
    List<Action> GetTutorialActionList(int tutorialNum);
}

public class DequeueData
{
    public ReturnTutorialData dequeueTutorialData;
    public Transform childPrevParent;

    public DequeueData(ReturnTutorialData _dequeueTutorialData, Transform _childPrevParent)
    {
        dequeueTutorialData = _dequeueTutorialData;
        childPrevParent = _childPrevParent;
    }

    public void ReturnPrevPos()
    {
        if (childPrevParent == null || dequeueTutorialData == null)
            return;

        dequeueTutorialData.transform.parent = childPrevParent;
        childPrevParent.gameObject.SetActive(false);
        childPrevParent.gameObject.SetActive(true);
        dequeueTutorialData = null;
        childPrevParent = null;
    }
}

public class ReturnTutorialData
{
    internal Transform transform;
    internal int type;
    public ReturnTutorialData(Transform _transform, int _type)
    {
        transform = _transform;
        type = _type;
    }
}


public class SaveAction
{
    public Action onClickAction;
    public Action onPressAction;
    public Action stopPressAction;

    public void GetOnClickAction()
    {
        if (onClickAction != null)
        {
            onClickAction();
            onClickAction = null;
        }
    }
    public void GetOnPressAction()
    {
        if (onPressAction != null)
        {
            stopPressAction = onPressAction;
            onPressAction();
            onPressAction = null;
        }
    }
    public void GetStopPressAction()
    {
        if (stopPressAction != null)
        {
            stopPressAction();
            stopPressAction = null;
        }
    }
}