using System;
using UnityEngine;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Time")]
    [TypeIcon(typeof(Unity.VisualScripting.Timer))]
    public class Debounce : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public float elapsed;
            public float lastEnterTime;
            public bool active;
            public bool unscaledTime;
            public bool isListening;
            public Delegate update;
        }

        [DoNotSerialize, PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize, PortLabelHidden]
        public ControlOutput exit;

        [DoNotSerialize]
        public ValueInput interval;

        [DoNotSerialize, PortLabel("Unscaled")]
        public ValueInput unscaledTime;

        protected override void Definition()
        {
            isControlRoot = true;

            enter = ControlInput(nameof(enter), Enter);
            exit = ControlOutput(nameof(exit));
            interval = ValueInput<float>(nameof(interval), 1f);
            unscaledTime = ValueInput<bool>(nameof(unscaledTime), false);

            Succession(enter, exit);
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
            using (var flow = Flow.New(reference))
            {
                Update(flow);
            }
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

            if (data.elapsed - data.lastEnterTime < flow.GetValue<float>(interval))
            {
                return;
            }

            data.active = false;

            flow.Invoke(exit);
        }

        ControlOutput Enter(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);

            data.lastEnterTime = data.elapsed;
            data.active = true;

            return null;
        }
    }
}