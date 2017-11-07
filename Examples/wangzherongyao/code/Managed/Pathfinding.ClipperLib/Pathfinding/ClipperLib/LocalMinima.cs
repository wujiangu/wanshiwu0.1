namespace Pathfinding.ClipperLib
{
    using System;

    internal class LocalMinima
    {
        public TEdge LeftBound;
        public LocalMinima Next;
        public TEdge RightBound;
        public long Y;
    }
}

