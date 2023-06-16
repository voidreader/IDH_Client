using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum DialogueType
{
    None,
    Lobby,
    MyRoom
}
public class CharacterDialogueScript : MonoBehaviour {

   
    public static CharacterDialogueScript Create(Transform _parent)
    {
        GameObject nObj = new GameObject("CharacterDialogueMgr");
        nObj.transform.parent = _parent;
        nObj.transform.localScale = Vector3.one;
        CharacterDialogueScript characterDialogueScript = nObj.AddComponent<CharacterDialogueScript>();
        //characterDialogueScript.dialogueDataList = new List<DialogueDataStruct>();
        return characterDialogueScript;
    }

    protected class DialogueDataStruct
    {
        internal DialogueType dialogueType;
        internal Vector3 typePos;

        internal Transform parent;
        internal GameObject objDialogue;
        internal UILabel lbDialogue;
        internal Action action;
        internal float limitTime;

        public DialogueDataStruct(GameObject _obj)
        {
            parent = null;
            objDialogue = _obj;
            lbDialogue = objDialogue.GetComponentInChildren<UILabel>();
            limitTime = 0f;
        }
        public bool CheckDialogue()
        {
            if(limitTime >=0)
            {
                limitTime -= Time.deltaTime * GameCore.timeScale;
                switch(dialogueType)
                {
                    case DialogueType.Lobby:

                        //objDialogue.transform.localPosition = typePos;
                        break;
                    case DialogueType.MyRoom:
                        objDialogue.transform.localPosition = GameCore.Instance.WorldPosToUIPos(parent.position);
                        break;
                    default:
                        break;           
                }
                return true;
            }
            return false;
        }
        public void Free()
        {
            Destroy(objDialogue);
            lbDialogue = null;
            parent = null;
        }
    }


    //private List<DialogueDataStruct> dialogueDataList;
    DialogueDataStruct dialogueData;
    [SerializeField] private Vector3 myRoomPos = new Vector3(-90f, 150f, 0);
    [SerializeField] private Vector3 lobbyPos = new Vector3(-100f, 358f, 0f);


    public void FreeDialogue()
    {
        /*
        for(int i = 0; i < dialogueDataList.Count; i++)
        {
            dialogueDataList[i].Free();
        }
        dialogueDataList.Clear();
        Destroy(this.gameObject);
        */
        if(dialogueData != null)
        {
            dialogueData.Free();
            dialogueData = null;
        }
        Destroy(this.gameObject);
    }
    internal bool SetDialogue(Transform _parent, UnitDataMap _unitData, DialogueType _dialogueType, Action _action = null)
    {
        //if (objDialogue == null)
        //objDialogue = new List<DialogueDataStruct>();
        /*
        for (int i = 0; i < dialogueDataList.Count; i++)
        {
            if (dialogueDataList[i].objDialogue.activeSelf == false)
            {
                SetDialogueData(dialogueDataList[i], _parent, _unitData, _dialogueType, _action);
                return true;
            }
            else
            {
                return false;
            }
        }
        DialogueDataStruct nDialogueData = new DialogueDataStruct(GameCore.Instance.ResourceMgr.GetInstanceLocalObject("Lobby/speechBubble",this.transform));
        dialogueDataList.Add(nDialogueData);
        SetDialogueData(nDialogueData, _parent, _unitData, _dialogueType, _action);
        return true;
        */
        if(dialogueData != null)
        {
            if (dialogueData.objDialogue.activeSelf == true)
                return false;
            SetDialogueData(dialogueData, _parent, _unitData, _dialogueType, _action);
            return true;
        }
        dialogueData = new DialogueDataStruct(GameCore.Instance.ResourceMgr.GetInstanceLocalObject(CSTR.RSC_SpeechBubble, this.transform));
        SetDialogueData(dialogueData, _parent, _unitData, _dialogueType, _action);
        return true;
    }
    private void SetDialogueData(DialogueDataStruct dialogueDataStruct, Transform _parent, UnitDataMap _unitData, DialogueType _dialogueType, Action _action)
    {
        int randomValue = UnityEngine.Random.Range(0, 3);
        string _value = GameCore.Instance.DataMgr.GetProfileStringData(_unitData.charIdType).dialogues[randomValue];

        dialogueDataStruct.lbDialogue.text = _value.Replace("\\n", "\n");
        var textWidth = dialogueDataStruct.lbDialogue.width;
        //dialogueDataStruct.parent = _parent;
        dialogueDataStruct.parent = _parent;
        dialogueDataStruct.dialogueType = _dialogueType;
        switch(_dialogueType)
        {
            case DialogueType.Lobby:
                dialogueDataStruct.typePos = lobbyPos;
                dialogueDataStruct.objDialogue.transform.parent = _parent;
                dialogueDataStruct.objDialogue.transform.localPosition = lobbyPos - new Vector3(textWidth, 0f, 0f);
                break;
            case DialogueType.MyRoom:
                dialogueDataStruct.action = _action;
                dialogueDataStruct.typePos = myRoomPos;
                dialogueDataStruct.objDialogue.transform.GetChild(0).localPosition = myRoomPos - new Vector3(textWidth, 0f, 0f);
                dialogueDataStruct.objDialogue.transform.localPosition = GameCore.Instance.WorldPosToUIPos(_parent.position);
                break;
            default:
                dialogueDataStruct.typePos = Vector3.zero;
                break;
        }
        GameCore.Instance.SoundMgr.SetCharacterVoiceSound(_unitData, randomValue);
        dialogueDataStruct.limitTime = GameCore.Instance.SoundMgr.GetCharacterVoiceClipLength(_unitData, randomValue);
        dialogueDataStruct.objDialogue.SetActive(true);
    }
    private void ResetDialogue(DialogueDataStruct dialogueDataStruct)
    {
        if (dialogueDataStruct.action != null) dialogueDataStruct.action();
        dialogueDataStruct.action = null;

        dialogueDataStruct.parent = null;
        dialogueDataStruct.objDialogue.transform.parent = this.transform;
        dialogueDataStruct.objDialogue.SetActive(false);
    }
    private void Update()
    {
        /*
        if (dialogueDataList.Count == 0)
            return;
        for(int i = 0; i < dialogueDataList.Count; i++)
        {
            if (dialogueDataList[i].objDialogue.activeSelf == false)
                continue;
            if (dialogueDataList[i].CheckDialogue() == false)
                ResetDialogue(dialogueDataList[i]);
        }
        */
        if (dialogueData == null)
            return;
        if (dialogueData.objDialogue.activeSelf == false)
            return;
        if (dialogueData.CheckDialogue() == false)
            ResetDialogue(dialogueData);
    }

}
