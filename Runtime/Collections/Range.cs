using System.Collections;
using System.Linq;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Collections")]
    [TypeIcon(typeof(Unity.VisualScripting.AotList))]
    public class Range : Unit
    {
        [DoNotSerialize, PortLabel("Start")]
        public ValueInput start;

        [DoNotSerialize, PortLabel("Count")]
        public ValueInput count;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput list;

        protected override void Definition()
        {
            start = ValueInput<int>(nameof(start), 0);
            count = ValueInput<int>(nameof(count), 10);
            list = ValueOutput<IEnumerable>(nameof(list), GetRange);

            Requirement(start, list);
            Requirement(count, list);
        }

        IEnumerable GetRange(Flow flow)
        {
            return Enumerable.Range(
                flow.GetValue<int>(start),
                flow.GetValue<int>(count)
            );
        }
    }
}