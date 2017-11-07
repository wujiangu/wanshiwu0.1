using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

[System.Serializable]
public class XlsxDataEditorWindow : EditorWindow
{

    public static XlsxDataEditorWindow editorWindow;

    [MenuItem("自定义工具/转表")]
    public static void Init()
    {
        editorWindow = EditorWindow.GetWindow<XlsxDataEditorWindow>(false, "转表", true);
        editorWindow.Show();
        editorWindow.LoadConfig();
    }

    //public Dictionary<string, bool> m_pDictToggle = new Dictionary<string, bool>();

    public string[] m_pFileList = XlsxDataManager.GetXLSFileList();

    public bool m_bIsTextOnly = false;

    public bool m_bIsWaitForCompile = true;

    public bool[] m_pToggle = new bool[300];
    
    public string[] m_pXlsMD5 = new string[300];
    public string[] m_pProtoMD5 = new string[300];
    public bool[] m_bProtoChanged = new bool[300];

    public List<string> m_pListToTxt = new List<string>();


    //public Dictionary<string, string> m_pDictXlsMD5 = new Dictionary<string, string>();

    public Result m_eResulte = Result.Waitting;

    public enum Result
    {
        Waitting = 0,
        Running,
        Select,
        Finish,
    };

    private readonly string PREFIX = "XlsxDataManger_";

    private void SaveConfig()
    {
        EditorPrefs.SetInt(PREFIX + "Count", m_pFileList.Length);

        EditorPrefs.SetBool(PREFIX + "IsTextOnly", m_bIsTextOnly);

        EditorPrefs.SetBool(PREFIX + "IsWaitForCompile", m_bIsWaitForCompile);

        for (int i = 0; i < m_pFileList.Length; i++)
        {
            string filepath = m_pFileList[i];
            EditorPrefs.SetString(PREFIX + string.Format("PATH_{0}", i), filepath);

            bool bToggle = m_pToggle[i];
            EditorPrefs.SetBool(PREFIX + string.Format("TOGGLE_{0}", i), bToggle);

            string sMD5 = m_pXlsMD5[i];
            EditorPrefs.SetString(PREFIX + string.Format("MD5_{0}", i), sMD5);
            
            //string sProtoMD5 = m_pProtoMD5[i];
            //EditorPrefs.SetString(PREFIX + string.Format("PROTO_MD5_{0}", i), sProtoMD5);                   
        }
    }

    private void LoadConfig()
    {
        int iCount = EditorPrefs.GetInt(PREFIX + "Count");
        m_pFileList = new string[iCount];

        m_bIsTextOnly = EditorPrefs.GetBool(PREFIX + "IsTextOnly");

        m_bIsWaitForCompile = EditorPrefs.GetBool(PREFIX + "IsWaitForCompile");

        for (int i = 0; i < m_pFileList.Length; i++)
        {
            m_pFileList[i] = EditorPrefs.GetString(PREFIX + string.Format("PATH_{0}", i));
            m_pToggle[i] = EditorPrefs.GetBool(PREFIX + string.Format("TOGGLE_{0}", i));
            m_pXlsMD5[i] = EditorPrefs.GetString(PREFIX + string.Format("MD5_{0}", i));
            //m_pProtoMD5[i] = EditorPrefs.GetString(PREFIX + string.Format("PROTO_MD5_{0}", i));
        }
    }

    private void Refresh()
    {

        for (int i = 0; i < m_pFileList.Length; i++)
        {
            string path = m_pFileList[i];
            string hash = cs.TablePathConfig.GetMd5Hash(path);
            if (hash == m_pXlsMD5[i])
            {
                m_pToggle[i] = false;
            }
            else
            {
                m_pToggle[i] = true;
            }
        }
    }

    private Vector2 m_pSelectedVec = new Vector2();
    private Vector2 m_pResulteVec = new Vector2();

    private bool mBuildProto = false;
    private StringBuilder mCountBuilder = new StringBuilder(2000);

    private string mFilter = "";

    private bool mShowDirtyFlag = false;

    public void OnGUI()
    {
        if (EditorApplication.isCompiling)
        {
            EditorGUILayout.HelpBox(string.Format("正在编译中\n"), MessageType.Warning);
            return;
        }

        if (EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox(string.Format("游戏正在运行\n"), MessageType.Warning);
            return;
        }
    

        if (m_bIsWaitForCompile && mBuildProto && EditorApplication.isCompiling)
        {
            mCountBuilder.Append("..");
            EditorGUILayout.HelpBox(string.Format("正在编译中{0}\n如果本页面木有刷新，随便点击点击点击", mCountBuilder.ToString()), MessageType.Warning);
            return;
        }

        if (mBuildProto)
        {
            m_pListToTxt.Clear();

            for (int i = 0; i < m_pFileList.Length; i++)
            {
                string path = m_pFileList[i];
                if (m_pToggle[i])
                {
                    m_pListToTxt.Add(path);
                    m_pXlsMD5[i] = cs.TablePathConfig.GetMd5Hash(path);
                }
            }

            XlsxDataUnit.GenerateText(m_pListToTxt.ToArray(), false);

            // TODO bobo
            //TableManager.DestroyInstance();

            //ResourceManager.instance.ClearCache();
            //ResourceManager.DestroyInstance();

            // TODO bobo
            //AssetLoader.instance.ClearAll(true);
            //AssetLoader.DestroyInstance();


            SaveConfig();

            m_eResulte = Result.Finish;
            mBuildProto = false;
        }

        EditorGUI.BeginChangeCheck();
        {

            EditorGUILayout.BeginVertical();
            {
                bool value = EditorGUILayout.Toggle("只转数据表", m_bIsTextOnly);
                if (value != m_bIsTextOnly)
                {
                    m_bIsTextOnly = value;
                }

                bool vvalue = EditorGUILayout.Toggle("等待编译", m_bIsWaitForCompile);
                if (vvalue != m_bIsWaitForCompile)
                {
                    m_bIsWaitForCompile = vvalue;
                }

                var str = EditorGUILayout.TextField("筛选", mFilter);
                if (str != mFilter)
                {
                    mFilter = str;
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            m_pSelectedVec = EditorGUILayout.BeginScrollView(m_pSelectedVec, GUILayout.Height(500));
            {
                EditorGUILayout.BeginVertical("ObjectFieldThumb");
                {
                    for (int i = 0; i < m_pFileList.Length; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUI.indentLevel++;

                            if (i % 2 == 0)
                            {
                                GUI.color = Color.yellow;
                            }

                            var filename = System.IO.Path.GetFileName(m_pFileList[i]);

                            if (mFilter.Length <= 0 || filename.ToLower().StartsWith(mFilter.ToLower()) || m_pToggle[i])
                            {
                                EditorGUILayout.LabelField(filename, GUILayout.Width(220));

                                bool value = EditorGUILayout.Toggle("", m_pToggle[i], GUILayout.Width(100));
                                if (value != m_pToggle[i])
                                {
                                    m_pToggle[i] = value;
                                }

                                //#if UNITY_STANDALONE_WIN
                                if (XlsxDataManager.StyledButton("开!"))
                                {
                                    ProcessStartInfo processInfo = new ProcessStartInfo();
                                    processInfo.FileName = System.IO.Path.GetFullPath(m_pFileList[i]);
                                    processInfo.Arguments = "";

                                    Process process = new Process();
                                    process.StartInfo = processInfo;
                                    process.Start();
                                }
                            }
//#endif
                            GUI.color = Color.white;

                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal("ObjectFieldThumb");
            {
                if (XlsxDataManager.StyledButton("刷新"))
                {
                    m_pFileList = XlsxDataManager.GetXLSFileList();

                    for (int i = 0; i < m_pFileList.Length; i++)
                    {
                        m_pToggle[i] = true;
                    }

                    Refresh();

                    m_pListToTxt.Clear();
                    for (int i = 0; i < m_pFileList.Length; i++)
                    {
                        if (m_pToggle[i])
                        {
                            m_pListToTxt.Add(m_pFileList[i]);
                        }
                    }

                    SaveConfig();

                    m_eResulte = Result.Select;
                }

                if (XlsxDataManager.StyledButton("全选"))
                {
                    for (int i = 0; i < m_pFileList.Length; i++)
                    {
                        m_pToggle[i] = true;
                    }

                    //Refresh();

                    SaveConfig();

                    m_eResulte = Result.Waitting;
                }

                if (XlsxDataManager.StyledButton("反选"))
                {

                    for (int i = 0; i < m_pFileList.Length; i++)
                    {
                        m_pToggle[i] = !m_pToggle[i];
                    }

                    //Refresh();

                    SaveConfig();

                    m_eResulte = Result.Waitting;
                }

                if (XlsxDataManager.StyledButton("清空"))
                {

                    for (int i = 0; i < m_pFileList.Length; i++)
                    {
                        m_pToggle[i] = false;
                    }

                    //Refresh();

                    SaveConfig();

                    m_eResulte = Result.Waitting;
                }


                if (XlsxDataManager.StyledButton("转表"))
                {
                    mCountBuilder.Length = 0;
                    m_pListToTxt.Clear();

                    for (int i = 0; i < m_pFileList.Length; i++)
                    {
                        string path = m_pFileList[i];
                        if (m_pToggle[i])
                        {
                            m_pListToTxt.Add(path);
                            m_pXlsMD5[i] = cs.TablePathConfig.GetMd5Hash(path);
                        }
                    }

                    XlsxDataUnit.GenerateProtoCS(m_pListToTxt.ToArray(), m_bIsTextOnly);
                    mBuildProto = true;

                    SaveConfig();

                    m_eResulte = Result.Finish;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal("ObjectFieldThumb");
            {
                m_pResulteVec = EditorGUILayout.BeginScrollView(m_pResulteVec, GUILayout.Height(180));
                {
                    switch (m_eResulte)
                    {
                        case Result.Waitting:
                            EditorGUILayout.LabelField("");
                            break;
                        case Result.Select:
                            EditorGUILayout.LabelField("修改过的表格:");
                            foreach (string unit in m_pListToTxt)
                            {
                                EditorGUILayout.LabelField(unit);
                            }
                            break;
                        case Result.Finish:
                            EditorGUILayout.LabelField(string.Format("转表完成 {0}：", System.DateTime.Now.ToLongTimeString().ToString()));
                            foreach (string unit in m_pListToTxt)
                            {
                                EditorGUILayout.LabelField(unit);
                            }
                            break;
                        default:
                            break;
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndHorizontal();
        }

        if (EditorGUI.EndChangeCheck())
        {
            // changed
            SaveConfig();
        }

    }
}
