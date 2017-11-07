namespace Pathfinding.Poly2Tri
{
    using System;
    using System.Text;

    public class AdvancingFront
    {
        public AdvancingFrontNode Head;
        protected AdvancingFrontNode Search;
        public AdvancingFrontNode Tail;

        public AdvancingFront(AdvancingFrontNode head, AdvancingFrontNode tail)
        {
            this.Head = head;
            this.Tail = tail;
            this.Search = head;
            this.AddNode(head);
            this.AddNode(tail);
        }

        public void AddNode(AdvancingFrontNode node)
        {
        }

        private AdvancingFrontNode FindSearchNode(double x)
        {
            return this.Search;
        }

        public AdvancingFrontNode LocateNode(TriangulationPoint point)
        {
            return this.LocateNode(point.X);
        }

        private AdvancingFrontNode LocateNode(double x)
        {
            AdvancingFrontNode node = this.FindSearchNode(x);
            if (x < node.Value)
            {
                while ((node = node.Prev) != null)
                {
                    if (x >= node.Value)
                    {
                        this.Search = node;
                        return node;
                    }
                }
            }
            else
            {
                while ((node = node.Next) != null)
                {
                    if (x < node.Value)
                    {
                        this.Search = node.Prev;
                        return node.Prev;
                    }
                }
            }
            return null;
        }

        public AdvancingFrontNode LocatePoint(TriangulationPoint point)
        {
            double x = point.X;
            AdvancingFrontNode next = this.FindSearchNode(x);
            double num2 = next.Point.X;
            if (x == num2)
            {
                if (point != next.Point)
                {
                    if (point != next.Prev.Point)
                    {
                        if (point != next.Next.Point)
                        {
                            throw new Exception("Failed to find Node for given afront point");
                        }
                        next = next.Next;
                    }
                    else
                    {
                        next = next.Prev;
                    }
                }
            }
            else if (x < num2)
            {
                while ((next = next.Prev) != null)
                {
                    if (point == next.Point)
                    {
                        break;
                    }
                }
            }
            else
            {
                while ((next = next.Next) != null)
                {
                    if (point == next.Point)
                    {
                        break;
                    }
                }
            }
            this.Search = next;
            return next;
        }

        public void RemoveNode(AdvancingFrontNode node)
        {
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (AdvancingFrontNode node = this.Head; node != this.Tail; node = node.Next)
            {
                builder.Append(node.Point.X).Append("->");
            }
            builder.Append(this.Tail.Point.X);
            return builder.ToString();
        }
    }
}

