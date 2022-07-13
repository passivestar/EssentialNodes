using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

namespace EssentialNodes
{
    [UnitCategory("EssentialNodes/Random")]
    [TypeIcon(typeof(UnityEngine.Mathf))]
    public class Noise : Unit
    {
        public enum ValueType
        {
            Float,
            Color,
            Vector2,
            Vector3,
            Vector4,
            Quaternion
        }

        [UnitHeaderInspectable]
        public ValueType type = ValueType.Float;

        [Inspectable]
        public bool customTime = false;

        [DoNotSerialize]
        public ValueInput startingValue;

        [DoNotSerialize]
        public ValueInput time;

        [DoNotSerialize]
        public ValueInput amplitude;

        [DoNotSerialize]
        public ValueInput octaves;

        [DoNotSerialize]
        public ValueInput frequency;

        [DoNotSerialize]
        public ValueInput persistence;

        [DoNotSerialize, PortLabel("Unscaled")]
        public ValueInput unscaledTime;

        [DoNotSerialize, PortLabelHidden]
        public ValueOutput result;

        float? _startingValueFloat = null;
        Color? _startingValueColor = null;
        Vector2? _startingValueVector2 = null;
        Vector3? _startingValueVector3 = null;
        Vector4? _startingValueVector4 = null;
        Quaternion? _startingValueQuaternion = null;

        const float _componentOffset = 1111.1111f;

        int _numberOfOctaves = 1;
        List<float> _octaveOffsets = new();

        protected override void Definition()
        {
            switch (type)
            {
                case ValueType.Float:
                    startingValue = ValueInput<float>(nameof(startingValue), 0f);
                    result = ValueOutput<float>(nameof(result), GetNoiseFloat);
                    break;
                case ValueType.Color:
                    startingValue = ValueInput<Color>(nameof(startingValue), Color.white);
                    result = ValueOutput<Color>(nameof(result), GetNoiseColor);
                    break;
                case ValueType.Vector2:
                    startingValue = ValueInput<Vector2>(nameof(startingValue), Vector2.zero);
                    result = ValueOutput<Vector2>(nameof(result), GetNoiseVector2);
                    break;
                case ValueType.Vector3:
                    startingValue = ValueInput<Vector3>(nameof(startingValue), Vector3.zero);
                    result = ValueOutput<Vector3>(nameof(result), GetNoiseVector3);
                    break;
                case ValueType.Vector4:
                    startingValue = ValueInput<Vector4>(nameof(startingValue), Vector4.zero);
                    result = ValueOutput<Vector4>(nameof(result), GetNoiseVector4);
                    break;
                case ValueType.Quaternion:
                    startingValue = ValueInput<Quaternion>(nameof(startingValue), Quaternion.identity);
                    result = ValueOutput<Quaternion>(nameof(result), GetNoiseQuaternion);
                    break;
            }

            if (customTime)
            {
                time = ValueInput<float>(nameof(time), 0);
                Requirement(time, result);
            }

            amplitude = ValueInput<float>(nameof(amplitude), 1f);
            octaves = ValueInput<int>(nameof(octaves), 1);
            frequency = ValueInput<float>(nameof(frequency), 1f);
            persistence = ValueInput<float>(nameof(persistence), 1f);

            if (!customTime)
            {
                unscaledTime = ValueInput<bool>(nameof(unscaledTime), false);
                Requirement(unscaledTime, result);
            }

            Requirement(amplitude, result);
            Requirement(octaves, result);
            Requirement(frequency, result);
            Requirement(persistence, result);
        }

        void InitializeOctaveOffsets(Flow flow)
        {
            _numberOfOctaves = flow.GetValue<int>(octaves);
            for (int i = 0; i < _numberOfOctaves; i++)
            {
                _octaveOffsets.Add(UnityEngine.Random.Range(-100000f, 100000f));
            }
        }

        float GetNoise(float time, float amplitude, float frequency, float persistence, float offset = 0f)
        {
            float total = 0, amp = 1f, freq = 1f, maxValue = 0;
            for (int i = 0; i < _numberOfOctaves; i++)
            {
                var val = time * frequency + (_octaveOffsets[i] + offset) * frequency;
                var generatedValue = 1f - Mathf.PerlinNoise(val, val) * 2f;
                total += generatedValue * amp;
                amp *= persistence;
                freq *= frequency;
                maxValue += amp;
            }
            return total / maxValue * amplitude;
        }

        float GetTime(Flow flow)
        {
            if (customTime)
            {
                return flow.GetValue<float>(time);
            }
            else
            {
                return flow.GetValue<bool>(unscaledTime) ? Time.unscaledTime : Time.time;
            }
        }

        float GetNoiseFloat(Flow flow)
        {
            if (_startingValueFloat == null)
                _startingValueFloat = flow.GetValue<float>(startingValue);
            if (_octaveOffsets.Count == 0) InitializeOctaveOffsets(flow);
            return (float)_startingValueFloat + GetNoise(
                GetTime(flow),
                flow.GetValue<float>(amplitude),
                flow.GetValue<float>(frequency),
                flow.GetValue<float>(persistence)
            );
        }

        Color GetNoiseColor(Flow flow)
        {
            if (_startingValueColor == null)
                _startingValueColor = flow.GetValue<Color>(startingValue);
            if (_octaveOffsets.Count == 0) InitializeOctaveOffsets(flow);

            var time = GetTime(flow);
            var amplitude = flow.GetValue<float>(this.amplitude);
            var frequency = flow.GetValue<float>(this.frequency);
            var persistence = flow.GetValue<float>(this.persistence);

            return (Color)_startingValueColor + new Color(
                GetNoise(time, amplitude, frequency, persistence),
                GetNoise(time, amplitude, frequency, persistence, _componentOffset),
                GetNoise(time, amplitude, frequency, persistence, _componentOffset * 2)
            );
        }

        Vector2 GetNoiseVector2(Flow flow)
        {
            if (_startingValueVector2 == null)
                _startingValueVector2 = flow.GetValue<Vector2>(startingValue);
            if (_octaveOffsets.Count == 0) InitializeOctaveOffsets(flow);

            var time = GetTime(flow);
            var amplitude = flow.GetValue<float>(this.amplitude);
            var frequency = flow.GetValue<float>(this.frequency);
            var persistence = flow.GetValue<float>(this.persistence);

            return (Vector2)_startingValueVector2 + new Vector2(
                GetNoise(time, amplitude, frequency, persistence),
                GetNoise(time, amplitude, frequency, persistence, _componentOffset)
            );
        }

        Vector3 GetNoiseVector3(Flow flow)
        {
            if (_startingValueVector3 == null)
                _startingValueVector3 = flow.GetValue<Vector3>(startingValue);
            if (_octaveOffsets.Count == 0) InitializeOctaveOffsets(flow);

            var time = GetTime(flow);
            var amplitude = flow.GetValue<float>(this.amplitude);
            var frequency = flow.GetValue<float>(this.frequency);
            var persistence = flow.GetValue<float>(this.persistence);

            return (Vector3)_startingValueVector3 + new Vector3(
                GetNoise(time, amplitude, frequency, persistence),
                GetNoise(time, amplitude, frequency, persistence, _componentOffset),
                GetNoise(time, amplitude, frequency, persistence, _componentOffset * 2f)
            );
        }

        Vector4 GetNoiseVector4(Flow flow)
        {
            if (_startingValueVector4 == null)
                _startingValueVector4 = flow.GetValue<Vector4>(startingValue);
            if (_octaveOffsets.Count == 0) InitializeOctaveOffsets(flow);

            var time = GetTime(flow);
            var amplitude = flow.GetValue<float>(this.amplitude);
            var frequency = flow.GetValue<float>(this.frequency);
            var persistence = flow.GetValue<float>(this.persistence);

            return (Vector4)_startingValueVector4 + new Vector4(
                GetNoise(time, amplitude, frequency, persistence),
                GetNoise(time, amplitude, frequency, persistence, _componentOffset),
                GetNoise(time, amplitude, frequency, persistence, _componentOffset * 2f),
                GetNoise(time, amplitude, frequency, persistence, _componentOffset * 3f)
            );
        }

        Quaternion GetNoiseQuaternion(Flow flow)
        {
            if (_startingValueQuaternion == null)
                _startingValueQuaternion = flow.GetValue<Quaternion>(startingValue);
            if (_octaveOffsets.Count == 0) InitializeOctaveOffsets(flow);

            var time = GetTime(flow);
            var amplitude = flow.GetValue<float>(this.amplitude);
            var frequency = flow.GetValue<float>(this.frequency);
            var persistence = flow.GetValue<float>(this.persistence);

            return (Quaternion)_startingValueQuaternion * Quaternion.Euler(
                GetNoise(time, amplitude, frequency, persistence),
                GetNoise(time, amplitude, frequency, persistence, _componentOffset),
                GetNoise(time, amplitude, frequency, persistence, _componentOffset * 2)
            );
        }
    }
}