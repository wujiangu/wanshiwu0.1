namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class CSkinInfo
    {
        private uint m_wearId;
        public static DictionaryView<uint, ListView<ResHeroSkin>> s_heroSkinDic = new DictionaryView<uint, ListView<ResHeroSkin>>();

        public static int GetCombatEft(uint heroId, uint skinId)
        {
            ResHeroSkin heroSkin = GetHeroSkin(heroId, skinId);
            if (heroSkin != null)
            {
                return (int) heroSkin.dwCombatAbility;
            }
            return 0;
        }

        public static ResHeroSkin GetHeroSkin(uint uniSkinId)
        {
            return GameDataMgr.heroSkinDatabin.GetDataByKey(uniSkinId);
        }

        public static ResHeroSkin GetHeroSkin(uint heroId, uint skinId)
        {
            ListView<ResHeroSkin> view = null;
            s_heroSkinDic.TryGetValue(heroId, out view);
            if (view != null)
            {
                for (int i = 0; i < view.Count; i++)
                {
                    if (((view[i] != null) && (view[i].dwHeroID == heroId)) && (view[i].dwSkinID == skinId))
                    {
                        return view[i];
                    }
                }
            }
            return GetHeroSkin((heroId * 100) + skinId);
        }

        public static int GetHeroSkinCnt(uint heroId)
        {
            return s_heroSkinDic[heroId].Count;
        }

        public static string GetHeroSkinPic(uint heroId, uint skinId)
        {
            string str = string.Empty;
            ResHeroSkin heroSkin = GetHeroSkin(heroId, skinId);
            if (heroSkin != null)
            {
                str = StringHelper.UTF8BytesToString(ref heroSkin.szSkinPicID);
            }
            return str;
        }

        public static void GetHeroSkinProp(uint heroId, uint skinId, ref int[] propArr, ref int[] propPctArr)
        {
            int index = 0;
            int num2 = 0x24;
            for (index = 0; index < num2; index++)
            {
                propArr[index] = 0;
                propPctArr[index] = 0;
            }
            ResHeroSkin heroSkin = GetHeroSkin(heroId, skinId);
            if (heroSkin != null)
            {
                for (index = 0; index < heroSkin.astAttr.Length; index++)
                {
                    if (heroSkin.astAttr[index].wType == 0)
                    {
                        break;
                    }
                    if (heroSkin.astAttr[index].bValType == 0)
                    {
                        propArr[heroSkin.astAttr[index].wType] += heroSkin.astAttr[index].iValue;
                    }
                    else if (heroSkin.astAttr[index].bValType == 1)
                    {
                        propPctArr[heroSkin.astAttr[index].wType] += heroSkin.astAttr[index].iValue;
                    }
                }
            }
        }

        public static int GetIndexBySkinId(uint heroId, uint skinId)
        {
            ListView<ResHeroSkin> view = s_heroSkinDic[heroId];
            if (view != null)
            {
                for (int i = 0; i < view.Count; i++)
                {
                    if (view[i].dwSkinID == skinId)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static uint GetSkinCfgId(uint heroId, uint skinId)
        {
            ResHeroSkin heroSkin = GetHeroSkin(heroId, skinId);
            if (heroSkin != null)
            {
                return heroSkin.dwID;
            }
            return 0;
        }

        public static uint GetSkinIdByIndex(uint heroId, int index)
        {
            ListView<ResHeroSkin> view = s_heroSkinDic[heroId];
            if (((view != null) && (index >= 0)) && (index < view.Count))
            {
                return view[index].dwSkinID;
            }
            return 0;
        }

        public static string GetSkinName(uint skinUniId)
        {
            ResHeroSkin dataByKey = GameDataMgr.heroSkinDatabin.GetDataByKey(skinUniId);
            if (dataByKey != null)
            {
                return StringHelper.UTF8BytesToString(ref dataByKey.szSkinName);
            }
            return string.Empty;
        }

        public static stPayInfoSet GetSkinPayInfoSet(uint heroId, uint skinId)
        {
            ResSkinPromotion resPromotion = new ResSkinPromotion();
            stPayInfoSet set = new stPayInfoSet();
            resPromotion = GetSkinPromotion(heroId, skinId);
            return CMallSystem.GetPayInfoSetOfGood(GetHeroSkin(heroId, skinId), resPromotion);
        }

        public static ResSkinPromotion GetSkinPromotion(uint uniSkinId)
        {
            ResHeroSkin heroSkin = GetHeroSkin(uniSkinId);
            if (heroSkin != null)
            {
                CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                if (masterRoleInfo == null)
                {
                    return null;
                }
                for (int i = 0; i < 5; i++)
                {
                    uint key = heroSkin.PromotionID[i];
                    if ((key != 0) && GameDataMgr.skinPromotionDict.ContainsKey(key))
                    {
                        ResSkinPromotion promotion = new ResSkinPromotion();
                        if ((GameDataMgr.skinPromotionDict.TryGetValue(key, out promotion) && (promotion.dwOnTimeGen <= masterRoleInfo.getCurrentTimeSinceLogin())) && (promotion.dwOffTimeGen >= masterRoleInfo.getCurrentTimeSinceLogin()))
                        {
                            return promotion;
                        }
                    }
                }
            }
            return null;
        }

        public static ResSkinPromotion GetSkinPromotion(uint heroId, uint skinId)
        {
            return GetSkinPromotion(GetSkinCfgId(heroId, skinId));
        }

        public uint GetWearSkinId()
        {
            return this.m_wearId;
        }

        public void Init(ushort skinId)
        {
            this.m_wearId = skinId;
        }

        public static void InitHeroSkinDicData()
        {
            s_heroSkinDic.Clear();
            Dictionary<long, object>.Enumerator enumerator = GameDataMgr.heroSkinDatabin.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<long, object> current = enumerator.Current;
                ResHeroSkin item = current.Value as ResHeroSkin;
                if (((item != null) && (item.bIsShow != 0)) && (GameDataMgr.heroDatabin.GetDataByKey(item.dwHeroID) != null))
                {
                    if (!s_heroSkinDic.ContainsKey(item.dwHeroID))
                    {
                        ListView<ResHeroSkin> view = new ListView<ResHeroSkin>();
                        s_heroSkinDic.Add(item.dwHeroID, view);
                    }
                    s_heroSkinDic[item.dwHeroID].Add(item);
                }
            }
        }

        public static bool IsCanBuy(uint heroId, uint skinId)
        {
            ResHeroSkin heroSkin = GetHeroSkin(heroId, skinId);
            if (heroSkin == null)
            {
                return false;
            }
            ResSkinPromotion skinPromotion = new ResSkinPromotion();
            stPayInfoSet payInfoSetOfGood = new stPayInfoSet();
            skinPromotion = GetSkinPromotion(heroSkin.dwID);
            if (skinPromotion != null)
            {
                payInfoSetOfGood = CMallSystem.GetPayInfoSetOfGood(false, 0, skinPromotion.bIsBuyCoupons > 0, skinPromotion.dwBuyCoupons, skinPromotion.bIsBuyDiamond > 0, skinPromotion.dwBuyDiamond, 0x2710);
            }
            else
            {
                payInfoSetOfGood = CMallSystem.GetPayInfoSetOfGood(heroSkin);
            }
            return (payInfoSetOfGood.m_payInfoCount > 0);
        }

        public static bool IsOwnSkin(uint skinId, ulong ownBits)
        {
            ulong num = ((ulong) 1L) << skinId;
            return ((num & ownBits) != 0);
        }

        public static void ResolveHeroSkin(uint skinCfgId, out uint heroId, out uint skinId)
        {
            heroId = 0;
            skinId = 0;
            ResHeroSkin dataByKey = GameDataMgr.heroSkinDatabin.GetDataByKey(skinCfgId);
            if (dataByKey != null)
            {
                heroId = dataByKey.dwHeroID;
                skinId = dataByKey.dwSkinID;
            }
        }

        public void SetWearSkinId(uint skinId)
        {
            this.m_wearId = skinId;
        }
    }
}

