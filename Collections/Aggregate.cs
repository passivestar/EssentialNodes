using System.Collections;
using System.Linq;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Collections")]
    [TypeIcon(typeof(Unity.VisualScripting.AotList))]
    public class Aggregate : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ValueInput inputList;

        [DoNotSerialize]
        public ValueInput initial;

        [DoNotSerialize]
        public ValueInput newAcc;

        [DoNotSerialize]
        public ValueOutput result;

        [DoNotSerialize]
        public ValueOutput acc;

        [DoNotSerialize]
        public ValueOutput item;

        object _acc;
        object _item;

        protected override void Definition()
        {
            inputList = ValueInput<IEnumerable>(nameof(inputList));
            initial = ValueInput<object>(nameof(initial));
            newAcc = ValueInput<object>(nameof(newAcc));
            result = ValueOutput<object>(nameof(result), GetResult);
            acc = ValueOutput<object>(nameof(acc), GetAccumulator);
            item = ValueOutput<object>(nameof(item), GetItem);

            Requirement(inputList, result);
            Requirement(initial, result);
            Requirement(newAcc, result);
        }

        object GetResult(Flow flow)
        {
            var initial = flow.GetValue<object>(this.initial);
            return flow.GetValue<IEnumerable>(inputList)
                .Cast<object>()
                .Aggregate(initial, (acc, item) =>
                {
                    _acc = acc;
                    _item = item;
                    return flow.GetValue<object>(newAcc);
                });
        }

        object GetAccumulator(Flow flow) => _acc;
        object GetItem(Flow flow) => _item;
    }
}