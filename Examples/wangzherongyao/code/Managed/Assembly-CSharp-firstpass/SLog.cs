using System;

public class SLog
{
    private static SLogObj logObj = new SLogObj();

    public static void Close()
    {
        logObj.Close();
    }

    public static void Flush()
    {
        logObj.Flush();
    }

    public static void Log(string str)
    {
        logObj.Log(str);
    }

    public static string TargetPath
    {
        get
        {
            return logObj.TargetPath;
        }
        set
        {
            logObj.TargetPath = value;
        }
    }
}

