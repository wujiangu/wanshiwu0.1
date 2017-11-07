namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.GameLogic.GameKernal;
    using Assets.Scripts.UI;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    public class MinimapSys
    {
        private GameObject bigMap_obj;
        private EMapType curMapType;
        public static string enemy_base = "UGUI/Sprite/Battle/Img_Map_Base_Red";
        public static string enemy_base_outline = "UGUI/Sprite/Battle/Img_Map_Base_Red_outline";
        public static string enemy_tower = "UGUI/Sprite/Battle/Img_Map_Tower_Red";
        public static string enemy_tower_outline = "UGUI/Sprite/Battle/Img_Map_Tower_Red_outline";
        private DragonIcon m_dragonIcon;
        public CUIFormScript m_FormScript;
        private GameObject miniMap_obj;
        public static string self_base = "UGUI/Sprite/Battle/Img_Map_Base_Green";
        public static string self_base_outline = "UGUI/Sprite/Battle/Img_Map_Base_Green_outline";
        public static string self_tower = "UGUI/Sprite/Battle/Img_Map_Tower_Green";
        public static string self_tower_outline = "UGUI/Sprite/Battle/Img_Map_Tower_Green_outline";

        private void _init_view(GameObject minimap, GameObject bigmap)
        {
            if ((minimap != null) && (bigmap != null))
            {
            }
        }

        public void Clear()
        {
            this.unRegEvent();
            if (this.m_dragonIcon != null)
            {
                this.m_dragonIcon.Clear();
                this.m_dragonIcon = null;
            }
            this.miniMap_obj = null;
            this.bigMap_obj = null;
            this.m_FormScript = null;
        }

        public EMapType CurMapType()
        {
            return this.curMapType;
        }

        public void Init(CUIFormScript formObj, ResDT_LevelCommonInfo pvp_level_cfg, SLevelContext levelContext)
        {
            if (((formObj != null) && (pvp_level_cfg != null)) && (levelContext != null))
            {
                this.regEvent();
                RectTransform transform = null;
                this.m_FormScript = formObj;
                this.miniMap_obj = Utility.FindChild(formObj.gameObject, "panelTopLeft/MiniMap");
                this.bigMap_obj = Utility.FindChild(formObj.gameObject, "panelTopLeft/BigMap");
                DebugHelper.Assert(this.miniMap_obj != null, "---MinimapSys miniMap_obj == null, check out...");
                DebugHelper.Assert(this.bigMap_obj != null, "---MinimapSys bigMap_obj == null, check out...");
                if ((this.miniMap_obj != null) && (this.bigMap_obj != null))
                {
                    this.miniMap_obj.CustomSetActive(true);
                    this.bigMap_obj.CustomSetActive(false);
                    this._init_view(this.miniMap_obj, this.bigMap_obj);
                    Image component = Utility.FindChild(formObj.gameObject, "panelTopLeft/MiniMap").GetComponent<Image>();
                    DebugHelper.Assert(component != null, "---MinimapSys miniMapImage == null, check out...");
                    if (component != null)
                    {
                        if (levelContext.isPVPMode)
                        {
                            float num;
                            transform = this.initMap(enBattleFormWidget.Bigmap, levelContext, false, out num);
                            transform.anchoredPosition = new Vector2(transform.rect.width * 0.5f, -transform.rect.height * 0.5f);
                            transform = this.initMap(enBattleFormWidget.Minimap, levelContext, true, out num);
                            if ((pvp_level_cfg != null) && (pvp_level_cfg.bMaxAcntNum == 6))
                            {
                                transform.anchoredPosition = new Vector2(transform.anchoredPosition.x + ((transform.rect.width * 0.5f) - (num * 0.5f)), transform.anchoredPosition.y);
                                GameObject obj2 = Utility.FindChild(this.m_FormScript.gameObject, "panelTopLeft/DragonInfo");
                                GameObject obj3 = Utility.FindChild(this.m_FormScript.gameObject, "panelTopLeft/Button_Signal_1");
                                DebugHelper.Assert(obj2 != null, "---MinimapSys dragon_info == null, check out...");
                                DebugHelper.Assert(obj3 != null, "---MinimapSys button_signal_1 == null, check out...");
                                if (obj2 != null)
                                {
                                    RectTransform transform2 = obj2.gameObject.transform as RectTransform;
                                    transform2.anchoredPosition = new Vector2(transform.anchoredPosition.x, transform2.anchoredPosition.y);
                                }
                                if (obj3 != null)
                                {
                                    RectTransform transform3 = obj3.gameObject.transform as RectTransform;
                                    transform3.anchoredPosition = new Vector2((transform.rect.width - (transform3.rect.width * 0.5f)) + 43f, transform3.anchoredPosition.y);
                                }
                            }
                        }
                        else
                        {
                            component.gameObject.CustomSetActive(false);
                        }
                        this.curMapType = EMapType.Mini;
                        bool flag = false;
                        bool flag2 = false;
                        if (levelContext.LevelType == RES_LEVEL_TYPE.RES_LEVEL_TYPE_GUIDE)
                        {
                            switch (levelContext.iLevelID)
                            {
                                case 2:
                                    flag = true;
                                    flag2 = false;
                                    break;

                                case 3:
                                case 6:
                                case 7:
                                    flag = true;
                                    flag2 = true;
                                    break;
                            }
                        }
                        else if ((pvp_level_cfg != null) && ((pvp_level_cfg.bMaxAcntNum == 6) || (pvp_level_cfg.bMaxAcntNum == 10)))
                        {
                            flag = true;
                            flag2 = pvp_level_cfg.bMaxAcntNum == 10;
                        }
                        if (flag)
                        {
                            GameObject node = Utility.FindChild(this.m_FormScript.gameObject, "panelTopLeft/MiniMap/Container_MiniMapPointer_Dragon");
                            DebugHelper.Assert(node != null, "---MinimapSys dragonicons == null, check out...");
                            if (node != null)
                            {
                                this.m_dragonIcon = new DragonIcon();
                                this.m_dragonIcon.Init(node, Utility.FindChild(this.m_FormScript.gameObject, "panelTopLeft/BigMap/Container_BigMapPointer_Dragon"), flag2);
                            }
                        }
                    }
                }
            }
        }

        private RectTransform initMap(enBattleFormWidget mapIndex, SLevelContext levelContext, bool bMinimap, out float preWidth)
        {
            preWidth = 0f;
            GameObject widget = this.m_FormScript.GetWidget((int) mapIndex);
            DebugHelper.Assert(widget != null, "initMap GetWidget is null");
            if (widget == null)
            {
                return null;
            }
            Image component = widget.GetComponent<Image>();
            DebugHelper.Assert(component != null, "initMap map.GetComponent<Image>() is null");
            if (component == null)
            {
                return null;
            }
            RectTransform transform = widget.transform as RectTransform;
            preWidth = transform.rect.width;
            Vector2 sizeDelta = transform.sizeDelta;
            widget.CustomSetActive(bMinimap);
            string prefabPath = CUIUtility.s_Sprite_Dynamic_Map_Dir + (!bMinimap ? levelContext.bigMapPath : levelContext.miniMapPath);
            component.SetSprite(prefabPath, this.m_FormScript, true, false, false);
            component.SetNativeSize();
            if (bMinimap)
            {
                this.initWorldUITransformFactor(sizeDelta, levelContext, bMinimap, out Singleton<CBattleSystem>.instance.world_UI_Factor_Small, out Singleton<CBattleSystem>.instance.UI_world_Factor_Small);
                return transform;
            }
            this.initWorldUITransformFactor(sizeDelta, levelContext, bMinimap, out Singleton<CBattleSystem>.instance.world_UI_Factor_Big, out Singleton<CBattleSystem>.instance.UI_world_Factor_Big);
            return transform;
        }

        private void initWorldUITransformFactor(Vector2 mapImgSize, SLevelContext levelContext, bool bMiniMap, out Vector2 world_UI_Factor, out Vector2 UI_world_Factor)
        {
            int num = !bMiniMap ? levelContext.bigMapWidth : levelContext.mapWidth;
            int num2 = !bMiniMap ? levelContext.bigMapHeight : levelContext.mapHeight;
            float x = mapImgSize.x / ((float) num);
            float y = mapImgSize.y / ((float) num2);
            world_UI_Factor = new Vector2(x, y);
            float num5 = ((float) num) / mapImgSize.x;
            float num6 = ((float) num2) / mapImgSize.y;
            UI_world_Factor = new Vector2(num5, num6);
            if (levelContext.bCameraFlip)
            {
                world_UI_Factor = new Vector2(-x, -y);
                UI_world_Factor = new Vector2(-num5, -num6);
            }
        }

        private void OnBigMap_Click_3_long(CUIEvent uievent)
        {
            this.send_signal(uievent, this.bigMap_obj, 0);
        }

        private void OnBigMap_Click_5_Dalong(CUIEvent uievent)
        {
            this.send_signal(uievent, this.bigMap_obj, 0);
        }

        private void OnBigMap_Click_5_Xiaolong(CUIEvent uievent)
        {
            this.send_signal(uievent, this.bigMap_obj, 0);
        }

        private void OnBigMap_Click_Hero(CUIEvent uievent)
        {
            this.send_signal(uievent, this.bigMap_obj, 0);
        }

        private void OnBigMap_Click_Map(CUIEvent uievent)
        {
            this.send_signal(uievent, this.bigMap_obj, 1);
        }

        private void OnBigMap_Click_Organ(CUIEvent uievent)
        {
            this.send_signal(uievent, this.bigMap_obj, 0);
        }

        private void OnBigMap_Open_BigMap(CUIEvent uievent)
        {
            this.Switch(EMapType.Big);
            this.RefreshMapPointers();
        }

        private void OnBuildingAttacked(ref DefaultGameEventParam evtParam)
        {
            if (evtParam.src != 0)
            {
                ActorRoot handle = evtParam.src.handle;
                if ((handle != null) && HudUT.IsTower(handle))
                {
                    HudComponent3D hudControl = handle.HudControl;
                    if (hudControl != null)
                    {
                        GameObject target = hudControl.GetCurrent_MapObj();
                        TowerHitMgr towerHitMgr = Singleton<CBattleSystem>.instance.GetTowerHitMgr();
                        if ((target != null) && (towerHitMgr != null))
                        {
                            towerHitMgr.TryActive(handle.ObjID, target);
                        }
                        Image image = hudControl.GetBigTower_Img();
                        Image image2 = hudControl.GetSmallTower_Img();
                        if (((image != null) && (image2 != null)) && (handle.ValueComponent != null))
                        {
                            float single = handle.ValueComponent.GetHpRate().single;
                            image.fillAmount = single;
                            image2.fillAmount = single;
                        }
                    }
                }
            }
        }

        private void RefreshMapPointers()
        {
            List<PoolObjHandle<ActorRoot>> heroActors = Singleton<GameObjMgr>.GetInstance().HeroActors;
            for (int i = 0; i < heroActors.Count; i++)
            {
                PoolObjHandle<ActorRoot> handle = heroActors[i];
                if (((handle != 0) && (handle.handle != null)) && (handle.handle.HudControl != null))
                {
                    handle.handle.HudControl.RefreshMapPointerBig();
                }
            }
        }

        private void regEvent()
        {
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BigMap_Open_BigMap, new CUIEventManager.OnUIEventHandler(this.OnBigMap_Open_BigMap));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BigMap_Click_5_Dalong, new CUIEventManager.OnUIEventHandler(this.OnBigMap_Click_5_Dalong));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BigMap_Click_5_Xiaolong, new CUIEventManager.OnUIEventHandler(this.OnBigMap_Click_5_Xiaolong));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BigMap_Click_3_long, new CUIEventManager.OnUIEventHandler(this.OnBigMap_Click_3_long));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BigMap_Click_Organ, new CUIEventManager.OnUIEventHandler(this.OnBigMap_Click_Organ));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BigMap_Click_Hero, new CUIEventManager.OnUIEventHandler(this.OnBigMap_Click_Hero));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.BigMap_Click_Map, new CUIEventManager.OnUIEventHandler(this.OnBigMap_Click_Map));
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorBeAttack, new RefAction<DefaultGameEventParam>(this.OnBuildingAttacked));
        }

        private void send_signal(CUIEvent uiEvent, GameObject img, int signal_id = 0)
        {
            if ((uiEvent != null) && (img != null))
            {
                byte tag = (byte) uiEvent.m_eventParams.tag;
                byte type = (byte) uiEvent.m_eventParams.tag2;
                uint tagUInt = 0;
                uint targetHeroID = 0;
                if (signal_id == 0)
                {
                    signal_id = uiEvent.m_eventParams.tag3;
                }
                if (type == 3)
                {
                    tagUInt = uiEvent.m_eventParams.tagUInt;
                    targetHeroID = uiEvent.m_eventParams.heroId;
                }
                Singleton<CBattleSystem>.instance.GetMinimapSys().Switch(EMapType.Mini);
                Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
                if (hostPlayer != null)
                {
                    ActorRoot root = ((hostPlayer == null) || (hostPlayer.Captain == 0)) ? null : hostPlayer.Captain.handle;
                    if (root != null)
                    {
                        VInt num5;
                        Vector2 vector = CUIUtility.WorldToScreenPoint(uiEvent.m_srcFormScript.GetCamera(), img.transform.position);
                        Vector3 zero = Vector3.zero;
                        zero.x = (uiEvent.m_pointerEventData.position.x - vector.x) * Singleton<CBattleSystem>.GetInstance().UI_world_Factor_Big.x;
                        zero.y = (root == null) ? 0.15f : ((Vector3) root.location).y;
                        zero.z = (uiEvent.m_pointerEventData.position.y - vector.y) * Singleton<CBattleSystem>.GetInstance().UI_world_Factor_Big.y;
                        PathfindingUtility.GetGroundY((VInt3) zero, out num5);
                        zero.y = num5.scalar;
                        SignalPanel signalPanel = Singleton<CBattleSystem>.instance.GetSignalPanel();
                        if (signalPanel != null)
                        {
                            signalPanel.SendFrameCommand(signal_id, (int) zero.x, (int) zero.y, (int) zero.z, tag, type, tagUInt, targetHeroID);
                        }
                    }
                }
            }
        }

        public static Image SetTower_Image(bool bAlie, int value, GameObject mapPointer, CUIFormScript formScript)
        {
            if ((mapPointer == null) || (formScript == null))
            {
                return null;
            }
            Image component = mapPointer.GetComponent<Image>();
            Image image = mapPointer.transform.Find("front").GetComponent<Image>();
            if ((component == null) || (image == null))
            {
                return null;
            }
            if (value == 2)
            {
                component.SetSprite(!bAlie ? enemy_base : self_base, formScript, true, false, false);
                image.SetSprite(!bAlie ? enemy_base_outline : self_base_outline, formScript, true, false, false);
                return component;
            }
            if ((value == 1) || (value == 4))
            {
                component.SetSprite(!bAlie ? enemy_tower : self_tower, formScript, true, false, false);
                image.SetSprite(!bAlie ? enemy_tower_outline : self_tower_outline, formScript, true, false, false);
            }
            return component;
        }

        public void Switch(EMapType type)
        {
            if (type != EMapType.None)
            {
                this.curMapType = type;
                GameObject widget = this.m_FormScript.GetWidget(0x2d);
                if (this.curMapType == EMapType.Mini)
                {
                    if (this.miniMap_obj != null)
                    {
                        this.miniMap_obj.CustomSetActive(true);
                    }
                    if (this.bigMap_obj != null)
                    {
                        this.bigMap_obj.CustomSetActive(false);
                    }
                    if (widget != null)
                    {
                        widget.CustomSetActive(true);
                    }
                }
                else
                {
                    if (this.miniMap_obj != null)
                    {
                        this.miniMap_obj.CustomSetActive(false);
                    }
                    if (this.bigMap_obj != null)
                    {
                        this.bigMap_obj.CustomSetActive(true);
                    }
                    if (widget != null)
                    {
                        widget.CustomSetActive(false);
                    }
                }
            }
        }

        private void unRegEvent()
        {
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.BigMap_Open_BigMap, new CUIEventManager.OnUIEventHandler(this.OnBigMap_Open_BigMap));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.BigMap_Click_5_Dalong, new CUIEventManager.OnUIEventHandler(this.OnBigMap_Click_5_Dalong));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.BigMap_Click_5_Xiaolong, new CUIEventManager.OnUIEventHandler(this.OnBigMap_Click_5_Xiaolong));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.BigMap_Click_3_long, new CUIEventManager.OnUIEventHandler(this.OnBigMap_Click_3_long));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.BigMap_Click_Organ, new CUIEventManager.OnUIEventHandler(this.OnBigMap_Click_Organ));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.BigMap_Click_Hero, new CUIEventManager.OnUIEventHandler(this.OnBigMap_Click_Hero));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.BigMap_Click_Map, new CUIEventManager.OnUIEventHandler(this.OnBigMap_Click_Map));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorBeAttack, new RefAction<DefaultGameEventParam>(this.OnBuildingAttacked));
        }

        public enum ElementType
        {
            None,
            Tower,
            Base,
            Hero,
            Dragon_5_big,
            Dragon_5_small,
            Dragon_3
        }

        public enum EMapType
        {
            None,
            Mini,
            Big
        }
    }
}

