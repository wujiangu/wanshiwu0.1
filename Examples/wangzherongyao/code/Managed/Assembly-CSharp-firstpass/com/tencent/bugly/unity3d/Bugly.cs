namespace com.tencent.bugly.unity3d
{
    using com.tencent.bugly.unity3d.android;
    using System;
    using UnityEngine;

    public class Bugly
    {
        public static void EnableExceptionHandler()
        {
            if ((Application.platform != RuntimePlatform.IPhonePlayer) && (Application.platform == RuntimePlatform.Android))
            {
                com.tencent.bugly.unity3d.android.Bugly.EnableExceptionHandler();
            }
        }

        public static void EnableLog(bool enable)
        {
            if ((Application.platform != RuntimePlatform.IPhonePlayer) && (Application.platform == RuntimePlatform.Android))
            {
                com.tencent.bugly.unity3d.android.Bugly.EnableLog(enable);
            }
        }

        public static void HandleException(Exception e)
        {
            if ((Application.platform != RuntimePlatform.IPhonePlayer) && (Application.platform == RuntimePlatform.Android))
            {
                com.tencent.bugly.unity3d.android.Bugly.OnExceptionCaught(e);
            }
        }

        public static void InitSDK(string appId)
        {
            if ((Application.platform != RuntimePlatform.IPhonePlayer) && (Application.platform == RuntimePlatform.Android))
            {
                com.tencent.bugly.unity3d.android.Bugly.InitWithAppId(appId);
            }
        }

        public static void RegisterHandler(LogSeverity level)
        {
            if ((Application.platform != RuntimePlatform.IPhonePlayer) && (Application.platform == RuntimePlatform.Android))
            {
                com.tencent.bugly.unity3d.android.Bugly.RegisterExceptionHandler(level);
            }
        }

        public static void SetAppVersion(string version)
        {
            if ((Application.platform != RuntimePlatform.IPhonePlayer) && (Application.platform == RuntimePlatform.Android))
            {
                com.tencent.bugly.unity3d.android.Bugly.SetVersion(version);
            }
        }

        public static void SetChannel(string channel)
        {
            if ((Application.platform != RuntimePlatform.IPhonePlayer) && (Application.platform == RuntimePlatform.Android))
            {
                com.tencent.bugly.unity3d.android.Bugly.SetChannel(channel);
            }
        }

        public static void SetGameObjectForCallback(string gameObject)
        {
            if ((gameObject == null) || (gameObject.Trim().Length == 0))
            {
                gameObject = "Main Camera";
            }
            if ((Application.platform != RuntimePlatform.IPhonePlayer) && (Application.platform == RuntimePlatform.Android))
            {
                com.tencent.bugly.unity3d.android.Bugly.SetCallbackObject(gameObject);
            }
        }

        public static void SetReportDelayTime(string delay)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                long num = 0L;
                try
                {
                    if (delay != null)
                    {
                        delay = delay.Trim();
                        if (delay.Length > 0)
                        {
                            num = Convert.ToInt64(delay);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debugger.Error(string.Format("Fail to set report delay time cause by {0}", exception.ToString()));
                    num = 0L;
                }
                com.tencent.bugly.unity3d.android.Bugly.SetDelayReportTime(num);
            }
        }

        public static void SetUserId(string userId)
        {
            if ((Application.platform != RuntimePlatform.IPhonePlayer) && (Application.platform == RuntimePlatform.Android))
            {
                com.tencent.bugly.unity3d.android.Bugly.SetUserId(userId);
            }
        }
    }
}

