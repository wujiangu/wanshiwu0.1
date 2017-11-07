namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    [MessageHandlerClass]
    public class CHeroAwakeSystem : Singleton<CHeroAwakeSystem>
    {
        public CHeroInfo m_heroInfo;
        public static string s_heroAwakeFinishFormPath = "UGUI/Form/System/HeroAwake/Form_HeroAwake_Finish.prefab";
        public static string s_heroAwakeFormPath = "UGUI/Form/System/HeroAwake/Form_HeroAwake.prefab";
        public static string s_heroAwakeTaskFormPath = "UGUI/Form/System/HeroAwake/Form_HeroAwake_Task.prefab";

        private void HeroAwake_FinishTask(CUIEvent uiEvent)
        {
            SendAwakeOptMsg(this.m_heroInfo.cfgInfo.dwCfgID, 1);
        }

        private void HeroAwake_Open(CUIEvent uiEvent)
        {
            this.m_heroInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().GetHeroInfo(uiEvent.m_eventParams.tagUInt, false);
            if (this.m_heroInfo != null)
            {
                if (this.m_heroInfo.m_awakeState == 2)
                {
                    this.OpenAwakeTaskForm();
                }
                else if (this.m_heroInfo.m_awakeState == 3)
                {
                    Singleton<CUIManager>.GetInstance().OpenTips(Singleton<CTextManager>.instance.GetText("HeroAwake_Tips2"), false, 1f, null, new object[0]);
                }
                else
                {
                    this.OpenStartAwakeForm();
                }
            }
        }

        private void HeroAwake_StartAwake(CUIEvent uiEvent)
        {
            if (this.m_heroInfo != null)
            {
                if (this.m_heroInfo.m_awakeState == 0)
                {
                    Singleton<CUIManager>.GetInstance().OpenTips(Singleton<CTextManager>.instance.GetText("HeroAwake_Tips1"), false, 1f, null, new object[0]);
                }
                else
                {
                    SendAwakeOptMsg(this.m_heroInfo.cfgInfo.dwCfgID, 0);
                }
            }
        }

        public override void Init()
        {
        }

        public void OpenAwakeAwardForm()
        {
            CUIFormScript script = Singleton<CUIManager>.GetInstance().OpenForm(s_heroAwakeFinishFormPath, false, true);
            if (script != null)
            {
                Text component = script.transform.Find("Panel/panelRewards/award1/lblDesc").GetComponent<Text>();
                Text text2 = script.transform.Find("Panel/panelRewards/award2/lblDesc").GetComponent<Text>();
                CUI3DImageScript script2 = script.transform.Find("3DImage").GetComponent<CUI3DImageScript>();
                ResTalentLib dataByKey = GameDataMgr.talentLib.GetDataByKey(this.m_heroInfo.cfgInfo.dwWakeTalentID);
                if (dataByKey != null)
                {
                    component.text = StringHelper.UTF8BytesToString(ref dataByKey.szDesc);
                }
                ResHeroSkin heroSkin = CSkinInfo.GetHeroSkin(this.m_heroInfo.cfgInfo.dwWakeSkinID);
                if (heroSkin != null)
                {
                    string[] args = new string[] { StringHelper.UTF8BytesToString(ref heroSkin.szSkinName) };
                    text2.text = Singleton<CTextManager>.instance.GetText("HeroAwake_Tips3", args);
                }
                ObjNameData data = CUICommonSystem.GetHeroPrefabPath(this.m_heroInfo.cfgInfo.dwCfgID, (int) heroSkin.dwSkinID, true);
                GameObject model = (script2 == null) ? null : script2.AddGameObject(data.ObjectName, false, false);
                if (model != null)
                {
                    CHeroAnimaSystem instance = Singleton<CHeroAnimaSystem>.GetInstance();
                    instance.Set3DModel(model);
                    instance.InitAnimatList();
                    instance.InitAnimatSoundList(this.m_heroInfo.cfgInfo.dwCfgID, heroSkin.dwSkinID);
                    instance.OnModePlayAnima("Come");
                }
            }
        }

        public void OpenAwakeTaskForm()
        {
            CUIFormScript script = Singleton<CUIManager>.GetInstance().OpenForm(s_heroAwakeTaskFormPath, false, true);
            this.RefreshAwakeTaskForm();
        }

        public void OpenStartAwakeForm()
        {
            CUIFormScript script = Singleton<CUIManager>.GetInstance().OpenForm(s_heroAwakeFormPath, false, true);
            if (script != null)
            {
                Image component = script.transform.Find("Panel/PanelLeft/HeroCell/Hero").GetComponent<Image>();
                Text text = script.transform.Find("Panel/PanelLeft/lblContent").GetComponent<Text>();
                Text text2 = script.transform.Find("Panel/PanelLeft/panelRewards/award1/lblDesc").GetComponent<Text>();
                Text text3 = script.transform.Find("Panel/PanelLeft/panelRewards/award2/lblDesc").GetComponent<Text>();
                GameObject prefab = CUIUtility.GetSpritePrefeb(CUIUtility.s_Sprite_Dynamic_BustHero_Dir + StringHelper.UTF8BytesToString(ref this.m_heroInfo.cfgInfo.szImagePath), true, true);
                component.SetSprite(prefab);
                text.text = StringHelper.UTF8BytesToString(ref this.m_heroInfo.cfgInfo.szWakeDesc);
                ResTalentLib dataByKey = GameDataMgr.talentLib.GetDataByKey(this.m_heroInfo.cfgInfo.dwWakeTalentID);
                if (dataByKey != null)
                {
                    text2.text = StringHelper.UTF8BytesToString(ref dataByKey.szDesc);
                }
                ResHeroSkin heroSkin = CSkinInfo.GetHeroSkin(this.m_heroInfo.cfgInfo.dwWakeSkinID);
                if (heroSkin != null)
                {
                    string[] args = new string[] { StringHelper.UTF8BytesToString(ref heroSkin.szSkinName) };
                    text3.text = Singleton<CTextManager>.instance.GetText("HeroAwake_Tips3", args);
                }
            }
        }

        [MessageHandler(0x490)]
        public static void ReciveHeroAWakeStateCHG(CSPkg msg)
        {
            SCPKG_CMD_HERO_WAKECHG stHeroWakeChg = msg.stPkgData.stHeroWakeChg;
            CHeroInfo heroInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().GetHeroInfo(stHeroWakeChg.dwHeroID, false);
            heroInfo.m_awakeState = stHeroWakeChg.bWakeState;
            if (heroInfo.m_awakeState == 3)
            {
                Singleton<CUIManager>.GetInstance().CloseForm(s_heroAwakeTaskFormPath);
                Singleton<CHeroAwakeSystem>.GetInstance().OpenAwakeAwardForm();
            }
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
        }

        [MessageHandler(0x492)]
        public static void ReciveHeroAWakeStepCHG(CSPkg msg)
        {
            SCPKG_CMD_HERO_WAKESTEP stHeroWakeStep = msg.stPkgData.stHeroWakeStep;
            CHeroInfo heroInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().GetHeroInfo(stHeroWakeStep.dwHeroID, false);
            heroInfo.m_awakeStepID = stHeroWakeStep.dwWakeStep;
            heroInfo.m_isStepFinish = false;
            Singleton<CUIManager>.GetInstance().CloseForm(s_heroAwakeFormPath);
            Singleton<CHeroAwakeSystem>.GetInstance().OpenAwakeTaskForm();
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
        }

        public void RefreshAwakeTaskForm()
        {
            CUIFormScript form = Singleton<CUIManager>.GetInstance().GetForm(s_heroAwakeTaskFormPath);
            if (form != null)
            {
                Image component = form.transform.Find("Panel/PanelLeft/HeroCell/Hero").GetComponent<Image>();
                Text text = form.transform.Find("Panel/PanelLeft/lblTitle").GetComponent<Text>();
                Transform transform = form.transform.Find("Panel/PanelLeft/lblState");
                Text text2 = form.transform.Find("Panel/PanelLeft/lblContent").GetComponent<Text>();
                Button btn = form.transform.Find("Panel/PanelLeft/btnReciveTask").GetComponent<Button>();
                Transform transform2 = form.transform.Find("Panel/PanelLeft/panelType1");
                Text text3 = transform2.Find("lblTaskDesc").GetComponent<Text>();
                Transform transform3 = transform2.Find("itemInfo/itemCell");
                Text text4 = transform2.Find("itemInfo/lblName").GetComponent<Text>();
                Text text5 = transform2.Find("itemInfo/lblProce").GetComponent<Text>();
                Button button2 = transform2.Find("itemInfo/btnReciveTask").GetComponent<Button>();
                Button button3 = transform2.Find("getInfo/btnReciveTask").GetComponent<Button>();
                Transform transform4 = form.transform.Find("Panel/PanelLeft/panelType2");
                Text text6 = transform4.Find("taskInfo/lblTaskDesc").GetComponent<Text>();
                Text text7 = transform4.Find("taskInfo/lblProce").GetComponent<Text>();
                GameObject prefab = CUIUtility.GetSpritePrefeb(CUIUtility.s_Sprite_Dynamic_BustHero_Dir + StringHelper.UTF8BytesToString(ref this.m_heroInfo.cfgInfo.szImagePath), true, true);
                component.SetSprite(prefab);
                ResHeroWakeInfo dataByKey = GameDataMgr.heroAwakDatabin.GetDataByKey(GameDataMgr.GetDoubleKey(this.m_heroInfo.cfgInfo.dwCfgID, this.m_heroInfo.m_awakeStepID));
                if (dataByKey != null)
                {
                    text.text = StringHelper.UTF8BytesToString(ref dataByKey.szWakeTitle);
                    text2.text = StringHelper.UTF8BytesToString(ref dataByKey.szWakeDesc);
                    CRoleInfo masterRoleInfo = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo();
                    if (masterRoleInfo != null)
                    {
                        if (dataByKey.bOptType == 1)
                        {
                            transform2.gameObject.CustomSetActive(true);
                            transform4.gameObject.CustomSetActive(false);
                            CUseable itemUseable = CUseableManager.CreateUseable(COM_ITEM_TYPE.COM_OBJTYPE_ITEMPROP, (uint) dataByKey.OptParam[1], 0);
                            int useableStackCount = masterRoleInfo.GetUseableContainer(enCONTAINER_TYPE.ITEM).GetUseableStackCount(itemUseable.m_type, itemUseable.m_baseID);
                            stUIEventParams eventParams = new stUIEventParams {
                                iconUseable = itemUseable
                            };
                            itemUseable.m_stackCount = dataByKey.OptParam[2];
                            string[] args = new string[] { itemUseable.m_stackCount.ToString(), itemUseable.m_name };
                            text3.text = Singleton<CTextManager>.instance.GetText("HeroAwake_Tips4", args);
                            text4.text = itemUseable.m_name;
                            CUICommonSystem.SetItemCell(form, transform3.gameObject, itemUseable, true, false);
                            if (useableStackCount >= itemUseable.m_stackCount)
                            {
                                button2.gameObject.CustomSetActive(false);
                                transform.gameObject.CustomSetActive(true);
                                CUICommonSystem.SetButtonEnable(btn, true, true, true);
                                text5.text = string.Format("{0}/{1}", useableStackCount, itemUseable.m_stackCount);
                            }
                            else
                            {
                                button2.gameObject.CustomSetActive(true);
                                transform.gameObject.CustomSetActive(false);
                                CUICommonSystem.SetButtonEnable(btn, false, false, true);
                                text5.text = string.Format("<color=red>{0}</color>/{1}", useableStackCount, itemUseable.m_stackCount);
                                CUIEventScript script2 = button2.GetComponent<CUIEventScript>();
                                if (script2 != null)
                                {
                                    script2.SetUIEvent(enUIEventType.Click, enUIEventID.HeroInfo_Material_Direct_Buy, eventParams);
                                }
                            }
                            CUIEventScript script3 = button3.GetComponent<CUIEventScript>();
                            if (script3 != null)
                            {
                                script3.SetUIEvent(enUIEventType.Click, enUIEventID.Tips_ItemSourceInfoOpen, eventParams);
                            }
                        }
                        else
                        {
                            transform2.gameObject.CustomSetActive(false);
                            transform4.gameObject.CustomSetActive(true);
                            int num2 = dataByKey.OptParam[0];
                            CTask task = Singleton<CTaskSys>.instance.model.GetTask((uint) num2);
                            if (task == null)
                            {
                                task = TaskUT.Create_Task((uint) num2);
                                if (task == null)
                                {
                                    return;
                                }
                                task.SetState(CTask.State.None);
                            }
                            if (task != null)
                            {
                                text6.text = UT.Bytes2String(task.m_resTask.szTaskDesc);
                                string str = "    ";
                                for (int i = 0; i < task.m_prerequisiteInfo.Length; i++)
                                {
                                    if (task.m_prerequisiteInfo[i].m_valueTarget > 0)
                                    {
                                        string str2;
                                        if (!this.m_heroInfo.m_isStepFinish)
                                        {
                                            str2 = str;
                                            object[] objArray1 = new object[] { str2, task.m_prerequisiteInfo[i].m_value, "/", task.m_prerequisiteInfo[i].m_valueTarget, " " };
                                            str = string.Concat(objArray1);
                                        }
                                        else
                                        {
                                            str2 = str;
                                            object[] objArray2 = new object[] { str2, task.m_prerequisiteInfo[i].m_valueTarget, "/", task.m_prerequisiteInfo[i].m_valueTarget, " " };
                                            str = string.Concat(objArray2);
                                        }
                                        break;
                                    }
                                }
                                text7.text = str;
                            }
                            if (this.m_heroInfo.m_isStepFinish || (task.m_taskState == 3))
                            {
                                transform.gameObject.CustomSetActive(true);
                                CUICommonSystem.SetButtonEnable(btn, true, true, true);
                            }
                            else
                            {
                                transform.gameObject.CustomSetActive(false);
                                CUICommonSystem.SetButtonEnable(btn, false, false, true);
                            }
                        }
                    }
                }
            }
        }

        public static void SendAwakeOptMsg(uint heroID, byte optType)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x491);
            msg.stPkgData.stHeroWakeOpt.bOptType = optType;
            msg.stPkgData.stHeroWakeOpt.dwHeroID = heroID;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }
    }
}

