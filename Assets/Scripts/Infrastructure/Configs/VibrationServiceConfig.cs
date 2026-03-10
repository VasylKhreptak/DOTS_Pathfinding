using System;
using System.Collections.Generic;
using Infrastructure.Services.Vibration.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Infrastructure.Configs
{
    [CreateAssetMenu(fileName = "VibrationConfig", menuName = "Configs/VibrationConfig")]
    public class VibrationServiceConfig : SerializedScriptableObject
    {
        [SerializeField] private Dictionary<VibrationPreset, PresetConfig> _presets;

        public IReadOnlyDictionary<VibrationPreset, PresetConfig> Presets => _presets;

        [Serializable]
        public class PresetConfig
        {
            [SerializeField] private float _intensity = 1f;
            [SerializeField] private float _duration = 0.2f;

            public float Intensity => _intensity;
            public float Duration => _duration;
        }
    }
}