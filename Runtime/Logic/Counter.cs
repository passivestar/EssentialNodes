using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Logic")]
    [TypeIcon(typeof(Unity.VisualScripting.Add<>))]
    public class Counter : Unit
    {
        [Inspectable]
        public bool hasTarget;

        [DoNotSerialize]
        public ControlInput increment;

        [DoNotSerialize]
        public ControlInput decrement;

        [DoNotSerialize, PortLabelHidden]
        public ControlOutput exit;

        [DoNotSerialize]
        public ControlOutput done;

        [DoNotSerialize, PortLabelHidden]
        public ValueInput add;

        [DoNotSerialize]
        public ValueInput target;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput result;

        int _currentValue;

        protected override void Definition()
        {
            increment = ControlInput(nameof(increment), flow =>
            {
                _currentValue += flow.GetValue<int>(add);
                if (hasTarget)
                {
                    CheckTarget(flow);
                }
                return exit;
            });

            decrement = ControlInput(nameof(decrement), flow =>
            {
                _currentValue -= flow.GetValue<int>(add);
                if (hasTarget)
                {
                    CheckTarget(flow);
                }
                return exit;
            });

            exit = ControlOutput(nameof(exit));
            add = ValueInput<int>(nameof(add), 1);
            if (hasTarget)
            {
                target = ValueInput<int>(nameof(target), 10);
                done = ControlOutput(nameof(done));
                Succession(increment, done);
                Succession(decrement, done);
            }
            result = ValueOutput<int>(nameof(result), _ => _currentValue);

            Succession(increment, exit);
            Succession(decrement, exit);
        }

        void CheckTarget(Flow flow)
        {
            if (_currentValue == flow.GetValue<int>(target))
            {
                flow.Invoke(done);
            }
        }
    }
}