namespace Assets.Scripts.Framework
{
    using System;
    using System.Runtime.InteropServices;

    public class MultiValueListDictionary<TKey, TValue> : DictionaryView<TKey, ListView<TValue>>
    {
        public void Add(TKey key, TValue value)
        {
            ListView<TValue> view = null;
            if (!base.TryGetValue(key, out view))
            {
                view = new ListView<TValue>();
                base.Add(key, view);
            }
            view.Add(value);
        }

        public ListView<TValue> GetValues(TKey key, bool returnEmptySet = true)
        {
            ListView<TValue> view = null;
            if (!base.TryGetValue(key, out view) && returnEmptySet)
            {
                view = new ListView<TValue>();
            }
            return view;
        }
    }
}

