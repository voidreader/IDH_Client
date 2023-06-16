//#define TEST_MODE

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using System.Security.Cryptography;
using System.Reflection;



/// <summary>
/// 에셋번들 데이터를 관리한다.(현재는 로드하는 기능만 구현됨)
/// </summary>
internal class AssetBundleMgr
{
	Dictionary<string, AssetBundle> bundlePool = new Dictionary<string, AssetBundle>();

	List<string> resFileInNowLoaing = new List<string>();       // 로드중인 리소스 파일명 리스트
    public TextAsset bundle;
    private string gainedKey = "";
    private string gainedIv = ""; 

    T[] LoadAsset<T>(string _bundleName, string _assetName, bool _list) where T : UnityEngine.Object
    {
        
        _assetName = _assetName.LastIndexOf('.') <= 0 ? _assetName : _assetName.Substring(0, _assetName.LastIndexOf('.'));

#if UNITY_EDITOR

        string[] paths = null;
        if (_list)  paths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(_bundleName);
        else        paths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(_bundleName, _assetName);

        return GetObjectListOnEditor<T>(paths, _bundleName, _assetName, _list);

#elif TEST_MODE

        WWW www = new WWW(Path.Combine(FilePath.assetPath[ABType.AB_Prefab], _bundleName));

        while (!www.isDone) ;

        if (!bundlePool.ContainsKey(_bundleName))
            bundlePool.Add(_bundleName, www.assetBundle);

        return GetObjectsOnAndroid<T>(_bundleName, _assetName, _list);

#else

        AssetBundle myLoadedAssetBundle = null;
        try
        {
            Debug.Log("(I*&****&^^*&^^^*&^* Load AssetBundle : " + _bundleName);
            string path = Path.Combine(FilePath.assetPath[ABType.AB_CSV], _bundleName);

            byte[] bytes = File.ReadAllBytes(path);
            byte[] key = StringToByte(gainedKey);  
            byte[] iv = StringToByte(gainedIv);  
                
            // Decrypt assembly.  
            RC2 rc2 = new RC2CryptoServiceProvider();  
            rc2.Mode = CipherMode.CBC;  
            ICryptoTransform xform = rc2.CreateDecryptor(key, iv);  
            byte[] decrypted = xform.TransformFinalBlock(bytes, 0, bytes.Length);
            myLoadedAssetBundle = AssetBundle.LoadFromMemory(decrypted);                
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }

        bundlePool.Add(_bundleName, myLoadedAssetBundle);

        return GetObjectsOnAndroid<T>(_bundleName, _assetName, _list);
#endif
    }



    T[] LoadAssetFromMemory<T>(string _bundleName, string _assetName, bool _list) where T : UnityEngine.Object
    {
        _assetName = _assetName.LastIndexOf('.') <= 0 ? _assetName : _assetName.Substring(0, _assetName.LastIndexOf('.'));

#if UNITY_EDITOR
        string[] paths = null;
        if (_list)  paths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(_bundleName);
        else        paths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(_bundleName, _assetName);
        return GetObjectListOnEditor<T>(paths, _bundleName, _assetName, _list);
#else
        return GetObjectsOnAndroid<T>(_bundleName, _assetName, _list);
#endif
    }



    T[] GetObjectListOnEditor<T>(string[] paths, string _bundleName, string _assetName, bool _list) where T : UnityEngine.Object
    {
        T[] obj = null;
#if UNITY_EDITOR
        if (paths.Length == 0)
        {
            Debug.LogWarning("Assetbundle Load Failed : " + _bundleName + "   " + bundlePool.ContainsKey(_bundleName) + "  " + _assetName);
            if (_list == true) obj = null;
            else obj = new T[] { null };
        }
        else
        {
            obj = new T[paths.Length];
            for (int i = 0; i < paths.Length; ++i)
                obj[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(paths[i]);
        }
#endif
        return obj;
    }



    T[] GetObjectsOnAndroid<T>(string _bundleName, string _assetName, bool _list) where T : UnityEngine.Object
    {
        T[] obj = null;

        AssetBundle ab = bundlePool[_bundleName];
        if (ab == null)
        {
            Debug.LogWarning("Assetbundle Load Failed : " + _bundleName + "   " + bundlePool.ContainsKey(_bundleName));
            if (_list == true) obj = null;
            else obj = new T[] { null };
        }
        else
        {
            if (_list == true)
            {
                obj = ab.LoadAllAssets<T>();
            }
            else
            {
                var ob = ab.LoadAsset<T>(_assetName);
                if (ob != null)
                    obj = new T[] { ob };
            }
        }

        return obj;
    }



    ///<summary>
    ///암호화 시 key is too small 오류 해결 매서드
    /// * string에서 byte 배열 변환 하는 과정에서 2바이트씩 읽어야 하는데 1바이트씩 읽어 와서 생기는 오류
    ///</summary>
    private byte[] StringToByte(string hex)
    {
        int charLength = hex.Length;
        byte[] bytes = new byte[charLength / 2];
        for (int i = 0; i < charLength; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }



    /// <summary>
    /// 에셋번들 파일에서 에셋번들 데이터 로드
    /// </summary>
    /// <param name="_id">로드할 에셋번들 아이디</param>
    /// <returns>로드 되었는가 여부</returns>
    internal IEnumerator LoadAssetBundle<T>(int _id, bool _list, Action<ResourceData> _cbNewResourceLoaded) where T : UnityEngine.Object
    {
        T[] obj = null;
        //Debug.Log("AssetBundle Load " + _id);

        // 로드테이블에서 로드 데이터를 가져온다.
        var data = GameCore.Instance.DataMgr.GetAssetBundleData(_id);
        if (data == null) // 해당 ID의 데이터가 없다면 false
        {
            Debug.LogWarning("NotFind AssetBundle ID : " + _id);
            if (_cbNewResourceLoaded != null)
                _cbNewResourceLoaded(new ResourceData() { obj = new UnityEngine.Object[] { null }, type = ABType.AB_Prefab, id = _id, lifeType = -1  });
            yield break;
        }

        yield return LoadAssetBundle<T>(data.file, data.type, data.name, _list, _cbNewResourceLoaded, data.id, data.LifeType);
    }



    internal IEnumerator LoadAssetBundle<T>(int _id, string name, bool _list, Action<ResourceData> _cbNewResourceLoaded) where T : UnityEngine.Object
    {
        T[] obj = null;
        //Debug.Log("AssetBundle Load " + _id);

        // 로드테이블에서 로드 데이터를 가져온다.
        var data = GameCore.Instance.DataMgr.GetAssetBundleData(_id);
        if (data == null) // 해당 ID의 데이터가 없다면 false
        {
            Debug.LogWarning("NotFind AssetBundle ID : " + _id);
            yield break;
        }

        yield return LoadAssetBundle<T>(data.file, data.type, name, _list, _cbNewResourceLoaded, data.id, data.LifeType);
    }



    internal IEnumerator LoadAssetBundle<T>(string bundleName, ABType type, string name, bool _list, Action<ResourceData> _cbNewResourceLoaded, int _id = -1, int _lifeType = -1) where T : UnityEngine.Object
    {
        T[] obj = null;

        //Debug.Log("AssetBundle Load " + _id);

        // 해당 에셋 번들이 로드 중이라면
        if (resFileInNowLoaing.Contains(bundleName))
        { // 대기한다.
            while (resFileInNowLoaing.Contains(bundleName))
                yield return null;

            obj = LoadAssetFromMemory<T>(bundleName, name, _list);
        }
        else
        {
            // 에셋번들을 로드한다.
            //Debug.Log("AssetBundle Load : " + data.file + "   " + bundlePool.ContainsKey(data.file));
            if (!bundlePool.ContainsKey(bundleName))
            {
                resFileInNowLoaing.Add(bundleName);

                obj = LoadAsset<T>(bundleName, name, _list);

                resFileInNowLoaing.Remove(bundleName);
            }
            else
                obj = LoadAssetFromMemory<T>(bundleName, name, _list);
        }

        //Debug.Log("Assetbundle Load Success : " + data.file + "." + name);

        var rscData = new ResourceData()
        {
            id = _id,
            obj = obj,
            lifeType = _lifeType,
            type = type
        };

        _cbNewResourceLoaded(rscData);
    }



    internal void UnLoadAssetBundle(int _id)
    {
        var data = GameCore.Instance.DataMgr.GetAssetBundleData(_id);
        if (data == null)
            return;
        string bundleName = data.file;
        if (!bundlePool.ContainsKey(bundleName))
            return;
        AssetBundle assetBundle = bundlePool[bundleName];
        bundlePool.Remove(bundleName);
        assetBundle.Unload(true);
    }



    internal int LoadCount()
	{
		return resFileInNowLoaing.Count;
	}



    internal void SetDecryptKey(string _gainedKey, string _gainedIv)
    {
        gainedKey = _gainedKey;
        gainedIv = _gainedIv;
    }






    public IEnumerator LoadBundleAsync(string _bundleName, Action _cbDone)
    {
        yield return null;
        if (bundlePool.ContainsKey(_bundleName))
        {
            if (_cbDone != null)
                _cbDone.Invoke();
            yield break;
        }

#if UNITY_EDITOR
        // donothing
#else
        if (resFileInNowLoaing.Contains(_bundleName))
        { // 대기한다.
            while (resFileInNowLoaing.Contains(_bundleName))
                yield return null;
        }
        else if (!bundlePool.ContainsKey(_bundleName))
        {
            resFileInNowLoaing.Add(_bundleName);

#if TEST_MODE
            WWW www = new WWW(Path.Combine(FilePath.assetPath[ABType.AB_Prefab], _bundleName));
            yield return www;

            if (!bundlePool.ContainsKey(_bundleName))
                bundlePool.Add(_bundleName, www.assetBundle);
#else
            string path = Path.Combine(FilePath.assetPath[ABType.AB_CSV], _bundleName);

            Debug.Log("Start " + _bundleName + " Load.. " + Time.realtimeSinceStartup + " " + Time.time);
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] bytes = new byte[(int)fs.Length];
            bool bWait = true;
            fs.BeginRead(bytes, 0, (int)fs.Length, (ar) =>
            {
                bWait = false;
                if (!ar.IsCompleted)
                    Debug.LogWarning("파일 읽기 실패");
            }, null);

            while (bWait)
                yield return null;

            Debug.Log(_bundleName + " Loaded " + Time.realtimeSinceStartup + " " + Time.time);

            byte[] key = StringToByte(gainedKey);
            byte[] iv = StringToByte(gainedIv);

            // Decrypt assembly.  
            RC2 rc2 = new RC2CryptoServiceProvider();
            rc2.Mode = CipherMode.CBC;
            ICryptoTransform xform = rc2.CreateDecryptor(key, iv);
            byte[] decrypted = xform.TransformFinalBlock(bytes, 0, bytes.Length);

            Debug.Log(_bundleName + " Decrypted " + Time.realtimeSinceStartup + " " + Time.time);

            var req = AssetBundle.LoadFromMemoryAsync(decrypted);

            yield return req;

            Debug.Log(_bundleName + " Load Done " + Time.realtimeSinceStartup + " " + Time.time);

            if (!bundlePool.ContainsKey(_bundleName))
                bundlePool.Add(_bundleName, req.assetBundle);
#endif

            resFileInNowLoaing.Remove(_bundleName);
        }
#endif
        if (_cbDone != null)
            _cbDone.Invoke();
    }

}
