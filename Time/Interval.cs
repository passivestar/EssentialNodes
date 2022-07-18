using System;
using UnityEngine;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Time")]
    [TypeIcon(typeof(Unity.VisualScripting.Timer))]
    public class Interval : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public float elapsed;
            public float elapsedRatio;
            public float duration;
            public bool active;
            public bool unscaledTime;
            public bool isListening;
            public Delegate update;
        }

        [DoNotSerialize]
        public ControlInput start;

        [DoNotSerialize]
        public ControlInput pause;

        [DoNotSerialize]
        public ControlInput resume;

        [DoNotSerialize]
        public ControlInput toggle;

        [DoNotSerialize]
        public ControlOutput started;

        [DoNotSerialize]
        public ControlOutput tick;

        [DoNotSerialize]
        public ControlOutput cycle;

        [DoNotSerialize]
        public ValueInput duration;

        [DoNotSerialize, PortLabel("Unscaled")]
        public ValueInput unscaledTime;

        [DoNotSerialize, PortLabel("Elapsed")]
        public ValueOutput elapsedSeconds;

        [DoNotSerialize, PortLabel("Elapsed %")]
        public ValueOutput elapsedRatio;

        [DoNotSerialize, PortLabel("Remaining")]
        public ValueOutput remainingSeconds;

        [DoNotSerialize, PortLabel("Remaining %")]
        public ValueOutput remainingRatio;

        protected override void Definition()
        {
            isControlRoot = true;

            start = ControlInput(nameof(start), Start);
            pause = ControlInput(nameof(pause), Pause);
            resume = ControlInput(nameof(resume), Resume);
            toggle = ControlInput(nameof(toggle), Toggle);

            started = ControlOutput(nameof(started));
            tick = ControlOutput(nameof(tick));
            cycle = ControlOutput(nameof(cycle));

            duration = ValueInput<float>(nameof(duration), 1f);
            unscaledTime = ValueInput<bool>(nameof(unscaledTime), false);

            elapsedSeconds = ValueOutput<bool>(nameof(elapsedSeconds));
            elapsedRatio = ValueOutput<bool>(nameof(elapsedRatio));
            remainingSeconds = ValueOutput<bool>(nameof(remainingSeconds));
            remainingRatio = ValueOutput<bool>(nameof(remainingRatio));

            Succession(start, started);
            Succession(start, tick);
            Succession(start, cycle);
        }

        public IGraphElementData CreateData()
        {
            return new Data();
        }

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);

            if (data.isListening)
            {
                return;
            }

            var reference = stack.ToReference();
            var hook = new EventHook(EventHooks.Update, stack.machine);
            Action<EmptyEventArgs> update = args => TriggerUpdate(reference);
            EventBus.Register(hook, update);
            data.update = update;
            data.isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);

            if (!data.isListening)
            {
                return;
            }

            var hook = new EventHook(EventHooks.Update, stack.machine);
            EventBus.Unregister(hook, data.update);
            data.update = null;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer)
        {
            return pointer.GetElementData<Data>(this).isListening;
        }

        void TriggerUpdate(GraphReference reference)
        {
            using var flow = Flow.New(reference);
            Update(flow);
        }

        void AssignOutputValues(Flow flow, Data data)
        {
            flow.SetValue(elapsedSeconds, data.elapsed);
            flow.SetValue(elapsedRatio, data.elapsedRatio);

            flow.SetValue(remainingSeconds, Mathf.Max(0, data.duration - data.elapsed));
            flow.SetValue(remainingRatio, Mathf.Clamp01((data.duration - data.elapsed) / data.duration));
        }

        void Update(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);

            if (!data.active)
            {
                return;
            }

            var delta = data.unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            data.elapsed += delta;
            data.elapsed = Mathf.Min(data.elapsed, data.duration);
            data.elapsedRatio = Mathf.Clamp01(data.elapsed / data.duration);

            AssignOutputValues(flow, data);

            var stack = flow.PreserveStack();

            flow.Invoke(tick);

            if (data.elapsed >= data.duration)
            {
                flow.RestoreStack(stack);
                flow.Invoke(cycle);
            }

            flow.DisposePreservedStack(stack);
        }

        ControlOutput Start(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);

            data.duration = flow.GetValue<float>(duration);
            data.unscaledTime = flow.GetValue<bool>(unscaledTime);
            data.active = true;

            AssignOutputValues(flow, data);
            return started;
        }

        ControlOutput Toggle(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);

            if (data.active)
            {
                return Pause(flow);
            }
            else
            {
                return Resume(flow);
            }
        }

        ControlOutput Pause(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            data.active = false;
            return null;
        }

        ControlOutput Resume(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            data.active = true;
            return null;
        }
    }
}
