namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic.DataCenter;
    using Assets.Scripts.GameLogic.GameKernal;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public static class ActorHelper
    {
        public static PoolObjHandle<ActorRoot> AttachActorRoot(GameObject go, bool isFirstTime = true)
        {
            DebugHelper.Assert(!Singleton<BattleLogic>.instance.isFighting || Singleton<GameLogic>.instance.bInLogicTick);
            ActorConfig component = null;
            if (isFirstTime)
            {
                component = go.AddComponent<ActorConfig>();
            }
            else
            {
                component = go.GetComponent<ActorConfig>();
                if (null == component)
                {
                    component = go.AddComponent<ActorConfig>();
                }
            }
            ActorMeta theActorMeta = new ActorMeta {
                ActorType = ActorTypeDef.Invalid
            };
            PoolObjHandle<ActorRoot> handle = component.AttachActorRoot(go, ref theActorMeta, null);
            handle.handle.Spawned();
            return handle;
        }

        public static PoolObjHandle<ActorRoot> AttachActorRoot(GameObject go, ActorTypeDef actorType, bool isFirstTime = true)
        {
            DebugHelper.Assert(!Singleton<BattleLogic>.instance.isFighting || Singleton<GameLogic>.instance.bInLogicTick);
            ActorConfig component = null;
            if (isFirstTime)
            {
                component = go.AddComponent<ActorConfig>();
            }
            else
            {
                component = go.GetComponent<ActorConfig>();
                if (null == component)
                {
                    component = go.AddComponent<ActorConfig>();
                }
            }
            ActorMeta theActorMeta = new ActorMeta {
                ActorType = actorType
            };
            PoolObjHandle<ActorRoot> handle = component.AttachActorRoot(go, ref theActorMeta, null);
            handle.handle.Spawned();
            return handle;
        }

        public static void DetachActorRoot(GameObject go)
        {
            ActorConfig component = go.GetComponent<ActorConfig>();
            if (component != null)
            {
                component.DetachActorRoot();
            }
        }

        public static List<PoolObjHandle<ActorRoot>> FilterActors(List<PoolObjHandle<ActorRoot>> actorList, ActorFilterDelegate filter)
        {
            List<PoolObjHandle<ActorRoot>> list = new List<PoolObjHandle<ActorRoot>>();
            int count = actorList.Count;
            for (int i = 0; i < count; i++)
            {
                PoolObjHandle<ActorRoot> actor = actorList[i];
                if ((filter == null) || filter(ref actor))
                {
                    list.Add(actor);
                }
            }
            return list;
        }

        public static PoolObjHandle<ActorRoot> GetActorRoot(GameObject go)
        {
            if (go != null)
            {
                ActorConfig component = go.GetComponent<ActorConfig>();
                if (component != null)
                {
                    return component.GetActorHandle();
                }
            }
            return new PoolObjHandle<ActorRoot>(null);
        }

        public static Player GetOwnerPlayer(ref PoolObjHandle<ActorRoot> actor)
        {
            return Singleton<GamePlayerCenter>.instance.GetPlayer(actor.handle.TheActorMeta.PlayerId);
        }

        public static OperateMode GetPlayerOperateMode(ref PoolObjHandle<ActorRoot> actor)
        {
            return Singleton<GamePlayerCenter>.instance.GetPlayer(actor.handle.TheActorMeta.PlayerId).GetOperateMode();
        }

        public static bool IsCaptainActor(ref PoolObjHandle<ActorRoot> actor)
        {
            Player player = Singleton<GamePlayerCenter>.instance.GetPlayer(actor.handle.TheActorMeta.PlayerId);
            return ((player != null) && (player.Captain == actor));
        }

        public static bool IsHostActor(ref PoolObjHandle<ActorRoot> actor)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
            return ((hostPlayer != null) && hostPlayer.IsAtMyTeam(ref actor.handle.TheActorMeta));
        }

        public static bool IsHostCampActor(ref PoolObjHandle<ActorRoot> actor)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
            return (actor.handle.TheActorMeta.ActorCamp == hostPlayer.PlayerCamp);
        }

        public static bool IsHostCtrlActor(ref PoolObjHandle<ActorRoot> actor)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
            return ((hostPlayer != null) && (hostPlayer.Captain == actor));
        }

        public static bool IsHostEnemyActor(ref PoolObjHandle<ActorRoot> actor)
        {
            Player hostPlayer = Singleton<GamePlayerCenter>.instance.GetHostPlayer();
            return ((hostPlayer != null) && (actor.handle.TheActorMeta.ActorCamp != hostPlayer.PlayerCamp));
        }
    }
}

