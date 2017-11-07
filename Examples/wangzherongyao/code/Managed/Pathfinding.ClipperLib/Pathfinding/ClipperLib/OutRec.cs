namespace Pathfinding.ClipperLib
{
    using System;

    internal class OutRec
    {
        public OutPt BottomPt;
        public OutRec FirstLeft;
        public int Idx;
        public bool IsHole;
        public bool IsOpen;
        public Pathfinding.ClipperLib.PolyNode PolyNode;
        public OutPt Pts;
    }
}

