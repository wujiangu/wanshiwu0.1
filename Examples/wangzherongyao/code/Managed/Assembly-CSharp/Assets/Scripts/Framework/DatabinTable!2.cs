namespace Assets.Scripts.Framework
{
    using System;
    using System.Collections.Generic;

    public class DatabinTable<T, K> : DatabinTableBase
    {
        public DatabinTable(string InName, string InKey)
        {
            base.DataName = InName;
            base.KeyName = InKey;
            base.isDoubleKey = false;
            base.mapItems.Clear();
            base.bLoaded = false;
            Singleton<ResourceLoader>.GetInstance().LoadDatabin(InName, new ResourceLoader.BinLoadCompletedDelegate(this.onRecordLoaded));
        }

        public DatabinTable(string InName, string InKey1, string InKey2)
        {
            base.DataName = InName;
            base.KeyName1 = InKey1;
            base.KeyName2 = InKey2;
            base.isDoubleKey = true;
            base.mapItems.Clear();
            base.bLoaded = false;
            Singleton<ResourceLoader>.GetInstance().LoadDatabin(InName, new ResourceLoader.BinLoadCompletedDelegate(this.onRecordLoaded));
        }

        public void Accept(Action<T> InVisitor)
        {
            this.Reload();
            DebugHelper.Assert(base.isLoaded, "you can't visit databin when it is not loaded.");
            if (base.isLoaded)
            {
                Dictionary<long, object>.Enumerator enumerator = base.mapItems.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<long, object> current = enumerator.Current;
                    InVisitor((T) current.Value);
                }
            }
        }

        public void CopyTo(ref T[] InArrayRef)
        {
            this.Reload();
            DebugHelper.Assert(InArrayRef.Length == base.mapItems.Count, "Failed Databin CopyTo,size miss.");
            int num = 0;
            Dictionary<long, object>.Enumerator enumerator = base.mapItems.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<long, object> current = enumerator.Current;
                InArrayRef[num++] = (T) current.Value;
            }
        }

        public int Count()
        {
            this.Reload();
            return base.mapItems.Count;
        }

        public T FindIf(Func<T, bool> InFunc)
        {
            this.Reload();
            if (base.isLoaded)
            {
                Dictionary<long, object>.Enumerator enumerator = base.mapItems.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<long, object> current = enumerator.Current;
                    if (InFunc.Invoke((T) current.Value))
                    {
                        KeyValuePair<long, object> pair2 = enumerator.Current;
                        return (T) pair2.Value;
                    }
                }
            }
            return default(T);
        }

        public T GetAnyData()
        {
            this.Reload();
            if (base.isLoaded && (base.mapItems.Count > 0))
            {
                Dictionary<long, object>.Enumerator enumerator = base.mapItems.GetEnumerator();
                enumerator.MoveNext();
                KeyValuePair<long, object> current = enumerator.Current;
                return (T) current.Value;
            }
            return default(T);
        }

        public T GetDataByIndex(int id)
        {
            this.Reload();
            if (base.isLoaded)
            {
                Dictionary<long, object>.Enumerator enumerator = base.mapItems.GetEnumerator();
                for (int i = 0; enumerator.MoveNext(); i++)
                {
                    if (i == id)
                    {
                        KeyValuePair<long, object> current = enumerator.Current;
                        return (T) current.Value;
                    }
                }
            }
            return default(T);
        }

        public T GetDataByKey(byte key)
        {
            this.Reload();
            return (T) base.GetDataByKeyInner((long) key);
        }

        public T GetDataByKey(int key)
        {
            this.Reload();
            return (T) base.GetDataByKeyInner((long) key);
        }

        public T GetDataByKey(long key)
        {
            this.Reload();
            return (T) base.GetDataByKeyInner(key);
        }

        public T GetDataByKey(uint key)
        {
            this.Reload();
            return (T) base.GetDataByKeyInner((long) key);
        }

        private void onRecordLoaded(ref byte[] rawData)
        {
            base.LoadTdrBin(rawData, typeof(T));
            base.bLoaded = true;
        }

        protected void Reload()
        {
            if (!base.isLoaded)
            {
                Singleton<ResourceLoader>.GetInstance().LoadDatabin(base.Name, new ResourceLoader.BinLoadCompletedDelegate(this.onRecordLoaded));
            }
        }

        public int count
        {
            get
            {
                return this.Count();
            }
        }
    }
}

