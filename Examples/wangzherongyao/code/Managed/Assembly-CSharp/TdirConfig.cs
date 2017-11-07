using Mono.Xml;
using System;
using System.Collections;
using System.IO;
using System.Security;
using System.Text;
using UnityEngine;

public class TdirConfig
{
    public static int AppID_android = 0x9e00011;
    public static int AppID_iOS = 0x9e00012;
    public static TdirServerType cheatServerType = TdirServerType.Normal;
    public static string[] iplist_experience = new string[] { "exp.mtcls.qq.com", "61.151.234.47", "182.254.42.103", "140.207.62.111", "140.207.123.164", "117.144.242.28", "117.135.171.74", "103.7.30.91", "101.227.130.79" };
    public static string[] iplist_experience_test = new string[] { "testb4.mtcls.qq.com", "101.227.153.86" };
    public static string[] iplist_middle = new string[] { "middle.mtcls.qq.com", "101.226.141.88" };
    public static string[] iplist_normal = new string[] { "mtcls.qq.com", "61.151.224.100", "58.251.61.169", "203.205.151.237", "203.205.147.178", "183.61.49.177", "183.232.103.166", "182.254.4.176", "182.254.10.82", "140.207.127.61", "117.144.242.115" };
    public static string[] iplist_test = new string[] { "testa4.mtcls.qq.com", "101.227.153.83" };
    public static string[] iplist_testForTester = new string[] { "testc.mtcls.qq.com", "183.61.39.51" };
    public static int[] portlist_experience = new int[] { 0x2712, 0x2714, 0x2716 };
    public static int[] portlist_experience_test = new int[] { 0x2712, 0x2714, 0x2716 };
    public static int[] portlist_middle = new int[] { 0x4e22, 0x4e24, 0x4e26 };
    public static int[] portlist_normal = new int[] { 0x4e22, 0x4e24, 0x4e26 };
    public static int[] portlist_test = new int[] { 0x2712, 0x2714, 0x2716 };
    public static int[] portlist_testForTester = new int[] { 0x2712, 0x2714, 0x2716 };
    private static TdirConfigData tdirConfigData = null;
    public static string tdirConfigDataPath = "/TdirConfigData.xml";
    public static TdirServerType WoYaoQiehuanJing = TdirServerType.NULL;

    public static TdirConfigData GetFileTdirAndTverData()
    {
        if ((tdirConfigData == null) && File.Exists(Application.persistentDataPath + tdirConfigDataPath))
        {
            try
            {
                byte[] bytes = CFileManager.ReadFile(Application.persistentDataPath + tdirConfigDataPath);
                if ((bytes != null) && (bytes.Length > 0))
                {
                    tdirConfigData = new TdirConfigData();
                    string xml = Encoding.UTF8.GetString(bytes);
                    Mono.Xml.SecurityParser parser = new Mono.Xml.SecurityParser();
                    parser.LoadXml(xml);
                    IEnumerator enumerator = parser.ToXml().Children.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            SecurityElement current = (SecurityElement) enumerator.Current;
                            if (current.Tag == "serverType")
                            {
                                tdirConfigData.serverType = int.Parse(current.Text);
                            }
                            else if (current.Tag == "versionType")
                            {
                                tdirConfigData.versionType = int.Parse(current.Text);
                            }
                        }
                    }
                    finally
                    {
                        IDisposable disposable = enumerator as IDisposable;
                        if (disposable == null)
                        {
                        }
                        disposable.Dispose();
                    }
                }
            }
            catch (Exception)
            {
                tdirConfigData = null;
            }
        }
        return tdirConfigData;
    }

    public static int GetTdirAppId()
    {
        return AppID_android;
    }

    public static string[] GetTdirIPList()
    {
        TdirConfigData fileTdirAndTverData = GetFileTdirAndTverData();
        if (fileTdirAndTverData != null)
        {
            if (fileTdirAndTverData.serverType == 1)
            {
                return iplist_test;
            }
            if (fileTdirAndTverData.serverType == 2)
            {
                return iplist_middle;
            }
            if (fileTdirAndTverData.serverType == 3)
            {
                return iplist_normal;
            }
            if (fileTdirAndTverData.serverType == 4)
            {
                return iplist_experience;
            }
            if (fileTdirAndTverData.serverType == 5)
            {
                return iplist_experience_test;
            }
            if (fileTdirAndTverData.serverType == 6)
            {
                return iplist_testForTester;
            }
        }
        if (cheatServerType == TdirServerType.Test)
        {
            return iplist_test;
        }
        if (cheatServerType == TdirServerType.Mid)
        {
            return iplist_middle;
        }
        if (cheatServerType != TdirServerType.Normal)
        {
            if (cheatServerType == TdirServerType.Exp)
            {
                return iplist_experience;
            }
            if (cheatServerType == TdirServerType.ExpTest)
            {
                return iplist_experience_test;
            }
            if (cheatServerType == TdirServerType.TestForTester)
            {
                return iplist_testForTester;
            }
        }
        return iplist_normal;
    }

    public static int[] GetTdirPortList()
    {
        TdirConfigData fileTdirAndTverData = GetFileTdirAndTverData();
        if (fileTdirAndTverData != null)
        {
            if (fileTdirAndTverData.serverType == 1)
            {
                return portlist_test;
            }
            if (fileTdirAndTverData.serverType == 2)
            {
                return portlist_middle;
            }
            if (fileTdirAndTverData.serverType == 3)
            {
                return portlist_normal;
            }
            if (fileTdirAndTverData.serverType == 4)
            {
                return portlist_experience;
            }
            if (fileTdirAndTverData.serverType == 5)
            {
                return portlist_experience_test;
            }
            if (fileTdirAndTverData.serverType == 6)
            {
                return portlist_testForTester;
            }
        }
        if (cheatServerType == TdirServerType.Test)
        {
            return portlist_test;
        }
        if (cheatServerType == TdirServerType.Mid)
        {
            return portlist_middle;
        }
        if (cheatServerType != TdirServerType.Normal)
        {
            if (cheatServerType == TdirServerType.Exp)
            {
                return portlist_experience;
            }
            if (cheatServerType == TdirServerType.ExpTest)
            {
                return portlist_experience_test;
            }
            if (cheatServerType == TdirServerType.TestForTester)
            {
                return portlist_testForTester;
            }
        }
        return portlist_normal;
    }
}

