using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Logic")]
    [TypeIcon(typeof(Unity.VisualScripting.ForEach))]
    public class CycleFlow : Unit
    {
        [UnitHeaderInspectable]
        public int branchesCount
        {
            get => _branchesCount;
            set => _branchesCount = Mathf.Clamp(value, 0, 10);
        }

        [DoNotSerialize, PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        public List<ControlOutput> branches = new();

        [DoNotSerialize]
        public ValueOutput index;

        [SerializeAs(nameof(branchesCount))]
        int _branchesCount = 2;

        int _i = -1;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), _ =>
            {
                _i = (_i + 1) % branches.Count;
                return branches[_i];
            });

            for (var i = 0; i < branchesCount; i++)
            {
                var output = ControlOutput($"{i}");
                branches.Add(output);
                Succession(enter, output);
            }

            index = ValueOutput<int>(nameof(index), _ => _i);
        }
    }
}