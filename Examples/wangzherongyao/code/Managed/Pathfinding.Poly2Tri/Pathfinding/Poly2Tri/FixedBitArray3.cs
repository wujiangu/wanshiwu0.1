namespace Pathfinding.Poly2Tri
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    [StructLayout(LayoutKind.Sequential)]
    public struct FixedBitArray3 : IEnumerable<bool>, IEnumerable
    {
        public bool _0;
        public bool _1;
        public bool _2;
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this._0;

                    case 1:
                        return this._1;

                    case 2:
                        return this._2;
                }
                throw new IndexOutOfRangeException();
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this._0 = value;
                        break;

                    case 1:
                        this._1 = value;
                        break;

                    case 2:
                        this._2 = value;
                        break;

                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
        public void Clear()
        {
            this._0 = this._1 = this._2 = false;
        }

        [DebuggerHidden]
        private IEnumerable<bool> Enumerate()
        {
            return new <Enumerate>c__Iterator1 { <>f__this = this, $PC = -2 };
        }

        public IEnumerator<bool> GetEnumerator()
        {
            return this.Enumerate().GetEnumerator();
        }
        [CompilerGenerated]
        private sealed class <Enumerate>c__Iterator1 : IEnumerator<bool>, IEnumerable<bool>, IEnumerable, IEnumerator, IDisposable
        {
            internal bool $current;
            internal int $PC;
            internal FixedBitArray3 <>f__this;
            internal int <i>__0;

            [DebuggerHidden]
            public void Dispose()
            {
                this.$PC = -1;
            }

            public bool MoveNext()
            {
                uint num = (uint) this.$PC;
                this.$PC = -1;
                switch (num)
                {
                    case 0:
                        this.<i>__0 = 0;
                        break;

                    case 1:
                        this.<i>__0++;
                        break;

                    default:
                        goto Label_0071;
                }
                if (this.<i>__0 < 3)
                {
                    this.$current = this.<>f__this[this.<i>__0];
                    this.$PC = 1;
                    return true;
                }
                this.$PC = -1;
            Label_0071:
                return false;
            }

            [DebuggerHidden]
            public void Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
            {
                if (Interlocked.CompareExchange(ref this.$PC, 0, -2) == -2)
                {
                    return this;
                }
                return new FixedBitArray3.<Enumerate>c__Iterator1 { <>f__this = this.<>f__this };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<bool>.GetEnumerator();
            }

            bool IEnumerator<bool>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.$current;
                }
            }
        }
    }
}

