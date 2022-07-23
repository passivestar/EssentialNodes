using System;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Math")]
    [TypeIcon(typeof(Unity.VisualScripting.Comparison))]
    public class Compare : Unit
    {
        public enum OperationType
        {
            Greater,
            GreaterOrEqual,
            Less,
            LessOrEqual,
            Equal,
            NotEqual
        }

        [UnitHeaderInspectable]
        public OperationType type;

        [UnitHeaderInspectable("Flow")]
        public bool control;

        [DoNotSerialize, PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        public ControlOutput @true;

        [DoNotSerialize]
        public ControlOutput @false;

        [DoNotSerialize]
        public ValueInput a;

        [DoNotSerialize]
        public ValueInput b;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput result;

        bool _resultValue;

        protected override void Definition()
        {
            if (control)
            {
                enter = ControlInput(nameof(enter), flow =>
                {
                    return GetOperation(type)(flow) ? @true : @false;
                });

                @true = ControlOutput(nameof(@true));
                @false = ControlOutput(nameof(@false));
            }

            a = ValueInput<float>(nameof(a), 0);
            b = ValueInput<float>(nameof(b), 0);

            result = ValueOutput<bool>(nameof(result), GetOperation(type));

            Requirement(a, result);
            Requirement(b, result);
        }

        Func<Flow, bool> GetOperation(OperationType operationType)
        {
            return operationType switch
            {
                OperationType.Greater => OperationGreater,
                OperationType.GreaterOrEqual => OperationGreaterOrEqual,
                OperationType.Less => OperationLess,
                OperationType.LessOrEqual => OperationLessOrEqual,
                OperationType.Equal => OperationEqual,
                OperationType.NotEqual => OperationNotEqual,
                _ => OperationGreater,
            };
        }

        bool OperationGreater(Flow flow)
        {
            _resultValue = flow.GetValue<float>(a) > flow.GetValue<float>(b);
            return _resultValue;
        }

        bool OperationGreaterOrEqual(Flow flow)
        {
            _resultValue = flow.GetValue<float>(a) >= flow.GetValue<float>(b);
            return _resultValue;
        }

        bool OperationLess(Flow flow)
        {
            _resultValue = flow.GetValue<float>(a) < flow.GetValue<float>(b);
            return _resultValue;
        }

        bool OperationLessOrEqual(Flow flow)
        {
            _resultValue = flow.GetValue<float>(a) <= flow.GetValue<float>(b);
            return _resultValue;
        }

        bool OperationEqual(Flow flow)
        {
            _resultValue = flow.GetValue<float>(a) == flow.GetValue<float>(b);
            return _resultValue;
        }

        bool OperationNotEqual(Flow flow)
        {
            _resultValue = flow.GetValue<float>(a) != flow.GetValue<float>(b);
            return _resultValue;
        }
    }
}
