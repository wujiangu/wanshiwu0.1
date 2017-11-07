namespace Pathfinding.Poly2Tri
{
    using System;

    public class DTSweepDebugContext : TriangulationDebugContext
    {
        private DTSweepConstraint _activeConstraint;
        private AdvancingFrontNode _activeNode;
        private TriangulationPoint _activePoint;
        private DelaunayTriangle _primaryTriangle;
        private DelaunayTriangle _secondaryTriangle;

        public override void Clear()
        {
            this.PrimaryTriangle = null;
            this.SecondaryTriangle = null;
            this.ActivePoint = null;
            this.ActiveNode = null;
            this.ActiveConstraint = null;
        }

        public DTSweepConstraint ActiveConstraint
        {
            set
            {
                this._activeConstraint = value;
                base._tcx.Update("set ActiveConstraint");
            }
        }

        public AdvancingFrontNode ActiveNode
        {
            set
            {
                this._activeNode = value;
                base._tcx.Update("set ActiveNode");
            }
        }

        public TriangulationPoint ActivePoint
        {
            set
            {
                this._activePoint = value;
                base._tcx.Update("set ActivePoint");
            }
        }

        public DelaunayTriangle PrimaryTriangle
        {
            set
            {
                this._primaryTriangle = value;
                base._tcx.Update("set PrimaryTriangle");
            }
        }

        public DelaunayTriangle SecondaryTriangle
        {
            set
            {
                this._secondaryTriangle = value;
                base._tcx.Update("set SecondaryTriangle");
            }
        }
    }
}

