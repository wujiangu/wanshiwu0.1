namespace TMPro
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class KerningTable
    {
        [CompilerGenerated]
        private static Func<KerningPair, int> <>f__am$cache1;
        [CompilerGenerated]
        private static Func<KerningPair, int> <>f__am$cache2;
        public List<KerningPair> kerningPairs = new List<KerningPair>();

        public void AddKerningPair()
        {
            if (this.kerningPairs.Count == 0)
            {
                this.kerningPairs.Add(new KerningPair(0, 0, 0f));
            }
            else
            {
                int left = this.kerningPairs.Last<KerningPair>().AscII_Left;
                int right = this.kerningPairs.Last<KerningPair>().AscII_Right;
                float xadvanceOffset = this.kerningPairs.Last<KerningPair>().XadvanceOffset;
                this.kerningPairs.Add(new KerningPair(left, right, xadvanceOffset));
            }
        }

        public int AddKerningPair(int left, int right, float offset)
        {
            <AddKerningPair>c__AnonStorey46 storey = new <AddKerningPair>c__AnonStorey46 {
                left = left,
                right = right
            };
            if (this.kerningPairs.FindIndex(new Predicate<KerningPair>(storey.<>m__48)) == -1)
            {
                this.kerningPairs.Add(new KerningPair(storey.left, storey.right, offset));
                return 0;
            }
            return -1;
        }

        public void RemoveKerningPair(int index)
        {
            this.kerningPairs.RemoveAt(index);
        }

        public void RemoveKerningPair(int left, int right)
        {
            <RemoveKerningPair>c__AnonStorey47 storey = new <RemoveKerningPair>c__AnonStorey47 {
                left = left,
                right = right
            };
            int index = this.kerningPairs.FindIndex(new Predicate<KerningPair>(storey.<>m__49));
            if (index != -1)
            {
                this.kerningPairs.RemoveAt(index);
            }
        }

        public void SortKerningPairs()
        {
            if (this.kerningPairs.Count > 0)
            {
                if (<>f__am$cache1 == null)
                {
                    <>f__am$cache1 = new Func<KerningPair, int>(null, (IntPtr) <SortKerningPairs>m__4A);
                }
                if (<>f__am$cache2 == null)
                {
                    <>f__am$cache2 = new Func<KerningPair, int>(null, (IntPtr) <SortKerningPairs>m__4B);
                }
                this.kerningPairs = Enumerable.ThenBy<KerningPair, int>(Enumerable.OrderBy<KerningPair, int>(this.kerningPairs, <>f__am$cache1), <>f__am$cache2).ToList<KerningPair>();
            }
        }

        [CompilerGenerated]
        private sealed class <AddKerningPair>c__AnonStorey46
        {
            internal int left;
            internal int right;

            internal bool <>m__48(KerningPair item)
            {
                return ((item.AscII_Left == this.left) && (item.AscII_Right == this.right));
            }
        }

        [CompilerGenerated]
        private sealed class <RemoveKerningPair>c__AnonStorey47
        {
            internal int left;
            internal int right;

            internal bool <>m__49(KerningPair item)
            {
                return ((item.AscII_Left == this.left) && (item.AscII_Right == this.right));
            }
        }
    }
}

