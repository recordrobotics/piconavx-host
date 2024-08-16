using piconavx.ui.graphics;
using piconavx.ui.graphics.ui;
using System;

namespace piconavx.ui
{
    /// <summary>
    /// Base class for all EventRepeaters. Contains the repeating logic
    /// </summary>
    public abstract class EventRepeater
    {
        private bool started;
        public bool Started => started;

        private double initialDelay = 0.5;
        public double InitialDelay { get => initialDelay; set => initialDelay = value; }

        private double repeatInterval = 0.033;
        public double RepeatInterval { get => repeatInterval; set => repeatInterval = value; }

        protected abstract void InvokeEvent();

        protected EventRepeater()
        {
            started = true;
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
            Scene.InvokeLater(InvokeEvent, DeferralMode.NextEvent);
        }

        protected void Stop()
        {
            started = false;
            Scene.Update -= Scene_Update;
        }

        private double timer = 0;
        private bool passedDelay = false;
        private void Scene_Update(double delta)
        {
            if (started)
            {
                timer += delta;
                if ((!passedDelay && timer >= initialDelay) || (passedDelay && timer >= repeatInterval))
                {
                    passedDelay = true;
                    timer = 0;
                    InvokeEvent();
                }
            }
        }
    }

    public class EventRepeater<P>() : EventRepeater where P : Enum
    {
        public PrioritizedList<PrioritizedAction<P>> Event = [];

        private PrioritizedAction<P>? stop = null;
        public PrioritizedAction<P> Stop(P priority, PrioritizedList<PrioritizedAction<P>> evt)
        {
            stop ??= new PrioritizedAction<P>(priority, () => { Stop(); Scene.InvokeLater(() => { if (stop != null) evt -= stop; }, DeferralMode.NextEvent); });
            return stop;
        }

        protected override void InvokeEvent()
        {
            foreach (var evt in Event)
            {
                evt.Action.Invoke();
            }
        }
    }

    public class EventRepeater<P, T1>(T1 arg0) : EventRepeater() where P : Enum
    {
        public PrioritizedList<PrioritizedAction<P, T1>> Event = [];

        private readonly Tuple<T1> eventData = new(arg0);

        private PrioritizedAction<P, T1>? stop = null;
        public PrioritizedAction<P, T1> Stop(P priority, PrioritizedList<PrioritizedAction<P, T1>> evt, bool matchArg0 = false)
        {
            stop ??= new PrioritizedAction<P, T1>(priority, (arg0) => { if ((!matchArg0 || (arg0?.Equals(eventData.Item1) ?? false))) { Stop(); Scene.InvokeLater(() => { if (stop != null) evt -= stop; }, DeferralMode.NextEvent); } });
            return stop;
        }

        protected override void InvokeEvent()
        {
            foreach (var evt in Event)
            {
                evt.Action.Invoke(eventData.Item1);
            }
        }
    }

    public class EventRepeater<P, T1, T2>(T1 arg0, T2 arg1) : EventRepeater() where P : Enum
    {
        public PrioritizedList<PrioritizedAction<P, T1, T2>> Event = [];

        private readonly Tuple<T1, T2> eventData = new(arg0, arg1);

        private PrioritizedAction<P, T1, T2>? stop = null;
        public PrioritizedAction<P, T1, T2> Stop(P priority, PrioritizedList<PrioritizedAction<P, T1, T2>> evt, bool matchArg0 = false, bool matchArg1 = false)
        {
            stop ??= new PrioritizedAction<P, T1, T2>(priority, (arg0, arg1) => { if ((!matchArg0 || (arg0?.Equals(eventData.Item1) ?? false)) && (!matchArg1 || (arg1?.Equals(eventData.Item2) ?? false))) { Stop(); Scene.InvokeLater(() => { if (stop != null) evt -= stop; }, DeferralMode.NextEvent); } });
            return stop;
        }

        protected override void InvokeEvent()
        {
            foreach (var evt in Event)
            {
                evt.Action.Invoke(eventData.Item1, eventData.Item2);
            }
        }
    }

    public class EventRepeater<P, T1, T2, T3>(T1 arg0, T2 arg1, T3 arg2) : EventRepeater() where P : Enum
    {
        public PrioritizedList<PrioritizedAction<P, T1, T2, T3>> Event = [];

        private readonly Tuple<T1, T2, T3> eventData = new(arg0, arg1, arg2);

        private PrioritizedAction<P, T1, T2, T3>? stop = null;
        public PrioritizedAction<P, T1, T2, T3> Stop(P priority, PrioritizedList<PrioritizedAction<P, T1, T2, T3>> evt, bool matchArg0 = false, bool matchArg1 = false, bool matchArg2 = false)
        {
            stop ??= new PrioritizedAction<P, T1, T2, T3>(priority, (arg0, arg1, arg2) => { if ((!matchArg0 || (arg0?.Equals(eventData.Item1) ?? false)) && (!matchArg1 || (arg1?.Equals(eventData.Item2) ?? false)) && (!matchArg2 || (arg2?.Equals(eventData.Item3) ?? false))) { Stop(); Scene.InvokeLater(() => { if (stop != null) evt -= stop; }, DeferralMode.NextEvent); } });
            return stop;
        }

        protected override void InvokeEvent()
        {
            foreach (var evt in Event)
            {
                evt.Action.Invoke(eventData.Item1, eventData.Item2, eventData.Item3);
            }
        }
    }

    public class EventRepeater<P, T1, T2, T3, T4>(T1 arg0, T2 arg1, T3 arg2, T4 arg3) : EventRepeater() where P : Enum
    {
        public PrioritizedList<PrioritizedAction<P, T1, T2, T3, T4>> Event = [];

        private readonly Tuple<T1, T2, T3, T4> eventData = new(arg0, arg1, arg2, arg3);

        private PrioritizedAction<P, T1, T2, T3, T4>? stop = null;
        public PrioritizedAction<P, T1, T2, T3, T4> Stop(P priority, PrioritizedList<PrioritizedAction<P, T1, T2, T3, T4>> evt, bool matchArg0 = false, bool matchArg1 = false, bool matchArg2 = false, bool matchArg3 = false)
        {
            stop ??= new PrioritizedAction<P, T1, T2, T3, T4>(priority, (arg0, arg1, arg2, arg3) => { if ((!matchArg0 || (arg0?.Equals(eventData.Item1) ?? false)) && (!matchArg1 || (arg1?.Equals(eventData.Item2) ?? false)) && (!matchArg2 || (arg2?.Equals(eventData.Item3) ?? false)) && (!matchArg3 || (arg2?.Equals(eventData.Item4) ?? false))) { Stop(); Scene.InvokeLater(() => { if (stop != null) evt -= stop; }, DeferralMode.NextEvent); } });
            return stop;
        }

        protected override void InvokeEvent()
        {
            foreach (var evt in Event)
            {
                evt.Action.Invoke(eventData.Item1, eventData.Item2, eventData.Item3, eventData.Item4);
            }
        }
    }
}
