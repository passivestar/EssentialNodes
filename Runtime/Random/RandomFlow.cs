using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Random")]
    [UnitShortTitle("Random"), UnitSubtitle("Flow")]
    [TypeIcon(typeof(UnityEngine.Random))]
    public class RandomFlow : Unit
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

        System.Random _random = new System.Random();

        [SerializeAs(nameof(branchesCount))]
        int _branchesCount = 2;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), _ =>
            {
                return branches[_random.Next(0, branches.Count - 1)];
            });

            for (var i = 0; i < branchesCount; i++)
            {
                var output = ControlOutput($"{i}");
                branches.Add(output);
                Succession(enter, output);
            }
        }
    }
}
