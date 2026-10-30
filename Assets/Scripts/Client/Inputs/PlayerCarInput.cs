using System;
using Racing.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racing.Client.Inputs
{
    /// <summary>
    /// Maps the InputSystem_Actions Player map to ICarInput.
    /// Move = throttle/brake + steer, Jump = handbrake, Sprint = boost, Interact = recover.
    /// </summary>
    public sealed class PlayerCarInput : ICarInput, IDisposable
    {
        const string MapName = "Player";

        readonly InputActionMap map;
        readonly InputAction move;
        readonly InputAction handbrake;
        readonly InputAction boost;
        readonly InputAction recover;

        /// <param name="actions">Defaults to the project-wide actions (Project Settings > Input System Package).</param>
        public PlayerCarInput(InputActionAsset actions = null)
        {
            if (actions == null)
                actions = InputSystem.actions;
            if (actions == null)
                throw new InvalidOperationException("No project-wide input actions are assigned.");

            map = actions.FindActionMap(MapName, throwIfNotFound: true);
            move = map.FindAction("Move", throwIfNotFound: true);
            handbrake = map.FindAction("Jump", throwIfNotFound: true);
            boost = map.FindAction("Sprint", throwIfNotFound: true);
            recover = map.FindAction("Interact", throwIfNotFound: true);
            map.Enable();
        }

        public float Throttle => Mathf.Max(0f, Drive.y);
        public float Brake => Mathf.Max(0f, -Drive.y);
        public float Steer => Drive.x;
        public bool Handbrake => handbrake.IsPressed();
        public bool Boost => boost.IsPressed();
        public bool Recover => recover.IsPressed();

        /// <summary>
        /// Move maps a circle (keyboard diagonals come in as 0.71, 0.71). Stretch it to a square
        /// so holding W and D still gives full throttle and full steer.
        /// </summary>
        Vector2 Drive
        {
            get
            {
                Vector2 value = move.ReadValue<Vector2>();
                float largest = Mathf.Max(Mathf.Abs(value.x), Mathf.Abs(value.y));
                if (largest < 0.001f)
                    return Vector2.zero;
                Vector2 square = value * (value.magnitude / largest);
                return new Vector2(Mathf.Clamp(square.x, -1f, 1f), Mathf.Clamp(square.y, -1f, 1f));
            }
        }

        public void Dispose() => map.Disable();
    }
}
