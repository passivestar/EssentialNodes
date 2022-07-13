using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Collections")]
    [TypeIcon(typeof(Unity.VisualScripting.AotList))]
    public class Select : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ValueInput inputList;

        [DoNotSerialize]
        public ValueInput processedItem;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput outputList;

        [DoNotSerialize]
        public ValueOutput item;

        object _current;

        protected override void Definition()
        {
            inputList = ValueInput<IEnumerable>(nameof(inputList));
            processedItem = ValueInput<object>(nameof(processedItem));
            outputList = ValueOutput<IEnumerable>(nameof(outputList), GetList);
            item = ValueOutput<object>(nameof(item), GetItem);

            Requirement(inputList, outputList);
            Requirement(processedItem, outputList);
        }

        IEnumerable<object> GetList(Flow flow)
        {
            return flow.GetValue<IEnumerable>(inputList)
                .Cast<object>()
                .Select(item =>
                {
                    _current = item;
                    return flow.GetValue<object>(processedItem);
                });
        }

        object GetItem(Flow flow) => _current;
    }
}