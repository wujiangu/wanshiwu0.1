namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using CSProtocol;
    using ResData;
    using System;
    using System.Runtime.InteropServices;

    [MessageHandlerClass]
    public class TaskNetUT
    {
        [MessageHandler(0x5dd)]
        public static void OnDailyTaskRes(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            Singleton<CTaskSys>.instance.OnUSUALTASK_RES(ref msg);
        }

        [MessageHandler(0x643)]
        public static void OnHuoyue_Error_NTF(CSPkg msg)
        {
            Singleton<CTaskSys>.instance.OnHuoyue_Error_NTF(ref msg);
        }

        [MessageHandler(0x640)]
        public static void OnHuoyue_Info_NTF(CSPkg msg)
        {
            Singleton<CTaskSys>.instance.OnHuoyue_Info_NTF(ref msg);
        }

        [MessageHandler(0x642)]
        public static void OnHuoyue_Reward_RES(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            Singleton<CTaskSys>.instance.OnHuoyue_Reward_RES(ref msg);
        }

        [MessageHandler(0xa34)]
        public static void OnMonthWeekCardUseRsp(CSPkg pkg)
        {
            SCPKG_MONTH_WEEK_CARD_USE_RSP stMonthWeekCardUseRsp = pkg.stPkgData.stMonthWeekCardUseRsp;
            RES_PROP_VALFUNC_TYPE bCardType = (RES_PROP_VALFUNC_TYPE) stMonthWeekCardUseRsp.bCardType;
            Singleton<CTaskSys>.instance.SetCardExpireTime(bCardType, stMonthWeekCardUseRsp.dwExpireTime);
            Singleton<CUIManager>.GetInstance().OpenTips(Singleton<CTextManager>.GetInstance().GetText((bCardType != RES_PROP_VALFUNC_TYPE.RES_PROP_VALFUNC_MONTH_CARD) ? "WeekCardTakeEffect" : "MonthCardTakeEffect"), false, 2f, null, new object[0]);
        }

        [MessageHandler(0x5e3)]
        public static void OnSCID_NEWTASKGET_NTF(CSPkg msg)
        {
            Singleton<CTaskSys>.instance.OnSCID_NEWTASKGET_NTF(ref msg);
        }

        [MessageHandler(0x5df)]
        public static void OnTASKSUBMIT_RES(CSPkg msg)
        {
            Singleton<CUIManager>.GetInstance().CloseSendMsgAlert();
            Singleton<CTaskSys>.instance.OnTASKSUBMIT_RES(ref msg);
        }

        [MessageHandler(0x5e0)]
        public static void OnTaskUpdate(CSPkg msg)
        {
            Singleton<CTaskSys>.instance.On_TASKUPD_NTF(ref msg);
        }

        public static void Send_GetHuoyue_Reward(ushort id)
        {
            if (id != 0)
            {
                CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x641);
                msg.stPkgData.stHuoYueDuRewardReq.wHuoYueDuId = id;
                Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
            }
        }

        public static void Send_SubmitTask(uint taskid)
        {
            if (taskid != 0)
            {
                CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x5de);
                msg.stPkgData.stSubmitTaskReq.dwTaskID = taskid;
                Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, false);
            }
        }

        public static void Send_Update_Task(uint taskid = 0)
        {
            CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x5dc);
            msg.stPkgData.stUsualTaskReq.bRefreshMethod = (taskid != 0) ? ((byte) 1) : ((byte) 0);
            msg.stPkgData.stUsualTaskReq.dwTaskID = taskid;
            Singleton<NetworkModule>.GetInstance().SendLobbyMsg(ref msg, taskid != 0);
        }
    }
}

