using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Infrastructure.Tools;
using UI.Animations.Selectables.Core;
using UI.Wrappers.Core;
using UnityEngine;

namespace UI.Animations.Selectables
{
    public class ScaleAnimation : BaseAnimation
    {
        [Header("References")]
        [SerializeField] private Transform _targetTransform;

        [Header("Preferences")]
        [SerializeField] private Dictionary<SelectableState, AnimationConfig> _animationConfigs;
        [SerializeField] private AnimationConfig _defaultAnimationConfig;

        private readonly AutoResetCancellationTokenSource _cts = new AutoResetCancellationTokenSource();

        #region MonoBehaviour

        protected override void OnValidate()
        {
            base.OnValidate();

            _targetTransform ??= transform;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _cts.Cancel();
        }

        #endregion

        protected override void OnStateInfoChanged(SelectableStateInfo stateInfo)
        {
            AnimationConfig animationConfig = GetAnimationConfig(stateInfo);

            _cts.Cancel();
            SetScale(animationConfig.Scale, stateInfo.Instant ? 0f : animationConfig.Duration, animationConfig.Ease, _cts.Token).Forget();
        }

        private AnimationConfig GetAnimationConfig(SelectableStateInfo stateInfo) => _animationConfigs.GetValueOrDefault(stateInfo.State, _defaultAnimationConfig);

        private UniTask SetScale(Vector3 scale, float duration, Ease ease, CancellationToken token) =>
            _targetTransform
                .DOScale(scale, duration)
                .SetEase(ease)
                .SetUpdate(true)
                .Play()
                .WithCancellation(token);

        [Serializable]
        private class AnimationConfig
        {
            [SerializeField] private Vector3 _scale = Vector3.one;
            [SerializeField] private Ease _ease = Ease.InCubic;
            [SerializeField] private float _duration = 0.15f;

            public Vector3 Scale => _scale;
            public Ease Ease => _ease;
            public float Duration => _duration;
        }
    }
}