using System;
using Racing.Vehicles.Dynamics;
using UnityEngine;

namespace Racing.Client.Cameras.Effects
{
    /// <summary>
    /// Shake from collisions (trauma that decays) plus a constant rumble from speed and boost.
    /// </summary>
    [Serializable]
    public sealed class CameraShake : ICameraEffect
    {
        [SerializeField] float maxPositionOffset = 0.25f;
        [SerializeField] float maxRotationOffset = 3f;
        [SerializeField] float frequency = 20f;
        [Tooltip("Trauma removed per second.")]
        [SerializeField] float traumaDecay = 1.5f;
        [Tooltip("Trauma added per m/s of impact speed.")]
        [SerializeField] float impactTrauma = 0.05f;
        [Tooltip("Shake at top speed while on the ground.")]
        [SerializeField] float speedRumble = 0.12f;
        [SerializeField] float boostRumble = 0.15f;

        float trauma;

        public void AddTrauma(float amount) => trauma = Mathf.Clamp01(trauma + amount);

        public void AddImpact(float impactSpeed) => AddTrauma(impactSpeed * impactTrauma);

        public CameraEffectOffset Evaluate(CarController car, float deltaTime)
        {
            trauma = Mathf.Max(0f, trauma - traumaDecay * deltaTime);

            float rumble = car.GroundedWheels > 0 ? car.SpeedRatio * car.SpeedRatio * speedRumble : 0f;
            if (car.IsBoosting)
                rumble += boostRumble;

            // Squaring trauma makes small hits subtle and big hits violent.
            float shake = Mathf.Clamp01(trauma * trauma + rumble);
            if (shake <= 0f)
                return default;

            float time = Time.time * frequency;
            return new CameraEffectOffset(
                new Vector3(Noise(time, 0), Noise(time, 1), 0f) * (maxPositionOffset * shake),
                new Vector3(Noise(time, 2), Noise(time, 3), Noise(time, 4)) * (maxRotationOffset * shake),
                0f);
        }

        static float Noise(float time, int channel) => Mathf.PerlinNoise(time, channel * 10.7f) * 2f - 1f;
    }
}
