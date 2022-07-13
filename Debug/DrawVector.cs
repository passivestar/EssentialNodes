using UnityEngine;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Debug")]
    [TypeIcon(typeof(UnityEngine.Debug))]
    public class DrawVector : Unit
    {
        [Inspectable]
        public bool useGameObjectPosition = true;

        [DoNotSerialize, PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize, PortLabelHidden]
        public ControlOutput exit;

        [DoNotSerialize, NullMeansSelf, PortLabelHidden]
        public ValueInput gameObject;

        [DoNotSerialize]
        public ValueInput pos;

        [DoNotSerialize]
        public ValueInput vector;

        [DoNotSerialize]
        public ValueInput color;

        [DoNotSerialize]
        public ValueInput duration;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Draw);
            exit = ControlOutput(nameof(exit));

            if (useGameObjectPosition)
            {
                gameObject = ValueInput<GameObject>(nameof(gameObject), null).NullMeansSelf();
            }
            else
            {
                pos = ValueInput<Vector3>(nameof(pos), Vector3.zero);
            }
            vector = ValueInput<Vector3>(nameof(vector), Vector3.zero);
            color = ValueInput<Color>(nameof(color), Color.white);
            duration = ValueInput<float>(nameof(duration), .1f);

            Succession(enter, exit);
        }

        ControlOutput Draw(Flow flow)
        {
            var position = useGameObjectPosition
                ? flow.GetValue<GameObject>(gameObject).transform.position
                : flow.GetValue<Vector3>(pos);

            Debug.DrawLine(
                position,
                position + flow.GetValue<Vector3>(vector),
                flow.GetValue<Color>(color),
                flow.GetValue<float>(duration)
            );

            return exit;
        }
    }
}