using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Random")]
    [UnitShortTitle("Random"), UnitSubtitle("Bool")]
    [TypeIcon(typeof(UnityEngine.Random))]
    public class RandomBool : Unit
    {
        [DoNotSerialize]
        public ValueInput bias;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput result;

        protected override void Definition()
        {
            bias = ValueInput<float>(nameof(bias), .5f);
            result = ValueOutput<bool>(nameof(result), Operation);

            Requirement(bias, result);
        }

        bool Operation(Flow flow)
        {
            return UnityEngine.Random.value < flow.GetValue<float>(bias);
        }
    }
}