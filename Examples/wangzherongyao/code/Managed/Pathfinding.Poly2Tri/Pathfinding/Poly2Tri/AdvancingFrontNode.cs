namespace Pathfinding.Poly2Tri
{
    using System;

    public class AdvancingFrontNode
    {
        public AdvancingFrontNode Next;
        public TriangulationPoint Point;
        public AdvancingFrontNode Prev;
        public DelaunayTriangle Triangle;
        public double Value;

        public AdvancingFrontNode(TriangulationPoint point)
        {
            this.Point = point;
            this.Value = point.X;
        }

        public bool HasNext
        {
            get
            {
                return (this.Next != null);
            }
        }

        public bool HasPrev
        {
            get
            {
                return (this.Prev != null);
            }
        }
    }
}

