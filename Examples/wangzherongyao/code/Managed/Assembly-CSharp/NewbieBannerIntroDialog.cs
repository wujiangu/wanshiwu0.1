using Assets.Scripts.UI;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class NewbieBannerIntroDialog
{
    public const int AUTOMOVE_TIME = 0x1f40;
    public static string BANNER_INTRO_DIALOG_PATH = "UGUI/Form/System/Newbie/Form_BannerIntroDialog";
    private bool m_bAutoMove;
    private GameObject m_BottomBtn;
    private GameObject m_BtnLeft;
    private string m_btnName;
    private GameObject m_BtnRight;
    private Text m_confirmBtnTxt;
    private int m_curImgIndex;
    private CUIEvent m_evtPars;
    private CUIFormScript m_form = Singleton<CUIManager>.GetInstance().OpenForm(BANNER_INTRO_DIALOG_PATH, false, true);
    private string[] m_imgPath;
    private Vector2 m_lastPos = Vector2.zero;
    private Text m_pageNumberTxt;
    private int[] m_PickIdxList;
    private GameObject m_PickObject;
    private CUIStepListScript m_stepList;
    private int m_TimerSeq;
    private string m_title;
    private Text m_titleTxt;
    private int m_totalImgNum;
    public static NewbieBannerIntroDialog s_theDialog;

    private NewbieBannerIntroDialog(string[] imgPath, int imgNum, CUIEvent uieventPars = null, string title = null, string btnName = null, bool bAutoMove = true)
    {
        if (this.m_form != null)
        {
            CUIContainerScript component = this.m_form.GetWidget(2).transform.FindChild("pickObj").GetComponent<CUIContainerScript>();
            this.m_totalImgNum = Math.Min(Math.Min(imgPath.Length, imgNum), component.m_prepareElementAmount);
            this.m_imgPath = imgPath;
            this.m_evtPars = uieventPars;
            this.m_title = title;
            this.m_btnName = btnName;
            this.m_PickIdxList = new int[this.m_totalImgNum];
            this.m_bAutoMove = bAutoMove;
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_BannerIntroDlg_ClickPrePage, new CUIEventManager.OnUIEventHandler(this.OnMoveToPrePage));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_BannerIntroDlg_ClickNextPage, new CUIEventManager.OnUIEventHandler(this.OnMoveToNextPage));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_BannerIntroDlg_DragStart, new CUIEventManager.OnUIEventHandler(this.OnDragStart));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_BannerIntroDlg_DragEnd, new CUIEventManager.OnUIEventHandler(this.OnDragEnd));
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Newbie_BannerIntroDlg_Close, new CUIEventManager.OnUIEventHandler(this.OnDialogClose));
            this.InitForm();
            this.InitPickObjElement(this.m_totalImgNum);
            this.RefreshUI(0);
            this.m_TimerSeq = Singleton<CTimerManager>.GetInstance().AddTimer(0x1f40, -1, new CTimer.OnTimeUpHandler(this.AutoMoveBannerImg));
            s_theDialog = this;
        }
    }

    private void AutoMoveBannerImg(int delt)
    {
        if (this.m_bAutoMove && (this.m_curImgIndex < (this.m_totalImgNum - 1)))
        {
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Newbie_BannerIntroDlg_ClickNextPage);
        }
    }

    private void InitForm()
    {
        if (this.m_form != null)
        {
            this.m_titleTxt = this.m_form.GetWidget(0).GetComponent<Text>();
            this.m_confirmBtnTxt = this.m_form.GetWidget(1).transform.FindChild("Text").GetComponent<Text>();
            this.m_stepList = this.m_form.GetWidget(2).GetComponent<CUIStepListScript>();
            this.m_BottomBtn = this.m_form.GetWidget(1).gameObject;
            this.m_PickObject = this.m_form.GetWidget(3).gameObject;
            this.m_BtnLeft = this.m_form.GetWidget(4).gameObject;
            this.m_BtnRight = this.m_form.GetWidget(5).gameObject;
            this.m_stepList.SetElementAmount(this.m_totalImgNum);
            for (int i = 0; i < this.m_totalImgNum; i++)
            {
                if (this.m_stepList.GetElemenet(i) != null)
                {
                    this.m_stepList.GetElemenet(i).transform.FindChild("Image").GetComponent<Image>().SetSprite(this.m_imgPath[i], this.m_form, true, false, false);
                }
            }
            this.m_BottomBtn.CustomSetActive(false);
            this.m_stepList.SetDontUpdate(true);
        }
    }

    private void InitPickObjElement(int nImageCount)
    {
        GameObject pickObject = this.m_PickObject;
        if (pickObject != null)
        {
            CUIContainerScript component = pickObject.GetComponent<CUIContainerScript>();
            if (component != null)
            {
                component.RecycleAllElement();
                for (int i = 0; i < nImageCount; i++)
                {
                    this.m_PickIdxList[i] = component.GetElement();
                }
            }
        }
    }

    private void OnDialogClose(CUIEvent uiEvt)
    {
        Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_BannerIntroDlg_ClickPrePage, new CUIEventManager.OnUIEventHandler(this.OnMoveToPrePage));
        Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_BannerIntroDlg_ClickNextPage, new CUIEventManager.OnUIEventHandler(this.OnMoveToNextPage));
        Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_BannerIntroDlg_DragStart, new CUIEventManager.OnUIEventHandler(this.OnDragStart));
        Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_BannerIntroDlg_DragEnd, new CUIEventManager.OnUIEventHandler(this.OnDragEnd));
        Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Newbie_BannerIntroDlg_Close, new CUIEventManager.OnUIEventHandler(this.OnDialogClose));
        Singleton<CTimerManager>.GetInstance().RemoveTimer(new CTimer.OnTimeUpHandler(this.AutoMoveBannerImg));
        if (this.m_evtPars != null)
        {
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(this.m_evtPars);
        }
        s_theDialog = null;
        this.m_form = null;
    }

    private void OnDragEnd(CUIEvent uiEvt)
    {
        Vector2 position = uiEvt.m_pointerEventData.position;
        if (position.x < this.m_lastPos.x)
        {
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Newbie_BannerIntroDlg_ClickNextPage);
        }
        else if (position.x > this.m_lastPos.x)
        {
            Singleton<CUIEventManager>.GetInstance().DispatchUIEvent(enUIEventID.Newbie_BannerIntroDlg_ClickPrePage);
        }
        Singleton<CTimerManager>.GetInstance().ResetTimer(this.m_TimerSeq);
    }

    private void OnDragStart(CUIEvent uiEvt)
    {
        this.m_lastPos = uiEvt.m_pointerEventData.position;
        Singleton<CTimerManager>.GetInstance().ResetTimer(this.m_TimerSeq);
    }

    private void OnMoveToNextPage(CUIEvent uiEvt)
    {
        if (this.m_curImgIndex < (this.m_totalImgNum - 1))
        {
            this.m_curImgIndex++;
            this.m_stepList.MoveElementInScrollArea(this.m_curImgIndex, false);
            this.RefreshUI(this.m_curImgIndex);
        }
        Singleton<CTimerManager>.GetInstance().ResetTimer(this.m_TimerSeq);
    }

    private void OnMoveToPrePage(CUIEvent uiEvt)
    {
        if (this.m_curImgIndex > 0)
        {
            this.m_curImgIndex--;
            this.m_stepList.MoveElementInScrollArea(this.m_curImgIndex, false);
            this.RefreshUI(this.m_curImgIndex);
        }
        Singleton<CTimerManager>.GetInstance().ResetTimer(this.m_TimerSeq);
    }

    private void RefreshUI(int idx)
    {
        GameObject pickObject = this.m_PickObject;
        if (pickObject != null)
        {
            CUIContainerScript component = pickObject.GetComponent<CUIContainerScript>();
            if (component != null)
            {
                for (int i = 0; i < this.m_PickIdxList.Length; i++)
                {
                    if (i == idx)
                    {
                        GameObject element = component.GetElement(this.m_PickIdxList[i]);
                        if (element != null)
                        {
                            Transform transform = element.transform.FindChild("Image_Pointer");
                            if (transform != null)
                            {
                                transform.gameObject.CustomSetActive(true);
                            }
                        }
                    }
                    else
                    {
                        GameObject obj4 = component.GetElement(this.m_PickIdxList[i]);
                        if (obj4 != null)
                        {
                            Transform transform2 = obj4.transform.FindChild("Image_Pointer");
                            if (transform2 != null)
                            {
                                transform2.gameObject.CustomSetActive(false);
                            }
                        }
                    }
                }
            }
        }
        this.m_BottomBtn.CustomSetActive(idx == (this.m_totalImgNum - 1));
        this.m_PickObject.CustomSetActive(idx != (this.m_totalImgNum - 1));
        this.m_BtnLeft.CustomSetActive(idx != 0);
        this.m_BtnRight.CustomSetActive(idx != (this.m_totalImgNum - 1));
    }

    public static void Show(string[] imgPath, int imgNum, CUIEvent uieventPars = null, string title = null, string btnName = null, bool bAutoMove = true)
    {
        s_theDialog = new NewbieBannerIntroDialog(imgPath, imgNum, uieventPars, title, btnName, bAutoMove);
    }

    public enum enIntroDlgWidget
    {
        enTitleTxt,
        enBottomBtn,
        enStepList,
        enPickObject,
        enBtnLeft,
        enBtnRight
    }
}

