namespace Assets.Scripts.GameSystem
{
    using System;
    using UnityEngine;

    public class CAnnouncementImage
    {
        public int m_pointElementSequence;
        public Texture2D m_texture2D;
        public string m_url;

        public void Dispose()
        {
            this.m_url = null;
            this.m_texture2D = null;
            this.m_pointElementSequence = 0;
        }
    }
}

