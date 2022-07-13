using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Math")]
    [TypeIcon(typeof(Unity.VisualScripting.And))]
    public class InRange : Unit
    {
        [UnitHeaderInspectable("Flow")]
        public bool control;

        [DoNotSerialize, PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        public ControlOutput @true;

        [DoNotSerialize]
        public ControlOutput @false;

        [DoNotSerialize, PortLabelHidden]
        public ValueInput valueIn;

        [DoNotSerialize]
        public ValueInput min;

        [DoNotSerialize]
        public ValueInput max;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput result;

        bool _resultValue;

        protected override void Definition()
        {
            if (control)
            {
                enter = ControlInput(nameof(enter), flow =>
                {
                    return Operation(flow) ? @true : @false;
                });

                @true = ControlOutput(nameof(@true));
                @false = ControlOutput(nameof(@false));
            }

            valueIn = ValueInput<float>(nameof(valueIn));
            min = ValueInput<float>(nameof(min), 0);
            max = ValueInput<float>(nameof(max), 1);

            result = ValueOutput<bool>(
                nameof(result),
                control ? flow => _resultValue : Operation
            );

            Requirement(min, result);
            Requirement(max, result);
        }

        bool Operation(Flow flow)
        {
            var value = flow.GetValue<float>(valueIn);
            _resultValue = value >= flow.GetValue<float>(min)
                && value <= flow.GetValue<float>(max);
            return _resultValue;
        }
    }
}