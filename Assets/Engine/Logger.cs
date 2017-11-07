using UnityEngine;
using System.Collections;

namespace cs
{
    public enum ELogLevel
    {
        Temp,
        Normal,
        Important,
    }

    public enum ELogColor
    {
        White,
        Red,
    }



    public class Logger
    {
        static public void Log(string a_str, params object[] a_params)
        {
            Debug.LogFormat(a_str, a_params);
        }

        static public void Warning()
        {

        }

        static public void Error(string a_str, params object[] a_params)
        {
            Debug.LogErrorFormat(a_str, a_params);
        }
    }
}


