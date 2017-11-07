using System;
using System.Collections.Generic;
using UnityEngine;

public class BeaconHelper : Singleton<BeaconHelper>
{
    private static float m_Time;

    public void Event_ApplicationPause(bool pause)
    {
        List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>> {
            new KeyValuePair<string, string>("WorldID", MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString()),
            new KeyValuePair<string, string>("GameType", Singleton<GameBuilder>.GetInstance().m_kGameType.ToString()),
            new KeyValuePair<string, string>("MapID", Singleton<GameBuilder>.GetInstance().m_iMapId.ToString()),
            new KeyValuePair<string, string>("Status", pause.ToString())
        };
        if (pause)
        {
            m_Time = Time.time;
            events.Add(new KeyValuePair<string, string>("PauseTime", string.Empty));
        }
        else
        {
            float num = Time.time - m_Time;
            events.Add(new KeyValuePair<string, string>("PauseTime", num.ToString()));
            m_Time = 0f;
        }
        events.Add(new KeyValuePair<string, string>("MyScore", string.Empty));
        events.Add(new KeyValuePair<string, string>("EnemyScore", string.Empty));
        events.Add(new KeyValuePair<string, string>("RoomID", string.Empty));
        Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_ApplicationPause", events, true);
    }

    public void Event_CommonReport(string eventName)
    {
        List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>> {
            new KeyValuePair<string, string>("IS_IOS", "0"),
            new KeyValuePair<string, string>("LoginPlatForm", Singleton<ApolloHelper>.GetInstance().CurPlatform.ToString()),
            new KeyValuePair<string, string>("worldid", MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString())
        };
        Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent(eventName, events, true);
    }

    public void EventBase(ref List<KeyValuePair<string, string>> events)
    {
        events.Add(new KeyValuePair<string, string>("IS_IOS", "0"));
        events.Add(new KeyValuePair<string, string>("LoginPlatForm", Singleton<ApolloHelper>.GetInstance().CurPlatform.ToString()));
        events.Add(new KeyValuePair<string, string>("worldid", MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString()));
    }

    public void EventPhotoReport(string status, float totalTime, string errorCode)
    {
        List<KeyValuePair<string, string>> events = new List<KeyValuePair<string, string>> {
            new KeyValuePair<string, string>("WorldID", MonoSingleton<TdirMgr>.GetInstance().SelectedTdir.logicWorldID.ToString()),
            new KeyValuePair<string, string>("status", status),
            new KeyValuePair<string, string>("totaltime", totalTime.ToString()),
            new KeyValuePair<string, string>("errorCode", errorCode)
        };
        Singleton<ApolloHelper>.GetInstance().ApolloRepoertEvent("Service_GetPhoto", events, true);
    }
}

