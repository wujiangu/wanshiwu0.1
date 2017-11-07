using Assets.Scripts.Common;
using Assets.Scripts.Framework;
using Assets.Scripts.GameLogic;
using Assets.Scripts.GameLogic.GameKernal;
using System;
using UnityEngine;

public class CameraSystem : MonoSingleton<CameraSystem>
{
    protected bool bFreeCamera;
    protected bool bFreeRotate;
    protected Plane[] CachedFrustum;
    public Moba_Camera MobaCamera;
    private static float s_CameraMoveScale = 0.02f;

    public bool CheckVisiblity(Bounds InBounds)
    {
        if (this.CachedFrustum != null)
        {
            return GeometryUtility.TestPlanesAABB(this.CachedFrustum, InBounds);
        }
        return true;
    }

    public void MoveCamera(float offX, float offY)
    {
        if (this.MobaCamera != null)
        {
            SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
            if ((curLvelContext != null) && curLvelContext.bCameraFlip)
            {
                offX = -offX;
                offY = -offY;
            }
            this.MobaCamera.settings.absoluteLockLocation.x += offX * s_CameraMoveScale;
            this.MobaCamera.settings.absoluteLockLocation.z += offY * s_CameraMoveScale;
        }
    }

    private void OnCameraHeightChanged()
    {
        if (this.MobaCamera != null)
        {
            this.MobaCamera.currentZoomRate = GameSettings.CameraHeightRateValue;
            this.MobaCamera.CameraUpdate();
        }
    }

    private void OnFocusSwitched(ref DefaultGameEventParam prm)
    {
        if ((prm.src != 0) && ActorHelper.IsHostCtrlActor(ref prm.src))
        {
            this.SetFocusActor(prm.src);
            if (!prm.src.handle.ActorControl.IsDeadState && !this.bFreeCamera)
            {
                this.enableLockedCamera = true;
                this.enableAbsoluteLocationLockCamera = false;
            }
        }
    }

    private void OnPlayerDead(ref DefaultGameEventParam prm)
    {
        if (((prm.src != 0) && ActorHelper.IsHostCtrlActor(ref prm.src)) && !this.bFreeCamera)
        {
            if ((this.MobaCamera != null) && (prm.src.handle.ActorControl != null))
            {
                this.MobaCamera.SetAbsoluteLockLocation((Vector3) prm.src.handle.ActorControl.actorLocation);
            }
            Singleton<CBattleSystem>.GetInstance().StartCameraDrag();
            this.enableLockedCamera = false;
            this.enableAbsoluteLocationLockCamera = true;
            if (this.MobaCamera != null)
            {
                this.MobaCamera._lockTransitionRate = 1f;
            }
        }
    }

    private void OnPlayerRevive(ref DefaultGameEventParam prm)
    {
        if (((prm.src != 0) && ActorHelper.IsHostCtrlActor(ref prm.src)) && !this.bFreeCamera)
        {
            this.enableLockedCamera = true;
            this.enableAbsoluteLocationLockCamera = false;
            Singleton<CBattleSystem>.GetInstance().EndCameraDrag();
        }
    }

    public void PrepareFight()
    {
        Player hostPlayer = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
        DebugHelper.Assert(hostPlayer != null, "local player is null in CameraSystem.PerpareFight", null);
        PoolObjHandle<ActorRoot> focus = (hostPlayer == null) ? new PoolObjHandle<ActorRoot>() : hostPlayer.Captain;
        this.SetFocusActor(focus);
        if (this.MobaCamera != null)
        {
            this.MobaCamera.currentZoomRate = GameSettings.CameraHeightRateValue;
            this.MobaCamera.CameraUpdate();
        }
        Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_CaptainSwitch, new RefAction<DefaultGameEventParam>(this.OnFocusSwitched));
        Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorDead, new RefAction<DefaultGameEventParam>(this.OnPlayerDead));
        Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorRevive, new RefAction<DefaultGameEventParam>(this.OnPlayerRevive));
        Singleton<GameEventSys>.instance.RmvEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorMoveCity, new RefAction<DefaultGameEventParam>(this.OnPlayerRevive));
        Singleton<GameEventSys>.instance.RmvEventHandler(GameEventDef.Event_CameraHeightChange, new Action(this, (IntPtr) this.OnCameraHeightChanged));
        Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_CaptainSwitch, new RefAction<DefaultGameEventParam>(this.OnFocusSwitched));
        Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorDead, new RefAction<DefaultGameEventParam>(this.OnPlayerDead));
        Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorRevive, new RefAction<DefaultGameEventParam>(this.OnPlayerRevive));
        Singleton<GameEventSys>.instance.AddEventHandler<DefaultGameEventParam>(GameEventDef.Event_ActorMoveCity, new RefAction<DefaultGameEventParam>(this.OnPlayerRevive));
        Singleton<GameEventSys>.instance.AddEventHandler(GameEventDef.Event_CameraHeightChange, new Action(this, (IntPtr) this.OnCameraHeightChanged));
    }

    private void SetCameraLerp(int timerSequence)
    {
        if (this.MobaCamera != null)
        {
        }
    }

    public void SetFocusActor(PoolObjHandle<ActorRoot> focus)
    {
        if (this.MobaCamera == null)
        {
            GameObject obj2 = GameObject.Find("MainCamera");
            if (obj2 != null)
            {
                this.MobaCamera = obj2.GetComponent<Moba_Camera>();
                SLevelContext curLvelContext = Singleton<BattleLogic>.instance.GetCurLvelContext();
                if ((curLvelContext != null) && curLvelContext.bCameraFlip)
                {
                    this.MobaCamera.settings.rotation.defualtRotation = new Vector2(this.MobaCamera.settings.rotation.defualtRotation.x, 180f);
                    this.MobaCamera.currentCameraRotation = (Vector3) this.MobaCamera.settings.rotation.defualtRotation;
                }
                this.MobaCamera.currentZoomRate = GameSettings.CameraHeightRateValue;
            }
        }
        if (this.MobaCamera != null)
        {
            this.MobaCamera.SetTargetTransform(focus);
            this.MobaCamera.SetCameraLocked(true);
        }
        Singleton<CBattleSystem>.GetInstance().EndCameraDrag();
    }

    private void Start()
    {
    }

    public void ToggleFreeCamera()
    {
        this.bFreeCamera = !this.bFreeCamera;
        this.enableLockedCamera = !this.bFreeCamera;
        this.enableAbsoluteLocationLockCamera = false;
    }

    public void ToggleFreeDragCamera(bool bFree)
    {
        this.enableLockedCamera = !bFree;
        this.enableAbsoluteLocationLockCamera = bFree;
    }

    public void ToggleRotate()
    {
        this.bFreeRotate = !this.bFreeRotate;
        this.MobaCamera.lockRotateX = !this.bFreeRotate;
        this.MobaCamera.lockRotateY = !this.bFreeRotate;
    }

    private void Update()
    {
        this.CachedFrustum = (this.MobaCamera == null) ? null : this.MobaCamera.frustum;
    }

    protected bool enableAbsoluteLocationLockCamera
    {
        get
        {
            return ((this.MobaCamera != null) && this.MobaCamera.GetAbsoluteLocked());
        }
        set
        {
            if (this.MobaCamera != null)
            {
                this.MobaCamera.SetAbsoluteLocked(value);
            }
        }
    }

    protected bool enableLockedCamera
    {
        get
        {
            return ((this.MobaCamera != null) && this.MobaCamera.GetCameraLocked());
        }
        set
        {
            if (this.MobaCamera != null)
            {
                this.MobaCamera.SetCameraLocked(value);
            }
        }
    }
}

