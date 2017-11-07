using System;
using UnityEngine;

internal class GlobalConfig : MonoSingleton<GlobalConfig>
{
    [FriendlyName("是否开启游戏内控制台")]
    public bool bEnableInGameConsole;
    [FriendlyName("是否开启特效裁剪优化")]
    public bool bEnableParticleCullOptimize = true;
    [FriendlyName("PVE真实时间Tick")]
    public bool bEnableRealTimeTickInPVE;
    [FriendlyName("人为操作模拟")]
    public bool bSimulateHumanOperation;
    [FriendlyName("模拟丢包")]
    public bool bSimulateLosePackage;
    [FriendlyName("宝箱怪掉落概率")]
    public int ChestMonsterDropItemProbability = 0x4b;
    [FriendlyName("掉落物飞起高度")]
    public int DropItemFlyHeight = 0x2710;
    [FriendlyName("掉落物飞翔时间")]
    public int DropItemFlyTime = 0x4b0;
    [FriendlyName("摇杆最大移动距离")]
    public int JoysticMaxExtendDist = 200;
    [FriendlyName("摇杆初始位置xy偏移")]
    public Vector2 JoysticRootPos = new Vector2(240f, 240f);
    [FriendlyName("普通怪掉落概率")]
    public int NormalMonsterDropItemProbability = 10;
    [FriendlyName("机关掉落概率")]
    public int OrganDropItemProbability = 50;
    [FriendlyName("拾取范围")]
    public int PickupRange = 0xbb8;
    [FriendlyName("刷单个怪之间的间隔")]
    public int SoldierWaveInterval = 0x3e8;
    public int WaypointIgnoreDist = 0x1869f;
}

