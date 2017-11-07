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
    public class CLoudSpeakerSys : Singleton<CLoudSpeakerSys>
    {
        public const int LOUDSPEAKER_ID = 0x273a;
        private ListView<COMDT_CHAT_MSG_HORN> loudSpeakerList = new ListView<COMDT_CHAT_MSG_HORN>();
        private uint m_characterLimit;
        private uint m_itemID;
        private int m_loudSpeakerCount;
        private uint m_loudSpeakerIndex;
        private int m_speakerCount;
        private uint m_speakerIndex;
        private int m_timerLoudSpeaker = -1;
        private int m_timerReq = -1;
        private int m_timerSpeaker = -1;
        public const int REQ_TIME_DELTA = 5;
        private static string s_characterLimitString = string.Empty;
        public static readonly string SPEAKER_FORM_PATH = "UGUI/Form/System/LoudSpeaker/Form_LoudSpeaker.prefab";
        public const int SPEAKER_ID = 0x2739;
        private ListView<COMDT_CHAT_MSG_HORN> speakerList = new ListView<COMDT_CHAT_MSG_HORN>();

        public void AddSpeakerArray(CS_HORN_TYPE type, COMDT_CHAT_MSG_HORN[] astMsgInfo, uint len)
        {
            ListView<COMDT_CHAT_MSG_HORN> speakerList = this.GetSpeakerList(type);
            int lastSpeakerIndex = (int) this.GetLastSpeakerIndex(type);
            if (lastSpeakerIndex == 0)
            {
                lastSpeakerIndex = -1;
            }
            for (int i = 0; i < len; i++)
            {
                if (lastSpeakerIndex < astMsgInfo[i].dwMsgIdx)
                {
                    speakerList.Add(astMsgInfo[i]);
                    Singleton<CChatController>.instance.model.Add_Palyer_Info(astMsgInfo[i].stFrom);
                    CChatEntity chatEnt = (type != CS_HORN_TYPE.CS_HORNTYPE_SMALL) ? CChatUT.Build_4_LoudSpeaker(astMsgInfo[i]) : CChatUT.Build_4_Speaker(astMsgInfo[i]);
                    Singleton<CChatController>.instance.model.channelMgr.Add_ChatEntity(chatEnt, EChatChannel.Lobby, 0L, 0);
                    if (type == CS_HORN_TYPE.CS_HORNTYPE_SMALL)
                    {
                        this.m_speakerIndex = astMsgInfo[i].dwMsgIdx;
                    }
                    else
                    {
                        this.m_loudSpeakerIndex = astMsgInfo[i].dwMsgIdx;
                    }
                }
            }
            if ((type == CS_HORN_TYPE.CS_HORNTYPE_SMALL) && (len > 0))
            {
                this.OnSpeakerNodeOpen();
            }
            if ((type == CS_HORN_TYPE.CS_HORNTYPE_BIGER) && (len > 0))
            {
                this.OnLoudSpeakerTipsOpen(null);
            }
        }

        public void Clear()
        {
            if (this.m_timerReq != -1)
            {
                Singleton<CTimerManager>.instance.RemoveTimer(this.m_timerReq);
            }
            this.m_timerReq = -1;
            if (this.m_timerSpeaker != -1)
            {
                Singleton<CTimerManager>.instance.RemoveTimer(this.m_timerSpeaker);
            }
            this.m_timerSpeaker = -1;
            if (this.m_timerLoudSpeaker != -1)
            {
                Singleton<CTimerManager>.instance.RemoveTimer(this.m_timerLoudSpeaker);
            }
            this.m_timerLoudSpeaker = -1;
            this.m_speakerIndex = 0;
            this.m_loudSpeakerIndex = 0;
            this.m_speakerCount = 0;
            this.m_loudSpeakerCount = 0;
            this.m_itemID = 0;
            this.speakerList.Clear();
            this.loudSpeakerList.Clear();
        }

        private string GetInputText()
        {
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(SPEAKER_FORM_PATH);
            if ((form != null) && (form.gameObject != null))
            {
                return Utility.GetComponetInChild<InputField>(form.gameObject, "pnlBg/Panel_Main/InputField").text;
            }
            return string.Empty;
        }

        private uint GetLastSpeakerIndex(CS_HORN_TYPE type)
        {
            ListView<COMDT_CHAT_MSG_HORN> speakerList = this.GetSpeakerList(type);
            if (speakerList.Count > 0)
            {
                return speakerList[speakerList.Count - 1].dwMsgIdx;
            }
            return 0;
        }

        private ListView<COMDT_CHAT_MSG_HORN> GetSpeakerList(CS_HORN_TYPE type)
        {
            if (type == CS_HORN_TYPE.CS_HORNTYPE_SMALL)
            {
                return this.speakerList;
            }
            return this.loudSpeakerList;
        }

        private void GetSpeakerMsg(CS_HORN_TYPE type, uint index)
        {
            if (index != 0)
            {
                index++;
            }
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x519);
            msg.stPkgData.stGetHornMsgReq.bHornType = (byte) type;
            msg.stPkgData.stGetHornMsgReq.dwMsgIdx = index;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
        }

        public static uint GetSpeakerVaildTime(CS_HORN_TYPE type)
        {
            ResHornInfo dataByKey = null;
            ListView<COMDT_CHAT_MSG_HORN> speakerList = Singleton<CLoudSpeakerSys>.instance.GetSpeakerList(type);
            if (type == CS_HORN_TYPE.CS_HORNTYPE_SMALL)
            {
                dataByKey = GameDataMgr.speakerDatabin.GetDataByKey(0x2739);
            }
            else
            {
                dataByKey = GameDataMgr.speakerDatabin.GetDataByKey(0x273a);
            }
            if (speakerList.Count == 0)
            {
                return dataByKey.dwMaxShowSec;
            }
            return dataByKey.dwMinShowSec;
        }

        public override void Init()
        {
            base.Init();
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Speaker_Form_Open, new CUIEventManager.OnUIEventHandler(this.OnSpeakerFormOpen));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Speaker_Form_Clsoe, new CUIEventManager.OnUIEventHandler(this.OnSpeakerFormClose));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Speaker_Send, new CUIEventManager.OnUIEventHandler(this.OnSpeakerSend));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Speaker_Form_Update, new CUIEventManager.OnUIEventHandler(this.OnUpdateCharacterLimitText));
            s_characterLimitString = Singleton<CTextManager>.instance.GetText("Speaker_CharacterLimit");
        }

        public bool IsLoudSpeakerShowing()
        {
            return ((this.m_timerLoudSpeaker != -1) && (this.m_loudSpeakerCount < GetSpeakerVaildTime(CS_HORN_TYPE.CS_HORNTYPE_BIGER)));
        }

        public bool IsSpeakerShowing()
        {
            return ((this.m_timerSpeaker != -1) && (this.m_speakerCount < GetSpeakerVaildTime(CS_HORN_TYPE.CS_HORNTYPE_SMALL)));
        }

        [MessageHandler(0x51a)]
        public static void OnGetSpeakerMsgRsp(CSPkg msg)
        {
            CS_HORN_TYPE bHornType = (CS_HORN_TYPE) msg.stPkgData.stGetHornMsgRsp.bHornType;
            Singleton<CLoudSpeakerSys>.instance.AddSpeakerArray(bHornType, msg.stPkgData.stGetHornMsgRsp.astMsgInfo, msg.stPkgData.stGetHornMsgRsp.wMsgCnt);
            Singleton<EventRouter>.GetInstance().BroadCastEvent("Chat_LobbyChatData_Change");
        }

        private void OnLoudSpeakerTipsOpen(CUIEvent uiEvent)
        {
            if (!this.IsLoudSpeakerShowing())
            {
                COMDT_CHAT_MSG_HORN data = this.PopSpeakerList(CS_HORN_TYPE.CS_HORNTYPE_BIGER);
                if (data != null)
                {
                    if (this.m_timerLoudSpeaker != -1)
                    {
                        Singleton<CTimerManager>.instance.RemoveTimer(this.m_timerLoudSpeaker);
                        this.m_timerLoudSpeaker = -1;
                    }
                    this.ShowLoudSpeaker(data);
                }
            }
        }

        private void OnSpeakerFormClose(CUIEvent uiEvent)
        {
            Singleton<CUIManager>.instance.CloseForm(SPEAKER_FORM_PATH);
        }

        private void OnSpeakerFormOpen(CUIEvent uiEvent)
        {
            uint key = uiEvent.m_eventParams.commonUInt32Param1;
            switch (key)
            {
                case 0x2739:
                case 0x273a:
                {
                    ResHornInfo dataByKey = GameDataMgr.speakerDatabin.GetDataByKey(key);
                    this.m_itemID = key;
                    this.m_characterLimit = dataByKey.dwWordLimit;
                    if (dataByKey == null)
                    {
                        return;
                    }
                    CUIFormScript script = Singleton<CUIManager>.instance.OpenForm(SPEAKER_FORM_PATH, false, false);
                    if ((script == null) || (script.gameObject == null))
                    {
                        return;
                    }
                    GameObject obj2 = Utility.FindChild(script.gameObject, "pnlBg/Title/speakerText");
                    GameObject obj3 = Utility.FindChild(script.gameObject, "pnlBg/Title/loudSpeakerText");
                    GameObject obj4 = Utility.FindChild(script.gameObject, "pnlBg/Model/speaker");
                    GameObject obj5 = Utility.FindChild(script.gameObject, "pnlBg/Model/loudspeaker");
                    InputField componetInChild = Utility.GetComponetInChild<InputField>(script.gameObject, "pnlBg/Panel_Main/InputField");
                    Utility.GetComponetInChild<CUITimerScript>(script.gameObject, "Timer").ReStartTimer();
                    if (key == 0x2739)
                    {
                        obj2.CustomSetActive(true);
                        obj3.CustomSetActive(false);
                        obj4.CustomSetActive(true);
                        obj5.CustomSetActive(false);
                        componetInChild.characterLimit = (int) this.m_characterLimit;
                    }
                    else
                    {
                        obj2.CustomSetActive(false);
                        obj3.CustomSetActive(true);
                        obj4.CustomSetActive(false);
                        obj5.CustomSetActive(true);
                        componetInChild.characterLimit = (int) this.m_characterLimit;
                    }
                    break;
                }
            }
        }

        private void OnSpeakerNodeOpen()
        {
            if (!this.IsSpeakerShowing())
            {
                COMDT_CHAT_MSG_HORN data = this.PopSpeakerList(CS_HORN_TYPE.CS_HORNTYPE_SMALL);
                if (data != null)
                {
                    if (this.m_timerSpeaker != -1)
                    {
                        Singleton<CTimerManager>.instance.RemoveTimer(this.m_timerSpeaker);
                        this.m_timerSpeaker = -1;
                    }
                    this.ShowSpeaker(data);
                }
            }
        }

        private void OnSpeakerSend(CUIEvent uiEvent)
        {
            string inputText = this.GetInputText();
            if (string.IsNullOrEmpty(inputText))
            {
                Singleton<CUIManager>.instance.OpenTips("Chat_Common_Tips_10", true, 1f, null, new object[0]);
            }
            else
            {
                CUseable useableByBaseID = Singleton<CRoleInfoManager>.GetInstance().GetMasterRoleInfo().GetUseableContainer(enCONTAINER_TYPE.ITEM).GetUseableByBaseID(COM_ITEM_TYPE.COM_OBJTYPE_ITEMPROP, this.m_itemID);
                if (useableByBaseID != null)
                {
                    this.OnSpeakerSend(inputText, useableByBaseID.m_objID);
                }
            }
        }

        private void OnSpeakerSend(string content, ulong uniqueID)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x463);
            msg.stPkgData.stHornUseReq.ullUniqueID = uniqueID;
            msg.stPkgData.stHornUseReq.szContent = Utility.BytesConvert(content);
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, true);
        }

        [MessageHandler(0x464)]
        public static void OnSpeakerSendRsp(CSPkg msg)
        {
            if (msg.stPkgData.stHornUseRsp.iResult == 0)
            {
                Singleton<CUIManager>.instance.CloseForm(SPEAKER_FORM_PATH);
                Singleton<CUIManager>.GetInstance().CloseForm(CBagSystem.s_bagFormPath);
            }
            else if (msg.stPkgData.stHornUseRsp.iResult == 2)
            {
                Singleton<CUIManager>.instance.OpenTips("Speaker_Use_Err_2", true, 1f, null, new object[0]);
            }
            else if (msg.stPkgData.stHornUseRsp.iResult == 4)
            {
                Singleton<CUIManager>.instance.OpenTips("Speaker_Use_Err_4", true, 1f, null, new object[0]);
            }
            else
            {
                object[] replaceArr = new object[] { msg.stPkgData.stHornUseRsp.iResult };
                Singleton<CUIManager>.instance.OpenTips("Speaker_Use_Err_1", true, 1f, null, replaceArr);
            }
            Singleton<CUIManager>.instance.CloseSendMsgAlert();
        }

        private void OnTimerLoudSpeaker(int timerSequence)
        {
            this.m_loudSpeakerCount++;
            if (this.m_loudSpeakerCount >= GetSpeakerVaildTime(CS_HORN_TYPE.CS_HORNTYPE_BIGER))
            {
                Singleton<CTimerManager>.instance.RemoveTimer(timerSequence);
                this.m_timerLoudSpeaker = -1;
                this.m_loudSpeakerCount = 0;
                COMDT_CHAT_MSG_HORN data = this.PopSpeakerList(CS_HORN_TYPE.CS_HORNTYPE_BIGER);
                CUIFormScript form = Singleton<CUIManager>.instance.GetForm(LobbyForm.FORM_PATH);
                if (form != null)
                {
                    CUIAutoScroller component = form.GetWidget(5).GetComponent<CUIAutoScroller>();
                    if (component != null)
                    {
                        GameObject widget = form.GetWidget(6);
                        if (widget != null)
                        {
                            component.StopAutoScroll();
                            if (data == null)
                            {
                                component.gameObject.CustomSetActive(false);
                                widget.CustomSetActive(false);
                                this.GetSpeakerMsg(CS_HORN_TYPE.CS_HORNTYPE_BIGER, this.m_loudSpeakerIndex);
                            }
                            else
                            {
                                component.gameObject.CustomSetActive(true);
                                widget.CustomSetActive(true);
                                this.ShowLoudSpeaker(data);
                            }
                        }
                    }
                }
            }
        }

        private void OnTimerReq(int timerSequence)
        {
            if (Singleton<BattleLogic>.instance.isRuning)
            {
                this.m_speakerIndex = 0;
                this.m_loudSpeakerIndex = 0;
            }
            else
            {
                if (this.speakerList.Count <= 1)
                {
                    this.GetSpeakerMsg(CS_HORN_TYPE.CS_HORNTYPE_SMALL, this.m_speakerIndex);
                }
                if (this.loudSpeakerList.Count <= 1)
                {
                    this.GetSpeakerMsg(CS_HORN_TYPE.CS_HORNTYPE_BIGER, this.m_loudSpeakerIndex);
                }
            }
        }

        private void OnTimerSpeaker(int timerSequence)
        {
            this.m_speakerCount++;
            if (this.m_speakerCount >= GetSpeakerVaildTime(CS_HORN_TYPE.CS_HORNTYPE_SMALL))
            {
                Singleton<CTimerManager>.instance.RemoveTimer(timerSequence);
                this.m_timerSpeaker = -1;
                this.m_speakerCount = 0;
                COMDT_CHAT_MSG_HORN data = this.PopSpeakerList(CS_HORN_TYPE.CS_HORNTYPE_SMALL);
                if (data != null)
                {
                    this.ShowSpeaker(data);
                }
                else
                {
                    Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Speaker_EntryNode_TimeUp);
                    this.GetSpeakerMsg(CS_HORN_TYPE.CS_HORNTYPE_SMALL, this.m_speakerIndex);
                }
            }
        }

        private void OnUpdateCharacterLimitText(CUIEvent uiEvent)
        {
            CUIFormScript form = Singleton<CUIManager>.instance.GetForm(SPEAKER_FORM_PATH);
            if ((form != null) && (form.gameObject != null))
            {
                Text componetInChild = Utility.GetComponetInChild<Text>(form.gameObject, "pnlBg/Panel_Main/CharLimitTxt");
                if (componetInChild != null)
                {
                    InputField field = Utility.GetComponetInChild<InputField>(form.gameObject, "pnlBg/Panel_Main/InputField");
                    if (field != null)
                    {
                        int num = ((int) this.m_characterLimit) - field.text.Length;
                        if (num < 0)
                        {
                            num = 0;
                        }
                        componetInChild.text = string.Format(s_characterLimitString, num);
                    }
                }
            }
        }

        private COMDT_CHAT_MSG_HORN PopSpeakerList(CS_HORN_TYPE type)
        {
            COMDT_CHAT_MSG_HORN comdt_chat_msg_horn = null;
            ListView<COMDT_CHAT_MSG_HORN> speakerList = this.GetSpeakerList(type);
            if (speakerList.Count > 0)
            {
                comdt_chat_msg_horn = speakerList[0];
                speakerList.RemoveAt(0);
            }
            return comdt_chat_msg_horn;
        }

        public void ShowLoudSpeaker(COMDT_CHAT_MSG_HORN data)
        {
            this.m_loudSpeakerCount = 0;
            this.m_timerLoudSpeaker = Singleton<CTimerManager>.instance.AddTimer(0x3e8, 0, new CTimer.OnTimeUpHandler(this.OnTimerLoudSpeaker));
            if (Singleton<BattleLogic>.instance.isRuning)
            {
                this.loudSpeakerList.Clear();
            }
            else
            {
                CUIFormScript form = Singleton<CUIManager>.instance.GetForm(LobbyForm.FORM_PATH);
                if (form != null)
                {
                    CUIAutoScroller component = form.GetWidget(5).GetComponent<CUIAutoScroller>();
                    if (component != null)
                    {
                        GameObject widget = form.GetWidget(6);
                        if (widget != null)
                        {
                            string rawText = UT.Bytes2String(data.szContent);
                            string str = CChatUT.Build_4_LoudSpeaker_EntryString(data.stFrom.ullUid, (uint) data.stFrom.iLogicWorldID, rawText);
                            component.SetText(CUIUtility.RemoveEmoji(str));
                            component.gameObject.CustomSetActive(true);
                            widget.CustomSetActive(true);
                            component.StopAutoScroll();
                            component.StartAutoScroll(true);
                        }
                    }
                }
            }
        }

        public void ShowSpeaker(COMDT_CHAT_MSG_HORN data)
        {
            this.m_speakerCount = 0;
            string a = CChatUT.Build_4_Speaker_EntryString(data.stFrom.ullUid, (uint) data.stFrom.iLogicWorldID, UT.Bytes2String(data.szContent));
            this.m_timerSpeaker = Singleton<CTimerManager>.instance.AddTimer(0x3e8, 0, new CTimer.OnTimeUpHandler(this.OnTimerSpeaker));
            Singleton<CChatController>.instance.model.sysData.Add_NewContent_Entry_Speaker(a);
            Singleton<EventRouter>.GetInstance().BroadCastEvent("Chat_ChatEntry_Change");
        }

        public void StartReqTimer()
        {
            if (this.m_timerReq == -1)
            {
                this.m_timerReq = Singleton<CTimerManager>.instance.AddTimer(0x1388, 0, new CTimer.OnTimeUpHandler(this.OnTimerReq));
                this.OnTimerReq(0);
            }
        }

        public override void UnInit()
        {
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Speaker_Form_Open, new CUIEventManager.OnUIEventHandler(this.OnSpeakerFormOpen));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Speaker_Form_Clsoe, new CUIEventManager.OnUIEventHandler(this.OnSpeakerFormClose));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Speaker_Send, new CUIEventManager.OnUIEventHandler(this.OnSpeakerSend));
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Speaker_Form_Update, new CUIEventManager.OnUIEventHandler(this.OnUpdateCharacterLimitText));
            base.UnInit();
        }
    }
}

