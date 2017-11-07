namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.UI;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ExchangeWgt : ActivityWidget
    {
        private readonly DictionaryView<int, GameObject> _elementsGo;
        private GameObject _elementTmpl;
        private const float SpacingY = 5f;

        public ExchangeWgt(GameObject node, ActivityView view) : base(node, view)
        {
            this._elementsGo = new DictionaryView<int, GameObject>();
            ListView<ActivityPhase> phaseList = view.activity.PhaseList;
            this._elementsGo.Clear();
            this._elementTmpl = Utility.FindChild(node, "Template");
            float height = this._elementTmpl.GetComponent<RectTransform>().rect.height;
            for (int i = 0; i < phaseList.Count; i++)
            {
                GameObject obj2 = null;
                obj2 = (GameObject) UnityEngine.Object.Instantiate(this._elementTmpl);
                if (obj2 != null)
                {
                    obj2.GetComponent<RectTransform>().sizeDelta = this._elementTmpl.GetComponent<RectTransform>().sizeDelta;
                    obj2.transform.SetParent(this._elementTmpl.transform.parent);
                    obj2.transform.localPosition = this._elementTmpl.transform.localPosition + new Vector3(0f, -(height + 5f) * i, 0f);
                    obj2.transform.localScale = Vector3.one;
                    obj2.transform.localRotation = Quaternion.identity;
                    this._elementsGo.Add(i, obj2);
                }
            }
            node.GetComponent<RectTransform>().sizeDelta = new Vector2(node.GetComponent<RectTransform>().sizeDelta.x, (height * phaseList.Count) + ((phaseList.Count - 1) * 5f));
            this._elementTmpl.CustomSetActive(false);
            Singleton<CUIEventManager>.GetInstance().AddUIEventListener(enUIEventID.Activity_Exchange, new CUIEventManager.OnUIEventHandler(this.OnClickExchange));
            view.activity.OnTimeStateChange += new Activity.ActivityEvent(this.OnStateChange);
            this.UpdateAllElements();
        }

        public override void Clear()
        {
            base.view.activity.OnTimeStateChange -= new Activity.ActivityEvent(this.OnStateChange);
            Singleton<CUIEventManager>.GetInstance().RemoveUIEventListener(enUIEventID.Activity_Exchange, new CUIEventManager.OnUIEventHandler(this.OnClickExchange));
            DictionaryView<int, GameObject>.Enumerator enumerator = this._elementsGo.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<int, GameObject> current = enumerator.Current;
                GameObject obj2 = current.Value;
                if (obj2 != null)
                {
                    UnityEngine.Object.Destroy(obj2);
                }
            }
            this._elementsGo.Clear();
            this._elementTmpl = null;
        }

        private void OnClickExchange(CUIEvent uiEvent)
        {
            uint index = (uint) uiEvent.m_eventParams.commonUInt64Param1;
            if ((base.view != null) && (base.view.activity != null))
            {
                ExchangeActivity activity = base.view.activity as ExchangeActivity;
                if (activity != null)
                {
                    activity.ReqDoExchange(index);
                }
            }
        }

        private void OnStateChange(Activity acty)
        {
            this.Validate();
        }

        private void UpdateAllElements()
        {
            ListView<ActivityPhase> phaseList = base.view.activity.PhaseList;
            for (int i = 0; i < phaseList.Count; i++)
            {
                this.UpdateOneElement(i);
            }
        }

        private void UpdateOneElement(int index)
        {
            ListView<ActivityPhase> phaseList = base.view.activity.PhaseList;
            if ((index < phaseList.Count) && (this._elementsGo != null))
            {
                ExchangePhase phase = phaseList[index] as ExchangePhase;
                GameObject obj2 = null;
                bool flag = this._elementsGo.TryGetValue(index, out obj2);
                if ((phase != null) && (obj2 != null))
                {
                    obj2.CustomSetActive(true);
                    ResDT_Item_Info info = null;
                    ResDT_Item_Info info2 = null;
                    ResDT_Item_Info stResItemInfo = null;
                    stResItemInfo = phase.Config.stResItemInfo;
                    if (phase.Config.bColItemCnt > 0)
                    {
                        info = phase.Config.astColItemInfo[0];
                    }
                    if (phase.Config.bColItemCnt > 1)
                    {
                        info2 = phase.Config.astColItemInfo[1];
                    }
                    CUseableContainer useableContainer = Singleton<CRoleInfoManager>.instance.GetMasterRoleInfo().GetUseableContainer(enCONTAINER_TYPE.ITEM);
                    if (useableContainer != null)
                    {
                        int num = (info != null) ? useableContainer.GetUseableStackCount((COM_ITEM_TYPE) info.wItemType, info.dwItemID) : 0;
                        int num2 = (info2 != null) ? useableContainer.GetUseableStackCount((COM_ITEM_TYPE) info2.wItemType, info2.dwItemID) : 0;
                        if (stResItemInfo != null)
                        {
                            GameObject gameObject = obj2.transform.FindChild("DuihuanBtn").gameObject;
                            gameObject.GetComponent<CUIEventScript>().m_onClickEventParams.commonUInt64Param1 = phase.Config.bIdx;
                            if (base.view.activity.timeState == Activity.TimeState.Close)
                            {
                                CUICommonSystem.SetButtonEnable(gameObject.GetComponent<Button>(), false, false, true);
                            }
                            if (info != null)
                            {
                                CUseable useable2 = CUseableManager.CreateUseable((COM_ITEM_TYPE) info.wItemType, info.dwItemID, 1);
                                GameObject obj4 = obj2.transform.FindChild("Panel/ItemCell1").gameObject;
                                CUICommonSystem.SetItemCell(base.view.form.formScript, obj4, useable2, true, false);
                                int useableStackCount = useableContainer.GetUseableStackCount((COM_ITEM_TYPE) info.wItemType, info.dwItemID);
                                ushort wItemCnt = info.wItemCnt;
                                Text component = obj2.transform.FindChild("Panel/ItemCell1/ItemCount").gameObject.GetComponent<Text>();
                                if (useableStackCount < wItemCnt)
                                {
                                    component.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Exchange_ItemNotEnoughCount"), useableStackCount, wItemCnt);
                                    CUICommonSystem.SetButtonEnable(gameObject.GetComponent<Button>(), false, false, true);
                                }
                                else
                                {
                                    component.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Exchange_ItemCount"), useableStackCount, wItemCnt);
                                }
                            }
                            if (info2 != null)
                            {
                                CUseable useable3 = CUseableManager.CreateUseable((COM_ITEM_TYPE) info2.wItemType, info2.dwItemID, 1);
                                GameObject obj5 = obj2.transform.FindChild("Panel/ItemCell2").gameObject;
                                obj5.CustomSetActive(true);
                                CUICommonSystem.SetItemCell(base.view.form.formScript, obj5, useable3, true, false);
                                int num5 = useableContainer.GetUseableStackCount((COM_ITEM_TYPE) info2.wItemType, info2.dwItemID);
                                ushort num6 = info2.wItemCnt;
                                Text text2 = obj2.transform.FindChild("Panel/ItemCell2/ItemCount").gameObject.GetComponent<Text>();
                                if (num5 < num6)
                                {
                                    text2.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Exchange_ItemNotEnoughCount"), num5, num6);
                                    CUICommonSystem.SetButtonEnable(gameObject.GetComponent<Button>(), false, false, true);
                                }
                                else
                                {
                                    text2.text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Exchange_ItemCount"), num5, num6);
                                }
                            }
                            else
                            {
                                obj2.transform.FindChild("Panel/ItemCell2").gameObject.CustomSetActive(false);
                                obj2.transform.FindChild("Panel/Add").gameObject.CustomSetActive(false);
                            }
                            CUseable itemUseable = CUseableManager.CreateUseable((COM_ITEM_TYPE) stResItemInfo.wItemType, stResItemInfo.dwItemID, 1);
                            GameObject itemCell = obj2.transform.FindChild("Panel/GetItemCell").gameObject;
                            CUICommonSystem.SetItemCell(base.view.form.formScript, itemCell, itemUseable, true, false);
                            ExchangeActivity activity = base.view.activity as ExchangeActivity;
                            if (activity != null)
                            {
                                GameObject obj9 = obj2.transform.FindChild("ExchangeCount").gameObject;
                                uint maxExchangeCount = activity.GetMaxExchangeCount(phase.Config.bIdx);
                                uint exchangeCount = activity.GetExchangeCount(phase.Config.bIdx);
                                if (maxExchangeCount > 0)
                                {
                                    obj9.CustomSetActive(true);
                                    obj9.GetComponent<Text>().text = string.Format(Singleton<CTextManager>.GetInstance().GetText("Exchange_TimeLimit"), exchangeCount, maxExchangeCount);
                                    if (exchangeCount >= maxExchangeCount)
                                    {
                                        CUICommonSystem.SetButtonEnable(gameObject.GetComponent<Button>(), false, false, true);
                                    }
                                }
                                else
                                {
                                    obj9.CustomSetActive(false);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Validate()
        {
            this.UpdateAllElements();
        }
    }
}

