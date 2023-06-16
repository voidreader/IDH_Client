using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIManager : SingletonInst<UIManager>
{
    private Dictionary<EUIPage, UIBase> m_dicUIPage = new Dictionary<EUIPage, UIBase>();
    private Dictionary<EUIPopup, UIBase> m_dicUIPopup = new Dictionary<EUIPopup, UIBase>();
    private Dictionary<EUIComponents, GameObject> m_dicUICompo = new Dictionary<EUIComponents, GameObject>();

    private List<EUIPage> m_listUIPage = new List<EUIPage>();

    private ResourceManager m_ResourceMgr = null;

    private GameObject m_goUIRoot = null;
    private GameObject m_goRootHud = null;
    private GameObject m_goRootPage = null;
    private GameObject m_goRootPopup = null;
    private GameObject m_goRootSystem = null;

    private EUIPage m_eCurPage = EUIPage.End;


    protected override void Awake()
    {
        //m_arrCameras = new BaseCamera[(int)ECameraIndex.End];
        m_ResourceMgr = ResourceManager.Instance;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void Init()
    {
        Clear();
    }

    public override void Clear()
    {
        m_dicUIPage.Clear();
        m_dicUIPopup.Clear();
        m_dicUICompo.Clear();
        m_listUIPage.Clear();
    }

    public GameObject GetUIRoot() { return m_goUIRoot; }
    public GameObject GetParentHud() { return m_goRootHud; }
    public GameObject GetParentPage() { return m_goRootPage; }
    public GameObject GetParentPopup() { return m_goRootPopup; }
    public GameObject GetParentSystem() { return m_goRootSystem; }

    public void SetParentPage(GameObject rootPage) { m_goRootPage = rootPage; }
    public void SetParentPopup(GameObject rootPage) { m_goRootPopup = rootPage; }

    public void InitUIRoot()
    {
        Clear();

        m_goUIRoot = GameObject.Find(CommonType.UI_ROOT_NAME);
        if (null == m_goUIRoot)
        {
            string strPath = string.Format("{0}{1}", CommonType.PATH_UI_ROOT, CommonType.UI_ROOT_NAME);
            GameObject goPref = Resources.Load(strPath) as GameObject;
            if (null != goPref)
                m_goUIRoot = Instantiate(goPref) as GameObject;
        }

        if (null != m_goUIRoot)
        {
            Transform tfUIRoot = m_goUIRoot.transform;
            m_goRootHud = tfUIRoot.Find(CommonType.UI_ROOT_HUD).gameObject;
            m_goRootPage = tfUIRoot.Find(CommonType.UI_ROOT_PAGE).gameObject;
            m_goRootPopup = tfUIRoot.Find(CommonType.UI_ROOT_POPUP).gameObject;
            m_goRootSystem = tfUIRoot.Find(CommonType.UI_ROOT_SYSTEM).gameObject;
        }
    }

    private UIBase LoadPage(EUIPage ePage)
    {
        GameObject goPref = m_ResourceMgr.CreateUIObject(EUIType.Page, ePage.ToString());
        if (null != goPref)
        {
            GameObject goBase = NGUITools.AddChild(GetParentPage().gameObject, goPref);
            if (null != goBase)
            {
                UIBase uiBase = goBase.GetComponent<UIBase>();
                if (m_dicUIPage.ContainsKey(ePage))
                    m_dicUIPage.Remove(ePage);
                m_dicUIPage.Add(ePage, uiBase);

                return uiBase;
            }
        }
        return null;
    }

    private UIBase LoadPopup(EUIPopup ePopup, bool bSystem)
    {
        UIBase uiBase = null;

        GameObject goPref = m_ResourceMgr.CreateUIObject(EUIType.Popup, ePopup.ToString());
        if (null != goPref)
        {
            if (m_dicUIPopup.TryGetValue(ePopup, out uiBase))
                return uiBase;

            GameObject goBase = null;
            goBase = (bSystem) ? NGUITools.AddChild(GetParentSystem().gameObject, goPref) : NGUITools.AddChild(GetParentPopup().gameObject, goPref);
            if (null != goBase)
            {
                uiBase = goBase.GetComponent<UIBase>();
                m_dicUIPopup.Add(ePopup, uiBase);
            }
        }
        return uiBase;
    }

    public GameObject LoadComponent(EUIComponents eCompo)
    {
        GameObject goPref = null;
        if (m_dicUICompo.TryGetValue(eCompo, out goPref))
            return goPref;

        goPref = m_ResourceMgr.CreateUIObject(EUIType.Components, eCompo.ToString());
        if (null != goPref)
        {
            m_dicUICompo.Add(eCompo, goPref);
            return goPref;
        }
        return null;
    }

    public T GetPage<T>(EUIPage ePage) where T : UIBase
    {
        UIBase uiBase = null;
        if (m_dicUIPage.TryGetValue(ePage, out uiBase))
        {
            if (uiBase != null)
            {
                return uiBase as T;
            }
            else
            {
                m_dicUIPage.Remove(ePage);
                return LoadPage(ePage) as T;
            }
        }
        return LoadPage(ePage) as T;
    }

    public UIBase GetPage(EUIPage ePage)
    {
        UIBase uiBase = null;
        if (m_dicUIPage.TryGetValue(ePage, out uiBase))
        {
            if (uiBase != null)
            {
                return uiBase;
            }
            else
            {
                m_dicUIPage.Remove(ePage);
                return LoadPage(ePage);
            }
        }
        return LoadPage(ePage);
    }

    public T GetPopup<T>(EUIPopup ePopup, bool bSystem = false) where T : UIBase
    {
        UIBase uiBase = null;
        if (m_dicUIPopup.TryGetValue(ePopup, out uiBase))
            return uiBase as T;
        return LoadPopup(ePopup, bSystem) as T;
    }

    public UIBase GetPopup(EUIPopup ePopup, bool bSystem = false)
    {
        UIBase uiBase = null;
        if (m_dicUIPopup.TryGetValue(ePopup, out uiBase))
            return uiBase;
        return LoadPopup(ePopup, bSystem);
    }

    public void HidePage(EUIPage ePage)
    {
        if (m_dicUIPage.ContainsKey(ePage))
            m_dicUIPage[ePage].Close();
    }

    public void HidePageAll(bool bClearStack = false)
    {
        var varPage = m_dicUIPage.GetEnumerator();
        while (varPage.MoveNext())
        {
            UIBase uiPage = varPage.Current.Value;
            if (null != uiPage)
                uiPage.Close();
        }

        if (bClearStack)
            m_listUIPage.Clear();
    }

    public void HidePopup(EUIPopup ePopup)
    {
        if (m_dicUIPopup.ContainsKey(ePopup))
            m_dicUIPopup[ePopup].Close();
    }

    public void HidePopupAll()
    {
        var varPopup = m_dicUIPopup.GetEnumerator();
        while (varPopup.MoveNext())
        {
            UIBase uiPopup = varPopup.Current.Value;
            if (null != uiPopup)
                uiPopup.Close();
        }
    }

    public void OpenStackPage(EUIPage ePage)
    {
        if (ePage != m_eCurPage) HidePage(m_eCurPage);

        if (!m_dicUIPage.ContainsKey(ePage))
            LoadPage(ePage);

        m_eCurPage = ePage;
        m_dicUIPage[ePage].Open();

        // Page Stack
        if (m_listUIPage.Contains(ePage))
            m_listUIPage.Remove(ePage);
        m_listUIPage.Add(ePage);
    }

    public void CloseStackPage(EUIPage ePage)
    {
        if (m_listUIPage.Contains(ePage))
            m_listUIPage.Remove(ePage);

        if (0 != m_listUIPage.Count)
        {
            int nLastIndex = m_listUIPage.Count - 1;
            OpenStackPage(m_listUIPage[nLastIndex]);
        }
    }

    public void UpdateGoods()
    {
        if (EUIPage.End != m_eCurPage) GetPage(m_eCurPage).UpdateGoods();
    }

    public void SetLoading(bool bShow)
    {
        //if (bShow) GetPopup<UILoading>(EUIPopup.UILoading).Open();
        //else GetPopup<UILoading>(EUIPopup.UILoading).Close();
    }

    public void SetInvisibleBlind(bool bIs)
    {
        //if (bIs)
        //    GetPopup<UIPopupConnect>(EUIPopup.UIPopupConnect, true).Open();
        //else
        //    GetPopup<UIPopupConnect>(EUIPopup.UIPopupConnect, true).Close();
    }

    public void PlaySFX(ESoundID eID)
    {
        //SoundManager.Instance.MakeSFX(eID);
    }

    #region Camera Varialble
    //private BaseCamera[] m_arrCameras = null;
    private ECameraType m_eCamType = ECameraType.Follow;
    /*
    public void AddCamera(ECameraIndex eIndex, BaseCamera cCamera)
    {
        if (null == m_arrCameras)
            return;

        if (null == cCamera)
            return;

        int nIndex = (int)eIndex;
        m_arrCameras[nIndex] = cCamera;
    }

    public void RemoveCamera(ECameraIndex eIndex)
    {
        int nIndex = (int)eIndex;
        if (null == m_arrCameras || nIndex >= m_arrCameras.Length)
            return;

        m_arrCameras[nIndex] = null;
    }

    public void SetCameraMode(ECameraType eCamType)
    {
        if (eCamType == m_eCamType)
            return;

        if (ECameraType.Follow == eCamType || ECameraType.FollowTarget == eCamType)
            m_eCamType = eCamType;
    }

    public Camera GetCamera(ECameraIndex eIndex)
    {
        int nIndex = (int)eIndex;
        if (eIndex < 0 || m_arrCameras == null || eIndex >= ECameraIndex.End || m_arrCameras[nIndex] == null)
            return null;

        return m_arrCameras[nIndex].GetCamera();
    }

    public ECameraType GetCameraType()
    {
        return m_eCamType;
    }

    public void SetCameraEnable(ECameraIndex eIndex, bool bEnable)
    {
        if (ECameraIndex.End == eIndex)
            return;

        m_arrCameras[(int)eIndex].SetCameraEnable(bEnable);
    }
    */
    #endregion

}

