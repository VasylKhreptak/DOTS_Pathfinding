using UnityEngine;

namespace Infrastructure.Services.Input.Core
{
    public interface IUIInputActions
    {
        public IInputAction<Vector2> Navigate { get; }
        public IInputAction<bool> Submit { get; }
        public IInputAction<bool> Cancel { get; }
        public IInputAction<Vector2> Point { get; }
        public IInputAction<bool> Click { get; }
        public IInputAction<Vector2> ScrollWheel { get; }
        public IInputAction<bool> MiddleClick { get; }
        public IInputAction<bool> RightClick { get; }

        public void SetActive(bool active);
    }
}