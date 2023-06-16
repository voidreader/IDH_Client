using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace BSS
{
    public enum BuildSettingTemplate
    {
        Custom,
        google,
        one,
        one_a,
        alpah,
        develop
    }

    public enum URLType
    {
        LiveServer,
        TestServer,
        InternalServer,
    }

    [System.Flags]
    public enum BSD_DefineSymbols
    {
        None,
        TEST_MODE       = 1 << 0, // 에셋번들 파일 존재 여부가 이것에 종속된다.
        NO_INSPECTION   = 1 << 1,
        SKIP_TOTURIAL   = 1 << 2,

        Count           = 1 << 3 // Never Used (목록이수정된다면 이값을 조정하여 항상 마지막값이 되도록 해야한다.)
    }

    // 이것은 선언명을 그대로 패키지명의 텍스트로 사용하므로 바꾸어선 안된다.
    public enum TargetStore // MainTemplate 변경이 이것에 종속된다.
    {
        google,
        one,
        ios,
    }

    // 이것은 선언명을 그대로 경로의 텍스트로 사용하므로 바꾸어선 안된다. (파일이 담긴 최종 폴더명과 일치해야 한다.)
    public enum AppIconType
    {
        Regular,
        Test,
    }




    public class BuildSettingSupport
    {
        public static readonly int sceneIndex = 0;

        public static readonly string[] urls = new string[] {   
        /*LiveServer*/     @"ws://slb-3071035.ncloudslb.com:3000/socket.io/?EIO=4&transport=websocket",
        /*TestServer*/     @"ws://49.236.145.83:3100/socket.io/?EIO=4&transport=websocket",
        /*DevelopServer*/  @"ws://game.fivestargames.co.kr:4000/socket.io/?EIO=4&transport=websocket",
        };

        public const string defPackageNameFormat = "imageframe.{0}.idh";

        /// <summary>
        /// 데이터를 모아 저장되는 구조체
        /// </summary>
        [System.Serializable]
        public struct Data
        {
            public int force; // 강제성의 정도로, 빌드옵션에는 영향이 없으나 
                              // 타겟빌드를 변경할때 어느정도까지 변경이 될지를 결정한다. 
                              // (0 - develop), (1 - alpha, one_a), (2-google,one)

            // GameCore Options
            public URLType url;            // 연결할 서버주소
            public int UserID;
            public bool logLoadTable;
            public bool logSound;
            public bool logSpineAnimation;
            public bool logNetwork;
            public bool testTeamSkill;
            public bool testStrikeSkill;

            public BSD_DefineSymbols defineSymbols;  // BSD_DefineSymbols와 배열의 크기가 일치 해야한다.

            public short majorVersion;
            public short minorVersion;
            public short patchVersion;
            public char versionWord;    // version에 들어가는 구분 문자
            public short DebugVersion;
            public int versionCode; 


            public ScriptingImplementation scriptBackend;  // 스크립트 백엔드
            public bool splitBuild;     // 스플릿 빌드 여부
            public bool developBuild;   // 개발자 빌드 여부

            public TargetStore targetStore;    // 타켓 스토어
            public AppIconType icon;           // 앱 아이콘을 결정한다. 



            public void Apply()
            {
                ApplyGameCoreState();
                ApplyDefineState();
                ApplyVersionState();
                ApplyTargetStore();
                ApplyIconState();
                ApplyEtcStates();
            }



            #region Apply State Sub Functions



            void ApplyGameCoreState()
            {
                var gc = GetGameCore();
                var sc = gc.GetComponent<SocketIO.SocketIOComponent>();
                bool dirty = false;

                if (sc.url != urls[(int)url])                   { dirty = true; sc.url = urls[(int)url]; }
                if (gc.UserID != UserID)                        { dirty = true; gc.UserID = UserID; }
                if (gc.bLogLoadTable != logLoadTable)           { dirty = true; gc.bLogLoadTable = logLoadTable; }
                if (gc.BLogNetworkData != logNetwork)           { dirty = true; gc.BLogNetworkData = logNetwork; }
                if (gc.bLogSound != logSound)                   { dirty = true; gc.bLogSound = logSound; }
                if (gc.bLogSpineAnmation != logSpineAnimation)  { dirty = true; gc.bLogSpineAnmation = logSpineAnimation; }
                if (gc.bTestTeamSkill != testTeamSkill)         { dirty = true; gc.bTestTeamSkill = testTeamSkill; }
                if (gc.bTestStrikeSkill != testStrikeSkill)     { dirty = true; gc.bTestStrikeSkill = testStrikeSkill; }
                if (dirty)
                {
                    EditorUtility.SetDirty(gc.gameObject);
                    var targetScene = GetMainScene();
                    EditorSceneManager.MarkSceneDirty(targetScene);
                    EditorSceneManager.SaveScene(targetScene);
                }
            }


            void ApplyDefineState()
            {
                var sb = new StringBuilder();
                for(int i = 0; true; ++i)
                {
                    var define = (BSD_DefineSymbols)(1 << i);
                    if (BSD_DefineSymbols.Count <= define)
                        break;

                    if ((defineSymbols & define) == 0)
                        continue;

                    if (sb.Length != 0)
                        sb.Append(';');

                    sb.Append(define.ToString());
                }

                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, sb.ToString());
            }
            

            void ApplyVersionState()
            {
                switch (EditorUserBuildSettings.selectedBuildTargetGroup)
                {
                    case BuildTargetGroup.Android:  PlayerSettings.Android.bundleVersionCode = versionCode;     break;
                    case BuildTargetGroup.iOS:      PlayerSettings.iOS.buildNumber = versionCode.ToString();    break;
                }

                var sb = new StringBuilder();
                sb.Append(string.Format("{0}.{1}.{2}", majorVersion, minorVersion, patchVersion));
                if (versionWord != 0)   sb.Append(versionWord);
                if (DebugVersion != 0)  sb.Append(DebugVersion.ToString("00"));
                PlayerSettings.bundleVersion = sb.ToString();
            }



            void ApplyTargetStore()
            {
                // change package Name
                PlayerSettings.SetApplicationIdentifier(
                        EditorUserBuildSettings.selectedBuildTargetGroup,
                        GetGeneratePackageName());

                // change MainTemplate.gradle
                var result = AssetDatabase.CopyAsset(
                    string.Format("Assets/Plugins/Android/mainTemplate_{0}.gradle", targetStore),
                    "Assets/Plugins/Android/mainTemplate.gradle");

                if (result == false)
                    Debug.LogError("파일 복사 실패.");
            }


            void ApplyIconState()
            {
                var pathFormat = "Assets/sprites/AppIcon/{0}/AOS_{1}x{1}.png";

                var txIcons = new Texture2D[] {
                    AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(pathFormat, icon, 48))
                };
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, txIcons);

                if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android)
                {
                    txIcons = new Texture2D[] {
                        AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(pathFormat, icon, 192)),
                        AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(pathFormat, icon, 144)),
                        AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(pathFormat, icon, 96)),
                        AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(pathFormat, icon, 72)),
                        AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(pathFormat, icon, 48)),
                        AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(pathFormat, icon, 36))
                    };
                    PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, txIcons);
                }
            }


            void ApplyEtcStates()
            {
                PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup, scriptBackend);
#if UNITY_ANDROID
                switch (scriptBackend)
                {
                    case ScriptingImplementation.IL2CPP:
                        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
                        break;
                    case ScriptingImplementation.Mono2x:
                        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
                        break;
                }
#endif

                PlayerSettings.Android.useAPKExpansionFiles = splitBuild;
                EditorUserBuildSettings.development = developBuild;
            }



#endregion Apply State Sub Functions



            public static Data NowState()
            {
                var data = new Data();
                FillGameCoreState(ref data);
                FillDefineState(ref data);
                FillVersionState(ref data);
                FillTargetStore(ref data);
                FillIconState(ref data);
                FillEtcStates(ref data);

                return data;
            }



#region FillState Sub Functions



            static void FillGameCoreState(ref Data _data)
            {
                var gc = GetGameCore();

                _data.url = URLType.TestServer;
                var tempURL = gc.GetComponent<SocketIO.SocketIOComponent>().url;
                for (int i = 0; i < urls.Length; ++i)
                {
                    if (urls[i] != tempURL) continue;
                    _data.url = (URLType)i; break;
                }

                _data.UserID =            gc.UserID;
                _data.logLoadTable =      gc.bLogLoadTable;
                _data.logSound =          gc.bLogSound;
                _data.logSpineAnimation = gc.bLogSpineAnmation;
                _data.logNetwork =        gc.BLogNetworkData;
                _data.testTeamSkill =     gc.bTestTeamSkill;
                _data.testStrikeSkill =   gc.bTestStrikeSkill;
            }


            static void FillDefineState(ref Data _data)
            {
                // 현재의 심볼들을 가져와 배열을 만든다.
                string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                string[] allDefines = definesString.Split(';');

                _data.defineSymbols = BSD_DefineSymbols.None;
                for (int i = 0; true; ++i)
                {
                    var define = (BSD_DefineSymbols)(1 << i);
                    if (BSD_DefineSymbols.Count <= define)
                        break;

                    var strDef = define.ToString();
                    foreach (var def in allDefines)
                    {
                        if (strDef == def)
                        {
                            _data.defineSymbols |= define;
                            break;
                        }
                    }
                }
            }

            static void FillVersionState(ref Data _data)
            {
                var code = PlayerSettings.Android.bundleVersionCode;
                if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.iOS)
                    int.TryParse(PlayerSettings.iOS.buildNumber, out code);
                _data.versionCode = code;

                var strVersion = PlayerSettings.bundleVersion;
                var arrVersion = strVersion.Split('.');
                if (arrVersion.Length < 3)
                    return;

                short.TryParse(arrVersion[0], out _data.majorVersion);
                short.TryParse(arrVersion[1], out _data.minorVersion);

                var patch = arrVersion[2];
                int patchIdx = 0;
                for (; patchIdx < patch.Length; ++patchIdx)
                    if (!('0' <= patch[patchIdx] && patch[patchIdx] <= '9'))
                    {
                        _data.versionWord = patch[patchIdx];
                        break;
                    }

                short.TryParse(patch.Substring(0, patchIdx), out _data.patchVersion);

                _data.DebugVersion = 0;
                if (++patchIdx < patch.Length)
                    short.TryParse(patch.Substring(patchIdx, patch.Length - patchIdx), out _data.DebugVersion);
            }


            static void FillTargetStore(ref Data _data)
            {
                var packageName = PlayerSettings.GetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup);
                var splitData = packageName.Split('.');

                _data.targetStore = default(TargetStore);
                for (var i = (TargetStore)0; true; ++i)
                {
                    if (i.ToString() == splitData[1])
                    {
                        _data.targetStore = i;
                        return;
                    }
                    else if (i.ToString() == ((int)i).ToString())
                        return;
                }
            }

            static void FillIconState(ref Data _data)
            {
                _data.icon = AppIconType.Test;

                var icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);
                if (icons == null || icons.Length == 0)
                    return;

                var path = AssetDatabase.GetAssetPath(icons[0]);
                var folderName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path));

                for (var i = (AppIconType)0; true; ++i)
                {
                    if (i.ToString() == folderName)
                    {
                        _data.icon = i;
                        return;
                    }
                    else if (i.ToString() == ((int)i).ToString())
                        return;
                }
            }


            static void FillEtcStates(ref Data _data)
            {
                _data.scriptBackend = PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup);
                _data.splitBuild = PlayerSettings.Android.useAPKExpansionFiles;
                _data.developBuild = EditorUserBuildSettings.development;
            }



#endregion FillState Sub Functions



#region IsChange Functions



            public bool IsChangedURL(GameCore _gc)
            {
                return urls[(int)url] != _gc.GetComponent<SocketIO.SocketIOComponent>().url;
            }


            public bool IsChangedUserID(GameCore _gc)
            {
                return UserID != _gc.UserID;
            }


            public bool IsChangedLogLoadTable(GameCore _gc)
            {
                return logLoadTable != _gc.bLogLoadTable;
            }


            public bool IsChangedLogSound(GameCore _gc)
            {
                return logSound != _gc.bLogSound;
            }


            public bool IsChangedLogSpineAnimation(GameCore _gc)
            {
                return logSpineAnimation != _gc.bLogSpineAnmation;
            }


            public bool IsChangedLogNetwork(GameCore _gc)
            {
                return logNetwork != _gc.BLogNetworkData;
            }

            public bool IsChangedTestTeamSkil(GameCore _gc)
            {
                return testTeamSkill != _gc.bTestTeamSkill;
            }

            public bool IsChangedTestStrikeSkil(GameCore _gc)
            {
                return testStrikeSkill != _gc.bTestStrikeSkill;
            }

            public bool IsChangedDefineSymbols()
            {
                string[] allDefines;
                string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                if (definesString != string.Empty)  allDefines = definesString.Split(';');
                else                                allDefines = new string[0];

                int count = 0;
                var defsybs = BSD_DefineSymbols.None;
                for (int i = 0; true; ++i)
                {
                    var define = (BSD_DefineSymbols)(1 << i);
                    if (BSD_DefineSymbols.Count <= define)
                        break;

                    var strDef = define.ToString();
                    foreach (var def in allDefines)
                    {
                        if (strDef == def)
                        {
                            defsybs |= define;
                            count++;
                            break;
                        }
                    }
                }

                if (defsybs != defineSymbols ||  // 선언된 디파인심볼이 다르거나
                    allDefines.Length != count)  // 정의되지 않은 디파인 존재시
                    return true;

                return false;
            }


            public bool IsChangedVersion()
            {
                var sb = new StringBuilder();
                sb.Append(string.Format("{0}.{1}.{2}", majorVersion, minorVersion, patchVersion));
                if (versionWord != 0)   sb.Append(versionWord);
                if (DebugVersion != 0)  sb.Append(DebugVersion.ToString("00"));

                return PlayerSettings.bundleVersion != sb.ToString();
            }


            public bool IsChangedVersionCode()
            {
                var code = PlayerSettings.Android.bundleVersionCode;
                if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.iOS)
                    int.TryParse(PlayerSettings.iOS.buildNumber, out code);

                return versionCode != code;
            }


            public bool IsChangedScriptBackend()
            {
                if (scriptBackend == PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup))
                {
                    switch (scriptBackend)
                    {
#if UNITY_ANDROID
                        case ScriptingImplementation.Mono2x:
                            return PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARMv7;
                        case ScriptingImplementation.IL2CPP:
                            return PlayerSettings.Android.targetArchitectures != (AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7);
                        default:
                            return true;
#else
                        default:
                            return false;
#endif
                    }
                }
                else
                    return true;

            }


            public bool IsChangedSpirteBuild()
            {
                return splitBuild != PlayerSettings.Android.useAPKExpansionFiles;
            }


            public bool IsChangedDevelopBuild()
            {
                return developBuild != EditorUserBuildSettings.development;
            }


            public bool IsChangedTargetStore()
            {
                var names = PlayerSettings.GetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup).Split('.');
                if (names != null && names.Length == 3)
                    return names[1] != targetStore.ToString();
                else
                    return true;
            }

            public bool IsChangedAppIcon()
            {
                var txIcons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);
                if (txIcons != null && 1 <= txIcons.Length)
                {
                    var path = AssetDatabase.GetAssetPath(txIcons[0]);
                    path = System.IO.Path.GetDirectoryName(path);
                    var folder = System.IO.Path.GetFileName(path);
                    return folder != icon.ToString();
                }
                else
                    return true;
            }



#endregion IsChange Functions



            public string GetGeneratePackageName()
            {
                var names = PlayerSettings.GetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup).Split('.');
                if (names == null || names.Length != 3)
                    return string.Format(defPackageNameFormat, targetStore);
                else
                    return string.Format("{0}.{1}.{2}", names[0], targetStore, names[2]);
            }

        }




        static Data GoogleBSD = new Data()
        {
            force = 2,
            url = URLType.LiveServer,
            UserID = 0,
            logLoadTable = false,
            logSound = false,
            logSpineAnimation = false,
            logNetwork = true,
            testTeamSkill = false,
            testStrikeSkill = false,

            defineSymbols = BSD_DefineSymbols.None,

            versionWord = 'g',
            DebugVersion = 0,

            scriptBackend = ScriptingImplementation.IL2CPP,
            splitBuild = true,
            developBuild = false,

            targetStore = TargetStore.google,
            icon = AppIconType.Regular
        };

        static Data OneBSD = new Data()
        {
            force = 2,
            url = URLType.LiveServer,
            UserID = 0,
            logLoadTable = false,
            logSound = false,
            logSpineAnimation = false,
            logNetwork = true,
            testTeamSkill = false,
            testStrikeSkill = false,

            defineSymbols = BSD_DefineSymbols.None,

            versionWord = 'o',
            DebugVersion = 0,

            scriptBackend = ScriptingImplementation.IL2CPP,
            splitBuild = false,
            developBuild = false,

            targetStore = TargetStore.one,
            icon = AppIconType.Regular
        };

        static Data One_aBSD = new Data()
        {
            force = 1,
            url = URLType.LiveServer,
            UserID = 0,
            testTeamSkill = false,
            testStrikeSkill = false,


            defineSymbols = BSD_DefineSymbols.NO_INSPECTION,

            versionWord = 'm',
            DebugVersion = 0,

            scriptBackend = ScriptingImplementation.IL2CPP,
            splitBuild = false,
            developBuild = true,

            targetStore = TargetStore.one,
            icon = AppIconType.Regular
        };

        static Data TestBSD = new Data()
        {
            force = 1,
            url = URLType.TestServer,
            UserID = 0,
            testTeamSkill = false,
            testStrikeSkill = false,

            defineSymbols = BSD_DefineSymbols.NO_INSPECTION | BSD_DefineSymbols.TEST_MODE,

            versionWord = 'a',
            DebugVersion = 0,

            scriptBackend = ScriptingImplementation.Mono2x,
            splitBuild = false,
            developBuild = true,

            targetStore = TargetStore.one,
            icon = AppIconType.Test
        };

        static Data DevelopBSD = new Data()
        {
            force = 0,
            url = URLType.InternalServer,

            versionWord = 'd',

            scriptBackend = ScriptingImplementation.Mono2x,
            splitBuild = false,
            developBuild = true,

            targetStore = TargetStore.one,
            icon = AppIconType.Test
        };

        Data bsd;




        public static void ChangeTargetBuild(BuildSettingTemplate target, ref Data _data)
        {
            switch (target)
            {
                case BuildSettingTemplate.Custom:   return;
                case BuildSettingTemplate.google:   ChangeTargetBuild(ref _data, ref GoogleBSD); return;
                case BuildSettingTemplate.one:      ChangeTargetBuild(ref _data, ref OneBSD); return;
                case BuildSettingTemplate.one_a:    ChangeTargetBuild(ref _data, ref One_aBSD); return;
                case BuildSettingTemplate.alpah:    ChangeTargetBuild(ref _data, ref TestBSD); return;
                case BuildSettingTemplate.develop:  ChangeTargetBuild(ref _data, ref DevelopBSD); return;
                default:                            return;
            }
        }


        public static bool IsTargetBuild(BuildSettingTemplate target, ref Data _data)
        {
            switch (target)
            {
                case BuildSettingTemplate.Custom:   return true;
                case BuildSettingTemplate.google:   return IsTargetBuild(ref _data, ref GoogleBSD);
                case BuildSettingTemplate.one:      return IsTargetBuild(ref _data, ref OneBSD);
                case BuildSettingTemplate.one_a:    return IsTargetBuild(ref _data, ref One_aBSD);
                case BuildSettingTemplate.alpah:    return IsTargetBuild(ref _data, ref TestBSD);
                case BuildSettingTemplate.develop:  return IsTargetBuild(ref _data, ref DevelopBSD);
                default:                            return false;
            }
        }

        public static bool IsTargetBuildGroup(BuildSettingTemplate target, ref Data _data)
        {
            switch (target)
            {
                case BuildSettingTemplate.develop:
                case BuildSettingTemplate.Custom:
                    if (_data.targetStore == TargetStore.ios)
                        return EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.iOS;
                    else
                        return EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android;
                    return true;

                default:
                    return false;

                case BuildSettingTemplate.google:   
                case BuildSettingTemplate.one:      
                case BuildSettingTemplate.one_a:    
                case BuildSettingTemplate.alpah:
                    if (_data.targetStore == TargetStore.ios)
                        return EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.iOS;
                    else
                        return EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android;


            }
        }


        static void ChangeTargetBuild(ref Data _data, ref Data _template)
        {
            _data.versionWord = _template.versionWord;
            _data.scriptBackend = _template.scriptBackend;
            _data.splitBuild = _template.splitBuild;
            _data.icon = _template.icon;

            if (_template.force <= 0)
                return;

            _data.url = _template.url;
            _data.targetStore = _template.targetStore;
            _data.UserID = _template.UserID;
            _data.defineSymbols = _template.defineSymbols;
            _data.icon = _template.icon;
            _data.testTeamSkill = _template.testTeamSkill;
            _data.testStrikeSkill = _template.testStrikeSkill;

            if (_template.force <= 1)
                return;

            _data.developBuild = _template.developBuild;
            _data.DebugVersion = _template.DebugVersion;
            _data.logLoadTable = _template.logLoadTable;
            _data.logSound = _template.logSound;
            _data.logSpineAnimation = _template.logSpineAnimation;
            _data.logNetwork = _template.logNetwork;
        }


        static bool IsTargetBuild(ref Data _data, ref Data _template)
        {
            if (_data.versionWord != _template.versionWord ||
                _data.scriptBackend != _template.scriptBackend ||
                _data.splitBuild != _template.splitBuild ||
                _data.icon != _template.icon)
                return false;

            if (_template.force <= 0)
                return true;

            if (_data.url != _template.url || 
                _data.targetStore != _template.targetStore ||
                _data.UserID != _template.UserID ||
                _data.defineSymbols != _template.defineSymbols ||
                _data.icon != _template.icon ||
                _data.testTeamSkill != _template.testTeamSkill ||
                _data.testStrikeSkill != _template.testStrikeSkill)
                return false;

            if (_template.force <= 1)
                return true;
                
            if (_data.developBuild != _template.developBuild ||
                _data.DebugVersion != _template.DebugVersion ||
                _data.logLoadTable != _template.logLoadTable ||
                _data.logSound != _template.logSound ||
                _data.logSpineAnimation != _template.logSpineAnimation ||
                _data.logNetwork != _template.logNetwork)
                return false;

            return true;
        }

        public void CollectState()
        {
            bsd = Data.NowState();
        }


        public void ApplyState()
        {
            bsd.Apply();
        }








        /// <summary>
        /// 메인씬의 게임 코어를 가져온다.
        /// </summary>
        /// <returns></returns>
        public static GameCore GetGameCore()
        {
            var targetScene = GetMainScene();
            var list = targetScene.GetRootGameObjects();
            foreach (var obj in list)
            {
                var cc = obj.GetComponent<GameCore>();
                if (cc == null)
                    continue;

                return cc;
            }

            return null;
        }


        /// <summary>
        /// 게임코어가 있는 씬을 반환한다.(만약 열려있지 않다면 연상태로 만든다.)
        /// </summary>
        /// <returns></returns>
        public static UnityEngine.SceneManagement.Scene GetMainScene()
        {
            var targetScene = UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(sceneIndex);
            // 씬이 열려 있지 않다면 연다.
            if (!targetScene.isLoaded)
            {
                var path = EditorBuildSettings.scenes[sceneIndex].path;
                targetScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }

            return targetScene;
        }
    }

}