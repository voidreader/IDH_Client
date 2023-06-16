using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneBase : MonoBehaviour
{
	protected GameManager m_GameMgr = null;
	protected UIManager m_UIMgr = null;
	protected NetManager m_NetMgr = null;
	//protected DataManager m_DataMgr = null;
	//protected EventManager m_EventMgr = null;

	protected bool m_bIntiailized = false;
	protected ESceneType m_eCurScene = ESceneType.End;

	protected virtual void Initialize() { }
	protected virtual void InitAPI() { }
	protected virtual void Clear() { }

	protected virtual IEnumerator CoInitialize() { yield return null; }

	/* 씬 변환 상태를 조각내거나 중간에 뭔가 액션이 필요하다면 이 곳에서 컨트롤 예정.
		* 시간 관계상 미리 만들어 두진 않고 필요 시 작성 예정
	private enum ESceneState { Reset, Preload, Load, Unload, Postload, Ready, Run, Count };
	private ESceneState m_eState;

	public delegate void UpdateDelegate();
	private UpdateDelegate[] updateDelegates;

	public delegate void ReadyDelegte();
	public delegate void LoadingStopDelegate();
	public delegate void ProgressDelegate(float progress);

	public static event ReadyDelegte OnSceneReady = delegate { };
	public static event LoadingStopDelegate OnSceneLoadingStop = delegate { };
	public static event ProgressDelegate OnSceneLoading = delegate { };
	*/

	private void Start()
	{
		//if (null == m_GameMgr) m_GameMgr = GameManager.Instance;
		if (null == m_UIMgr) m_UIMgr = UIManager.Instance;
		if (null == m_NetMgr) m_NetMgr = NetManager.Instance;
		//if (null == m_DataMgr) m_DataMgr = DataManager.Instance;
		//if (null == m_EventMgr) m_EventMgr = EventManager.Instance;

		//if (null == m_TableMgr) m_TableMgr = TableManager.Instance;

		m_UIMgr.InitUIRoot();
		Initialize();
		InitAPI();
	}

	public ESceneType GetCurSceneType() { return m_eCurScene; }

	public virtual void OnAdsViewCallback(bool bFinished)
	{
        /*
		if (bFinished)
			m_NetMgr.RequestMinigameAdsVideoReward();
            */
	}

	public virtual void SetErrorMsg(string strMsg, bool bShutdown = false)
	{
        /*
		UIPopupOKCancel uiPopup = m_UIMgr.GetPopup<UIPopupOKCancel>(EUIPopup.UIPopupOKCancel, true);
		uiPopup.OpenMessage(strMsg);
        */
	}

	public virtual void SetErrorMsgGoIntro(string strMsg)
	{
        /*
		UIPopupOKCancel uiPopup = m_UIMgr.GetPopup<UIPopupOKCancel>(EUIPopup.UIPopupOKCancel, true);
		uiPopup.OpenMessage(strMsg, m_GameMgr.GoIntro);
        */
	}

	public void SetNextScene(ESceneType eScene)
	{
		if (ESceneType.End != eScene)
		{
			string strSceneName = eScene.ToString();
			SceneLoad(strSceneName);
		}
		else
		{
			Debug.LogError("Scene Name is Empty..");
		}
	}

	void SceneLoad(string strSceneName)
	{
		Clear();
		//EntityManager.Instance.Clear();
		m_UIMgr.Clear();
		System.GC.Collect();
		UnityEngine.SceneManagement.SceneManager.LoadScene(strSceneName);
	}

	protected void HideConnectServer()
	{
		//m_NetMgr.IsWaiting = false;
		m_UIMgr.SetInvisibleBlind(false);
	}
}

