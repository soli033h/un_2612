using System;
using Racing.Vehicles.Dynamics;
using UnityEngine;

namespace Racing.Client.Cameras.Effects
{
    /// <summary>
    /// Rolls the camera and shifts it sideways while drifting.
    /// </summary>
    [Serializable]
    public sealed class DriftTiltEffect : ICameraEffect
    {
        [Tooltip("Drift angle, in degrees, that produces the full effect.")]
        [SerializeField] float fullEffectAngle = 40f;
        [SerializeField] float maxRoll = 5f;
        [SerializeField] float lateralOffset = 0.8f;
        [SerializeField] float sharpness = 3f;

        float amount;

        public CameraEffectOffset Evaluate(CarController car, float deltaTime)
        {
            float target = car.IsDrifting ? Mathf.Clamp(car.DriftAngle / fullEffectAngle, -1f, 1f) : 0f;
            amount = Mathf.Lerp(amount, target, 1f - Mathf.Exp(-sharpness * deltaTime));
            return new CameraEffectOffset(new Vector3(amount * lateralOffset, 0f, 0f), new Vector3(0f, 0f, -amount * maxRoll), 0f);
        }
    }
}
