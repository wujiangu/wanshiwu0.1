﻿namespace Pathfinding
{
    using System;

    [Serializable]
    public abstract class PathModifier : IPathModifier
    {
        public int priority;
        [NonSerialized]
        public Seeker seeker;

        protected PathModifier()
        {
        }

        public abstract void Apply(Path p, ModifierData source);
        [Obsolete]
        public virtual void ApplyOriginal(Path p)
        {
        }

        public void Awake(Seeker s)
        {
            this.seeker = s;
            if (s != null)
            {
                s.RegisterModifier(this);
            }
        }

        public void OnDestroy(Seeker s)
        {
            if (s != null)
            {
                s.DeregisterModifier(this);
            }
        }

        [Obsolete]
        public virtual void PreProcess(Path p)
        {
        }

        public abstract ModifierData input { get; }

        public abstract ModifierData output { get; }

        public int Priority
        {
            get
            {
                return this.priority;
            }
            set
            {
                this.priority = value;
            }
        }
    }
}

