using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

namespace cs
{
    public class LuaManager : Singleton<LuaManager>
    {
        public void DoFile(string a_strName)
        {
            m_luaState.DoFile(a_strName);
        }

        //public object[] CallFunction(string funcName, params object[] args)
        //{
        //    LuaFunction func = m_luaState.GetFunction(funcName);
        //    if (func != null)
        //    {
        //        return func.LazyCall(args);
        //    }
        //    return null;
        //}

        protected override void _Initialize()
        {
            if (m_luaState == null)
            {
                m_luaState = new LuaState();
            }
            m_luaState.Start();
            LuaBinder.Bind(m_luaState);

            string[] arrPaths = Setting.Get().LuaPaths;
            for (int i = 0; i < arrPaths.Length; ++i)
            {
                m_luaState.AddSearchPath(Application.dataPath + arrPaths[i]);
            }
        }

        protected override void _Clear()
        {
            m_luaState.Dispose();
            m_luaState = null;
        }

        private LuaState m_luaState;
    }
}


