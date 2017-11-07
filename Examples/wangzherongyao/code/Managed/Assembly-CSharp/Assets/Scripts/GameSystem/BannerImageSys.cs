namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using ResData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using tsf4g_tdr_csharp;
    using UnityEngine;
    using UnityEngine.UI;

    public class BannerImageSys : MonoSingleton<BannerImageSys>
    {
        public static string GlobalLoadPath = "http://image.smoba.qq.com/Banner/img/";
        private BannerImage m_BannerImage;
        private bool m_bLoadAllImage;
        private bool m_bLoadXmlSucc;
        private bool m_bTest;
        private BtnControlInfo m_DeepLinkInfo;
        private string m_GlobalBannerImagePath = string.Empty;
        private BtnControlInfo m_QQBoxInfo;

        public void ClearSeverData()
        {
            this.m_BannerImage = null;
        }

        public static int ComparebyShowIdx(BannerImageInfo info1, BannerImageInfo info2)
        {
            if (info1.resImgInfo.dwShowID > info2.resImgInfo.dwShowID)
            {
                return 1;
            }
            if (info1.resImgInfo.dwShowID == info2.resImgInfo.dwShowID)
            {
                return 0;
            }
            return -1;
        }

        public BannerImage GetCurBannerImage()
        {
            return this.m_BannerImage;
        }

        protected override void Init()
        {
            this.m_GlobalBannerImagePath = CFileManager.GetCachePath();
            string cachePath = CFileManager.GetCachePath("BannerImage");
            try
            {
                if (!Directory.Exists(cachePath))
                {
                    Directory.CreateDirectory(cachePath);
                }
                this.m_GlobalBannerImagePath = cachePath;
            }
            catch (Exception exception)
            {
                Debug.Log("bannerimagesys cannot create dictionary " + exception);
                this.m_GlobalBannerImagePath = CFileManager.GetCachePath();
            }
            this.m_DeepLinkInfo.bLoadSucc = false;
            this.m_QQBoxInfo.bLoadSucc = false;
        }

        private bool isPreloadImageType(BannerType kType)
        {
            if (((kType != BannerType.BannerType_InGame) && (kType != BannerType.BannerType_URL)) && ((kType != BannerType.BannerType_Loading) && (kType != BannerType.BannerType_CheckIn)))
            {
                return false;
            }
            return true;
        }

        [DebuggerHidden]
        private IEnumerator LoadConfigBin(string path)
        {
            return new <LoadConfigBin>c__Iterator18 { path = path, <$>path = path, <>f__this = this };
        }

        public void LoadConfigServer()
        {
            this.ClearSeverData();
            int count = GameDataMgr.svr2BannerImageDict.Count;
            if (count > 0)
            {
                this.m_BannerImage = new BannerImage();
                this.m_BannerImage.verisoncode = 0;
                this.m_BannerImage.ImageListCount = count;
                this.m_BannerImage.m_ImageInfoList = new BannerImageInfo[count];
                DictionaryView<uint, ResBannerImage>.Enumerator enumerator = GameDataMgr.svr2BannerImageDict.GetEnumerator();
                for (int i = 0; enumerator.MoveNext(); i++)
                {
                    KeyValuePair<uint, ResBannerImage> current = enumerator.Current;
                    ResBannerImage image = current.Value;
                    this.m_BannerImage.m_ImageInfoList[i] = new BannerImageInfo();
                    this.m_BannerImage.m_ImageInfoList[i].resImgInfo = image;
                    this.m_BannerImage.m_ImageInfoList[i].imgLoadSucc = false;
                }
            }
            if (this.m_BannerImage != null)
            {
                this.PreloadBannerImage();
            }
        }

        private void PreloadBannerImage()
        {
            <PreloadBannerImage>c__AnonStorey45 storey = new <PreloadBannerImage>c__AnonStorey45 {
                <>f__this = this
            };
            if (this.m_BannerImage != null)
            {
                storey.nCount = this.m_BannerImage.ImageListCount;
                for (int i = 0; i < storey.nCount; i++)
                {
                    BannerImageInfo info = this.m_BannerImage.m_ImageInfoList[i];
                    string szImgUrl = info.resImgInfo.szImgUrl;
                    if (this.isPreloadImageType((BannerType) info.resImgInfo.dwBannerType))
                    {
                        this.m_BannerImage.m_ImageInfoList[i].imgLoadSucc = false;
                        szImgUrl = string.Format("{0}{1}", GlobalLoadPath, szImgUrl);
                        base.StartCoroutine(MonoSingleton<IDIPSys>.GetInstance().DownloadImageByTag(szImgUrl, i, new IDIPSys.LoadRCallBack2(storey.<>m__3A), this.GlobalBannerImagePath));
                    }
                    else if (info.resImgInfo.dwBannerType == 3)
                    {
                        this.m_DeepLinkInfo.linkType = (int) info.resImgInfo.dwJumpEntrance;
                        this.m_DeepLinkInfo.linkUrl = info.resImgInfo.szHttpUrl;
                        this.m_DeepLinkInfo.startTime = info.resImgInfo.dwStartTime;
                        this.m_DeepLinkInfo.endTime = info.resImgInfo.dwEndTime;
                        this.m_DeepLinkInfo.bLoadSucc = true;
                    }
                    else if (info.resImgInfo.dwBannerType == 7)
                    {
                        this.m_QQBoxInfo.linkUrl = info.resImgInfo.szHttpUrl;
                        this.m_QQBoxInfo.startTime = info.resImgInfo.dwStartTime;
                        this.m_QQBoxInfo.endTime = info.resImgInfo.dwEndTime;
                        this.m_QQBoxInfo.bLoadSucc = true;
                    }
                }
            }
        }

        public void TrySetCheckInImage(Image img)
        {
            <TrySetCheckInImage>c__AnonStorey46 storey = new <TrySetCheckInImage>c__AnonStorey46 {
                img = img
            };
            if ((this.m_BannerImage != null) && (storey.img != null))
            {
                string str = null;
                for (int i = 0; i < this.m_BannerImage.ImageListCount; i++)
                {
                    ResBannerImage resImgInfo = this.m_BannerImage.m_ImageInfoList[i].resImgInfo;
                    if (resImgInfo.dwBannerType == 6)
                    {
                        str = string.Format("{0}{1}", GlobalLoadPath, resImgInfo.szImgUrl);
                        break;
                    }
                }
                if (string.IsNullOrEmpty(str))
                {
                    str = string.Format("{0}{1}", GlobalLoadPath, "CheckIn/20151028.png");
                }
                if (!string.IsNullOrEmpty(str))
                {
                    base.StartCoroutine(MonoSingleton<IDIPSys>.GetInstance().DownloadImageByTag(str, 0, new IDIPSys.LoadRCallBack2(storey.<>m__3B), MonoSingleton<BannerImageSys>.GetInstance().GlobalBannerImagePath));
                }
            }
        }

        public void TrySetLoadingImage(uint picIndex, Image img)
        {
            <TrySetLoadingImage>c__AnonStorey47 storey = new <TrySetLoadingImage>c__AnonStorey47 {
                img = img
            };
            if ((this.m_BannerImage != null) && (storey.img != null))
            {
                string str = null;
                for (int i = 0; i < this.m_BannerImage.ImageListCount; i++)
                {
                    ResBannerImage resImgInfo = this.m_BannerImage.m_ImageInfoList[i].resImgInfo;
                    if ((resImgInfo.dwBannerType == 4) && (resImgInfo.dwShowID == picIndex))
                    {
                        str = string.Format("{0}{1}", GlobalLoadPath, resImgInfo.szImgUrl);
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(str))
                {
                    base.StartCoroutine(MonoSingleton<IDIPSys>.GetInstance().DownloadImageByTag(str, 0, new IDIPSys.LoadRCallBack2(storey.<>m__3C), MonoSingleton<BannerImageSys>.GetInstance().GlobalBannerImagePath));
                }
            }
        }

        public BtnControlInfo DeepLinkInfo
        {
            get
            {
                return this.m_DeepLinkInfo;
            }
        }

        public string GlobalBannerImagePath
        {
            get
            {
                return this.m_GlobalBannerImagePath;
            }
        }

        public bool LoadALLImage
        {
            get
            {
                return this.m_bLoadAllImage;
            }
        }

        public bool LoadXmlSUCC
        {
            get
            {
                return this.m_bLoadXmlSucc;
            }
        }

        public BtnControlInfo QQBOXInfo
        {
            get
            {
                return this.m_QQBoxInfo;
            }
        }

        [CompilerGenerated]
        private sealed class <LoadConfigBin>c__Iterator18 : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal string <$>path;
            internal BannerImageSys <>f__this;
            internal int <count>__4;
            internal int <i>__6;
            internal System.Type <InValueType>__5;
            internal tsf4g_csharp_interface <record>__7;
            internal TResHeadAll <resHead>__3;
            internal TdrReadBuf <srcBuf>__2;
            internal byte[] <temp>__1;
            internal WWW <www>__0;
            internal string path;

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
                        this.<www>__0 = new WWW(this.path);
                        this.$current = this.<www>__0;
                        this.$PC = 1;
                        return true;

                    case 1:
                        if (!string.IsNullOrEmpty(this.<www>__0.error))
                        {
                            this.<>f__this.m_bLoadXmlSucc = false;
                            BugLocateLogSys.Log("bannerimage xml load error = " + this.<www>__0.error);
                            break;
                        }
                        this.<temp>__1 = this.<www>__0.bytes;
                        this.<srcBuf>__2 = new TdrReadBuf(ref this.<temp>__1, this.<temp>__1.Length);
                        this.<resHead>__3 = new TResHeadAll();
                        this.<resHead>__3.load(ref this.<srcBuf>__2);
                        this.<count>__4 = this.<resHead>__3.mHead.iCount;
                        if (this.<count>__4 > 0)
                        {
                            this.<>f__this.m_BannerImage = new BannerImageSys.BannerImage();
                            this.<>f__this.m_BannerImage.verisoncode = this.<resHead>__3.mHead.iResVersion;
                            this.<>f__this.m_BannerImage.ImageListCount = this.<count>__4;
                            this.<>f__this.m_BannerImage.m_ImageInfoList = new BannerImageSys.BannerImageInfo[this.<count>__4];
                            this.<InValueType>__5 = typeof(ResBannerImage);
                            this.<i>__6 = 0;
                            while (this.<i>__6 < this.<count>__4)
                            {
                                this.<record>__7 = Activator.CreateInstance(this.<InValueType>__5) as tsf4g_csharp_interface;
                                object[] inParameters = new object[] { this.<InValueType>__5.Name };
                                DebugHelper.Assert(this.<record>__7 != null, "Failed Create Object, Type:{0}", inParameters);
                                this.<record>__7.load(ref this.<srcBuf>__2, 0);
                                this.<>f__this.m_BannerImage.m_ImageInfoList[this.<i>__6] = new BannerImageSys.BannerImageInfo();
                                this.<>f__this.m_BannerImage.m_ImageInfoList[this.<i>__6].resImgInfo = this.<record>__7 as ResBannerImage;
                                this.<>f__this.m_BannerImage.m_ImageInfoList[this.<i>__6].imgLoadSucc = false;
                                this.<i>__6++;
                            }
                            this.<>f__this.m_bLoadXmlSucc = true;
                            if (this.<>f__this.m_BannerImage != null)
                            {
                                this.<>f__this.PreloadBannerImage();
                            }
                            break;
                        }
                        goto Label_0274;

                    default:
                        goto Label_0274;
                }
                this.$PC = -1;
            Label_0274:
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
        private sealed class <PreloadBannerImage>c__AnonStorey45
        {
            internal BannerImageSys <>f__this;
            internal int nCount;

            internal void <>m__3A(Texture2D text2, int imageIDX)
            {
                if (imageIDX < this.nCount)
                {
                    this.<>f__this.m_BannerImage.m_ImageInfoList[imageIDX].imgLoadSucc = true;
                }
            }
        }

        [CompilerGenerated]
        private sealed class <TrySetCheckInImage>c__AnonStorey46
        {
            internal Image img;

            internal void <>m__3B(Texture2D text2D, int imageIndex)
            {
                if ((this.img != null) && (text2D != null))
                {
                    this.img.SetSprite(Sprite.Create(text2D, new Rect(0f, 0f, (float) text2D.width, (float) text2D.height), new Vector2(0.5f, 0.5f)), ImageAlphaTexLayout.None);
                }
            }
        }

        [CompilerGenerated]
        private sealed class <TrySetLoadingImage>c__AnonStorey47
        {
            internal Image img;

            internal void <>m__3C(Texture2D text2D, int imageIndex)
            {
                if ((this.img != null) && (text2D != null))
                {
                    this.img.SetSprite(Sprite.Create(text2D, new Rect(0f, 0f, (float) text2D.width, (float) text2D.height), new Vector2(0.5f, 0.5f)), ImageAlphaTexLayout.None);
                }
            }
        }

        public class BannerImage
        {
            public int ImageListCount;
            public BannerImageSys.BannerImageInfo[] m_ImageInfoList;
            public int verisoncode;
        }

        public class BannerImageInfo
        {
            public bool imgLoadSucc = false;
            public ResBannerImage resImgInfo = new ResBannerImage();
        }

        public enum BannerPosition
        {
            Lobby,
            Mall
        }

        public enum BannerType
        {
            BannerType_None,
            BannerType_URL,
            BannerType_InGame,
            BannerType_DeepLink,
            BannerType_Loading,
            BannerType_Outer_Browser,
            BannerType_CheckIn,
            BANNER_TYPE_QQ_BOX
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BtnControlInfo
        {
            public bool bLoadSucc;
            public int linkType;
            public string linkUrl;
            public long startTime;
            public long endTime;
            public bool isTimeValid(long curTime)
            {
                if (!this.bLoadSucc)
                {
                    return false;
                }
                return ((this.startTime < curTime) && (this.endTime >= curTime));
            }
        }
    }
}

