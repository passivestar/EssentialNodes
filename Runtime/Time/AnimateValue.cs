using System;
using UnityEngine;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Time")]
    [TypeIcon(typeof(Unity.VisualScripting.WaitForSecondsUnit))]
    public class AnimateValue : Unit, IGraphElementWithData, IGraphEventListener
    {
        public enum ValueType
        {
            Float,
            Angle,
            Color,
            Vector2,
            Vector3,
            Vector4,
            Quaternion
        }

        public sealed class Data : IGraphElementData
        {
            public float elapsed;
            public float elapsedRatio;
            public float duration;
            public bool active;
            public bool unscaledTime;
            public bool loop;
            public bool isListening;
            public bool playingInReverse;
            public AnimationCurve curve;
            public Delegate update;
        }

        [Inspectable]
        public bool useCurve = false;

        [Inspectable]
        public bool useValues = false;

        [Inspectable]
        public bool showTimeMetrics = false;

        [Inspectable]
        public bool showToggles = false;

        [Inspectable]
        public bool showSetTime = false;

        [Inspectable]
        public ValueType type = ValueType.Float;

        [DoNotSerialize]
        public ControlInput play;

        [DoNotSerialize]
        public ControlInput playFromStart;

        [DoNotSerialize]
        public ControlInput stop;

        [DoNotSerialize]
        public ControlInput reverse;

        [DoNotSerialize]
        public ControlInput reverseFromEnd;

        [DoNotSerialize]
        public ControlInput togglePlayState;

        [DoNotSerialize]
        public ControlInput toggleDirection;

        [DoNotSerialize]
        public ControlInput setNewTime;

        [DoNotSerialize]
        public ControlOutput started;

        [DoNotSerialize]
        public ControlOutput startedInReverse;

        [DoNotSerialize]
        public ControlOutput tick;

        [DoNotSerialize]
        public ControlOutput finished;

        [DoNotSerialize]
        public ControlOutput finishedInReverse;

        [DoNotSerialize]
        public ValueInput newTime;

        [DoNotSerialize]
        public ValueInput duration;

        [DoNotSerialize]
        public ValueInput unscaledTime;

        [DoNotSerialize]
        public ValueInput loop;

        [DoNotSerialize, PortLabelHidden]
        public ValueInput curve;

        [DoNotSerialize]
        public ValueInput relativeValues;

        [DoNotSerialize]
        public ValueInput valueA;

        [DoNotSerialize]
        public ValueInput valueB;

        [DoNotSerialize, PortLabel("In Reverse")]
        public ValueOutput playingInReverse;

        [DoNotSerialize, PortLabel("Elapsed")]
        public ValueOutput elapsedSeconds;

        [DoNotSerialize, PortLabel("Elapsed %")]
        public ValueOutput elapsedRatio;

        [DoNotSerialize, PortLabel("Remaining")]
        public ValueOutput remainingSeconds;

        [DoNotSerialize, PortLabel("Remaining %")]
        public ValueOutput remainingRatio;

        [DoNotSerialize]
        public ValueOutput value;

        object _valueA;
        object _valueB;

        protected override void Definition()
        {
            isControlRoot = true;

            play = ControlInput(nameof(play), Play);
            playFromStart = ControlInput(nameof(playFromStart), PlayFromStart);
            stop = ControlInput(nameof(stop), Stop);
            reverse = ControlInput(nameof(reverse), Reverse);
            reverseFromEnd = ControlInput(nameof(reverseFromEnd), ReverseFromEnd);

            if (showToggles)
            {
                togglePlayState = ControlInput(nameof(togglePlayState), TogglePlayState);
                toggleDirection = ControlInput(nameof(toggleDirection), ToggleDirection);
            }

            if (showSetTime)
            {
                setNewTime = ControlInput(nameof(setNewTime), SetNewTime);
            }

            started = ControlOutput(nameof(started));
            startedInReverse = ControlOutput(nameof(startedInReverse));
            tick = ControlOutput(nameof(tick));
            finished = ControlOutput(nameof(finished));
            finishedInReverse = ControlOutput(nameof(finishedInReverse));

            if (showSetTime)
            {
                newTime = ValueInput<float>(nameof(newTime), 0f);
                Requirement(newTime, setNewTime);
            }

            duration = ValueInput<float>(nameof(duration), 1f);
            unscaledTime = ValueInput<bool>(nameof(unscaledTime), false);
            loop = ValueInput<bool>(nameof(loop), false);

            if (useCurve)
            {
                curve = ValueInput<AnimationCurve>(nameof(curve), AnimationCurve.EaseInOut(0, 0, 1, 1));
            }

            playingInReverse = ValueOutput<bool>(nameof(playingInReverse));

            if (showTimeMetrics)
            {
                elapsedSeconds = ValueOutput<float>(nameof(elapsedSeconds));
                elapsedRatio = ValueOutput<float>(nameof(elapsedRatio));
                remainingSeconds = ValueOutput<float>(nameof(remainingSeconds));
                remainingRatio = ValueOutput<float>(nameof(remainingRatio));
            }

            if (useValues)
            {
                relativeValues = ValueInput<bool>(nameof(relativeValues), true);
                switch (type)
                {
                    case ValueType.Float:
                        valueA = ValueInput<float>(nameof(valueA), 0f);
                        valueB = ValueInput<float>(nameof(valueB), 1f);
                        value = ValueOutput<float>(nameof(value));
                        break;
                    case ValueType.Angle:
                        valueA = ValueInput<float>(nameof(valueA), 0f);
                        valueB = ValueInput<float>(nameof(valueB), 180f);
                        value = ValueOutput<float>(nameof(value));
                        break;
                    case ValueType.Color:
                        valueA = ValueInput<Color>(nameof(valueA), Color.white);
                        valueB = ValueInput<Color>(nameof(valueB), Color.black);
                        value = ValueOutput<Color>(nameof(value));
                        break;
                    case ValueType.Vector2:
                        valueA = ValueInput<Vector2>(nameof(valueA), Vector2.zero);
                        valueB = ValueInput<Vector2>(nameof(valueB), Vector2.zero);
                        value = ValueOutput<Vector2>(nameof(value));
                        break;
                    case ValueType.Vector3:
                        valueA = ValueInput<Vector3>(nameof(valueA), Vector3.zero);
                        valueB = ValueInput<Vector3>(nameof(valueB), Vector3.zero);
                        value = ValueOutput<Vector3>(nameof(value));
                        break;
                    case ValueType.Vector4:
                        valueA = ValueInput<Vector3>(nameof(valueA), Vector4.zero);
                        valueB = ValueInput<Vector3>(nameof(valueB), Vector4.zero);
                        value = ValueOutput<Vector3>(nameof(value));
                        break;
                    case ValueType.Quaternion:
                        valueA = ValueInput<Quaternion>(nameof(valueA));
                        valueB = ValueInput<Quaternion>(nameof(valueB));
                        value = ValueOutput<Quaternion>(nameof(value));
                        break;
                }
            }

            Succession(play, started);
            Succession(play, finished);
            Succession(reverse, startedInReverse);
            Succession(reverse, finished);
        }

        public IGraphElementData CreateData()
        {
            return new Data();
        }

        public void StartListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);

            if (data.isListening)
            {
                return;
            }

            var reference = stack.ToReference();
            var hook = new EventHook(EventHooks.Update, stack.machine);
            Action<EmptyEventArgs> update = args => TriggerUpdate(reference);
            EventBus.Register(hook, update);
            data.update = update;
            data.isListening = true;
        }

        public void StopListening(GraphStack stack)
        {
            var data = stack.GetElementData<Data>(this);

            if (!data.isListening)
            {
                return;
            }

            var hook = new EventHook(EventHooks.Update, stack.machine);
            EventBus.Unregister(hook, data.update);
            data.update = null;
            data.isListening = false;
        }

        public bool IsListening(GraphPointer pointer)
        {
            return pointer.GetElementData<Data>(this).isListening;
        }

        void TriggerUpdate(GraphReference reference)
        {
            using var flow = Flow.New(reference);
            Update(flow);
        }

        void AssignOutputValues(Flow flow, Data data)
        {
            flow.SetValue(playingInReverse, data.playingInReverse);

            if (showTimeMetrics)
            {
                flow.SetValue(elapsedSeconds, data.elapsed);
                flow.SetValue(elapsedRatio, data.elapsedRatio);

                flow.SetValue(remainingSeconds, Mathf.Max(0, data.duration - data.elapsed));
                flow.SetValue(remainingRatio, Mathf.Clamp01((data.duration - data.elapsed) / data.duration));
            }

            if (useValues)
            {
                var t = GetInterpolationFactor(data);
                switch (type)
                {
                    case ValueType.Float:
                        flow.SetValue(value, Mathf.Lerp((float)_valueA, (float)_valueB, t));
                        break;
                    case ValueType.Angle:
                        flow.SetValue(value, Mathf.LerpAngle((float)_valueA, (float)_valueB, t));
                        break;
                    case ValueType.Color:
                        flow.SetValue(value, Color.Lerp((Color)_valueA, (Color)_valueB, t));
                        break;
                    case ValueType.Vector2:
                        flow.SetValue(value, Vector2.Lerp((Vector2)_valueA, (Vector2)_valueB, t));
                        break;
                    case ValueType.Vector3:
                        flow.SetValue(value, Vector3.Lerp((Vector3)_valueA, (Vector3)_valueB, t));
                        break;
                    case ValueType.Vector4:
                        flow.SetValue(value, Vector4.Lerp((Vector4)_valueA, (Vector4)_valueB, t));
                        break;
                    case ValueType.Quaternion:
                        flow.SetValue(value, Quaternion.Lerp((Quaternion)_valueA, (Quaternion)_valueB, t));
                        break;
                }
            }
        }

        (T from, T to, bool relative) GetInputValues<T>(Flow flow)
        {
            return (
                flow.GetValue<T>(this.valueA),
                flow.GetValue<T>(this.valueB),
                flow.GetValue<bool>(relativeValues)
            );
        }

        float GetInterpolationFactor(Data data)
        {
            return useCurve ? data.curve.Evaluate(data.elapsedRatio) : data.elapsedRatio;
        }

        void Update(Flow flow, bool force = false)
        {
            var data = flow.stack.GetElementData<Data>(this);

            if (!data.active && !force)
            {
                return;
            }

            var delta = data.unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (data.playingInReverse)
            {
                data.elapsed -= delta;
                data.elapsed = Mathf.Max(data.elapsed, 0);
            }
            else
            {
                data.elapsed += delta;
                data.elapsed = Mathf.Min(data.elapsed, data.duration);
            }

            data.elapsedRatio = Mathf.Clamp01(data.elapsed / data.duration);

            var stack = flow.PreserveStack();

            AssignOutputValues(flow, data);

            flow.Invoke(tick);

            if (data.playingInReverse && data.elapsed <= 0
             || !data.playingInReverse && data.elapsed >= data.duration)
            {
                if (data.loop)
                {
                    data.elapsed = data.playingInReverse ? data.duration : 0;
                }
                else
                {
                    data.active = false;
                }
                flow.RestoreStack(stack);
                flow.Invoke(data.playingInReverse ? finishedInReverse : finished);
            }

            flow.DisposePreservedStack(stack);
        }

        void StartPlaying(Flow flow, bool inReverse)
        {
            var data = flow.stack.GetElementData<Data>(this);

            data.duration = flow.GetValue<float>(duration);
            data.unscaledTime = flow.GetValue<bool>(unscaledTime);
            data.loop = flow.GetValue<bool>(loop);
            data.curve = flow.GetValue<AnimationCurve>(curve);
            data.playingInReverse = inReverse;
            data.active = true;

            if (_valueA == null)
            {
                switch (type)
                {
                    case ValueType.Float:
                    case ValueType.Angle:
                        {
                            var (from, to, relative) = GetInputValues<float>(flow);
                            _valueA = from;
                            _valueB = relative ? from + to : to;
                            break;
                        }
                    case ValueType.Color:
                        {
                            var (from, to, relative) = GetInputValues<Color>(flow);
                            _valueA = from;
                            _valueB = relative ? from + to : to;
                            break;
                        }
                    case ValueType.Vector2:
                        {
                            var (from, to, relative) = GetInputValues<Vector2>(flow);
                            _valueA = from;
                            _valueB = relative ? from + to : to;
                            break;
                        }
                    case ValueType.Vector3:
                        {
                            var (from, to, relative) = GetInputValues<Vector3>(flow);
                            _valueA = from;
                            _valueB = relative ? from + to : to;
                            break;
                        }
                    case ValueType.Vector4:
                        {
                            var (from, to, relative) = GetInputValues<Vector4>(flow);
                            _valueA = from;
                            _valueB = relative ? from + to : to;
                            break;
                        }
                    case ValueType.Quaternion:
                        {
                            var (from, to, relative) = GetInputValues<Quaternion>(flow);
                            _valueA = from;
                            _valueB = relative ? from * to : to;
                            break;
                        }
                }
            }

            AssignOutputValues(flow, data);
        }

        ControlOutput Play(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            if (data.active && data.playingInReverse)
            {
                data.playingInReverse = false;
            }
            else
            {
                StartPlaying(flow, false);
            }
            return started;
        }

        ControlOutput PlayFromStart(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            data.elapsed = 0;
            return Play(flow);
        }

        ControlOutput TogglePlayState(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);

            if (data.active)
            {
                return Stop(flow);
            }
            else if (data.playingInReverse)
            {
                return Reverse(flow);
            }
            else
            {
                return Play(flow);
            }
        }

        ControlOutput ToggleDirection(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            data.playingInReverse = !data.playingInReverse;
            return null;
        }

        ControlOutput Stop(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            data.active = false;
            return null;
        }

        ControlOutput Reverse(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            if (data.active && !data.playingInReverse)
            {
                data.playingInReverse = true;
            }
            else
            {
                StartPlaying(flow, true);
            }
            return startedInReverse;
        }

        ControlOutput ReverseFromEnd(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            data.elapsed = data.duration;
            return Reverse(flow);
        }

        ControlOutput SetNewTime(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            data.elapsed = flow.GetValue<float>(newTime);
            AssignOutputValues(flow, data);
            Update(flow, true);
            return null;
        }
    }
}
