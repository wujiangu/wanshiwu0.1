namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using ResData;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class CExploreView
    {
        private static float lastScrollX = 0f;
        public static readonly enUIEventID[] s_eventIDs = new enUIEventID[] { enUIEventID.Arena_OpenForm, enUIEventID.Adv_OpenChapterForm, enUIEventID.Burn_OpenForm };
        public static readonly Color[] s_exploreColors;
        public static readonly string[] s_exploreTypes;
        public static readonly RES_SPECIALFUNCUNLOCK_TYPE[] s_unlockTypes;

        static CExploreView()
        {
            RES_SPECIALFUNCUNLOCK_TYPE[] res_specialfuncunlock_typeArray1 = new RES_SPECIALFUNCUNLOCK_TYPE[3];
            res_specialfuncunlock_typeArray1[0] = RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_ARENA;
            res_specialfuncunlock_typeArray1[2] = RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_LIUGUOYUANZHENG;
            s_unlockTypes = res_specialfuncunlock_typeArray1;
            s_exploreTypes = new string[] { "Explore_Common_Type_2", "Explore_Common_Type_1", "Explore_Common_Type_3" };
            s_exploreColors = new Color[] { new Color(1f, 0f, 0.8470588f), new Color(0f, 0.627451f, 1f), new Color(1f, 0f, 0.04313726f) };
        }

        public static void InitExloreList(CUIFormScript form)
        {
            if (form != null)
            {
                int length = s_eventIDs.Length;
                CUIListElementScript elemenet = null;
                CUIStepListScript component = form.transform.Find("ExploreList").gameObject.GetComponent<CUIStepListScript>();
                component.SetElementAmount(length);
                for (int i = 0; i < length; i++)
                {
                    elemenet = component.GetElemenet(i);
                    elemenet.GetComponent<CUIEventScript>().m_onClickEventID = s_eventIDs[i];
                    elemenet.gameObject.transform.Find("TitleBg/ExlporeNameText").GetComponent<Text>().text = Singleton<CTextManager>.instance.GetText(s_exploreTypes[i]);
                    elemenet.gameObject.transform.Find("TitleBg/Image").GetComponent<Image>().color = s_exploreColors[i];
                    Image image = elemenet.gameObject.transform.Find("Icon").gameObject.GetComponent<Image>();
                    GameObject prefab = CUIUtility.GetSpritePrefeb(CUIUtility.s_Sprite_Dynamic_Adventure_Dir + (i + 1), false, false);
                    if (prefab != null)
                    {
                        image.SetSprite(prefab);
                    }
                    GameObject gameObject = elemenet.transform.FindChild("Lock").gameObject;
                    GameObject obj4 = elemenet.transform.FindChild("Unlock").gameObject;
                    RES_SPECIALFUNCUNLOCK_TYPE type = s_unlockTypes[i];
                    if (Singleton<CFunctionUnlockSys>.instance.FucIsUnlock(type))
                    {
                        image.color = CUIUtility.s_Color_White;
                        gameObject.CustomSetActive(false);
                    }
                    else
                    {
                        image.color = CUIUtility.s_Color_GrayShader;
                        gameObject.CustomSetActive(true);
                        ResSpecialFucUnlock dataByKey = GameDataMgr.specialFunUnlockDatabin.GetDataByKey((uint) type);
                        if (dataByKey != null)
                        {
                            gameObject.GetComponentInChildren<Text>().text = Utility.UTF8Convert(dataByKey.szLockedTip);
                        }
                    }
                    if (s_unlockTypes[i] == RES_SPECIALFUNCUNLOCK_TYPE.RES_SPECIALFUNCUNLOCKTYPE_NONE)
                    {
                        int lastChapter = CAdventureSys.GetLastChapter(1);
                        ResChapterInfo info = GameDataMgr.chapterInfoDatabin.GetDataByKey(lastChapter);
                        if (info != null)
                        {
                            obj4.CustomSetActive(true);
                            obj4.GetComponentInChildren<Text>().text = string.Format(Singleton<CTextManager>.instance.GetText("Adventure_Chapter_Max_Tips"), Utility.UTF8Convert(info.szChapterName));
                        }
                    }
                    else
                    {
                        obj4.CustomSetActive(false);
                    }
                }
                component.SelectElementImmediately(1);
            }
        }

        public static void OnExploreListScroll(GameObject root)
        {
            Vector2 scrollValue = root.transform.Find("ExploreList").gameObject.GetComponent<CUIListScript>().GetScrollValue();
            float num = scrollValue.x - lastScrollX;
            lastScrollX = scrollValue.x;
            Transform transform = root.transform.Find("FW_MovePanel/textureFrame");
            float zAngle = (num != 0f) ? ((num / (1f / ((float) (CAdventureSys.CHAPTER_NUM - 1)))) * 120f) : 0f;
            transform.Rotate(0f, 0f, zAngle);
        }
    }
}

