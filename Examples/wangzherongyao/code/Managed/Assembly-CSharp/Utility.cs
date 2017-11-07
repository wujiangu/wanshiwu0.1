using Apollo;
using Assets.Scripts.Framework;
using ResData;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Utility
{
    private static ulong[] _DW = new ulong[] { 0x2540be400L, 0x5f5e100L, 0xf4240L, 0x2710L, 100L };
    private static readonly int CHINESE_CHAR_END = Convert.ToInt32("9fff", 0x10);
    private static readonly int CHINESE_CHAR_START = Convert.ToInt32("4e00", 0x10);
    private static readonly int MAX_CHINESE_NAME_LEN = 12;
    private static readonly int MAX_EN_NAME_LEN = 9;
    private static readonly int MIN_CHINESE_NAME_LEN = 4;
    private static readonly int MIN_EN_NAME_LEN = 3;
    public static uint s_daySecond = 0x15180;
    public const long UTC_OFFSET_LOCAL = 0x7080L;
    public const long UTCTICK_PER_SECONDS = 0x989680L;

    public static byte[] BytesConvert(string s)
    {
        return Encoding.UTF8.GetBytes(s.TrimEnd(new char[1]));
    }

    public static object BytesToObject(byte[] Bytes)
    {
        using (MemoryStream stream = new MemoryStream(Bytes))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);
        }
    }

    public static NameResult CheckRoleName(string inputName)
    {
        int byteCount = 0;
        byteCount = GetByteCount(inputName);
        bool flag = true;
        if (string.IsNullOrEmpty(inputName))
        {
            return NameResult.Null;
        }
        for (int i = 0; i < inputName.Length; i++)
        {
            if (!IsQuanjiaoChar(inputName.Substring(i, 1)))
            {
                flag = false;
            }
        }
        if (flag)
        {
            if ((byteCount > MAX_CHINESE_NAME_LEN) || (byteCount < MIN_CHINESE_NAME_LEN))
            {
                return NameResult.OutOfLength;
            }
        }
        else if ((byteCount > MAX_EN_NAME_LEN) || (byteCount < MIN_EN_NAME_LEN))
        {
            return NameResult.OutOfLength;
        }
        return NameResult.Vaild;
    }

    public static string CreateMD5Hash(string input)
    {
        MD5 md = MD5.Create();
        byte[] bytes = Encoding.ASCII.GetBytes(input);
        byte[] buffer2 = md.ComputeHash(bytes);
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < buffer2.Length; i++)
        {
            builder.Append(buffer2[i].ToString("X2"));
        }
        return builder.ToString();
    }

    public static string DateTimeFormatString(DateTime dt, enDTFormate fm)
    {
        if (fm == enDTFormate.DATE)
        {
            return string.Format("{0:D4}-{1:D2}-{2:D2}", dt.Year, dt.Month, dt.Day);
        }
        if (fm == enDTFormate.TIME)
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", dt.Hour, dt.Minute, dt.Second);
        }
        return (string.Format("{0:D4}-{1:D2}-{2:D2}", dt.Year, dt.Month, dt.Day) + " " + string.Format("{0:D2}:{1:D2}:{2:D2}", dt.Hour, dt.Minute, dt.Second));
    }

    public static GameObject FindChild(GameObject p, string path)
    {
        if (p != null)
        {
            Transform transform = p.transform.FindChild(path);
            return ((transform == null) ? null : transform.gameObject);
        }
        return null;
    }

    public static GameObject FindChildByName(Component component, string childpath)
    {
        return FindChildByName(component.gameObject, childpath);
    }

    public static GameObject FindChildByName(GameObject root, string childpath)
    {
        GameObject obj2 = null;
        char[] separator = new char[] { '/' };
        string[] strArray = childpath.Split(separator);
        GameObject gameObject = root;
        foreach (string str in strArray)
        {
            bool flag = false;
            IEnumerator enumerator = gameObject.transform.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Transform current = (Transform) enumerator.Current;
                    if (current.gameObject.name == str)
                    {
                        gameObject = current.gameObject;
                        flag = true;
                        goto Label_0098;
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
        Label_0098:
            if (!flag)
            {
                break;
            }
        }
        if (gameObject != root)
        {
            obj2 = gameObject;
        }
        return obj2;
    }

    public static GameObject FindChildSafe(GameObject p, string path)
    {
        if (p != null)
        {
            Transform transform = p.transform.FindChild(path);
            if (transform != null)
            {
                return transform.gameObject;
            }
        }
        return null;
    }

    public static float FrameToTime(int frame)
    {
        return (frame * Time.fixedDeltaTime);
    }

    public static string GetBubbleText(uint tag)
    {
        string str = "BubbleText with tag [" + tag + "] was not found!";
        ResTextData dataByKey = GameDataMgr.textBubbleDatabin.GetDataByKey(tag);
        if (dataByKey != null)
        {
            str = UTF8Convert(dataByKey.szContent);
        }
        return str;
    }

    public static int GetByteCount(string inputStr)
    {
        int num = 0;
        for (int i = 0; i < inputStr.Length; i++)
        {
            if (IsQuanjiaoChar(inputStr.Substring(i, 1)))
            {
                num += 2;
            }
            else
            {
                num++;
            }
        }
        return num;
    }

    public static T GetComponetInChild<T>(GameObject p, string path) where T: MonoBehaviour
    {
        if ((p == null) || (p.transform == null))
        {
            return null;
        }
        Transform transform = p.transform.FindChild(path);
        if (transform == null)
        {
            return null;
        }
        return transform.GetComponent<T>();
    }

    private static int GetCpuClock(string cpuFile)
    {
        string s = getFileContent(cpuFile);
        int result = 0;
        if (!int.TryParse(s, out result))
        {
            result = 0;
        }
        return Mathf.FloorToInt((float) (result / 0x3e8));
    }

    public static int GetCpuCurrentClock()
    {
        return GetCpuClock("/sys/devices/system/cpu/cpu0/cpufreq/scaling_cur_freq");
    }

    public static int GetCpuMaxClock()
    {
        return GetCpuClock("/sys/devices/system/cpu/cpu0/cpufreq/cpuinfo_max_freq");
    }

    public static int GetCpuMinClock()
    {
        return GetCpuClock("/sys/devices/system/cpu/cpu0/cpufreq/cpuinfo_min_freq");
    }

    private static string getFileContent(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception.Message);
            return null;
        }
    }

    public static Vector3 GetGameObjectSize(GameObject obj)
    {
        Vector3 zero = Vector3.zero;
        if (obj.GetComponent<Renderer>() != null)
        {
            zero = obj.GetComponent<Renderer>().bounds.size;
        }
        foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
        {
            Vector3 size = renderer.bounds.size;
            zero.x = Math.Max(zero.x, size.x);
            zero.y = Math.Max(zero.y, size.y);
            zero.z = Math.Max(zero.z, size.z);
        }
        return zero;
    }

    public static uint GetGlobalRefreshTimeSeconds()
    {
        uint dwConfValue = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 10).dwConfValue;
        int num2 = Hours2Second((int) (dwConfValue / 100)) + Minutes2Seconds((int) (dwConfValue % 100));
        DateTime time = ToUtcTime2Local((long) Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().getCurrentTimeSinceLogin());
        DateTime time2 = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, DateTimeKind.Utc);
        return ToUtcSeconds(time2.AddSeconds((double) num2));
    }

    public static IApolloSnsService GetIApolloSnsService()
    {
        return (IApollo.Instance.GetService(1) as IApolloSnsService);
    }

    public static uint GetNewDayDeltaSec(int nowSec)
    {
        DateTime time = ToUtcTime2Local((long) nowSec);
        DateTime time2 = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan span = (TimeSpan) (time2.AddSeconds(86400.0) - time);
        return (uint) span.TotalSeconds;
    }

    public static string GetRecentOnlineTimeString(int recentOnlineTime)
    {
        string str = string.Empty;
        if (recentOnlineTime == 0)
        {
            return str;
        }
        int num = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().getCurrentTimeSinceLogin();
        if (num <= recentOnlineTime)
        {
            return str;
        }
        TimeSpan span = new TimeSpan(0, 0, num - recentOnlineTime);
        if (span.Days == 0)
        {
            if (span.Hours == 0)
            {
                if (span.Minutes == 0)
                {
                    return Singleton<CTextManager>.GetInstance().GetText("Guild_Tips_lastTime_default");
                }
                string[] textArray1 = new string[] { span.Minutes.ToString() };
                return Singleton<CTextManager>.GetInstance().GetText("Guild_Tips_lastTime_min", textArray1);
            }
            string[] textArray2 = new string[] { span.Hours.ToString(), span.Minutes.ToString() };
            return Singleton<CTextManager>.GetInstance().GetText("Guild_Tips_lastTime_hour_min", textArray2);
        }
        string[] args = new string[] { span.Days.ToString() };
        return Singleton<CTextManager>.GetInstance().GetText("Guild_Tips_lastTime_day", args);
    }

    public static string GetTimeBeforString(long beforSecondsFromUtc, long nowSecondsFromUtc)
    {
        TimeSpan span = new TimeSpan((beforSecondsFromUtc + 0x7080L) * 0x989680L);
        TimeSpan span2 = new TimeSpan((nowSecondsFromUtc + 0x7080L) * 0x989680L);
        int num = ((int) span2.TotalDays) - ((int) span.TotalDays);
        if (num >= 1)
        {
            string[] args = new string[] { num.ToString() };
            return Singleton<CTextManager>.GetInstance().GetText("Time_DayBefore", args);
        }
        return Singleton<CTextManager>.GetInstance().GetText("Time_Today");
    }

    public static string GetTimeSpanFormatString(int timeSpanSeconds)
    {
        TimeSpan span = new TimeSpan(timeSpanSeconds * 0x989680L);
        if (span.Days > 0)
        {
            return (span.Days.ToString() + Singleton<CTextManager>.GetInstance().GetText("Common_Day") + span.Hours.ToString() + Singleton<CTextManager>.GetInstance().GetText("Common_Hour"));
        }
        string str = (span.Hours >= 10) ? span.Hours.ToString() : ("0" + span.Hours);
        string str2 = (span.Minutes >= 10) ? span.Minutes.ToString() : ("0" + span.Minutes);
        string str3 = (span.Seconds >= 10) ? span.Seconds.ToString() : ("0" + span.Seconds);
        return string.Format("{0}:{1}:{2}", str, str2, str3);
    }

    public static System.Type GetType(string typeName)
    {
        if (!string.IsNullOrEmpty(typeName))
        {
            System.Type type = System.Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }
        }
        return null;
    }

    public static long GetZeroBaseSecond(long utcSec)
    {
        DateTime time = new DateTime(0x7b2, 1, 1);
        DateTime time2 = time.AddTicks((utcSec + 0x7080L) * 0x989680L);
        DateTime time3 = new DateTime(time2.Year, time2.Month, time2.Day, 0, 0, 0);
        TimeSpan span = (TimeSpan) (time3 - time);
        return (((long) span.TotalSeconds) - 0x7080L);
    }

    public static int Hours2Second(int hour)
    {
        return ((hour * 60) * 60);
    }

    public static bool IsChineseChar(char key)
    {
        int num = Convert.ToInt32(key);
        return ((num >= CHINESE_CHAR_START) && (num <= CHINESE_CHAR_END));
    }

    public static bool IsOverOneDay(int timeSpanSeconds)
    {
        TimeSpan span = new TimeSpan(timeSpanSeconds * 0x989680L);
        return (span.Days > 0);
    }

    public static bool IsQuanjiaoChar(string inputStr)
    {
        return (Encoding.Default.GetByteCount(inputStr) > 1);
    }

    public static bool IsSameDay(long secondsFromUtcStart1, long secondsFromUtcStart2)
    {
        DateTime time = new DateTime(0x7b2, 1, 1);
        DateTime time2 = time.AddTicks((secondsFromUtcStart1 + 0x7080L) * 0x989680L);
        DateTime time3 = time.AddTicks((secondsFromUtcStart2 + 0x7080L) * 0x989680L);
        return DateTime.Equals(time2.get_Date(), time3.get_Date());
    }

    public static bool IsSpecialChar(char key)
    {
        return ((!IsChineseChar(key) && !char.IsLetter(key)) && !char.IsNumber(key));
    }

    public static bool IsValidText(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (IsSpecialChar(text[i]))
            {
                return false;
            }
        }
        return true;
    }

    public static int Minutes2Seconds(int min)
    {
        return (min * 60);
    }

    public static byte[] ObjectToBytes(object obj)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            new BinaryFormatter().Serialize(stream, obj);
            return stream.GetBuffer();
        }
    }

    public static string ProtErrCodeToStr(int ProtId, int ErrCode)
    {
        int num2;
        string text = string.Empty;
        int num = ProtId;
        switch (num)
        {
            case 0x3fe:
            case 0x400:
            case 0x401:
            case 0x7de:
                break;

            case 0x7db:
                goto Label_0304;

            case 0xb55:
                num2 = ErrCode;
                if (num2 == 3)
                {
                    return Singleton<CTextManager>.GetInstance().GetText("Arena_ARENASETBATTLELIST_ERR_FAILED");
                }
                return text;

            case 0xb58:
                switch (ErrCode)
                {
                    case 1:
                        return Singleton<CTextManager>.GetInstance().GetText("Arena_ARENAADDMEM_ERR_ALREADYIN");

                    case 2:
                        return Singleton<CTextManager>.GetInstance().GetText("Arena_ARENAADDMEM_ERR_BATTLELISTISNULL");
                }
                return text;

            case 0x498:
                switch (ErrCode)
                {
                    case 1:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Recommend_Recommend_Sys");

                    case 2:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Recommend_Recommend_ID");

                    case 3:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Recommend_Recommend_Out_Date");

                    case 4:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Recommend_Recommend_Dup");

                    case 5:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Recommend_Recommend_Pay");

                    case 6:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Recommend_Recommend_Money");

                    case 7:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Recommend_Recommend_Exchange_Coupons_Err");

                    case 8:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Recommend_Recommend_Exchange_Coupons_Invalid");

                    case 9:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Recommend_Recommend_Failed");
                }
                return text;

            case 0x49a:
                switch (ErrCode)
                {
                    case 1:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Recommend_Roulette_Server");

                    case 2:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Recommend_Roulette_ID");

                    case 3:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Recommend_Roulette_Out_Date");

                    case 4:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Recommend_Roulette_Money");

                    case 5:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Recommend_Roulette_No_Data");
                }
                return text;

            case 0x727:
            case 0x729:
                switch (ErrCode)
                {
                    case 1:
                        return Singleton<CTextManager>.GetInstance().GetText("CS_PRESENTHEROSKIN_SYS");

                    case 2:
                        return Singleton<CTextManager>.GetInstance().GetText("CS_PRESENTHEROSKIN_LOCK");

                    case 3:
                        return Singleton<CTextManager>.GetInstance().GetText("CS_PRESENTHEROSKIN_NOALLOW");

                    case 4:
                        return Singleton<CTextManager>.GetInstance().GetText("CS_PRESENTHEROSKIN_UNFRIEND");

                    case 5:
                        return Singleton<CTextManager>.GetInstance().GetText("CS_PRESENTHEROSKIN_COINLIMIT");

                    case 6:
                        return Singleton<CTextManager>.GetInstance().GetText("CS_PRESENTHEROSKIN_MAILFAIL");
                }
                return text;

            case 0x12c2:
                switch (ErrCode)
                {
                    case 1:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Roulette_Money_Not_Enough");

                    case 2:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Roulette_Money_Type_Invalid");

                    case 3:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Roulette_Lottery_Cnt_Invalid");

                    case 4:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Roulette_Pay_Failed");

                    case 5:
                    case 7:
                        return text;

                    case 6:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Roulette_Pay_Time_Out");

                    case 8:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Roulette_Other_Err");
                }
                return text;

            case 0x12c4:
                switch (ErrCode)
                {
                    case 1:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Roulette_Money_Not_Enough");

                    case 2:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Roulette_Extern_Period_Index_Invalid");

                    case 3:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Roulette_Extern_No_Reach");

                    case 4:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Roulette_Extern_Drawed");

                    case 5:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Roulette_Extern_Reward_Id");

                    case 6:
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Roulette_Other_Err");
                }
                return text;

            default:
                switch (num)
                {
                    case 0x41b:
                        switch (ErrCode)
                        {
                            case 1:
                                return Singleton<CTextManager>.GetInstance().GetText("Arena_SINGLEGAME_ERR_FAIL");

                            case 2:
                                return Singleton<CTextManager>.GetInstance().GetText("Arena_SINGLEGAMEOFARENA_ERR_SELFLOCK");

                            case 3:
                                return Singleton<CTextManager>.GetInstance().GetText("Arena_SINGLEGAMEOFARENA_ERR_TARGETLOCK");

                            case 4:
                                return Singleton<CTextManager>.GetInstance().GetText("Arena_SINGLEGAMEOFARENA_ERR_TARGETCHG");

                            case 5:
                                return Singleton<CTextManager>.GetInstance().GetText("Arena_SINGLEGAMEOFARENA_ERR_NOTFINDTARGET");

                            case 6:
                                return Singleton<CTextManager>.GetInstance().GetText("Arena_SINGLEGAMEOFARENA_ERR_OTHERS");

                            case 7:
                                return Singleton<CTextManager>.GetInstance().GetText("Arena_ERR_LIMIT_CNT");

                            case 8:
                                return Singleton<CTextManager>.GetInstance().GetText("Arena_ERR_LIMIT_CD");

                            case 9:
                                return Singleton<CTextManager>.GetInstance().GetText("Arena_ERR_REWARD");

                            case 10:
                                return text;

                            case 11:
                                text = Singleton<CTextManager>.GetInstance().GetText("ERR_Freehero_Expire");
                                Singleton<EventRouter>.instance.BroadCastEvent(EventID.SINGLEGAME_ERR_FREEHERO);
                                return text;
                        }
                        return text;

                    case 0x41f:
                        switch (ErrCode)
                        {
                            case 1:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Sweep_Star");

                            case 2:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Sweep_VIP");

                            case 3:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Sweep_AP");

                            case 4:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Sweep_Ticket");

                            case 5:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Sweep_DianQuan");

                            case 6:
                                return text;

                            case 7:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Sweep_Other");
                        }
                        return text;

                    case 0x423:
                        num2 = ErrCode;
                        if (num2 != 10)
                        {
                            return text;
                        }
                        return Singleton<CTextManager>.GetInstance().GetText("Err_Quit_Single_Game");

                    case 0x437:
                        break;

                    case 0x580:
                        switch (ErrCode)
                        {
                            case 1:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Mystery_Shop_Not_Open");

                            case 2:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Mystery_Shop_Not_Close");

                            case 3:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Mystery_Shop_Buy_Limit");

                            case 4:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Mystery_Shop_Discount_Not_Found");

                            case 5:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Mystery_Shop_Product_Not_Found");

                            case 6:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Mystery_Shop_Product_Limit_Buy");

                            case 7:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Mystery_Shop_Account_Not_Found");

                            case 8:
                                return text;

                            case 9:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Mystery_Shop_Invalid_Shop");
                        }
                        return text;

                    case 0x71a:
                        switch (ErrCode)
                        {
                            case 7:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Buy_Hero_ForbidDianQuan");

                            case 8:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Buy_Hero_ForbidCoin");

                            case 9:
                            case 11:
                            case 12:
                                return text;

                            case 10:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Buy_Hero_ForbidDiamond");

                            case 13:
                                return Singleton<CTextManager>.GetInstance().GetText("Err_Buy_Hero_Time_Error");
                        }
                        return text;

                    case 0x7e7:
                    case 0x7ee:
                        goto Label_0304;

                    default:
                        return text;
                }
                break;
        }
        switch (ErrCode)
        {
            case 1:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Resource_Limit");

            case 2:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Game_Abort");

            case 3:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Room_Timeout");

            case 4:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Account_Leave");

            case 5:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Room_Not_Found");

            case 6:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Game_Already_Start");

            case 7:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Room_Member_Full");

            case 8:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Oper_Failed");

            case 9:
                return Singleton<CTextManager>.GetInstance().GetText("Err_No_Privilege");

            case 10:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Invalid_Param");

            case 11:
            case 12:
            case 13:
            case 14:
            case 20:
                return text;

            case 15:
                return Singleton<CTextManager>.GetInstance().GetText("Err_NM_Cancel");

            case 0x10:
                return Singleton<CTextManager>.GetInstance().GetText("Err_NM_Teamcancel");

            case 0x11:
                return Singleton<CTextManager>.GetInstance().GetText("Err_NM_Teamexit");

            case 0x12:
                return Singleton<CTextManager>.GetInstance().GetText("Err_NM_Othercancel");

            case 0x13:
                return Singleton<CTextManager>.GetInstance().GetText("Err_NM_Otherexit");

            case 0x15:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Version_Different");

            case 0x16:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Version_Higher");

            case 0x17:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Version_Lower");

            case 0x18:
                return Singleton<CTextManager>.GetInstance().GetText("Err_ENTERTAINMENT_Lock");

            default:
                return text;
        }
    Label_0304:
        switch (ErrCode)
        {
            case 1:
                return Singleton<CTextManager>.GetInstance().GetText("Err_No_Privilege");

            case 2:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Invalid_Param");

            case 3:
            case 4:
            case 5:
            case 6:
            case 13:
                return text;

            case 7:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Team_Destory");

            case 8:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Game_Already_Start");

            case 9:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Team_Member_Full");

            case 10:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Game_Ready_Error");

            case 11:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Leave_Team_Failed");

            case 12:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Join_Litmi_Rank_Failed");

            case 14:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Version_Higher");

            case 15:
                return Singleton<CTextManager>.GetInstance().GetText("Err_Version_Lower");

            case 0x10:
                return Singleton<CTextManager>.GetInstance().GetText("Err_ENTERTAINMENT_Lock");
        }
        return text;
    }

    public static byte[] ReadFile(string path)
    {
        FileStream stream = null;
        if (CFileManager.IsFileExist(path))
        {
            try
            {
                stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                int length = (int) stream.Length;
                byte[] array = new byte[length];
                stream.Read(array, 0, length);
                stream.Close();
                stream.Dispose();
                return array;
            }
            catch (Exception)
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }
        return null;
    }

    public static byte[] SByteArrToByte(sbyte[] p)
    {
        byte[] buffer = new byte[p.Length];
        for (int i = 0; i < p.Length; i++)
        {
            buffer[i] = (byte) p[i];
        }
        return buffer;
    }

    public static DateTime SecondsToDateTime(int y, int m, int d, int secondsInDay)
    {
        int hour = secondsInDay / 0xe10;
        secondsInDay = secondsInDay % 0xe10;
        return new DateTime(y, m, d, hour, secondsInDay / 60, secondsInDay % 60);
    }

    public static string SecondsToTimeText(uint secs)
    {
        uint num = secs / 0xe10;
        secs -= num * 0xe10;
        uint num2 = secs / 60;
        secs -= num2 * 60;
        return string.Format("{0:D2}:{1:D2}:{2:D2}", num, num2, secs);
    }

    public static void SetChildrenActive(GameObject root, bool active)
    {
        IEnumerator enumerator = root.transform.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                Transform current = (Transform) enumerator.Current;
                if (current != root.transform)
                {
                    current.gameObject.CustomSetActive(active);
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

    public static void StringToByteArray(string str, ref byte[] buffer)
    {
        byte[] bytes = Encoding.Default.GetBytes(str);
        Array.Copy(bytes, buffer, Math.Min(bytes.Length, buffer.Length));
        buffer[buffer.Length - 1] = 0;
    }

    public static DateTime StringToDateTime(string dtStr, DateTime defaultIfNone)
    {
        ulong num;
        if (ulong.TryParse(dtStr, out num))
        {
            return ULongToDateTime(num);
        }
        return defaultIfNone;
    }

    public static int TimeToFrame(float time)
    {
        return (int) Math.Ceiling((double) (time / Time.fixedDeltaTime));
    }

    public static uint ToUtcSeconds(DateTime dateTime)
    {
        DateTime time = new DateTime(0x7b2, 1, 1);
        if (dateTime.CompareTo(time) >= 0)
        {
            TimeSpan span = (TimeSpan) (dateTime - time);
            if (span.TotalSeconds > 28800.0)
            {
                TimeSpan span2 = (TimeSpan) (dateTime - time);
                return (((uint) span2.TotalSeconds) - 0x7080);
            }
        }
        return 0;
    }

    public static DateTime ToUtcTime2Local(long secondsFromUtcStart)
    {
        DateTime time = new DateTime(0x7b2, 1, 1);
        return time.AddTicks((secondsFromUtcStart + 0x7080L) * 0x989680L);
    }

    public static DateTime ULongToDateTime(ulong dtVal)
    {
        ulong[] numArray = new ulong[6];
        for (int i = 0; i < _DW.Length; i++)
        {
            numArray[i] = dtVal / _DW[i];
            dtVal -= numArray[i] * _DW[i];
        }
        numArray[_DW.Length] = dtVal;
        return new DateTime((int) numArray[0], (int) numArray[1], (int) numArray[2], (int) numArray[3], (int) numArray[4], (int) numArray[5]);
    }

    public static string UTF8Convert(string str)
    {
        return str;
    }

    public static string UTF8Convert(byte[] p)
    {
        return StringHelper.UTF8BytesToString(ref p);
    }

    public static string UTF8Convert(sbyte[] p, int len)
    {
        return UTF8Convert(SByteArrToByte(p));
    }

    public static bool WriteFile(string path, byte[] bytes)
    {
        FileStream stream = null;
        try
        {
            if (!CFileManager.IsFileExist(path))
            {
                stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            }
            else
            {
                stream = new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
            }
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
            stream.Close();
            stream.Dispose();
            return true;
        }
        catch (Exception)
        {
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
            }
            return false;
        }
    }

    public enum enDTFormate
    {
        FULL,
        DATE,
        TIME
    }

    public enum NameResult
    {
        Vaild,
        Null,
        OutOfLength,
        InVaildChar
    }
}

