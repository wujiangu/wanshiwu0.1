namespace Assets.Scripts.GameLogic
{
    using Assets.Scripts.Common;
    using System;
    using System.Collections.Generic;

    public class BufferMarkRule
    {
        private BuffHolderComponent buffHolder;
        private DictionaryView<int, BufferMark> buffMarkSet = new DictionaryView<int, BufferMark>();

        public void AddBufferMark(PoolObjHandle<ActorRoot> _originator, int _markID)
        {
            BufferMark mark;
            if (this.buffMarkSet.TryGetValue(_markID, out mark))
            {
                mark.AddLayer(1);
                mark.AddTrigger(_originator);
            }
            else
            {
                mark = new BufferMark(_markID);
                if (mark.cfgData != null)
                {
                    mark.Init(this.buffHolder);
                    this.buffMarkSet.Add(_markID, mark);
                }
            }
        }

        public void Init(BuffHolderComponent _buffHolder)
        {
            this.buffHolder = _buffHolder;
        }

        public void RemoveBufferMark(PoolObjHandle<ActorRoot> _originator, int _markID)
        {
            BufferMark mark;
            if (this.buffMarkSet.TryGetValue(_markID, out mark))
            {
                mark.DecLayer(1);
            }
        }

        public void TriggerBufferMark(PoolObjHandle<ActorRoot> _originator, int _markID)
        {
            BufferMark mark;
            if (this.buffMarkSet.TryGetValue(_markID, out mark))
            {
                mark.UpperTrigger(_originator);
            }
        }

        public void UpdateLogic(int nDelta)
        {
            DictionaryView<int, BufferMark>.Enumerator enumerator = this.buffMarkSet.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<int, BufferMark> current = enumerator.Current;
                current.Value.UpdateLogic(nDelta);
            }
        }
    }
}

