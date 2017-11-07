namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using CSProtocol;
    using System;
    using System.Collections.Generic;

    public class Horizon : IUpdateLogic
    {
        private bool _enabled = false;
        private bool _fighting = false;
        private uint _globalSight = 0x186a0;
        public const byte UPDATE_CYCLE = 8;

        public void FightOver()
        {
            this.Enabled = false;
        }

        public void FightStart()
        {
            this._fighting = true;
            this._globalSight = GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x38).dwConfValue;
            this._enabled = Singleton<BattleLogic>.instance.GetCurLvelContext().horizonEnableMethod == EnableMethod.EnableAll;
        }

        public void UpdateLogic(int delta)
        {
            if (this._enabled && this._fighting)
            {
                uint num = Singleton<FrameSynchr>.instance.CurFrameNum % 8;
                List<PoolObjHandle<ActorRoot>> gameActors = Singleton<GameObjMgr>.GetInstance().GameActors;
                int count = gameActors.Count;
                for (int i = 0; i < count; i++)
                {
                    PoolObjHandle<ActorRoot> handle = gameActors[i];
                    ActorRoot root = handle.handle;
                    if (((root.ObjID % 8) == num) && !root.ActorControl.IsDeadState)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (j != root.TheActorMeta.ActorCamp)
                            {
                                COM_PLAYERCAMP actorCamp = root.TheActorMeta.ActorCamp;
                                List<PoolObjHandle<ActorRoot>> campActors = Singleton<GameObjMgr>.GetInstance().GetCampActors((COM_PLAYERCAMP) j);
                                int num5 = campActors.Count;
                                for (int k = 0; k < num5; k++)
                                {
                                    PoolObjHandle<ActorRoot> handle2 = campActors[k];
                                    ActorRoot root2 = handle2.handle;
                                    if (!root2.HorizonMarker.IsSightVisited(actorCamp))
                                    {
                                        long sightRadius;
                                        if (root.HorizonMarker.SightRadius != 0)
                                        {
                                            sightRadius = root.HorizonMarker.SightRadius;
                                        }
                                        else
                                        {
                                            sightRadius = this._globalSight;
                                        }
                                        VInt3 num8 = root2.location - root.location;
                                        if (num8.sqrMagnitudeLong2D < (sightRadius * sightRadius))
                                        {
                                            root2.HorizonMarker.VisitSight(actorCamp);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool Enabled
        {
            get
            {
                return this._enabled;
            }
            set
            {
                if (value != this._enabled)
                {
                    this._enabled = value;
                    List<PoolObjHandle<ActorRoot>> gameActors = Singleton<GameObjMgr>.GetInstance().GameActors;
                    int count = gameActors.Count;
                    for (int i = 0; i < count; i++)
                    {
                        PoolObjHandle<ActorRoot> handle = gameActors[i];
                        handle.handle.HorizonMarker.Enabled = this._enabled;
                    }
                }
            }
        }

        public enum EnableMethod
        {
            DisableAll,
            EnableAll,
            EnableMarkNoMist,
            INVALID
        }
    }
}

