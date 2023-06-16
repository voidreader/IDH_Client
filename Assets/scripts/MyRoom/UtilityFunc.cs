using UnityEngine;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.IO;

public class UtilityFunc : MonoBehaviour
{
    #region Singleton

    private static UtilityFunc UtilityInst = null;

    public static UtilityFunc Inst
    {
        get
        {
            if (null == UtilityInst)
            {
                UtilityInst = FindObjectOfType(typeof(UtilityFunc)) as UtilityFunc;

                if (null == UtilityInst)
                {
                    GameObject utility = new GameObject("UtilityFunc");
                    UtilityInst = utility.AddComponent<UtilityFunc>() as UtilityFunc;
                }
            }

            return UtilityInst;
        }
    }

    #endregion

    //private Encoding encoding = Encoding.GetEncoding("euc-kr");
    //private Encoding encoding = Encoding.GetEncoding(51949);

    GameObject effectPool;
    GameObject etcPool;

    public string _secretKey;
    void Awake()
    {
        effectPool = GameObject.Find("EffectPool");
        etcPool = GameObject.Find("EtcPool");
    }


    public GameObject GetChildObj(string strName, GameObject gameObject = null) 
    {
        Transform[] childTM;
        // ture 속성이 있어야 검색된다.
        if (gameObject != null)
            childTM = gameObject.GetComponentsInChildren<Transform>(true);            
        else
            childTM = this.gameObject.GetComponentsInChildren<Transform>(true);

        foreach (Transform Obj in childTM)
        {
            // 프리펩의 순차로 검색한다.
            // 빠르게 검색하고 싶은것은 프리펩상에서 위로 올려야한다.
            //Debug.Log("Obj.name" + Obj.name);
            if (Obj.name == strName)
            {
                return Obj.gameObject;
            }
        }
        return null;
    }
     
    public GameObject GetChildObj(GameObject gameObject, string strName, bool bContained = false)
    {
        // ture 속성이 있어야 숨겨진( Active = false 인 오브젝트 ) 검색된다.
        Transform[] childTM = gameObject.GetComponentsInChildren<Transform>(true);

        foreach (Transform Obj in childTM)
        {
            if (bContained != true)
            {
                if (Obj.name == strName)
                    return Obj.gameObject;
            }
            else
            {
                if (Obj.name.Contains(strName))
                    return Obj.gameObject;
            }
        }
        return null;
    }

    public string getEucKrString(string eucKrString)
    {
        return eucKrString;

        //byte[] eucKrByte = encoding.GetBytes(eucKrString);
        //return encoding.GetString(eucKrByte);

        //byte[] eucKrByte2 = Encoding.Convert(encoding, Encoding.UTF8, eucKrByte);
        //return Encoding.UTF8.GetString(eucKrByte2);
    }

    public UIAtlas getAtlas(string path)
    {
        //return Resources.Load("UI_Resources/Familier_Face/UI_Face_01", typeof(UIAtlas)) as UIAtlas
        return Resources.Load(path, typeof(UIAtlas)) as UIAtlas;
    }

    //숫자 1천 단위로 , 찍어주기
    public string maskMoney(long field)
    {
        if ( field == 0 )
	        return "0";

        string money = "" + field;
        int len = money.Length;
        string minus = "N";

        // 음수의 값이 넘어온다면 음수 표시를 제거하고 처리 한 후 - 부호를 붙인다.
        if ( money.Substring(0,1) == "-" )
        {
	        minus = "Y";
	        money = money.Substring(1, money.Length );
	        len = money.Length;
        }

        var result = "";
    
        if(len > 3)
        {
            if(len % 3 != 0){
                result = result + money.Substring(0, len % 3) + ",";
           }

           money = money.Substring(len % 3); 
           len = money.Length; 

           while(len > 3)
           {
              result = result + money.Substring(0, 3) + ","; 
              money = money.Substring(3); 
              len = money.Length;
           }

           result = result + money;
        } else{
            result = money; 
        }

        // 음수여부  체크 후 '-' 부호를 붙인다.
        if ( minus == "Y" )
        {
            return "-"+result;
        }
        else  return result; 
    }

    //자식들 없애기
    public void DestroyAllChild(string parentNm, GameObject parentObj)
    {
        Transform[] childTM;
        childTM = parentObj.GetComponentsInChildren<Transform>(true);
        foreach (Transform Obj in childTM)
        {
            if (Obj.gameObject.name != parentNm)
            {
                Obj.parent = null;
                GameObject.Destroy(Obj.gameObject);
            }
        }
    }
    //자식들 안보이게하기
    public void DeActivateChildren(string parentNm, GameObject parentObj)
    {
        Transform[] childTM;
        childTM = parentObj.GetComponentsInChildren<Transform>(true);
        foreach (Transform Obj in childTM)
        {
            if (Obj.gameObject.name != parentNm)
                Obj.gameObject.SetActive(false);
        }
    }

    public void ActivateAll( GameObject parentObj)
    {
        Transform[] childTM;
        childTM = parentObj.GetComponentsInChildren<Transform>(true);
        foreach (Transform Obj in childTM)
        {
            //if (Obj.gameObject.name != parentNm)
                Obj.gameObject.SetActive(true);
        }
    }


    public int CalcRemainder(int src, int divideUnit)
    {
        //if (src < divideUnit)
        //    return 0;

        int rtn = 0;
        rtn = src % divideUnit;

        if (rtn == src)
        {
            return src;
            //return src / (divideUnit / 10);
        }

        return rtn;
    }

    List<string> spawnList = new List<string>();

    public bool AddSpawnList(string strName)
    {
        for( int i=0; i < spawnList.Count; i++ )
        {
            if (spawnList[i] == strName)
            {
                return false;
            }
        }

        spawnList.Add(strName);

        //for (int i = 0; i < spawnList.Count; i++)
        //    Debug.LogError("spawnList[i] = "+ spawnList[i]);

        return true;
    }

    public bool IsExistSpawnList(string strName)
    {
        if (spawnList.Contains(strName))
            return true;

        return false;
    }

    public void ClearSpawnList()
    {
        spawnList.Clear();
    }

    public void ChangeAnimatorClip(Animator animator, string aniName, AnimationClip aniClip)
    {
        RuntimeAnimatorController myController = animator.runtimeAnimatorController;
        AnimatorOverrideController myOverrideController = new AnimatorOverrideController();
        myOverrideController.runtimeAnimatorController = myController;
        myOverrideController[aniName] = aniClip;
        animator.runtimeAnimatorController = myOverrideController;
    }

    public string ConvertToTime( float fRemainedTime)
    {
        if (fRemainedTime > 0)
        {
            float fHour = fRemainedTime / 60 / 60;
            int h = Mathf.FloorToInt(fHour);

            float fMin = (fRemainedTime - h * 60 * 60);
            int m = Mathf.FloorToInt(fMin / 60);

            //int s = Mathf.FloorToInt((fRemainedTime - m * 60));
            int s = Mathf.FloorToInt(fMin % 60);

            return string.Format("{0:D2}:{1:D2}:{2:D2}", h, m, s);
            //return string.Format("{0:D2}:{1:D2}", m, s);
        }

        return string.Format("{0:D2}:{1:D2}:{2:D2}", 0, 0, 0); ;
        //return string.Format("{0:D2}:{1:D2}", 0, 0); ;  
    }


    public void _refresh_shader(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            Material sharedMaterial = renderers[i].sharedMaterial;

            if (sharedMaterial != null)
            {
                if (sharedMaterial.shader != null)
                {
                    string shaderName = sharedMaterial.shader.name;
                    Shader newShader = Shader.Find(shaderName);

                    if (newShader != null)
                        renderers[i].sharedMaterial.shader = newShader;
                }
            }
        }
    }

    IEnumerator LoadObject(string path)
    {
        using (WWW www = new WWW(path))
        {
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                //DebugConsole.I.AddLog(string.Format("ResourcesLoadManager: LoadObject() :{0} WWWError", www.error));
            }
            else
            {
                if (www != null && www.assetBundle.mainAsset != null)
                {
                    //m_objectList[name] = (GameObject)www.assetBundle.mainAsset;
                }

                www.Dispose();
            }
        }
    }

    // 파일이 있는지 검사를 하지만 버젼검사는 아니다.
    IEnumerator LoadBundle()
    {
        string bundlePath = "http://lazli.tistory.com/attachment/cfile26.uf@2522EE4451CAA1FA06B1C9.unity3d";
        string docPath = Application.dataPath + "/Resources/Bundle/assetbundle.unity3d";
        Debug.Log("docPath=" + docPath);

        FileInfo fInfo = new FileInfo(docPath);
        WWW www;
        if (!fInfo.Exists)
        {
            www = new WWW(bundlePath);
            Debug.Log(Application.dataPath);
            yield return www;

            if (www.isDone)
            {
                FileStream fs = new FileStream(docPath, FileMode.CreateNew);
                BinaryWriter w = new BinaryWriter(fs);
                w.Write(www.bytes);
                w.Close();
                fs.Close();
            }
        }

        www = new WWW("file:///" + docPath);
        Debug.Log(Application.dataPath);
        yield return www;
        if (www.isDone)
        {
            //GameObject obj = Instantiate((GameObject)(www.assetBundle.mainAsset)) as GameObject;
            GameObject obj = Instantiate(www.assetBundle.LoadAsset("ABTestPrefab", typeof(GameObject))) as GameObject;


            Vector3 localPos = obj.transform.localPosition;
            Vector3 localScale = obj.transform.localScale;

            obj.transform.parent = GameObject.Find("Anchor").transform;

            obj.transform.localPosition = localPos;
            obj.transform.localScale = localScale;
        }
    }

    /*
    public void ReadVersionFile(string output_path)
    {
        StreamReader fileReader = File.OpenText(output_path);
        char[] separator = new char[] { ',' };
        string line;
        string[] split;

        // 헤더 스킵
        line = fileReader.ReadLine();
        split = line.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);

        AssetBundleManager.Inst.clientReleaseVersion = float.Parse(split[1]);
        AssetBundleManager.Inst.clientBuildTarget = split[2];

        while ((line = fileReader.ReadLine()) != null)
        {
            if (string.IsNullOrEmpty(line)) break;
            split = line.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);

            string url = split[0];
            int version = int.Parse(split[1]);
            uint crc = uint.Parse(split[2]);
            Debug.LogError("client url = " + url + " version = " + version + " crc = " + crc);

            AssetBundleManager.Inst.AddClientReleaseRef(url, version, crc);

        }

        fileReader.Close();
    }
    */
/*
[윈도우 에디터]
Application.persistentDataPath : 사용자디렉토리/AppData/LocalLow/회사이름/프로덕트이름
파일 읽기 쓰기 가능
Application.dataPath : 프로젝트디렉토리/Assets
Application.streamingAssetsPath : 프로젝트디렉토리/Assets/StreamingAssets
파일 읽기 쓰기 가능


[윈도우 응용프로그램]
Application.persistentDataPath : 사용자디렉토리/AppData/LocalLow/회사이름/프로덕트이름
파일 읽기 쓰기 가능
Application.dataPath : 실행파일/실행파일_Data
Application.streamingAssetsPath : 실행파일/실행파일_Data/StreamingAssets
파일 읽기 쓰기 가능

[맥 에디터]
Application.persistentDataPath : 사용자디렉토리/Library/Caches/unity.회사이름.프로덕트이름
파일 읽기 쓰기 가능
Application.dataPath : 프로젝트디렉토리/Assets
Application.streamingAssetsPath : 프로젝트디렉토리/Assets/StreamingAssets
파일 읽기 쓰기 가능

[맥 응용프로그램]
Application.persistentDataPath : 사용자디렉토리/Library/Caches/unity.회사이름.프로덕트이름
파일 읽기 쓰기 가능
Application.dataPath : 실행파일.app/Contents
Application.streamingAssetsPath : 실행파일.app/Contents/Data/StreamingAssets
파일 읽기 쓰기 가능

[웹 플랫폼]
웹에서는 명시적인 파일 쓰기 불가능. 애셋번들로 해야함
Application.persistentDataPath : /
Application.dataPath : unity3d파일이 있는 폴더
Application.streamingAssetsPath : 값 없음.

[안드로이드 External]
Application.persistentDataPath : /mnt/sdcard/Android/data/번들이름/files
파일 읽기 쓰기 가능
Application.dataPath : /data/app/번들이름-번호.apk
Application.streamingAssetsPath : jar:file:///data/app/번들이름.apk!/assets 
파일이 아닌 WWW로 읽기 가능

[안드로이드 Internal] 
Application.persistentDataPath : /data/data/번들이름/files/
파일 읽기 쓰기 가능
Application.dataPath : /data/app/번들이름-번호.apk
Application.streamingAssetsPath : jar:file:///data/app/번들이름.apk!/assets
파일이 아닌 WWW로 읽기 가능

[iOS]
Application.persistentDataPath : /var/mobile/Applications/프로그램ID/Documents 
파일 읽기 쓰기 가능
Application.dataPath : /var/mobile/Applications/프로그램ID/앱이름.app/Data
Application.streamingAssetsPath : /var/mobile/Applications/프로그램ID/앱이름.app/Data/Raw 
파일 읽기 가능, 쓰기 불가능
*/

//해당 패키지에 파일로 접근하면 아래 소스 참조하세요 
    public string pathForDocumentsFile(string filename) 
    {
        //string currentPath = null;
        if (Application.platform == RuntimePlatform.IPhonePlayer) 
        { 
            string path = Application.persistentDataPath.Substring(0, Application.persistentDataPath.Length - 5); 
            path = path.Substring(0, path.LastIndexOf('/')); 
            //currentPath = path+"/Documents"; 
            return Path.Combine(Path.Combine(path, "Documents"), filename); 
        } 
        else if (Application.platform == RuntimePlatform.Android) 
        { 
            string path = Application.persistentDataPath; 
            //string path = System.IO.Path.GetFullPath("."); 
            path = path.Substring(0, path.LastIndexOf('/')); 
            //currentPath = path; 
            return Path.Combine(path, filename); 
            
        } 
        else 
        { 
            string path = Application.dataPath; 
            path = path.Substring(0, path.LastIndexOf('/')); 
            path = path + "/Texts/"; 
            //currentPath = path; 
            return Path.Combine(path, filename); 
        } 
    }

    //editText의 onTextChanged(final CharSequence s, final int start, final int before, final int count )
    //{
    //     String result = convertUnicode(s);
    //}
    /*
    public  String convertUnicode(CharSequence s) 
    {
        StringBuffer result = new StringBuffer();
        Matcher m = Pattern.compile("\\\\u([0-9a-zA-Z]{4,4})\\b").matcher(s);
        while ( m.find() )
        {
            char c = (char) Integer.parseInt(m.group(1), 16);
            m.appendReplacement(result, String.valueOf(c) );
        }
        m.appendTail(result);
        return result.toString();
    }
    */


}









