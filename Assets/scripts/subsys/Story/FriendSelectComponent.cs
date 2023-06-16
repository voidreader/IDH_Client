using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendSelectComponent : MonoBehaviour
{
    [SerializeField] UITable friendListRoot;

    [SerializeField] UIGrid myFriendGrid;
    [SerializeField] UIGrid rcmdFriendGrid;

    [SerializeField] GameObject goEmptyMyFriend;
    [SerializeField] GameObject goEmptyRcmdFriend;


    List<FriendSelectItem> friendItems = new List<FriendSelectItem>();
    int selectedFriendindex = -1;

    internal int SelectedFriendindex { get { return selectedFriendindex; } }


    bool bDelayedRePosition = false;

    internal FriendSelectItem SelectFriendItem
    {
        get
        {
            if (selectedFriendindex < 0 || friendItems.Count <= selectedFriendindex)
                return null;
            else
                return friendItems[selectedFriendindex];
        }
    }



    public void SetFriend(FriendSData[] _myFriends, FriendSData[] _rcmdFriends)
    {
        if (friendItems.Count != 0)
        {
            for (int i = 0; i < friendItems.Count; ++i)
                Destroy(friendItems[i].gameObject);
            friendItems.Clear();
        }

        bool bFirstSelected = false;

        if (_myFriends != null)
        {
            for (int i = 0; i < _myFriends.Length; ++i)
            {
                var item = FriendSelectItem.Create(myFriendGrid.transform);
                item.Init(_myFriends[i], OnSelectFriend);
                friendItems.Add(item);
            }

            if (_myFriends.Length != 0)
            {
                bFirstSelected = true;
                OnSelectFriend(_myFriends[0].USER_UID);
            }
        }

        if (_rcmdFriends != null)
        {
            for (int i = 0; i < _rcmdFriends.Length; ++i)
            {
                var item = FriendSelectItem.Create(rcmdFriendGrid.transform);
                item.Init(_rcmdFriends[i], OnSelectFriend);
                friendItems.Add(item);
            }

            if (!bFirstSelected && _rcmdFriends.Length != 0)
                OnSelectFriend(_rcmdFriends[0].USER_UID);
        }

        goEmptyMyFriend.SetActive(_myFriends == null || _myFriends.Length == 0);
        goEmptyRcmdFriend.SetActive(_rcmdFriends == null || _rcmdFriends.Length == 0);

        
        if (gameObject.activeInHierarchy)
            StartCoroutine(DelayedReposition());
        else
            bDelayedRePosition = true;
            
    }

    private void OnEnable()
    {
        if(bDelayedRePosition)
        {
            bDelayedRePosition = false;
            StartCoroutine(DelayedReposition());
        }
    }

    IEnumerator DelayedReposition()
    {
        myFriendGrid.enabled = true;
        rcmdFriendGrid.enabled = true;
        yield return null;
        if (friendItems.Count > 0)
            friendListRoot.enabled = true;
    }

    public void OnSelectFriend(long _friendUID)
    {
        // find select Item
        int idx = -1;
        for (int i = 0; i < friendItems.Count; ++i)
            if (friendItems[i].GetUID() == _friendUID)
            {
                idx = i;
                break;
            }

        if (idx == -1)
        {
            Debug.LogError("Not Found Friend UID! " + _friendUID);
            return;
        }

        // off prev selected
        if (selectedFriendindex != -1)
            friendItems[selectedFriendindex].SetSelect(false);

        // if toggle
        if (selectedFriendindex == idx)
        {
            selectedFriendindex = -1;
            return;
        }

        // on now Selected
        friendItems[idx].SetSelect(true);

        // save prev
        selectedFriendindex = idx;
    }
}
