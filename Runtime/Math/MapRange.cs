using UnityEngine;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Math")]
    [TypeIcon(typeof(Unity.VisualScripting.Lerp<>))]
    public class MapRange : Unit
    {
        [UnitHeaderInspectable("Use Curve")]
        public bool useCurve = false;

        [DoNotSerialize, PortLabelHidden]
        public ValueInput value;

        [DoNotSerialize, PortLabel("In Min")]
        public ValueInput valueMin;

        [DoNotSerialize, PortLabel("In Max")]
        public ValueInput valueMax;

        [DoNotSerialize]
        public ValueInput outMin;

        [DoNotSerialize]
        public ValueInput outMax;

        [DoNotSerialize, PortLabelHidden]
        public ValueInput curve;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput result;

        protected override void Definition()
        {
            value = ValueInput<float>(nameof(value));
            valueMin = ValueInput<float>(nameof(valueMin), 0f);
            valueMax = ValueInput<float>(nameof(valueMax), 1f);
            outMin = ValueInput<float>(nameof(outMin), 0f);
            outMax = ValueInput<float>(nameof(outMax), 1f);
            result = ValueOutput<float>(nameof(result), Operation);

            if (useCurve)
            {
                curve = ValueInput<AnimationCurve>(nameof(curve), AnimationCurve.EaseInOut(0, 0, 1, 1));
                Requirement(curve, result);
            }

            Requirement(value, result);
            Requirement(valueMin, result);
            Requirement(valueMax, result);
            Requirement(outMin, result);
            Requirement(outMax, result);
        }

        float Operation(Flow flow)
        {
            var t = Mathf.InverseLerp(
                flow.GetValue<float>(valueMin),
                flow.GetValue<float>(valueMax),
                flow.GetValue<float>(value)
            );
            if (useCurve)
            {
                t = flow.GetValue<AnimationCurve>(curve).Evaluate(t);
            }
            var min = flow.GetValue<float>(outMin);
            var max = flow.GetValue<float>(outMax);
            var val = Mathf.Lerp(min, max, t);
            return Mathf.Clamp(val, min, max);
        }
    }
}