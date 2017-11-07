using Assets.Scripts.Framework;
using Assets.Scripts.UI;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class FPSForm : CUIFormScript
{
    private Color color = Color.white;
    public static string extMsg = string.Empty;
    public Text m_fpsText;
    private string revision = string.Empty;
    public static string sFPS = string.Empty;

    private void Start()
    {
        CBinaryObject content = Singleton<CResourceManager>.GetInstance().GetResource("Revision.txt", typeof(TextAsset), enResourceType.Numeric, false, false).m_content as CBinaryObject;
        if (null != content)
        {
            this.revision = Encoding.UTF8.GetString(content.m_data);
        }
        Singleton<CResourceManager>.GetInstance().RemoveCachedResource("Revision.txt");
    }

    private void Update()
    {
        sFPS = string.Format("{0:#0.0}", GameFramework.m_fFps);
        this.color = (GameFramework.m_fFps < 20f) ? ((GameFramework.m_fFps <= 10f) ? Color.yellow : Color.red) : Color.green;
        this.color.a = 0.3f;
        this.m_fpsText.color = this.color;
        object[] args = new object[] { sFPS, this.revision, Singleton<FrameSynchr>.instance.GameSvrPing, Singleton<FrameSynchr>.instance.GameSvrPing + (Singleton<FrameSynchr>.instance.SvrFrameLater * Singleton<FrameSynchr>.instance.SvrFrameDelta), extMsg };
        this.m_fpsText.text = string.Format("{0} FPS, R:{1}, ping({2})cmdDelay({3}) {4}", args);
    }
}

