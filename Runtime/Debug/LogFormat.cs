using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Debug")]
    [TypeIcon(typeof(UnityEngine.Debug))]
    public class LogFormat : Unit
    {
        [UnitHeaderInspectable("Arguments")]
        public int argsCount
        {
            get => _argsCount;
            set => _argsCount = Mathf.Clamp(value, 0, 10);
        }

        [DoNotSerialize, PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize, PortLabelHidden]
        public ControlOutput exit;

        [DoNotSerialize, PortLabelHidden]
        public ValueInput message;

        [DoNotSerialize]
        public List<ValueInput> args = new();

        [SerializeAs(nameof(argsCount))]
        int _argsCount = 0;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), LogMessage);
            exit = ControlOutput(nameof(exit));
            message = ValueInput<string>(nameof(message), "");
            for (var i = 0; i < _argsCount; i++)
            {
                args.Add(ValueInput<object>($"{i}"));
            }

            Succession(enter, exit);
        }

        ControlOutput LogMessage(Flow flow)
        {
            var message = flow.GetValue<string>(this.message);
            var args = this.args.Select(arg => flow.GetValue<object>(arg)).ToArray();
            Debug.Log(string.Format(message, args));
            return exit;
        }
    }
}