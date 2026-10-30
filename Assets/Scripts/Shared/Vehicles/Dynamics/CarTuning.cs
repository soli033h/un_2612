using System;
using UnityEngine;

namespace Racing.Vehicles.Dynamics
{
    /// <summary>
    /// Chassis and assist tuning shared by every car. Per-car differences come from CarStats.
    /// </summary>
    [Serializable]
    public sealed class CarTuning
    {
        [Header("Suspension")]
        public float suspensionDistance = 0.2f;
        [Tooltip("Natural frequency of each corner's spring, in Hz.")]
        [Range(0.5f, 3f)] public float suspensionFrequency = 1.6f;
        [Range(0.1f, 1f)] public float suspensionDampingRatio = 0.45f;
        public float wheelMass = 20f;
        [Tooltip("Anti-roll bar force per kg of car mass at full travel difference.")]
        public float antiRollPerKg = 5f;

        [Header("Drivetrain")]
        [Range(0f, 1f)] public float frontBrakeBias = 0.6f;
        public float handbrakeDeceleration = 14f;
        public float engineBrakeDeceleration = 0.8f;
        public float reverseSpeedLimit = 12f;
        [Tooltip("Rear forward slip above which traction control halves the drive torque.")]
        public float tractionSlipLimit = 0.35f;

        [Header("Aero and assists")]
        [Tooltip("Downforce at top speed as a fraction of the car's weight.")]
        [Range(0f, 1.5f)] public float downforceAtTopSpeed = 0.6f;
        [Tooltip("Sideways velocity removed per second while not drifting.")]
        public float lateralGripAssist = 1f;
        [Tooltip("Yaw damping applied when the player is not steering.")]
        public float yawStabilityAssist = 0.6f;
        [Tooltip("How hard the car rights itself in the air.")]
        public float airLeveling = 2.5f;

        [Header("Recovery")]
        [Tooltip("Gap between the tyres and the ground when the car is placed or reset.")]
        public float spawnClearance = 0.3f;
        [Tooltip("Seconds on its side or roof before the car resets itself.")]
        public float flippedResetDelay = 2.5f;
    }
}
