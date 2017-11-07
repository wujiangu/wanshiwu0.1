namespace Pathfinding.Poly2Tri
{
    using System;
    using System.Collections.Generic;

    public class Polygon : Triangulatable
    {
        protected List<Polygon> _holes;
        protected PolygonPoint _last;
        protected List<TriangulationPoint> _points = new List<TriangulationPoint>();
        protected List<TriangulationPoint> _steinerPoints;
        protected List<DelaunayTriangle> _triangles;

        public Polygon(IList<PolygonPoint> points)
        {
            if (points.Count < 3)
            {
                throw new ArgumentException("List has fewer than 3 points", "points");
            }
            this._points.Capacity = Math.Max(this._points.Capacity, this._points.Count + points.Count);
            for (int i = 0; i < points.Count; i++)
            {
                this._points.Add(points[i]);
            }
            if (points[0].Equals(points[points.Count - 1]))
            {
                this._points.RemoveAt(this._points.Count - 1);
            }
        }

        public void AddHole(Polygon poly)
        {
            if (this._holes == null)
            {
                this._holes = new List<Polygon>();
            }
            this._holes.Add(poly);
        }

        public void AddPoints(IEnumerable<PolygonPoint> list)
        {
            foreach (PolygonPoint point2 in list)
            {
                point2.Previous = this._last;
                if (this._last != null)
                {
                    point2.Next = this._last.Next;
                    this._last.Next = point2;
                }
                this._last = point2;
                this._points.Add(point2);
            }
            PolygonPoint point = (PolygonPoint) this._points[0];
            this._last.Next = point;
            point.Previous = this._last;
        }

        public void AddTriangle(DelaunayTriangle t)
        {
            this._triangles.Add(t);
        }

        public void AddTriangles(IEnumerable<DelaunayTriangle> list)
        {
            this._triangles.AddRange(list);
        }

        public void ClearTriangles()
        {
            if (this._triangles != null)
            {
                this._triangles.Clear();
            }
        }

        public void Prepare(TriangulationContext tcx)
        {
            if (this._triangles == null)
            {
                this._triangles = new List<DelaunayTriangle>(this._points.Count);
            }
            else
            {
                this._triangles.Clear();
            }
            for (int i = 0; i < (this._points.Count - 1); i++)
            {
                tcx.NewConstraint(this._points[i], this._points[i + 1]);
            }
            tcx.NewConstraint(this._points[0], this._points[this._points.Count - 1]);
            tcx.Points.AddRange(this._points);
            if (this._holes != null)
            {
                foreach (Polygon polygon in this._holes)
                {
                    for (int j = 0; j < (polygon._points.Count - 1); j++)
                    {
                        tcx.NewConstraint(polygon._points[j], polygon._points[j + 1]);
                    }
                    tcx.NewConstraint(polygon._points[0], polygon._points[polygon._points.Count - 1]);
                    tcx.Points.AddRange(polygon._points);
                }
            }
            if (this._steinerPoints != null)
            {
                tcx.Points.AddRange(this._steinerPoints);
            }
        }

        public IList<Polygon> Holes
        {
            get
            {
                return this._holes;
            }
        }

        public IList<TriangulationPoint> Points
        {
            get
            {
                return this._points;
            }
        }

        public IList<DelaunayTriangle> Triangles
        {
            get
            {
                return this._triangles;
            }
        }

        public Pathfinding.Poly2Tri.TriangulationMode TriangulationMode
        {
            get
            {
                return Pathfinding.Poly2Tri.TriangulationMode.Polygon;
            }
        }
    }
}

