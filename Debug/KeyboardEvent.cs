using System;
using Unity.VisualScripting;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#else
using UnityEngine;
#endif

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Debug")]
    [TypeIcon(typeof(UnityEngine.Debug))]
    public class KeyboardEvent : Unit, IGraphElementWithData, IGraphEventListener
    {
        public sealed class Data : IGraphElementData
        {
            public bool isListening;
            public Delegate update;
        }

        [DoNotSerialize, PortLabelHidden]
        public ControlOutput exit;

        [DoNotSerialize, PortLabelHidden]
        public ValueInput key;

        protected override void Definition()
        {
            isControlRoot = true;
            exit = ControlOutput(nameof(exit));
#if ENABLE_INPUT_SYSTEM
            key = ValueInput<Key>(nameof(key), Key.Digit1);
#else
            key = ValueInput<KeyCode>(nameof(key), KeyCode.Alpha1);
#endif
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
#if ENABLE_INPUT_SYSTEM
            var key = flow.GetValue<Key>(this.key);
            if (Keyboard.current[key].wasPressedThisFrame)
#else
            var key = flow.GetValue<KeyCode>(this.key);
            if (Input.GetKeyDown(key))
#endif
            {
                flow.Invoke(exit);
            }
        }
    }
}