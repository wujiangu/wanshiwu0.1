namespace Pathfinding.Poly2Tri
{
    using System;
    using System.Runtime.CompilerServices;

    public class DTSweepContext : TriangulationContext
    {
        private DTSweepPointComparator _comparator = new DTSweepPointComparator();
        private readonly float ALPHA = 0.3f;
        public DTSweepBasin Basin = new DTSweepBasin();
        public DTSweepEdgeEvent EdgeEvent = new DTSweepEdgeEvent();
        public AdvancingFront Front;

        public DTSweepContext()
        {
            this.Clear();
        }

        public void AddNode(AdvancingFrontNode node)
        {
            this.Front.AddNode(node);
        }

        public override void Clear()
        {
            base.Clear();
            base.Triangles.Clear();
        }

        public void CreateAdvancingFront()
        {
            DelaunayTriangle item = new DelaunayTriangle(base.Points[0], this.Tail, this.Head);
            base.Triangles.Add(item);
            AdvancingFrontNode head = new AdvancingFrontNode(item.Points[1]) {
                Triangle = item
            };
            AdvancingFrontNode node = new AdvancingFrontNode(item.Points[0]) {
                Triangle = item
            };
            AdvancingFrontNode tail = new AdvancingFrontNode(item.Points[2]);
            this.Front = new AdvancingFront(head, tail);
            this.Front.AddNode(node);
            this.Front.Head.Next = node;
            node.Next = this.Front.Tail;
            node.Prev = this.Front.Head;
            this.Front.Tail.Prev = node;
        }

        public void FinalizeTriangulation()
        {
            base.Triangulatable.AddTriangles(base.Triangles);
            base.Triangles.Clear();
        }

        public AdvancingFrontNode LocateNode(TriangulationPoint point)
        {
            return this.Front.LocateNode(point);
        }

        public void MapTriangleToNodes(DelaunayTriangle t)
        {
            for (int i = 0; i < 3; i++)
            {
                if (t.Neighbors[i] == null)
                {
                    AdvancingFrontNode node = this.Front.LocatePoint(t.PointCWFrom(t.Points[i]));
                    if (node != null)
                    {
                        node.Triangle = t;
                    }
                }
            }
        }

        public void MeshClean(DelaunayTriangle triangle)
        {
            this.MeshCleanReq(triangle);
        }

        private void MeshCleanReq(DelaunayTriangle triangle)
        {
            if ((triangle != null) && !triangle.IsInterior)
            {
                triangle.IsInterior = true;
                base.Triangulatable.AddTriangle(triangle);
                for (int i = 0; i < 3; i++)
                {
                    if (!triangle.EdgeIsConstrained[i])
                    {
                        this.MeshCleanReq(triangle.Neighbors[i]);
                    }
                }
            }
        }

        public override TriangulationConstraint NewConstraint(TriangulationPoint a, TriangulationPoint b)
        {
            return new DTSweepConstraint(a, b);
        }

        public override void PrepareTriangulation(Triangulatable t)
        {
            double num2;
            double num4;
            base.PrepareTriangulation(t);
            double x = num2 = base.Points[0].X;
            double y = num4 = base.Points[0].Y;
            foreach (TriangulationPoint point in base.Points)
            {
                if (point.X > x)
                {
                    x = point.X;
                }
                if (point.X < num2)
                {
                    num2 = point.X;
                }
                if (point.Y > y)
                {
                    y = point.Y;
                }
                if (point.Y < num4)
                {
                    num4 = point.Y;
                }
            }
            double num5 = this.ALPHA * (x - num2);
            double num6 = this.ALPHA * (y - num4);
            TriangulationPoint point2 = new TriangulationPoint(x + num5, num4 - num6);
            TriangulationPoint point3 = new TriangulationPoint(num2 - num5, num4 - num6);
            this.Head = point2;
            this.Tail = point3;
            base.Points.Sort(this._comparator);
        }

        public void RemoveFromList(DelaunayTriangle triangle)
        {
            base.Triangles.Remove(triangle);
        }

        public void RemoveNode(AdvancingFrontNode node)
        {
            this.Front.RemoveNode(node);
        }

        public override TriangulationAlgorithm Algorithm
        {
            get
            {
                return TriangulationAlgorithm.DTSweep;
            }
        }

        public TriangulationPoint Head { get; set; }

        public override bool IsDebugEnabled
        {
            get
            {
                return base.IsDebugEnabled;
            }
        }

        public TriangulationPoint Tail { get; set; }
    }
}

