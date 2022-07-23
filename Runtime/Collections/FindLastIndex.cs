// Specifies that we should avoid copying the elements of an enumerator into a temp collection
// and should instead evaluate the condition on all elements as they're iterated. When enabled,
// this mimics the behavior of Linq methods like Last
#define AVOID_COPY_OVER_EVALUATION

using System.Collections;
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

        private int FindLastIndexInIList(Flow flow, IList list)
        {
            for (var i = list.Count - 1; i >= 0; --i)
            {
                _current = list[i];
                if (flow.GetValue<bool>(condition))
                    return i;
            }
            return -1;
        }

#if AVOID_COPY_OVER_EVALUATION
        int GetResult(Flow flow)
        {
            var enumerable = flow.GetValue<IEnumerable>(inputList);
            if (enumerable is IList list)
                return FindLastIndexInIList(flow, list);

            var (lastFound, i) = (-1, 0);
            foreach (var item in enumerable)
            {
                _current = item;
                if (flow.GetValue<bool>(condition))
                    lastFound = i;
                ++i;
            }
            return lastFound;
        }
#else
        int GetResult(Flow flow) => FindLastIndexInIList(flow, flow.GetValue<IEnumerable>(inputList).ToIList());
#endif

        object GetItem(Flow flow) => _current;
    }
}
