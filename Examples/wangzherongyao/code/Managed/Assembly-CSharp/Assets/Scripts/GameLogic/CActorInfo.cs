namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Framework;
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    [Serializable]
    public class CActorInfo : ScriptableObject
    {
        public int Acceleration;
        public string ActorName;
        public ActSound[] ActSounds = new ActSound[0];
        public AnimaSoundElement[] AnimaSound = new AnimaSoundElement[0];
        public string[] ArtLobbyShowLOD = new string[2];
        public string[] ArtPrefabLOD = new string[3];
        public string BgStory;
        public string BtResourcePath;
        public int callMonsterConfigID;
        public CollisionShapeType collisionType;
        public string deadAgePath;
        public int DecelerateDistance;
        public int DyingDialogGroupId;
        public int hudHeight;
        public GameObject hudPrefab;
        public HudCompType HudType;
        public int iBulletHeight = 200;
        public VInt3 iCollisionCenter = VInt3.zero;
        public VInt3 iCollisionSize = new VInt3(400, 400, 400);
        public int IgnoreDistance;
        public string Instruction;
        public float LobbyScale = 1f;
        public string[] LobbySoundBanks = new string[0];
        public int MaxSpeed;
        public int MinDecelerateSpeed;
        public SkillElement[] MySkills = new SkillElement[0];
        public Texture2D PortraitSprite;
        public int ReviveTime;
        public int RotateSpeed;
        public SkinElement[] SkinPrefab = new SkinElement[0];
        public string[] SoundBanks = new string[0];
        public TransformConfig[] TransConfigs = new TransformConfig[2];

        public VCollisionShape CreateCollisionShape()
        {
            DebugHelper.Assert(!Singleton<BattleLogic>.instance.isFighting || Singleton<GameLogic>.instance.bInLogicTick);
            if (this.collisionType == CollisionShapeType.Box)
            {
                return new VCollisionBox { Pos = this.iCollisionCenter, Size = this.iCollisionSize };
            }
            if (this.collisionType == CollisionShapeType.Sphere)
            {
                return new VCollisionSphere { Pos = this.iCollisionCenter, Radius = this.iCollisionSize.x };
            }
            DebugHelper.Assert(false, "初始化碰撞体类型错误");
            return null;
        }

        public static CActorInfo GetActorInfo(string path, enResourceType resourceType)
        {
            CResource resource = Singleton<CResourceManager>.GetInstance().GetResource(path, typeof(CActorInfo), resourceType, false, false);
            if (resource == null)
            {
                return null;
            }
            return (resource.m_content as CActorInfo);
        }

        public string GetArtPrefabName(int skinId = 0, int InLOD = -1)
        {
            int modelLOD = 0;
            if ((InLOD < 0) || (InLOD > 2))
            {
                modelLOD = GameSettings.ModelLOD;
            }
            else
            {
                modelLOD = InLOD;
            }
            if ((Singleton<BattleLogic>.GetInstance().GetCurLvelContext() != null) && !Singleton<BattleLogic>.GetInstance().GetCurLvelContext().isPVPLevel)
            {
                modelLOD--;
            }
            modelLOD = Mathf.Clamp(modelLOD, 0, 2);
            if ((skinId >= 1) && (skinId <= this.SkinPrefab.Length))
            {
                return this.SkinPrefab[skinId - 1].ArtSkinPrefabLOD[modelLOD];
            }
            return this.ArtPrefabLOD[modelLOD];
        }

        public string GetArtPrefabNameLobby(int skinId = 0)
        {
            int num;
            if (GameSettings.ModelLOD == 2)
            {
                num = 1;
            }
            else
            {
                num = 0;
            }
            if ((skinId >= 1) && (skinId <= this.SkinPrefab.Length))
            {
                return this.SkinPrefab[skinId - 1].ArtSkinLobbyShowLOD[num];
            }
            return this.ArtLobbyShowLOD[num];
        }

        public TransformConfig GetTransformConfig(ETransformConfigUsage InUsage)
        {
            DebugHelper.Assert(((this.TransConfigs != null) && (InUsage >= ETransformConfigUsage.NPCInStory)) && (InUsage < this.TransConfigs.Length));
            return this.TransConfigs[(int) InUsage];
        }

        public TransformConfig GetTransformConfigIfHaveOne(ETransformConfigUsage InUsage)
        {
            return (!this.HasTransformConfig(InUsage) ? null : this.TransConfigs[(int) InUsage]);
        }

        public bool HasTransformConfig(ETransformConfigUsage InUsage)
        {
            return ((this.TransConfigs != null) && (InUsage < this.TransConfigs.Length));
        }
    }
}

