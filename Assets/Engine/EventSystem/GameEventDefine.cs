using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

namespace cs
{
    public class EventDefine
    {
        public EventDefine(int a_nID, string a_strName)
        {
            m_nID = a_nID;
            m_strName = a_strName;
        }

        public int ID { get { return m_nID; } }

        public string Name { get { return m_strName; } }

        public void RegisterHandler(UnityAction<Event> a_handler)
        {
            if (m_listHandlers == null)
            {
                m_listHandlers = new List<UnityAction<Event>>();
            }
            
            if (m_listHandlers.Contains(a_handler) == false)
            {
                m_listHandlers.Add(a_handler);
            }
        }

        public void RegisterHandler(LuaFunction a_handler)
        {
            if (m_listLuaHandles == null)
            {
                m_listLuaHandles = new List<LuaFunction>();
            }

            if (m_listLuaHandles.Contains(a_handler) == false)
            {
                m_listLuaHandles.Add(a_handler);
            }
        }

        public void UnregisterHandler(UnityAction<Event> a_handler)
        {
            if (m_listHandlers != null)
            {
                m_listHandlers.Remove(a_handler);
            }
        }

        public void UnregisterHandler(LuaFunction a_handler)
        {
            if (a_handler != null)
            {
                m_listLuaHandles.Remove(a_handler);
                a_handler.Dispose();
                a_handler = null;
            }
        }

        public void Handle(Event a_event)
        {
            if (m_listHandlers != null)
            {
                try
                {
                    List<UnityAction<Event>> listTemp = ListPool<UnityAction<Event>>.Get();
                    listTemp.AddRange(m_listHandlers);
                    for (int i = 0; i < listTemp.Count; ++i)
                    {
                        listTemp[i](a_event);
                    }
                    ListPool<UnityAction<Event>>.Release(listTemp);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e.ToString());
                }
            }

            if (m_listLuaHandles != null)
            {
                try
                {
                    List<LuaFunction> listTemp = ListPool<LuaFunction>.Get();
                    listTemp.AddRange(m_listLuaHandles);
                    for (int i = 0; i < listTemp.Count; ++i)
                    {
                        LuaFunction luaFunction = listTemp[i];
                        luaFunction.BeginPCall();
                        luaFunction.PushObject(a_event.varParam1);
                        luaFunction.PushObject(a_event.varParam2);
                        luaFunction.PushObject(a_event.varParam3);
                        luaFunction.PushObject(a_event.varParam4);
                        luaFunction.PCall();
                        luaFunction.EndPCall();
                    }
                    ListPool<LuaFunction>.Release(listTemp);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e.ToString());
                }
            }
        }

        private int m_nID;
        private string m_strName;
        private List<UnityAction<Event>> m_listHandlers;
        private List<LuaFunction> m_listLuaHandles;
    }
}

