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

        IEnumerable ReverseIList(IList list)
        {
            for (var i = list.Count - 1; i >= 0; --i)
                yield return list[i];
        }

        object GetResult(Flow flow)
        {
            var enumerable = flow.GetValue<IEnumerable>(inputList);
            // It's probably safe to assume that nobody will be starting iteration, then mutating the collection while the enumerator is still active
            // since this is for a visual scripting environment, so we can avoid copying the entire enumerable if we have an IList
            if (enumerable is IList list)
                return ReverseIList(list);
            return enumerable.Cast<object>().Reverse();
        }
    }
}
