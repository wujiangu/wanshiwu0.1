namespace Pathfinding.Poly2Tri
{
    using System;

    public static class P2T
    {
        private static TriangulationAlgorithm _defaultAlgorithm;

        public static TriangulationContext CreateContext(TriangulationAlgorithm algorithm)
        {
            if (algorithm != TriangulationAlgorithm.DTSweep)
            {
            }
            return new DTSweepContext();
        }

        public static void Triangulate(Polygon p)
        {
            Triangulate(_defaultAlgorithm, p);
        }

        public static void Triangulate(TriangulationContext tcx)
        {
            if (tcx.Algorithm != TriangulationAlgorithm.DTSweep)
            {
            }
            DTSweep.Triangulate((DTSweepContext) tcx);
        }

        public static void Triangulate(TriangulationAlgorithm algorithm, Triangulatable t)
        {
            TriangulationContext tcx = CreateContext(algorithm);
            tcx.PrepareTriangulation(t);
            Triangulate(tcx);
        }
    }
}

