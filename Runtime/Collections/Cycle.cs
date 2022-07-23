using System.Collections;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Collections")]
    [TypeIcon(typeof(Unity.VisualScripting.AotList))]
    public class Cycle : Unit
    {
        [DoNotSerialize, PortLabelHidden]
        public ValueInput list;

        [DoNotSerialize]
        public ValueInput reverse;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput item;

        int _i = -1;

        protected override void Definition()
        {
            list = ValueInput<IEnumerable>(nameof(list));
            reverse = ValueInput<bool>(nameof(reverse), false);
            item = ValueOutput<object>(nameof(item), Next);
        }

        object Next(Flow flow)
        {
            var list = flow.GetValue<IEnumerable>(this.list).Cast<object>();
            var reverse = flow.GetValue<bool>(this.reverse);
            var count = list.Count();

            if (count > 0)
            {
                _i = (int)Mathf.Repeat(reverse ? _i - 1 : _i + 1, count);
                return list.ElementAt(_i);
            }

            return null;
        }
    }
}