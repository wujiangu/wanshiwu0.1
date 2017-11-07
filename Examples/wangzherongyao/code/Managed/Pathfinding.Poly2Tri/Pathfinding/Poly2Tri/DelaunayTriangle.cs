namespace Pathfinding.Poly2Tri
{
    using System;
    using System.Runtime.CompilerServices;

    public class DelaunayTriangle
    {
        public FixedBitArray3 EdgeIsConstrained;
        public FixedBitArray3 EdgeIsDelaunay;
        public FixedArray3<DelaunayTriangle> Neighbors;
        public FixedArray3<TriangulationPoint> Points;

        public DelaunayTriangle(TriangulationPoint p1, TriangulationPoint p2, TriangulationPoint p3)
        {
            this.Points[0] = p1;
            this.Points[1] = p2;
            this.Points[2] = p3;
        }

        public bool Contains(TriangulationPoint p)
        {
            return this.Points.Contains(p);
        }

        public int EdgeIndex(TriangulationPoint p1, TriangulationPoint p2)
        {
            int index = this.Points.IndexOf(p1);
            int num2 = this.Points.IndexOf(p2);
            bool flag = (index == 0) || (num2 == 0);
            bool flag2 = (index == 1) || (num2 == 1);
            bool flag3 = (index == 2) || (num2 == 2);
            if (flag2 && flag3)
            {
                return 0;
            }
            if (flag && flag3)
            {
                return 1;
            }
            if (flag && flag2)
            {
                return 2;
            }
            return -1;
        }

        public bool GetConstrainedEdgeCCW(TriangulationPoint p)
        {
            return this.EdgeIsConstrained[(this.IndexOf(p) + 2) % 3];
        }

        public bool GetConstrainedEdgeCW(TriangulationPoint p)
        {
            return this.EdgeIsConstrained[(this.IndexOf(p) + 1) % 3];
        }

        public bool GetDelaunayEdgeCCW(TriangulationPoint p)
        {
            return this.EdgeIsDelaunay[(this.IndexOf(p) + 2) % 3];
        }

        public bool GetDelaunayEdgeCW(TriangulationPoint p)
        {
            return this.EdgeIsDelaunay[(this.IndexOf(p) + 1) % 3];
        }

        public int IndexCCWFrom(TriangulationPoint p)
        {
            return ((this.IndexOf(p) + 1) % 3);
        }

        public int IndexOf(TriangulationPoint p)
        {
            int index = this.Points.IndexOf(p);
            if (index == -1)
            {
                throw new Exception("Calling index with a point that doesn't exist in triangle");
            }
            return index;
        }

        public void Legalize(TriangulationPoint oPoint, TriangulationPoint nPoint)
        {
            this.RotateCW();
            this.Points[this.IndexCCWFrom(oPoint)] = nPoint;
        }

        public void MarkConstrainedEdge(int index)
        {
            this.EdgeIsConstrained[index] = true;
        }

        public void MarkConstrainedEdge(TriangulationPoint p, TriangulationPoint q)
        {
            int num = this.EdgeIndex(p, q);
            if (num != -1)
            {
                this.EdgeIsConstrained[num] = true;
            }
        }

        public void MarkNeighbor(DelaunayTriangle t)
        {
            bool flag = t.Contains(this.Points[0]);
            bool flag2 = t.Contains(this.Points[1]);
            bool flag3 = t.Contains(this.Points[2]);
            if (flag2 && flag3)
            {
                this.Neighbors[0] = t;
                t.MarkNeighbor(this.Points[1], this.Points[2], this);
            }
            else if (flag && flag3)
            {
                this.Neighbors[1] = t;
                t.MarkNeighbor(this.Points[0], this.Points[2], this);
            }
            else
            {
                if (!flag || !flag2)
                {
                    throw new Exception("Failed to mark neighbor, doesn't share an edge!");
                }
                this.Neighbors[2] = t;
                t.MarkNeighbor(this.Points[0], this.Points[1], this);
            }
        }

        private void MarkNeighbor(TriangulationPoint p1, TriangulationPoint p2, DelaunayTriangle t)
        {
            int num = this.EdgeIndex(p1, p2);
            if (num == -1)
            {
                throw new Exception("Error marking neighbors -- t doesn't contain edge p1-p2!");
            }
            this.Neighbors[num] = t;
        }

        public DelaunayTriangle NeighborAcrossFrom(TriangulationPoint point)
        {
            return this.Neighbors[this.Points.IndexOf(point)];
        }

        public DelaunayTriangle NeighborCCWFrom(TriangulationPoint point)
        {
            return this.Neighbors[(this.Points.IndexOf(point) + 2) % 3];
        }

        public DelaunayTriangle NeighborCWFrom(TriangulationPoint point)
        {
            return this.Neighbors[(this.Points.IndexOf(point) + 1) % 3];
        }

        public TriangulationPoint OppositePoint(DelaunayTriangle t, TriangulationPoint p)
        {
            return this.PointCWFrom(t.PointCWFrom(p));
        }

        public TriangulationPoint PointCCWFrom(TriangulationPoint point)
        {
            return this.Points[(this.IndexOf(point) + 1) % 3];
        }

        public TriangulationPoint PointCWFrom(TriangulationPoint point)
        {
            return this.Points[(this.IndexOf(point) + 2) % 3];
        }

        private void RotateCW()
        {
            TriangulationPoint point = this.Points[2];
            this.Points[2] = this.Points[1];
            this.Points[1] = this.Points[0];
            this.Points[0] = point;
        }

        public void SetConstrainedEdgeCCW(TriangulationPoint p, bool ce)
        {
            this.EdgeIsConstrained[(this.IndexOf(p) + 2) % 3] = ce;
        }

        public void SetConstrainedEdgeCW(TriangulationPoint p, bool ce)
        {
            this.EdgeIsConstrained[(this.IndexOf(p) + 1) % 3] = ce;
        }

        public void SetDelaunayEdgeCCW(TriangulationPoint p, bool ce)
        {
            this.EdgeIsDelaunay[(this.IndexOf(p) + 2) % 3] = ce;
        }

        public void SetDelaunayEdgeCW(TriangulationPoint p, bool ce)
        {
            this.EdgeIsDelaunay[(this.IndexOf(p) + 1) % 3] = ce;
        }

        public override string ToString()
        {
            object[] objArray1 = new object[] { this.Points[0], ",", this.Points[1], ",", this.Points[2] };
            return string.Concat(objArray1);
        }

        public bool IsInterior { get; set; }
    }
}

