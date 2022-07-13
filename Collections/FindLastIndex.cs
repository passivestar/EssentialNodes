using System.Collections.Generic;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Collections")]
    [UnitShortTitle("Find Last"), UnitSubtitle("Index")]
    [TypeIcon(typeof(Unity.VisualScripting.AotList))]
    public class FindLastIndex : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ValueInput inputList;

        [DoNotSerialize]
        public ValueInput condition;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput index;

        [DoNotSerialize]
        public ValueOutput item;

        object _current;

        protected override void Definition()
        {
            inputList = ValueInput<List<object>>(nameof(inputList));
            condition = ValueInput<bool>(nameof(condition));
            index = ValueOutput<int>(nameof(index), GetResult);
            item = ValueOutput<object>(nameof(item), GetItem);

            Requirement(inputList, index);
            Requirement(condition, index);
        }

        int GetResult(Flow flow)
        {
            return flow.GetValue<List<object>>(inputList)
                .FindLastIndex(item =>
                {
                    _current = item;
                    return flow.GetValue<bool>(condition);
                });
        }

        object GetItem(Flow flow) => _current;
    }
}