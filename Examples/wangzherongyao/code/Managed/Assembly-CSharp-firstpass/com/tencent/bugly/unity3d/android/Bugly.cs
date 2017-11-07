namespace com.tencent.bugly.unity3d.android
{
    using com.tencent.bugly.unity3d;
    using System;
    using UnityEngine;

    public static class Bugly
    {
        public static void EnableExceptionHandler()
        {
            BuglyAgent.GetInstance().RegisterExceptionHandler();
        }

        public static void EnableLog(bool enable)
        {
            BuglyAgent.GetInstance().EnableLog(enable);
        }

        public static void InitWithAppId(string appId)
        {
            BuglyAgent.GetInstance().InitWithAppId(appId);
        }

        public static void OnExceptionCaught(Exception e)
        {
            BuglyAgent.GetInstance().OnExceptionCaught(e);
        }

        public static void RegisterExceptionHandler(LogSeverity level)
        {
            BuglyAgent.GetInstance().SetLogLevel(level);
        }

        public static void SetCallbackObject(string gameObject)
        {
            BuglyAgent.GetInstance().SetCallbackObject(gameObject);
        }

        public static void SetChannel(string channel)
        {
            BuglyAgent.GetInstance().SetChannel(channel);
        }

        public static void SetCrashHappenCallback(string callbackName)
        {
            if (callbackName != null)
            {
                BuglyAgent.GetInstance().SetCrashHappenCallback(callbackName);
            }
        }

        public static void SetCrashUploadCallback(string callbackName)
        {
            BuglyAgent.GetInstance().SetCrashUploadCallback(callbackName);
        }

        public static void SetDelayReportTime(long delay)
        {
            BuglyAgent.GetInstance().SetDelayReportTime(delay);
        }

        public static void SetUserId(string userId)
        {
            BuglyAgent.GetInstance().SetUserId(userId);
        }

        public static void SetVersion(string version)
        {
            BuglyAgent.GetInstance().SetVersion(version);
        }

        public static void UnregisterExceptionHandler()
        {
        }

        private sealed class BuglyAgent : ExceptionHandler
        {
            private AndroidJavaObject _bugly = new AndroidJavaClass("com.tencent.bugly.unity.UnityAgent").CallStatic<AndroidJavaObject>("getInstance", new object[0]);
            private string _gameObjectForCallback = "Main Camera";
            public static readonly com.tencent.bugly.unity3d.android.Bugly.BuglyAgent instance = new com.tencent.bugly.unity3d.android.Bugly.BuglyAgent();

            private BuglyAgent()
            {
            }

            public void EnableLog(bool enable)
            {
                object[] args = new object[] { enable };
                this._bugly.Call("setLogEnable", args);
            }

            public static com.tencent.bugly.unity3d.android.Bugly.BuglyAgent GetInstance()
            {
                return instance;
            }

            public void InitWithAppId(string appId)
            {
                base.RegisterExceptionHandler();
                object[] args = new object[] { appId };
                this._bugly.Call("initSDK", args);
            }

            public override void OnUncaughtExceptionReport(string type, string message, string stack)
            {
                this.ReportException(type, message, stack);
            }

            private void ReportException(string errorClass, string errorMessage, string callStack)
            {
                object[] args = new object[] { errorClass, errorMessage, callStack };
                this._bugly.Call("traceException", args);
            }

            public void SetCallbackObject(string gameObject)
            {
                if (gameObject != null)
                {
                    this._gameObjectForCallback = gameObject;
                }
            }

            public void SetChannel(string channel)
            {
                object[] args = new object[] { channel };
                this._bugly.Call("setChannel", args);
            }

            public void SetCrashHappenCallback(string callbackName)
            {
                if (callbackName != null)
                {
                    object[] args = new object[] { this._gameObjectForCallback, callbackName };
                    this._bugly.Call("setCrashHappenListener", args);
                }
            }

            public void SetCrashUploadCallback(string callbackName)
            {
                if (callbackName != null)
                {
                    object[] args = new object[] { this._gameObjectForCallback, callbackName };
                    this._bugly.Call("setCrashUploadListener", args);
                }
            }

            public void SetDelayReportTime(long delay)
            {
                object[] args = new object[] { delay };
                this._bugly.Call("setDelay", args);
            }

            public void SetLogLevel(LogSeverity level)
            {
                base._logLevel = level;
            }

            public void SetUserId(string userId)
            {
                object[] args = new object[] { userId };
                this._bugly.Call("setUserId", args);
            }

            public void SetVersion(string version)
            {
                object[] args = new object[] { version };
                this._bugly.Call("setVersion", args);
            }
        }
    }
}

