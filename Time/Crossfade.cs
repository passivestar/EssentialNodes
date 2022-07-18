using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Time")]
    [TypeIcon(typeof(Unity.VisualScripting.WaitForSecondsUnit))]
    public class CrossFade : Unit
    {
        [UnitHeaderInspectable("Outputs")]
        public int outputCount 
        {
            get => _outputCount;
            set => _outputCount = Mathf.Clamp(value, 0, 10);
        }

        [DoNotSerialize, PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize, PortLabelHidden]
        public ControlOutput exit;

        [DoNotSerialize]
        public ValueInput index;

        [DoNotSerialize]
        public ValueInput speed;

        [DoNotSerialize, PortLabel("Unscaled")]
        public ValueInput unscaledTime;

        [DoNotSerialize]
        public List<ValueOutput> valueResults = new();

        [SerializeAs("outputs")]
        int _outputCount = 2;

        [DoNotSerialize]
        List<float> _values;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Enter);
            exit = ControlOutput(nameof(exit));

            index = ValueInput<int>(nameof(index), 0);
            speed = ValueInput<float>(nameof(speed), 1f);
            unscaledTime = ValueInput<bool>(nameof(unscaledTime), false);

            for (int i = 0; i < _outputCount; i++)
            {
                var ind = i;
                valueResults.Add(ValueOutput<float>($"{i}", _ => _values[ind]));
            }

            Succession(enter, exit);
        }

        void InitializeValues(int index)
        {
            _values = Enumerable.Repeat(0f, _outputCount).ToList();
            _values[index] = 1f;
        }

        float GetDelta(Flow flow)
        {
            return flow.GetValue<bool>(unscaledTime) ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        ControlOutput Enter(Flow flow)
        {
            var index = flow.GetValue<int>(this.index);
            if (_values == null) InitializeValues(index);

            var speed = flow.GetValue<float>(this.speed);
            var delta = GetDelta(flow);

            for (var i = 0; i < _outputCount; i++)
            {
                _values[i] = Mathf.Lerp(_values[i], i == index ? 1f : 0f, speed * delta);
            }

            return exit;
        }
    }
}
