using Infrastructure.Services.Input.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Infrastructure.Services.Input.NewInputSystem
{
    public class GameplayInputActions : IGameplayInputActions
    {
        public GameplayInputActions(InputActions.GameplayActions gameplayActions)
        {
            Move = new FuncInputAction<Vector2>(() => gameplayActions.Move.ReadValue<Vector2>());
            Pause = new FuncInputAction<bool>(() => gameplayActions.OpenPause.WasPressedThisFrame());
        }

        public IInputAction<Vector2> Move { get; }

        public IInputAction<bool> Pause { get; }

        public void SetActive(bool active)
        {
            Move.Enabled = active;
            Pause.Enabled = active;
        }
    }
}