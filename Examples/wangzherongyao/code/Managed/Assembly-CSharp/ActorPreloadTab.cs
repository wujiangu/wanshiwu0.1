using Assets.Scripts.GameLogic.DataCenter;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct ActorPreloadTab
{
    public ActorMeta theActor;
    public AssetLoadBase modelPrefab;
    public List<AssetLoadBase> ageActions;
    public List<AssetLoadBase> parPrefabs;
    public List<AssetLoadBase> mesPrefabs;
    public List<AssetLoadBase> spritePrefabs;
    public List<AssetLoadBase> soundBanks;
    public List<AssetLoadBase> behaviorXml;
    public int MarkID;
    public float spawnCnt;
    public void AddParticle(string path)
    {
        AssetLoadBase item = new AssetLoadBase {
            assetPath = path
        };
        this.parPrefabs.Add(item);
    }

    public void AddSprite(string path)
    {
        AssetLoadBase item = new AssetLoadBase {
            assetPath = path
        };
        this.spritePrefabs.Add(item);
    }

    public void AddAction(string path)
    {
        AssetLoadBase item = new AssetLoadBase {
            assetPath = path
        };
        this.ageActions.Add(item);
    }

    public void AddMesh(string path)
    {
        AssetLoadBase item = new AssetLoadBase {
            assetPath = path
        };
        this.mesPrefabs.Add(item);
    }
}

