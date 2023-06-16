using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.UI;

internal class AssetLoader : MonoSingleton<AssetLoader>
{
    enum AssetLoadType
    {
        URL,
        LOCAL
    }

    struct AssetBundleData
    {
        public long version;
        public long size;

        public AssetBundleData(long _v, long _s)
        {
            version = _v;
            size = _s;
        }
    }

    string versionFilePath;

   
#if UNITY_ANDROID
    //string CDNServerUrl = "https://llkchpdkgbty3639394.cdn.ntruss.com/Android/";
    string CDNServerUrl = "https://dxrbwmqteggw3331372.cdn.ntruss.com/Android/";
    //string CDNServerUrl = "https://mqussttkzvqj3853381.cdn.ntruss.com/Android/";
#elif UNITY_IOS
    string CDNServerUrl = "https://llkchpdkgbty3639394.cdn.ntruss.com/iOS/";
#elif UNITY_EDITOR
    //string CDNServerUrl = "https://llkchpdkgbty3639394.cdn.ntruss.com/Android/";
    //string CDNServerUrl = "https://dxrbwmqteggw3331372.cdn.ntruss.com/Android/";
    //string CDNServerUrl = "https://mqussttkzvqj3853381.cdn.ntruss.com/Android/";
#endif


    // 에셋 번들을 저장할 경로
    string assetBundleDirectory;// = Application.persistentDataPath + "/Android";
    string versionTxt = "Version.txt";
    string assetBundleListFileName = "ABL.txt";
    AssetLoadType type;

    string serverVersionData; // 서버 버전 리스트 텍스트 임시 저장
    Dictionary<string, AssetBundleData> serverAssetBundleData = new Dictionary<string, AssetBundleData>(); // 다운로드 해야하는 에셋번들 데이터
    HashSet<string> exceptList = new HashSet<string>(); // 다운 받지말아야하는 리스트
    long downloadValue = 0;
    long serverLastVersion;

    long downloadTotalSize;

    bool downloadResult = false; // 현재 다운로드중인 파일의 성공여부 (true: 성공, false: 실패)
    bool downloadSucceed = false;

    //List<long> versionList = new List<long>();
    //string[] serverVersionList;
    //string[] versionVolumeList;
    //int serverVersionListIndex = 0;
    ////int clientVersion;
    //int appVersion;

    //string assetBundleListData;
    [SerializeField] string TestAppVersion;
    [SerializeField] int TestResourceVersion;

    Action cbComplete;
    Action cbStartDownload;
    Action<float, float, long> cbProgress;


    void Awake()
    {
        versionFilePath = Application.persistentDataPath + "/" + versionTxt;
        assetBundleDirectory = Application.persistentDataPath + "/Android";

#if UNITY_EDITOR
        //type = AssetLoadType.LOCAL;
        type = AssetLoadType.URL;
        // Test
        assetBundleDirectory = "../TestFolder/01";
        versionFilePath = "../TestFolder/Key.txt";
#else
        type = AssetLoadType.URL;
#endif

        //type = AssetLoadType.URL;

        //StartCoroutine(CheckAndGenerateDownloadList());
    }

    public void StartDownloadAssetbundle(Action _cbComplete, Action _cbStartDownload, Action<float,float,long> _cbProgress)
    {
#if TEST_MODE
        if (_cbComplete != null)
            _cbComplete.Invoke();
#else
        cbComplete = _cbComplete;
        cbStartDownload = _cbStartDownload;
        cbProgress = _cbProgress;

        // Loding등에서 터치를 못하게 막아놓은것을 다운로드중 해제함.
        // 버튼이 막혀서 사용할 수 없는 이유 : 사이드 이팩트때문에 발생한 버그로 추정됨
        // 2020-02-19 이현철
        GameCore.Instance.SetActiveBlockPanelInvisable(false);
        StartCoroutine(CheckAndGenerateDownloadList());
#endif
    }


    /// <summary>
    /// 에셋번들 다운로드 리스트 생성 (파일 크키 불일치 파일들까지 재 다운로드)
    /// 만약 다운로드가 필요하다면 팝업까지 생성
    /// 만약 다운도르가 필요없다면 즉시 종료(call LoadingSys.SetDownLoadComplete();)
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckAndGenerateDownloadList()
    {
        // Inquiry Server Version List
        /// serverVersionData = InquiryServerVersionList();
        yield return StartCoroutine(InquiryServerVersionList());

        // 로컬 클라이언트의 에셋번들 버전을 캐싱
        var clientVersion = GetClientAssetbundleVersion();

        // 다운로드 가능한 모든 버전을 정수배열화 한다.
        var versions = AllVersionList(serverVersionData);

        // 최신 버전값 저장
        serverLastVersion = versions[versions.Length - 1];

        // 다운로드해야하는 에셋번들을 serverAssetBundleData에 넣는다.
        exceptList.Clear();
        for (long i = versions.Length - 1; 0 <= i; --i)
        {
            Debug.Log("> GetAssetBundleList :: " + versions[i]);
            yield return StartCoroutine(GetAssetBundleList(versions[i], clientVersion >= versions[i]));
        }

        // 다운로드해야하는 총 용량
        downloadTotalSize = GetTotalDownLoadSize();
        Debug.Log("다운로드 : " + GetTotalDownLoadSize());

        if (downloadTotalSize != 0 && !downloadSucceed)
            ShowDownloadAgree();              // 다운로드를 묻는 팝업
        else if (cbComplete != null)
            cbComplete();                     // 다운로드를 하지 않고 바로 넘어감
    }


    /// <summary>
    /// 에셋번들 다운로드 알람 창을 띄운다.
    /// </summary>
    public void ShowDownloadAgree()
    {
        //총 용량 체크
        float totalSize = downloadTotalSize / 1000000f;

        GameCore.Instance.ShowAgree("리소스 다운로드", "다운로드가 필요한 컨텐츠가 있습니다.\n"
        + "[C][7E00FF]용량 : " + totalSize.ToString("0.##") + " Mb[-][/C]\n" + "다운로드 하시겠습니까?\n"
        + "(Wi-Fi 가 아닌 환경에서는 통신 요금이 부과될 수 있습니다.)"
        , 0, () =>
        {
            GameCore.Instance.CloseMsgWindow();

            if (cbStartDownload != null)
                cbStartDownload();

            StartCoroutine(CoRunDownload());
        }, () =>
        {
            GameCore.Instance.CloseMsgWindow();
            GameCore.Instance.ShowNotice("업데이트 에러", "리소스 버전이 달라 게임을 플레이 할 수 없습니다.\n업데이트 후 다시 시도해 주세요.", () =>
            {
                GameCore.Instance.QuitApplication();
            }, 0, false);
        }, false);
    }


    /// <summary>
    /// 다운로드
    /// </summary>
    /// <returns></returns>
    IEnumerator CoRunDownload()
    {
        // Download
        foreach (var data in serverAssetBundleData)
        {
            //Debug.Log("Start Download : " + data.Key);
            downloadResult = false;
            while (!downloadResult)
                yield return StartCoroutine(CoDownloadAssetBundle(data.Value.version, data.Key, data.Value.size));
            
            //Debug.Log("Complete Download : " + data.Key);
        }

        // save last version
        File.WriteAllText(versionFilePath, serverLastVersion.ToString());
        // End Download 
        if (cbComplete != null)
        {
            downloadSucceed = true;
            cbComplete();
        }
            
    }


    /// <summary>
    /// 서버로부터 버전 리스트를 받아온다.
    /// serverVersionData = Data;
    /// </summary>
    /// <returns></returns>
    IEnumerator InquiryServerVersionList()
    {
        // Inquiry Server Version List
        if (type == AssetLoadType.URL)
        {
            string url = string.Format("{0}{1}", CDNServerUrl, versionTxt);

            Debug.Log("cdn url :: " + url);

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.useHttpContinue = false;
                request.SendWebRequest();

                while (!request.isDone) yield return null;

                if (CheckRequestError(request))
                    yield break;

                serverVersionData = ByteToString(request.downloadHandler.data);
                Debug.Log("서버 버전 데이터 : " + serverVersionData);
            }
        }
        else
        {
            serverVersionData = File.ReadAllText(string.Format("Assets/AssetBundleTest/{0}", versionTxt));
            yield return null;
        }
    }


    /// <summary>
    /// 서버로부터 원하는 버전의 에셋번들 리스트를 받아온다.
    /// serverAssetBundleData에 저장 (AutoAddNeedDownLoadAssetBundle();)
    /// </summary>
    /// <param name="_version"></param>
    /// <returns></returns>
    IEnumerator GetAssetBundleList(long _version, bool _diff = false)
    {
        if (type == AssetLoadType.URL)
        {
            string url = string.Format("{0}{1}/{2}", CDNServerUrl, _version.ToString(), assetBundleListFileName);

            Debug.Log("> GetAssetBundleList :: " + url);
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.useHttpContinue = false;
                request.SendWebRequest();

                while (!request.isDone) yield return null;

                if (CheckRequestError(request))
                    yield break;

                var assetBundleListData = ByteToString(request.downloadHandler.data);
                Debug.Log("Version : " + _version + "\nAssetBundleListData\n\n" + assetBundleListData + "\nDiffe? : " + _diff);

                if (_diff)  AutoAddDifferentDownLoadAssetBundle(_version, assetBundleListData);
                else        AutoAddNeedDownLoadAssetBundle(_version, assetBundleListData);
            }
        }
        else
        {
            //string url = string.Format("{0}/{1}", _version.ToString(), assetBundleListFileName);
            //var assetBundleListData = File.ReadAllText("Assets/AssetBundleTest/" + url);
            //if (_diff)  AutoAddDifferentDownLoadAssetBundle(_version, assetBundleListData);
            //else        AutoAddNeedDownLoadAssetBundle(_version, assetBundleListData);
        }
    }


    /// <summary>
    /// 다운로드 해야하는 에셋번들의 데이터만 serverAssetBundleData에 저장한다.
    /// </summary>
    /// <param name="_assetBundleListData"></param>
    void AutoAddNeedDownLoadAssetBundle(long _version, string _assetBundleListData)
    {
        var assetBundleList = _assetBundleListData.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        foreach(var data in assetBundleList)
        {
            string[] temp = data.Split(new char[] { ',' });
            if (serverAssetBundleData.ContainsKey(temp[0]))
                continue;

            serverAssetBundleData.Add(temp[0], new AssetBundleData(_version, long.Parse(temp[1])));
        }
    }

    /// <summary>
    /// 파일 크키가 다른 에셋번들의 데이터만 serverAssetBundleData에 저장한다.
    /// </summary>
    /// <param name="_assetBundleListData"></param>
    void AutoAddDifferentDownLoadAssetBundle(long _version, string _assetBundleListData)
    {
        var assetBundleList = _assetBundleListData.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        foreach (var data in assetBundleList)
        {
            string[] temp = data.Split(new char[] { ',' });
            bool dirty = false;

            if (exceptList.Contains(temp[0]) || 
                serverAssetBundleData.ContainsKey(temp[0]))
                continue;

            exceptList.Add(temp[0]);

            try
            {
                var path = Path.Combine(assetBundleDirectory, temp[0]);
                if (!File.Exists(path))
                {
                    //Debug.Log(_version + " " + temp[0] + " is Update. (" + temp[1] + ") NoExist");
                    dirty = true;
                }
                else
                {
                    var fileLength = new FileInfo(path).Length;
                    if (long.Parse(temp[1]) != fileLength)
                    {
                        //Debug.Log(_version + " " + temp[0] + " is Update. (" + fileLength + " / " + temp[1] + ")");
                        dirty = true;
                    }
                }
            }
            catch(Exception e)
            {
                //Debug.Log(_version + " " + temp[0] + " is Update. (" + temp[1] + ") Error");
                Debug.LogError("Error. " + e.Message);
                dirty = true;
            }

            if (dirty)
            {
                serverAssetBundleData.Add(temp[0], new AssetBundleData(_version, long.Parse(temp[1])));
            }
        }
    }


    /// <summary>
    /// Get Client assetbundle version
    /// </summary>
    /// <returns></returns>
    int GetClientAssetbundleVersion()
    {
//#if UNITY_EDITOR
  //      return TestResourceVersion;
//#else
        if(File.Exists(versionFilePath))
        {
            int version = 0;
            try
            {
                int.TryParse(File.ReadAllText(versionFilePath), out version);
                return version;
            }
            catch { }
        }

        return 0;
//#endif
    }


    /// <summary>
    /// 다운로드 받아야하는 버전들 리스트 반환
    /// </summary>
    /// <returns></returns>
    public long[] NeedVersionList(string _serverVersionData)
    {
        var clientVersion = GetClientAssetbundleVersion();

        string[] temp = _serverVersionData.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        var versionList = new List<long>();
        for (int i = 0; i < temp.Length; i++)
        {
            string[] result = temp[i].Split(new char[] { ',' });
            var version = long.Parse(result[0]);
            if (clientVersion < version)
                versionList.Add(version);
        }

        return versionList.ToArray();
    }


    /// <summary>
    /// 모든 버전들 리스트 반환
    /// </summary>
    /// <returns></returns>
    public long[] AllVersionList(string _serverVersionData)
    {
        string[] temp = _serverVersionData.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        var versionList = new List<long>();
        for (int i = 0; i < temp.Length; i++)
        {
            string[] result = temp[i].Split(new char[] { ',' });
            versionList.Add(long.Parse(result[0]));
        }

        return versionList.ToArray();
    }

    /// <summary>
    /// 특정 에셋번들을 다운로드하여 로컬에 저장한다.
    /// </summary>
    /// <param name="_version"></param>
    /// <param name="_name"></param>
    /// <param name="_volume"></param>
    /// <returns></returns>
    IEnumerator CoDownloadAssetBundle(long _version, string _name, long _volume)
    {
        if (type == AssetLoadType.URL)
        {
            string url = "";
            if (_name == "sfx")  url = string.Format("{0}{1}/{2}", CDNServerUrl, _version, _name);
            else                url = string.Format("{0}{1}/{2}.bin", CDNServerUrl, _version, _name);

            // 웹 서버에 요청을 생성한다.
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.useHttpContinue = false;
                request.SendWebRequest();
                if (CheckRequestError(request))
                    yield break;


                while (!request.isDone)
                {
                    // 다운로드 프로그레스 갱신
                    float value = (_volume * request.downloadProgress) + downloadValue;
                    float perValue = value / downloadTotalSize;
                    if (cbProgress != null)
                        cbProgress(perValue, value, downloadTotalSize);
                    yield return null;
                }

                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.LogError("[x] " + _name + " Download Err. " + request.error);
                    yield break;
                }


                if (request.downloadHandler.data.Length < _volume)
                {
                    Debug.LogError("[x] " + _name + " Volume : " + request.downloadHandler.data.Length + " / " + _volume);
                    yield break;
                }

                string fullPath = Path.Combine(assetBundleDirectory, _name);
                string directory = Path.GetDirectoryName(fullPath);

                // 에셋 번들을 저장할 경로의 폴더가 존재하지 않는다면 생성시킨다.
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                
                //Debug.Log("Finish DownLoad: " + url);
                // Show results as text
                //Debug.Log(request.downloadHandler.text);
                //Debug.Log(fullPath);
                // 파일 입출력을 통해 받아온 에셋을 저장하는 과정

                using (FileStream fs = new FileStream(fullPath, System.IO.FileMode.Create))
                {
                    fs.Write(request.downloadHandler.data, 0, (int)request.downloadedBytes);
                    downloadValue += _volume;
                    downloadResult = true;
                }
            }
        }
        else
        {
            float progress = 0;
            while (progress < 1.1)
            {
                float value = (_volume * progress) + downloadValue;
                float perValue = value / downloadTotalSize;
                if (cbProgress != null)
                    cbProgress(perValue, value, downloadTotalSize);
                yield return new WaitForSeconds(0.1f);
                progress += 0.1f;
            }

            downloadValue += _volume;
            downloadResult = true;
        }
    }

    /// <summary>
    /// serverAssetBundleData의 데이터로 총 다운로드 사이즈를 반환한다.
    /// </summary>
    /// <returns></returns>
    long GetTotalDownLoadSize()
    {
        long totalSize = 0;
        foreach(var data in serverAssetBundleData)
            totalSize += data.Value.size;

        return totalSize;
    }


    private bool CheckRequestError(UnityWebRequest request){
        bool errorFlag = true;

        if (request.isNetworkError)
        {
            GameCore.Instance.ShowAlert("request.isNetworkError");
            Debug.LogError("request.isNetworkError");
            Debug.LogError(request.error);
            GameCore.Instance.ShowNotice("업데이트 에러", "인터넷 연결상태가 좋지 않습니다.\n확인 후 다시 시도해 주세요.", () => {
                GameCore.Instance.QuitApplication();
            }, 0, false);
        }
        else if (request.isHttpError){
            GameCore.Instance.ShowAlert("request.isHttpError");
            Debug.LogError("request.isHttpError");
            Debug.LogError(request.error);
            GameCore.Instance.ShowNotice("업데이트 에러", "인터넷 연결상태가 좋지 않습니다.\n확인 후 다시 시도해 주세요.", () => {
                GameCore.Instance.QuitApplication();
            }, 0, false);
        } else {
            errorFlag = false;
        }
        return errorFlag;
    }
	
    private string ByteToString(byte[] strByte) { string str = Encoding.Default.GetString(strByte); return str; }
}