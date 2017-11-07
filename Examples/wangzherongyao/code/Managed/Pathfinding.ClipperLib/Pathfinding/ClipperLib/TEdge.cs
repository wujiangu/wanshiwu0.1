namespace Pathfinding.ClipperLib
{
    using System;

    internal class TEdge
    {
        public IntPoint Bot;
        public IntPoint Curr;
        public IntPoint Delta;
        public double Dx;
        public TEdge Next;
        public TEdge NextInAEL;
        public TEdge NextInLML;
        public TEdge NextInSEL;
        public int OutIdx;
        public PolyType PolyTyp;
        public TEdge Prev;
        public TEdge PrevInAEL;
        public TEdge PrevInSEL;
        public EdgeSide Side;
        public IntPoint Top;
        public int WindCnt;
        public int WindCnt2;
        public int WindDelta;
    }
}

