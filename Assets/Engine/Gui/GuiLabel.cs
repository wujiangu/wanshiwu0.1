using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using LuaInterface;

namespace cs
{
    /// <summary>
    /// 图文混排和超链接文字
    /// 1、只显示文字，正常传入字符串。如：@"万事屋";
    /// 2、链接显示格式:<a color=#FFFF00 data={a:1, name:'wjg'}>万事屋</a> , 其中color为链接的颜色，可以传入一个16进制数
    ///    data为点击链接的返回值，返回类型为string;
    /// 3、图片显示格式:<quad name=Main/Image/expression/xb_b size=20 width=1 /> , 其中name为图片的路径，此路径是Resouces
    ///    文件夹下的相对路径，size可以调节图片的大小，width可以调节图片在图文混排的图片占位(一般情况下均设置为1)
    /// </summary>
    [RequireComponent(typeof(TextPicLink))]
    public class GuiLabel : GuiControl
    {
        public override bool Initialize()
        {
            if (base.Initialize())
            {
                m_textPicLink = gameObject.GetComponent<TextPicLink>();
                return true;
            }
            return false;
        }

        public override void Clear()
        {
            _ClearScriptCallback();

            if (m_textPicLink != null)
            {
                m_textPicLink.Clear();
            }

            base.Clear();
        }

        /// <summary>
        /// 设置文本的内容和格式
        /// </summary>
        public void SetLabel(string a_strConetnt)
        {
            if (m_textPicLink != null)
            {
                m_textPicLink.text = a_strConetnt;
            }
        }

        /// <summary>
        /// 设置链接点击回调
        /// </summary>
        public void SetHyperlinkCallback(UnityAction<string> a_callback)
        {
            m_textPicLink.onHrefClick.RemoveAllListeners();
            m_textPicLink.onHrefClick.AddListener(a_callback);
        }

        public void SetHyperlinkCallbackScript(LuaFunction a_func)
        {
            _ClearScriptCallback();
            m_luaHyperlinkCallback = a_func;
            if (m_luaHyperlinkCallback != null)
            {
                SetHyperlinkCallback(_OnHyperlinkClicked);
            }
        }

        private void _OnHyperlinkClicked(string a_strData)
        {
            if (m_luaHyperlinkCallback != null)
            {
                m_luaHyperlinkCallback.BeginPCall();
                m_luaHyperlinkCallback.Push(a_strData);
                m_luaHyperlinkCallback.PCall();
                m_luaHyperlinkCallback.EndPCall();
            }
        }

        private void _ClearScriptCallback()
        {
            if (m_luaHyperlinkCallback != null)
            {
                m_luaHyperlinkCallback.Dispose();
                m_luaHyperlinkCallback = null;

                SetHyperlinkCallback(null);
            }
        }

#if UNITY_EDITOR
        private void OnDestroy()
        {
            Clear();
        }
#endif

        private LuaFunction m_luaHyperlinkCallback;
        private TextPicLink m_textPicLink;
    }
}

