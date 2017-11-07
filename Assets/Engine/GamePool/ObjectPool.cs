using System.Collections.Generic;
using UnityEngine.Events;

namespace cs
{
    internal class ObjectPool<T> where T : class, new()
    {
        private readonly Stack<T> m_stack = new Stack<T>();
        private readonly UnityAction<T> m_onGet;
        private readonly UnityAction<T> m_onRelease;
        private readonly UnityAction<T> m_onDestroy;

        public int CountAll { get; private set; }
        public int CountActive { get { return CountAll - CountInactive; } }
        public int CountInactive { get { return m_stack.Count; } }

        public ObjectPool(UnityAction<T> a_onnGet, UnityAction<T> a_onRelease, UnityAction<T> a_onDestroy)
        {
            m_onGet = a_onnGet;
            m_onRelease = a_onRelease;
            m_onDestroy = a_onDestroy;
        }

        public T Get()
        {
            T element;
            if (m_stack.Count == 0)
            {
                element = new T();
                CountAll++;
            }
            else
            {
                element = m_stack.Pop();
            }
            if (m_onGet != null)
                m_onGet(element);
            return element;
        }

        public void Release(T element)
        {
            if (m_stack.Count > 0 && ReferenceEquals(m_stack.Peek(), element))
                UnityEngine.Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            if (m_onRelease != null)
                m_onRelease(element);
            m_stack.Push(element);
        }

        /// <summary>
        /// 销毁当前缓存着的对象，降低内存
        /// </summary>
        public void Clear()
        {
            T element;
            while (m_stack.Count > 0)
            {
                element = m_stack.Pop();
                if (m_onDestroy != null)
                {
                    m_onDestroy(element);
                }
                CountAll--;
            }
            element = null;
        }
    }
}

