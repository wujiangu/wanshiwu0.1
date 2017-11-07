using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;
using LuaInterface;

namespace cs
{
    public class EventSystem : Singleton<EventSystem>, IUpdate
    {
        public int GetBuildInEventCount()
        {
            return (int)EEventID.BuildInCount;
        }

        public void DefineEvent(int a_nID, string a_strName)
        {
            if (a_nID >= 0 && a_nID < m_nMaxEventCount)
            {
                if (m_arrEventDefines[a_nID] == null)
                {
                    m_arrEventDefines[a_nID] = new EventDefine(a_nID, a_strName);
                }
                else
                {
                    Logger.Error("事件重复定义（id：{0} oldName:{1} newName:{2}）", a_nID, m_arrEventDefines[a_nID].Name, a_strName);
                }
            }
            else
            {
                Logger.Error("事件数量超出上限（{0}/{1}）", a_nID, m_nMaxEventCount - 1);
            }
        }

        public void RegisterEventHandler(int a_nID, UnityAction<Event> a_eventHandler)
        {
            if (a_nID >= 0 && a_nID < m_nMaxEventCount)
            {
                if (m_arrEventDefines[a_nID] != null)
                {
                    m_arrEventDefines[a_nID].RegisterHandler(a_eventHandler);
                }
                else
                {
                    Logger.Error("事件定义不存在 id:{0}", a_nID);
                }
            }
            else
            {
                Logger.Error("事件定义不存在 id:{0}", a_nID);
            }
        }

        public void RegisterEventHandler(int a_nID, LuaFunction a_eventHandler)
        {
            if (a_nID >= 0 && a_nID < m_nMaxEventCount)
            {
                if (m_arrEventDefines[a_nID] != null)
                {
                    m_arrEventDefines[a_nID].RegisterHandler(a_eventHandler);
                }
                else
                {
                    Logger.Error("事件定义不存在 id:{0}", a_nID);
                }
            }
            else
            {
                Logger.Error("事件定义不存在 id:{0}", a_nID);
            }
        }

        public void UnRegisterEventHandler(int a_nID, UnityAction<Event> a_eventHandler)
        {
            if (a_nID >= 0 && a_nID < m_nMaxEventCount)
            {
                if (m_arrEventDefines[a_nID] != null)
                {
                    m_arrEventDefines[a_nID].UnregisterHandler(a_eventHandler);
                }
                else
                {
                    Logger.Error("事件定义不存在 id:{0}", a_nID);
                }
            }
            else
            {
                Logger.Error("事件定义不存在 id:{0}", a_nID);
            }
        }

        public void UnRegisterEventHandler(int a_nID, LuaFunction a_eventHandler)
        {
            if (a_nID >= 0 && a_nID < m_nMaxEventCount)
            {
                if (m_arrEventDefines[a_nID] != null)
                {
                    m_arrEventDefines[a_nID].UnregisterHandler(a_eventHandler);
                }
                else
                {
                    Logger.Error("事件定义不存在 id:{0}", a_nID);
                }
            }
            else
            {
                Logger.Error("事件定义不存在 id:{0}", a_nID);
            }
        }

        public void SendEvent(int a_nID, object param1 = null, object param2 = null, object param3 = null, object param4 = null)
        {
            for (int i = 0; i < m_eventBuffers.Count; ++i)
            {
                if (m_eventBuffers[i].eventDefine.ID == a_nID)
                {
                    Logger.Error("重复发送的事件：{0}", m_eventBuffers[i].eventDefine.Name);
                    return;
                }
            }
            
            if (a_nID >= 0 && a_nID < m_nMaxEventCount)
            {
                EventDefine gameEventDefine = m_arrEventDefines[a_nID];
                if (gameEventDefine != null)
                {
                    Event gameEvent = m_eventPool.Get();
                    gameEvent.eventDefine = gameEventDefine;
                    gameEvent.varParam1 = param1;
                    gameEvent.varParam2 = param2;
                    gameEvent.varParam3 = param3;
                    gameEvent.varParam4 = param4;

                    m_eventBuffers.Add(gameEvent);
                }
                else
                {
                    Logger.Error("事件定义不存在 id:{0}", a_nID);
                }
            }
            else
            {
                Logger.Error("事件定义不存在 id:{0}", a_nID);
            }
        }

        public void SendEvent(string a_strName, object param1 = null, object param2 = null, object param3 = null, object param4 = null)
        {
            for (int i = 0; i < m_eventBuffers.Count; ++i)
            {
                if (m_eventBuffers[i].eventDefine.Name == a_strName)
                {
                    Logger.Error("重复发送的事件：{0}", m_eventBuffers[i].eventDefine.Name);
                    return;
                }
            }

            EventDefine gameEventDefine = null;
            m_dictEventDefines.TryGetValue(a_strName, out gameEventDefine);
            if (gameEventDefine != null)
            {
                Event gameEvent = m_eventPool.Get();
                gameEvent.eventDefine = gameEventDefine;
                gameEvent.varParam1 = param1;
                gameEvent.varParam2 = param2;
                gameEvent.varParam3 = param3;
                gameEvent.varParam4 = param4;

                m_eventBuffers.Add(gameEvent);
            }
            else
            {
                Logger.Error("事件定义不存在 name:{0}", a_strName);
            }
        }

        public void Tick(float a_fELapsed)
        {
            m_listExcuteEvents.Clear();
            m_listExcuteEvents.AddRange(m_eventBuffers);
            m_eventBuffers.Clear();

            for (int i = 0; i < m_listExcuteEvents.Count; ++i)
            {
                Event gameEvent = m_listExcuteEvents[i];
                gameEvent.eventDefine.Handle(gameEvent);
                m_eventPool.Release(gameEvent);
            }
            m_listExcuteEvents.Clear();
        }

        protected override void _Initialize()
        {
            m_nMaxEventCount = Setting.Get().MaxEventCount;
            m_arrEventDefines = new EventDefine[m_nMaxEventCount];
            m_dictEventDefines = new Dictionary<string, EventDefine>();
            m_listExcuteEvents = new List<Event>(25);
            m_eventBuffers = new List<Event>(25);
            m_eventPool = new ObjectPool<Event>(var => { var.Initialize(); }, var => { var.Clear(); }, null);

            DefineEvent((int)EEventID.EngineScriptLoaded, EEventID.EngineScriptLoaded.ToString());
            DefineEvent((int)EEventID.GameStateChanged, EEventID.GameStateChanged.ToString());
        }

        protected override void _Clear()
        {
            m_arrEventDefines = null;
            m_dictEventDefines = null;
            m_listExcuteEvents = null;
            m_eventBuffers = null;
            m_eventPool = null;
        }

        private int m_nMaxEventCount = 0;
        private EventDefine[] m_arrEventDefines = null;
        private Dictionary<string, EventDefine> m_dictEventDefines = null;
        
        private List<Event> m_listExcuteEvents = null;
        private List<Event> m_eventBuffers = null;
        private ObjectPool<Event> m_eventPool = null;
    }
}
