using UnityEngine;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Time")]
    [TypeIcon(typeof(Unity.VisualScripting.WaitForSecondsUnit))]
    public class Transition : Unit
    {
        public enum ValueType
        {
            Boolean,
            Float,
            Angle,
            Color,
            Vector2,
            Vector3,
            Vector4,
            Quaternion,
            Transform
        }

        public enum TransitionType
        {
            Lerp,
            SmoothDamp,
            MoveTowards
        }

        [UnitHeaderInspectable]
        public ValueType valueType = ValueType.Float;

        [UnitHeaderInspectable]
        public TransitionType transitionType = TransitionType.Lerp;

        [Inspectable]
        public bool allowReinitialization = false;

        [Inspectable]
        public bool transformPositionTransition = true;

        [Inspectable]
        public bool transformRotationTransition = true;

        [Inspectable]
        public bool transformScaleTransition = false;

        [DoNotSerialize, PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        public ControlInput reinitialize;

        [DoNotSerialize, NullMeansSelf]
        public ValueInput initialValue;

        [DoNotSerialize, PortLabel("New Value")]
        public ValueInput value;

        [DoNotSerialize]
        public ValueInput speed;

        [DoNotSerialize]
        public ValueInput smoothTime;

        [DoNotSerialize]
        public ValueInput maxSpeed;

        [DoNotSerialize, PortLabel("Unscaled")]
        public ValueInput unscaledTime;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput result;

        float _currentValueFloat;
        Color _currentValueColor;
        Vector2 _currentValueVector2;
        Vector3 _currentValueVector3;
        Vector4 _currentValueVector4;
        Quaternion _currentValueQuaternion;
        Transform _currentValueTransform;

        float _currentVelocityFloat;
        Vector2 _currentVelocityVector2;
        Vector3 _currentVelocityVector3;

        bool _initialized;

        protected override void Definition()
        {
            if (
                transitionType is TransitionType.SmoothDamp
                && valueType is ValueType.Color or ValueType.Vector4 or ValueType.Quaternion or ValueType.Transform
                || transitionType is TransitionType.MoveTowards
                && valueType is ValueType.Color
            )
            {
                Debug.LogWarning($"{transitionType} is not supported for {valueType}");
                return;
            }

            switch (valueType)
            {
                case ValueType.Boolean:
                    initialValue = ValueInput<float>(nameof(initialValue), 0f);
                    value = ValueInput<bool>(nameof(value));
                    result = ValueOutput<float>(
                        nameof(result),
                        transitionType switch
                        {
                            TransitionType.Lerp => TransitionBoolean,
                            TransitionType.SmoothDamp => TransitionBooleanSmoothDamp,
                            TransitionType.MoveTowards => TransitionBooleanMoveTowards,
                            _ => TransitionBoolean
                        }
                    );
                    break;
                case ValueType.Float:
                    initialValue = ValueInput<float>(nameof(initialValue), 0f);
                    value = ValueInput<float>(nameof(value));
                    result = ValueOutput<float>(
                        nameof(result),
                        transitionType switch
                        {
                            TransitionType.Lerp => TransitionFloat,
                            TransitionType.SmoothDamp => TransitionFloatSmoothDamp,
                            TransitionType.MoveTowards => TransitionFloatMoveTowards,
                            _ => TransitionFloat
                        }
                    );
                    break;
                case ValueType.Angle:
                    initialValue = ValueInput<float>(nameof(initialValue), 0f);
                    value = ValueInput<float>(nameof(value));
                    result = ValueOutput<float>(
                        nameof(result),
                        transitionType switch
                        {
                            TransitionType.Lerp => TransitionAngle,
                            TransitionType.SmoothDamp => TransitionAngleSmoothDamp,
                            TransitionType.MoveTowards => TransitionAngleMoveTowards,
                            _ => TransitionAngle
                        }
                    );
                    break;
                case ValueType.Color:
                    initialValue = ValueInput<Color>(nameof(initialValue), Color.white);
                    value = ValueInput<Color>(nameof(value));
                    result = ValueOutput<Color>(
                        nameof(result),
                        TransitionColor
                    );
                    break;
                case ValueType.Vector2:
                    initialValue = ValueInput<Vector2>(nameof(initialValue), Vector2.zero);
                    value = ValueInput<Vector2>(nameof(value));
                    result = ValueOutput<Vector2>(
                        nameof(result),
                        transitionType switch
                        {
                            TransitionType.Lerp => TransitionVector2,
                            TransitionType.SmoothDamp => TransitionVector2SmoothDamp,
                            TransitionType.MoveTowards => TransitionVector2MoveTowards,
                            _ => TransitionVector2
                        }
                    );
                    break;
                case ValueType.Vector3:
                    initialValue = ValueInput<Vector3>(nameof(initialValue), Vector3.zero);
                    value = ValueInput<Vector3>(nameof(value));
                    result = ValueOutput<Vector3>(
                        nameof(result),
                        transitionType switch
                        {
                            TransitionType.Lerp => TransitionVector3,
                            TransitionType.SmoothDamp => TransitionVector3SmoothDamp,
                            TransitionType.MoveTowards => TransitionVector3MoveTowards,
                            _ => TransitionVector3
                        }
                    );
                    break;
                case ValueType.Vector4:
                    initialValue = ValueInput<Vector4>(nameof(initialValue), Vector4.zero);
                    value = ValueInput<Vector4>(nameof(value));
                    result = ValueOutput<Vector4>(
                        nameof(result),
                        transitionType switch
                        {
                            TransitionType.Lerp => TransitionVector4,
                            TransitionType.MoveTowards => TransitionVector4MoveTowards,
                            _ => TransitionVector4
                        }
                    );
                    break;
                case ValueType.Quaternion:
                    initialValue = ValueInput<Quaternion>(nameof(initialValue));
                    value = ValueInput<Quaternion>(nameof(value));
                    result = ValueOutput<Quaternion>(
                        nameof(result),
                        transitionType switch
                        {
                            TransitionType.Lerp => TransitionQuaternion,
                            TransitionType.MoveTowards => TransitionQuaternionMoveTowards,
                            _ => TransitionQuaternion
                        }
                    );
                    break;
                case ValueType.Transform:
                    initialValue = ValueInput<Transform>(nameof(initialValue), null).NullMeansSelf();
                    value = ValueInput<Transform>(nameof(value), null);

                    // Transforms are transitioned in place
                    enter = ControlInput(
                        nameof(enter),
                        transitionType switch
                        {
                            TransitionType.Lerp => TransitionTransform,
                            TransitionType.MoveTowards => TransitionTransformMoveTowards,
                            _ => TransitionTransform
                        }
                    );
                    break;
            }

            if (transitionType == TransitionType.SmoothDamp)
            {
                smoothTime = ValueInput<float>(nameof(smoothTime), .5f);
                maxSpeed = ValueInput<float>(nameof(maxSpeed), 1000f);
            }
            else
            {
                speed = ValueInput<float>(nameof(speed), 1f);
            }
            unscaledTime = ValueInput<bool>(nameof(unscaledTime), false);

            if (allowReinitialization)
            {
                reinitialize = ControlInput(nameof(reinitialize), flow =>
                {
                    _initialized = false;
                    return null;
                });
            }
        }

        float GetDelta(Flow flow)
        {
            return flow.GetValue<bool>(unscaledTime) ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        void InitializeValue<T>(Flow flow, ref T current)
        {
            current = flow.GetValue<T>(initialValue);
            _initialized = true;
        }

        float TransitionBoolean(Flow flow)
        {
            if (!_initialized) InitializeValue<float>(flow, ref _currentValueFloat);
            return _currentValueFloat = Mathf.Lerp(
                _currentValueFloat,
                flow.GetValue<bool>(value) ? 1f : 0f,
                flow.GetValue<float>(speed) * GetDelta(flow)
            );
        }

        float TransitionBooleanSmoothDamp(Flow flow)
        {
            if (!_initialized) InitializeValue<float>(flow, ref _currentValueFloat);
            return _currentValueFloat = Mathf.SmoothDamp(
                _currentValueFloat,
                flow.GetValue<bool>(value) ? 1f : 0f,
                ref _currentVelocityFloat,
                flow.GetValue<float>(smoothTime),
                flow.GetValue<float>(maxSpeed),
                GetDelta(flow)
            );
        }

        float TransitionBooleanMoveTowards(Flow flow)
        {
            if (!_initialized) InitializeValue<float>(flow, ref _currentValueFloat);
            return _currentValueFloat = Mathf.MoveTowards(
                _currentValueFloat,
                flow.GetValue<bool>(value) ? 1f : 0f,
                flow.GetValue<float>(speed) * GetDelta(flow)
            );
        }

        float TransitionFloat(Flow flow)
        {
            if (!_initialized) InitializeValue<float>(flow, ref _currentValueFloat);
            return _currentValueFloat = Mathf.Lerp(
                _currentValueFloat,
                flow.GetValue<float>(value),
                flow.GetValue<float>(speed) * GetDelta(flow)
            );
        }

        float TransitionFloatSmoothDamp(Flow flow)
        {
            if (!_initialized) InitializeValue<float>(flow, ref _currentValueFloat);
            return _currentValueFloat = Mathf.SmoothDamp(
                _currentValueFloat,
                flow.GetValue<float>(value),
                ref _currentVelocityFloat,
                flow.GetValue<float>(smoothTime),
                flow.GetValue<float>(maxSpeed),
                GetDelta(flow)
            );
        }

        float TransitionFloatMoveTowards(Flow flow)
        {
            if (!_initialized) InitializeValue<float>(flow, ref _currentValueFloat);
            return _currentValueFloat = Mathf.MoveTowards(
                _currentValueFloat,
                flow.GetValue<float>(value),
                flow.GetValue<float>(speed) * GetDelta(flow)
            );
        }

        float TransitionAngle(Flow flow)
        {
            if (!_initialized) InitializeValue<float>(flow, ref _currentValueFloat);
            return _currentValueFloat = Mathf.LerpAngle(
                _currentValueFloat,
                flow.GetValue<float>(value),
                flow.GetValue<float>(speed) * GetDelta(flow)
            );
        }

        float TransitionAngleSmoothDamp(Flow flow)
        {
            if (!_initialized) InitializeValue<float>(flow, ref _currentValueFloat);
            return _currentValueFloat = Mathf.SmoothDampAngle(
                _currentValueFloat,
                flow.GetValue<float>(value),
                ref _currentVelocityFloat,
                flow.GetValue<float>(smoothTime),
                flow.GetValue<float>(maxSpeed),
                GetDelta(flow)
            );
        }

        float TransitionAngleMoveTowards(Flow flow)
        {
            if (!_initialized) InitializeValue<float>(flow, ref _currentValueFloat);
            return _currentValueFloat = Mathf.MoveTowardsAngle(
                _currentValueFloat,
                flow.GetValue<float>(value),
                flow.GetValue<float>(speed) * GetDelta(flow)
            );
        }

        Color TransitionColor(Flow flow)
        {
            if (!_initialized) InitializeValue<Color>(flow, ref _currentValueColor);
            return _currentValueColor = Color.Lerp(
                _currentValueColor,
                flow.GetValue<Color>(value),
                flow.GetValue<float>(speed) * GetDelta(flow)
            );
        }

        Vector2 TransitionVector2(Flow flow)
        {
            if (!_initialized) InitializeValue<Vector2>(flow, ref _currentValueVector2);
            return _currentValueVector2 = Vector2.Lerp(
                _currentValueVector2,
                flow.GetValue<Vector2>(value),
                flow.GetValue<float>(speed) * GetDelta(flow)
            );
        }

        Vector2 TransitionVector2SmoothDamp(Flow flow)
        {
            if (!_initialized) InitializeValue<Vector2>(flow, ref _currentValueVector2);
            return _currentValueVector2 = Vector2.SmoothDamp(
                _currentValueVector2,
                flow.GetValue<Vector2>(value),
                ref _currentVelocityVector2,
                flow.GetValue<float>(smoothTime),
                flow.GetValue<float>(maxSpeed),
                GetDelta(flow)
            );
        }

        Vector2 TransitionVector2MoveTowards(Flow flow)
        {
            if (!_initialized) InitializeValue<Vector2>(flow, ref _currentValueVector2);
            return _currentValueVector2 = Vector2.MoveTowards(
                _currentValueVector2,
                flow.GetValue<Vector2>(value),
                flow.GetValue<float>(speed) * GetDelta(flow)
            );
        }

        Vector3 TransitionVector3(Flow flow)
        {
            if (!_initialized) InitializeValue<Vector3>(flow, ref _currentValueVector3);
            return _currentValueVector3 = Vector3.Lerp(
                _currentValueVector3,
                flow.GetValue<Vector3>(value),
                flow.GetValue<float>(speed) * GetDelta(flow)
            );
        }

        Vector3 TransitionVector3SmoothDamp(Flow flow)
        {
            if (!_initialized) InitializeValue<Vector3>(flow, ref _currentValueVector3);
            return _currentValueVector3 = Vector3.SmoothDamp(
                _currentValueVector3,
                flow.GetValue<Vector3>(value),
                ref _currentVelocityVector3,
                flow.GetValue<float>(smoothTime),
                flow.GetValue<float>(maxSpeed),
                GetDelta(flow)
            );
        }

        Vector3 TransitionVector3MoveTowards(Flow flow)
        {
            if (!_initialized) InitializeValue<Vector3>(flow, ref _currentValueVector3);
            return _currentValueVector3 = Vector3.MoveTowards(
                _currentValueVector3,
                flow.GetValue<Vector3>(value),
                flow.GetValue<float>(speed) * GetDelta(flow)
            );
        }

        Vector4 TransitionVector4(Flow flow)
        {
            if (!_initialized) InitializeValue<Vector4>(flow, ref _currentValueVector4);
            return _currentValueVector4 = Vector4.Lerp(
                _currentValueVector4,
                flow.GetValue<Vector4>(value),
                flow.GetValue<float>(speed) * GetDelta(flow)
            );
        }

        Vector4 TransitionVector4MoveTowards(Flow flow)
        {
            if (!_initialized) InitializeValue<Vector4>(flow, ref _currentValueVector4);
            return _currentValueVector4 = Vector4.MoveTowards(
                _currentValueVector4,
                flow.GetValue<Vector4>(value),
                flow.GetValue<float>(speed) * GetDelta(flow)
            );
        }

        Quaternion TransitionQuaternion(Flow flow)
        {
            if (!_initialized) InitializeValue<Quaternion>(flow, ref _currentValueQuaternion);
            return _currentValueQuaternion = Quaternion.Slerp(
                _currentValueQuaternion,
                flow.GetValue<Quaternion>(value),
                flow.GetValue<float>(speed) * GetDelta(flow)
            );
        }

        Quaternion TransitionQuaternionMoveTowards(Flow flow)
        {
            if (!_initialized) InitializeValue<Quaternion>(flow, ref _currentValueQuaternion);
            return _currentValueQuaternion = Quaternion.RotateTowards(
                _currentValueQuaternion,
                flow.GetValue<Quaternion>(value),
                flow.GetValue<float>(speed) * Mathf.Rad2Deg * GetDelta(flow)
            );
        }

        ControlOutput TransitionTransform(Flow flow)
        {
            var target = flow.GetValue<Transform>(value);
            if (!_initialized) InitializeValue<Transform>(flow, ref _currentValueTransform);

            var t = flow.GetValue<float>(speed) * GetDelta(flow);

            if (transformPositionTransition)
            {
                _currentValueTransform.position = Vector3.Lerp(
                    _currentValueTransform.position,
                    target.position,
                    t
                );
            }

            if (transformRotationTransition)
            {
                _currentValueTransform.rotation = Quaternion.Slerp(
                    _currentValueTransform.rotation,
                    target.rotation,
                    t
                );                
            }

            if (transformScaleTransition)
            {
                _currentValueTransform.localScale = Vector3.Lerp(
                    _currentValueTransform.localScale,
                    target.localScale,
                    t
                );
            }

            return null;
        }

        ControlOutput TransitionTransformMoveTowards(Flow flow)
        {
            var target = flow.GetValue<Transform>(value);
            if (!_initialized) InitializeValue<Transform>(flow, ref _currentValueTransform);

            var t = flow.GetValue<float>(speed) * GetDelta(flow);

            if (transformPositionTransition)
            {
                _currentValueTransform.position = Vector3.MoveTowards(
                    _currentValueTransform.position,
                    target.position,
                    t
                );
            }

            if (transformRotationTransition)
            {
                _currentValueTransform.rotation = Quaternion.RotateTowards(
                    _currentValueTransform.rotation,
                    target.rotation,
                    t * Mathf.Rad2Deg
                );
            }

            if (transformScaleTransition)
            {
                _currentValueTransform.localScale = Vector3.MoveTowards(
                    _currentValueTransform.localScale,
                    target.localScale,
                    t
                );
            }

            return null;
        }
    }
}