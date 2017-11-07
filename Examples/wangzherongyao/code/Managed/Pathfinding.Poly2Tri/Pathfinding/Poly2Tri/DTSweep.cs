namespace Pathfinding.Poly2Tri
{
    using System;
    using System.Collections.Generic;

    public static class DTSweep
    {
        private const double PI_3div4 = 2.3561944901923448;
        private const double PI_div2 = 1.5707963267948966;

        private static double BasinAngle(AdvancingFrontNode node)
        {
            double x = node.Point.X - node.Next.Next.Point.X;
            double y = node.Point.Y - node.Next.Next.Point.Y;
            return Math.Atan2(y, x);
        }

        private static void EdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            try
            {
                tcx.EdgeEvent.ConstrainedEdge = edge;
                tcx.EdgeEvent.Right = edge.P.X > edge.Q.X;
                if (tcx.IsDebugEnabled)
                {
                    tcx.DTDebugContext.PrimaryTriangle = node.Triangle;
                }
                if (!IsEdgeSideOfTriangle(node.Triangle, edge.P, edge.Q))
                {
                    FillEdgeEvent(tcx, edge, node);
                    EdgeEvent(tcx, edge.P, edge.Q, node.Triangle, edge.Q);
                }
            }
            catch (PointOnEdgeException)
            {
                throw;
            }
        }

        private static void EdgeEvent(DTSweepContext tcx, TriangulationPoint ep, TriangulationPoint eq, DelaunayTriangle triangle, TriangulationPoint point)
        {
            if (tcx.IsDebugEnabled)
            {
                tcx.DTDebugContext.PrimaryTriangle = triangle;
            }
            if (!IsEdgeSideOfTriangle(triangle, ep, eq))
            {
                TriangulationPoint pb = triangle.PointCCWFrom(point);
                Orientation orientation = TriangulationUtil.Orient2d(eq, pb, ep);
                if (orientation == Orientation.Collinear)
                {
                    throw new PointOnEdgeException("EdgeEvent - Point on constrained edge not supported yet", eq, pb, ep);
                }
                TriangulationPoint point3 = triangle.PointCWFrom(point);
                Orientation orientation2 = TriangulationUtil.Orient2d(eq, point3, ep);
                if (orientation2 == Orientation.Collinear)
                {
                    throw new PointOnEdgeException("EdgeEvent - Point on constrained edge not supported yet", eq, point3, ep);
                }
                if (orientation == orientation2)
                {
                    if (orientation == Orientation.CW)
                    {
                        triangle = triangle.NeighborCCWFrom(point);
                    }
                    else
                    {
                        triangle = triangle.NeighborCWFrom(point);
                    }
                    EdgeEvent(tcx, ep, eq, triangle, point);
                }
                else
                {
                    FlipEdgeEvent(tcx, ep, eq, triangle, point);
                }
            }
        }

        private static void Fill(DTSweepContext tcx, AdvancingFrontNode node)
        {
            DelaunayTriangle item = new DelaunayTriangle(node.Prev.Point, node.Point, node.Next.Point);
            item.MarkNeighbor(node.Prev.Triangle);
            item.MarkNeighbor(node.Triangle);
            tcx.Triangles.Add(item);
            node.Prev.Next = node.Next;
            node.Next.Prev = node.Prev;
            tcx.RemoveNode(node);
            if (!Legalize(tcx, item))
            {
                tcx.MapTriangleToNodes(item);
            }
        }

        private static void FillAdvancingFront(DTSweepContext tcx, AdvancingFrontNode n)
        {
            AdvancingFrontNode node;
            double num;
            for (node = n.Next; node.HasNext; node = node.Next)
            {
                num = HoleAngle(node);
                if ((num > 1.5707963267948966) || (num < -1.5707963267948966))
                {
                    break;
                }
                Fill(tcx, node);
            }
            for (node = n.Prev; node.HasPrev; node = node.Prev)
            {
                num = HoleAngle(node);
                if ((num > 1.5707963267948966) || (num < -1.5707963267948966))
                {
                    break;
                }
                Fill(tcx, node);
            }
            if ((n.HasNext && n.Next.HasNext) && (BasinAngle(n) < 2.3561944901923448))
            {
                FillBasin(tcx, n);
            }
        }

        private static void FillBasin(DTSweepContext tcx, AdvancingFrontNode node)
        {
            if (TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Orientation.CCW)
            {
                tcx.Basin.leftNode = node;
            }
            else
            {
                tcx.Basin.leftNode = node.Next;
            }
            tcx.Basin.bottomNode = tcx.Basin.leftNode;
            while (tcx.Basin.bottomNode.HasNext && (tcx.Basin.bottomNode.Point.Y >= tcx.Basin.bottomNode.Next.Point.Y))
            {
                tcx.Basin.bottomNode = tcx.Basin.bottomNode.Next;
            }
            if (tcx.Basin.bottomNode != tcx.Basin.leftNode)
            {
                tcx.Basin.rightNode = tcx.Basin.bottomNode;
                while (tcx.Basin.rightNode.HasNext && (tcx.Basin.rightNode.Point.Y < tcx.Basin.rightNode.Next.Point.Y))
                {
                    tcx.Basin.rightNode = tcx.Basin.rightNode.Next;
                }
                if (tcx.Basin.rightNode != tcx.Basin.bottomNode)
                {
                    tcx.Basin.width = tcx.Basin.rightNode.Point.X - tcx.Basin.leftNode.Point.X;
                    tcx.Basin.leftHighest = tcx.Basin.leftNode.Point.Y > tcx.Basin.rightNode.Point.Y;
                    FillBasinReq(tcx, tcx.Basin.bottomNode);
                }
            }
        }

        private static void FillBasinReq(DTSweepContext tcx, AdvancingFrontNode node)
        {
            if (!IsShallow(tcx, node))
            {
                Fill(tcx, node);
                if ((node.Prev != tcx.Basin.leftNode) || (node.Next != tcx.Basin.rightNode))
                {
                    if (node.Prev == tcx.Basin.leftNode)
                    {
                        if (TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Orientation.CW)
                        {
                            return;
                        }
                        node = node.Next;
                    }
                    else if (node.Next == tcx.Basin.rightNode)
                    {
                        if (TriangulationUtil.Orient2d(node.Point, node.Prev.Point, node.Prev.Prev.Point) == Orientation.CCW)
                        {
                            return;
                        }
                        node = node.Prev;
                    }
                    else if (node.Prev.Point.Y < node.Next.Point.Y)
                    {
                        node = node.Prev;
                    }
                    else
                    {
                        node = node.Next;
                    }
                    FillBasinReq(tcx, node);
                }
            }
        }

        private static void FillEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            if (tcx.EdgeEvent.Right)
            {
                FillRightAboveEdgeEvent(tcx, edge, node);
            }
            else
            {
                FillLeftAboveEdgeEvent(tcx, edge, node);
            }
        }

        private static void FillLeftAboveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            while (node.Prev.Point.X > edge.P.X)
            {
                if (tcx.IsDebugEnabled)
                {
                    tcx.DTDebugContext.ActiveNode = node;
                }
                if (TriangulationUtil.Orient2d(edge.Q, node.Prev.Point, edge.P) == Orientation.CW)
                {
                    FillLeftBelowEdgeEvent(tcx, edge, node);
                }
                else
                {
                    node = node.Prev;
                }
            }
        }

        private static void FillLeftBelowEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            if (tcx.IsDebugEnabled)
            {
                tcx.DTDebugContext.ActiveNode = node;
            }
            if (node.Point.X > edge.P.X)
            {
                if (TriangulationUtil.Orient2d(node.Point, node.Prev.Point, node.Prev.Prev.Point) == Orientation.CW)
                {
                    FillLeftConcaveEdgeEvent(tcx, edge, node);
                }
                else
                {
                    FillLeftConvexEdgeEvent(tcx, edge, node);
                    FillLeftBelowEdgeEvent(tcx, edge, node);
                }
            }
        }

        private static void FillLeftConcaveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            Fill(tcx, node.Prev);
            if (((node.Prev.Point != edge.P) && (TriangulationUtil.Orient2d(edge.Q, node.Prev.Point, edge.P) == Orientation.CW)) && (TriangulationUtil.Orient2d(node.Point, node.Prev.Point, node.Prev.Prev.Point) == Orientation.CW))
            {
                FillLeftConcaveEdgeEvent(tcx, edge, node);
            }
        }

        private static void FillLeftConvexEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            if (TriangulationUtil.Orient2d(node.Prev.Point, node.Prev.Prev.Point, node.Prev.Prev.Prev.Point) == Orientation.CW)
            {
                FillLeftConcaveEdgeEvent(tcx, edge, node.Prev);
            }
            else if (TriangulationUtil.Orient2d(edge.Q, node.Prev.Prev.Point, edge.P) == Orientation.CW)
            {
                FillLeftConvexEdgeEvent(tcx, edge, node.Prev);
            }
        }

        private static void FillRightAboveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            while (node.Next.Point.X < edge.P.X)
            {
                if (tcx.IsDebugEnabled)
                {
                    tcx.DTDebugContext.ActiveNode = node;
                }
                if (TriangulationUtil.Orient2d(edge.Q, node.Next.Point, edge.P) == Orientation.CCW)
                {
                    FillRightBelowEdgeEvent(tcx, edge, node);
                }
                else
                {
                    node = node.Next;
                }
            }
        }

        private static void FillRightBelowEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            if (tcx.IsDebugEnabled)
            {
                tcx.DTDebugContext.ActiveNode = node;
            }
            if (node.Point.X < edge.P.X)
            {
                if (TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Orientation.CCW)
                {
                    FillRightConcaveEdgeEvent(tcx, edge, node);
                }
                else
                {
                    FillRightConvexEdgeEvent(tcx, edge, node);
                    FillRightBelowEdgeEvent(tcx, edge, node);
                }
            }
        }

        private static void FillRightConcaveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            Fill(tcx, node.Next);
            if (((node.Next.Point != edge.P) && (TriangulationUtil.Orient2d(edge.Q, node.Next.Point, edge.P) == Orientation.CCW)) && (TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Orientation.CCW))
            {
                FillRightConcaveEdgeEvent(tcx, edge, node);
            }
        }

        private static void FillRightConvexEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            if (TriangulationUtil.Orient2d(node.Next.Point, node.Next.Next.Point, node.Next.Next.Next.Point) == Orientation.CCW)
            {
                FillRightConcaveEdgeEvent(tcx, edge, node.Next);
            }
            else if (TriangulationUtil.Orient2d(edge.Q, node.Next.Next.Point, edge.P) == Orientation.CCW)
            {
                FillRightConvexEdgeEvent(tcx, edge, node.Next);
            }
        }

        private static void FinalizationConvexHull(DTSweepContext tcx)
        {
            DelaunayTriangle triangle;
            AdvancingFrontNode next = tcx.Front.Head.Next;
            AdvancingFrontNode c = next.Next;
            AdvancingFrontNode node3 = c.Next;
            TriangulationPoint point = next.Point;
            TurnAdvancingFrontConvex(tcx, next, c);
            next = tcx.Front.Tail.Prev;
            if (next.Triangle.Contains(next.Next.Point) && next.Triangle.Contains(next.Prev.Point))
            {
                triangle = next.Triangle.NeighborAcrossFrom(next.Point);
                RotateTrianglePair(next.Triangle, next.Point, triangle, triangle.OppositePoint(next.Triangle, next.Point));
                tcx.MapTriangleToNodes(next.Triangle);
                tcx.MapTriangleToNodes(triangle);
            }
            next = tcx.Front.Head.Next;
            if (next.Triangle.Contains(next.Prev.Point) && next.Triangle.Contains(next.Next.Point))
            {
                triangle = next.Triangle.NeighborAcrossFrom(next.Point);
                RotateTrianglePair(next.Triangle, next.Point, triangle, triangle.OppositePoint(next.Triangle, next.Point));
                tcx.MapTriangleToNodes(next.Triangle);
                tcx.MapTriangleToNodes(triangle);
            }
            point = tcx.Front.Head.Point;
            c = tcx.Front.Tail.Prev;
            triangle = c.Triangle;
            TriangulationPoint point2 = c.Point;
            while (true)
            {
                tcx.RemoveFromList(triangle);
                point2 = triangle.PointCCWFrom(point2);
                if (point2 == point)
                {
                    break;
                }
                triangle = triangle.NeighborCCWFrom(point2);
            }
            point = tcx.Front.Head.Next.Point;
            point2 = triangle.PointCWFrom(tcx.Front.Head.Point);
            triangle = triangle.NeighborCWFrom(tcx.Front.Head.Point);
            do
            {
                tcx.RemoveFromList(triangle);
                point2 = triangle.PointCCWFrom(point2);
                triangle = triangle.NeighborCCWFrom(point2);
            }
            while (point2 != point);
            tcx.FinalizeTriangulation();
        }

        private static void FinalizationPolygon(DTSweepContext tcx)
        {
            DelaunayTriangle triangle = tcx.Front.Head.Next.Triangle;
            TriangulationPoint p = tcx.Front.Head.Next.Point;
            while (!triangle.GetConstrainedEdgeCW(p))
            {
                triangle = triangle.NeighborCCWFrom(p);
            }
            tcx.MeshClean(triangle);
        }

        private static void FlipEdgeEvent(DTSweepContext tcx, TriangulationPoint ep, TriangulationPoint eq, DelaunayTriangle t, TriangulationPoint p)
        {
            DelaunayTriangle ot = t.NeighborAcrossFrom(p);
            TriangulationPoint pd = ot.OppositePoint(t, p);
            if (ot == null)
            {
                throw new InvalidOperationException("[BUG:FIXME] FLIP failed due to missing triangle");
            }
            if (tcx.IsDebugEnabled)
            {
                tcx.DTDebugContext.PrimaryTriangle = t;
                tcx.DTDebugContext.SecondaryTriangle = ot;
            }
            if (TriangulationUtil.InScanArea(p, t.PointCCWFrom(p), t.PointCWFrom(p), pd))
            {
                RotateTrianglePair(t, p, ot, pd);
                tcx.MapTriangleToNodes(t);
                tcx.MapTriangleToNodes(ot);
                if ((p == eq) && (pd == ep))
                {
                    if ((eq == tcx.EdgeEvent.ConstrainedEdge.Q) && (ep == tcx.EdgeEvent.ConstrainedEdge.P))
                    {
                        t.MarkConstrainedEdge(ep, eq);
                        ot.MarkConstrainedEdge(ep, eq);
                        Legalize(tcx, t);
                        Legalize(tcx, ot);
                    }
                }
                else
                {
                    Orientation o = TriangulationUtil.Orient2d(eq, pd, ep);
                    t = NextFlipTriangle(tcx, o, t, ot, p, pd);
                    FlipEdgeEvent(tcx, ep, eq, t, p);
                }
            }
            else
            {
                TriangulationPoint point2 = NextFlipPoint(ep, eq, ot, pd);
                FlipScanEdgeEvent(tcx, ep, eq, t, ot, point2);
                EdgeEvent(tcx, ep, eq, t, p);
            }
        }

        private static void FlipScanEdgeEvent(DTSweepContext tcx, TriangulationPoint ep, TriangulationPoint eq, DelaunayTriangle flipTriangle, DelaunayTriangle t, TriangulationPoint p)
        {
            DelaunayTriangle triangle = t.NeighborAcrossFrom(p);
            TriangulationPoint pd = triangle.OppositePoint(t, p);
            if (triangle == null)
            {
                throw new Exception("[BUG:FIXME] FLIP failed due to missing triangle");
            }
            if (tcx.IsDebugEnabled)
            {
                tcx.DTDebugContext.PrimaryTriangle = t;
                tcx.DTDebugContext.SecondaryTriangle = triangle;
            }
            if (TriangulationUtil.InScanArea(eq, flipTriangle.PointCCWFrom(eq), flipTriangle.PointCWFrom(eq), pd))
            {
                FlipEdgeEvent(tcx, eq, pd, triangle, pd);
            }
            else
            {
                TriangulationPoint point2 = NextFlipPoint(ep, eq, triangle, pd);
                FlipScanEdgeEvent(tcx, ep, eq, flipTriangle, triangle, point2);
            }
        }

        private static double HoleAngle(AdvancingFrontNode node)
        {
            double x = node.Point.X;
            double y = node.Point.Y;
            double num3 = node.Next.Point.X - x;
            double num4 = node.Next.Point.Y - y;
            double num5 = node.Prev.Point.X - x;
            double num6 = node.Prev.Point.Y - y;
            return Math.Atan2((num3 * num6) - (num4 * num5), (num3 * num5) + (num4 * num6));
        }

        private static bool IsEdgeSideOfTriangle(DelaunayTriangle triangle, TriangulationPoint ep, TriangulationPoint eq)
        {
            int index = triangle.EdgeIndex(ep, eq);
            if (index == -1)
            {
                return false;
            }
            triangle.MarkConstrainedEdge(index);
            triangle = triangle.Neighbors[index];
            if (triangle != null)
            {
                triangle.MarkConstrainedEdge(ep, eq);
            }
            return true;
        }

        private static bool IsShallow(DTSweepContext tcx, AdvancingFrontNode node)
        {
            double num;
            if (tcx.Basin.leftHighest)
            {
                num = tcx.Basin.leftNode.Point.Y - node.Point.Y;
            }
            else
            {
                num = tcx.Basin.rightNode.Point.Y - node.Point.Y;
            }
            return (tcx.Basin.width > num);
        }

        private static bool Legalize(DTSweepContext tcx, DelaunayTriangle t)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!t.EdgeIsDelaunay[i])
                {
                    DelaunayTriangle ot = t.Neighbors[i];
                    if (ot != null)
                    {
                        TriangulationPoint p = t.Points[i];
                        TriangulationPoint point2 = ot.OppositePoint(t, p);
                        int index = ot.IndexOf(point2);
                        if (ot.EdgeIsConstrained[index] || ot.EdgeIsDelaunay[index])
                        {
                            t.EdgeIsConstrained[i] = ot.EdgeIsConstrained[index];
                        }
                        else if (TriangulationUtil.SmartIncircle(p, t.PointCCWFrom(p), t.PointCWFrom(p), point2))
                        {
                            t.EdgeIsDelaunay[i] = true;
                            ot.EdgeIsDelaunay[index] = true;
                            RotateTrianglePair(t, p, ot, point2);
                            if (!Legalize(tcx, t))
                            {
                                tcx.MapTriangleToNodes(t);
                            }
                            if (!Legalize(tcx, ot))
                            {
                                tcx.MapTriangleToNodes(ot);
                            }
                            t.EdgeIsDelaunay[i] = false;
                            ot.EdgeIsDelaunay[index] = false;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static AdvancingFrontNode NewFrontTriangle(DTSweepContext tcx, TriangulationPoint point, AdvancingFrontNode node)
        {
            DelaunayTriangle item = new DelaunayTriangle(point, node.Point, node.Next.Point);
            item.MarkNeighbor(node.Triangle);
            tcx.Triangles.Add(item);
            AdvancingFrontNode node2 = new AdvancingFrontNode(point) {
                Next = node.Next,
                Prev = node
            };
            node.Next.Prev = node2;
            node.Next = node2;
            tcx.AddNode(node2);
            if (tcx.IsDebugEnabled)
            {
                tcx.DTDebugContext.ActiveNode = node2;
            }
            if (!Legalize(tcx, item))
            {
                tcx.MapTriangleToNodes(item);
            }
            return node2;
        }

        private static TriangulationPoint NextFlipPoint(TriangulationPoint ep, TriangulationPoint eq, DelaunayTriangle ot, TriangulationPoint op)
        {
            switch (TriangulationUtil.Orient2d(eq, op, ep))
            {
                case Orientation.CW:
                    return ot.PointCCWFrom(op);

                case Orientation.CCW:
                    return ot.PointCWFrom(op);

                case Orientation.Collinear:
                    throw new PointOnEdgeException("Point on constrained edge not supported yet", eq, op, ep);
            }
            throw new NotImplementedException("Orientation not handled");
        }

        private static DelaunayTriangle NextFlipTriangle(DTSweepContext tcx, Orientation o, DelaunayTriangle t, DelaunayTriangle ot, TriangulationPoint p, TriangulationPoint op)
        {
            int num;
            if (o == Orientation.CCW)
            {
                num = ot.EdgeIndex(p, op);
                ot.EdgeIsDelaunay[num] = true;
                Legalize(tcx, ot);
                ot.EdgeIsDelaunay.Clear();
                return t;
            }
            num = t.EdgeIndex(p, op);
            t.EdgeIsDelaunay[num] = true;
            Legalize(tcx, t);
            t.EdgeIsDelaunay.Clear();
            return ot;
        }

        private static AdvancingFrontNode PointEvent(DTSweepContext tcx, TriangulationPoint point)
        {
            AdvancingFrontNode node = tcx.LocateNode(point);
            if (tcx.IsDebugEnabled)
            {
                tcx.DTDebugContext.ActiveNode = node;
            }
            AdvancingFrontNode node2 = NewFrontTriangle(tcx, point, node);
            if (point.X <= (node.Point.X + TriangulationUtil.EPSILON))
            {
                Fill(tcx, node);
            }
            tcx.AddNode(node2);
            FillAdvancingFront(tcx, node2);
            return node2;
        }

        private static void RotateTrianglePair(DelaunayTriangle t, TriangulationPoint p, DelaunayTriangle ot, TriangulationPoint op)
        {
            DelaunayTriangle triangle = t.NeighborCCWFrom(p);
            DelaunayTriangle triangle2 = t.NeighborCWFrom(p);
            DelaunayTriangle triangle3 = ot.NeighborCCWFrom(op);
            DelaunayTriangle triangle4 = ot.NeighborCWFrom(op);
            bool constrainedEdgeCCW = t.GetConstrainedEdgeCCW(p);
            bool constrainedEdgeCW = t.GetConstrainedEdgeCW(p);
            bool ce = ot.GetConstrainedEdgeCCW(op);
            bool flag4 = ot.GetConstrainedEdgeCW(op);
            bool delaunayEdgeCCW = t.GetDelaunayEdgeCCW(p);
            bool delaunayEdgeCW = t.GetDelaunayEdgeCW(p);
            bool flag7 = ot.GetDelaunayEdgeCCW(op);
            bool flag8 = ot.GetDelaunayEdgeCW(op);
            t.Legalize(p, op);
            ot.Legalize(op, p);
            ot.SetDelaunayEdgeCCW(p, delaunayEdgeCCW);
            t.SetDelaunayEdgeCW(p, delaunayEdgeCW);
            t.SetDelaunayEdgeCCW(op, flag7);
            ot.SetDelaunayEdgeCW(op, flag8);
            ot.SetConstrainedEdgeCCW(p, constrainedEdgeCCW);
            t.SetConstrainedEdgeCW(p, constrainedEdgeCW);
            t.SetConstrainedEdgeCCW(op, ce);
            ot.SetConstrainedEdgeCW(op, flag4);
            t.Neighbors.Clear();
            ot.Neighbors.Clear();
            if (triangle != null)
            {
                ot.MarkNeighbor(triangle);
            }
            if (triangle2 != null)
            {
                t.MarkNeighbor(triangle2);
            }
            if (triangle3 != null)
            {
                t.MarkNeighbor(triangle3);
            }
            if (triangle4 != null)
            {
                ot.MarkNeighbor(triangle4);
            }
            t.MarkNeighbor(ot);
        }

        private static void Sweep(DTSweepContext tcx)
        {
            List<TriangulationPoint> points = tcx.Points;
            for (int i = 1; i < points.Count; i++)
            {
                TriangulationPoint point = points[i];
                AdvancingFrontNode node = PointEvent(tcx, point);
                if (point.HasEdges)
                {
                    foreach (DTSweepConstraint constraint in point.Edges)
                    {
                        if (tcx.IsDebugEnabled)
                        {
                            tcx.DTDebugContext.ActiveConstraint = constraint;
                        }
                        EdgeEvent(tcx, constraint, node);
                    }
                }
                tcx.Update(null);
            }
        }

        public static void Triangulate(DTSweepContext tcx)
        {
            tcx.CreateAdvancingFront();
            Sweep(tcx);
            if (tcx.TriangulationMode == TriangulationMode.Polygon)
            {
                FinalizationPolygon(tcx);
            }
            else
            {
                FinalizationConvexHull(tcx);
            }
            tcx.Done();
        }

        private static void TurnAdvancingFrontConvex(DTSweepContext tcx, AdvancingFrontNode b, AdvancingFrontNode c)
        {
            AdvancingFrontNode node = b;
            while (c != tcx.Front.Tail)
            {
                if (tcx.IsDebugEnabled)
                {
                    tcx.DTDebugContext.ActiveNode = c;
                }
                if (TriangulationUtil.Orient2d(b.Point, c.Point, c.Next.Point) == Orientation.CCW)
                {
                    Fill(tcx, c);
                    c = c.Next;
                }
                else
                {
                    if ((b != node) && (TriangulationUtil.Orient2d(b.Prev.Point, b.Point, c.Point) == Orientation.CCW))
                    {
                        Fill(tcx, b);
                        b = b.Prev;
                        continue;
                    }
                    b = c;
                    c = c.Next;
                }
            }
        }
    }
}

