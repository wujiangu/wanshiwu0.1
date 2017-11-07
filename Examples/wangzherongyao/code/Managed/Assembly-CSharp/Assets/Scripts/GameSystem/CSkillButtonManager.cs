namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.GameLogic.GameKernal;
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    public class CSkillButtonManager
    {
        private SkillButton[] _skillButtons = new SkillButton[s_maxButtonCount];
        private const float c_directionalSkillIndicatorRespondMinRadius = 30f;
        private const float c_skillIndicatorMoveRadius = 30f;
        private const float c_skillIndicatorRadius = 110f;
        private const float c_skillIndicatorRespondMinRadius = 15f;
        private Vector2 m_currentSkillDownScreenPosition = Vector2.zero;
        private bool m_currentSkillIndicatorEnabled;
        private bool m_currentSkillIndicatorInCancelArea;
        private bool m_currentSkillIndicatorJoystickEnabled;
        private bool m_currentSkillIndicatorResponed;
        private Vector2 m_currentSkillIndicatorScreenPosition = Vector2.zero;
        private SkillSlotType m_currentSkillSlotType = SkillSlotType.SLOT_SKILL_COUNT;
        private bool m_currentSkillTipsResponed;
        private enSkillIndicatorMode m_skillIndicatorMode = enSkillIndicatorMode.FixedPosition;
        private static byte s_maxButtonCount = 7;
        private static enBattleFormWidget[] s_skillButtons = new enBattleFormWidget[] { enBattleFormWidget.Button_Attack, enBattleFormWidget.Button_Skill_1, enBattleFormWidget.Button_Skill_2, enBattleFormWidget.Button_Skill_3, enBattleFormWidget.Button_Recover, enBattleFormWidget.Button_Talent, enBattleFormWidget.Button_Skill_6, enBattleFormWidget.Button_ComAtkSelectSoldier };
        private static enBattleFormWidget[] s_skillCDTexts = new enBattleFormWidget[] { enBattleFormWidget.None, enBattleFormWidget.Text_Skill_1_CD, enBattleFormWidget.Text_Skill_2_CD, enBattleFormWidget.Text_Skill_3_CD, enBattleFormWidget.Text_Skill_4_CD, enBattleFormWidget.Text_Skill_5_CD, enBattleFormWidget.Text_Skill_6_CD, enBattleFormWidget.None };
        private static Color s_skillCursorBGColorBlue = new Color(0.1686275f, 0.7607843f, 1f, 1f);
        private static Color s_skillCursorBGColorRed = new Color(0.972549f, 0.1843137f, 0.1843137f, 1f);

        public CSkillButtonManager()
        {
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorDead, new RefAction<DefaultGameEventParam>(this.onActorDead));
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorRevive, new RefAction<DefaultGameEventParam>(this.onActorRevive));
            Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_CaptainSwitch, new RefAction<DefaultGameEventParam>(this.OnCaptainSwitched));
        }

        public void CancelLimitButton(SkillSlotType skillSlotType)
        {
            if (skillSlotType != SkillSlotType.SLOT_SKILL_0)
            {
                Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
                SkillButton button = this.GetButton(skillSlotType);
                DebugHelper.Assert(button != null);
                if (button != null)
                {
                    if (button.m_button != null)
                    {
                        SkillSlot slot;
                        CUIEventScript component = button.m_button.GetComponent<CUIEventScript>();
                        if (((component != null) && (hostPlayer != null)) && ((hostPlayer.Captain != 0) && hostPlayer.Captain.handle.SkillControl.TryGetSkillSlot(skillSlotType, out slot)))
                        {
                            if (slot.EnableButtonFlag)
                            {
                                component.enabled = true;
                            }
                            else
                            {
                                component.enabled = false;
                            }
                        }
                    }
                    GameObject gameObject = button.GetAnimationPresent().transform.Find("disableFrame").gameObject;
                    DebugHelper.Assert(gameObject != null);
                    if (gameObject != null)
                    {
                        gameObject.CustomSetActive(false);
                    }
                    button.bLimitedFlag = false;
                    if (!button.bDisableFlag)
                    {
                        CUICommonSystem.PlayAnimator(button.GetAnimationPresent(), enSkillButtonAnimationName.normal.ToString());
                    }
                }
            }
        }

        public void CancelUseSkillSlot(SkillSlotType skillSlotType)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            if ((hostPlayer != null) && (hostPlayer.Captain != 0))
            {
                hostPlayer.Captain.handle.SkillControl.CancelUseSkillSlot(skillSlotType);
            }
        }

        public void ChangeSkill(SkillSlotType skillSlotType, ref ChangeSkillEventParam skillParam)
        {
            this.SetComboEffect(skillSlotType, skillParam.changeTime, skillParam.changeTime);
            if (skillSlotType != SkillSlotType.SLOT_SKILL_0)
            {
            }
        }

        private void ChangeSkillCursorBGSprite(CUIFormScript battleFormScript, bool isSkillCursorInCancelArea)
        {
            if (battleFormScript != null)
            {
                GameObject widget = battleFormScript.GetWidget(0x19);
                Image image = (widget == null) ? null : widget.GetComponent<Image>();
                if (image != null)
                {
                    image.color = GetCursorBGColor(isSkillCursorInCancelArea);
                }
            }
        }

        public void Clear()
        {
            if (this._skillButtons != null)
            {
                for (int i = 0; i < this._skillButtons.Length; i++)
                {
                    if (this._skillButtons[i] != null)
                    {
                        this._skillButtons[i].Clear();
                    }
                }
            }
        }

        public void DisableSkillCursor(CUIFormScript battleFormScript)
        {
            this.m_currentSkillIndicatorEnabled = false;
            this.m_currentSkillIndicatorJoystickEnabled = false;
            this.m_currentSkillIndicatorResponed = false;
            this.m_currentSkillTipsResponed = false;
            this.m_currentSkillIndicatorInCancelArea = false;
            DebugHelper.Assert(battleFormScript != null);
            if (battleFormScript != null)
            {
                GameObject widget = battleFormScript.GetWidget(0x19);
                DebugHelper.Assert(widget != null);
                if (widget != null)
                {
                    widget.CustomSetActive(false);
                    (widget.transform.FindChild("Cursor").gameObject.transform as RectTransform).anchoredPosition = Vector2.zero;
                }
                GameObject obj4 = battleFormScript.GetWidget(0x1f);
                if (obj4 != null)
                {
                    obj4.CustomSetActive(false);
                }
            }
        }

        public void DragSkillButton(CUIFormScript formScript, SkillSlotType skillSlotType, Vector2 dragScreenPosition)
        {
            if ((this.m_currentSkillSlotType == skillSlotType) && this.m_currentSkillIndicatorEnabled)
            {
                bool currentSkillIndicatorInCancelArea = this.m_currentSkillIndicatorInCancelArea;
                Vector2 cursor = this.MoveSkillCursor(formScript, skillSlotType, ref dragScreenPosition, out this.m_currentSkillIndicatorInCancelArea);
                if (currentSkillIndicatorInCancelArea != this.m_currentSkillIndicatorInCancelArea)
                {
                    this.ChangeSkillCursorBGSprite(formScript, this.m_currentSkillIndicatorInCancelArea);
                }
                this.MoveSkillCursorInScene(skillSlotType, ref cursor, this.m_currentSkillIndicatorInCancelArea);
            }
        }

        public void EnableSkillCursor(CUIFormScript battleFormScript, ref Vector2 screenPosition, bool enableSkillCursorJoystick, SkillSlotType skillSlotType, bool isSkillCanBeCancled)
        {
            this.m_currentSkillIndicatorEnabled = true;
            this.m_currentSkillIndicatorResponed = false;
            this.m_currentSkillTipsResponed = false;
            if (enableSkillCursorJoystick)
            {
                this.m_currentSkillIndicatorJoystickEnabled = true;
                if (battleFormScript != null)
                {
                    GameObject widget = battleFormScript.GetWidget(0x19);
                    if (widget != null)
                    {
                        widget.CustomSetActive(true);
                        Vector3 skillIndicatorFixedPosition = this.GetButton(skillSlotType).m_skillIndicatorFixedPosition;
                        if ((this.m_skillIndicatorMode == enSkillIndicatorMode.General) || (skillIndicatorFixedPosition == Vector3.zero))
                        {
                            widget.transform.position = CUIUtility.ScreenToWorldPoint(battleFormScript.GetCamera(), screenPosition, widget.transform.position.z);
                            this.m_currentSkillIndicatorScreenPosition = screenPosition;
                        }
                        else
                        {
                            widget.transform.position = skillIndicatorFixedPosition;
                            this.m_currentSkillIndicatorScreenPosition = CUIUtility.WorldToScreenPoint(battleFormScript.GetCamera(), skillIndicatorFixedPosition);
                        }
                    }
                }
                this.ChangeSkillCursorBGSprite(battleFormScript, this.m_currentSkillIndicatorInCancelArea);
            }
            if (battleFormScript != null)
            {
                GameObject obj3 = battleFormScript.GetWidget(0x1f);
                if (obj3 != null)
                {
                    obj3.CustomSetActive(isSkillCanBeCancled);
                }
            }
        }

        ~CSkillButtonManager()
        {
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorDead, new RefAction<DefaultGameEventParam>(this.onActorDead));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorRevive, new RefAction<DefaultGameEventParam>(this.onActorRevive));
            Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_CaptainSwitch, new RefAction<DefaultGameEventParam>(this.OnCaptainSwitched));
        }

        public SkillButton GetButton(SkillSlotType skillSlotType)
        {
            int index = (int) skillSlotType;
            if ((index < 0) || (index >= this._skillButtons.Length))
            {
                return null;
            }
            SkillButton button = this._skillButtons[index];
            if (button == null)
            {
                CUIFormScript formScript = Singleton<CBattleSystem>.GetInstance().m_FormScript;
                button = new SkillButton {
                    m_button = (formScript != null) ? formScript.GetWidget((int) s_skillButtons[index]) : null,
                    m_cdText = (formScript != null) ? formScript.GetWidget((int) s_skillCDTexts[index]) : null
                };
                this._skillButtons[index] = button;
                if (button.m_button != null)
                {
                    Transform transform = button.m_button.transform.FindChild("IndicatorPosition");
                    if (transform != null)
                    {
                        button.m_skillIndicatorFixedPosition = transform.position;
                    }
                }
            }
            return button;
        }

        public SkillSlotType GetCurSkillSlotType()
        {
            return this.m_currentSkillSlotType;
        }

        public static Color GetCursorBGColor(bool cancel)
        {
            return (!cancel ? s_skillCursorBGColorBlue : s_skillCursorBGColorRed);
        }

        public void Initialise(PoolObjHandle<ActorRoot> actor)
        {
            if (((actor != 0) && (actor.handle.SkillControl != null)) && (actor.handle.SkillControl.SkillSlotArray != null))
            {
                SkillSlot[] skillSlotArray = actor.handle.SkillControl.SkillSlotArray;
                this._skillButtons = new SkillButton[s_maxButtonCount];
                for (int i = 0; i < s_maxButtonCount; i++)
                {
                    SkillSlotType skillSlotType = (SkillSlotType) i;
                    SkillButton button = this.GetButton(skillSlotType);
                    SkillSlot slot = skillSlotArray[i];
                    DebugHelper.Assert(button != null);
                    if (button == null)
                    {
                        continue;
                    }
                    button.bDisableFlag = false;
                    button.bLimitedFlag = false;
                    if (slot != null)
                    {
                        if (!slot.EnableButtonFlag)
                        {
                            button.bDisableFlag = true;
                        }
                        else
                        {
                            button.bDisableFlag = false;
                        }
                    }
                    if (actor.handle.SkillControl.DisableSkill[i] == 1)
                    {
                        button.bLimitedFlag = true;
                    }
                    else
                    {
                        button.bLimitedFlag = false;
                    }
                    if (skillSlotType == SkillSlotType.SLOT_SKILL_6)
                    {
                        SLevelContext curLvelContext = Singleton<BattleLogic>.GetInstance().GetCurLvelContext();
                        if (curLvelContext.GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_GUIDE)
                        {
                            if (((curLvelContext.iLevelID == CBattleGuideManager.GuideLevelID5v5) && (slot != null)) && ((slot.SkillObj != null) && (slot.SkillObj.cfgData != null)))
                            {
                                goto Label_01A7;
                            }
                            button.m_button.CustomSetActive(false);
                            continue;
                        }
                        ResDT_LevelCommonInfo info = CLevelCfgLogicManager.FindLevelConfigMultiGame(curLvelContext.iLevelID);
                        if (((info == null) || (info.bMaxAcntNum != 10)) || (((slot == null) || (slot.SkillObj == null)) || (slot.SkillObj.cfgData == null)))
                        {
                            button.m_button.CustomSetActive(false);
                            continue;
                        }
                    }
                Label_01A7:
                    if (((Singleton<BattleLogic>.instance.GetCurLvelContext().GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_BURNING) || (Singleton<BattleLogic>.instance.GetCurLvelContext().GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_ARENA)) || (Singleton<BattleLogic>.instance.GetCurLvelContext().GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_ADVENTURE))
                    {
                        if (((Singleton<BattleLogic>.instance.GetCurLvelContext().GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_BURNING) || (Singleton<BattleLogic>.instance.GetCurLvelContext().GameType == COM_GAME_TYPE.COM_SINGLE_GAME_OF_ARENA)) && ((skillSlotType == SkillSlotType.SLOT_SKILL_4) || (skillSlotType == SkillSlotType.SLOT_SKILL_5)))
                        {
                            if ((button.m_button != null) && (button.m_cdText != null))
                            {
                                button.m_button.CustomSetActive(false);
                                button.m_cdText.CustomSetActive(false);
                            }
                            continue;
                        }
                        if ((skillSlotType >= SkillSlotType.SLOT_SKILL_1) && (skillSlotType <= SkillSlotType.SLOT_SKILL_3))
                        {
                            if (button.m_button != null)
                            {
                                GameObject skillLvlFrameImg = button.GetSkillLvlFrameImg(true);
                                if (skillLvlFrameImg != null)
                                {
                                    skillLvlFrameImg.CustomSetActive(false);
                                }
                                GameObject obj3 = button.GetSkillLvlFrameImg(false);
                                if (obj3 != null)
                                {
                                    obj3.CustomSetActive(false);
                                }
                                GameObject skillFrameImg = button.GetSkillFrameImg();
                                if (skillFrameImg != null)
                                {
                                    skillFrameImg.CustomSetActive(true);
                                }
                            }
                            if (slot != null)
                            {
                                int dwConfValue = 0;
                                switch (skillSlotType)
                                {
                                    case SkillSlotType.SLOT_SKILL_1:
                                    case SkillSlotType.SLOT_SKILL_2:
                                        dwConfValue = (int) GameDataMgr.globalInfoDatabin.GetDataByKey(0x8e).dwConfValue;
                                        break;

                                    default:
                                        if (skillSlotType == SkillSlotType.SLOT_SKILL_3)
                                        {
                                            dwConfValue = (int) GameDataMgr.globalInfoDatabin.GetDataByKey(0x8f).dwConfValue;
                                        }
                                        break;
                                }
                                slot.SetSkillLevel(dwConfValue);
                            }
                        }
                    }
                    if (button.m_button != null)
                    {
                        CUIEventScript component = button.m_button.GetComponent<CUIEventScript>();
                        CUIEventScript script2 = button.GetDisableButton().GetComponent<CUIEventScript>();
                        if (slot != null)
                        {
                            component.enabled = true;
                            script2.m_onClickEventID = enUIEventID.Battle_Disable_Alert;
                            script2.enabled = true;
                            switch (skillSlotType)
                            {
                                case SkillSlotType.SLOT_SKILL_1:
                                case SkillSlotType.SLOT_SKILL_2:
                                case SkillSlotType.SLOT_SKILL_3:
                                    if (slot.EnableButtonFlag)
                                    {
                                        component.enabled = true;
                                    }
                                    else
                                    {
                                        component.enabled = false;
                                    }
                                    break;
                            }
                            if (button.GetDisableButton() != null)
                            {
                                if (slot.GetSkillLevel() > 0)
                                {
                                    this.SetEnableButton(skillSlotType);
                                    script2.m_onDownEventID = enUIEventID.None;
                                    script2.m_onUpEventID = enUIEventID.None;
                                    if (skillSlotType != SkillSlotType.SLOT_SKILL_0)
                                    {
                                        script2.m_onHoldEventID = enUIEventID.Battle_OnSkillButtonHold;
                                        script2.m_onHoldEndEventID = enUIEventID.Battle_OnSkillButtonHoldEnd;
                                    }
                                    if (((actor.handle.ActorControl != null) && actor.handle.ActorControl.IsDeadState) || ((slot.SlotType != SkillSlotType.SLOT_SKILL_0) && !slot.IsCDReady))
                                    {
                                        this.SetDisableButton(skillSlotType);
                                    }
                                }
                                else
                                {
                                    this.SetDisableButton(skillSlotType);
                                }
                            }
                            if (button.m_button != null)
                            {
                                button.m_button.CustomSetActive(true);
                            }
                            if (button.m_cdText != null)
                            {
                                button.m_cdText.CustomSetActive(true);
                            }
                            if (skillSlotType != SkillSlotType.SLOT_SKILL_0)
                            {
                                button.m_button.transform.Find("Present/SkillImg").GetComponent<Image>().SetSprite(CUIUtility.s_Sprite_Dynamic_Skill_Dir + slot.SkillObj.IconName, Singleton<CBattleSystem>.GetInstance().m_FormScript, true, false, false);
                                if (((skillSlotType == SkillSlotType.SLOT_SKILL_4) || (skillSlotType == SkillSlotType.SLOT_SKILL_5)) || (skillSlotType == SkillSlotType.SLOT_SKILL_6))
                                {
                                    Transform transform = button.m_button.transform.Find("lblName");
                                    if (transform != null)
                                    {
                                        if (slot.SkillObj.cfgData != null)
                                        {
                                            transform.GetComponent<Text>().text = slot.SkillObj.cfgData.szSkillName;
                                        }
                                        transform.gameObject.CustomSetActive(true);
                                    }
                                }
                                if (actor.handle.SkillControl.IsDisableSkillSlot(skillSlotType))
                                {
                                    this.SetLimitButton(skillSlotType);
                                }
                                else if ((slot.GetSkillLevel() > 0) && slot.IsEnergyEnough)
                                {
                                    this.CancelLimitButton(skillSlotType);
                                }
                                if (slot.GetSkillLevel() > 0)
                                {
                                    this.UpdateButtonCD(skillSlotType, (int) slot.CurSkillCD);
                                }
                                else if (button.m_cdText != null)
                                {
                                    button.m_cdText.CustomSetActive(false);
                                }
                            }
                            component.m_onDownEventParams.m_skillSlotType = skillSlotType;
                            component.m_onUpEventParams.m_skillSlotType = skillSlotType;
                            component.m_onHoldEventParams.m_skillSlotType = skillSlotType;
                            component.m_onHoldEndEventParams.m_skillSlotType = skillSlotType;
                            component.m_onDragStartEventParams.m_skillSlotType = skillSlotType;
                            component.m_onDragEventParams.m_skillSlotType = skillSlotType;
                            script2.m_onClickEventParams.m_skillSlotType = skillSlotType;
                            this.SetComboEffect(skillSlotType, slot.skillChangeEvent.GetEffectTIme(), slot.skillChangeEvent.GetEffectMaxTime());
                        }
                        else
                        {
                            component.enabled = false;
                            script2.enabled = false;
                            if (button.GetDisableButton() != null)
                            {
                                this.SetDisableButton(skillSlotType);
                            }
                            if (button.m_cdText != null)
                            {
                                button.m_cdText.CustomSetActive(false);
                            }
                        }
                    }
                }
                this.SetCommonAtkBtnState(GameSettings.TheCommonAttackType);
                Singleton<CBattleSystem>.instance.SetCommonAttackTargetEvent(GameSettings.TheCommonAttackType);
            }
        }

        private bool IsSkillCursorInCanceledArea(CUIFormScript battleFormScript, ref Vector2 screenPosition)
        {
            DebugHelper.Assert(battleFormScript != null, "battleFormScript!=null");
            if (battleFormScript != null)
            {
                GameObject widget = battleFormScript.GetWidget(0x1f);
                DebugHelper.Assert((widget != null) && (widget.transform is RectTransform), "skillCancel != null && skillCancel.transform is RectTransform");
                if (((widget != null) && widget.activeInHierarchy) && (widget.transform is RectTransform))
                {
                    Vector2 vector = CUIUtility.WorldToScreenPoint(battleFormScript.GetCamera(), widget.transform.position);
                    float width = battleFormScript.ChangeFormValueToScreen((widget.transform as RectTransform).sizeDelta.x);
                    float height = battleFormScript.ChangeFormValueToScreen((widget.transform as RectTransform).sizeDelta.y);
                    Rect rect = new Rect(vector.x - width, vector.y, width, height);
                    return rect.Contains(screenPosition);
                }
            }
            return false;
        }

        public bool IsUseSkillCursorJoystick(SkillSlotType skillSlotType)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            return (((hostPlayer != null) && (hostPlayer.Captain != 0)) && hostPlayer.Captain.handle.SkillControl.IsUseSkillJoystick(skillSlotType));
        }

        public Vector2 MoveSkillCursor(CUIFormScript battleFormScript, SkillSlotType skillSlotType, ref Vector2 screenPosition, out bool skillCanceled)
        {
            skillCanceled = this.IsSkillCursorInCanceledArea(battleFormScript, ref screenPosition);
            if (!this.m_currentSkillIndicatorJoystickEnabled)
            {
                return Vector2.zero;
            }
            if (!this.m_currentSkillIndicatorResponed)
            {
                Vector2 vector = screenPosition - this.m_currentSkillDownScreenPosition;
                if (battleFormScript.ChangeScreenValueToForm(vector.magnitude) > 15f)
                {
                    this.m_currentSkillIndicatorResponed = true;
                }
            }
            if (!this.m_currentSkillTipsResponed)
            {
                Vector2 vector2 = screenPosition - this.m_currentSkillDownScreenPosition;
                if (battleFormScript.ChangeScreenValueToForm(vector2.magnitude) > 30f)
                {
                    this.m_currentSkillTipsResponed = true;
                }
            }
            if (!this.m_currentSkillIndicatorResponed)
            {
                return Vector2.zero;
            }
            Vector2 vector3 = screenPosition - this.m_currentSkillIndicatorScreenPosition;
            Vector2 vector4 = vector3;
            float magnitude = vector3.magnitude;
            float num2 = magnitude;
            num2 = battleFormScript.ChangeScreenValueToForm(magnitude);
            vector4.x = battleFormScript.ChangeScreenValueToForm(vector3.x);
            vector4.y = battleFormScript.ChangeScreenValueToForm(vector3.y);
            if (num2 > 110f)
            {
                vector4 = (Vector2) (vector4.normalized * 110f);
            }
            GameObject widget = battleFormScript.GetWidget(0x19);
            if (widget != null)
            {
                Transform transform = widget.transform.FindChild("Cursor");
                if (transform != null)
                {
                    (transform as RectTransform).anchoredPosition = vector4;
                }
            }
            Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
            if (((hostPlayer != null) && (hostPlayer.Captain != 0)) && ((hostPlayer.Captain.handle.SkillControl.SkillSlotArray[(int) skillSlotType].SkillObj.cfgData.dwRangeAppointType == 3) && (num2 < 30f)))
            {
                return Vector2.zero;
            }
            return (Vector2) (vector4 / 110f);
        }

        public void MoveSkillCursorInScene(SkillSlotType skillSlotType, ref Vector2 cursor, bool isSkillCursorInCancelArea)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            if ((hostPlayer != null) && (hostPlayer.Captain != 0))
            {
                hostPlayer.Captain.handle.SkillControl.SelectSkillTarget(skillSlotType, cursor, isSkillCursorInCancelArea);
            }
        }

        private void onActorDead(ref DefaultGameEventParam prm)
        {
            if (Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain == prm.src)
            {
                for (int i = 0; i < 7; i++)
                {
                    this.SetDisableButton((SkillSlotType) i);
                }
            }
        }

        private void onActorRevive(ref DefaultGameEventParam prm)
        {
            if (Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain == prm.src)
            {
                for (int i = 0; i < 7; i++)
                {
                    this.SetEnableButton((SkillSlotType) i);
                }
            }
        }

        public void OnBattleSkillDisableAlert(SkillSlotType skillSlotType)
        {
            SkillSlot slot;
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            if (((hostPlayer != null) && (hostPlayer.Captain != 0)) && hostPlayer.Captain.handle.SkillControl.TryGetSkillSlot(skillSlotType, out slot))
            {
                if (!slot.IsCDReady)
                {
                    slot.SendSkillCooldownEvent();
                }
                else if (!slot.IsEnergyEnough)
                {
                    slot.SendSkillShortageEvent();
                }
            }
        }

        private void OnCaptainSwitched(ref DefaultGameEventParam prm)
        {
            Singleton<CBattleSystem>.GetInstance().ResetSkillButtonManager(prm.src);
        }

        public static void Preload(ref ActorPreloadTab result)
        {
        }

        public void ReadyUseSkillSlot(SkillSlotType skillSlotType)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            if ((hostPlayer != null) && (hostPlayer.Captain != 0))
            {
                hostPlayer.Captain.handle.SkillControl.ReadyUseSkillSlot(skillSlotType);
            }
        }

        public void RequestUseSkillSlot(SkillSlotType skillSlotType)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            if ((hostPlayer != null) && (hostPlayer.Captain != 0))
            {
                hostPlayer.Captain.handle.SkillControl.RequestUseSkillSlot(skillSlotType);
            }
        }

        public void ResetSkillIndicatorFixedPosition(SkillButton skillButton)
        {
            if ((skillButton != null) && (skillButton.m_button != null))
            {
                Transform transform = skillButton.m_button.transform.FindChild("IndicatorPosition");
                if (transform != null)
                {
                    skillButton.m_skillIndicatorFixedPosition = transform.position;
                }
            }
        }

        public void SendUseCommonAttack(sbyte Start, uint ObjID)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            if (((hostPlayer != null) && (hostPlayer.Captain != 0)) && !hostPlayer.Captain.handle.ActorControl.IsDeadState)
            {
                FrameCommand<UseCommonAttackCommand> command = FrameCommandFactory.CreateFrameCommand<UseCommonAttackCommand>();
                command.cmdData.Start = Start;
                command.cmdData.ObjID = ObjID;
                command.Send();
            }
        }

        public void SetButtonCDOver(SkillSlotType skillSlotType, bool isPlayMusic = true)
        {
            if ((skillSlotType != SkillSlotType.SLOT_SKILL_0) && this.SetEnableButton(skillSlotType))
            {
                SkillButton button = this.GetButton(skillSlotType);
                GameObject target = (button == null) ? null : button.GetAnimationCD();
                CUICommonSystem.PlayAnimator(target, enSkillButtonAnimationName.CD_End.ToString());
                if (isPlayMusic)
                {
                    Singleton<CSoundManager>.GetInstance().PlayBattleSound("UI_prompt_jineng", null);
                }
            }
        }

        public void SetButtonCDStart(SkillSlotType skillSlotType)
        {
            if (skillSlotType != SkillSlotType.SLOT_SKILL_0)
            {
                this.SetDisableButton(skillSlotType);
                SkillButton button = this.GetButton(skillSlotType);
                GameObject target = (button == null) ? null : button.GetAnimationCD();
                CUICommonSystem.PlayAnimator(target, enSkillButtonAnimationName.CD_Star.ToString());
            }
        }

        public void SetButtonHighLight(SkillSlotType skillSlotType, bool highLight)
        {
            SkillButton button = this.GetButton(skillSlotType);
            if ((button != null) && (button.m_button != null))
            {
                this.SetButtonHighLight(button.m_button, highLight);
            }
        }

        public void SetButtonHighLight(GameObject button, bool highLight)
        {
            Transform transform = button.transform.FindChild("Present/highlighter");
            if (transform != null)
            {
                transform.gameObject.CustomSetActive(highLight);
            }
        }

        private void SetComboEffect(SkillSlotType skillSlotType, int leftTime, int totalTime)
        {
            SkillButton button = this.GetButton(skillSlotType);
            if ((button != null) && (null != button.m_button))
            {
                button.effectTimeTotal = totalTime;
                button.effectTimeLeft = leftTime;
                GameObject obj2 = Utility.FindChildSafe(button.m_button, "Present/comboCD");
                if (obj2 != null)
                {
                    if ((button.effectTimeLeft > 0) && (button.effectTimeTotal > 0))
                    {
                        obj2.CustomSetActive(true);
                        button.effectTimeImage = obj2.GetComponent<Image>();
                    }
                    else
                    {
                        obj2.CustomSetActive(false);
                        button.effectTimeImage = null;
                    }
                }
            }
        }

        public void SetCommonAtkBtnState(CommonAttactType byAtkType)
        {
            CUIFormScript formScript = Singleton<CBattleSystem>.GetInstance().m_FormScript;
            if (formScript != null)
            {
                GameObject widget = formScript.GetWidget(0x44);
                GameObject obj3 = formScript.GetWidget(0x39);
                if (((widget != null) && (obj3 != null)) && (obj3.GetComponent<CUIEventScript>() != null))
                {
                    if (byAtkType == CommonAttactType.Type1)
                    {
                        widget.CustomSetActive(false);
                        obj3.CustomSetActive(false);
                    }
                    else if (byAtkType == CommonAttactType.Type2)
                    {
                        widget.CustomSetActive(true);
                        obj3.CustomSetActive(true);
                        bool bActive = false;
                        SkillButton button = this.GetButton(SkillSlotType.SLOT_SKILL_0);
                        if (button != null)
                        {
                            GameObject disableButton = button.GetDisableButton();
                            if (disableButton != null)
                            {
                                bActive = disableButton.activeSelf;
                            }
                        }
                        this.SetSelectTargetBtnState(bActive);
                    }
                    Singleton<EventRouter>.GetInstance().BroadCastEvent("CommonAttack_Type_Changed");
                }
            }
        }

        public void SetDisableButton(SkillSlotType skillSlotType)
        {
            CUIFormScript formScript = Singleton<CBattleSystem>.GetInstance().m_FormScript;
            if (formScript != null)
            {
                if (this.m_currentSkillSlotType == skillSlotType)
                {
                    this.SkillButtonUp(formScript, skillSlotType, false);
                }
                SkillButton button = this.GetButton(skillSlotType);
                if (button != null)
                {
                    if (button.m_button != null)
                    {
                        CUIEventScript component = button.m_button.GetComponent<CUIEventScript>();
                        if (component != null)
                        {
                            if (component.ClearInputStatus())
                            {
                                Singleton<CBattleSystem>.GetInstance().HideSkillDescInfo();
                            }
                            component.enabled = false;
                        }
                    }
                    button.bDisableFlag = true;
                    if (skillSlotType != SkillSlotType.SLOT_SKILL_0)
                    {
                        CUICommonSystem.PlayAnimator(button.GetAnimationPresent(), enSkillButtonAnimationName.disable.ToString());
                    }
                    else
                    {
                        GameObject animationPresent = button.GetAnimationPresent();
                        if (animationPresent != null)
                        {
                            Image image = animationPresent.GetComponent<Image>();
                            if (image != null)
                            {
                                image.color = CUIUtility.s_Color_DisableGray;
                            }
                        }
                        GameObject obj4 = button.GetDisableButton();
                        if (obj4 != null)
                        {
                            obj4.CustomSetActive(true);
                        }
                        this.SetSelectTargetBtnState(true);
                    }
                    GameObject disableButton = button.GetDisableButton();
                    if (disableButton != null)
                    {
                        disableButton.CustomSetActive(true);
                    }
                }
            }
        }

        public bool SetEnableButton(SkillSlotType skillSlotType)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            if ((hostPlayer != null) && (hostPlayer.Captain != 0))
            {
                SkillSlot slot;
                if (!hostPlayer.Captain.handle.SkillControl.TryGetSkillSlot(skillSlotType, out slot))
                {
                    return false;
                }
                if (skillSlotType == SkillSlotType.SLOT_SKILL_0)
                {
                    if (hostPlayer.Captain.handle.ActorControl.IsDeadState)
                    {
                        return false;
                    }
                }
                else if (!slot.EnableButtonFlag)
                {
                    return false;
                }
            }
            SkillButton button = this.GetButton(skillSlotType);
            if (button != null)
            {
                if (button.m_button != null)
                {
                    CUIEventScript component = button.m_button.GetComponent<CUIEventScript>();
                    if (component != null)
                    {
                        if (component.ClearInputStatus())
                        {
                            Singleton<CBattleSystem>.GetInstance().HideSkillDescInfo();
                        }
                        component.enabled = true;
                    }
                }
                button.bDisableFlag = false;
                if (!button.bLimitedFlag && (skillSlotType != SkillSlotType.SLOT_SKILL_0))
                {
                    CUICommonSystem.PlayAnimator(button.GetAnimationPresent(), enSkillButtonAnimationName.normal.ToString());
                }
                else if (!button.bLimitedFlag && (skillSlotType == SkillSlotType.SLOT_SKILL_0))
                {
                    GameObject animationPresent = button.GetAnimationPresent();
                    if (animationPresent != null)
                    {
                        Image image = animationPresent.GetComponent<Image>();
                        if (image != null)
                        {
                            image.color = CUIUtility.s_Color_Full;
                        }
                    }
                    GameObject obj4 = button.GetDisableButton();
                    if (obj4 != null)
                    {
                        obj4.CustomSetActive(false);
                    }
                    this.SetSelectTargetBtnState(false);
                }
                GameObject disableButton = button.GetDisableButton();
                if (disableButton != null)
                {
                    CUIEventScript script2 = disableButton.GetComponent<CUIEventScript>();
                    if (script2 != null)
                    {
                        if (script2.ClearInputStatus())
                        {
                            Singleton<CBattleSystem>.GetInstance().HideSkillDescInfo();
                        }
                        script2.enabled = true;
                    }
                    disableButton.CustomSetActive(false);
                }
            }
            return true;
        }

        public void SetEnergyDisableButton(SkillSlotType skillSlotType)
        {
            if (Singleton<CBattleSystem>.GetInstance().m_FormScript != null)
            {
                SkillButton button = this.GetButton(skillSlotType);
                if (button != null)
                {
                    if (button.m_button != null)
                    {
                        CUIEventScript component = button.m_button.GetComponent<CUIEventScript>();
                        if (component != null)
                        {
                            component.enabled = false;
                        }
                    }
                    button.bDisableFlag = true;
                    CUICommonSystem.PlayAnimator(button.GetAnimationPresent(), enSkillButtonAnimationName.disable.ToString());
                    GameObject disableButton = button.GetDisableButton();
                    if (disableButton != null)
                    {
                        disableButton.CustomSetActive(true);
                    }
                }
            }
        }

        public void SetlearnBtnHighLight(GameObject learnBtn, bool highLight)
        {
            Transform transform = learnBtn.transform.FindChild("highlighter");
            if (transform != null)
            {
                transform.gameObject.CustomSetActive(highLight);
            }
        }

        public void SetLimitButton(SkillSlotType skillSlotType)
        {
            if (skillSlotType != SkillSlotType.SLOT_SKILL_0)
            {
                CUIFormScript formScript = Singleton<CBattleSystem>.GetInstance().m_FormScript;
                if (formScript != null)
                {
                    if (this.m_currentSkillSlotType == skillSlotType)
                    {
                        this.SkillButtonUp(formScript, skillSlotType, false);
                    }
                    SkillButton button = this.GetButton(skillSlotType);
                    DebugHelper.Assert(button != null);
                    if (button != null)
                    {
                        if (button.m_button != null)
                        {
                            CUIEventScript component = button.m_button.GetComponent<CUIEventScript>();
                            if (component != null)
                            {
                                if (component.ClearInputStatus())
                                {
                                    Singleton<CBattleSystem>.GetInstance().HideSkillDescInfo();
                                }
                                component.enabled = false;
                            }
                        }
                        GameObject gameObject = button.GetAnimationPresent().transform.Find("disableFrame").gameObject;
                        DebugHelper.Assert(gameObject != null);
                        if (gameObject != null)
                        {
                            gameObject.CustomSetActive(true);
                        }
                        button.bLimitedFlag = true;
                        CUICommonSystem.PlayAnimator(button.GetAnimationPresent(), enSkillButtonAnimationName.disable.ToString());
                    }
                }
            }
        }

        private void SetSelectTargetBtnState(bool bActive)
        {
            if (GameSettings.TheCommonAttackType == CommonAttactType.Type2)
            {
                CUIFormScript formScript = Singleton<CBattleSystem>.GetInstance().m_FormScript;
                if (formScript != null)
                {
                    GameObject widget = formScript.GetWidget(0x44);
                    GameObject obj3 = formScript.GetWidget(0x39);
                    if ((widget != null) && (obj3 != null))
                    {
                        Color color = CUIUtility.s_Color_Full;
                        if (bActive)
                        {
                            color = CUIUtility.s_Color_DisableGray;
                        }
                        GameObject gameObject = obj3.transform.FindChild("disable").gameObject;
                        if (gameObject != null)
                        {
                            gameObject.CustomSetActive(bActive);
                        }
                        GameObject obj5 = obj3.transform.FindChild("Present").gameObject;
                        if (obj5 != null)
                        {
                            Image component = obj5.GetComponent<Image>();
                            if (component != null)
                            {
                                component.color = color;
                            }
                        }
                        gameObject = widget.transform.FindChild("disable").gameObject;
                        if (gameObject != null)
                        {
                            gameObject.CustomSetActive(bActive);
                        }
                        obj5 = widget.transform.FindChild("Present").gameObject;
                        if (obj5 != null)
                        {
                            Image image2 = obj5.GetComponent<Image>();
                            if (image2 != null)
                            {
                                image2.color = color;
                            }
                        }
                    }
                }
            }
        }

        public void SetSkillIndicatorMode(enSkillIndicatorMode indicaMode)
        {
            this.m_skillIndicatorMode = indicaMode;
        }

        public void SkillButtonDown(CUIFormScript formScript, SkillSlotType skillSlotType, Vector2 downScreenPosition)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            if ((hostPlayer != null) && (hostPlayer.Captain != 0))
            {
                int skillLevel = 0;
                if (((hostPlayer.Captain.handle.SkillControl != null) && (skillSlotType >= SkillSlotType.SLOT_SKILL_0)) && ((skillSlotType < SkillSlotType.SLOT_SKILL_COUNT) && (hostPlayer.Captain.handle.SkillControl.SkillSlotArray[(int) skillSlotType] != null)))
                {
                    skillLevel = hostPlayer.Captain.handle.SkillControl.SkillSlotArray[(int) skillSlotType].GetSkillLevel();
                }
                if (skillLevel <= 0)
                {
                    return;
                }
            }
            if (this.m_currentSkillSlotType != SkillSlotType.SLOT_SKILL_COUNT)
            {
                this.SkillButtonUp(formScript, this.m_currentSkillSlotType, false);
            }
            this.m_currentSkillSlotType = skillSlotType;
            this.m_currentSkillDownScreenPosition = downScreenPosition;
            this.m_currentSkillIndicatorEnabled = false;
            this.m_currentSkillIndicatorJoystickEnabled = false;
            this.m_currentSkillIndicatorInCancelArea = false;
            GameObject animationPresent = this.GetButton(skillSlotType).GetAnimationPresent();
            if (hostPlayer != null)
            {
                if (skillSlotType == SkillSlotType.SLOT_SKILL_0)
                {
                    CUICommonSystem.PlayAnimator(animationPresent, enSkillButtonAnimationName.atkPressDown.ToString());
                    this.SendUseCommonAttack(1, 0);
                    Singleton<CUIParticleSystem>.GetInstance().AddParticle(CUIParticleSystem.s_particleSkillBtnEffect_Path, 0.5f, animationPresent, formScript);
                }
                else
                {
                    if (skillSlotType != SkillSlotType.SLOT_SKILL_0)
                    {
                        CUICommonSystem.PlayAnimator(animationPresent, enSkillButtonAnimationName.pressDown.ToString());
                    }
                    else
                    {
                        CUICommonSystem.PlayAnimator(animationPresent, enSkillButtonAnimationName.atkPressDown.ToString());
                    }
                    this.ReadyUseSkillSlot(skillSlotType);
                    this.EnableSkillCursor(formScript, ref downScreenPosition, this.IsUseSkillCursorJoystick(skillSlotType), skillSlotType, skillSlotType != SkillSlotType.SLOT_SKILL_0);
                }
            }
        }

        public void SkillButtonUp(CUIFormScript formScript)
        {
            if ((this.m_currentSkillSlotType != SkillSlotType.SLOT_SKILL_COUNT) && (formScript != null))
            {
                this.SkillButtonUp(formScript, this.m_currentSkillSlotType, false);
            }
        }

        public void SkillButtonUp(CUIFormScript formScript, SkillSlotType skillSlotType, bool isTriggeredActively)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
            if (((hostPlayer != null) && (this.m_currentSkillSlotType == skillSlotType)) && (formScript != null))
            {
                if (hostPlayer.Captain != 0)
                {
                    int skillLevel = 0;
                    if (((hostPlayer.Captain.handle.SkillControl != null) && (skillSlotType >= SkillSlotType.SLOT_SKILL_0)) && ((skillSlotType < SkillSlotType.SLOT_SKILL_COUNT) && (hostPlayer.Captain.handle.SkillControl.SkillSlotArray[(int) skillSlotType] != null)))
                    {
                        skillLevel = hostPlayer.Captain.handle.SkillControl.SkillSlotArray[(int) skillSlotType].GetSkillLevel();
                    }
                    if (skillLevel <= 0)
                    {
                        return;
                    }
                }
                SkillButton button = this.GetButton(skillSlotType);
                if (button != null)
                {
                    GameObject animationPresent = button.GetAnimationPresent();
                    if (skillSlotType == SkillSlotType.SLOT_SKILL_0)
                    {
                        CUICommonSystem.PlayAnimator(animationPresent, enSkillButtonAnimationName.atkPressUp.ToString());
                        this.SendUseCommonAttack(0, 0);
                    }
                    else
                    {
                        if (skillSlotType != SkillSlotType.SLOT_SKILL_0)
                        {
                            CUICommonSystem.PlayAnimator(animationPresent, enSkillButtonAnimationName.pressUp.ToString());
                        }
                        else
                        {
                            CUICommonSystem.PlayAnimator(animationPresent, enSkillButtonAnimationName.atkPressUp.ToString());
                        }
                        if (isTriggeredActively && !this.m_currentSkillIndicatorInCancelArea)
                        {
                            if (skillSlotType != SkillSlotType.SLOT_SKILL_0)
                            {
                                this.RequestUseSkillSlot(skillSlotType);
                            }
                        }
                        else
                        {
                            this.CancelUseSkillSlot(skillSlotType);
                        }
                        if (this.m_currentSkillIndicatorEnabled)
                        {
                            this.DisableSkillCursor(formScript);
                        }
                    }
                    this.m_currentSkillSlotType = SkillSlotType.SLOT_SKILL_COUNT;
                    this.m_currentSkillDownScreenPosition = Vector2.zero;
                }
            }
        }

        public void UpdateButtonCD(SkillSlotType skillSlotType, int cd)
        {
            SkillButton button = this.GetButton(skillSlotType);
            if (cd <= 0)
            {
                this.SetEnableButton(skillSlotType);
            }
            this.UpdateButtonCDText((button == null) ? null : button.m_button, (button == null) ? null : button.m_cdText, cd);
        }

        private void UpdateButtonCDText(GameObject button, GameObject cdText, int cd)
        {
            if (cdText != null)
            {
                if (cd <= 0)
                {
                    cdText.CustomSetActive(false);
                }
                else
                {
                    cdText.CustomSetActive(true);
                    Text component = cdText.GetComponent<Text>();
                    if (component != null)
                    {
                        component.text = string.Format("{0}", Mathf.CeilToInt((float) (cd / 0x3e8)) + 1);
                    }
                }
            }
            if ((button != null) && (cdText != null))
            {
                cdText.transform.position = button.transform.position;
            }
        }

        public void UpdateLogic(int delta)
        {
            for (int i = 0; i < this._skillButtons.Length; i++)
            {
                SkillButton button = this._skillButtons[i];
                if ((button != null) && (null != button.effectTimeImage))
                {
                    button.effectTimeLeft -= delta;
                    if (button.effectTimeLeft < 0)
                    {
                        button.effectTimeLeft = 0;
                    }
                    button.effectTimeImage.CustomFillAmount(((float) button.effectTimeLeft) / ((float) button.effectTimeTotal));
                    if (button.effectTimeLeft <= 0)
                    {
                        button.effectTimeTotal = 0;
                        button.effectTimeImage.gameObject.CustomSetActive(false);
                        button.effectTimeImage = null;
                    }
                }
            }
        }

        public bool CurrentSkillIndicatorResponed
        {
            get
            {
                return this.m_currentSkillIndicatorResponed;
            }
        }

        public bool CurrentSkillTipsResponed
        {
            get
            {
                return this.m_currentSkillTipsResponed;
            }
        }
    }
}

