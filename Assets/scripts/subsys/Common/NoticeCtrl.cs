using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoticeCtrl : MonoBehaviour
{
    [SerializeField] UITweener twFading;
    [SerializeField] UILabel lbNotice;

    Queue<NoticeSData> noticeQueue = new Queue<NoticeSData>();



    private void Awake()
    {
        twFading.ResetToBeginning();
    }


    internal void EnqNotice(NoticeSData _notice)
    {
        if (noticeQueue.Contains(_notice))
            return;

        noticeQueue.Enqueue(_notice);
        gameObject.SetActive(true);
    }


    private void Update()
    {
        // 씬 전환 중에는 출력하지 않음
        if (GameCore.Instance.SubsysMgr.IsChanging)
            return;

        // 공지 사항이 출력 중에는 다음으로 넘어가지 않음
        if (twFading.enabled)
            return;

        // 더 이상 출력할 공지 사항이 없는 경우 비활성화 함
        if (noticeQueue.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        // 만약 공지시간이 만료되었다면 데이터를 제거 한다.
        if (noticeQueue.Peek().END_TIME < GameCore.nowTime)
        {
            noticeQueue.Dequeue();
            return;
        }

        ShowNotice();
    }


    private void ShowNotice()
    {
        lbNotice.text = noticeQueue.Dequeue().CONTENT;
        twFading.ResetToBeginning();
        twFading.PlayForward();
    }

    internal void ClearData()
    {
        twFading.ResetToBeginning();
        twFading.enabled = false;
        noticeQueue.Clear();
    }
}
