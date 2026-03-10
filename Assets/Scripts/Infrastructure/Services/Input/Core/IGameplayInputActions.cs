using UnityEngine;

namespace Infrastructure.Services.Input.Core
{
    public interface IGameplayInputActions
    {
        public IInputAction<Vector2> Move { get; }
        public IInputAction<bool> Pause { get; }

        public void SetActive(bool active);
    }
}