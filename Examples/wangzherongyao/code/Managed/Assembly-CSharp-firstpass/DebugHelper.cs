using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class DebugHelper : MonoBehaviour
{
    private static string CachedLogRootPath;
    public SLogTypeDef cfgMode = SLogTypeDef.LogType_System;
    private static DebugHelper instance = null;
    private static SLogTypeDef logMode = SLogTypeDef.LogType_Custom;
    private static SLogObj[] s_logers = new SLogObj[4];
    public static string timeTag;

    public static void Assert(bool InCondition)
    {
        Assert(InCondition, null, null);
    }

    public static void Assert(bool InCondition, string InFormat)
    {
        Assert(InCondition, InFormat, null);
    }

    public static void Assert(bool InCondition, string InFormat, params object[] InParameters)
    {
        if (!InCondition)
        {
            try
            {
                string str = null;
                if (!string.IsNullOrEmpty(InFormat))
                {
                    try
                    {
                        if (InParameters != null)
                        {
                            str = string.Format(InFormat, InParameters);
                        }
                        else
                        {
                            str = InFormat;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    str = string.Format(" no detail, but stacktrace is :{0}", Environment.StackTrace);
                }
                if (str != null)
                {
                    string message = "Assert failed! " + str;
                    Debug.LogWarning(message);
                    CrashAttchLog(message);
                }
                else
                {
                    Debug.LogWarning("Assert failed!");
                }
            }
            catch (Exception)
            {
            }
        }
    }

    private void Awake()
    {
        Assert(instance == null);
        instance = this;
        logMode = this.cfgMode;
        int num = 4;
        for (int i = 0; i < num; i++)
        {
            s_logers[i] = new SLogObj();
        }
        OpenLoger(SLogCategory.Normal, string.Format("{0}/sgame_log.txt", Application.persistentDataPath));
        OpenLoger(SLogCategory.Skill, string.Format("{0}/sgame_skill.txt", Application.persistentDataPath));
        OpenLoger(SLogCategory.Misc, string.Format("{0}/sgame_misc.txt", Application.persistentDataPath));
    }

    [Conditional("UNITY_EDITOR")]
    public static void ClearConsole()
    {
    }

    public static void CloseLoger(SLogCategory logType)
    {
        int index = (int) logType;
        s_logers[index].Flush();
        s_logers[index].Close();
        s_logers[index].TargetPath = null;
    }

    [Conditional("UNITY_STANDALONE_WIN"), Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void ConsoleLog(string logmsg)
    {
        Debug.Log(logmsg);
    }

    [Conditional("UNITY_STANDALONE_WIN"), Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void ConsoleLogError(string logmsg)
    {
        Debug.LogError(logmsg);
    }

    [Conditional("UNITY_STANDALONE_WIN"), Conditional("FORCE_LOG"), Conditional("UNITY_EDITOR")]
    public static void ConsoleLogWarning(string logmsg)
    {
        Debug.LogWarning(logmsg);
    }

    public static void CrashAttchLog(string str)
    {
        string className = "com.tencent.tmgp.sgame.SGameUtility";
        AndroidJavaClass class2 = new AndroidJavaClass(className);
        object[] args = new object[] { str };
        class2.CallStatic("dtLog", args);
        class2.Dispose();
    }

    [Conditional("UNITY_STANDALONE_WIN"), Conditional("FORCE_LOG"), Conditional("UNITY_EDITOR")]
    public static void EditorAssert(bool InCondition, string InFormat = null, params object[] InParameters)
    {
        Assert(InCondition, InFormat, InParameters);
    }

    [Conditional("UNITY_STANDALONE_WIN"), Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void Log(string logmsg)
    {
        if ((logMode == SLogTypeDef.LogType_Custom) && string.IsNullOrEmpty(s_logers[0].TargetPath))
        {
            OpenLoger(SLogCategory.Normal, string.Format("{0}/sgame_log.txt", Application.persistentDataPath));
        }
    }

    [Conditional("FORCE_LOG"), Conditional("UNITY_STANDALONE_WIN"), Conditional("UNITY_EDITOR")]
    public static void LogError(string errmsg)
    {
        Debug.LogError(errmsg);
    }

    [Conditional("FORCE_LOG"), Conditional("UNITY_EDITOR"), Conditional("UNITY_STANDALONE_WIN")]
    public static void LogInternal(SLogCategory logType, string logmsg)
    {
        s_logers[(int) logType].Log(logmsg);
    }

    [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG"), Conditional("UNITY_STANDALONE_WIN")]
    public static void LogMisc(string logmsg)
    {
        if ((logMode != SLogTypeDef.LogType_System) && ((logMode == SLogTypeDef.LogType_Custom) && string.IsNullOrEmpty(s_logers[3].TargetPath)))
        {
            OpenLoger(SLogCategory.Misc, string.Format("{0}/sgame_misc.txt", Application.persistentDataPath));
        }
    }

    [Conditional("UNITY_STANDALONE_WIN"), Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
    public static void LogSkill(string logmsg)
    {
        if ((logMode != SLogTypeDef.LogType_System) && ((logMode == SLogTypeDef.LogType_Custom) && string.IsNullOrEmpty(s_logers[1].TargetPath)))
        {
            OpenLoger(SLogCategory.Skill, string.Format("{0}/sgame_skill.txt", Application.persistentDataPath));
        }
    }

    [Conditional("FORCE_LOG"), Conditional("UNITY_EDITOR"), Conditional("UNITY_STANDALONE_WIN")]
    public static void LogWarning(string warmsg)
    {
        Debug.LogWarning(warmsg);
    }

    protected void OnDestroy()
    {
        int num = 4;
        for (int i = 0; i < num; i++)
        {
            s_logers[i].Flush();
            s_logers[i].Close();
        }
        Singleton<BackgroundWorker>.DestroyInstance();
    }

    public static void OpenLoger(SLogCategory logType, string logFile)
    {
        int index = (int) logType;
        s_logers[index].Flush();
        s_logers[index].Close();
        s_logers[index].TargetPath = logFile;
    }

    public static string logRootPath
    {
        get
        {
            if (CachedLogRootPath == null)
            {
                string path = string.Format("{0}/Replay/", Application.persistentDataPath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                CachedLogRootPath = path;
            }
            return CachedLogRootPath;
        }
    }
}

