using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
	[HideInInspector] public GameObject m_CachedObject = null;
	[HideInInspector] public Transform m_CachedTransform = null;

	private bool m_bInitialized = false;
	private bool m_bActive = false;

	protected UIManager m_UIMgr = null;
	//protected DataManager m_DataMgr = null;
	protected NetManager m_NetMgr = null;
	protected SoundManager m_SoundMgr = null;

	//protected PageTitle m_Title = null;
	protected System.Action m_cbBackButton = null;

	public bool IsActivate { get { return (m_bActive && m_CachedObject.activeSelf) ? true : false; } }

	private void Awake()
	{
		m_CachedObject = gameObject;
		m_CachedTransform = transform;

		Initialize();
	}

	private void Start()
	{
		m_CachedObject.SetActive(m_bActive);
	}

	public virtual void Initialize()
	{
		if (!m_bInitialized)
		{
			m_bInitialized	= true;
			m_bActive		= true;
		}

		//m_UIMgr	= UIManager.Instance;
		//m_DataMgr = DataManager.Instance;
		//m_NetMgr = NetManager.Instance;
		//m_SoundMgr = SoundManager.Instance;
	}

	public virtual void Open()
	{
		m_bActive = true;
		m_CachedObject.SetActive(m_bActive);

		OpenProcess();
	}

	public virtual void Close()
	{
		m_bActive = false;
		m_CachedObject.SetActive(m_bActive);

		CloseProcess();
	}

	protected virtual void InitTitleNGoods(EUIPage ePage, string strTitle, System.Action OnCallback = null)
	{
        /*
		if (null == m_Title)
		{
			GameObject goPref = m_UIMgr.LoadComponent(EUIComponents.PageTitle);
			if (null != goPref)
				m_Title = NGUITools.AddChild(m_CachedObject, goPref).GetComponent<PageTitle>();
		}

		m_Title.Init(ePage, strTitle, m_DataMgr.GetMyInfo().GetGoodsValue(EGoodsType.Coin), m_DataMgr.GetMyInfo().GetGoodsValue(EGoodsType.Cocoa), OnCallback);
        */
	}

	public virtual void UpdateGoods()
	{
		//m_Title.UpdateGoods(m_DataMgr.GetMyInfo().GetGoodsValue(EGoodsType.Coin), m_DataMgr.GetMyInfo().GetGoodsValue(EGoodsType.Cocoa));
	}

	private void OnDestroy()
	{
		DestroyProcess();
	}

	protected virtual void OpenProcess() { }
	protected virtual void CloseProcess() { }
	protected virtual void DestroyProcess() { }
}
