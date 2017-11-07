namespace Assets.Scripts.GameLogic
{
    using AGE;
    using Assets.Scripts.Common;
    using Assets.Scripts.Framework;
    using Assets.Scripts.GameLogic.DataCenter;
    using Assets.Scripts.GameLogic.GameKernal;
    using Assets.Scripts.GameSystem;
    using Assets.Scripts.Sound;
    using Assets.Scripts.UI;
    using behaviac;
    using ResData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class GameLoader : MonoSingleton<GameLoader>
    {
        private int _nProgress;
        public List<ActorMeta> actorList = new List<ActorMeta>();
        private List<ActorPreloadTab> actorPreload;
        private float coroutineTime;
        public bool isLoadStart;
        private ArrayList levelArtistList = new ArrayList();
        private ArrayList levelDesignList = new ArrayList();
        private ArrayList levelList = new ArrayList();
        private LoadCompleteDelegate LoadCompleteEvent;
        private LoadProgressDelegate LoadProgressEvent;
        private static GameSerializer s_serializer = new GameSerializer();
        private List<string> soundBankList = new List<string>();
        public ListView<ActorConfig> staticActors = new ListView<ActorConfig>();

        public void AddActor(ref ActorMeta actorMeta)
        {
            this.actorList.Add(actorMeta);
        }

        public void AddArtistSerializedLevel(string name)
        {
            this.levelArtistList.Add(name);
        }

        public void AddDesignSerializedLevel(string name)
        {
            this.levelDesignList.Add(name);
        }

        public void AddLevel(string name)
        {
            this.levelList.Add(name);
        }

        public void AddSoundBank(string name)
        {
            this.soundBankList.Add(name);
        }

        public void AddStaticActor(ActorConfig actor)
        {
            this.staticActors.Add(actor);
        }

        public void AdvanceStopLoad()
        {
            if (this.isLoadStart)
            {
                Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("ActorInfo");
                Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("CharIcon");
                Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("CharBattle");
                Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("CharShow");
                Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("CharLoading");
                Singleton<CResourceManager>.GetInstance().UnloadUnusedAssets();
                GC.Collect();
                Singleton<EventRouter>.instance.BroadCastEvent(EventID.ADVANCE_STOP_LOADING);
            }
            this.ResetLoader();
        }

        [DebuggerHidden]
        private IEnumerator CoroutineLoad()
        {
            return new <CoroutineLoad>c__Iterator16 { <>f__this = this };
        }

        public void Load(LoadProgressDelegate progress, LoadCompleteDelegate finish)
        {
            if (!this.isLoadStart)
            {
                UnityEngine.Debug.Log("GameLoader Start Load");
                this.LoadProgressEvent = progress;
                this.LoadCompleteEvent = finish;
                this._nProgress = 0;
                this.isLoadStart = true;
                base.StartCoroutine("CoroutineLoad");
            }
        }

        [DebuggerHidden]
        private IEnumerator LoadActorAssets(LHCWrapper InWrapper)
        {
            return new <LoadActorAssets>c__Iterator10 { InWrapper = InWrapper, <$>InWrapper = InWrapper, <>f__this = this };
        }

        [DebuggerHidden]
        private IEnumerator LoadAgeRecursiveAssets(LHCWrapper InWrapper)
        {
            return new <LoadAgeRecursiveAssets>c__Iterator12 { InWrapper = InWrapper, <$>InWrapper = InWrapper, <>f__this = this };
        }

        [DebuggerHidden]
        private IEnumerator LoadArtistLevel(LoaderHelperWrapper InWrapper)
        {
            return new <LoadArtistLevel>c__IteratorD { InWrapper = InWrapper, <$>InWrapper = InWrapper, <>f__this = this };
        }

        [DebuggerHidden]
        private IEnumerator LoadCommonAssetBundle(LoaderHelperWrapper InWrapper)
        {
            return new <LoadCommonAssetBundle>c__IteratorB { InWrapper = InWrapper, <$>InWrapper = InWrapper, <>f__this = this };
        }

        [DebuggerHidden]
        private IEnumerator LoadCommonAssets(LoaderHelperWrapper InWrapper)
        {
            return new <LoadCommonAssets>c__IteratorF { InWrapper = InWrapper, <$>InWrapper = InWrapper, <>f__this = this };
        }

        [DebuggerHidden]
        private IEnumerator LoadCommonEffect(LoaderHelperWrapper InWrapper)
        {
            return new <LoadCommonEffect>c__IteratorC { InWrapper = InWrapper, <$>InWrapper = InWrapper, <>f__this = this };
        }

        [DebuggerHidden]
        private IEnumerator LoadDesignLevel(LoaderHelperWrapper InWrapper)
        {
            return new <LoadDesignLevel>c__IteratorE { InWrapper = InWrapper, <$>InWrapper = InWrapper, <>f__this = this };
        }

        [DebuggerHidden]
        private IEnumerator LoadNoActorAssets(LHCWrapper InWrapper)
        {
            return new <LoadNoActorAssets>c__Iterator11 { InWrapper = InWrapper, <$>InWrapper = InWrapper, <>f__this = this };
        }

        [DebuggerHidden]
        private IEnumerator PreSpawnSoldiers(LoaderHelperWrapper InWrapper)
        {
            return new <PreSpawnSoldiers>c__Iterator15 { InWrapper = InWrapper, <$>InWrapper = InWrapper, <>f__this = this };
        }

        public void ResetLoader()
        {
            this.levelList.Clear();
            this.actorList.Clear();
            this.levelDesignList.Clear();
            this.levelArtistList.Clear();
            this.soundBankList.Clear();
            this.staticActors.Clear();
            this._nProgress = 0;
            if (this.isLoadStart)
            {
                base.StopCoroutine("PreSpawnSoldiers");
                base.StopCoroutine("SpawnDynamicActor");
                base.StopCoroutine("SpawnStaticActor");
                base.StopCoroutine("LoadAgeRecursiveAssets");
                base.StopCoroutine("LoadNoActorAssets");
                base.StopCoroutine("LoadActorAssets");
                base.StopCoroutine("LoadCommonAssets");
                base.StopCoroutine("LoadDesignLevel");
                base.StopCoroutine("LoadArtistLevel");
                base.StopCoroutine("LoadCommonAssetBundle");
                base.StopCoroutine("LoadCommonEffect");
                base.StopCoroutine("CoroutineLoad");
                this.isLoadStart = false;
            }
        }

        private bool ShouldYieldReturn()
        {
            return ((Time.realtimeSinceStartup - this.coroutineTime) > 0.08f);
        }

        [DebuggerHidden]
        private IEnumerator SpawnDynamicActor(LoaderHelperWrapper InWrapper)
        {
            return new <SpawnDynamicActor>c__Iterator14 { InWrapper = InWrapper, <$>InWrapper = InWrapper, <>f__this = this };
        }

        [DebuggerHidden]
        private IEnumerator SpawnStaticActor(LoaderHelperWrapper InWrapper)
        {
            return new <SpawnStaticActor>c__Iterator13 { InWrapper = InWrapper, <$>InWrapper = InWrapper, <>f__this = this };
        }

        private void UpdateProgress(LoaderHelperCamera lhc, int oldProgress, int duty, int index, int count)
        {
            this.coroutineTime = Time.realtimeSinceStartup;
            this.nProgress = oldProgress + ((duty * index) / count);
            this.LoadProgressEvent(this.nProgress * 0.0001f);
            if (lhc != null)
            {
                lhc.Update();
            }
        }

        public int nProgress
        {
            get
            {
                return this._nProgress;
            }
            set
            {
                if (value >= this._nProgress)
                {
                    this._nProgress = value;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <CoroutineLoad>c__Iterator16 : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal GameLoader <>f__this;
            internal Animator <animator>__7;
            internal List<string> <anims>__8;
            internal CUIFormScript <battleUiForm>__4;
            internal GameObject <go>__5;
            internal GameObject <go2>__6;
            internal int <i>__0;
            internal int <i>__1;
            internal int <i>__9;
            internal int <j>__10;
            internal LoaderHelperCamera <lhc>__3;
            internal LoaderHelper <loadHelper>__2;

            [DebuggerHidden]
            public void Dispose()
            {
                this.$PC = -1;
            }

            public bool MoveNext()
            {
                uint num = (uint) this.$PC;
                this.$PC = -1;
                switch (num)
                {
                    case 0:
                        this.$current = null;
                        this.$PC = 1;
                        goto Label_0A3A;

                    case 1:
                    {
                        DynamicShadow.DisableAllDynamicShadows();
                        DynamicShadow.InitDefaultGlobalVariables();
                        Singleton<CUIManager>.GetInstance().ClearFormPool();
                        Singleton<CGameObjectPool>.GetInstance().ClearPooledObjects();
                        enResourceType[] resourceTypes = new enResourceType[5];
                        resourceTypes[1] = enResourceType.UI3DImage;
                        resourceTypes[2] = enResourceType.UIForm;
                        resourceTypes[3] = enResourceType.UIPrefab;
                        resourceTypes[4] = enResourceType.UISprite;
                        Singleton<CResourceManager>.GetInstance().RemoveCachedResources(resourceTypes);
                        this.<>f__this.nProgress = 200;
                        this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                        this.$current = null;
                        this.$PC = 2;
                        goto Label_0A3A;
                    }
                    case 2:
                        this.<>f__this.nProgress = 300;
                        this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                        this.$current = null;
                        this.$PC = 3;
                        goto Label_0A3A;

                    case 3:
                        this.<>f__this.nProgress = 400;
                        this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                        this.$current = null;
                        this.$PC = 4;
                        goto Label_0A3A;

                    case 4:
                        if (this.<>f__this.levelList.Count == 0)
                        {
                            this.<>f__this.levelList.Add("EmptyScene");
                        }
                        PlaneShadowSettings.SetDefault();
                        this.<>f__this.nProgress = 500;
                        this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                        this.$current = null;
                        this.$PC = 5;
                        goto Label_0A3A;

                    case 5:
                        this.<i>__0 = 0;
                        break;

                    case 6:
                        this.<i>__0++;
                        break;

                    case 7:
                        if (((this.<>f__this.levelArtistList.Count > 0) || (this.<>f__this.levelDesignList.Count > 0)) && (Camera.allCameras != null))
                        {
                            this.<i>__1 = 0;
                            while (this.<i>__1 < Camera.allCameras.Length)
                            {
                                if (Camera.main != null)
                                {
                                    UnityEngine.Object.Destroy(Camera.allCameras[this.<i>__1].gameObject);
                                }
                                this.<i>__1++;
                            }
                        }
                        this.<>f__this.nProgress = 0x3e8;
                        this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                        this.$current = 0;
                        this.$PC = 8;
                        goto Label_0A3A;

                    case 8:
                    {
                        this.<loadHelper>__2 = new LoaderHelper();
                        this.<lhc>__3 = new LoaderHelperCamera();
                        GameLoader.LoaderHelperWrapper wrapper = new GameLoader.LoaderHelperWrapper {
                            loadHelper = this.<loadHelper>__2,
                            duty = 350
                        };
                        this.$current = this.<>f__this.StartCoroutine("LoadCommonAssetBundle", wrapper);
                        this.$PC = 9;
                        goto Label_0A3A;
                    }
                    case 9:
                    {
                        GameLoader.LoaderHelperWrapper wrapper2 = new GameLoader.LoaderHelperWrapper {
                            loadHelper = this.<loadHelper>__2,
                            duty = 150
                        };
                        this.$current = this.<>f__this.StartCoroutine("LoadCommonEffect", wrapper2);
                        this.$PC = 10;
                        goto Label_0A3A;
                    }
                    case 10:
                    {
                        GameLoader.LoaderHelperWrapper wrapper3 = new GameLoader.LoaderHelperWrapper {
                            loadHelper = this.<loadHelper>__2,
                            duty = 0x3e8
                        };
                        this.$current = this.<>f__this.StartCoroutine("LoadArtistLevel", wrapper3);
                        this.$PC = 11;
                        goto Label_0A3A;
                    }
                    case 11:
                    {
                        GameLoader.LoaderHelperWrapper wrapper4 = new GameLoader.LoaderHelperWrapper {
                            loadHelper = this.<loadHelper>__2,
                            duty = 500
                        };
                        this.$current = this.<>f__this.StartCoroutine("LoadDesignLevel", wrapper4);
                        this.$PC = 12;
                        goto Label_0A3A;
                    }
                    case 12:
                    {
                        GameLoader.LoaderHelperWrapper wrapper5 = new GameLoader.LoaderHelperWrapper {
                            loadHelper = this.<loadHelper>__2,
                            duty = 500
                        };
                        this.$current = this.<>f__this.StartCoroutine("LoadCommonAssets", wrapper5);
                        this.$PC = 13;
                        goto Label_0A3A;
                    }
                    case 13:
                    {
                        GameLoader.LHCWrapper wrapper6 = new GameLoader.LHCWrapper {
                            lhc = this.<lhc>__3,
                            loadHelper = this.<loadHelper>__2,
                            duty = 500
                        };
                        this.$current = this.<>f__this.StartCoroutine("LoadActorAssets", wrapper6);
                        this.$PC = 14;
                        goto Label_0A3A;
                    }
                    case 14:
                    {
                        GameLoader.LHCWrapper wrapper7 = new GameLoader.LHCWrapper {
                            lhc = this.<lhc>__3,
                            loadHelper = this.<loadHelper>__2,
                            duty = 0x3e8
                        };
                        this.$current = this.<>f__this.StartCoroutine("LoadNoActorAssets", wrapper7);
                        this.$PC = 15;
                        goto Label_0A3A;
                    }
                    case 15:
                    {
                        GameLoader.LHCWrapper wrapper8 = new GameLoader.LHCWrapper {
                            lhc = this.<lhc>__3,
                            loadHelper = this.<loadHelper>__2,
                            duty = 0xf3c
                        };
                        this.$current = this.<>f__this.StartCoroutine("LoadAgeRecursiveAssets", wrapper8);
                        this.$PC = 0x10;
                        goto Label_0A3A;
                    }
                    case 0x10:
                    case 0x11:
                        if (!this.<lhc>__3.Update())
                        {
                            this.$current = 0;
                            this.$PC = 0x11;
                        }
                        else
                        {
                            this.<lhc>__3.Destroy();
                            this.<lhc>__3 = null;
                            GameLoader.LoaderHelperWrapper wrapper9 = new GameLoader.LoaderHelperWrapper {
                                loadHelper = this.<loadHelper>__2,
                                duty = 200
                            };
                            this.$current = this.<>f__this.StartCoroutine("SpawnStaticActor", wrapper9);
                            this.$PC = 0x12;
                        }
                        goto Label_0A3A;

                    case 0x12:
                    {
                        GameLoader.LoaderHelperWrapper wrapper10 = new GameLoader.LoaderHelperWrapper {
                            loadHelper = this.<loadHelper>__2,
                            duty = 200
                        };
                        this.$current = this.<>f__this.StartCoroutine("SpawnDynamicActor", wrapper10);
                        this.$PC = 0x13;
                        goto Label_0A3A;
                    }
                    case 0x13:
                        this.<>f__this.nProgress = 0x2648;
                        this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                        this.$current = 0;
                        this.$PC = 20;
                        goto Label_0A3A;

                    case 20:
                        if (GameSettings.AllowOutlineFilter)
                        {
                            OutlineFilter.EnableOutlineFilter();
                        }
                        this.$current = 0;
                        this.$PC = 0x15;
                        goto Label_0A3A;

                    case 0x15:
                        Shader.WarmupAllShaders();
                        this.$current = 0;
                        this.$PC = 0x16;
                        goto Label_0A3A;

                    case 0x16:
                        Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("ActorInfo");
                        Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("CharIcon");
                        Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("CharBattle");
                        Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("CharShow");
                        Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("CharLoading");
                        Singleton<CResourceManager>.GetInstance().UnloadUnusedAssets();
                        this.<>f__this.nProgress = 0x26ac;
                        this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                        this.$current = 0;
                        this.$PC = 0x17;
                        goto Label_0A3A;

                    case 0x17:
                        this.<battleUiForm>__4 = Singleton<CUIManager>.GetInstance().OpenForm(CBattleSystem.s_battleUIForm, false, true);
                        this.<go>__5 = this.<battleUiForm>__4.gameObject.FindChildBFS("KillNotify_New");
                        this.<go2>__6 = this.<go>__5.FindChildBFS("KillNotify_Sub");
                        this.<animator>__7 = this.<go2>__6.GetComponent<Animator>();
                        this.<go>__5.SetActive(true);
                        this.<anims>__8 = KillNotifyUT.GetAllAnimations();
                        this.<i>__9 = 0;
                        goto Label_092B;

                    case 0x18:
                        this.<i>__9++;
                        goto Label_092B;

                    case 0x19:
                        this.<>f__this.nProgress = 0x2710;
                        this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                        this.$current = 0;
                        this.$PC = 0x1a;
                        goto Label_0A3A;

                    case 0x1a:
                        this.<>f__this.actorPreload = null;
                        this.<>f__this.isLoadStart = false;
                        this.<>f__this.LoadCompleteEvent();
                        Singleton<GameDataMgr>.GetInstance().UnloadDataBin();
                        GC.Collect();
                        UnityEngine.Debug.Log("GameLoader Finish Load");
                        this.$PC = -1;
                        goto Label_0A38;

                    default:
                        goto Label_0A38;
                }
                if (this.<i>__0 < this.<>f__this.levelList.Count)
                {
                    this.$current = Application.LoadLevelAsync((string) this.<>f__this.levelList[this.<i>__0]);
                    this.$PC = 6;
                }
                else
                {
                    this.<>f__this.nProgress = 600;
                    this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                    this.$current = 0;
                    this.$PC = 7;
                }
                goto Label_0A3A;
            Label_092B:
                if (this.<i>__9 < this.<anims>__8.Count)
                {
                    this.<animator>__7.Play(this.<anims>__8[this.<i>__9]);
                    this.<j>__10 = 0;
                    while (this.<j>__10 < 6)
                    {
                        this.<animator>__7.Update(0.5f);
                        this.<j>__10++;
                    }
                    this.$current = 0;
                    this.$PC = 0x18;
                }
                else
                {
                    this.<go>__5.SetActive(false);
                    Singleton<BattleSkillHudControl>.CreateInstance();
                    Singleton<BattleLogic>.GetInstance().PrepareFight();
                    GameLoader.LoaderHelperWrapper wrapper11 = new GameLoader.LoaderHelperWrapper {
                        loadHelper = this.<loadHelper>__2,
                        duty = 100
                    };
                    this.$current = this.<>f__this.StartCoroutine("PreSpawnSoldiers", wrapper11);
                    this.$PC = 0x19;
                }
                goto Label_0A3A;
            Label_0A38:
                return false;
            Label_0A3A:
                return true;
            }

            [DebuggerHidden]
            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <LoadActorAssets>c__Iterator10 : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal GameLoader.LHCWrapper <$>InWrapper;
            internal GameLoader <>f__this;
            internal AGE.Action <action>__29;
            internal ActorMeta <actorMeta>__7;
            internal string <btPath>__31;
            internal int <count>__4;
            internal int <currentParticleLOD>__21;
            internal int <duty>__1;
            internal ResHeroCfgInfo <heroCfgInfo>__9;
            internal Player <hostPlayer>__8;
            internal int <i>__13;
            internal int <i>__15;
            internal int <i>__6;
            internal string <iconPath>__12;
            internal int <index>__5;
            internal int <j>__10;
            internal int <j>__20;
            internal int <j>__24;
            internal int <j>__25;
            internal int <j>__28;
            internal int <j>__30;
            internal LoaderHelperCamera <lhc>__0;
            internal LoaderHelper <loadHelper>__2;
            internal ActorPreloadTab <loadInfo>__14;
            internal ActorPreloadTab <loadInfo>__16;
            internal int <lod>__22;
            internal int <markID>__26;
            internal int <oldProgress>__3;
            internal string <parPathKey>__23;
            internal CResourcePackerInfo <pkger>__18;
            internal Dictionary<object, AssetRefType> <refAssets>__27;
            internal ResSkillCfgInfo <skillCfgInfo>__11;
            internal GameObject <tmpObj>__17;
            internal int <x>__19;
            internal GameLoader.LHCWrapper InWrapper;

            [DebuggerHidden]
            public void Dispose()
            {
                this.$PC = -1;
            }

            public bool MoveNext()
            {
                uint num = (uint) this.$PC;
                this.$PC = -1;
                switch (num)
                {
                    case 0:
                        this.<lhc>__0 = this.InWrapper.lhc;
                        this.<duty>__1 = this.InWrapper.duty;
                        this.<loadHelper>__2 = this.InWrapper.loadHelper;
                        this.<oldProgress>__3 = this.<>f__this.nProgress;
                        this.<>f__this.actorPreload = this.InWrapper.loadHelper.GetActorPreload();
                        this.<count>__4 = this.<>f__this.actorPreload.Count;
                        this.<index>__5 = 0;
                        this.<i>__6 = 0;
                        while (this.<i>__6 < this.<>f__this.actorPreload.Count)
                        {
                            ActorPreloadTab tab = this.<>f__this.actorPreload[this.<i>__6];
                            this.<actorMeta>__7 = tab.theActor;
                            this.<hostPlayer>__8 = Singleton<GamePlayerCenter>.GetInstance().GetHostPlayer();
                            if (this.<hostPlayer>__8.PlayerId == this.<actorMeta>__7.PlayerId)
                            {
                                this.<heroCfgInfo>__9 = GameDataMgr.heroDatabin.GetDataByKey(this.<actorMeta>__7.ConfigId);
                                this.<j>__10 = 0;
                                while (this.<j>__10 < this.<heroCfgInfo>__9.astSkill.Length)
                                {
                                    this.<skillCfgInfo>__11 = GameDataMgr.skillDatabin.GetDataByKey(this.<heroCfgInfo>__9.astSkill[this.<j>__10].iSkillID);
                                    object[] inParameters = new object[] { this.<heroCfgInfo>__9.astSkill[this.<j>__10].iSkillID };
                                    DebugHelper.Assert(this.<skillCfgInfo>__11 != null, "Failed Found skill config id = {0}", inParameters);
                                    this.<iconPath>__12 = string.Empty;
                                    if (this.<skillCfgInfo>__11 != null)
                                    {
                                        this.<iconPath>__12 = StringHelper.UTF8BytesToString(ref this.<skillCfgInfo>__11.szIconPath);
                                    }
                                    if (!string.IsNullOrEmpty(this.<iconPath>__12))
                                    {
                                        Singleton<CResourceManager>.GetInstance().LoadAllResourceInResourcePackerInfo(Singleton<CResourceManager>.GetInstance().GetResourceBelongedPackerInfo(CUIUtility.s_Sprite_Dynamic_Skill_Dir + this.<iconPath>__12), enResourceType.UISprite);
                                    }
                                    this.<j>__10++;
                                }
                            }
                            this.<i>__6++;
                        }
                        this.<i>__13 = 0;
                        while (this.<i>__13 < this.<>f__this.actorPreload.Count)
                        {
                            this.<loadInfo>__14 = this.<>f__this.actorPreload[this.<i>__13];
                            this.<count>__4 += (((this.<loadInfo>__14.parPrefabs.Count + this.<loadInfo>__14.mesPrefabs.Count) + this.<loadInfo>__14.soundBanks.Count) + this.<loadInfo>__14.ageActions.Count) + this.<loadInfo>__14.behaviorXml.Count;
                            this.<i>__13++;
                        }
                        this.<i>__15 = 0;
                        while (this.<i>__15 < this.<>f__this.actorPreload.Count)
                        {
                            this.<loadInfo>__16 = this.<>f__this.actorPreload[this.<i>__15];
                            this.<tmpObj>__17 = null;
                            if (!this.InWrapper.lhc.HasLoaded(this.<loadInfo>__16.modelPrefab.assetPath))
                            {
                                this.<pkger>__18 = Singleton<CResourceManager>.instance.GetResourceBelongedPackerInfo(this.<loadInfo>__16.modelPrefab.assetPath);
                                if ((this.<pkger>__18 != null) && this.<pkger>__18.m_isAssetBundle)
                                {
                                    if (!this.<pkger>__18.IsAssetBundleLoaded())
                                    {
                                        this.<pkger>__18.LoadAssetBundle(CFileManager.GetIFSExtractPath());
                                    }
                                    this.<x>__19 = 0;
                                    while (this.<x>__19 < this.<pkger>__18.m_resourceInfos.Count)
                                    {
                                        stResourceInfo info = this.<pkger>__18.m_resourceInfos[this.<x>__19];
                                        if (string.Equals(info.m_extension, ".prefab", StringComparison.OrdinalIgnoreCase))
                                        {
                                            stResourceInfo info2 = this.<pkger>__18.m_resourceInfos[this.<x>__19];
                                            if (!info2.m_fullPathInResourcesWithoutExtension.Contains("UGUI/Sprite/Dynamic"))
                                            {
                                                stResourceInfo info3 = this.<pkger>__18.m_resourceInfos[this.<x>__19];
                                                if (!string.Equals(info3.m_fullPathInResourcesWithoutExtension, CFileManager.EraseExtension(this.<loadInfo>__16.modelPrefab.assetPath), StringComparison.OrdinalIgnoreCase))
                                                {
                                                    goto Label_04AB;
                                                }
                                            }
                                        }
                                        stResourceInfo info4 = this.<pkger>__18.m_resourceInfos[this.<x>__19];
                                        stResourceInfo info5 = this.<pkger>__18.m_resourceInfos[this.<x>__19];
                                        Singleton<CResourceManager>.instance.GetResource(info4.m_fullPathInResourcesWithoutExtension, Singleton<CResourceManager>.GetInstance().GetResourceContentType(info5.m_extension), enResourceType.BattleScene, true, false);
                                    Label_04AB:
                                        this.<x>__19++;
                                    }
                                    if (this.<loadInfo>__16.theActor.ActorType == ActorTypeDef.Actor_Type_Hero)
                                    {
                                        this.<pkger>__18.UnloadAssetBundle(false);
                                    }
                                }
                                this.<tmpObj>__17 = Singleton<CGameObjectPool>.instance.GetGameObject(this.<loadInfo>__16.modelPrefab.assetPath, enResourceType.BattleScene);
                                this.<lhc>__0.AddObj(this.<loadInfo>__16.modelPrefab.assetPath, this.<tmpObj>__17);
                            }
                            this.<j>__20 = 0;
                            while (this.<j>__20 < this.<loadInfo>__16.parPrefabs.Count)
                            {
                                AssetLoadBase base2 = this.<loadInfo>__16.parPrefabs[this.<j>__20];
                                if (!this.<lhc>__0.HasLoaded(base2.assetPath))
                                {
                                    this.<currentParticleLOD>__21 = GameSettings.ParticleLOD;
                                    this.<lod>__22 = this.<currentParticleLOD>__21;
                                    while (this.<lod>__22 <= 2)
                                    {
                                        AssetLoadBase base3 = this.<loadInfo>__16.parPrefabs[this.<j>__20];
                                        this.<parPathKey>__23 = base3.assetPath + "_lod_" + this.<lod>__22;
                                        if (!this.<lhc>__0.HasLoaded(this.<parPathKey>__23))
                                        {
                                            GameSettings.ParticleLOD = this.<lod>__22;
                                            AssetLoadBase base4 = this.<loadInfo>__16.parPrefabs[this.<j>__20];
                                            this.<tmpObj>__17 = MonoSingleton<SceneMgr>.instance.GetPooledGameObjLOD(base4.assetPath, true, SceneObjType.ActionRes, Vector3.zero);
                                            this.<lhc>__0.AddObj(this.<parPathKey>__23, this.<tmpObj>__17);
                                        }
                                        this.<lod>__22++;
                                    }
                                    GameSettings.ParticleLOD = this.<currentParticleLOD>__21;
                                }
                                this.<index>__5++;
                                if (!this.<>f__this.ShouldYieldReturn())
                                {
                                    goto Label_06BA;
                                }
                                this.$current = 0;
                                this.$PC = 1;
                                goto Label_0B84;
                            Label_0691:
                                this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__3, this.<duty>__1, this.<index>__5, this.<count>__4);
                            Label_06BA:
                                this.<j>__20++;
                            }
                            this.<j>__24 = 0;
                            while (this.<j>__24 < this.<loadInfo>__16.mesPrefabs.Count)
                            {
                                AssetLoadBase base5 = this.<loadInfo>__16.mesPrefabs[this.<j>__24];
                                if (!this.<lhc>__0.HasLoaded(base5.assetPath))
                                {
                                    AssetLoadBase base6 = this.<loadInfo>__16.mesPrefabs[this.<j>__24];
                                    this.<tmpObj>__17 = MonoSingleton<SceneMgr>.instance.GetPooledGameObjLOD(base6.assetPath, false, SceneObjType.ActionRes, Vector3.zero);
                                    AssetLoadBase base7 = this.<loadInfo>__16.mesPrefabs[this.<j>__24];
                                    this.<lhc>__0.AddObj(base7.assetPath, this.<tmpObj>__17);
                                }
                                this.<index>__5++;
                                if (!this.<>f__this.ShouldYieldReturn())
                                {
                                    goto Label_07E3;
                                }
                                this.$current = 0;
                                this.$PC = 2;
                                goto Label_0B84;
                            Label_07BA:
                                this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__3, this.<duty>__1, this.<index>__5, this.<count>__4);
                            Label_07E3:
                                this.<j>__24++;
                            }
                            this.<j>__25 = 0;
                            while (this.<j>__25 < this.<loadInfo>__16.soundBanks.Count)
                            {
                                AssetLoadBase base8 = this.<loadInfo>__16.soundBanks[this.<j>__25];
                                Singleton<CSoundManager>.instance.LoadBank(base8.assetPath, CSoundManager.BankType.Battle);
                                this.<index>__5++;
                                if (!this.<>f__this.ShouldYieldReturn())
                                {
                                    goto Label_08A1;
                                }
                                this.$current = 0;
                                this.$PC = 3;
                                goto Label_0B84;
                            Label_0878:
                                this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__3, this.<duty>__1, this.<index>__5, this.<count>__4);
                            Label_08A1:
                                this.<j>__25++;
                            }
                            this.<markID>__26 = this.<loadInfo>__16.MarkID;
                            this.<refAssets>__27 = this.<loadHelper>__2.GetRefAssets(this.<markID>__26);
                            this.<j>__28 = 0;
                            while (this.<j>__28 < this.<loadInfo>__16.ageActions.Count)
                            {
                                AssetLoadBase base9 = this.<loadInfo>__16.ageActions[this.<j>__28];
                                this.<action>__29 = MonoSingleton<ActionManager>.instance.LoadActionResource(base9.assetPath);
                                if (this.<action>__29 != null)
                                {
                                    this.<action>__29.GetAssociatedResources(this.<refAssets>__27, this.<markID>__26);
                                }
                                this.<index>__5++;
                                if (!this.<>f__this.ShouldYieldReturn())
                                {
                                    goto Label_09AE;
                                }
                                this.$current = 0;
                                this.$PC = 4;
                                goto Label_0B84;
                            Label_0985:
                                this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__3, this.<duty>__1, this.<index>__5, this.<count>__4);
                            Label_09AE:
                                this.<j>__28++;
                            }
                            this.<j>__30 = 0;
                            while (this.<j>__30 < this.<loadInfo>__16.behaviorXml.Count)
                            {
                                AssetLoadBase base10 = this.<loadInfo>__16.behaviorXml[this.<j>__30];
                                this.<btPath>__31 = base10.assetPath;
                                Workspace.Load(this.<btPath>__31, false);
                                this.<index>__5++;
                                if (!this.<>f__this.ShouldYieldReturn())
                                {
                                    goto Label_0A74;
                                }
                                this.$current = 0;
                                this.$PC = 5;
                                goto Label_0B84;
                            Label_0A4B:
                                this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__3, this.<duty>__1, this.<index>__5, this.<count>__4);
                            Label_0A74:
                                this.<j>__30++;
                            }
                            this.<index>__5++;
                            if (!this.<>f__this.ShouldYieldReturn())
                            {
                                goto Label_0AFC;
                            }
                            this.$current = 0;
                            this.$PC = 6;
                            goto Label_0B84;
                        Label_0AD3:
                            this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__3, this.<duty>__1, this.<index>__5, this.<count>__4);
                        Label_0AFC:
                            this.<i>__15++;
                        }
                        this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__3, this.<duty>__1, 1, 1);
                        this.$current = 0;
                        this.$PC = 7;
                        goto Label_0B84;

                    case 1:
                        goto Label_0691;

                    case 2:
                        goto Label_07BA;

                    case 3:
                        goto Label_0878;

                    case 4:
                        goto Label_0985;

                    case 5:
                        goto Label_0A4B;

                    case 6:
                        goto Label_0AD3;

                    case 7:
                        this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__3, this.<duty>__1, 1, 1);
                        this.$PC = -1;
                        break;
                }
                return false;
            Label_0B84:
                return true;
            }

            [DebuggerHidden]
            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <LoadAgeRecursiveAssets>c__Iterator12 : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal GameLoader.LHCWrapper <$>InWrapper;
            internal GameLoader <>f__this;
            internal AGE.Action <action>__24;
            internal int <count>__8;
            internal int <currentParticleLOD>__18;
            internal int <duty>__1;
            internal int <i>__5;
            internal int <idx>__11;
            internal int <idx>__13;
            internal int <index>__9;
            internal int <j>__16;
            internal int <j>__21;
            internal int <j>__23;
            internal LoaderHelperCamera <lhc>__0;
            internal LoaderHelper <loadHelper>__2;
            internal int <lod>__19;
            internal int <markID>__15;
            internal int <numPasses>__3;
            internal int <oldProgress>__4;
            internal string <parPathKey>__20;
            internal int <progress>__10;
            internal Dictionary<object, AssetRefType> <refAssets>__25;
            internal ActorPreloadTab <restAssets>__12;
            internal ActorPreloadTab <restAssets>__14;
            internal List<ActorPreloadTab> <restAssetsList>__7;
            internal int <subDuty>__6;
            internal GameObject <tmpObj>__17;
            internal GameObject <tmpObj>__22;
            internal GameLoader.LHCWrapper InWrapper;

            [DebuggerHidden]
            public void Dispose()
            {
                this.$PC = -1;
            }

            public bool MoveNext()
            {
                uint num = (uint) this.$PC;
                this.$PC = -1;
                switch (num)
                {
                    case 0:
                        this.<lhc>__0 = this.InWrapper.lhc;
                        this.<duty>__1 = this.InWrapper.duty;
                        this.<loadHelper>__2 = this.InWrapper.loadHelper;
                        this.<numPasses>__3 = 10;
                        this.<oldProgress>__4 = this.<>f__this.nProgress;
                        this.<i>__5 = 0;
                        break;

                    case 1:
                        goto Label_02F4;

                    case 2:
                        goto Label_0423;

                    case 3:
                        goto Label_051F;

                    case 4:
                        this.<>f__this.UpdateProgress(this.<lhc>__0, this.<progress>__10, this.<subDuty>__6, 1, 1);
                        this.<i>__5++;
                        break;

                    case 5:
                        this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__4, this.<duty>__1, 1, 1);
                        this.$PC = -1;
                        goto Label_0667;

                    default:
                        goto Label_0667;
                }
                if (this.<i>__5 < this.<numPasses>__3)
                {
                    if (this.<i>__5 < 3)
                    {
                        this.<subDuty>__6 = this.<duty>__1 / 4;
                    }
                    else
                    {
                        this.<subDuty>__6 = this.<duty>__1 / (4 * (this.<numPasses>__3 - 3));
                    }
                    this.<restAssetsList>__7 = this.<loadHelper>__2.AnalyseAgeRefAssets(this.<loadHelper>__2.ageRefAssets2);
                    this.<loadHelper>__2.ageRefAssets2.Clear();
                    this.<count>__8 = 0;
                    this.<index>__9 = 0;
                    this.<progress>__10 = this.<>f__this.nProgress;
                    this.<idx>__11 = 0;
                    while (this.<idx>__11 < this.<restAssetsList>__7.Count)
                    {
                        this.<restAssets>__12 = this.<restAssetsList>__7[this.<idx>__11];
                        this.<count>__8 += (this.<restAssets>__12.parPrefabs.Count + this.<restAssets>__12.mesPrefabs.Count) + this.<restAssets>__12.ageActions.Count;
                        this.<idx>__11++;
                    }
                    this.<idx>__13 = 0;
                    while (this.<idx>__13 < this.<restAssetsList>__7.Count)
                    {
                        this.<restAssets>__14 = this.<restAssetsList>__7[this.<idx>__13];
                        this.<markID>__15 = this.<restAssets>__14.MarkID;
                        this.<j>__16 = 0;
                        while (this.<j>__16 < this.<restAssets>__14.parPrefabs.Count)
                        {
                            this.<tmpObj>__17 = null;
                            this.<currentParticleLOD>__18 = GameSettings.ParticleLOD;
                            this.<lod>__19 = this.<currentParticleLOD>__18;
                            while (this.<lod>__19 <= 2)
                            {
                                AssetLoadBase base2 = this.<restAssets>__14.parPrefabs[this.<j>__16];
                                this.<parPathKey>__20 = base2.assetPath + "_lod_" + this.<lod>__19;
                                if (!this.<lhc>__0.HasLoaded(this.<parPathKey>__20))
                                {
                                    GameSettings.ParticleLOD = this.<lod>__19;
                                    AssetLoadBase base3 = this.<restAssets>__14.parPrefabs[this.<j>__16];
                                    this.<tmpObj>__17 = MonoSingleton<SceneMgr>.instance.GetPooledGameObjLOD(base3.assetPath, true, SceneObjType.ActionRes, Vector3.zero);
                                    this.<lhc>__0.AddObj(this.<parPathKey>__20, this.<tmpObj>__17);
                                }
                                this.<lod>__19++;
                            }
                            GameSettings.ParticleLOD = this.<currentParticleLOD>__18;
                            this.<index>__9++;
                            if (!this.<>f__this.ShouldYieldReturn())
                            {
                                goto Label_031D;
                            }
                            this.$current = 0;
                            this.$PC = 1;
                            goto Label_0669;
                        Label_02F4:
                            this.<>f__this.UpdateProgress(this.<lhc>__0, this.<progress>__10, this.<subDuty>__6, this.<index>__9, this.<count>__8);
                        Label_031D:
                            this.<j>__16++;
                        }
                        this.<j>__21 = 0;
                        while (this.<j>__21 < this.<restAssets>__14.mesPrefabs.Count)
                        {
                            this.<tmpObj>__22 = null;
                            AssetLoadBase base4 = this.<restAssets>__14.mesPrefabs[this.<j>__21];
                            if (!this.<lhc>__0.HasLoaded(base4.assetPath))
                            {
                                AssetLoadBase base5 = this.<restAssets>__14.mesPrefabs[this.<j>__21];
                                this.<tmpObj>__22 = MonoSingleton<SceneMgr>.instance.GetPooledGameObjLOD(base5.assetPath, false, SceneObjType.ActionRes, Vector3.zero);
                                AssetLoadBase base6 = this.<restAssets>__14.mesPrefabs[this.<j>__21];
                                this.<lhc>__0.AddObj(base6.assetPath, this.<tmpObj>__22);
                            }
                            this.<index>__9++;
                            if (!this.<>f__this.ShouldYieldReturn())
                            {
                                goto Label_044C;
                            }
                            this.$current = 0;
                            this.$PC = 2;
                            goto Label_0669;
                        Label_0423:
                            this.<>f__this.UpdateProgress(this.<lhc>__0, this.<progress>__10, this.<subDuty>__6, this.<index>__9, this.<count>__8);
                        Label_044C:
                            this.<j>__21++;
                        }
                        this.<j>__23 = 0;
                        while (this.<j>__23 < this.<restAssets>__14.ageActions.Count)
                        {
                            AssetLoadBase base7 = this.<restAssets>__14.ageActions[this.<j>__23];
                            this.<action>__24 = MonoSingleton<ActionManager>.instance.LoadActionResource(base7.assetPath);
                            if (this.<action>__24 != null)
                            {
                                this.<refAssets>__25 = this.<loadHelper>__2.GetRefAssets(this.<markID>__15);
                                this.<action>__24.GetAssociatedResources(this.<refAssets>__25, this.<markID>__15);
                            }
                            this.<index>__9++;
                            if (!this.<>f__this.ShouldYieldReturn())
                            {
                                goto Label_0548;
                            }
                            this.$current = 0;
                            this.$PC = 3;
                            goto Label_0669;
                        Label_051F:
                            this.<>f__this.UpdateProgress(this.<lhc>__0, this.<progress>__10, this.<subDuty>__6, this.<index>__9, this.<count>__8);
                        Label_0548:
                            this.<j>__23++;
                        }
                        this.<idx>__13++;
                    }
                    this.<>f__this.UpdateProgress(this.<lhc>__0, this.<progress>__10, this.<subDuty>__6, 1, 1);
                    this.$current = 0;
                    this.$PC = 4;
                }
                else
                {
                    this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__4, this.<duty>__1, 1, 1);
                    this.$current = 0;
                    this.$PC = 5;
                }
                goto Label_0669;
            Label_0667:
                return false;
            Label_0669:
                return true;
            }

            [DebuggerHidden]
            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <LoadArtistLevel>c__IteratorD : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal GameLoader.LoaderHelperWrapper <$>InWrapper;
            internal GameLoader <>f__this;
            internal string <artAssetNameHigh>__2;
            internal string <artAssetNameLow>__4;
            internal string <artAssetNameMid>__3;
            internal GameObject <artRoot>__9;
            internal int <duty>__11;
            internal string <fullPath>__7;
            internal int <i>__1;
            internal LevelResAsset <levelArtist>__6;
            internal string[] <levelNames>__5;
            internal int <lod>__8;
            internal int <oldProgress>__0;
            internal Transform <staticRoot>__10;
            internal GameLoader.LoaderHelperWrapper InWrapper;

            [DebuggerHidden]
            public void Dispose()
            {
                this.$PC = -1;
            }

            public bool MoveNext()
            {
                uint num = (uint) this.$PC;
                this.$PC = -1;
                switch (num)
                {
                    case 0:
                        this.<oldProgress>__0 = this.<>f__this.nProgress;
                        this.<i>__1 = 0;
                        goto Label_0227;

                    case 1:
                        this.$PC = -1;
                        goto Label_02C5;

                    default:
                        goto Label_02C5;
                }
            Label_018B:
                if (null != this.<levelArtist>__6)
                {
                    this.<artRoot>__9 = GameLoader.s_serializer.Load(this.<levelArtist>__6);
                    if (null != this.<artRoot>__9)
                    {
                        this.<staticRoot>__10 = this.<artRoot>__9.transform.Find("StaticMesh");
                        if (null != this.<staticRoot>__10)
                        {
                            StaticBatchingUtility.Combine(this.<staticRoot>__10.gameObject);
                        }
                        Singleton<CResourceManager>.GetInstance().RemoveCachedResource(this.<fullPath>__7);
                    }
                }
                this.<i>__1++;
            Label_0227:
                if (this.<i>__1 < this.<>f__this.levelArtistList.Count)
                {
                    this.<artAssetNameHigh>__2 = this.<>f__this.levelArtistList[this.<i>__1] + "/" + this.<>f__this.levelArtistList[this.<i>__1];
                    this.<artAssetNameMid>__3 = this.<artAssetNameHigh>__2.Replace("_High", "_Mid");
                    this.<artAssetNameLow>__4 = this.<artAssetNameHigh>__2.Replace("_High", "_Low");
                    this.<levelNames>__5 = new string[] { this.<artAssetNameHigh>__2, this.<artAssetNameMid>__3, this.<artAssetNameLow>__4 };
                    this.<levelArtist>__6 = null;
                    this.<fullPath>__7 = string.Empty;
                    this.<lod>__8 = GameSettings.ModelLOD;
                    this.<lod>__8 = Mathf.Clamp(this.<lod>__8, 0, 2);
                    while (this.<lod>__8 >= 0)
                    {
                        this.<fullPath>__7 = "SceneExport/Artist/" + this.<levelNames>__5[this.<lod>__8] + ".asset";
                        this.<levelArtist>__6 = (LevelResAsset) Singleton<CResourceManager>.GetInstance().GetResource(this.<fullPath>__7, typeof(LevelResAsset), enResourceType.BattleScene, false, false).m_content;
                        if (null != this.<levelArtist>__6)
                        {
                            break;
                        }
                        this.<lod>__8--;
                    }
                    goto Label_018B;
                }
                Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("Scene");
                this.<duty>__11 = this.InWrapper.duty;
                this.<>f__this.nProgress = this.<oldProgress>__0 + this.<duty>__11;
                this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                Singleton<RenderWorker>.GetInstance().BeginLevel();
                this.$current = 0;
                this.$PC = 1;
                return true;
            Label_02C5:
                return false;
            }

            [DebuggerHidden]
            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <LoadCommonAssetBundle>c__IteratorB : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal GameLoader.LoaderHelperWrapper <$>InWrapper;
            internal GameLoader <>f__this;
            internal int <duty>__0;
            internal int <oldProgress>__1;
            internal GameLoader.LoaderHelperWrapper InWrapper;

            [DebuggerHidden]
            public void Dispose()
            {
                this.$PC = -1;
            }

            public bool MoveNext()
            {
                uint num = (uint) this.$PC;
                this.$PC = -1;
                switch (num)
                {
                    case 0:
                        this.<duty>__0 = this.InWrapper.duty;
                        this.<oldProgress>__1 = this.<>f__this.nProgress;
                        Singleton<CResourceManager>.GetInstance().LoadAssetBundle("AssetBundle/Hero_CommonRes.assetbundle");
                        this.$current = 0;
                        this.$PC = 1;
                        goto Label_0168;

                    case 1:
                        Singleton<CResourceManager>.GetInstance().LoadAssetBundle("AssetBundle/Skill_CommonEffect1.assetbundle");
                        this.$current = 0;
                        this.$PC = 2;
                        goto Label_0168;

                    case 2:
                        Singleton<CResourceManager>.GetInstance().LoadAssetBundle("AssetBundle/Skill_CommonEffect2.assetbundle");
                        this.$current = 0;
                        this.$PC = 3;
                        goto Label_0168;

                    case 3:
                        Singleton<CResourceManager>.GetInstance().LoadAssetBundle("AssetBundle/Systems_Effects.assetbundle");
                        this.$current = 0;
                        this.$PC = 4;
                        goto Label_0168;

                    case 4:
                        Singleton<CResourceManager>.GetInstance().LoadAssetBundle("AssetBundle/UGUI_Talent.assetbundle");
                        Singleton<CResourceManager>.GetInstance().LoadAssetBundle("AssetBundle/UGUI_Map.assetbundle");
                        this.<>f__this.nProgress = this.<oldProgress>__1 + this.<duty>__0;
                        this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                        this.$current = 0;
                        this.$PC = 5;
                        goto Label_0168;

                    case 5:
                        this.$PC = -1;
                        break;
                }
                return false;
            Label_0168:
                return true;
            }

            [DebuggerHidden]
            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <LoadCommonAssets>c__IteratorF : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal GameLoader.LoaderHelperWrapper <$>InWrapper;
            internal GameLoader <>f__this;
            internal int <duty>__2;
            internal int <i>__1;
            internal int <oldProgress>__0;
            internal GameLoader.LoaderHelperWrapper InWrapper;

            [DebuggerHidden]
            public void Dispose()
            {
                this.$PC = -1;
            }

            public bool MoveNext()
            {
                uint num = (uint) this.$PC;
                this.$PC = -1;
                switch (num)
                {
                    case 0:
                        this.<oldProgress>__0 = this.<>f__this.nProgress;
                        this.<i>__1 = 0;
                        while (this.<i>__1 < this.<>f__this.soundBankList.Count)
                        {
                            Singleton<CSoundManager>.instance.LoadBank(this.<>f__this.soundBankList[this.<i>__1], CSoundManager.BankType.Battle);
                            this.<i>__1++;
                        }
                        this.$current = 0;
                        this.$PC = 1;
                        goto Label_011A;

                    case 1:
                        MonoSingleton<SceneMgr>.instance.PreloadCommonAssets();
                        this.<duty>__2 = this.InWrapper.duty;
                        this.<>f__this.nProgress = this.<oldProgress>__0 + this.<duty>__2;
                        this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                        this.$current = 0;
                        this.$PC = 2;
                        goto Label_011A;

                    case 2:
                        this.$PC = -1;
                        break;
                }
                return false;
            Label_011A:
                return true;
            }

            [DebuggerHidden]
            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <LoadCommonEffect>c__IteratorC : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal GameLoader.LoaderHelperWrapper <$>InWrapper;
            internal GameLoader <>f__this;
            internal int <duty>__0;
            internal int <oldProgress>__1;
            internal GameLoader.LoaderHelperWrapper InWrapper;

            [DebuggerHidden]
            public void Dispose()
            {
                this.$PC = -1;
            }

            public bool MoveNext()
            {
                uint num = (uint) this.$PC;
                this.$PC = -1;
                switch (num)
                {
                    case 0:
                        this.<duty>__0 = this.InWrapper.duty;
                        this.<oldProgress>__1 = this.<>f__this.nProgress;
                        MonoSingleton<SceneMgr>.instance.PreloadCommonEffects();
                        this.$current = 0;
                        this.$PC = 1;
                        goto Label_00C4;

                    case 1:
                        this.<>f__this.nProgress = this.<oldProgress>__1 + this.<duty>__0;
                        this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                        this.$current = 0;
                        this.$PC = 2;
                        goto Label_00C4;

                    case 2:
                        this.$PC = -1;
                        break;
                }
                return false;
            Label_00C4:
                return true;
            }

            [DebuggerHidden]
            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <LoadDesignLevel>c__IteratorE : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal GameLoader.LoaderHelperWrapper <$>InWrapper;
            internal GameLoader <>f__this;
            internal CBinaryObject <binaryObject>__6;
            internal string <desgineAssetNameHigh>__2;
            internal string <desgineAssetNameLow>__4;
            internal string <desgineAssetNameMid>__3;
            internal GameObject <designRoot>__9;
            internal int <duty>__11;
            internal string <fullPath>__7;
            internal int <i>__1;
            internal string[] <levelNames>__5;
            internal int <lod>__8;
            internal int <oldProgress>__0;
            internal Transform <staticRoot>__10;
            internal GameLoader.LoaderHelperWrapper InWrapper;

            [DebuggerHidden]
            public void Dispose()
            {
                this.$PC = -1;
            }

            public bool MoveNext()
            {
                uint num = (uint) this.$PC;
                this.$PC = -1;
                switch (num)
                {
                    case 0:
                        this.<oldProgress>__0 = this.<>f__this.nProgress;
                        this.<i>__1 = 0;
                        goto Label_022C;

                    case 1:
                        this.$PC = -1;
                        goto Label_02CA;

                    default:
                        goto Label_02CA;
                }
            Label_018B:
                if (null != this.<binaryObject>__6)
                {
                    this.<designRoot>__9 = GameLoader.s_serializer.Load(this.<binaryObject>__6.m_data);
                    if (null != this.<designRoot>__9)
                    {
                        this.<staticRoot>__10 = this.<designRoot>__9.transform.Find("StaticMesh");
                        if (null != this.<staticRoot>__10)
                        {
                            StaticBatchingUtility.Combine(this.<staticRoot>__10.gameObject);
                        }
                        Singleton<CResourceManager>.GetInstance().RemoveCachedResource(this.<fullPath>__7);
                    }
                }
                this.<i>__1++;
            Label_022C:
                if (this.<i>__1 < this.<>f__this.levelDesignList.Count)
                {
                    this.<desgineAssetNameHigh>__2 = this.<>f__this.levelDesignList[this.<i>__1] + "/" + this.<>f__this.levelDesignList[this.<i>__1];
                    this.<desgineAssetNameMid>__3 = this.<desgineAssetNameHigh>__2.Replace("_High", "_Mid");
                    this.<desgineAssetNameLow>__4 = this.<desgineAssetNameHigh>__2.Replace("_High", "_Low");
                    this.<levelNames>__5 = new string[] { this.<desgineAssetNameHigh>__2, this.<desgineAssetNameMid>__3, this.<desgineAssetNameLow>__4 };
                    this.<binaryObject>__6 = null;
                    this.<fullPath>__7 = string.Empty;
                    this.<lod>__8 = GameSettings.ModelLOD;
                    this.<lod>__8 = Mathf.Clamp(this.<lod>__8, 0, 2);
                    while (this.<lod>__8 >= 0)
                    {
                        this.<fullPath>__7 = "SceneExport/Design/" + this.<levelNames>__5[this.<lod>__8] + ".bytes";
                        this.<binaryObject>__6 = Singleton<CResourceManager>.GetInstance().GetResource(this.<fullPath>__7, typeof(TextAsset), enResourceType.BattleScene, false, false).m_content as CBinaryObject;
                        if (null != this.<binaryObject>__6)
                        {
                            break;
                        }
                        this.<lod>__8--;
                    }
                    goto Label_018B;
                }
                Singleton<CResourceManager>.GetInstance().UnloadAssetBundlesByTag("Scene");
                Singleton<SceneManagement>.GetInstance().InitScene();
                this.<duty>__11 = this.InWrapper.duty;
                this.<>f__this.nProgress = this.<oldProgress>__0 + this.<duty>__11;
                this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                this.$current = 0;
                this.$PC = 1;
                return true;
            Label_02CA:
                return false;
            }

            [DebuggerHidden]
            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <LoadNoActorAssets>c__Iterator11 : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal GameLoader.LHCWrapper <$>InWrapper;
            internal GameLoader <>f__this;
            internal AGE.Action <action>__23;
            internal int <count>__8;
            internal int <currentParticleLOD>__12;
            internal int <duty>__1;
            internal GameObject <gameObj>__20;
            internal int <i>__22;
            internal int <i>__5;
            internal int <index>__9;
            internal int <j>__10;
            internal int <j>__15;
            internal int <j>__18;
            internal LoaderHelperCamera <lhc>__0;
            internal LoaderHelper <loadHelper>__2;
            internal int <lod>__13;
            internal ActorMeta <meta>__6;
            internal int <oldProgress>__3;
            internal ActorPreloadTab <otherLoad>__4;
            internal string <parPathKey>__14;
            internal string <path>__7;
            internal Dictionary<object, AssetRefType> <refAssets>__24;
            internal SpriteRenderer <sr>__21;
            internal CResource <tempObj>__19;
            internal ListView<Texture> <textures>__17;
            internal GameObject <tmpObj>__11;
            internal GameObject <tmpObj>__16;
            internal GameLoader.LHCWrapper InWrapper;

            [DebuggerHidden]
            public void Dispose()
            {
                this.$PC = -1;
            }

            public bool MoveNext()
            {
                uint num = (uint) this.$PC;
                this.$PC = -1;
                switch (num)
                {
                    case 0:
                        this.<lhc>__0 = this.InWrapper.lhc;
                        this.<duty>__1 = this.InWrapper.duty;
                        this.<loadHelper>__2 = this.InWrapper.loadHelper;
                        this.<oldProgress>__3 = this.<>f__this.nProgress;
                        this.<otherLoad>__4 = this.<loadHelper>__2.GetGlobalPreload();
                        this.<i>__5 = 0;
                        while (this.<i>__5 < this.<>f__this.actorList.Count)
                        {
                            this.<meta>__6 = this.<>f__this.actorList[this.<i>__5];
                            if (this.<meta>__6.ActorType == ActorTypeDef.Actor_Type_Hero)
                            {
                                this.<path>__7 = KillNotifyUT.GetHero_Icon((uint) this.<meta>__6.ConfigId, 0, false);
                                if (!string.IsNullOrEmpty(this.<path>__7))
                                {
                                    this.<otherLoad>__4.AddSprite(this.<path>__7);
                                }
                            }
                            this.<i>__5++;
                        }
                        this.<count>__8 = ((this.<otherLoad>__4.parPrefabs.Count + this.<otherLoad>__4.mesPrefabs.Count) + this.<otherLoad>__4.spritePrefabs.Count) + this.<otherLoad>__4.ageActions.Count;
                        this.<index>__9 = 0;
                        this.<j>__10 = 0;
                        while (this.<j>__10 < this.<otherLoad>__4.parPrefabs.Count)
                        {
                            this.<tmpObj>__11 = null;
                            AssetLoadBase base2 = this.<otherLoad>__4.parPrefabs[this.<j>__10];
                            if (!this.<lhc>__0.HasLoaded(base2.assetPath))
                            {
                                this.<currentParticleLOD>__12 = GameSettings.ParticleLOD;
                                this.<lod>__13 = this.<currentParticleLOD>__12;
                                while (this.<lod>__13 <= 2)
                                {
                                    AssetLoadBase base3 = this.<otherLoad>__4.parPrefabs[this.<j>__10];
                                    this.<parPathKey>__14 = base3.assetPath + "_lod_" + this.<lod>__13;
                                    if (!this.<lhc>__0.HasLoaded(this.<parPathKey>__14))
                                    {
                                        GameSettings.ParticleLOD = this.<lod>__13;
                                        AssetLoadBase base4 = this.<otherLoad>__4.parPrefabs[this.<j>__10];
                                        this.<tmpObj>__11 = MonoSingleton<SceneMgr>.instance.GetPooledGameObjLOD(base4.assetPath, true, SceneObjType.ActionRes, Vector3.zero);
                                        this.<lhc>__0.AddObj(this.<parPathKey>__14, this.<tmpObj>__11);
                                    }
                                    this.<lod>__13++;
                                }
                                GameSettings.ParticleLOD = this.<currentParticleLOD>__12;
                            }
                            this.<index>__9++;
                            if (!this.<>f__this.ShouldYieldReturn())
                            {
                                goto Label_02FC;
                            }
                            this.$current = 0;
                            this.$PC = 1;
                            goto Label_0735;
                        Label_02D3:
                            this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__3, this.<duty>__1, this.<index>__9, this.<count>__8);
                        Label_02FC:
                            this.<j>__10++;
                        }
                        this.<j>__15 = 0;
                        while (this.<j>__15 < this.<otherLoad>__4.mesPrefabs.Count)
                        {
                            this.<tmpObj>__16 = null;
                            AssetLoadBase base5 = this.<otherLoad>__4.mesPrefabs[this.<j>__15];
                            if (!this.<lhc>__0.HasLoaded(base5.assetPath))
                            {
                                AssetLoadBase base6 = this.<otherLoad>__4.mesPrefabs[this.<j>__15];
                                this.<tmpObj>__16 = MonoSingleton<SceneMgr>.instance.GetPooledGameObjLOD(base6.assetPath, false, SceneObjType.ActionRes, Vector3.zero);
                                AssetLoadBase base7 = this.<otherLoad>__4.mesPrefabs[this.<j>__15];
                                this.<lhc>__0.AddObj(base7.assetPath, this.<tmpObj>__16);
                            }
                            this.<index>__9++;
                            if (!this.<>f__this.ShouldYieldReturn())
                            {
                                goto Label_042C;
                            }
                            this.$current = 0;
                            this.$PC = 2;
                            goto Label_0735;
                        Label_0403:
                            this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__3, this.<duty>__1, this.<index>__9, this.<count>__8);
                        Label_042C:
                            this.<j>__15++;
                        }
                        this.<textures>__17 = new ListView<Texture>();
                        this.<j>__18 = 0;
                        while (this.<j>__18 < this.<otherLoad>__4.spritePrefabs.Count)
                        {
                            AssetLoadBase base8 = this.<otherLoad>__4.spritePrefabs[this.<j>__18];
                            this.<tempObj>__19 = Singleton<CResourceManager>.instance.GetResource(base8.assetPath, typeof(GameObject), enResourceType.UIPrefab, true, false);
                            if (((this.<tempObj>__19 != null) && (this.<tempObj>__19.m_content != null)) && (this.<tempObj>__19.m_content is GameObject))
                            {
                                this.<gameObj>__20 = (GameObject) this.<tempObj>__19.m_content;
                                this.<sr>__21 = this.<gameObj>__20.GetComponent<SpriteRenderer>();
                                if ((this.<sr>__21.sprite != null) && (this.<sr>__21.sprite.texture != null))
                                {
                                    this.<textures>__17.Add(this.<sr>__21.sprite.texture);
                                }
                            }
                            this.<index>__9++;
                            if (!this.<>f__this.ShouldYieldReturn())
                            {
                                goto Label_05B0;
                            }
                            this.$current = 0;
                            this.$PC = 3;
                            goto Label_0735;
                        Label_0587:
                            this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__3, this.<duty>__1, this.<index>__9, this.<count>__8);
                        Label_05B0:
                            this.<j>__18++;
                        }
                        this.<textures>__17.Clear();
                        this.<i>__22 = 0;
                        while (this.<i>__22 < this.<otherLoad>__4.ageActions.Count)
                        {
                            AssetLoadBase base9 = this.<otherLoad>__4.ageActions[this.<i>__22];
                            this.<action>__23 = MonoSingleton<ActionManager>.instance.LoadActionResource(base9.assetPath);
                            if (this.<action>__23 != null)
                            {
                                this.<refAssets>__24 = this.<loadHelper>__2.GetRefAssets(0);
                                this.<action>__23.GetAssociatedResources(this.<refAssets>__24, 0);
                            }
                            this.<index>__9++;
                            if (!this.<>f__this.ShouldYieldReturn())
                            {
                                goto Label_06AD;
                            }
                            this.$current = 0;
                            this.$PC = 4;
                            goto Label_0735;
                        Label_0684:
                            this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__3, this.<duty>__1, this.<index>__9, this.<count>__8);
                        Label_06AD:
                            this.<i>__22++;
                        }
                        this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__3, this.<duty>__1, 1, 1);
                        this.$current = 0;
                        this.$PC = 5;
                        goto Label_0735;

                    case 1:
                        goto Label_02D3;

                    case 2:
                        goto Label_0403;

                    case 3:
                        goto Label_0587;

                    case 4:
                        goto Label_0684;

                    case 5:
                        this.<>f__this.UpdateProgress(this.<lhc>__0, this.<oldProgress>__3, this.<duty>__1, 1, 1);
                        this.$PC = -1;
                        break;
                }
                return false;
            Label_0735:
                return true;
            }

            [DebuggerHidden]
            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <PreSpawnSoldiers>c__Iterator15 : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal GameLoader.LoaderHelperWrapper <$>InWrapper;
            internal GameLoader <>f__this;
            internal ActorMeta <actorMeta>__9;
            internal string <actorName>__11;
            internal int <count>__10;
            internal int <duty>__1;
            internal int <i>__3;
            internal int <i>__7;
            internal int <j>__12;
            internal PoolObjHandle<ActorRoot> <monster>__13;
            internal ActorRoot <monsterActor>__14;
            internal int <num>__5;
            internal int <oldProgress>__0;
            internal ActorPreloadTab <preloadTab>__4;
            internal ActorPreloadTab <preloadTab>__8;
            internal int <spawnCountTotal>__2;
            internal int <spawnIndex>__6;
            internal GameLoader.LoaderHelperWrapper InWrapper;

            [DebuggerHidden]
            public void Dispose()
            {
                this.$PC = -1;
            }

            public bool MoveNext()
            {
                uint num = (uint) this.$PC;
                this.$PC = -1;
                switch (num)
                {
                    case 0:
                        this.<oldProgress>__0 = this.<>f__this.nProgress;
                        this.<duty>__1 = this.InWrapper.duty;
                        this.<spawnCountTotal>__2 = 0;
                        this.<i>__3 = 0;
                        while (this.<i>__3 < this.<>f__this.actorPreload.Count)
                        {
                            this.<preloadTab>__4 = this.<>f__this.actorPreload[this.<i>__3];
                            this.<num>__5 = Mathf.Max(Mathf.RoundToInt(this.<preloadTab>__4.spawnCnt), 1);
                            if (this.<preloadTab>__4.theActor.ActorType == ActorTypeDef.Actor_Type_Monster)
                            {
                                this.<spawnCountTotal>__2 += this.<num>__5;
                            }
                            this.<i>__3++;
                        }
                        GameObjMgr.isPreSpawnActors = true;
                        this.<spawnIndex>__6 = 0;
                        this.<i>__7 = 0;
                        while (this.<i>__7 < this.<>f__this.actorPreload.Count)
                        {
                            this.<preloadTab>__8 = this.<>f__this.actorPreload[this.<i>__7];
                            this.<actorMeta>__9 = this.<preloadTab>__8.theActor;
                            if (this.<actorMeta>__9.ActorType == ActorTypeDef.Actor_Type_Monster)
                            {
                                this.<count>__10 = Mathf.Max(Mathf.RoundToInt(this.<preloadTab>__8.spawnCnt), 1);
                                this.<actorName>__11 = null;
                                this.<j>__12 = 0;
                                while (this.<j>__12 < this.<count>__10)
                                {
                                    this.<monster>__13 = Singleton<GameObjMgr>.GetInstance().SpawnActorEx(null, ref this.<actorMeta>__9, VInt3.zero, VInt3.forward, false, true);
                                    if (this.<monster>__13 != 0)
                                    {
                                        this.<monsterActor>__14 = this.<monster>__13.handle;
                                        this.<monsterActor>__14.InitActor();
                                        this.<monsterActor>__14.PrepareFight();
                                        this.<monsterActor>__14.gameObject.name = this.<monsterActor>__14.TheStaticData.TheResInfo.Name;
                                        this.<monsterActor>__14.StartFight();
                                        if (this.<actorName>__11 == null)
                                        {
                                            this.<actorName>__11 = this.<monsterActor>__14.TheStaticData.TheResInfo.Name;
                                        }
                                        Singleton<GameObjMgr>.instance.AddToCache(this.<monster>__13);
                                    }
                                    if (!this.<>f__this.ShouldYieldReturn())
                                    {
                                        goto Label_0282;
                                    }
                                    this.$current = 0;
                                    this.$PC = 1;
                                    goto Label_033E;
                                Label_0253:
                                    this.<>f__this.UpdateProgress(null, this.<oldProgress>__0, this.<duty>__1, ++this.<spawnIndex>__6, this.<spawnCountTotal>__2);
                                Label_0282:
                                    this.<j>__12++;
                                }
                            }
                            this.<i>__7++;
                        }
                        this.<>f__this.nProgress = this.<oldProgress>__0 + this.<duty>__1;
                        this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                        this.$current = 0;
                        this.$PC = 2;
                        goto Label_033E;

                    case 1:
                        goto Label_0253;

                    case 2:
                        GameObjMgr.isPreSpawnActors = false;
                        HudComponent3D.PreallocMapPointer(20, 40);
                        SObjPool<SRefParam>.Alloc(0x400);
                        this.$PC = -1;
                        break;
                }
                return false;
            Label_033E:
                return true;
            }

            [DebuggerHidden]
            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <SpawnDynamicActor>c__Iterator14 : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal GameLoader.LoaderHelperWrapper <$>InWrapper;
            internal GameLoader <>f__this;
            internal PoolObjHandle<ActorRoot> <actor>__6;
            internal ActorMeta <actorMeta>__3;
            internal VInt3 <bornDir>__5;
            internal VInt3 <bornPos>__4;
            internal int <duty>__0;
            internal int <i>__2;
            internal int <oldProgress>__1;
            internal GameLoader.LoaderHelperWrapper InWrapper;

            [DebuggerHidden]
            public void Dispose()
            {
                this.$PC = -1;
            }

            public bool MoveNext()
            {
                uint num = (uint) this.$PC;
                this.$PC = -1;
                switch (num)
                {
                    case 0:
                        this.<duty>__0 = this.InWrapper.duty;
                        this.<oldProgress>__1 = this.<>f__this.nProgress;
                        this.<i>__2 = 0;
                        break;

                    case 1:
                        this.<i>__2++;
                        break;

                    default:
                        goto Label_01F1;
                }
                if (this.<i>__2 < this.<>f__this.actorList.Count)
                {
                    this.<actorMeta>__3 = this.<>f__this.actorList[this.<i>__2];
                    this.<bornPos>__4 = new VInt3();
                    this.<bornDir>__5 = new VInt3();
                    if (this.<actorMeta>__3.ActorType == ActorTypeDef.Actor_Type_Hero)
                    {
                        DebugHelper.Assert(Singleton<BattleLogic>.instance.mapLogic != null, "what? BattleLogic.instance.mapLogic==null");
                        Singleton<BattleLogic>.GetInstance().mapLogic.GetRevivePosDir(ref this.<actorMeta>__3, true, out this.<bornPos>__4, out this.<bornDir>__5);
                    }
                    this.<actor>__6 = Singleton<GameObjMgr>.instance.SpawnActorEx(null, ref this.<actorMeta>__3, this.<bornPos>__4, this.<bornDir>__5, false, true);
                    if (this.<actor>__6 != 0)
                    {
                        Singleton<GameObjMgr>.GetInstance().HoldDynamicActor(this.<actor>__6);
                    }
                    this.<>f__this.nProgress = this.<oldProgress>__1 + ((this.<duty>__0 * (this.<i>__2 + 1)) / this.<>f__this.actorList.Count);
                    this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                    this.$current = 0;
                    this.$PC = 1;
                    return true;
                }
                this.<>f__this.nProgress = this.<oldProgress>__1 + this.<duty>__0;
                this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                this.$PC = -1;
            Label_01F1:
                return false;
            }

            [DebuggerHidden]
            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <SpawnStaticActor>c__Iterator13 : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal GameLoader.LoaderHelperWrapper <$>InWrapper;
            internal GameLoader <>f__this;
            internal PoolObjHandle<ActorRoot> <actor>__6;
            internal ActorMeta <actorMeta>__3;
            internal VInt3 <bornDir>__5;
            internal VInt3 <bornPos>__4;
            internal int <duty>__0;
            internal int <i>__2;
            internal int <oldProgress>__1;
            internal GameLoader.LoaderHelperWrapper InWrapper;

            [DebuggerHidden]
            public void Dispose()
            {
                this.$PC = -1;
            }

            public bool MoveNext()
            {
                uint num = (uint) this.$PC;
                this.$PC = -1;
                switch (num)
                {
                    case 0:
                        this.<duty>__0 = this.InWrapper.duty;
                        this.<oldProgress>__1 = this.<>f__this.nProgress;
                        this.<i>__2 = 0;
                        break;

                    case 1:
                        this.<i>__2++;
                        break;

                    case 2:
                        this.<>f__this.staticActors.Clear();
                        this.$PC = -1;
                        goto Label_0287;

                    default:
                        goto Label_0287;
                }
                if (this.<i>__2 < this.<>f__this.staticActors.Count)
                {
                    this.<actorMeta>__3 = new ActorMeta();
                    this.<actorMeta>__3.ActorType = this.<>f__this.staticActors[this.<i>__2].ActorType;
                    this.<actorMeta>__3.ConfigId = this.<>f__this.staticActors[this.<i>__2].ConfigID;
                    this.<actorMeta>__3.ActorCamp = this.<>f__this.staticActors[this.<i>__2].CmpType;
                    this.<bornPos>__4 = (VInt3) this.<>f__this.staticActors[this.<i>__2].transform.position;
                    this.<bornDir>__5 = (VInt3) this.<>f__this.staticActors[this.<i>__2].transform.forward;
                    this.<actor>__6 = Singleton<GameObjMgr>.instance.SpawnActorEx(this.<>f__this.staticActors[this.<i>__2].gameObject, ref this.<actorMeta>__3, this.<bornPos>__4, this.<bornDir>__5, false, true);
                    if (this.<actor>__6 != 0)
                    {
                        Singleton<GameObjMgr>.GetInstance().HoldStaticActor(this.<actor>__6);
                    }
                    this.<>f__this.nProgress = this.<oldProgress>__1 + ((this.<duty>__0 * (this.<i>__2 + 1)) / this.<>f__this.staticActors.Count);
                    this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                    this.$current = 0;
                    this.$PC = 1;
                }
                else
                {
                    this.<>f__this.nProgress = this.<oldProgress>__1 + this.<duty>__0;
                    this.<>f__this.LoadProgressEvent(this.<>f__this.nProgress * 0.0001f);
                    this.$current = 0;
                    this.$PC = 2;
                }
                return true;
            Label_0287:
                return false;
            }

            [DebuggerHidden]
            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LHCWrapper
        {
            public LoaderHelperCamera lhc;
            public LoaderHelper loadHelper;
            public int duty;
        }

        public delegate void LoadCompleteDelegate();

        [StructLayout(LayoutKind.Sequential)]
        private struct LoaderHelperWrapper
        {
            public LoaderHelper loadHelper;
            public int duty;
        }

        public delegate void LoadProgressDelegate(float progress);
    }
}

