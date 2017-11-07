namespace AGE
{
    using System;

    public abstract class TickEvent : BaseEvent
    {
        protected TickEvent()
        {
        }

        public virtual void PostProcess(Action _action, Track _track, int _localTime)
        {
        }

        public virtual void Process(Action _action, Track _track)
        {
        }

        public virtual void ProcessBlend(Action _action, Track _track, TickEvent _prevEvent, float _blendWeight)
        {
        }
    }
}

