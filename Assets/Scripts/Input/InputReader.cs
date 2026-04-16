using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    public event Action OnInteract;
    public event Action OnPause;

    private GameInputActions input;

    private void Awake()
    {
        input = new GameInputActions();
    }

    private void OnEnable()
    {
        input.Enable();

        input.Gameplay.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        input.Gameplay.Move.canceled += ctx => MoveInput = Vector2.zero;

        input.Gameplay.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
        input.Gameplay.Look.canceled += ctx => LookInput = Vector2.zero;

        input.Gameplay.Interact.performed += ctx => OnInteract?.Invoke();
        input.Gameplay.Pause.performed += ctx => OnPause?.Invoke();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    public void SwitchToUI()
    {
        input.Gameplay.Disable();
        input.UI.Enable();
    }

    public void SwitchToGameplay()
    {
        input.UI.Disable();
        input.Gameplay.Enable();
    }
}