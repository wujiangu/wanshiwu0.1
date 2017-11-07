namespace Pathfinding.Poly2Tri
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public abstract class TriangulationContext
    {
        public readonly List<TriangulationPoint> Points = new List<TriangulationPoint>(200);
        public readonly List<DelaunayTriangle> Triangles = new List<DelaunayTriangle>();

        protected TriangulationContext()
        {
        }

        public virtual void Clear()
        {
            this.Points.Clear();
            if (this.DebugContext != null)
            {
                this.DebugContext.Clear();
            }
            this.StepCount = 0;
        }

        public void Done()
        {
            this.StepCount++;
        }

        public abstract TriangulationConstraint NewConstraint(TriangulationPoint a, TriangulationPoint b);
        public virtual void PrepareTriangulation(Pathfinding.Poly2Tri.Triangulatable t)
        {
            this.Triangulatable = t;
            this.TriangulationMode = t.TriangulationMode;
            t.Prepare(this);
        }

        public void Update(string message)
        {
        }

        public abstract TriangulationAlgorithm Algorithm { get; }

        public TriangulationDebugContext DebugContext
        {
            [CompilerGenerated]
            get
            {
                return this.<DebugContext>k__BackingField;
            }
        }

        public DTSweepDebugContext DTDebugContext
        {
            get
            {
                return (this.DebugContext as DTSweepDebugContext);
            }
        }

        public virtual bool IsDebugEnabled
        {
            [CompilerGenerated]
            get
            {
                return this.<IsDebugEnabled>k__BackingField;
            }
        }

        public int StepCount { get; private set; }

        public Pathfinding.Poly2Tri.Triangulatable Triangulatable { get; private set; }

        public Pathfinding.Poly2Tri.TriangulationMode TriangulationMode { get; protected set; }
    }
}

