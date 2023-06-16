using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;



/// <summary>
/// 리소스데이터를 관리하기위한 구조체
/// </summary>
internal struct ResourceData
{
	internal int id;									// 리소스 ID (현재 에셋 번들 ID와 동일함)
	internal UnityEngine.Object[] obj;	// 리소스데이터
	internal ABType type;							// 리소스 타입
	internal int lifeType;						// 리소스 생명주기( -1 : 서브시스템교체까지, -2 : 고정 ) // 아직 미구현됨
}


/// <summary>
/// 게임에서 사용하는 모든 리소스를 관리
/// </summary>
internal class ResourceMgr
{
	AssetBundleMgr abMgr;																				      // 에셋번들 로드 관리
    public AssetBundleMgr AbMgr { get { return abMgr; } }

    Dictionary<ABType,Dictionary<int, ResourceData>> rscPool;					// 로드된 모든 리소스들(로컬 리소스 제외)
	Dictionary<string, UnityEngine.Object> localRscPool;                        // 로컬 리소스들(Resource폴더 속 리소스들)

	internal List<int> loadingResIds;																					// 현재 로딩중인 리소스 아이디들

	/// <summary>
	/// 생성자
	/// </summary>
	internal ResourceMgr()
	{
		if (abMgr == null)
		{
			abMgr = new AssetBundleMgr();
			rscPool = new Dictionary<ABType, Dictionary<int, ResourceData>>();
            rscPool.Add(ABType.AB_None, new Dictionary<int, ResourceData>());
			rscPool.Add(ABType.AB_Prefab, new Dictionary<int, ResourceData>());
			rscPool.Add(ABType.AB_Atlas, new Dictionary<int, ResourceData>());
			rscPool.Add(ABType.AB_Texture, new Dictionary<int, ResourceData>());
			rscPool.Add(ABType.AB_Audio, new Dictionary<int, ResourceData>());
            rscPool.Add(ABType.AB_UnityAtlas, new Dictionary<int, ResourceData>());
            rscPool.Add(ABType.AB_BattleStory, new Dictionary<int, ResourceData>());
            rscPool.Add(ABType.AB_CSV, new Dictionary<int, ResourceData>());
            // Todo : 에셋번들 타입 추가시 (테이블 추가)
        }

		if (localRscPool == null)
			localRscPool = new Dictionary<string, UnityEngine.Object>();

		loadingResIds = new List<int>();

	}

	internal int LoadingCount()
	{
		return loadingResIds.Count + abMgr.LoadCount();
	}

	/// <summary>
	/// lifeType 이 -1인 에셋을 모두 릴리즈합니다.
	/// </summary>
	internal void UnloadAllAsset()
	{
        foreach (var recDataList in rscPool)
        {
            List<int> removeIds = new List<int>();
            foreach (var recData in recDataList.Value)
            {
                if (recData.Value.lifeType != -2)
                {
                    removeIds.Add(recData.Key);
                }

            }
            RemoveFromList(recDataList.Key, removeIds);
        }

        Resources.UnloadUnusedAssets();

        var it = rscPool.GetEnumerator();
        while (it.MoveNext())
        {
            var itt = it.Current.Value.GetEnumerator();
            List<int> removeIds = new List<int>();
            while (it.MoveNext())
                if (itt.Current.Value.lifeType != -2)
                    removeIds.Add(itt.Current.Key);
            try
            {
                RemoveFromList(it.Current.Key, removeIds);
            }
            catch(KeyNotFoundException e)
            {
                Debug.Log("[Error] Check\n" + it.Current.Key + "\n");
            }
            
        }
    }

	/// <summary>
	/// _ids에 해당하는 모든 데이터를 삭제한다.
	/// </summary>
	/// <param name="_ids">삭제할 데이터의 아이디 리스트</param>
	internal void RemoveFromList(ABType _type, List<int> _ids)
	{
		for (int i = 0; i < _ids.Count; ++i)
			if (rscPool[_type].ContainsKey(_ids[i]))
			{
				rscPool[_type].Remove(_ids[i]);
				// Todo: Dispose Assetbundle
			}
	}

	/// <summary>
	/// 리소스의 오브젝트를 반환한다.
	/// </summary>
	/// <param name="_id"></param>
	/// <returns>리소스 오브젝트</returns>
	internal void GetObject<T>(ABType _type,int _id, Action<T> _cbLoadedComplete ) where T : UnityEngine.Object
	{
		if( !rscPool.ContainsKey(_type))
		{
			Debug.LogError("Not Exist Assetbundle Type " + _type);
            return;
		}

		if (rscPool[_type].ContainsKey(_id))
		{
            if (_cbLoadedComplete != null)
            {
                if (rscPool[_type][_id].obj != null)
                    _cbLoadedComplete(((T)rscPool[_type][_id].obj[0]));
                else
                    _cbLoadedComplete(null);
            }
            return;
		}

		// 로드 중인지 검사
		if (loadingResIds.Contains(_id))
		{// 로딩 대기 루틴
			GameCore.Instance.StartCoroutine(CoWaitForResLoad(_type, _id, () =>{
                if (_cbLoadedComplete != null)
                {
                    if (rscPool[_type].ContainsKey(_id) && rscPool[_type][_id].obj != null)
                        _cbLoadedComplete(((T)rscPool[_type][_id].obj[0]));
                    else
                        _cbLoadedComplete(null);
                }
			}));
			return;
		}

		//로드
		loadingResIds.Add(_id);
		GameCore.Instance.StartCoroutine(abMgr.LoadAssetBundle<T>(_id, false, (_data) =>{
			rscPool[_type].Add(_id, _data);
			loadingResIds.Remove(_id);
            if (_cbLoadedComplete != null)
            {
                if (rscPool[_type].ContainsKey(_id) && rscPool[_type][_id].obj != null)
                    _cbLoadedComplete(((T)rscPool[_type][_id].obj[0]));
                else
                    _cbLoadedComplete(null);
            }
        }));
	}
    /// <summary>
    /// 리소스의 오브젝트를 반환한다. (파일의 이름으로 번들 직접 참조)
    /// </summary>
    /// <param name="_id"></param>
    /// <returns>리소스 오브젝트</returns>
    internal void GetObjectByName<T>(ABType _type, int _id, int _bundleID, string _name, Action<T> _cbLoadedComplete) where T : UnityEngine.Object
    {
        if (!rscPool.ContainsKey(_type))
        {
            Debug.LogError("Not Exist Assetbundle Type " + _type);
            return;
        }

        if (rscPool[_type].ContainsKey(_id))
        {
            if (_cbLoadedComplete != null)
            {
                if (rscPool[_type][_id].obj != null)
                {
                    _cbLoadedComplete(((T)rscPool[_type][_id].obj[0]));
                }
                else
                {
                    Debug.LogError("Load Fail " + _type + "." + _id);
                    _cbLoadedComplete(null);
                }
            }
            return;
        }

        // 로드 중인지 검사
        if (loadingResIds.Contains(_id))
        {// 로딩 대기 루틴
            GameCore.Instance.StartCoroutine(CoWaitForResLoad(_type, _id, () => {
                if (_cbLoadedComplete != null)
                {
                    if (rscPool[_type].ContainsKey(_id) && rscPool[_type][_id].obj != null)
                        _cbLoadedComplete(((T)rscPool[_type][_id].obj[0]));
                    else
                        _cbLoadedComplete(null);
                }
            }));
            return;
        }

        //로드
        loadingResIds.Add(_id);
        GameCore.Instance.StartCoroutine(abMgr.LoadAssetBundle<T>(_bundleID, _name, false, (_data) => {
            rscPool[_type].Add(_id, _data);
            loadingResIds.Remove(_id);
            if (_cbLoadedComplete != null)
            {
                if (rscPool[_type].ContainsKey(_id) && rscPool[_type][_id].obj != null)
                    _cbLoadedComplete(((T)rscPool[_type][_id].obj[0]));
                else
                {
                    Debug.LogError("Load Fail " + _type + "." + _id);
                    _cbLoadedComplete(null);
                }
            }
        }));
    }
    /// <summary>
    /// 리소스의 오브젝트 리스트를 반환한다.
    /// </summary>
    /// <param name="_id"></param>
    /// <returns>리소스 오브젝트</returns>
    internal void GetObjectList<T>(ABType _type, int _id, Action<T[]> _cbLoadedComplete) where T : UnityEngine.Object
	{
		if (!rscPool.ContainsKey(_type))
		{
			Debug.LogError("Not Exist Assetbundle Type " + _type);
			return;
		}

		if (rscPool[_type].ContainsKey(_id))
		{
            if (_cbLoadedComplete != null)
                _cbLoadedComplete(((T[])rscPool[_type][_id].obj));
            return;
		}

		// 로드 중인지 검사
		if (loadingResIds.Contains(_id))
		{// 로딩 대기 루틴
			GameCore.Instance.StartCoroutine(CoWaitForResLoad(_type, _id, () =>
			{
                if (_cbLoadedComplete != null)
                {
                    if (rscPool[_type].ContainsKey(_id))
                        _cbLoadedComplete(((T[])rscPool[_type][_id].obj));
                    else
                        _cbLoadedComplete(null);
                }
            }));
			return;
		}

		//로드
		loadingResIds.Add(_id);
		GameCore.Instance.StartCoroutine(abMgr.LoadAssetBundle<T>(_id, true, (_data) =>
		{
			rscPool[_type].Add(_id, _data);
			loadingResIds.Remove(_id);
            if (_cbLoadedComplete != null)
                _cbLoadedComplete(((T[])rscPool[_type][_id].obj));
		}));
	}
    internal void RemoveAssetBundle(int assetID)
    {
        abMgr.UnLoadAssetBundle(assetID);
    }
    IEnumerator CoWaitForResLoad(ABType _type, int _id, Action _cb)
	{
        float waitTime = Time.time + 10;
        while (!rscPool[_type].ContainsKey(_id))
        {
            yield return null;
            if (waitTime <= Time.time)
            {
                Debug.LogError("Time Over!");
                break;
            }
        }

		_cb();
	}

	/// <summary>
	/// 리소스의 오브젝트의 본사본을 반환한다.
	/// </summary>
	/// <param name="_id"></param>
	/// <returns>리소스 오브젝트 또는 컴포넌트</returns>
	internal void GetInstanceObject(ABType _type, int _id, Action<GameObject> _cbInstantiateComplete )
	{
		if (!rscPool.ContainsKey(_type))
		{
			Debug.LogError("Not Exist Assetbundle Type " + _type);
			return;
		}

        if (rscPool[_type].ContainsKey(_id))
        {
            GameObject inputObject = null;
            if (rscPool[_type][_id].obj != null)
            {
                inputObject = rscPool[_type][_id].obj[0] as GameObject;
            }
            
            if (inputObject && _cbInstantiateComplete != null)
            {

                _cbInstantiateComplete(GameObject.Instantiate(inputObject, null));
            }

            return;
        }

        // 로드 중인지 검사
        if (loadingResIds.Contains(_id))
		{// 로딩 대기 루틴
			GameCore.Instance.StartCoroutine(CoWaitForResLoad(_type, _id, () => {
                if (rscPool.ContainsKey(_type) && rscPool[_type].ContainsKey(_id))
                {
                    if (_cbInstantiateComplete != null)
                    {
                        if (rscPool[_type][_id].obj[0] == null)
                            _cbInstantiateComplete(null);
                        else
                            _cbInstantiateComplete(GameObject.Instantiate(rscPool[_type][_id].obj[0] as GameObject, null));
                    }
                }
			}));
			return;
		}

		// 최초 로드
		loadingResIds.Add(_id);
		GameCore.Instance.StartCoroutine(abMgr.LoadAssetBundle<GameObject>(_id, false, (_data) =>{
            
            rscPool[_type].Add(_id, _data);
			if (_cbInstantiateComplete == null)
				Debug.Log("CB is Null    " + _type + " , " + _id);
			if(_data.obj == null)
				Debug.Log("Data is Null    " + _type + " , " + _id);
			loadingResIds.Remove(_id);

            if (_cbInstantiateComplete != null)
            {
                if (rscPool[_type][_id].obj[0] == null)
                {

                    _cbInstantiateComplete(null);

                   
                }
                else
                    _cbInstantiateComplete(GameObject.Instantiate(rscPool[_type][_id].obj[0] as GameObject, null));
            }
                
		}));
	}

	/// <summary>
	/// 로컬 리소스 캐싱 및 반환
	/// </summary>
	/// <typeparam name="T">리소스의 데이터형</typeparam>
	/// <param name="_name">로드하고자하는 로컬 리소스</param>
	/// <returns>성공시 해당 리소스, 실패시 null</returns>
	internal T GetLocalObject<T>(string _name , bool _cache = true) where T : UnityEngine.Object
	{
		if (localRscPool.ContainsKey(_name))
			return localRscPool[_name] as T;

		var rsc = Resources.Load<T>(_name);
		if( rsc != null )
		{
			if( _cache )
				localRscPool.Add(_name, rsc);

			return rsc as T;
		}

		Debug.LogError(_name + " Not find in Resources Folder.");
		return null;
	}

	/// <summary>
	/// 로컬 리소스 캐싱 및 반환 : 반환은 풀에서 처리.
	/// </summary>
	/// <typeparam name="T">리소스의 데이터형</typeparam>
	/// <param name="_name">로드하고자하는 로컬 리소스</param>
	/// <returns>성공시 해당 리소스, 실패시 null</returns>
	internal GameObject GetInstanceLocalObject(string _name, Transform _parent = null)
	{
		if (!localRscPool.ContainsKey(_name))
		{
			var rsc = Resources.Load<GameObject>(_name);
			if (rsc == null)
			{
				Debug.LogError(_name + " Not find in Resources Folder.");
				return null;
			}
			localRscPool.Add(_name, rsc);
		}
		return GameObject.Instantiate(localRscPool[_name], _parent) as GameObject;
	}
    internal void SetDecryptKey(string _gainedKey, string _gainedIv)
    {
        abMgr.SetDecryptKey(_gainedKey, _gainedIv);
    }


    public void PreloadingAssetBundle(int _bindleKey, Action _cbDone)
    {
        var data = GameCore.Instance.DataMgr.GetAssetBundleData(_bindleKey);

        if (data != null)
            GameCore.Instance.StartCoroutine(abMgr.LoadBundleAsync(data.file, _cbDone));
        else if (_cbDone != null)
            _cbDone();
    }

    public void PreloadingAssetBundle(string _bundleName, Action _cbDone)
    {
        GameCore.Instance.StartCoroutine(abMgr.LoadBundleAsync(_bundleName, _cbDone));
    }
}
