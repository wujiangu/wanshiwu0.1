namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;

    public class CChatModel
    {
        public CChatChannelMgr channelMgr = new CChatChannelMgr();
        private int index;
        public ListView<COMDT_CHAT_PLAYER_INFO> playerInfos = new ListView<COMDT_CHAT_PLAYER_INFO>();
        public List<string> selectHeroTemplateList = new List<string>();
        public CChatSysData sysData = new CChatSysData();

        public void Add_2_Friend_Tab(ulong ullUid, uint dwLogicWorldId, CChatEntity ent)
        {
            if (ent != null)
            {
                this.channelMgr.Add_ChatEntity(ent, EChatChannel.Friend, ullUid, dwLogicWorldId);
            }
        }

        public void Add_2_Guild_Tab(CChatEntity ent)
        {
            this.channelMgr.Add_ChatEntity(ent, EChatChannel.Guild, 0L, 0);
        }

        public void Add_2_Lobby_Tab(CChatEntity ent)
        {
            this.channelMgr.Add_ChatEntity(ent, EChatChannel.Lobby, 0L, 0);
        }

        public void Add_2_Room_Tab(CChatEntity ent)
        {
            if (ent != null)
            {
                this.channelMgr.Add_ChatEntity(ent, EChatChannel.Room, 0L, 0);
            }
        }

        public void Add_Palyer_Info(COMDT_CHAT_PLAYER_INFO info)
        {
            if (info != null)
            {
                int num = this.Has_PLAYER_INFO(info);
                if (num == -1)
                {
                    this.playerInfos.Add(info);
                }
                else
                {
                    this.playerInfos[num] = info;
                }
            }
        }

        public void Clear_HeroSelected()
        {
            this.index = 0;
            this.channelMgr.GetChannel(EChatChannel.Select_Hero).Clear();
        }

        public void ClearAll()
        {
            this.playerInfos.Clear();
            this.sysData.Clear();
            this.channelMgr.ClearAll();
        }

        public string Get_HeroSelect_ChatTemplate(int index)
        {
            if ((index >= 0) && (index < this.selectHeroTemplateList.Count))
            {
                return this.selectHeroTemplateList[index];
            }
            return string.Empty;
        }

        public COMDT_CHAT_PLAYER_INFO Get_Palyer_Info(ulong ullUid, uint iLogicWorldID)
        {
            COMDT_CHAT_PLAYER_INFO comdt_chat_player_info = null;
            for (int i = 0; i < this.playerInfos.Count; i++)
            {
                comdt_chat_player_info = this.playerInfos[i];
                if ((comdt_chat_player_info.ullUid == ullUid) && (comdt_chat_player_info.iLogicWorldID == iLogicWorldID))
                {
                    return comdt_chat_player_info;
                }
            }
            return null;
        }

        public int GetFriendTotal_UnreadCount()
        {
            return this.channelMgr.GetFriendTotal_UnreadCount();
        }

        public CChatEntity GetLastUnread_Selected()
        {
            ListView<CChatEntity> list = this.channelMgr.GetChannel(EChatChannel.Select_Hero).list;
            if (((list != null) && (list.Count != 0)) && (((list != null) && (this.index >= 0)) && (this.index < list.Count)))
            {
                return list[this.index++];
            }
            return null;
        }

        public int Has_PLAYER_INFO(COMDT_CHAT_PLAYER_INFO info)
        {
            int count = this.playerInfos.Count;
            for (int i = 0; i < count; i++)
            {
                if (this.playerInfos[i].ullUid == info.ullUid)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool IsTemplate_IndexValid(int index)
        {
            return ((index >= 0) && (index < this.selectHeroTemplateList.Count));
        }

        public void Load_HeroSelect_ChatTemplate()
        {
            if (this.selectHeroTemplateList.Count == 0)
            {
                DatabinTable<ResTextData, uint> selectHeroChatDatabin = GameDataMgr.m_selectHeroChatDatabin;
                if (selectHeroChatDatabin != null)
                {
                    Dictionary<long, object>.Enumerator enumerator = selectHeroChatDatabin.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<long, object> current = enumerator.Current;
                        ResTextData data = (ResTextData) current.Value;
                        this.selectHeroTemplateList.Add(StringHelper.UTF8BytesToString(ref data.szContent));
                    }
                }
            }
        }

        public void Remove_Palyer_Info(COMDT_CHAT_PLAYER_INFO info)
        {
            if (info != null)
            {
                this.playerInfos.Remove(info);
            }
        }

        public void SetRestFreeCnt(EChatChannel v, uint count)
        {
            this.sysData.restChatFreeCnt = count;
        }

        public void SetTimeStamp(EChatChannel v, uint time)
        {
            this.sysData.lastTimeStamp = time;
        }
    }
}

