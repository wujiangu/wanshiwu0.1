namespace Assets.Scripts.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.UI;

    public class CUIHttpImageScript : CUIComponent
    {
        public bool m_cacheTexture = true;
        public bool m_forceSetImageUrl;
        private enHttpImageState m_httpImageState;
        private Image m_image;
        private ImageAlphaTexLayout m_imageDefaultAlphaTexLayout;
        private Sprite m_imageDefaultSprite;
        public string m_imageUrl;
        public bool m_setNativeSize;
        private static CCachedTextureManager s_cachedTextureManager;

        [DebuggerHidden]
        private IEnumerator DownloadImage(string url)
        {
            return new <DownloadImage>c__Iterator22 { url = url, <$>url = url, <>f__this = this };
        }

        public override void Initialize(CUIFormScript formScript)
        {
            if (!base.m_isInitialized)
            {
                base.Initialize(formScript);
                this.m_image = base.gameObject.GetComponent<Image>();
                this.m_imageDefaultSprite = this.m_image.sprite;
                if (this.m_image is Image2)
                {
                    this.m_imageDefaultAlphaTexLayout = (this.m_image as Image2).alphaTexLayout;
                }
                if (this.m_cacheTexture && (s_cachedTextureManager == null))
                {
                    s_cachedTextureManager = new CCachedTextureManager();
                }
                this.m_httpImageState = enHttpImageState.Unload;
                if (base.gameObject.activeInHierarchy && !string.IsNullOrEmpty(this.m_imageUrl))
                {
                    this.LoadTexture(this.m_imageUrl);
                }
            }
        }

        private void LoadTexture(string url)
        {
            if (this.m_httpImageState != enHttpImageState.Loaded)
            {
                if (this.m_cacheTexture)
                {
                    Texture2D cachedTexture = s_cachedTextureManager.GetCachedTexture(url);
                    if (cachedTexture != null)
                    {
                        if (this.m_image != null)
                        {
                            this.m_image.SetSprite(Sprite.Create(cachedTexture, new Rect(0f, 0f, (float) cachedTexture.width, (float) cachedTexture.height), new Vector2(0.5f, 0.5f)), ImageAlphaTexLayout.None);
                            if (this.m_setNativeSize)
                            {
                                this.SetNativeSize();
                            }
                            this.m_httpImageState = enHttpImageState.Loaded;
                        }
                    }
                    else
                    {
                        base.StartCoroutine(this.DownloadImage(url));
                    }
                }
                else
                {
                    base.StartCoroutine(this.DownloadImage(url));
                }
            }
        }

        private void OnDisable()
        {
            if (base.m_isInitialized && (this.m_httpImageState == enHttpImageState.Loading))
            {
                base.StopAllCoroutines();
                this.m_httpImageState = enHttpImageState.Unload;
            }
        }

        private void OnEnable()
        {
            if ((base.m_isInitialized && (this.m_httpImageState == enHttpImageState.Unload)) && !string.IsNullOrEmpty(this.m_imageUrl))
            {
                this.LoadTexture(this.m_imageUrl);
            }
        }

        public void SetImageUrl(string url)
        {
            if (!string.IsNullOrEmpty(url) && (this.m_forceSetImageUrl || !string.Equals(url, this.m_imageUrl)))
            {
                this.m_imageUrl = url;
                if (this.m_image != null)
                {
                    this.m_image.SetSprite(this.m_imageDefaultSprite, this.m_imageDefaultAlphaTexLayout);
                }
                this.m_httpImageState = enHttpImageState.Unload;
                if (base.gameObject.activeInHierarchy)
                {
                    this.LoadTexture(this.m_imageUrl);
                }
            }
        }

        public void SetNativeSize()
        {
            if (this.m_image != null)
            {
                this.m_image.SetNativeSize();
            }
        }

        [CompilerGenerated]
        private sealed class <DownloadImage>c__Iterator22 : IDisposable, IEnumerator, IEnumerator<object>
        {
            internal object $current;
            internal int $PC;
            internal string <$>url;
            internal CUIHttpImageScript <>f__this;
            internal float <fStartTime>__0;
            internal string <imageType>__2;
            internal bool <isGif>__3;
            internal MemoryStream <memoryStream>__5;
            internal Texture2D <texture2D>__4;
            internal WWW <www>__1;
            internal string url;

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
                        this.<fStartTime>__0 = Time.time;
                        this.<>f__this.m_httpImageState = enHttpImageState.Loading;
                        this.<www>__1 = new WWW(this.url);
                        this.$current = this.<www>__1;
                        this.$PC = 1;
                        return true;

                    case 1:
                        if (!string.IsNullOrEmpty(this.<www>__1.error))
                        {
                            Singleton<BeaconHelper>.GetInstance().EventPhotoReport("1", Time.time - this.<fStartTime>__0, this.<www>__1.error);
                            goto Label_024D;
                        }
                        if (this.<>f__this.m_image == null)
                        {
                            goto Label_024D;
                        }
                        this.<imageType>__2 = null;
                        this.<www>__1.responseHeaders.TryGetValue("CONTENT-TYPE", out this.<imageType>__2);
                        this.<isGif>__3 = string.Equals(this.<imageType>__2, "image/gif");
                        this.<texture2D>__4 = null;
                        if (!this.<isGif>__3)
                        {
                            this.<texture2D>__4 = this.<www>__1.texture;
                            break;
                        }
                        this.<memoryStream>__5 = new MemoryStream(this.<www>__1.bytes);
                        try
                        {
                            this.<texture2D>__4 = GifHelper.GifToTexture(this.<memoryStream>__5, 0);
                        }
                        finally
                        {
                            if (this.<memoryStream>__5 != null)
                            {
                                this.<memoryStream>__5.Dispose();
                            }
                        }
                        break;

                    default:
                        goto Label_0254;
                }
                if (this.<texture2D>__4 != null)
                {
                    this.<>f__this.m_image.SetSprite(Sprite.Create(this.<texture2D>__4, new Rect(0f, 0f, (float) this.<texture2D>__4.width, (float) this.<texture2D>__4.height), new Vector2(0.5f, 0.5f)), ImageAlphaTexLayout.None);
                    if (this.<>f__this.m_setNativeSize)
                    {
                        this.<>f__this.SetNativeSize();
                    }
                    this.<>f__this.m_httpImageState = enHttpImageState.Loaded;
                    if (this.<>f__this.m_cacheTexture)
                    {
                        CUIHttpImageScript.s_cachedTextureManager.AddCachedTexture(this.url, this.<texture2D>__4.width, this.<texture2D>__4.height, this.<isGif>__3, this.<www>__1.bytes);
                    }
                    Singleton<BeaconHelper>.GetInstance().EventPhotoReport("0", Time.time - this.<fStartTime>__0, "SUCC");
                }
            Label_024D:
                this.$PC = -1;
            Label_0254:
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
    }
}

