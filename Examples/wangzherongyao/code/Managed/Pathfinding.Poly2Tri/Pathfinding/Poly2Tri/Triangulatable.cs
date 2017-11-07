namespace Pathfinding.Poly2Tri
{
    using System;
    using System.Collections.Generic;

    public interface Triangulatable
    {
        void AddTriangle(DelaunayTriangle t);
        void AddTriangles(IEnumerable<DelaunayTriangle> list);
        void ClearTriangles();
        void Prepare(TriangulationContext tcx);

        IList<TriangulationPoint> Points { get; }

        IList<DelaunayTriangle> Triangles { get; }

        Pathfinding.Poly2Tri.TriangulationMode TriangulationMode { get; }
    }
}

