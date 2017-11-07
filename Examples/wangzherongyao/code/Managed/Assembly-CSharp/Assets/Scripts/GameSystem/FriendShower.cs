namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.UI;
    using CSProtocol;
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

    public class FriendShower : MonoBehaviour
    {
        public GameObject add_node;
        public GameObject del_node;
        public uint dwLogicWorldID;
        public CUIFormScript formScript;
        public GameObject genderImage;
        public Image headIcon;
        public GameObject HeadIconBack;
        public CUIHttpImageScript HttpImage;
        public CUIEventScript inviteGuildBtn_eventScript;
        public Text inviteGuildBtnText;
        public Button inviteGuildButton;
        public Text LevelText;
        public Text NameText;
        public GameObject nobeIcon;
        public GameObject normal_node;
        public Button PKButton;
        public Image pvpIcon;
        public Text pvpText;
        public GameObject QQVipImage;
        public CUIEventScript reCallBtn_eventScript;
        public Button reCallButton;
        public Text reCallText;
        public GameObject request_node;
        public Text SendBtnText;
        public CUIEventScript sendHeartBtn_eventScript;
        public Button sendHeartButton;
        public Image sendHeartIcon;
        public Text time;
        public ulong ullUid;
        public Text VipLevel;

        public void HideSendButton()
        {
            if (this.sendHeartButton != null)
            {
                this.sendHeartButton.gameObject.CustomSetActive(false);
            }
        }

        public void SetBGray(bool bGray)
        {
            UT.SetImage(this.headIcon, bGray);
        }

        public void SetFriendItemType(ItemType type, bool bShowDelete = true)
        {
            this.add_node.CustomSetActive(false);
            this.normal_node.CustomSetActive(false);
            this.request_node.CustomSetActive(false);
            if (this.inviteGuildButton != null)
            {
                this.inviteGuildButton.gameObject.CustomSetActive(false);
            }
            if (type == ItemType.Add)
            {
                this.add_node.CustomSetActive(true);
            }
            if (type == ItemType.Normal)
            {
                this.normal_node.CustomSetActive(true);
                if (this.del_node != null)
                {
                    this.del_node.CustomSetActive(bShowDelete);
                }
            }
            if (type == ItemType.Request)
            {
                this.request_node.CustomSetActive(true);
            }
        }

        public void ShowGameState(COM_ACNT_GAME_STATE state, bool bOnline)
        {
            if ((state != COM_ACNT_GAME_STATE.COM_ACNT_GAME_STATE_IDLE) && bOnline)
            {
                this.ShowLastTime(true, "游戏中");
            }
        }

        public void ShowGenderType(COM_SNSGENDER genderType)
        {
            if (this.genderImage != null)
            {
                this.genderImage.gameObject.CustomSetActive(genderType != COM_SNSGENDER.COM_SNSGENDER_NONE);
                if (genderType == COM_SNSGENDER.COM_SNSGENDER_MALE)
                {
                    CUIUtility.SetImageSprite(this.genderImage.GetComponent<Image>(), string.Format("{0}icon/Ico_boy", "UGUI/Sprite/Dynamic/"), null, true, false, false);
                }
                else if (genderType == COM_SNSGENDER.COM_SNSGENDER_FEMALE)
                {
                    CUIUtility.SetImageSprite(this.genderImage.GetComponent<Image>(), string.Format("{0}icon/Ico_girl", "UGUI/Sprite/Dynamic/"), null, true, false, false);
                }
            }
        }

        public void ShowInviteButton(bool isShow, bool isEnable)
        {
            if (this.reCallButton != null)
            {
                if (CSysDynamicBlock.bFriendBlocked)
                {
                    this.reCallButton.gameObject.CustomSetActive(false);
                }
                else if (!isShow)
                {
                    this.reCallButton.gameObject.CustomSetActive(false);
                }
                else
                {
                    if (this.reCallText != null)
                    {
                        if (isEnable)
                        {
                            this.reCallText.text = Singleton<CTextManager>.instance.GetText("Friend_ReCall_Tips_1");
                        }
                        else
                        {
                            this.reCallText.text = Singleton<CTextManager>.instance.GetText("Friend_ReCall_Tips_2");
                        }
                    }
                    this.reCallButton.gameObject.CustomSetActive(true);
                    if (this.reCallBtn_eventScript != null)
                    {
                        this.reCallBtn_eventScript.SetUIEvent(enUIEventType.Click, enUIEventID.Friend_SNS_ReCall);
                    }
                    CUICommonSystem.SetButtonEnable(this.reCallButton, isEnable, isEnable, true);
                }
            }
        }

        public void ShowinviteGuild(bool isShow, bool isEnable)
        {
            if (this.inviteGuildButton != null)
            {
                if (CSysDynamicBlock.bFriendBlocked && this.inviteGuildButton.gameObject.activeSelf)
                {
                    this.inviteGuildButton.gameObject.SetActive(false);
                }
                else if (!isShow)
                {
                    this.inviteGuildButton.gameObject.CustomSetActive(false);
                }
                else
                {
                    this.inviteGuildButton.gameObject.CustomSetActive(true);
                    if (this.inviteGuildBtn_eventScript != null)
                    {
                        this.inviteGuildBtn_eventScript.SetUIEvent(enUIEventType.Click, enUIEventID.Friend_InviteGuild);
                    }
                    if (isEnable)
                    {
                        CUICommonSystem.SetButtonEnable(this.inviteGuildButton, true, true, true);
                        if (this.inviteGuildBtnText != null)
                        {
                            this.inviteGuildBtnText.text = Singleton<CFriendContoller>.instance.model.Guild_Invite_txt;
                        }
                    }
                    else
                    {
                        CUICommonSystem.SetButtonEnable(this.inviteGuildButton, false, false, true);
                        if (this.inviteGuildBtnText != null)
                        {
                            this.inviteGuildBtnText.text = Singleton<CFriendContoller>.instance.model.Guild_Has_Invited_txt;
                        }
                    }
                }
            }
        }

        public void ShowLastTime(bool bShow, string text)
        {
            if (this.time != null)
            {
                this.time.gameObject.CustomSetActive(bShow);
                this.time.text = text;
            }
        }

        public void ShowLevel(uint level)
        {
            if (this.LevelText != null)
            {
                this.LevelText.text = "LV." + level.ToString();
            }
        }

        public void ShowName(string name)
        {
            if (this.NameText != null)
            {
                this.NameText.text = name;
            }
            if (this.pvpText != null)
            {
                this.pvpText.gameObject.CustomSetActive(false);
            }
            if (this.pvpIcon != null)
            {
                this.pvpIcon.gameObject.CustomSetActive(false);
            }
            if (this.sendHeartIcon != null)
            {
                this.sendHeartIcon.gameObject.CustomSetActive(false);
            }
        }

        public void ShowPKButton(bool b)
        {
            if (this.PKButton != null)
            {
                if (CSysDynamicBlock.bFriendBlocked && this.PKButton.gameObject.activeSelf)
                {
                    this.PKButton.gameObject.SetActive(false);
                }
                else if (!b)
                {
                    if (this.PKButton.gameObject.activeSelf)
                    {
                        this.PKButton.gameObject.SetActive(false);
                    }
                }
                else if (!this.PKButton.gameObject.activeSelf)
                {
                    this.PKButton.gameObject.SetActive(true);
                }
            }
        }

        public void ShowPVP_Level(string text, string icon)
        {
        }

        public void ShowRecommendGuild(bool isShow, bool isEnabled)
        {
            if (this.inviteGuildButton != null)
            {
                if (CSysDynamicBlock.bFriendBlocked && this.inviteGuildButton.gameObject.activeSelf)
                {
                    this.inviteGuildButton.gameObject.SetActive(false);
                }
                else if (!isShow)
                {
                    if (this.inviteGuildButton.gameObject.activeSelf)
                    {
                        this.inviteGuildButton.gameObject.CustomSetActive(false);
                    }
                }
                else
                {
                    if (!this.inviteGuildButton.gameObject.activeSelf)
                    {
                        this.inviteGuildButton.gameObject.CustomSetActive(true);
                    }
                    if (this.inviteGuildBtn_eventScript != null)
                    {
                        this.inviteGuildBtn_eventScript.SetUIEvent(enUIEventType.Click, enUIEventID.Friend_RecommendGuild);
                    }
                    if (isEnabled)
                    {
                        CUICommonSystem.SetButtonEnable(this.inviteGuildButton, true, true, true);
                        if (this.inviteGuildBtnText != null)
                        {
                            this.inviteGuildBtnText.text = Singleton<CFriendContoller>.instance.model.Guild_Recommend_txt;
                        }
                    }
                    else
                    {
                        CUICommonSystem.SetButtonEnable(this.inviteGuildButton, false, false, true);
                        if (this.inviteGuildBtnText != null)
                        {
                            this.inviteGuildBtnText.text = Singleton<CFriendContoller>.instance.model.Guild_Has_Recommended_txt;
                        }
                    }
                }
            }
        }

        public void ShowSendButton(bool bEnable)
        {
            if ((this.sendHeartButton != null) && (this.sendHeartButton.gameObject != null))
            {
                CUICommonSystem.SetButtonEnable(this.sendHeartButton, bEnable, bEnable, true);
                this.sendHeartButton.gameObject.CustomSetActive(true);
                this.sendHeartButton.transform.Find("Text").GetComponent<Text>().text = Singleton<CTextManager>.instance.GetText(!bEnable ? "Have_Send" : "Send_Coin");
            }
        }

        public void ShowVipLevel(uint level)
        {
            if (this.VipLevel != null)
            {
                this.VipLevel.text = "VIP." + level.ToString();
            }
        }

        public enum ItemType
        {
            Add,
            Normal,
            Request
        }
    }
}

