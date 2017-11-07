namespace Assets.Scripts.Framework
{
    using AGE;
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic;
    using Assets.Scripts.GameSystem;
    using CSProtocol;
    using Pathfinding.RVO;
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;

    public class GameLogic : Singleton<GameLogic>, IGameModule
    {
        private bool bHasTailPart;
        public bool bInLogicTick;
        public uint GameRunningTick;
        private int nTailPartDelta;
        private BattleLogic optBattle;
        private DropItemMgr optDropMgr;
        private LobbyLogic optLobby;
        private GameObjMgr optObjMgr;
        private CRoleInfoManager optRoleMgr;

        public void ClearLogicData()
        {
            Singleton<CRoleInfoManager>.instance.ClearMasterRoleInfo();
            Singleton<CAdventureSys>.GetInstance().Clear();
            Singleton<CMatchingSystem>.GetInstance().Clear();
            Singleton<CRoomSystem>.GetInstance().Clear();
            Singleton<CSymbolSystem>.GetInstance().Clear();
            Singleton<ActivitySys>.GetInstance().Clear();
            Singleton<CFriendContoller>.instance.ClearAll();
            Singleton<CChatController>.instance.ClearAll();
            Singleton<BurnExpeditionController>.instance.ClearAll();
            if (MonoSingleton<NewbieGuideManager>.HasInstance())
            {
                MonoSingleton<NewbieGuideManager>.instance.StopCurrentGuide();
                MonoSingleton<NewbieGuideManager>.ClearDestroy();
            }
            Singleton<CMailSys>.instance.Clear();
            Singleton<CTaskSys>.instance.Clear();
            Singleton<CGuildSystem>.GetInstance().Clear();
            GameDataMgr.ClearServerResData();
            Singleton<CMallFactoryShopController>.GetInstance().Clear();
            Singleton<CMallMysteryShop>.GetInstance().Clear();
            Singleton<RankingSystem>.GetInstance().ClearAll();
            Singleton<LobbyUISys>.GetInstance().Clear();
            Singleton<CUnionBattleRankSystem>.GetInstance().Clear();
            Singleton<HeadIconSys>.instance.Clear();
            Singleton<CLoudSpeakerSys>.instance.Clear();
        }

        public override void Init()
        {
            this.optObjMgr = Singleton<GameObjMgr>.GetInstance();
            this.optLobby = Singleton<LobbyLogic>.GetInstance();
            this.optBattle = Singleton<BattleLogic>.GetInstance();
            Singleton<GameInput>.GetInstance();
            this.optDropMgr = Singleton<DropItemMgr>.GetInstance();
            this.optRoleMgr = Singleton<CRoleInfoManager>.GetInstance();
        }

        public void LateUpdate()
        {
            this.optObjMgr.LateUpdate();
        }

        public void OnPlayerLogout()
        {
            Singleton<NetworkModule>.GetInstance().CloseAllServerConnect();
            this.ClearLogicData();
        }

        public void OpenLobby()
        {
            Singleton<LobbyLogic>.GetInstance().OpenLobby();
        }

        private void SampleFrameSyncData()
        {
            if ((Singleton<FrameSynchr>.instance.bActive && ((Singleton<FrameSynchr>.instance.CurFrameNum % 500) == 0)) && Singleton<BattleLogic>.instance.isFighting)
            {
                List<PoolObjHandle<ActorRoot>> heroActors = Singleton<GameObjMgr>.instance.HeroActors;
                int num = 1 + (heroActors.Count * 5);
                int[] src = new int[num];
                int num2 = 0;
                src[num2++] = (int) Singleton<FrameSynchr>.instance.CurFrameNum;
                for (int i = 0; i < heroActors.Count; i++)
                {
                    PoolObjHandle<ActorRoot> handle = heroActors[i];
                    ActorRoot root = handle.handle;
                    src[num2++] = (int) root.ObjID;
                    src[num2++] = root.location.x;
                    src[num2++] = root.location.y;
                    src[num2++] = root.location.z;
                    src[num2++] = (int) root.ActorControl.myBehavior;
                }
                byte[] dst = new byte[num * 4];
                Buffer.BlockCopy(src, 0, dst, 0, dst.Length);
                MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
                provider.Initialize();
                provider.TransformFinalBlock(dst, 0, dst.Length);
                ulong num4 = (ulong) BitConverter.ToInt64(provider.get_Hash(), 0);
                ulong num5 = (ulong) BitConverter.ToInt64(provider.get_Hash(), 8);
                ulong num6 = num4 ^ num5;
                CSPkg msg = NetworkModule.CreateDefaultCSPKG(0x500);
                msg.stPkgData.stRelayHashChk.dwKFrapsNo = Singleton<FrameSynchr>.instance.CurFrameNum;
                msg.stPkgData.stRelayHashChk.ullHashToChk = num6;
                Singleton<NetworkModule>.instance.SendGameMsg(ref msg, 0);
            }
        }

        public void UpdateFrame()
        {
        }

        public void UpdateLogic(int nDelta, bool bPart)
        {
            this.GameRunningTick += (uint) nDelta;
            DebugHelper.Assert(!this.bHasTailPart);
            this.UpdateLogicPartA(nDelta);
            this.bHasTailPart = true;
            this.nTailPartDelta = nDelta;
            if (!bPart)
            {
                this.UpdateTails();
            }
        }

        private void UpdateLogicPartA(int nDelta)
        {
            this.bInLogicTick = true;
            Singleton<GameEventSys>.instance.UpdateEvent();
            ActionManager.Instance.UpdateLogic(nDelta);
            if (MTileHandlerHelper.Instance != null)
            {
                MTileHandlerHelper.Instance.UpdateLogic();
            }
            if (RVOSimulator.Instance != null)
            {
                RVOSimulator.Instance.UpdateLogic(nDelta);
            }
            this.optLobby.UpdateLogic(nDelta);
            this.optBattle.UpdateLogic(nDelta);
            this.bInLogicTick = false;
        }

        private void UpdateLogicPartB(int nDelta)
        {
            this.bInLogicTick = true;
            this.optObjMgr.UpdateLogic(nDelta);
            this.optDropMgr.UpdateLogic(nDelta);
            this.optRoleMgr.UpdateLogic(nDelta);
            if (Singleton<ShenFuSystem>.instance != null)
            {
                Singleton<ShenFuSystem>.instance.UpdateLogic(nDelta);
            }
            Singleton<CTimerManager>.instance.UpdateLogic(nDelta);
            this.SampleFrameSyncData();
            this.bInLogicTick = false;
        }

        public bool UpdateTails()
        {
            if (!this.bHasTailPart)
            {
                return false;
            }
            try
            {
                this.UpdateLogicPartB(this.nTailPartDelta);
                this.bHasTailPart = false;
            }
            catch (Exception)
            {
                this.bHasTailPart = false;
            }
            return true;
        }
    }
}

