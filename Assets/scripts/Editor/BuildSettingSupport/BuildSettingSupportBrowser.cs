using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;
using BSS;

public class BuildSettingSupportBrowser : EditorWindow
{

    private static BuildSettingSupportBrowser s_instance = null;
    internal static BuildSettingSupportBrowser instance
    {
        get
        {
            if (s_instance == null)
                s_instance = GetWindow<BuildSettingSupportBrowser>();
            return s_instance;
        }
    }


    Dictionary<int, GUIContent> defineSymbols;
    BuildSettingSupport.Data data;
    BuildSettingTemplate buildTarget;

    GameCore gc;
    Vector2 m_ScrollPosition;


    static readonly string[] defineSymbolsTooltip = new string[] {
        /*TEST_MODE*/       "에셋번들을 스트리밍 에셋폴더에서 읽어 사용함",
        /*NO_INSPECTION*/   "운영툴에서의 점검메시지를 무시함",
        /*SKIP_TOTURIAL*/   "튜토리얼 스킵",
    };

    static readonly GUIContent[] mGUIContent = new GUIContent[] {
        new GUIContent("URL", "연결하고자하는 서버"),
        new GUIContent("Store", "타겟 스토어"),
        new GUIContent("Version", "빌드 버전"),
        new GUIContent("▲", "increase Version"),
        new GUIContent("▲", "increase Develop Version"),
        new GUIContent("Version Code", "tooooooltip"),
        new GUIContent("▼", "decrease Version"),
        new GUIContent("▲", "decrease Develop Version"),
        new GUIContent("R", "Reset Develop Version"),
        new GUIContent("Define Symbols", "코드에서 사용되는 Define들 활성화 여부"),
        new GUIContent("Backend", "select a Script Backend. Target architectures follow changes"),
        new GUIContent("Icon", "Select App Icon"),
        new GUIContent("ETC", "기타 빌드 설정"),
        new GUIContent("Split Build", "분할 빌드"),
        new GUIContent("Develop Build", "개발자 빌드"),
        new GUIContent("GameCore", "게임코어 오브젝트의 인스펙터"),
        new GUIContent("Log LoadTable", "테이블 로드 로그"),
        new GUIContent("Log Sound", "사운드 출력 로그"),
        new GUIContent("Log Spine Animation", "스파인 애니메이션 로그"),
        new GUIContent("Log Network", "네트워크 패킷 로그"),
        new GUIContent("Test TeamSkill", "무조건 팀스킬 게이지가 200%로 참"),
        new GUIContent("Test StrikeSkill", "무조건 스트라이크스킬 게이지가 100%로 참"),
        new GUIContent("  - UserID", "게스트 로그인시 구별을 새로운 계정 생성을 위한 구분자 (기본값은 0이다.)"),
        new GUIContent("Delete", "Delete 'Bundle' Folder in StreamingAssets"),
        new GUIContent("Reset", "Reset From Now Settings"),
        new GUIContent("Apply", "Apply Settings"),
    };
    


    [MenuItem("Window/Build Support Browser", priority = 2050)]
    static void ShowWindow()
    {
        s_instance = null;
        instance.titleContent = new GUIContent("Build Setting Support");
        instance.Show();
    }


    int ToNumber(string _str)
    {
        int result;
        var tmp = Regex.Replace(_str, @"[^0-9]", "");
        int.TryParse(tmp, out result);

        return result;
    }

    char ToWord(string _str)
    {
        return _str == string.Empty ? '\0' : _str[0];
    }

    private void OnEnable()
    {
        if (defineSymbols == null)
        {
            int idx = 0;
            defineSymbols = new Dictionary<int, GUIContent>();
            for (int i = 1; i < (int)BSS.BSD_DefineSymbols.Count; i <<= 1)
                defineSymbols.Add(i, new GUIContent(((BSS.BSD_DefineSymbols)i).ToString(), defineSymbolsTooltip[idx++]));
        }

        if (gc == null)
        {
            gc = BuildSettingSupport.GetGameCore();
        }

        data = BuildSettingSupport.Data.NowState();

        buildTarget = (BuildSettingTemplate)PlayerPrefs.GetInt("BUILDTARGET", 0);
    }


    private void OnGUI()
    {
        if (EditorApplication.isPlaying)
        {
            GUILayout.Label("if you want to work, Stop Play Mode.");
            return;
        }

        if (gc == null)
            gc = BuildSettingSupport.GetGameCore();

        int idx = 0;
        m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

        GUILayout.Space(5);
        // Draw UI
        GUILayout.BeginVertical();
        {
            GUILayout.Space(10);

            // URL
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(mGUIContent[idx++], SetBold(data.IsChangedURL(gc)), GUILayout.Width(100));
                data.url = (URLType)EditorGUILayout.EnumPopup(data.url, GUILayout.Width(100));
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Target Store
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(mGUIContent[idx++], SetBold(data.IsChangedTargetStore()), GUILayout.Width(100));
                data.targetStore = (TargetStore)EditorGUILayout.EnumPopup(data.targetStore, GUILayout.Width(80));
                EditorGUILayout.LabelField(data.GetGeneratePackageName());
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Version
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false), GUILayout.Width(360));
            {
                GUILayout.BeginVertical();
                {
                    // Version
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(mGUIContent[idx++], SetBold(data.IsChangedVersion()), GUILayout.Width(100));
                        data.majorVersion = (short)ToNumber(GUILayout.TextField(data.majorVersion.ToString(), 4, GUILayout.Width(30))); GUILayout.Label(".", GUILayout.Width(8));
                        data.minorVersion = (short)ToNumber(GUILayout.TextField(data.minorVersion.ToString(), 4, GUILayout.Width(30))); GUILayout.Label(".", GUILayout.Width(8));
                        data.patchVersion = (short)ToNumber(GUILayout.TextField(data.patchVersion.ToString(), 6, GUILayout.Width(50)));
                        data.versionWord  = ToWord(GUILayout.TextField(data.versionWord.ToString(), 1, GUILayout.Width(20)));
                        data.DebugVersion = (short)ToNumber(GUILayout.TextField(data.DebugVersion.ToString(), 4, GUILayout.Width(30)));

                        if (GUILayout.Button(mGUIContent[idx++], GUILayout.Width(20)))
                        {
                            data.patchVersion++;
                            data.versionCode++;
                        }
                        if (GUILayout.Button(mGUIContent[idx++], GUILayout.Width(20)))
                        {
                            data.DebugVersion++;
                        }

                    }
                    GUILayout.EndHorizontal();

                    // Version Code
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(mGUIContent[idx++], SetBold(data.IsChangedVersionCode()), GUILayout.Width(100));
                        data.versionCode = ToNumber(GUILayout.TextField(data.versionCode.ToString(), GUILayout.Width(60)));

                        GUILayout.Space(140);

                        if (GUILayout.Button(mGUIContent[idx++], GUILayout.Width(20)))
                        {
                            data.patchVersion = (short)Mathf.Max(0, data.patchVersion - 1);
                            data.versionCode = (short)Mathf.Max(0, data.versionCode - 1);
                        }
                        if (GUILayout.Button(mGUIContent[idx++], GUILayout.Width(20)))
                        {
                            data.DebugVersion = (short)Mathf.Max(0, data.DebugVersion - 1);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                if (GUILayout.Button(mGUIContent[idx++], GUILayout.Width(40), GUILayout.Height(38)))
                {
                    data.DebugVersion = 0;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            //symbols
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(mGUIContent[idx++], SetBold(data.IsChangedDefineSymbols()), GUILayout.Width(100));
                GUILayout.BeginVertical();
                {
                    foreach (var define in defineSymbols)
                    {
                        var result = EditorGUILayout.ToggleLeft(define.Value, (data.defineSymbols & ((BSD_DefineSymbols)define.Key)) != 0);

                        if (result) data.defineSymbols |= (BSD_DefineSymbols)define.Key;
                        else data.defineSymbols &= ~(BSD_DefineSymbols)define.Key;
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            //backend
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(mGUIContent[idx++], SetBold(data.IsChangedScriptBackend()), GUILayout.Width(100));
                data.scriptBackend = (ScriptingImplementation)EditorGUILayout.EnumPopup(data.scriptBackend, GUILayout.Width(100));
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Icon
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(mGUIContent[idx++], SetBold(data.IsChangedAppIcon()), GUILayout.Width(100));
                data.icon = (AppIconType)EditorGUILayout.EnumPopup(data.icon, GUILayout.Width(100));
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // ETC
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(mGUIContent[idx++], GUILayout.Width(100));
                GUILayout.BeginVertical();
                {
                    data.splitBuild = EditorGUILayout.ToggleLeft(mGUIContent[idx++], data.splitBuild, SetBold(data.IsChangedSpirteBuild()));
                    data.developBuild = EditorGUILayout.ToggleLeft(mGUIContent[idx++], data.developBuild, SetBold(data.IsChangedDevelopBuild()));
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();




            GUILayout.Space(20);

            // GameCore Inspector
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(mGUIContent[idx++], GUILayout.Width(100));

                GUILayout.BeginVertical();
                {
                    data.logLoadTable = EditorGUILayout.ToggleLeft(mGUIContent[idx++], data.logLoadTable, SetBold(data.IsChangedLogLoadTable(gc)));
                    data.logSound = EditorGUILayout.ToggleLeft(mGUIContent[idx++], data.logSound, SetBold(data.IsChangedLogSound(gc)));
                    data.logSpineAnimation = EditorGUILayout.ToggleLeft(mGUIContent[idx++], data.logSpineAnimation, SetBold(data.IsChangedLogSpineAnimation(gc)));
                    data.logNetwork = EditorGUILayout.ToggleLeft(mGUIContent[idx++], data.logNetwork, SetBold(data.IsChangedLogNetwork(gc)));
                    data.testTeamSkill = EditorGUILayout.ToggleLeft(mGUIContent[idx++], data.testTeamSkill, SetBold(data.IsChangedTestTeamSkil(gc)));
                    data.testStrikeSkill = EditorGUILayout.ToggleLeft(mGUIContent[idx++], data.testStrikeSkill, SetBold(data.IsChangedTestStrikeSkil(gc)));
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            // UserID
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(mGUIContent[idx++], SetBold(data.IsChangedUserID(gc)), GUILayout.Width(100));
                data.UserID = ToNumber(GUILayout.TextField(data.UserID.ToString(), GUILayout.Width(100)));
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Streaming Assets in Assetbundle Files
            GUILayout.BeginHorizontal();
            {
                if (Directory.Exists(Path.Combine(Application.streamingAssetsPath, "Bundle")))
                {
                    var oldClr = GUI.color;
                    if ((data.defineSymbols & BSD_DefineSymbols.TEST_MODE) == 0)
                        GUI.color = new Color32(0xFF, 0x18, 0x18, 0xFF);
                    GUILayout.Label("Exist Assetbundle in StreamingAssets folder.");
                    GUI.color = oldClr;
                    if (GUILayout.Button(mGUIContent[idx++], GUILayout.Width(100)))
                    {
                        AssetDatabase.DeleteAsset("Assets/StreamingAssets/Bundle");
                    }
                }
                else
                {
                    idx++; // Button를 출력하지 않으므로 인덱스 증가

                    var oldClr = GUI.color;
                    if ((data.defineSymbols & BSD_DefineSymbols.TEST_MODE) != 0)
                        GUI.color = new Color32(0xFF, 0x18, 0x18, 0xFF);
                    GUILayout.Label("Not exist Assetbundle in StreamingAssets folder.");
                    GUI.color = oldClr;
                }
                    
            }
            GUILayout.EndHorizontal();

            // Check Target Build Group
            if (!BuildSettingSupport.IsTargetBuildGroup(buildTarget, ref data))
            {
                var oldClr = GUI.color;
                GUI.color = new Color32(0xFF, 0x18, 0x18, 0xFF);
                GUILayout.Label("TargetBuildGroup is not match. Please change TargetBuildGroup.");
                GUI.color = oldClr;
            }
        }
        GUILayout.EndVertical();


        // Bottom
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5);

                // Reset
                if (GUILayout.Button(mGUIContent[idx++], GUILayout.MinWidth(120)))
                {
                    data = BuildSettingSupport.Data.NowState();
                }

                // 현재 타겟 빌드와 설정이 일치하지 않을때 Custom으로 변경
                if (!BuildSettingSupport.IsTargetBuild(buildTarget, ref data))
                    buildTarget = BuildSettingTemplate.Custom;

                // Now Build Target
                var newBuildTarget = (BuildSettingTemplate)EditorGUILayout.EnumPopup(buildTarget, GUILayout.MinWidth(120));
                if (newBuildTarget != buildTarget)
                {
                    buildTarget = newBuildTarget;
                    BuildSettingSupport.ChangeTargetBuild(buildTarget, ref data);
                }

                // Apply
                if (GUILayout.Button(mGUIContent[idx++], GUILayout.MinWidth(120)))
                {
                    data.Apply();
                    PlayerPrefs.SetInt("BUILDTARGET", (int)buildTarget);
                }

                GUILayout.Space(5);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }
        GUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }



    public GUIStyle SetBold(bool _bold)
    {
        return _bold ? EditorStyles.boldLabel : EditorStyles.label;   
    }
}
