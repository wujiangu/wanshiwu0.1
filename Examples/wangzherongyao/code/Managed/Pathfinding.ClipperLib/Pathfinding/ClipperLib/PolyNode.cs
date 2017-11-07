namespace Pathfinding.ClipperLib
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class PolyNode
    {
        internal List<PolyNode> m_Childs = new List<PolyNode>();
        internal int m_Index;
        internal PolyNode m_Parent;
        internal List<IntPoint> m_polygon = new List<IntPoint>();

        internal void AddChild(PolyNode Child)
        {
            int count = this.m_Childs.Count;
            this.m_Childs.Add(Child);
            Child.m_Parent = this;
            Child.m_Index = count;
        }

        public int ChildCount
        {
            get
            {
                return this.m_Childs.Count;
            }
        }

        public List<PolyNode> Childs
        {
            get
            {
                return this.m_Childs;
            }
        }

        public List<IntPoint> Contour
        {
            get
            {
                return this.m_polygon;
            }
        }

        public bool IsOpen
        {
            [CompilerGenerated]
            set
            {
                this.<IsOpen>k__BackingField = value;
            }
        }
    }
}

