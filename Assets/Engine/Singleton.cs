using UnityEngine;
using System.Collections;

namespace cs
{
	
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static T ms_singleton = null;

        /// <summary>
        /// 获取单例
        /// </summary>
        /// <returns></returns>
        public static T Get()
        {
            if (ms_singleton == null)
            {
                ms_singleton = new T();
                ms_singleton._Initialize();
            }

            return ms_singleton;
        }
			
        /// <summary>
        /// 删除单例
        /// </summary>
        public static void Destroy()
        {
            if (ms_singleton != null)
            {
                ms_singleton._Clear();
                ms_singleton = null;
            }
        }

        /// <summary>
        /// 初始化实现
        /// </summary>
        protected abstract void _Initialize();

        /// <summary>
        /// 删除实现
        /// </summary>
        protected abstract void _Clear();
    }
}


