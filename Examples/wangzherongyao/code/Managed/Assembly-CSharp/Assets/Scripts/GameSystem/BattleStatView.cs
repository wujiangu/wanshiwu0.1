namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.GameLogic.GameKernal;
    using Assets.Scripts.UI;
    using CSProtocol;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    public class BattleStatView
    {
        private HeroItem[] _heroList0;
        private HeroItem[] _heroList1;
        private GameObject _root;
        private const int HERO_MAX_NUM = 5;
        private bool m_battleKDAChanged;
        public static string s_battleStateViewUIForm = "UGUI/Form/Battle/Form_BattleStateView.prefab";

        public void Clear()
        {
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_CloseStatView, new CUIEventManager.OnUIEventHandler(this.onCloseClick));
            Singleton<EventRouter>.instance.RemoveEventHandler(EventID.BATTLE_KDA_CHANGED, new Action(this, (IntPtr) this.OnBattleKDAChanged));
            Singleton<CUIManager>.GetInstance().CloseForm(s_battleStateViewUIForm);
            this._root = null;
            this._heroList0 = null;
            this._heroList1 = null;
        }

        public void Hide()
        {
            if (null != this._root)
            {
                Singleton<CUIManager>.GetInstance().GetForm(s_battleStateViewUIForm).Hide(true);
                Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Battle_ReviveTimeChange, new CUIEventManager.OnUIEventHandler(this.UpdateReviveTime));
                Singleton<EventRouter>.GetInstance().RemoveEventHandler<PoolObjHandle<ActorRoot>, int>("HeroSoulLevelChange", new Action<PoolObjHandle<ActorRoot>, int>(this, (IntPtr) this.onSoulLvlChange));
                Singleton<CUIParticleSystem>.instance.Show(null);
            }
        }

        public void Init()
        {
            if (Singleton<CUIManager>.GetInstance().GetForm(s_battleStateViewUIForm) == null)
            {
                CUIFormScript script = Singleton<CUIManager>.GetInstance().OpenForm(s_battleStateViewUIForm, false, true);
                this._root = script.gameObject.transform.Find("BattleStatView").gameObject;
                this._heroList0 = new HeroItem[5];
                this._heroList1 = new HeroItem[5];
                for (int i = 0; i < 5; i++)
                {
                    this._heroList0[i] = new HeroItem(Utility.FindChild(this._root, "HeroList_0/" + i));
                    this._heroList1[i] = new HeroItem(Utility.FindChild(this._root, "HeroList_1/" + i));
                }
                Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_CloseStatView, new CUIEventManager.OnUIEventHandler(this.onCloseClick));
                Singleton<EventRouter>.instance.AddEventHandler(EventID.BATTLE_KDA_CHANGED, new Action(this, (IntPtr) this.OnBattleKDAChanged));
                this.Hide();
            }
        }

        public void LateUpdate()
        {
            if (this.m_battleKDAChanged)
            {
                this.UpdateKDAView();
                this.m_battleKDAChanged = false;
            }
        }

        private void OnBattleKDAChanged()
        {
            this.m_battleKDAChanged = true;
        }

        private void onCloseClick(CUIEvent evt)
        {
            this.Hide();
        }

        private void onSoulLvlChange(PoolObjHandle<ActorRoot> act, int curVal)
        {
            if (this._root != null)
            {
                HeroItem[] itemArray = this._heroList0;
                for (int i = 0; i < itemArray.Length; i++)
                {
                    HeroItem item = itemArray[i];
                    if (((item != null) && item.Visible) && ((item.kdaData != null) && (item.kdaData.actorHero == act)))
                    {
                        item.level.text = curVal.ToString();
                    }
                    if (((i + 1) == itemArray.Length) && (itemArray == this._heroList0))
                    {
                        itemArray = this._heroList1;
                        i = -1;
                    }
                }
            }
        }

        public void Show()
        {
            if (null != this._root)
            {
                Singleton<CUIManager>.GetInstance().GetForm(s_battleStateViewUIForm).Appear();
                this.UpdateReviveTime(null);
                this.UpdateKDAView();
                Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Battle_ReviveTimeChange, new CUIEventManager.OnUIEventHandler(this.UpdateReviveTime));
                Singleton<EventRouter>.GetInstance().AddEventHandler<PoolObjHandle<ActorRoot>, int>("HeroSoulLevelChange", new Action<PoolObjHandle<ActorRoot>, int>(this, (IntPtr) this.onSoulLvlChange));
                Singleton<CUIParticleSystem>.instance.Hide(null);
            }
        }

        public void Toggle()
        {
            if (this.Visible)
            {
                this.Hide();
            }
            else
            {
                this.Show();
            }
        }

        private void UpdateKDAView()
        {
            if ((null != this._root) && !Singleton<CUIManager>.GetInstance().GetForm(s_battleStateViewUIForm).IsHided())
            {
                CPlayerKDAStat playerKDAStat = Singleton<BattleLogic>.GetInstance().battleStat.m_playerKDAStat;
                int num = 0;
                int num2 = 0;
                DictionaryView<uint, PlayerKDA>.Enumerator enumerator = playerKDAStat.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<uint, PlayerKDA> current = enumerator.Current;
                    PlayerKDA rkda = current.Value;
                    HeroItem item = null;
                    if (rkda.PlayerCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_1)
                    {
                        IEnumerator<HeroKDA> enumerator2 = rkda.GetEnumerator();
                        while (enumerator2.MoveNext())
                        {
                            item = (num >= this._heroList0.Length) ? null : this._heroList0[num++];
                            if (item != null)
                            {
                                item.Visible = true;
                                item.Validate(enumerator2.Current);
                            }
                        }
                    }
                    else if (rkda.PlayerCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_2)
                    {
                        IEnumerator<HeroKDA> enumerator3 = rkda.GetEnumerator();
                        while (enumerator3.MoveNext())
                        {
                            item = (num2 >= this._heroList1.Length) ? null : this._heroList1[num2++];
                            if (item != null)
                            {
                                item.Visible = true;
                                item.Validate(enumerator3.Current);
                            }
                        }
                    }
                }
                while (num < this._heroList0.Length)
                {
                    this._heroList0[num++].Visible = false;
                }
                while (num2 < this._heroList1.Length)
                {
                    this._heroList1[num2++].Visible = false;
                }
            }
        }

        private void UpdateReviveTime(CUIEvent evt = null)
        {
            if ((null != this._root) && !Singleton<CUIManager>.GetInstance().GetForm(s_battleStateViewUIForm).IsHided())
            {
                List<Player> allPlayers = Singleton<GamePlayerCenter>.instance.GetAllPlayers();
                int num = 0;
                int num2 = 0;
                List<Player>.Enumerator enumerator = allPlayers.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Player current = enumerator.Current;
                    HeroItem item = null;
                    if (current.PlayerCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_1)
                    {
                        item = (num >= this._heroList0.Length) ? null : this._heroList0[num++];
                        if (item != null)
                        {
                            if (current.Captain.handle.ActorControl.IsDeadState)
                            {
                                item.reviveTime.text = string.Format("{0}", Mathf.FloorToInt(current.Captain.handle.ActorControl.ReviveCooldown * 0.001f));
                                item.icon.color = CUIUtility.s_Color_Grey;
                            }
                            else
                            {
                                item.reviveTime.text = string.Empty;
                                item.icon.color = CUIUtility.s_Color_Full;
                            }
                        }
                    }
                    else if (current.PlayerCamp == COM_PLAYERCAMP.COM_PLAYERCAMP_2)
                    {
                        item = (num2 >= this._heroList1.Length) ? null : this._heroList1[num2++];
                        if (item != null)
                        {
                            if (current.Captain.handle.ActorControl.IsDeadState)
                            {
                                item.reviveTime.text = string.Format("{0}", Mathf.FloorToInt(current.Captain.handle.ActorControl.ReviveCooldown * 0.001f));
                                item.icon.color = CUIUtility.s_Color_Grey;
                                continue;
                            }
                            item.reviveTime.text = string.Empty;
                            item.icon.color = CUIUtility.s_Color_Full;
                        }
                    }
                }
            }
        }

        public bool Visible
        {
            get
            {
                return !Singleton<CUIManager>.GetInstance().GetForm(s_battleStateViewUIForm).IsHided();
            }
        }

        private class HeroItem
        {
            public Text assistNum;
            public Text deadNum;
            public Image[] equipList = new Image[6];
            public Text heroName;
            public Image icon;
            public HeroKDA kdaData;
            public Text killMon;
            public Text killNum;
            public Text level;
            public GameObject mineBg;
            public Text playerName;
            public Text reviveTime;
            public GameObject root;

            public HeroItem(GameObject node)
            {
                this.root = node;
                this.icon = Utility.GetComponetInChild<Image>(node, "HeadIcon");
                this.mineBg = Utility.FindChild(node, "MineBg");
                this.level = Utility.GetComponetInChild<Text>(node, "Level");
                this.playerName = Utility.GetComponetInChild<Text>(node, "PlayerName");
                this.heroName = Utility.GetComponetInChild<Text>(node, "HeroName");
                this.killNum = Utility.GetComponetInChild<Text>(node, "KillNum");
                this.deadNum = Utility.GetComponetInChild<Text>(node, "DeadNum");
                this.killMon = Utility.GetComponetInChild<Text>(node, "KillMon");
                this.assistNum = Utility.GetComponetInChild<Text>(node, "AssistNum");
                this.reviveTime = Utility.GetComponetInChild<Text>(node, "ReviveTime");
                GameObject p = Utility.FindChild(node, "TalentIcon");
                this.equipList[0] = Utility.GetComponetInChild<Image>(p, "img1");
                this.equipList[1] = Utility.GetComponetInChild<Image>(p, "img2");
                this.equipList[2] = Utility.GetComponetInChild<Image>(p, "img3");
                this.equipList[3] = Utility.GetComponetInChild<Image>(p, "img4");
                this.equipList[4] = Utility.GetComponetInChild<Image>(p, "img5");
                this.equipList[5] = Utility.GetComponetInChild<Image>(p, "img6");
                node.transform.FindChild("ReviveTime").gameObject.SetActive(true);
                this.kdaData = null;
            }

            public void Validate(HeroKDA kdaData)
            {
                this.kdaData = kdaData;
                this.icon.SetSprite(string.Format("{0}{1}", CUIUtility.s_Sprite_Dynamic_Icon_Dir, CSkinInfo.GetHeroSkinPic((uint) kdaData.HeroId, 0)), Singleton<CBattleSystem>.instance.m_FormScript, true, false, false);
                Player ownerPlayer = ActorHelper.GetOwnerPlayer(ref kdaData.actorHero);
                this.playerName.text = ownerPlayer.Name;
                this.heroName.text = kdaData.actorHero.handle.TheStaticData.TheResInfo.Name;
                this.level.text = kdaData.actorHero.handle.ValueComponent.actorSoulLevel.ToString();
                this.killNum.text = kdaData.numKill.ToString();
                this.deadNum.text = kdaData.numDead.ToString();
                this.killMon.text = (kdaData.numKillMonster + kdaData.numKillSoldier).ToString();
                this.killMon.text = kdaData.TotalCoin.ToString();
                this.assistNum.text = kdaData.numAssist.ToString();
                int num = 1;
                for (int i = 0; i < 6; i++)
                {
                    ushort equipID = kdaData.Equips[i].m_equipID;
                    if (equipID != 0)
                    {
                        num++;
                        CUICommonSystem.SetEquipIcon(equipID, this.equipList[i].gameObject, Singleton<CBattleSystem>.instance.m_FormScript);
                    }
                }
                for (int j = num; j <= 6; j++)
                {
                    this.equipList[j - 1].gameObject.GetComponent<Image>().SetSprite(string.Format("{0}EquipmentSpace", CUIUtility.s_Sprite_Dynamic_Talent_Dir), Singleton<CBattleSystem>.instance.m_FormScript, true, false, false);
                }
                if (ownerPlayer == Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer())
                {
                    this.playerName.color = CUIUtility.s_Text_Color_Self;
                    this.mineBg.CustomSetActive(true);
                }
                else
                {
                    this.mineBg.CustomSetActive(false);
                }
            }

            public bool Visible
            {
                get
                {
                    return ((this.root != null) && this.root.activeSelf);
                }
                set
                {
                    if (this.root != null)
                    {
                        this.root.CustomSetActive(value);
                    }
                }
            }
        }
    }
}

