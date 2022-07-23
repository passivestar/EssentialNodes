using UnityEngine;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Time")]
    [TypeIcon(typeof(Unity.VisualScripting.Timer))]
    public class Throttle : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize, PortLabelHidden]
        public ControlOutput exit;

        [DoNotSerialize]
        public ValueInput interval;

        [DoNotSerialize, PortLabel("Unscaled")]
        public ValueInput unscaledTime;

        float _elapsed;
        float _lastInvokeTime;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Enter);
            exit = ControlOutput(nameof(exit));
            interval = ValueInput<float>(nameof(interval), 1f);
            unscaledTime = ValueInput<bool>(nameof(unscaledTime), false);

            Succession(enter, exit);
        }

        ControlOutput Enter(Flow flow)
        {
            var unscaled = flow.GetValue<bool>(unscaledTime);
            var delta = unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            _elapsed += delta;

            if (_elapsed - _lastInvokeTime < flow.GetValue<float>(interval))
            {
                return null;
            }

            _lastInvokeTime = _elapsed;

            return exit;
        }
    }
}