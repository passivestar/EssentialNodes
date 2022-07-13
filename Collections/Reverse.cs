using System.Collections;
using System.Linq;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Collections")]
    [TypeIcon(typeof(Unity.VisualScripting.AotList))]
    public class Reverse : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ValueInput inputList;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput outputList;

        protected override void Definition()
        {
            inputList = ValueInput<IEnumerable>(nameof(inputList));
            outputList = ValueOutput<object>(nameof(outputList), GetResult);

            Requirement(inputList, outputList);
        }

        object GetResult(Flow flow)
        {
            return flow.GetValue<IEnumerable>(inputList).Cast<object>().Reverse();
        }
    }
}