namespace Assets.Scripts.GameSystem
{
    using CSProtocol;
    using System;

    public class RoomInfo
    {
        public ListView<MemberInfo>[] CampMemberList = new ListView<MemberInfo>[2];
        public uint dwRoomID;
        public uint dwRoomSeq;
        public int iRoomEntity;
        public RoomAttrib roomAttrib = new RoomAttrib();
        public PlayerUniqueID roomOwner = new PlayerUniqueID();
        public PlayerUniqueID selfInfo = new PlayerUniqueID();
        public uint selfObjID;

        public RoomInfo()
        {
            for (int i = 0; i < 2; i++)
            {
                this.CampMemberList[i] = new ListView<MemberInfo>();
            }
        }

        public ListView<MemberInfo> GetCampMemberList(COM_PLAYERCAMP camp)
        {
            return this.CampMemberList[((byte) camp) - 1];
        }

        public int GetFreePos(COM_PLAYERCAMP camp, int maxPlayerNum)
        {
            ListView<MemberInfo> view = this.CampMemberList[((byte) camp) - 1];
            for (int i = 0; i < (maxPlayerNum / 2); i++)
            {
                bool flag = false;
                for (int j = 0; j < view.Count; j++)
                {
                    MemberInfo info = view[j];
                    if ((info != null) && (info.dwPosOfCamp == i))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    return i;
                }
            }
            return -1;
        }

        public MemberInfo GetMasterMemberInfo()
        {
            if (this.selfObjID != 0)
            {
                return this.GetMemberInfo(this.selfObjID);
            }
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                return this.GetMemberInfo(masterRoleInfo.playerUllUID);
            }
            return null;
        }

        public MemberInfo GetMemberInfo(uint objID)
        {
            MemberInfo info = null;
            ListView<MemberInfo> campMemberList = this.GetCampMemberList(COM_PLAYERCAMP.COM_PLAYERCAMP_1);
            for (int i = 0; i < campMemberList.Count; i++)
            {
                if (campMemberList[i].dwObjId == objID)
                {
                    info = campMemberList[i];
                    break;
                }
            }
            if (info == null)
            {
                campMemberList = this.GetCampMemberList(COM_PLAYERCAMP.COM_PLAYERCAMP_2);
                for (int j = 0; j < campMemberList.Count; j++)
                {
                    if (campMemberList[j].dwObjId == objID)
                    {
                        return campMemberList[j];
                    }
                }
            }
            return info;
        }

        public MemberInfo GetMemberInfo(ulong playerUid)
        {
            MemberInfo info = null;
            ListView<MemberInfo> campMemberList = this.GetCampMemberList(COM_PLAYERCAMP.COM_PLAYERCAMP_1);
            for (int i = 0; i < campMemberList.Count; i++)
            {
                if (campMemberList[i].ullUid == playerUid)
                {
                    info = campMemberList[i];
                    break;
                }
            }
            if (info == null)
            {
                campMemberList = this.GetCampMemberList(COM_PLAYERCAMP.COM_PLAYERCAMP_2);
                for (int j = 0; j < campMemberList.Count; j++)
                {
                    if (campMemberList[j].ullUid == playerUid)
                    {
                        return campMemberList[j];
                    }
                }
            }
            return info;
        }

        public MemberInfo GetMemberInfo(COM_PLAYERCAMP camp, int campIndex)
        {
            ListView<MemberInfo> campMemberList = this.GetCampMemberList(camp);
            for (int i = 0; i < campMemberList.Count; i++)
            {
                if (campMemberList[i].dwPosOfCamp == campIndex)
                {
                    return campMemberList[i];
                }
            }
            return null;
        }

        public COM_PLAYERCAMP GetSelfCamp()
        {
            for (int i = 0; i < this.CampMemberList.Length; i++)
            {
                for (int j = 0; j < this.CampMemberList[i].Count; j++)
                {
                    if (this.CampMemberList[i][j].ullUid == this.selfInfo.ullUid)
                    {
                        return (COM_PLAYERCAMP) (i + 1);
                    }
                }
            }
            return COM_PLAYERCAMP.COM_PLAYERCAMP_1;
        }

        public bool IsHeroExist(uint heroID)
        {
            bool flag = false;
            flag = this.IsHeroExistWithCamp(COM_PLAYERCAMP.COM_PLAYERCAMP_1, heroID);
            if (!flag)
            {
                flag = this.IsHeroExistWithCamp(COM_PLAYERCAMP.COM_PLAYERCAMP_2, heroID);
            }
            return flag;
        }

        public bool IsHeroExistWithCamp(COM_PLAYERCAMP camp, uint heroID)
        {
            bool flag = false;
            ListView<MemberInfo> campMemberList = this.GetCampMemberList(camp);
            for (int i = 0; i < campMemberList.Count; i++)
            {
                if ((campMemberList[i] != null) && (campMemberList[i].ChoiceHero == null))
                {
                }
                for (int j = 0; j < campMemberList[i].ChoiceHero.Length; j++)
                {
                    if (campMemberList[i].ChoiceHero[j].stBaseInfo.stCommonInfo.dwHeroID == heroID)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            return flag;
        }
    }
}

