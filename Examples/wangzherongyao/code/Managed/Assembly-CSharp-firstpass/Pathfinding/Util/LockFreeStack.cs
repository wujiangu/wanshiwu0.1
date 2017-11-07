namespace Pathfinding.Util
{
    using Pathfinding;
    using System;
    using System.Threading;

    public class LockFreeStack
    {
        public Path head;

        public Path PopAll()
        {
            return Interlocked.Exchange<Path>(ref this.head, null);
        }

        public void Push(Path p)
        {
            do
            {
                p.next = this.head;
            }
            while (Interlocked.CompareExchange<Path>(ref this.head, p, p.next) != p.next);
        }
    }
}

