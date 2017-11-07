using Assets.Scripts.Common;
using Assets.Scripts.Framework;
using Assets.Scripts.GameLogic;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class FogOfWar
{
    private static byte[] _bitmapData = null;
    private static byte[] _bitmapDataTimer = null;
    public static int _BitmapHeight = 0x200;
    private static Texture2D _bitmapTexture = null;
    public static int _BitmapWidth = 0x200;
    private static bool _enable = false;
    private static int _MapHeight = 0x186a0;
    private static int _MapWidth = 0x186a0;
    private static byte[] _maskData = null;
    public const int BITMAP_SIZE = 0x80;
    public static readonly byte FOG_DELAY_FRAME = 90;
    private static int maskRadiusInPixel = 0;
    private static int maskSize = 0;
    private static List<ACTOR_POS> s_positionsOfSelfCompaign = new List<ACTOR_POS>();

    private static void AddSightAtPoint(int posX, int posY)
    {
        int num = ((_BitmapWidth * posX) / _MapWidth) + ((int) (0.5f * _BitmapWidth));
        int num2 = ((_BitmapHeight * posY) / _MapHeight) + ((int) (0.5f * _BitmapHeight));
        for (int i = Mathf.Max(num2 - maskRadiusInPixel, 0); i <= Mathf.Min((int) (_BitmapHeight - 1), (int) (num2 + maskRadiusInPixel)); i++)
        {
            for (int j = Mathf.Max(num - maskRadiusInPixel, 0); j <= Mathf.Min((int) (_BitmapWidth - 1), (int) (num + maskRadiusInPixel)); j++)
            {
                int num5 = maskRadiusInPixel + (j - num);
                int num6 = maskRadiusInPixel + (i - num2);
                byte num7 = _maskData[(num6 * maskSize) + num5];
                _bitmapData[(i * _BitmapWidth) + j] = (byte) Mathf.Max((int) _bitmapData[(i * _BitmapWidth) + j], (int) num7);
                if (num7 > 0)
                {
                    _bitmapDataTimer[(i * _BitmapWidth) + j] = FOG_DELAY_FRAME;
                }
            }
        }
    }

    public static void BeginLevel()
    {
        if ((Singleton<BattleLogic>.instance.GetCurLvelContext() != null) && (Singleton<BattleLogic>.instance.GetCurLvelContext().horizonEnableMethod == Horizon.EnableMethod.EnableAll))
        {
        }
        Reset(Singleton<BattleLogic>.GetInstance().GetCurLvelContext().mapWidth * 0x3e8, 0x80, (int) GameDataMgr.globalInfoDatabin.GetDataByKey((uint) 0x38).dwConfValue);
        ClearAllFog();
        Shader.SetGlobalFloat("_SceneSize", (float) Mathf.Max(1, Singleton<BattleLogic>.GetInstance().GetCurLvelContext().mapWidth));
    }

    private static void Clear()
    {
        if (_bitmapData != null)
        {
            Array.Clear(_bitmapData, 0, _bitmapData.Length);
            Array.Clear(_bitmapDataTimer, 0, _bitmapData.Length);
        }
    }

    public static void ClearAllFog()
    {
        if (_bitmapData != null)
        {
            for (int i = 0; i < _bitmapData.Length; i++)
            {
                _bitmapData[i] = 0xff;
            }
            CommitToMaterials();
        }
    }

    public static void CommitToMaterials()
    {
        if (_bitmapData != null)
        {
            _bitmapTexture.LoadRawTextureData(_bitmapData);
            _bitmapTexture.Apply();
            Shader.SetGlobalTexture("_FogOfWar", _bitmapTexture);
        }
    }

    public static void EnableShaderFogFunction()
    {
        Shader.EnableKeyword("_FOG_OF_WAR_ON");
    }

    public static byte[] GetData()
    {
        return _bitmapData;
    }

    private static int Power2(int value)
    {
        return (value * value);
    }

    public static void PrepareData()
    {
        s_positionsOfSelfCompaign.Clear();
        if ((_enable && ((Singleton<BattleLogic>.instance.GetCurLvelContext() != null) && (Singleton<BattleLogic>.instance.GetCurLvelContext().horizonEnableMethod == Horizon.EnableMethod.EnableAll))) && Singleton<BattleLogic>.instance.isFighting)
        {
            CommitToMaterials();
            VInt3 zero = VInt3.zero;
            VInt3 outDirWorld = VInt3.zero;
            if (Singleton<BattleLogic>.GetInstance().mapLogic.GetRevivePosDir(ref Singleton<GamePlayerCenter>.instance.GetHostPlayer().Captain.handle.TheActorMeta, true, out zero, out outDirWorld))
            {
                s_positionsOfSelfCompaign.Add(new ACTOR_POS(zero.x, zero.z));
            }
            for (int i = 0; i < Singleton<GameObjMgr>.GetInstance().GameActors.Count; i++)
            {
                PoolObjHandle<ActorRoot> handle = Singleton<GameObjMgr>.GetInstance().GameActors[i];
                ActorRoot root = handle.handle;
                if (((root.TheActorMeta.ActorCamp == Singleton<GamePlayerCenter>.instance.hostPlayerCamp) && (root.ActorMesh != null)) && !root.ActorControl.IsDeadState)
                {
                    s_positionsOfSelfCompaign.Add(new ACTOR_POS((int) (root.ActorMesh.transform.position.x * 1000f), (int) (root.ActorMesh.transform.position.z * 1000f)));
                }
            }
            for (int j = 0; j < Singleton<GameObjMgr>.GetInstance().OrganActors.Count; j++)
            {
                PoolObjHandle<ActorRoot> handle2 = Singleton<GameObjMgr>.GetInstance().OrganActors[j];
                ActorRoot root2 = handle2.handle;
                if (((root2.TheActorMeta.ActorCamp == Singleton<GamePlayerCenter>.instance.hostPlayerCamp) && (root2.ActorMesh != null)) && !root2.ActorControl.IsDeadState)
                {
                    s_positionsOfSelfCompaign.Add(new ACTOR_POS((int) (root2.ActorMesh.transform.position.x * 1000f), (int) (root2.ActorMesh.transform.position.z * 1000f)));
                }
            }
        }
    }

    private static void PrepareMask()
    {
        maskSize = 1 + (2 * FogOfWar.maskRadiusInPixel);
        _maskData = new byte[maskSize * maskSize];
        int maskRadiusInPixel = FogOfWar.maskRadiusInPixel;
        int num2 = Power2(FogOfWar.maskRadiusInPixel);
        float num3 = 0.7f;
        float num4 = num2 * num3;
        for (int i = 0; i < maskSize; i++)
        {
            for (int j = 0; j < maskSize; j++)
            {
                int num7 = Power2(i - maskRadiusInPixel) + Power2(j - maskRadiusInPixel);
                if (num7 <= num4)
                {
                    _maskData[(i * maskSize) + j] = 0xff;
                }
                else if (num7 <= num2)
                {
                    _maskData[(i * maskSize) + j] = (byte) Mathf.Lerp(255f, 0f, (num7 - num4) / (num2 - num4));
                }
                else
                {
                    _maskData[(i * maskSize) + j] = 0;
                }
            }
        }
    }

    private static void Reset(int mapSize, int BitmapSize, int sight)
    {
        _MapWidth = mapSize;
        _MapHeight = mapSize;
        _BitmapWidth = BitmapSize;
        _BitmapHeight = BitmapSize;
        _bitmapData = new byte[_BitmapWidth * _BitmapHeight];
        _bitmapDataTimer = new byte[_BitmapWidth * _BitmapHeight];
        if ((BitmapSize != 0) && (sight != 0))
        {
            if (_bitmapTexture != null)
            {
                _bitmapTexture.Resize(_BitmapWidth, _BitmapWidth, TextureFormat.Alpha8, false);
            }
            else
            {
                _bitmapTexture = new Texture2D(_BitmapWidth, _BitmapWidth, TextureFormat.Alpha8, false);
                _bitmapTexture.wrapMode = TextureWrapMode.Clamp;
            }
            if (mapSize != 0)
            {
                maskRadiusInPixel = (_BitmapWidth * sight) / _MapWidth;
                PrepareMask();
                Clear();
            }
        }
    }

    public static void Run()
    {
        if (_enable)
        {
            for (int i = 0; i < _bitmapDataTimer.Length; i++)
            {
                if (_bitmapDataTimer[i] > 0)
                {
                    _bitmapDataTimer[i] = (byte) (_bitmapDataTimer[i] - 1);
                }
                else
                {
                    _bitmapData[i] = 0;
                }
            }
            for (int j = 0; j < s_positionsOfSelfCompaign.Count; j++)
            {
                ACTOR_POS actor_pos = s_positionsOfSelfCompaign[j];
                ACTOR_POS actor_pos2 = s_positionsOfSelfCompaign[j];
                AddSightAtPoint(actor_pos.x, actor_pos2.y);
            }
        }
    }

    public static void SetFlip(bool flip)
    {
        if (flip)
        {
            Shader.EnableKeyword("_FOG_OF_WAR_FLIP_ON");
        }
        else
        {
            Shader.EnableKeyword("_FOG_OF_WAR_FLIP_OFF");
        }
    }

    public static bool enable
    {
        get
        {
            return _enable;
        }
        set
        {
            if (_enable != value)
            {
                _enable = value;
                if (!_enable)
                {
                    ClearAllFog();
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ACTOR_POS
    {
        public int x;
        public int y;
        public ACTOR_POS(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}

