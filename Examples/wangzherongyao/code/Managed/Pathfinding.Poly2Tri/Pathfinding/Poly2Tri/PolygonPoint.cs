namespace Pathfinding.Poly2Tri
{
    using System;
    using System.Runtime.CompilerServices;

    public class PolygonPoint : TriangulationPoint
    {
        public PolygonPoint(double x, double y) : base(x, y)
        {
        }

        public PolygonPoint Next { get; set; }

        public PolygonPoint Previous
        {
            [CompilerGenerated]
            set
            {
                this.<Previous>k__BackingField = value;
            }
        }
    }
}

