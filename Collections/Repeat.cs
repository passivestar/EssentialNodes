using System.Collections;
using System.Linq;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Collections")]
    [TypeIcon(typeof(Unity.VisualScripting.AotList))]
    public class Repeat : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ValueInput value;

        [DoNotSerialize]
        public ValueInput times;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput list;

        protected override void Definition()
        {
            value = ValueInput<object>(nameof(value));
            times = ValueInput<int>(nameof(times), 10);
            list = ValueOutput<IEnumerable>(nameof(list), GetList);

            Requirement(value, list);
            Requirement(times, list);
        }

        IEnumerable GetList(Flow flow)
        {
            return Enumerable.Repeat(
                flow.GetValue<object>(value),
                flow.GetValue<int>(times)
            );
        }
    }
}