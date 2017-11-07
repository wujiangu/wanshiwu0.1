namespace Pathfinding.ClipperLib
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Int128
    {
        private long hi;
        private ulong lo;
        public Int128(long _lo)
        {
            this.lo = (ulong) _lo;
            if (_lo < 0L)
            {
                this.hi = -1L;
            }
            else
            {
                this.hi = 0L;
            }
        }

        public Int128(long _hi, ulong _lo)
        {
            this.lo = _lo;
            this.hi = _hi;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is Int128))
            {
                return false;
            }
            Int128 num = (Int128) obj;
            return ((num.hi == this.hi) && (num.lo == this.lo));
        }

        public override int GetHashCode()
        {
            return (this.hi.GetHashCode() ^ this.lo.GetHashCode());
        }

        public static Int128 Int128Mul(long lhs, long rhs)
        {
            bool flag = (lhs < 0L) != (rhs < 0L);
            if (lhs < 0L)
            {
                lhs = -lhs;
            }
            if (rhs < 0L)
            {
                rhs = -rhs;
            }
            ulong num = (ulong) (lhs >> 0x20);
            ulong num2 = ((ulong) lhs) & 0xffffffffL;
            ulong num3 = (ulong) (rhs >> 0x20);
            ulong num4 = ((ulong) rhs) & 0xffffffffL;
            ulong num5 = num * num3;
            ulong num6 = num2 * num4;
            ulong num7 = (num * num4) + (num2 * num3);
            long num9 = (long) (num5 + (num7 >> 0x20));
            ulong num8 = (num7 << 0x20) + num6;
            if (num8 < num6)
            {
                num9 += 1L;
            }
            Int128 num10 = new Int128(num9, num8);
            return (!flag ? num10 : -num10);
        }

        public static bool operator ==(Int128 val1, Int128 val2)
        {
            if (val1 == val2)
            {
                return true;
            }
            if ((val1 == 0) || (val2 == 0))
            {
                return false;
            }
            return ((val1.hi == val2.hi) && (val1.lo == val2.lo));
        }

        public static bool operator >(Int128 val1, Int128 val2)
        {
            if (val1.hi != val2.hi)
            {
                return (val1.hi > val2.hi);
            }
            return (val1.lo > val2.lo);
        }

        public static bool operator <(Int128 val1, Int128 val2)
        {
            if (val1.hi != val2.hi)
            {
                return (val1.hi < val2.hi);
            }
            return (val1.lo < val2.lo);
        }

        public static Int128 operator +(Int128 lhs, Int128 rhs)
        {
            lhs.hi += rhs.hi;
            lhs.lo += rhs.lo;
            if (lhs.lo < rhs.lo)
            {
                lhs.hi += 1L;
            }
            return lhs;
        }

        public static Int128 operator -(Int128 lhs, Int128 rhs)
        {
            return (lhs + -rhs);
        }

        public static Int128 operator -(Int128 val)
        {
            if (val.lo == 0L)
            {
                return new Int128(-val.hi, 0L);
            }
            return new Int128(~val.hi, ~val.lo + ((ulong) 1L));
        }

        public static Int128 operator /(Int128 lhs, Int128 rhs)
        {
            if ((rhs.lo == 0L) && (rhs.hi == 0L))
            {
                throw new ClipperException("Int128: divide by zero");
            }
            bool flag = (rhs.hi < 0L) != (lhs.hi < 0L);
            if (lhs.hi < 0L)
            {
                lhs = -lhs;
            }
            if (rhs.hi < 0L)
            {
                rhs = -rhs;
            }
            if (rhs < lhs)
            {
                Int128 num = new Int128(0L);
                Int128 num2 = new Int128(1L);
                while ((rhs.hi >= 0L) && (rhs <= lhs))
                {
                    rhs.hi = rhs.hi << 1;
                    if (rhs.lo < 0L)
                    {
                        rhs.hi += 1L;
                    }
                    rhs.lo = rhs.lo << 1;
                    num2.hi = num2.hi << 1;
                    if (num2.lo < 0L)
                    {
                        num2.hi += 1L;
                    }
                    num2.lo = num2.lo << 1;
                }
                rhs.lo = rhs.lo >> 1;
                if ((rhs.hi & 1L) == 1L)
                {
                    rhs.lo |= 9223372036854775808L;
                }
                rhs.hi = rhs.hi >> 1;
                num2.lo = num2.lo >> 1;
                if ((num2.hi & 1L) == 1L)
                {
                    num2.lo |= 9223372036854775808L;
                }
                num2.hi = num2.hi >> 1;
                while ((num2.hi != 0L) || (num2.lo != 0L))
                {
                    if (lhs >= rhs)
                    {
                        lhs -= rhs;
                        num.hi |= num2.hi;
                        num.lo |= num2.lo;
                    }
                    rhs.lo = rhs.lo >> 1;
                    if ((rhs.hi & 1L) == 1L)
                    {
                        rhs.lo |= 9223372036854775808L;
                    }
                    rhs.hi = rhs.hi >> 1;
                    num2.lo = num2.lo >> 1;
                    if ((num2.hi & 1L) == 1L)
                    {
                        num2.lo |= 9223372036854775808L;
                    }
                    num2.hi = num2.hi >> 1;
                }
                return (!flag ? num : -num);
            }
            if (rhs == lhs)
            {
                return new Int128(1L);
            }
            return new Int128(0L);
        }
    }
}

