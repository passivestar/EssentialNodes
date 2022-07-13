using System.Collections;
using UnityEngine;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Time")]
    [TypeIcon(typeof(Unity.VisualScripting.WaitForSecondsUnit))]
    public class Buffer : Unit
    {
        [UnitHeaderInspectable("Use Max Time")]
        public bool useMaxTime;

        [DoNotSerialize, PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        public ControlInput reset;

        [DoNotSerialize, PortLabelHidden]
        public ControlOutput exit;

        [DoNotSerialize, PortLabel("Full")]
        public ControlOutput bufferFull;

        [DoNotSerialize, PortLabelHidden]
        public ValueInput value;

        [DoNotSerialize]
        public ValueInput count;

        [DoNotSerialize]
        public ValueInput maxTime;

        [DoNotSerialize, PortLabel("Unscaled")]
        public ValueInput unscaledTime;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput buffer;

        Queue _queue = new();
        Queue _queueTimeSamples = new();
        float _elapsed;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Enter);
            reset = ControlInput(nameof(reset), Reset);
            exit = ControlOutput(nameof(exit));
            bufferFull = ControlOutput(nameof(bufferFull));

            count = ValueInput<int>(nameof(count), 10);
            if (useMaxTime)
            {
                maxTime = ValueInput<float>(nameof(maxTime), 1f);
                unscaledTime = ValueInput<bool>(nameof(unscaledTime), false);
            }
            value = ValueInput<object>(nameof(value));
            buffer = ValueOutput<IEnumerable>(nameof(buffer), _ => _queue);

            Succession(enter, exit);
            Succession(enter, bufferFull);
        }

        ControlOutput Enter(Flow flow)
        {
            var newValue = flow.GetValue<object>(value);
            var maxCount = flow.GetValue<int>(count);

            _queue.Enqueue(newValue);
            if (_queue.Count > maxCount)
            {
                _queue.Dequeue();
            }
            else if (_queue.Count == maxCount)
            {
                flow.Invoke(bufferFull);
            }

            if (useMaxTime)
            {
                var unscaled = flow.GetValue<bool>(unscaledTime);
                var time = flow.GetValue<float>(maxTime);
                var delta = unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                _elapsed += delta;

                _queueTimeSamples.Enqueue(_elapsed);

                // Remove outdated samples from the queue
                while (_elapsed - (float)_queueTimeSamples.Peek() > time)
                {
                    _queue.Dequeue();
                    _queueTimeSamples.Dequeue();
                }
            }

            return exit;
        }

        ControlOutput Reset(Flow flow)
        {
            _queue.Clear();
            _queueTimeSamples.Clear();
            _elapsed = 0;
            return null;
        }
    }
}