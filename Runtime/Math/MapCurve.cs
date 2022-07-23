using UnityEngine;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Math")]
    [TypeIcon(typeof(Unity.VisualScripting.Lerp<>))]
    public class MapCurve : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ValueInput value;

        [DoNotSerialize, PortLabelHidden]
        public ValueInput curve;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput result;

        protected override void Definition()
        {
            value = ValueInput<float>(nameof(value));
            curve = ValueInput<AnimationCurve>(nameof(curve), AnimationCurve.EaseInOut(0, 0, 1, 1));
            result = ValueOutput<float>(nameof(result), Operation);

            Requirement(value, result);
            Requirement(curve, result);
        }

        float Operation(Flow flow)
        {
            var value = flow.GetValue<float>(this.value);
            var curve = flow.GetValue<AnimationCurve>(this.curve);
            return curve.Evaluate(value);
        }
    }
}