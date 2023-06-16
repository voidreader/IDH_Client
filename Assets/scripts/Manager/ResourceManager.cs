using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetBundles;


public delegate void AssetDownloadDelegate(int nState, int nCount);

public class ResourceManager : SingletonInst<ResourceManager>
{
	private Dictionary<string, GameObject>	m_dicCachedUIObject		= new Dictionary<string, GameObject>();
	private Dictionary<string, GameObject>	m_dicCachedGameObject	= new Dictionary<string, GameObject>();
	private Dictionary<string, Texture>		m_dicCachedTexture		= new Dictionary<string, Texture>();

	private AssetDownloadDelegate m_DownloadDelegate = null;
	public string m_strBundleURL = string.Empty;
	private bool m_bAssetBundleSimulateMode = false;

	private EBundleState m_eState = EBundleState.NONE;

	private Dictionary<string, AssetBundle> m_dicAssetBundle = new Dictionary<string, AssetBundle>();

	public bool Initialized { get; private set; }
	public bool LoadingComplete { get { return EBundleState.LOADING_COMPLETE == m_eState; } }

	protected override void Awake()
	{
		base.Awake();

		Initialized = false;

	#if UNITY_EDITOR
		m_bAssetBundleSimulateMode = AssetBundleManager.SimulateAssetBundleInEditor;
        m_bAssetBundleSimulateMode = false;
#endif
        m_eState = EBundleState.NONE;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void ChangeBundleState(EBundleState eState)
	{
		m_eState = eState;
	}

	public void StartLoadBundles(UISprite sprProgress, UILabel txtProgValue, AssetDownloadDelegate OnCallback)
	{
		m_DownloadDelegate = OnCallback;

		if (false == Initialized)
		{
		#if UNITY_EDITOR
			m_bAssetBundleSimulateMode = AssetBundleManager.SimulateAssetBundleInEditor;
		#endif

			StartCoroutine(CoAssetBundleProc(sprProgress, txtProgValue));
		}
	}

	IEnumerator DownloadBundle(UISprite sprProgress, UILabel txtValue, int nCurrent, int nTotal, string strURL, string strDirPath, string strBundleName)
	{
		yield return null;
        m_eState = EBundleState.DOWNLOAD_COMPLETE;
	}

	IEnumerator LoadingBundle(UISprite sprProgress, UILabel txtValue, int nCurrent, int nTotal, AssetBundleManifest manifest, string strDirPath, string strBundleName)
	{
		yield return null;
	}

	public IEnumerator CoAssetBundleProc(UISprite sprProgress, UILabel txtValue)
	{
		yield return null;
        Initialized = true;
	}


	public GameObject CreateUIObject(EUIType eUIType, string strPrefName)
	{
		if (string.IsNullOrEmpty(strPrefName))
		{
			Debug.LogError("UI Object create Error. Name:" + strPrefName);
			return null;
		}

		GameObject goPref = null;
		if (m_dicCachedUIObject.TryGetValue(strPrefName, out goPref))
			return goPref;

		goPref = LoadAssetBundleUIObject(eUIType, goPref, strPrefName);
        if( !goPref)
        {
            goPref = Resources.Load("UI/" + eUIType.ToString() +"/" + strPrefName) as GameObject;
        }

        if (null == goPref)
		{
			Debug.LogError("Resoure Load Error. Path:" + strPrefName);
			return null;
		}


        m_dicCachedUIObject.Add(strPrefName, goPref);
		return goPref;
	}


	public GameObject CreateObject(EResourceType eType, string strPrefName, Vector3 vPos, Quaternion qRot, bool bCaching = false)
	{
		if (string.IsNullOrEmpty(strPrefName))
		{
			Debug.LogError("Object create Error. Path:" + strPrefName);
			return null;
		}

		if ("0" == strPrefName) return null;

		GameObject goPref = null;
		if (!bCaching || !m_dicCachedGameObject.TryGetValue(strPrefName, out goPref))
		{
			goPref = LoadAssetBundleObject(eType, goPref, strPrefName);
		}

        if (!goPref)
        {
            goPref = Resources.Load("Bg/" + strPrefName) as GameObject;
        }

        if (null == goPref)
		{
			Debug.LogError("Resource Load Error. Type:" + eType + " PrefName:" + strPrefName);
			return null;
		}

		GameObject go = Instantiate(goPref, vPos, qRot) as GameObject;
		if (null != go)
		{
			if (bCaching)
			{
				//if (bPreload)
				//	PreloadTexture(go);

				if (!m_dicCachedGameObject.ContainsKey(strPrefName))
					m_dicCachedGameObject.Add(strPrefName, goPref);
			}
		}
		return go;
	}

	public GameObject CreateObject(EResourceType eType, string strPrefName, bool bCaching = true)
	{
		if (string.IsNullOrEmpty(strPrefName))
		{
			Debug.LogError("Object create Error. Type:" + eType + " Path:" + strPrefName);
			return null;
		}
		return CreateObject(eType, strPrefName, Vector3.zero, Quaternion.identity, bCaching);
	}

	public Texture GetTexture(string strTextureName, bool bCaching = true)
	{
		if (string.IsNullOrEmpty(strTextureName))
		{
			Debug.LogError("Texture Name is null");
			return null;
		}

		Texture tex = null;
		if (!bCaching || !m_dicCachedTexture.TryGetValue(strTextureName, out tex))
		{
			tex = LoadAssetTexture(strTextureName);
		}

		if (null == tex)
		{
			Debug.LogError("Resoure Load Error. Type:Texture Name:" + strTextureName);
			return null;
		}

		if (bCaching)
		{
			if (!m_dicCachedTexture.ContainsKey(strTextureName))
				m_dicCachedTexture.Add(strTextureName, tex);
		}
		return tex;
	}

	public void SetCachedTexture(EResourceType eType, string strPrefName) // Preload
	{
		if (string.IsNullOrEmpty(strPrefName))
		{
			Debug.LogError("Resource path error. Path:" + strPrefName);
			return;
		}

		GameObject goPref = null;
		if (!m_dicCachedGameObject.TryGetValue(strPrefName, out goPref))
		{
			goPref = LoadAssetBundleObject(eType, goPref, strPrefName);
			m_dicCachedGameObject.Add(strPrefName, goPref);
		}

		if (null == goPref)
		{
			Debug.LogError("Resource Load Error... ResourcePath:" + strPrefName);
			return;
		}

		PreloadTexture(goPref);
	}

	void PreloadTexture(GameObject goTarget)
	{
		if (null == goTarget)
		{
			Debug.LogError("Failed Texture Cache.. Object Name:" + goTarget.name);
			return;
		}

		Renderer[] TargetRenderer = goTarget.GetComponentsInChildren<Renderer>(true);
		for (int i = 0; i < TargetRenderer.Length; ++i)
		{
			if (null == TargetRenderer[i].sharedMaterials || 0 >= TargetRenderer[i].sharedMaterials.Length)
				continue;

			for (int j = 0; j < TargetRenderer[i].sharedMaterials.Length; ++j)
			{
				if (TargetRenderer[i].sharedMaterials[j] == null || TargetRenderer[i].sharedMaterials[j].mainTexture)
					continue;

				Texture tex = TargetRenderer[i].sharedMaterials[j].mainTexture;
				if (!m_dicCachedTexture.ContainsKey(tex.name))
					m_dicCachedTexture.Add(tex.name, tex);
			}
		}
	}

	GameObject LoadAssetBundleUIObject(EUIType eType, GameObject goPref, string strPrefName)
	{
		string strName = string.Empty;
		if (m_bAssetBundleSimulateMode)
		{
		#if UNITY_EDITOR
			strName = string.Format("{0}{1}/{2}.prefab", CommonType.ASSET_PATH_UI, eType.ToString(), strPrefName);
			goPref = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(strName);
		#endif
		}
		else
		{
			if (m_dicAssetBundle.ContainsKey(CommonType.ASSET_BUNDLE_UI))
			{
				goPref = m_dicAssetBundle[CommonType.ASSET_BUNDLE_UI].LoadAsset<GameObject>(strPrefName);
			}
		}

		return goPref;
	}

	public GameObject LoadAssetBundleObject(EResourceType eType, GameObject goPref, string strPrefName)
	{
		string strName = string.Empty;
		if (m_bAssetBundleSimulateMode)
		{
		#if UNITY_EDITOR
			switch (eType)
			{
				case EResourceType.Body:	strName = string.Format("{0}{1}.prefab", CommonType.ASSET_PATH_BODY, strPrefName); break;
				case EResourceType.Weapon:	strName = string.Format("{0}{1}.prefab", CommonType.ASSET_PATH_WEAPON, strPrefName); break;
				case EResourceType.Head:	strName = string.Format("{0}{1}.prefab", CommonType.ASSET_PATH_HEAD, strPrefName); break;
				case EResourceType.Helmet:	strName = string.Format("{0}{1}.prefab", CommonType.ASSET_PATH_HELMET, strPrefName); break;
				case EResourceType.Effect:	strName = string.Format("{0}{1}.prefab", CommonType.ASSET_PATH_EFFECT, strPrefName); break;
				case EResourceType.Object:	strName = string.Format("{0}{1}.prefab", CommonType.ASSET_PATH_OBJECT, strPrefName); break;
				case EResourceType.Prop:	strName = string.Format("{0}{1}.prefab", CommonType.ASSET_PATH_PROP, strPrefName); break;
			}

			goPref = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(strName);
		#endif
		}
		else
		{
			switch (eType)
			{
				case EResourceType.Body:	strName = CommonType.ASSET_BUNDLE_BODY; break;
				case EResourceType.Weapon:	strName = CommonType.ASSET_BUNDLE_WEAPON; break;
				case EResourceType.Head:	strName = CommonType.ASSET_BUNDLE_HEAD; break;
				case EResourceType.Helmet:	strName = CommonType.ASSET_BUNDLE_HELMET; break;
				case EResourceType.Effect:	strName = CommonType.ASSET_BUNDLE_EFFECT; break;
				case EResourceType.Object:	strName = CommonType.ASSET_BUNDLE_OBJECT; break;
				case EResourceType.Prop:	strName = CommonType.ASSET_BUNDLE_PROP; break;
			}

			if (m_dicAssetBundle.ContainsKey(strName))
				goPref = m_dicAssetBundle[strName].LoadAsset<GameObject>(strPrefName);
		}

		return goPref;
	}

	public Material LoadMaterial(string strMaterialName) 
	{
		Material mat = null;
		string strName = string.Empty;
		if (m_bAssetBundleSimulateMode)
		{
		#if UNITY_EDITOR
			strName = string.Format("{0}{1}.mat", CommonType.ASSET_PATH_OBJECT, strMaterialName);

			mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(strName);
		#endif
		}
		else
		{
			if (m_dicAssetBundle.ContainsKey(CommonType.ASSET_BUNDLE_OBJECT))
				mat = m_dicAssetBundle[CommonType.ASSET_BUNDLE_OBJECT].LoadAsset<Material>(strMaterialName);
		}

		return mat;
	}

	Texture LoadAssetTexture(string strTexture)
	{
		Texture tex = null;
		if (m_bAssetBundleSimulateMode)
		{
		#if UNITY_EDITOR
			string strPullPath = string.Format("{0}{1}.png", CommonType.ASSET_PATH_TEXTURE, strTexture);
			tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(strPullPath);
			if (null == tex)
				Debug.LogError("Texture load Error.. Path" + strPullPath);
		#endif
		}
		else
		{
			if (!LoadingComplete)
				Debug.LogError("Asset Bundle is not loaded..");

			string strName = strTexture.ToLower();
			if (m_dicAssetBundle.ContainsKey(CommonType.ASSET_BUNDLE_TEX))
				tex = m_dicAssetBundle[CommonType.ASSET_BUNDLE_TEX].LoadAsset<Texture>(strName);

			if (null == tex)
				Debug.LogError("Texture load Error.. Path: " + CommonType.ASSET_BUNDLE_TEX + " Name: " + strName);
		}
		return tex;
	}

	public TextAsset LoadTextAsset(string strAssetName)
	{
		TextAsset tAsset = null;
		if (m_bAssetBundleSimulateMode)
		{
		#if UNITY_EDITOR
			string strPullPath = string.Format("{0}{1}.bytes", CommonType.ASSET_PATH_TABLE, strAssetName);
			tAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(strPullPath);
			if (null == tAsset)
				Debug.LogError("TableData load Error.. Path" + strPullPath);
		#endif
		}
		else
		{
			if (!LoadingComplete)
				Debug.LogError("Asset Bundle is not loaded..");

			tAsset = m_dicAssetBundle[CommonType.ASSET_BUNDLE_TABLE].LoadAsset<TextAsset>(strAssetName);
		}

		return tAsset;
	}

	public AudioClip LoadAudioClip(string strAssetName)
	{
		if (EBundleState.LOADING_COMPLETE != m_eState)
		{
			Debug.LogError("AssetBundle is not loaded");
			return null;
		}

		if (m_bAssetBundleSimulateMode)
		{
		#if UNITY_EDITOR
			string strFile = string.Format("{0}{1}", CommonType.ASSET_PATH_SOUND, strAssetName);
			AudioClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(strFile + ".mp3");
			if (clip == null)
				clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(strFile + ".wav");

			if (clip == null)
				clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(strFile + ".ogg");

			return clip;
		#endif
		}

		if (m_dicAssetBundle.ContainsKey(CommonType.ASSET_BUNDLE_SOUND))
			return m_dicAssetBundle[CommonType.ASSET_BUNDLE_SOUND].LoadAsset<AudioClip>(strAssetName);

		return null;
	}

	public override void Clear()
	{
		m_dicCachedGameObject.Clear();
		m_dicCachedTexture.Clear();
		m_dicCachedUIObject.Clear();
	}
}
