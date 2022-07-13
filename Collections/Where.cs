using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Collections")]
    [TypeIcon(typeof(Unity.VisualScripting.AotList))]
    public class Where : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ValueInput inputList;

        [DoNotSerialize]
        public ValueInput condition;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput outputList;

        [DoNotSerialize]
        public ValueOutput item;

        object _current;

        protected override void Definition()
        {
            inputList = ValueInput<IEnumerable>(nameof(inputList));
            condition = ValueInput<bool>(nameof(condition));
            outputList = ValueOutput<IEnumerable>(nameof(outputList), GetList);
            item = ValueOutput<object>(nameof(item), GetItem);

            Requirement(inputList, outputList);
            Requirement(condition, outputList);
        }

        IEnumerable<object> GetList(Flow flow)
        {
            return flow.GetValue<IEnumerable>(inputList)
                .Cast<object>()
                .Where(item =>
                {
                    _current = item;
                    return flow.GetValue<bool>(condition);
                });
        }

        object GetItem(Flow flow) => _current;
    }
}