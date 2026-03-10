using Infrastructure.Services.Input.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Infrastructure.Services.Input.NewInputSystem
{
    public class UIInputActions : IUIInputActions
    {
        public UIInputActions(InputActions.UIActions uiActions)
        {
            Navigate = new FuncInputAction<Vector2>(() => uiActions.Navigate.ReadValue<Vector2>());
            Submit = new FuncInputAction<bool>(() => uiActions.Submit.WasPressedThisFrame());
            Cancel = new FuncInputAction<bool>(() => uiActions.Cancel.WasPressedThisFrame());
            Point = new FuncInputAction<Vector2>(() => uiActions.Point.ReadValue<Vector2>());
            Click = new FuncInputAction<bool>(() => uiActions.Click.WasPressedThisFrame());
            ScrollWheel = new FuncInputAction<Vector2>(() => uiActions.ScrollWheel.ReadValue<Vector2>());
            MiddleClick = new FuncInputAction<bool>(() => uiActions.MiddleClick.WasPressedThisFrame());
            RightClick = new FuncInputAction<bool>(() => uiActions.RightClick.WasPressedThisFrame());
        }

        public IInputAction<Vector2> Navigate { get; }
        public IInputAction<bool> Submit { get; }
        public IInputAction<bool> Cancel { get; }
        public IInputAction<Vector2> Point { get; }
        public IInputAction<bool> Click { get; }
        public IInputAction<Vector2> ScrollWheel { get; }
        public IInputAction<bool> MiddleClick { get; }
        public IInputAction<bool> RightClick { get; }

        public void SetActive(bool active)
        {
            Navigate.Enabled = active;
            Submit.Enabled = active;
            Cancel.Enabled = active;
            Point.Enabled = active;
            Click.Enabled = active;
            ScrollWheel.Enabled = active;
            MiddleClick.Enabled = active;
            RightClick.Enabled = active;
        }
    }
}