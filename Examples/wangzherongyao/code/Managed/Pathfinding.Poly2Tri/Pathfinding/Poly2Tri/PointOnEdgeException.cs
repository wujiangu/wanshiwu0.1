namespace Pathfinding.Poly2Tri
{
    using System;

    public class PointOnEdgeException : NotImplementedException
    {
        public readonly TriangulationPoint A;
        public readonly TriangulationPoint B;
        public readonly TriangulationPoint C;

        public PointOnEdgeException(string message, TriangulationPoint a, TriangulationPoint b, TriangulationPoint c) : base(message + "\n" + a.ToString() + "\n" + b.ToString() + "\n" + c.ToString())
        {
            this.A = a;
            this.B = b;
            this.C = c;
        }
    }
}

