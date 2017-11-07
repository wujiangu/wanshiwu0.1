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
    public struct FixedArray3<T> : IEnumerable<T>, IEnumerable where T: class
    {
        public T _0;
        public T _1;
        public T _2;
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public T this[int index]
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
        public bool Contains(T value)
        {
            for (int i = 0; i < 3; i++)
            {
                if (this[i] == value)
                {
                    return true;
                }
            }
            return false;
        }

        public int IndexOf(T value)
        {
            for (int i = 0; i < 3; i++)
            {
                if (this[i] == value)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Clear()
        {
            this._0 = this._1 = (T) (this._2 = null);
        }

        [DebuggerHidden]
        private unsafe IEnumerable<T> Enumerate()
        {
            return new <Enumerate>c__Iterator0<T> { <>f__this = *((FixedArray3<T>*) this), $PC = -2 };
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.Enumerate().GetEnumerator();
        }
        [CompilerGenerated]
        private sealed class <Enumerate>c__Iterator0 : IEnumerator<T>, IEnumerable<T>, IEnumerable, IEnumerator, IDisposable
        {
            internal T $current;
            internal int $PC;
            internal FixedArray3<T> <>f__this;
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
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                if (Interlocked.CompareExchange(ref this.$PC, 0, -2) == -2)
                {
                    return this;
                }
                return new FixedArray3<T>.<Enumerate>c__Iterator0 { <>f__this = this.<>f__this };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<T>.GetEnumerator();
            }

            T IEnumerator<T>.Current
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

