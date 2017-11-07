namespace Pathfinding.ClipperLib
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class ClipperBase
    {
        internal const long hiRange = 0x3fffffffffffffffL;
        protected const double horizontal = -3.4E+38;
        internal const long loRange = 0x3fffffffL;
        internal LocalMinima m_CurrentLM = null;
        internal List<List<TEdge>> m_edges = new List<List<TEdge>>();
        internal bool m_HasOpenPaths = false;
        internal LocalMinima m_MinimaList = null;
        internal bool m_UseFullRange = false;
        protected const int Skip = -2;
        protected const double tolerance = 1E-20;
        protected const int Unassigned = -1;

        internal ClipperBase()
        {
        }

        private TEdge AddBoundsToLML(TEdge E, bool Closed)
        {
            TEdge edge;
            bool flag;
            if (E.OutIdx == -2)
            {
                if (this.MoreBelow(E))
                {
                    E = E.Next;
                    edge = this.DescendToMin(ref E);
                }
                else
                {
                    edge = null;
                }
            }
            else
            {
                edge = this.DescendToMin(ref E);
            }
            if (E.OutIdx == -2)
            {
                this.DoMinimaLML(null, edge, Closed);
                flag = false;
                if ((E.Bot != E.Prev.Bot) && this.MoreBelow(E))
                {
                    E = E.Next;
                    edge = this.DescendToMin(ref E);
                    this.DoMinimaLML(edge, E, Closed);
                    flag = true;
                }
                else if (this.JustBeforeLocMin(E))
                {
                    E = E.Next;
                }
            }
            else
            {
                this.DoMinimaLML(edge, E, Closed);
                flag = true;
            }
            this.AscendToMax(ref E, flag, Closed);
            if ((E.OutIdx == -2) && (E.Top != E.Prev.Top))
            {
                if (this.MoreAbove(E))
                {
                    E = E.Next;
                    this.AscendToMax(ref E, false, Closed);
                    return E;
                }
                if (!(E.Top == E.Next.Top) && (!IsHorizontal(E.Next) || !(E.Top == E.Next.Bot)))
                {
                    return E;
                }
                E = E.Next;
            }
            return E;
        }

        public bool AddPath(List<IntPoint> pg, PolyType polyType, bool Closed)
        {
            if (!Closed)
            {
                throw new ClipperException("AddPath: Open paths have been disabled.");
            }
            int num = pg.Count - 1;
            bool flag = (num > 0) && (Closed || (pg[0] == pg[num]));
            while ((num > 0) && (pg[num] == pg[0]))
            {
                num--;
            }
            while ((num > 0) && (pg[num] == pg[num - 1]))
            {
                num--;
            }
            if ((Closed && (num < 2)) || (!Closed && (num < 1)))
            {
                return false;
            }
            List<TEdge> item = new List<TEdge>(num + 1);
            for (int i = 0; i <= num; i++)
            {
                item.Add(new TEdge());
            }
            try
            {
                item[1].Curr = pg[1];
                this.RangeTest(pg[0], ref this.m_UseFullRange);
                this.RangeTest(pg[num], ref this.m_UseFullRange);
                this.InitEdge(item[0], item[1], item[num], pg[0]);
                this.InitEdge(item[num], item[0], item[num - 1], pg[num]);
                for (int j = num - 1; j >= 1; j--)
                {
                    this.RangeTest(pg[j], ref this.m_UseFullRange);
                    this.InitEdge(item[j], item[j + 1], item[j - 1], pg[j]);
                }
            }
            catch
            {
                return false;
            }
            TEdge next = item[0];
            if (!flag)
            {
                next.Prev.OutIdx = -2;
            }
            TEdge e = next;
            TEdge edge3 = next;
        Label_01E7:
            while (e.Curr == e.Next.Curr)
            {
                if (e == e.Next)
                {
                    goto Label_0342;
                }
                if (e == next)
                {
                    next = e.Next;
                }
                e = this.RemoveEdge(e);
                edge3 = e;
            }
            if (e.Prev != e.Next)
            {
                if ((flag || (((e.Prev.OutIdx != -2) && (e.OutIdx != -2)) && (e.Next.OutIdx != -2))) && ((SlopesEqual(e.Prev.Curr, e.Curr, e.Next.Curr, this.m_UseFullRange) && Closed) && (!this.PreserveCollinear || !this.Pt2IsBetweenPt1AndPt3(e.Prev.Curr, e.Curr, e.Next.Curr))))
                {
                    if (e == next)
                    {
                        next = e.Next;
                    }
                    e = this.RemoveEdge(e).Prev;
                    edge3 = e;
                    goto Label_01E7;
                }
                e = e.Next;
                if (e != edge3)
                {
                    goto Label_01E7;
                }
            }
        Label_0342:
            if ((!Closed && (e == e.Next)) || (Closed && (e.Prev == e.Next)))
            {
                return false;
            }
            this.m_edges.Add(item);
            if (!Closed)
            {
                this.m_HasOpenPaths = true;
            }
            TEdge prev = next;
            e = next;
            do
            {
                this.InitEdge2(e, polyType);
                if (e.Top.Y < prev.Top.Y)
                {
                    prev = e;
                }
                e = e.Next;
            }
            while (e != next);
            if (this.AllHorizontal(e))
            {
                if (flag)
                {
                    e.Prev.OutIdx = -2;
                }
                this.AscendToMax(ref e, false, false);
                return true;
            }
            e = next.Prev;
            if (e.Prev == e.Next)
            {
                prev = e.Next;
            }
            else if (!flag && (e.Top.Y == prev.Top.Y))
            {
                if ((IsHorizontal(e) || IsHorizontal(e.Next)) && (e.Next.Bot.Y == prev.Top.Y))
                {
                    prev = e.Next;
                }
                else if (this.SharedVertWithPrevAtTop(e))
                {
                    prev = e;
                }
                else if (e.Top == e.Prev.Top)
                {
                    prev = e.Prev;
                }
                else
                {
                    prev = e.Next;
                }
            }
            else
            {
                e = prev;
                while ((IsHorizontal(prev) || (prev.Top == prev.Next.Top)) || (prev.Top == prev.Next.Bot))
                {
                    prev = prev.Next;
                    if (prev == e)
                    {
                        while (IsHorizontal(prev) || !this.SharedVertWithPrevAtTop(prev))
                        {
                            prev = prev.Next;
                        }
                        break;
                    }
                }
            }
            e = prev;
            do
            {
                e = this.AddBoundsToLML(e, Closed);
            }
            while (e != prev);
            return true;
        }

        public bool AddPolygon(List<IntPoint> pg, PolyType polyType)
        {
            return this.AddPath(pg, polyType, true);
        }

        private bool AllHorizontal(TEdge Edge)
        {
            if (!IsHorizontal(Edge))
            {
                return false;
            }
            for (TEdge edge = Edge.Next; edge != Edge; edge = edge.Next)
            {
                if (!IsHorizontal(edge))
                {
                    return false;
                }
            }
            return true;
        }

        private void AscendToMax(ref TEdge E, bool Appending, bool IsClosed)
        {
            if (E.OutIdx == -2)
            {
                E = E.Next;
                if (!this.MoreAbove(E.Prev))
                {
                    return;
                }
            }
            if ((IsHorizontal(E) && Appending) && (E.Bot != E.Prev.Bot))
            {
                this.ReverseHorizontal(E);
            }
            TEdge next = E;
            while ((E.Next.OutIdx != -2) && ((E.Next.Top.Y != E.Top.Y) || IsHorizontal(E.Next)))
            {
                E.NextInLML = E.Next;
                E = E.Next;
                if (IsHorizontal(E) && (E.Bot.X != E.Prev.Top.X))
                {
                    this.ReverseHorizontal(E);
                }
            }
            if (!Appending)
            {
                if (next.OutIdx == -2)
                {
                    next = next.Next;
                }
                if (next != E.Next)
                {
                    this.DoMinimaLML(null, next, IsClosed);
                }
            }
            E = E.Next;
        }

        public virtual void Clear()
        {
            this.DisposeLocalMinimaList();
            for (int i = 0; i < this.m_edges.Count; i++)
            {
                for (int j = 0; j < this.m_edges[i].Count; j++)
                {
                    this.m_edges[i][j] = null;
                }
                this.m_edges[i].Clear();
            }
            this.m_edges.Clear();
            this.m_UseFullRange = false;
            this.m_HasOpenPaths = false;
        }

        private TEdge DescendToMin(ref TEdge E)
        {
            TEdge next;
            E.NextInLML = null;
            if (IsHorizontal(E))
            {
                next = E;
                while (IsHorizontal(next.Next))
                {
                    next = next.Next;
                }
                if (next.Bot != next.Next.Top)
                {
                    this.ReverseHorizontal(E);
                }
            }
            while (true)
            {
                E = E.Next;
                if (E.OutIdx == -2)
                {
                    break;
                }
                if (IsHorizontal(E))
                {
                    next = this.GetLastHorz(E);
                    if ((next == E.Prev) || ((next.Next.Top.Y < E.Top.Y) && (next.Next.Bot.X > E.Prev.Bot.X)))
                    {
                        break;
                    }
                    if (E.Top.X != E.Prev.Bot.X)
                    {
                        this.ReverseHorizontal(E);
                    }
                    if (next.OutIdx == -2)
                    {
                        next = next.Prev;
                    }
                    while (E != next)
                    {
                        E.NextInLML = E.Prev;
                        E = E.Next;
                        if (E.Top.X != E.Prev.Bot.X)
                        {
                            this.ReverseHorizontal(E);
                        }
                    }
                }
                else if (E.Bot.Y == E.Prev.Bot.Y)
                {
                    break;
                }
                E.NextInLML = E.Prev;
            }
            return E.Prev;
        }

        private void DisposeLocalMinimaList()
        {
            while (this.m_MinimaList != null)
            {
                LocalMinima next = this.m_MinimaList.Next;
                this.m_MinimaList = null;
                this.m_MinimaList = next;
            }
            this.m_CurrentLM = null;
        }

        private void DoMinimaLML(TEdge E1, TEdge E2, bool IsClosed)
        {
            if (E1 == null)
            {
                if (E2 != null)
                {
                    LocalMinima newLm = new LocalMinima {
                        Next = null,
                        Y = E2.Bot.Y,
                        LeftBound = null
                    };
                    E2.WindDelta = 0;
                    newLm.RightBound = E2;
                    this.InsertLocalMinima(newLm);
                }
            }
            else
            {
                LocalMinima minima2 = new LocalMinima {
                    Y = E1.Bot.Y,
                    Next = null
                };
                if (IsHorizontal(E2))
                {
                    if (E2.Bot.X != E1.Bot.X)
                    {
                        this.ReverseHorizontal(E2);
                    }
                    minima2.LeftBound = E1;
                    minima2.RightBound = E2;
                }
                else if (E2.Dx < E1.Dx)
                {
                    minima2.LeftBound = E1;
                    minima2.RightBound = E2;
                }
                else
                {
                    minima2.LeftBound = E2;
                    minima2.RightBound = E1;
                }
                minima2.LeftBound.Side = EdgeSide.esLeft;
                minima2.RightBound.Side = EdgeSide.esRight;
                if (!IsClosed)
                {
                    minima2.LeftBound.WindDelta = 0;
                }
                else if (minima2.LeftBound.Next == minima2.RightBound)
                {
                    minima2.LeftBound.WindDelta = -1;
                }
                else
                {
                    minima2.LeftBound.WindDelta = 1;
                }
                minima2.RightBound.WindDelta = -minima2.LeftBound.WindDelta;
                this.InsertLocalMinima(minima2);
            }
        }

        private TEdge GetLastHorz(TEdge Edge)
        {
            TEdge next = Edge;
            while (((next.OutIdx != -2) && (next.Next != Edge)) && IsHorizontal(next.Next))
            {
                next = next.Next;
            }
            return next;
        }

        private void InitEdge(TEdge e, TEdge eNext, TEdge ePrev, IntPoint pt)
        {
            e.Next = eNext;
            e.Prev = ePrev;
            e.Curr = pt;
            e.OutIdx = -1;
        }

        private void InitEdge2(TEdge e, PolyType polyType)
        {
            if (e.Curr.Y >= e.Next.Curr.Y)
            {
                e.Bot = e.Curr;
                e.Top = e.Next.Curr;
            }
            else
            {
                e.Top = e.Curr;
                e.Bot = e.Next.Curr;
            }
            this.SetDx(e);
            e.PolyTyp = polyType;
        }

        private void InsertLocalMinima(LocalMinima newLm)
        {
            if (this.m_MinimaList == null)
            {
                this.m_MinimaList = newLm;
            }
            else if (newLm.Y >= this.m_MinimaList.Y)
            {
                newLm.Next = this.m_MinimaList;
                this.m_MinimaList = newLm;
            }
            else
            {
                LocalMinima minimaList = this.m_MinimaList;
                while ((minimaList.Next != null) && (newLm.Y < minimaList.Next.Y))
                {
                    minimaList = minimaList.Next;
                }
                newLm.Next = minimaList.Next;
                minimaList.Next = newLm;
            }
        }

        internal static bool IsHorizontal(TEdge e)
        {
            return (e.Delta.Y == 0L);
        }

        private bool JustBeforeLocMin(TEdge Edge)
        {
            TEdge e = Edge;
            if (!IsHorizontal(e))
            {
                return this.SharedVertWithNextIsBot(e);
            }
            while (IsHorizontal(e.Next))
            {
                e = e.Next;
            }
            return (e.Next.Top.Y < e.Bot.Y);
        }

        private bool MoreAbove(TEdge Edge)
        {
            if (IsHorizontal(Edge))
            {
                Edge = this.GetLastHorz(Edge);
                return (Edge.Next.Top.Y < Edge.Top.Y);
            }
            if (IsHorizontal(Edge.Next))
            {
                Edge = this.GetLastHorz(Edge.Next);
                return (Edge.Next.Top.Y < Edge.Top.Y);
            }
            return (Edge.Next.Top.Y < Edge.Top.Y);
        }

        private bool MoreBelow(TEdge Edge)
        {
            TEdge e = Edge;
            if (IsHorizontal(e))
            {
                while (IsHorizontal(e.Next))
                {
                    e = e.Next;
                }
                return (e.Next.Bot.Y > e.Bot.Y);
            }
            if (!IsHorizontal(e.Next))
            {
                return (e.Bot == e.Next.Top);
            }
            while (IsHorizontal(e.Next))
            {
                e = e.Next;
            }
            return (e.Next.Bot.Y > e.Bot.Y);
        }

        internal bool PointInPolygon(IntPoint pt, OutPt pp, bool UseFullRange)
        {
            OutPt next = pp;
            bool flag = false;
            if (UseFullRange)
            {
                do
                {
                    if ((next.Pt.Y > pt.Y) != (next.Prev.Pt.Y > pt.Y))
                    {
                        Int128 introduced2 = Int128.Int128Mul(next.Prev.Pt.X - next.Pt.X, pt.Y - next.Pt.Y);
                        if (new Int128(pt.X - next.Pt.X) < (introduced2 / new Int128(next.Prev.Pt.Y - next.Pt.Y)))
                        {
                            flag = !flag;
                        }
                    }
                    next = next.Next;
                }
                while (next != pp);
                return flag;
            }
            do
            {
                if (((next.Pt.Y > pt.Y) != (next.Prev.Pt.Y > pt.Y)) && ((pt.X - next.Pt.X) < (((next.Prev.Pt.X - next.Pt.X) * (pt.Y - next.Pt.Y)) / (next.Prev.Pt.Y - next.Pt.Y))))
                {
                    flag = !flag;
                }
                next = next.Next;
            }
            while (next != pp);
            return flag;
        }

        internal bool PointOnLineSegment(IntPoint pt, IntPoint linePt1, IntPoint linePt2, bool UseFullRange)
        {
            if (UseFullRange)
            {
                return ((((pt.X == linePt1.X) && (pt.Y == linePt1.Y)) || ((pt.X == linePt2.X) && (pt.Y == linePt2.Y))) || ((((pt.X > linePt1.X) == (pt.X < linePt2.X)) && ((pt.Y > linePt1.Y) == (pt.Y < linePt2.Y))) && (Int128.Int128Mul(pt.X - linePt1.X, linePt2.Y - linePt1.Y) == Int128.Int128Mul(linePt2.X - linePt1.X, pt.Y - linePt1.Y))));
            }
            return ((((pt.X == linePt1.X) && (pt.Y == linePt1.Y)) || ((pt.X == linePt2.X) && (pt.Y == linePt2.Y))) || ((((pt.X > linePt1.X) == (pt.X < linePt2.X)) && ((pt.Y > linePt1.Y) == (pt.Y < linePt2.Y))) && (((pt.X - linePt1.X) * (linePt2.Y - linePt1.Y)) == ((linePt2.X - linePt1.X) * (pt.Y - linePt1.Y)))));
        }

        internal bool PointOnPolygon(IntPoint pt, OutPt pp, bool UseFullRange)
        {
            OutPt next = pp;
            do
            {
                if (this.PointOnLineSegment(pt, next.Pt, next.Next.Pt, UseFullRange))
                {
                    return true;
                }
                next = next.Next;
            }
            while (next != pp);
            return false;
        }

        protected void PopLocalMinima()
        {
            if (this.m_CurrentLM != null)
            {
                this.m_CurrentLM = this.m_CurrentLM.Next;
            }
        }

        internal bool Pt2IsBetweenPt1AndPt3(IntPoint pt1, IntPoint pt2, IntPoint pt3)
        {
            if (((pt1 == pt3) || (pt1 == pt2)) || (pt3 == pt2))
            {
                return false;
            }
            if (pt1.X != pt3.X)
            {
                return ((pt2.X > pt1.X) == (pt2.X < pt3.X));
            }
            return ((pt2.Y > pt1.Y) == (pt2.Y < pt3.Y));
        }

        private void RangeTest(IntPoint Pt, ref bool useFullRange)
        {
            if (useFullRange)
            {
                if (((Pt.X > 0x3fffffffffffffffL) || (Pt.Y > 0x3fffffffffffffffL)) || ((-Pt.X > 0x3fffffffffffffffL) || (-Pt.Y > 0x3fffffffffffffffL)))
                {
                    throw new ClipperException("Coordinate outside allowed range");
                }
            }
            else if (((Pt.X > 0x3fffffffL) || (Pt.Y > 0x3fffffffL)) || ((-Pt.X > 0x3fffffffL) || (-Pt.Y > 0x3fffffffL)))
            {
                useFullRange = true;
                this.RangeTest(Pt, ref useFullRange);
            }
        }

        private TEdge RemoveEdge(TEdge e)
        {
            e.Prev.Next = e.Next;
            e.Next.Prev = e.Prev;
            TEdge next = e.Next;
            e.Prev = null;
            return next;
        }

        protected virtual void Reset()
        {
            this.m_CurrentLM = this.m_MinimaList;
            if (this.m_CurrentLM != null)
            {
                for (LocalMinima minima = this.m_MinimaList; minima != null; minima = minima.Next)
                {
                    TEdge leftBound = minima.LeftBound;
                    if (leftBound != null)
                    {
                        leftBound.Curr = leftBound.Bot;
                        leftBound.Side = EdgeSide.esLeft;
                        if (leftBound.OutIdx != -2)
                        {
                            leftBound.OutIdx = -1;
                        }
                    }
                    leftBound = minima.RightBound;
                    leftBound.Curr = leftBound.Bot;
                    leftBound.Side = EdgeSide.esRight;
                    if (leftBound.OutIdx != -2)
                    {
                        leftBound.OutIdx = -1;
                    }
                }
            }
        }

        private void ReverseHorizontal(TEdge e)
        {
            long x = e.Top.X;
            e.Top.X = e.Bot.X;
            e.Bot.X = x;
        }

        private void SetDx(TEdge e)
        {
            e.Delta.X = e.Top.X - e.Bot.X;
            e.Delta.Y = e.Top.Y - e.Bot.Y;
            if (e.Delta.Y == 0L)
            {
                e.Dx = -3.4E+38;
            }
            else
            {
                e.Dx = ((double) e.Delta.X) / ((double) e.Delta.Y);
            }
        }

        private bool SharedVertWithNextIsBot(TEdge Edge)
        {
            bool flag = true;
            TEdge prev = Edge;
            while (prev.Prev != Edge)
            {
                bool flag2 = prev.Next.Bot == prev.Bot;
                bool flag3 = prev.Prev.Bot == prev.Bot;
                if (flag2 != flag3)
                {
                    flag = flag2;
                    break;
                }
                flag2 = prev.Next.Top == prev.Top;
                flag3 = prev.Prev.Top == prev.Top;
                if (flag2 != flag3)
                {
                    flag = flag3;
                    break;
                }
                prev = prev.Prev;
            }
            while (prev != Edge)
            {
                flag = !flag;
                prev = prev.Next;
            }
            return flag;
        }

        private bool SharedVertWithPrevAtTop(TEdge Edge)
        {
            TEdge prev = Edge;
            bool flag = true;
            while (prev.Prev != Edge)
            {
                if (prev.Top == prev.Prev.Top)
                {
                    if (prev.Bot == prev.Prev.Bot)
                    {
                        prev = prev.Prev;
                        continue;
                    }
                    flag = true;
                }
                else
                {
                    flag = false;
                }
                break;
            }
            while (prev != Edge)
            {
                flag = !flag;
                prev = prev.Next;
            }
            return flag;
        }

        internal static bool SlopesEqual(TEdge e1, TEdge e2, bool UseFullRange)
        {
            if (UseFullRange)
            {
                Int128 introduced0 = Int128.Int128Mul(e1.Delta.Y, e2.Delta.X);
                return (introduced0 == Int128.Int128Mul(e1.Delta.X, e2.Delta.Y));
            }
            return ((e1.Delta.Y * e2.Delta.X) == (e1.Delta.X * e2.Delta.Y));
        }

        protected static bool SlopesEqual(IntPoint pt1, IntPoint pt2, IntPoint pt3, bool UseFullRange)
        {
            if (UseFullRange)
            {
                return (Int128.Int128Mul(pt1.Y - pt2.Y, pt2.X - pt3.X) == Int128.Int128Mul(pt1.X - pt2.X, pt2.Y - pt3.Y));
            }
            return ((((pt1.Y - pt2.Y) * (pt2.X - pt3.X)) - ((pt1.X - pt2.X) * (pt2.Y - pt3.Y))) == 0L);
        }

        public bool PreserveCollinear { get; set; }
    }
}

