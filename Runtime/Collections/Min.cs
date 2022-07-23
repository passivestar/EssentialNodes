using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Collections")]
    [TypeIcon(typeof(Unity.VisualScripting.AotList))]
    public class Min : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ValueInput inputList;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput result;

        protected override void Definition()
        {
            inputList = ValueInput<IEnumerable>(nameof(inputList));
            result = ValueOutput<float>(nameof(result), GetResult);

            Requirement(inputList, result);
        }

        float GetResult(Flow flow)
        {
            return flow.GetValue<IEnumerable>(inputList)
                .Cast<object>()
                .Select(Convert.ToSingle)
                .Min();
        }
    }
}