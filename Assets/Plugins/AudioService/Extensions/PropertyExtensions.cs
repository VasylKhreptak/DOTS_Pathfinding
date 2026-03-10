using DG.Tweening;
using Plugins.AudioService.Properties.Core;

namespace Plugins.AudioService.Extensions
{
    public static class PropertyExtensions
    {
        public static Tween TweenValue(this IProperty<float> property, int id, float targetValue, float duration)
        {
            float Getter() => property.GetValue(id);
            void Setter(float value) => property.SetValue(id, value);

            Tween tween = DOTween.To(Getter, Setter, targetValue, duration);

            tween.OnUpdate(() =>
            {
                if (property.IsAccessible(id) == false)
                    tween.Kill();
            });

            return tween;
        }
    }
}