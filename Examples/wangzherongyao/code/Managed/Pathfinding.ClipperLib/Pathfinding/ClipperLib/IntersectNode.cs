namespace Pathfinding.ClipperLib
{
    using System;

    internal class IntersectNode
    {
        public TEdge Edge1;
        public TEdge Edge2;
        public IntersectNode Next;
        public IntPoint Pt;
    }
}

