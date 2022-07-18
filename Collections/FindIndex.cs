using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Collections")]
    [UnitShortTitle("Find"), UnitSubtitle("Index")]
    [TypeIcon(typeof(Unity.VisualScripting.AotList))]
    public class FindIndex : Unit
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
            var enumerable = flow.GetValue<IEnumerable>(inputList);
            var i = 0;
            foreach (var item in enumerable)
            {
                _current = item;
                if (flow.GetValue<bool>(condition))
                    return i;
                ++i;
            }
            return -1;
        }

        object GetItem(Flow flow) => _current;
    }
}
