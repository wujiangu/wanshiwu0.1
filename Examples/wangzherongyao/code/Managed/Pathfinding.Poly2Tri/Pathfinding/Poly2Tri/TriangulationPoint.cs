namespace Pathfinding.Poly2Tri
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class TriangulationPoint
    {
        public double X;
        public double Y;

        public TriangulationPoint(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public void AddEdge(DTSweepConstraint e)
        {
            if (this.Edges == null)
            {
                this.Edges = new List<DTSweepConstraint>();
            }
            this.Edges.Add(e);
        }

        public override string ToString()
        {
            object[] objArray1 = new object[] { "[", this.X, ",", this.Y, "]" };
            return string.Concat(objArray1);
        }

        public List<DTSweepConstraint> Edges { get; private set; }

        public bool HasEdges
        {
            get
            {
                return (this.Edges != null);
            }
        }
    }
}

