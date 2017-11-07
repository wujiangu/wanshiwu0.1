using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cs
{
    /// <summary>
    /// 事件ID命名规则：
    /// 1、首字母按(a,b,c等)排序；
    /// 2、命名按照类型+功能(例如：打开/关闭背包----->PackageOpen/PackageClose);
    /// </summary>
    public enum EEventID
    {
        Invalid = 0,

        /// <summary>
        /// 引擎内置脚本加载完成
        /// </summary>
        EngineScriptLoaded,

        /// <summary>
        /// 游戏状态发生变化
        /// </summary>
        GameStateChanged,

        BuildInCount,
    }
}
