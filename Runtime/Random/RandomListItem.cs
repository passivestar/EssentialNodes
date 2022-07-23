using System.Collections;
using System.Linq;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Random")]
    [UnitShortTitle("Random"), UnitSubtitle("List Item")]
    [TypeIcon(typeof(UnityEngine.Random))]
    public class RandomListItem : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ValueInput list;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput result;
        
        readonly System.Random _random = new();

        protected override void Definition()
        {
            list = ValueInput<IEnumerable>(nameof(list));
            result = ValueOutput<object>(nameof(result), Operation);

            Requirement(list, result);
        }

        object Operation(Flow flow)
        {
            var enumerable = flow.GetValue<IEnumerable>(list);
            var listValue = enumerable.ToIList();
            var index = _random.Next(0, listValue.Count);
            return listValue[index];
        }
    }
}
