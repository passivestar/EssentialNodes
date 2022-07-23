using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Logic")]
    [TypeIcon(typeof(Unity.VisualScripting.NotEqual))]
    public class DetectChange : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        public ControlOutput changed;

        [DoNotSerialize]
        public ControlOutput unchanged;

        [DoNotSerialize, PortLabelHidden]
        public ValueInput valueIn;

        [DoNotSerialize, PortLabel("Changed")]
        public ValueOutput valueChanged;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput valueOut;

        bool _valueChanged;
        object _currentValue;
        object _previousValue;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), flow =>
            {
                _currentValue = flow.GetValue<object>(valueIn);
                _valueChanged = !_currentValue.Equals(_previousValue);
                _previousValue = _currentValue;
                return _valueChanged ? changed : unchanged;
            });
            changed = ControlOutput(nameof(changed));
            unchanged = ControlOutput(nameof(unchanged));
            valueIn = ValueInput<object>(nameof(valueIn));
            valueChanged = ValueOutput<bool>(nameof(valueChanged), _ => _valueChanged);
            valueOut = ValueOutput<object>(nameof(valueOut), _ => _currentValue);

            Succession(enter, changed);
            Succession(enter, unchanged);
            Requirement(valueIn, valueOut);
            Requirement(valueIn, valueChanged);
        }
    }
}