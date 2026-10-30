using System;
using Racing.Vehicles.Dynamics;
using UnityEngine;

namespace Racing.Client.Cameras.Effects
{
    /// <summary>
    /// Widens the FOV with speed and boost, and pulls the camera back while boosting.
    /// </summary>
    [Serializable]
    public sealed class SpeedFovEffect : ICameraEffect
    {
        [SerializeField] float fovAtTopSpeed = 12f;
        [SerializeField] float boostFov = 8f;
        [Tooltip("Metres the camera moves back while boosting.")]
        [SerializeField] float boostPullBack = 0.6f;
        [SerializeField] float sharpness = 4f;

        float fov;
        float pullBack;

        public CameraEffectOffset Evaluate(CarController car, float deltaTime)
        {
            float blend = 1f - Mathf.Exp(-sharpness * deltaTime);
            float boost = car.IsBoosting ? 1f : 0f;
            fov = Mathf.Lerp(fov, car.SpeedRatio * car.SpeedRatio * fovAtTopSpeed + boost * boostFov, blend);
            pullBack = Mathf.Lerp(pullBack, boost * boostPullBack, blend);
            return new CameraEffectOffset(new Vector3(0f, 0f, -pullBack), Vector3.zero, fov);
        }
    }
}
