using System.Collections;
using System.Linq;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Collections")]
    [TypeIcon(typeof(Unity.VisualScripting.AotList))]
    public class Take : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ValueInput inputList;

        [DoNotSerialize, PortLabelHidden]
        public ValueInput number;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput outputList;

        protected override void Definition()
        {
            inputList = ValueInput<IEnumerable>(nameof(inputList));
            number = ValueInput<int>(nameof(number), 1);
            outputList = ValueOutput<object>(nameof(outputList), GetResult);

            Requirement(inputList, outputList);
            Requirement(number, outputList);
        }

        object GetResult(Flow flow)
        {
            var n = flow.GetValue<int>(number);
            return flow.GetValue<IEnumerable>(inputList).Cast<object>().Take(n);
        }
    }
}