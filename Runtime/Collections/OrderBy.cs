using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Collections")]
    [TypeIcon(typeof(Unity.VisualScripting.AotList))]
    public class OrderBy : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ValueInput inputList;

        [DoNotSerialize]
        public ValueInput key;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput outputList;

        [DoNotSerialize]
        public ValueOutput item;

        object _current;

        protected override void Definition()
        {
            inputList = ValueInput<IEnumerable>(nameof(inputList));
            key = ValueInput<IComparable>(nameof(key));
            outputList = ValueOutput<IEnumerable>(nameof(outputList), GetList);
            item = ValueOutput<object>(nameof(item), GetItem);

            Requirement(inputList, outputList);
            Requirement(key, outputList);
        }

        IEnumerable<object> GetList(Flow flow)
        {
            return flow.GetValue<IEnumerable>(inputList)
                .Cast<object>()
                .OrderBy(item =>
                {
                    _current = item;
                    return flow.GetValue<IComparable>(key);
                });
        }

        object GetItem(Flow flow) => _current;
    }
}