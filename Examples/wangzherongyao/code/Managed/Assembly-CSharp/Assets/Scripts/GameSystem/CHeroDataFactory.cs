namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class CHeroDataFactory
    {
        [CompilerGenerated]
        private static Action<ResHeroCfgInfo> <>f__am$cache1;
        private static ListView<ResHeroCfgInfo> m_CfgHeroList;

        public static IHeroData CreateCustomHeroData(uint id)
        {
            return new CCustomHeroData(id);
        }

        public static IHeroData CreateHeroData(uint id)
        {
            CHeroInfo info2;
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            DebugHelper.Assert(masterRoleInfo != null, "CreateHeroData ---- Master role is null");
            if ((masterRoleInfo != null) && masterRoleInfo.GetHeroInfo(id, out info2, true))
            {
                return new CHeroInfoData(info2);
            }
            return new CHeroCfgData(id);
        }

        public static ListView<ResHeroCfgInfo> GetAllHeroList()
        {
            if (m_CfgHeroList == null)
            {
                m_CfgHeroList = new ListView<ResHeroCfgInfo>();
            }
            if (m_CfgHeroList.Count <= 0)
            {
                if (<>f__am$cache1 == null)
                {
                    <>f__am$cache1 = delegate (ResHeroCfgInfo x) {
                        if ((x.bIsPlayerUse > 0) && (!CSysDynamicBlock.bLobbyEntryBlocked || (x.bIOSHide == 0)))
                        {
                            m_CfgHeroList.Add(x);
                        }
                    };
                }
                GameDataMgr.heroDatabin.Accept(<>f__am$cache1);
            }
            return m_CfgHeroList;
        }

        public static ListView<IHeroData> GetHostHeroList(bool isIncludeValidExperienceHero)
        {
            ListView<IHeroData> view = new ListView<IHeroData>();
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                DictionaryView<uint, CHeroInfo>.Enumerator enumerator = masterRoleInfo.GetHeroInfoDic().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (isIncludeValidExperienceHero)
                    {
                        KeyValuePair<uint, CHeroInfo> current = enumerator.Current;
                        if (!masterRoleInfo.IsOwnHero(current.Key))
                        {
                            KeyValuePair<uint, CHeroInfo> pair2 = enumerator.Current;
                            if (!masterRoleInfo.IsValidExperienceHero(pair2.Key))
                            {
                                continue;
                            }
                        }
                        KeyValuePair<uint, CHeroInfo> pair3 = enumerator.Current;
                        view.Add(CreateHeroData(pair3.Key));
                    }
                    else
                    {
                        KeyValuePair<uint, CHeroInfo> pair4 = enumerator.Current;
                        if (masterRoleInfo.IsOwnHero(pair4.Key))
                        {
                            KeyValuePair<uint, CHeroInfo> pair5 = enumerator.Current;
                            view.Add(CreateHeroData(pair5.Key));
                        }
                    }
                }
                if (CSysDynamicBlock.bLobbyEntryBlocked)
                {
                    for (int i = view.Count - 1; i >= 0; i--)
                    {
                        IHeroData item = view[i];
                        if (item.heroCfgInfo.bIOSHide > 0)
                        {
                            view.Remove(item);
                        }
                    }
                }
            }
            return view;
        }

        public static ListView<IHeroData> GetPvPHeroList()
        {
            ListView<IHeroData> heroList = new ListView<IHeroData>();
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                DictionaryView<uint, CHeroInfo>.Enumerator enumerator = masterRoleInfo.GetHeroInfoDic().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<uint, CHeroInfo> current = enumerator.Current;
                    if ((current.Value.MaskBits & 2) > 0)
                    {
                        KeyValuePair<uint, CHeroInfo> pair2 = enumerator.Current;
                        heroList.Add(CreateHeroData(pair2.Key));
                    }
                }
                for (int i = 0; i < masterRoleInfo.freeHeroList.Count; i++)
                {
                    if (!masterRoleInfo.GetHeroInfoDic().ContainsKey(masterRoleInfo.freeHeroList[i]))
                    {
                        heroList.Add(CreateHeroData(masterRoleInfo.freeHeroList[i]));
                    }
                }
                if (CSysDynamicBlock.bLobbyEntryBlocked)
                {
                    for (int j = heroList.Count - 1; j >= 0; j--)
                    {
                        IHeroData item = heroList[j];
                        if (item.heroCfgInfo.bIOSHide > 0)
                        {
                            heroList.Remove(item);
                        }
                    }
                }
                CHeroOverviewSystem.SortHeroList(ref heroList);
            }
            return heroList;
        }

        public static ListView<IHeroData> GetTrainingHeroList()
        {
            ListView<IHeroData> heroList = new ListView<IHeroData>();
            List<uint> list = new List<uint>();
            CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
            if (masterRoleInfo != null)
            {
                DictionaryView<uint, CHeroInfo>.Enumerator enumerator = masterRoleInfo.GetHeroInfoDic().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<uint, CHeroInfo> current = enumerator.Current;
                    if ((current.Value.MaskBits & 2) > 0)
                    {
                        KeyValuePair<uint, CHeroInfo> pair2 = enumerator.Current;
                        heroList.Add(CreateHeroData(pair2.Key));
                        KeyValuePair<uint, CHeroInfo> pair3 = enumerator.Current;
                        list.Add(pair3.Key);
                    }
                }
                for (int i = 0; i < masterRoleInfo.freeHeroList.Count; i++)
                {
                    if (!masterRoleInfo.GetHeroInfoDic().ContainsKey(masterRoleInfo.freeHeroList[i]))
                    {
                        heroList.Add(CreateHeroData(masterRoleInfo.freeHeroList[i]));
                        list.Add(masterRoleInfo.freeHeroList[i]);
                    }
                }
                ListView<ResHeroCfgInfo> allHeroList = GetAllHeroList();
                for (int j = 0; j < allHeroList.Count; j++)
                {
                    if ((allHeroList[j].bIsTrainUse == 1) && !list.Contains(allHeroList[j].dwCfgID))
                    {
                        heroList.Add(CreateHeroData(allHeroList[j].dwCfgID));
                    }
                }
                if (CSysDynamicBlock.bLobbyEntryBlocked)
                {
                    for (int k = heroList.Count - 1; k >= 0; k--)
                    {
                        IHeroData item = heroList[k];
                        if (item.heroCfgInfo.bIOSHide > 0)
                        {
                            heroList.Remove(item);
                        }
                    }
                }
                CHeroOverviewSystem.SortHeroList(ref heroList);
            }
            return heroList;
        }
    }
}

