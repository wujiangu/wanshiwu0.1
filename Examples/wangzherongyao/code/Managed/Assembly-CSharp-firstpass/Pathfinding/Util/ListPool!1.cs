namespace Pathfinding.Util
{
    using System;
    using System.Collections.Generic;

    public static class ListPool<T>
    {
        private const int MaxCapacitySearchLength = 8;
        private static List<object> pool;

        static ListPool()
        {
            ListPool<T>.pool = new List<object>();
        }

        public static List<T> Claim()
        {
            List<object> pool = ListPool<T>.pool;
            lock (pool)
            {
                if (ListPool<T>.pool.Count > 0)
                {
                    List<T> list2 = (List<T>) ListPool<T>.pool[ListPool<T>.pool.Count - 1];
                    ListPool<T>.pool.RemoveAt(ListPool<T>.pool.Count - 1);
                    return list2;
                }
                return new List<T>();
            }
        }

        public static List<T> Claim(int capacity)
        {
            List<object> pool = ListPool<T>.pool;
            lock (pool)
            {
                if (ListPool<T>.pool.Count > 0)
                {
                    List<T> list2 = null;
                    int num = 0;
                    while ((num < ListPool<T>.pool.Count) && (num < 8))
                    {
                        list2 = (List<T>) ListPool<T>.pool[(ListPool<T>.pool.Count - 1) - num];
                        DebugHelper.Assert(list2 != null);
                        if (list2.Capacity >= capacity)
                        {
                            ListPool<T>.pool.RemoveAt((ListPool<T>.pool.Count - 1) - num);
                            return list2;
                        }
                        num++;
                    }
                    if (list2 == null)
                    {
                        return new List<T>(capacity);
                    }
                    list2.Capacity = capacity;
                    ListPool<T>.pool[ListPool<T>.pool.Count - num] = ListPool<T>.pool[ListPool<T>.pool.Count - 1];
                    ListPool<T>.pool.RemoveAt(ListPool<T>.pool.Count - 1);
                    return list2;
                }
                return new List<T>(capacity);
            }
        }

        public static void Clear()
        {
            List<object> pool = ListPool<T>.pool;
            lock (pool)
            {
                ListPool<T>.pool.Clear();
            }
        }

        public static int GetSize()
        {
            return ListPool<T>.pool.Count;
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            List<object> pool = ListPool<T>.pool;
            lock (pool)
            {
                for (int i = 0; i < ListPool<T>.pool.Count; i++)
                {
                    if (ListPool<T>.pool[i] == list)
                    {
                        throw new InvalidOperationException("The List is released even though it is in the pool");
                    }
                }
                ListPool<T>.pool.Add(list);
            }
        }

        public static void Warmup(int count, int size)
        {
            List<object> pool = ListPool<T>.pool;
            lock (pool)
            {
                List<T>[] listArray = new List<T>[count];
                for (int i = 0; i < count; i++)
                {
                    listArray[i] = ListPool<T>.Claim(size);
                }
                for (int j = 0; j < count; j++)
                {
                    ListPool<T>.Release(listArray[j]);
                }
            }
        }
    }
}

