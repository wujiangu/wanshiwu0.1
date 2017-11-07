namespace Pathfinding.ClipperLib
{
    using System;
    using System.Collections.Generic;

    public class PolyTree : PolyNode
    {
        internal List<PolyNode> m_AllPolys = new List<PolyNode>();

        public void Clear()
        {
            for (int i = 0; i < this.m_AllPolys.Count; i++)
            {
                this.m_AllPolys[i] = null;
            }
            this.m_AllPolys.Clear();
            base.m_Childs.Clear();
        }

        ~PolyTree()
        {
            this.Clear();
        }
    }
}

