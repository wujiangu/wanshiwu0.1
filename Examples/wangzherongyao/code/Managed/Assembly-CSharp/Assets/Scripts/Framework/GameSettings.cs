namespace Assets.Scripts.Framework
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class GameSettings
    {
        private static CastType _castType = CastType.LunPanCast;
        private static bool _EnableMusic = true;
        private static bool _enableOutline;
        private static bool _EnableReplay;
        private static bool _EnableSound = true;
        private static CommonAttactType _normalAttackType;
        private static SelectEnemyType _selectType = SelectEnemyType.SelectLowHp;
        private static CameraHeightType cameraHeight = CameraHeightType.Medium;
        public static int DefaultScreenHeight;
        public static int DefaultScreenWidth;
        public static SGameRenderQuality DeviceLevel = SGameRenderQuality.Low;
        private const string GameSettingCastType = "GameSettings_CastType";
        private const string GameSettingCommonAttackType = "GameSetting_CommonAttackType";
        private const string GameSettingEnableReplay = "GameSetting_EnableReplay";
        private const string GameSettingLunPanSensitivity = "GameSettings_LunPanCastSensitivity";
        private const string GameSettingSelecEnemyType = "GameSettings_SelectEnemyType";
        private const float LunPanMaxAngularSpd = 2f;
        private const float LunPanMaxSpd = 0.02f;
        private const float LunPanMinAngularSpd = 0.2f;
        private const float LunPanMinSpd = 0.2f;
        private static bool m_EnableVoice;
        private static int m_fpsShowType;
        private static int m_joystickMoveType;
        private static int m_joystickShowType;
        private const float MaxAll = 10f;
        public const int maxScreenHeight = 720;
        public const int maxScreenWidth = 0x500;
        public static SGameRenderQuality MaxShadowQuality;
        public static SGameRenderQuality ParticleQuality;
        public static SGameRenderQuality RenderQuality;
        public const string str_cameraHeight = "cameraHeight";
        public const string str_enableMusic = "sgameSettings_muteMusic";
        public const string str_enableSound = "sgameSettings_muteSound";
        public const string str_enableVoice = "sgameSettings_EnableVoice";
        public const string str_fpsShowType = "str_fpsShowType";
        public const string str_joystickMoveType = "joystickMoveType";
        public const string str_joystickShowType = "joystickShowType";
        public const string str_outlineFilter = "sgameSettings_outline";
        public const string str_particleQuality = "sgameSettings_ParticleQuality";
        public const string str_renderQuality = "sgameSettings_RenderQuality";

        static GameSettings()
        {
            LunPanSensitivity = 0f;
        }

        public static void ApplyActorShadowSettings(List<PoolObjHandle<ActorRoot>> actors)
        {
            for (int i = 0; i < actors.Count; i++)
            {
                PoolObjHandle<ActorRoot> handle = actors[i];
                ActorRoot root = handle.handle;
                if (((root != null) && (root.ShadowEffect != null)) && (root.gameObject != null))
                {
                    root.ShadowEffect.ApplyShadowSettings();
                }
            }
        }

        public static void ApplySettings_Music()
        {
            if (_EnableMusic)
            {
                Singleton<CSoundManager>.GetInstance().PostEvent("UnMute_Music", null);
            }
            else
            {
                Singleton<CSoundManager>.GetInstance().PostEvent("Mute_Music", null);
            }
        }

        public static void ApplySettings_Sound()
        {
            if (_EnableSound)
            {
                Singleton<CSoundManager>.GetInstance().PostEvent("UnMute_SFX", null);
            }
            else
            {
                Singleton<CSoundManager>.GetInstance().PostEvent("Mute_SFX", null);
            }
        }

        public static void ApplyShadowSettings()
        {
            if (Singleton<GameObjMgr>.HasInstance())
            {
                ApplyActorShadowSettings(Singleton<GameObjMgr>.instance.GameActors);
            }
        }

        public static void FightStart()
        {
            SendPlayerAttackTargetMode();
            SendPlayerCommonAttackMode();
        }

        public static void GetLunPanSensitivity(out float spd, out float angularSpd)
        {
            if (LunPanSensitivity >= 1f)
            {
                spd = angularSpd = 10f;
            }
            else
            {
                spd = 0.2f + ((1f - LunPanSensitivity) * -0.18f);
                angularSpd = 0.2f + ((1f - LunPanSensitivity) * 1.8f);
            }
        }

        public static void Init()
        {
            DeviceLevel = SGameRenderQuality.Low;
            DeviceLevel = DetectRenderQuality.check_Android();
            if (PlayerPrefs.HasKey("sgameSettings_RenderQuality"))
            {
                RenderQuality = (SGameRenderQuality) Mathf.Clamp(PlayerPrefs.GetInt("sgameSettings_RenderQuality", 0), 0, 2);
            }
            else
            {
                RenderQuality = DeviceLevel;
            }
            if (PlayerPrefs.HasKey("sgameSettings_ParticleQuality"))
            {
                ParticleQuality = (SGameRenderQuality) Mathf.Clamp(PlayerPrefs.GetInt("sgameSettings_ParticleQuality", 0), 0, 2);
            }
            else
            {
                ParticleQuality = RenderQuality;
            }
            EnableSound = PlayerPrefs.GetInt("sgameSettings_muteSound", 1) == 1;
            EnableMusic = PlayerPrefs.GetInt("sgameSettings_muteMusic", 1) == 1;
            if (PlayerPrefs.HasKey("sgameSettings_EnableVoice"))
            {
                EnableVoice = PlayerPrefs.GetInt("sgameSettings_EnableVoice", 1) == 1;
            }
            else
            {
                EnableVoice = false;
            }
            EnableOutline = PlayerPrefs.GetInt("sgameSettings_outline", 0) != 0;
            TheCastType = (CastType) PlayerPrefs.GetInt("GameSettings_CastType", 1);
            TheCommonAttackType = (CommonAttactType) PlayerPrefs.GetInt("GameSetting_CommonAttackType", 0);
            TheSelectType = (SelectEnemyType) PlayerPrefs.GetInt("GameSettings_SelectEnemyType", 1);
            LunPanSensitivity = !PlayerPrefs.HasKey("GameSettings_LunPanCastSensitivity") ? 1f : PlayerPrefs.GetFloat("GameSettings_LunPanCastSensitivity", 1f);
            if (DeviceLevel == SGameRenderQuality.Low)
            {
                cameraHeight = CameraHeightType.Low;
            }
            else
            {
                cameraHeight = CameraHeightType.Medium;
            }
            if (PlayerPrefs.HasKey("cameraHeight"))
            {
                CameraHeight = PlayerPrefs.GetInt("cameraHeight", 1);
            }
            JoyStickMoveType = PlayerPrefs.GetInt("joystickMoveType", 1);
            JoyStickShowType = PlayerPrefs.GetInt("joystickShowType", 0);
            FpsShowType = PlayerPrefs.GetInt("str_fpsShowType", 0);
        }

        public static void RefreshResolution()
        {
            if ((DefaultScreenWidth == 0) || (DefaultScreenHeight == 0))
            {
                DefaultScreenWidth = Screen.width;
                DefaultScreenHeight = Screen.height;
            }
            int a = Mathf.Max(DefaultScreenWidth, DefaultScreenHeight);
            int b = Mathf.Min(DefaultScreenWidth, DefaultScreenHeight);
            DefaultScreenWidth = a;
            DefaultScreenHeight = b;
            if (ShouldReduceResolution())
            {
                a = 0x500;
                b = (a * DefaultScreenHeight) / DefaultScreenWidth;
            }
            if ((a != Screen.width) || (b != Screen.height))
            {
                Screen.SetResolution(Mathf.Max(a, b), Mathf.Min(a, b), true);
            }
        }

        public static void Save()
        {
            PlayerPrefs.SetInt("sgameSettings_muteSound", !EnableSound ? 0 : 1);
            PlayerPrefs.SetInt("sgameSettings_muteMusic", !EnableMusic ? 0 : 1);
            PlayerPrefs.SetInt("sgameSettings_RenderQuality", (int) RenderQuality);
            PlayerPrefs.SetInt("sgameSettings_ParticleQuality", (int) ParticleQuality);
            PlayerPrefs.SetInt("sgameSettings_outline", !EnableOutline ? 0 : 1);
            PlayerPrefs.SetInt("sgameSettings_EnableVoice", !EnableVoice ? 0 : 1);
            PlayerPrefs.SetInt("GameSettings_CastType", (int) TheCastType);
            PlayerPrefs.SetInt("GameSetting_CommonAttackType", (int) TheCommonAttackType);
            PlayerPrefs.SetInt("GameSettings_SelectEnemyType", (int) TheSelectType);
            PlayerPrefs.SetFloat("GameSettings_LunPanCastSensitivity", LunPanSensitivity);
            PlayerPrefs.SetInt("cameraHeight", (int) cameraHeight);
            PlayerPrefs.SetInt("joystickMoveType", m_joystickMoveType);
            PlayerPrefs.SetInt("joystickShowType", m_joystickShowType);
            PlayerPrefs.SetInt("str_fpsShowType", m_fpsShowType);
            PlayerPrefs.Save();
        }

        private static void SendPlayerAttackTargetMode()
        {
            FrameCommand<PlayAttackTargetModeCommand> command = FrameCommandFactory.CreateFrameCommand<PlayAttackTargetModeCommand>();
            command.cmdData.AttackTargetMode = (sbyte) _selectType;
            command.Send();
        }

        private static void SendPlayerCommonAttackMode()
        {
            FrameCommand<PlayCommonAttackModeCommand> command = FrameCommandFactory.CreateFrameCommand<PlayCommonAttackModeCommand>();
            command.cmdData.CommonAttackMode = (byte) _normalAttackType;
            command.Send();
        }

        public static bool ShouldReduceResolution()
        {
            int num = (DefaultScreenWidth <= DefaultScreenHeight) ? DefaultScreenHeight : DefaultScreenWidth;
            int num2 = (DefaultScreenWidth <= DefaultScreenHeight) ? DefaultScreenWidth : DefaultScreenHeight;
            return ((num >= 0x500) || (num2 >= 720));
        }

        public static bool supportOutline()
        {
            int num = (Screen.width <= Screen.height) ? Screen.height : Screen.width;
            int num2 = (Screen.width <= Screen.height) ? Screen.width : Screen.height;
            return (((num >= 960) && (num2 >= 540)) && (DeviceLevel != SGameRenderQuality.Low));
        }

        public static bool AllowOutlineFilter
        {
            get
            {
                if (!EnableOutline)
                {
                    return false;
                }
                return supportOutline();
            }
        }

        public static bool AllowRadialBlur
        {
            get
            {
                return (DeviceLevel != SGameRenderQuality.Low);
            }
        }

        public static int CameraHeight
        {
            get
            {
                return (int) cameraHeight;
            }
            set
            {
                cameraHeight = (CameraHeightType) Mathf.Clamp(value, 0, 1);
                Singleton<GameEventSys>.instance.SendEvent(GameEventDef.Event_CameraHeightChange);
            }
        }

        public static float CameraHeightRateValue
        {
            get
            {
                if ((cameraHeight != CameraHeightType.Low) && (cameraHeight == CameraHeightType.Medium))
                {
                    return 1.2f;
                }
                return 1f;
            }
        }

        public static bool EnableMusic
        {
            get
            {
                return _EnableMusic;
            }
            set
            {
                _EnableMusic = value;
                ApplySettings_Music();
            }
        }

        public static bool EnableOutline
        {
            get
            {
                return _enableOutline;
            }
            set
            {
                if (_enableOutline != value)
                {
                    if ((Singleton<GameStateCtrl>.HasInstance() && Singleton<GameStateCtrl>.GetInstance().isBattleState) && supportOutline())
                    {
                        if (value)
                        {
                            OutlineFilter.EnableOutlineFilter();
                        }
                        else
                        {
                            OutlineFilter.DisableOutlineFilter();
                        }
                    }
                    _enableOutline = value;
                }
            }
        }

        public static bool enableReplay
        {
            get
            {
                return _EnableReplay;
            }
            set
            {
                if (_EnableReplay != value)
                {
                    _EnableReplay = value;
                }
            }
        }

        public static bool EnableSound
        {
            get
            {
                return _EnableSound;
            }
            set
            {
                _EnableSound = value;
                ApplySettings_Sound();
            }
        }

        public static bool EnableVoice
        {
            get
            {
                return m_EnableVoice;
            }
            set
            {
                m_EnableVoice = value;
                MonoSingleton<VoiceSys>.GetInstance().IsUseVoiceSysSetting = m_EnableVoice;
            }
        }

        public static int FpsShowType
        {
            get
            {
                return m_fpsShowType;
            }
            set
            {
                m_fpsShowType = value;
                if (Singleton<CBattleSystem>.GetInstance().m_FormScript != null)
                {
                    Singleton<CBattleSystem>.GetInstance().SetFpsShowType(m_fpsShowType);
                }
            }
        }

        public static bool IsHighQuality
        {
            get
            {
                return (RenderQuality == SGameRenderQuality.High);
            }
        }

        public static int JoyStickMoveType
        {
            get
            {
                return 1;
            }
            set
            {
                m_joystickMoveType = 1;
                if (Singleton<CBattleSystem>.GetInstance().m_FormScript != null)
                {
                    Singleton<CBattleSystem>.GetInstance().SetJoyStickMoveType(m_joystickMoveType);
                }
            }
        }

        public static int JoyStickShowType
        {
            get
            {
                return m_joystickShowType;
            }
            set
            {
                m_joystickShowType = value;
                if (Singleton<CBattleSystem>.GetInstance().m_FormScript != null)
                {
                    Singleton<CBattleSystem>.GetInstance().SetJoyStickShowType(m_joystickShowType);
                }
            }
        }

        public static float LunPanSensitivity
        {
            [CompilerGenerated]
            get
            {
                return <LunPanSensitivity>k__BackingField;
            }
            [CompilerGenerated]
            set
            {
                <LunPanSensitivity>k__BackingField = value;
            }
        }

        public static int ModelLOD
        {
            get
            {
                return (int) RenderQuality;
            }
            set
            {
                RenderQuality = (SGameRenderQuality) Mathf.Clamp(value, 0, 2);
            }
        }

        public static int ParticleLOD
        {
            get
            {
                return (int) ParticleQuality;
            }
            set
            {
                ParticleQuality = (SGameRenderQuality) Mathf.Clamp(value, 0, 2);
            }
        }

        public static SGameRenderQuality ShadowQuality
        {
            get
            {
                return (SGameRenderQuality) Mathf.Max((int) MaxShadowQuality, ModelLOD);
            }
            set
            {
                SGameRenderQuality shadowQuality = ShadowQuality;
                MaxShadowQuality = value;
                if (shadowQuality != MaxShadowQuality)
                {
                    ApplyShadowSettings();
                }
            }
        }

        public static CastType TheCastType
        {
            get
            {
                return _castType;
            }
            set
            {
                _castType = value;
                if (Singleton<GameInput>.instance != null)
                {
                    Singleton<GameInput>.instance.SetSmartUse(_castType != CastType.LunPanCast);
                }
            }
        }

        public static CommonAttactType TheCommonAttackType
        {
            get
            {
                return _normalAttackType;
            }
            set
            {
                _normalAttackType = value;
                SendPlayerCommonAttackMode();
            }
        }

        public static SelectEnemyType TheSelectType
        {
            get
            {
                return _selectType;
            }
            set
            {
                _selectType = value;
                SendPlayerAttackTargetMode();
            }
        }
    }
}

