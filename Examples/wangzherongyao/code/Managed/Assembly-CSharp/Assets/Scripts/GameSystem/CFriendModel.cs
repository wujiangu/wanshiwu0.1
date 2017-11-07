namespace Assets.Scripts.GameSystem
{
    using CSProtocol;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class CFriendModel
    {
        private static COM_APOLLO_TRANK_SCORE_TYPE _lastSortTpye;
        private DictionaryView<uint, ListView<COMDT_FRIEND_INFO>> _map = new DictionaryView<uint, ListView<COMDT_FRIEND_INFO>>();
        private ListView<COMDT_FRIEND_INFO> _rankingFriend = new ListView<COMDT_FRIEND_INFO>();
        private ListView<COMDT_FRIEND_INFO> cache_OnlineFriends_Results = new ListView<COMDT_FRIEND_INFO>();
        private ListView<FriendInGame> gameStateList = new ListView<FriendInGame>();
        public string Guild_Has_Invited_txt;
        public string Guild_Has_Recommended_txt;
        public string Guild_Invite_txt;
        public string Guild_Recommend_txt;
        public CFriendHeartData HeartData = new CFriendHeartData();
        public CFriendReCallData SnsReCallData = new CFriendReCallData();
        public ulong ullSvrCurSec;

        public CFriendModel()
        {
            IEnumerator enumerator = Enum.GetValues(typeof(FriendType)).GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    int current = (int) enumerator.Current;
                    this._map.Add((uint) current, new ListView<COMDT_FRIEND_INFO>());
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
            this.Guild_Invite_txt = Singleton<CTextManager>.GetInstance().GetText("Guild_Invite");
            this.Guild_Has_Invited_txt = Singleton<CTextManager>.GetInstance().GetText("Guild_Has_Invited");
            this.Guild_Recommend_txt = Singleton<CTextManager>.GetInstance().GetText("Guild_Recommend");
            this.Guild_Has_Recommended_txt = Singleton<CTextManager>.GetInstance().GetText("Guild_Has_Recommended");
        }

        public void Add(FriendType type, COMDT_FRIEND_INFO data, bool ingore_worldid = false)
        {
            this.AddRankingFriend(type, data);
            ListView<COMDT_FRIEND_INFO> list = this.GetList(type);
            int num = this.getIndex(data, list, ingore_worldid);
            if (num == -1)
            {
                UT.Add2List<COMDT_FRIEND_INFO>(data, list);
            }
            else
            {
                list[num] = data;
            }
            if (type == FriendType.RequestFriend)
            {
                Singleton<EventRouter>.instance.BroadCastEvent("Friend_LobbyIconRedDot_Refresh");
            }
        }

        public void AddRankingFriend(FriendType type, COMDT_FRIEND_INFO data)
        {
            if ((type == FriendType.SNS) || (type == FriendType.GameFriend))
            {
                bool flag = false;
                for (int i = 0; i < this._rankingFriend.Count; i++)
                {
                    if ((this._rankingFriend[i].stUin.ullUid == data.stUin.ullUid) && (this._rankingFriend[i].stUin.dwLogicWorldId == data.stUin.dwLogicWorldId))
                    {
                        this._rankingFriend[i] = data;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    this._rankingFriend.Add(data);
                }
            }
        }

        public void Clear(FriendType type)
        {
            this.GetList(type).Clear();
        }

        public void ClearAll()
        {
            DictionaryView<uint, ListView<COMDT_FRIEND_INFO>>.Enumerator enumerator = this._map.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<uint, ListView<COMDT_FRIEND_INFO>> current = enumerator.Current;
                ListView<COMDT_FRIEND_INFO> view = current.Value;
                if (view != null)
                {
                    view.Clear();
                }
            }
            this.gameStateList.Clear();
            this._rankingFriend.Clear();
            this.SnsReCallData.Clear();
            this.HeartData.Clear();
        }

        public void FilterRecommendFriends()
        {
            ListView<COMDT_FRIEND_INFO> view = this._map[3];
            if (view != null)
            {
                for (int i = 0; i < view.Count; i++)
                {
                    if (this.getFriendByUid(view[i].stUin.ullUid, FriendType.GameFriend) != null)
                    {
                        view.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        private static void FindAll(ListView<COMDT_FRIEND_INFO> InSearch, Predicate<COMDT_FRIEND_INFO> match, ListView<COMDT_FRIEND_INFO> outputList)
        {
            if ((InSearch != null) && (outputList != null))
            {
                outputList.Clear();
                for (int i = 0; i < InSearch.Count; i++)
                {
                    if (match(InSearch[i]))
                    {
                        outputList.Add(InSearch[i]);
                    }
                }
            }
        }

        public static int FriendDataSort(COMDT_FRIEND_INFO l, COMDT_FRIEND_INFO r)
        {
            if ((l != r) && ((l != null) && (r != null)))
            {
                if (r.bIsOnline != l.bIsOnline)
                {
                    return (r.bIsOnline - l.bIsOnline);
                }
                if (r.dwPvpLvl > l.dwPvpLvl)
                {
                    return 1;
                }
                if (r.dwPvpLvl < l.dwPvpLvl)
                {
                    return -1;
                }
            }
            return 0;
        }

        public int GetDataCount(FriendType type)
        {
            return this.GetList(type).Count;
        }

        public COMDT_FRIEND_INFO getFriendByName(string friendName, FriendType friendType)
        {
            ListView<COMDT_FRIEND_INFO> view = this._map[(uint) friendType];
            for (int i = 0; i < view.Count; i++)
            {
                if (Utility.UTF8Convert(view[i].szUserName) == friendName)
                {
                    return view[i];
                }
            }
            return null;
        }

        public COMDT_FRIEND_INFO getFriendByUid(ulong uid, FriendType friendType)
        {
            ListView<COMDT_FRIEND_INFO> view = this._map[(uint) friendType];
            for (int i = 0; i < view.Count; i++)
            {
                if (view[i].stUin.ullUid == uid)
                {
                    return view[i];
                }
            }
            return null;
        }

        private COMDT_FRIEND_INFO getFriendInfo(ulong ullUid, uint dwLogicWorldID, ListView<COMDT_FRIEND_INFO> list)
        {
            if (list == null)
            {
                return null;
            }
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                COMDT_FRIEND_INFO comdt_friend_info2 = list[i];
                if (((comdt_friend_info2 != null) && (comdt_friend_info2.stUin.ullUid == ullUid)) && (comdt_friend_info2.stUin.dwLogicWorldId == dwLogicWorldID))
                {
                    return comdt_friend_info2;
                }
            }
            return null;
        }

        public FriendInGame GetFriendInGaming(ulong ullUid, uint dwLogicWorldID)
        {
            FriendInGame game = null;
            for (int i = 0; i < this.gameStateList.Count; i++)
            {
                game = this.gameStateList[i];
                if ((game.ullUid == ullUid) && (game.dwLogicWorldID == dwLogicWorldID))
                {
                    return game;
                }
            }
            return null;
        }

        public COM_ACNT_GAME_STATE GetFriendInGamingState(ulong ullUid, uint dwLogicWorldID)
        {
            FriendInGame game = null;
            for (int i = 0; i < this.gameStateList.Count; i++)
            {
                game = this.gameStateList[i];
                if ((game.ullUid == ullUid) && (game.dwLogicWorldID == dwLogicWorldID))
                {
                    return game.State;
                }
            }
            return COM_ACNT_GAME_STATE.COM_ACNT_GAME_STATE_IDLE;
        }

        public COMDT_FRIEND_INFO GetGameOrSnsFriend(ulong ullUid, uint dwLogicWorldID)
        {
            COMDT_FRIEND_INFO comdt_friend_info = this.getFriendInfo(ullUid, dwLogicWorldID, this.GetList(FriendType.GameFriend));
            if (comdt_friend_info == null)
            {
                comdt_friend_info = this.getFriendInfo(ullUid, dwLogicWorldID, this.GetList(FriendType.SNS));
            }
            return comdt_friend_info;
        }

        private int getIndex(COMDT_FRIEND_INFO info, ListView<COMDT_FRIEND_INFO> list, bool ingore_worldid = false)
        {
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (UT.BEqual_ACNT_UNIQ(list[i].stUin, info.stUin, ingore_worldid))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public COMDT_FRIEND_INFO GetInfo(FriendType type, COMDT_ACNT_UNIQ uniq)
        {
            return this.getFriendInfo(uniq.ullUid, uniq.dwLogicWorldId, this.GetList(type));
        }

        public COMDT_FRIEND_INFO GetInfo(FriendType type, ulong ullUid, uint dwLogicWorldID)
        {
            return this.getFriendInfo(ullUid, dwLogicWorldID, this.GetList(type));
        }

        public COMDT_FRIEND_INFO GetInfoAtIndex(FriendType type, int index)
        {
            ListView<COMDT_FRIEND_INFO> list = this.GetList(type);
            if ((list != null) && ((index >= 0) && (index < list.Count)))
            {
                return list[index];
            }
            return null;
        }

        public ListView<COMDT_FRIEND_INFO> GetList(FriendType type)
        {
            return this._map[(uint) type];
        }

        public ListView<COMDT_FRIEND_INFO> GetOnlineFriendAndSnsFriendList()
        {
            ListView<COMDT_FRIEND_INFO> list = this.GetList(FriendType.GameFriend);
            ListView<COMDT_FRIEND_INFO> view2 = this.GetList(FriendType.SNS);
            this.cache_OnlineFriends_Results.Clear();
            for (int i = 0; i < view2.Count; i++)
            {
                if (OnlineFinder(view2[i]))
                {
                    this.cache_OnlineFriends_Results.Add(view2[i]);
                }
            }
            bool flag = false;
            for (int j = 0; j < list.Count; j++)
            {
                if (!OnlineFinder(list[j]))
                {
                    continue;
                }
                flag = false;
                for (int k = 0; k < this.cache_OnlineFriends_Results.Count; k++)
                {
                    if (this.cache_OnlineFriends_Results[k].stUin.ullUid == list[j].stUin.ullUid)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    this.cache_OnlineFriends_Results.Add(list[j]);
                }
            }
            return this.cache_OnlineFriends_Results;
        }

        public ListView<COMDT_FRIEND_INFO> GetOnlineFriendList()
        {
            FindAll(this.GetList(FriendType.GameFriend), new Predicate<COMDT_FRIEND_INFO>(CFriendModel.OnlineFinder), this.cache_OnlineFriends_Results);
            return this.cache_OnlineFriends_Results;
        }

        public ListView<COMDT_FRIEND_INFO> GetSortedRankingFriendList(COM_APOLLO_TRANK_SCORE_TYPE sortType)
        {
            this.SetSortType(sortType);
            this._rankingFriend.Sort(new Comparison<COMDT_FRIEND_INFO>(CFriendModel.RankingFriendSort));
            return this._rankingFriend;
        }

        public bool IsAnyFriendExist(bool requre_bOnline)
        {
            ListView<COMDT_FRIEND_INFO> list = this.GetList(FriendType.GameFriend);
            if (!requre_bOnline)
            {
                return (list.Count > 0);
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].bIsOnline == 1)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsContain(FriendType type, COMDT_FRIEND_INFO data)
        {
            ListView<COMDT_FRIEND_INFO> list = this.GetList(type);
            if (list == null)
            {
                return false;
            }
            return list.Contains(data);
        }

        public bool IsContain(FriendType type, ulong ullUid, uint dwLogicWorldID)
        {
            COMDT_FRIEND_INFO comdt_friend_info = null;
            ListView<COMDT_FRIEND_INFO> list = this.GetList(type);
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                comdt_friend_info = list[i];
                if (((comdt_friend_info != null) && (comdt_friend_info.stUin.ullUid == ullUid)) && (comdt_friend_info.stUin.dwLogicWorldId == dwLogicWorldID))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsFriendInGamingState(ulong ullUid, uint dwLogicWorldID, COM_ACNT_GAME_STATE State)
        {
            FriendInGame game = null;
            for (int i = 0; i < this.gameStateList.Count; i++)
            {
                game = this.gameStateList[i];
                if ((game.ullUid == ullUid) && (game.dwLogicWorldID == dwLogicWorldID))
                {
                    return (game.State == State);
                }
            }
            return false;
        }

        public bool IsGameFriend(ulong ullUid)
        {
            ListView<COMDT_FRIEND_INFO> view = this._map[1];
            for (int i = 0; i < view.Count; i++)
            {
                if (view[i].stUin.ullUid == ullUid)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsSnsFriend(ulong ullUid)
        {
            ListView<COMDT_FRIEND_INFO> view = this._map[4];
            for (int i = 0; i < view.Count; i++)
            {
                if (view[i].stUin.ullUid == ullUid)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool OnlineFinder(COMDT_FRIEND_INFO a)
        {
            return (a.bIsOnline == 1);
        }

        private static int RankingFriendSort(COMDT_FRIEND_INFO l, COMDT_FRIEND_INFO r)
        {
            int index = (int) _lastSortTpye;
            ulong ullUid = r.RankVal[index];
            ulong dwPvpLvl = l.RankVal[index];
            if (_lastSortTpye == COM_APOLLO_TRANK_SCORE_TYPE.COM_APOLLO_TRANK_SCORE_TYPE_LADDER_POINT)
            {
                if (ullUid == dwPvpLvl)
                {
                    dwPvpLvl = l.dwPvpLvl;
                    ullUid = r.dwPvpLvl;
                }
                if (ullUid == dwPvpLvl)
                {
                    dwPvpLvl = l.stUin.ullUid;
                    ullUid = r.stUin.ullUid;
                }
            }
            return -dwPvpLvl.CompareTo(ullUid);
        }

        public void Remove(FriendType type, COMDT_ACNT_UNIQ uniq)
        {
            COMDT_FRIEND_INFO info = this.GetInfo(type, uniq);
            this.Remove(type, info);
        }

        public void Remove(FriendType type, COMDT_FRIEND_INFO data)
        {
            this.RemoveRankingFriend(type, data);
            this.GetList(type).Remove(data);
            if (type == FriendType.RequestFriend)
            {
                Singleton<EventRouter>.instance.BroadCastEvent("Friend_LobbyIconRedDot_Refresh");
            }
            if (type == FriendType.Recommend)
            {
                Singleton<EventRouter>.instance.BroadCastEvent("Friend_RecommandFriend_Refresh");
            }
        }

        public void RemoveRankingFriend(FriendType type, COMDT_FRIEND_INFO data)
        {
            if ((type == FriendType.SNS) || (type == FriendType.GameFriend))
            {
                for (int i = 0; i < this._rankingFriend.Count; i++)
                {
                    if ((this._rankingFriend[i].stUin.ullUid == data.stUin.ullUid) && (this._rankingFriend[i].stUin.dwLogicWorldId == data.stUin.dwLogicWorldId))
                    {
                        this._rankingFriend.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public void SetFriendGameState(ulong ullUid, uint dwLogicWorldID, COM_ACNT_GAME_STATE State, string nickName = "", bool IgnoreWorld_id = false)
        {
            FriendInGame game = null;
            for (int i = 0; i < this.gameStateList.Count; i++)
            {
                game = this.gameStateList[i];
                bool flag = false;
                if (!IgnoreWorld_id)
                {
                    flag = (game.ullUid == ullUid) && (game.dwLogicWorldID == dwLogicWorldID);
                }
                else
                {
                    flag = game.ullUid == ullUid;
                }
                if (flag)
                {
                    game.State = State;
                    game.dwLogicWorldID = dwLogicWorldID;
                    return;
                }
            }
            this.gameStateList.Add(new FriendInGame(ullUid, dwLogicWorldID, State, nickName));
        }

        public void SetGameFriendGuildState(ulong uid, COM_PLAYER_GUILD_STATE guildState)
        {
            ListView<COMDT_FRIEND_INFO> list = this.GetList(FriendType.GameFriend);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].stUin.ullUid == uid)
                {
                    list[i].bGuildState = (byte) guildState;
                }
            }
        }

        public void SetSortType(COM_APOLLO_TRANK_SCORE_TYPE sortTpye)
        {
            _lastSortTpye = sortTpye;
        }

        public void SortGameFriend()
        {
            this.GetList(FriendType.GameFriend).Sort(new Comparison<COMDT_FRIEND_INFO>(CFriendModel.FriendDataSort));
        }

        public void SortSNSFriend()
        {
            this.GetList(FriendType.SNS).Sort(new Comparison<COMDT_FRIEND_INFO>(CFriendModel.FriendDataSort));
        }

        public class FriendInGame
        {
            public uint dwLogicWorldID;
            public string nickName;
            public COM_ACNT_GAME_STATE State;
            public ulong ullUid;

            public FriendInGame(ulong uid, uint worldID, COM_ACNT_GAME_STATE state, string nickName = "")
            {
                this.ullUid = uid;
                this.dwLogicWorldID = worldID;
                this.State = state;
                this.nickName = nickName;
            }
        }

        public enum FriendType
        {
            GameFriend = 1,
            Recommend = 3,
            RequestFriend = 2,
            SNS = 4
        }
    }
}

