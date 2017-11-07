using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace cs
{
    public static class ListPool<T>
    {
        public static List<T> Get()
        {
            return ms_listPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            ms_listPool.Release(toRelease);
        }

        static private ObjectPool<List<T>> ms_listPool = new ObjectPool<List<T>>(null, l => l.Clear(), null);
    }
}

