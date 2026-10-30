using Racing.Vehicles.Stats;
using UnityEngine;

namespace Racing.Vehicles.Dynamics
{
    /// <summary>
    /// Boost gauge and activation. A boost starts only above a minimum gauge and runs until released or empty.
    /// </summary>
    public sealed class BoostSystem
    {
        const float MinActivationGauge = 0.15f;
        const float OverspeedFactor = 1.15f;

        readonly BoostCharacteristics characteristics;

        public BoostSystem(BoostCharacteristics characteristics)
        {
            this.characteristics = characteristics;
        }

        /// <summary>0..1</summary>
        public float Gauge { get; private set; } = 1f;
        public bool IsBoosting { get; private set; }

        public void Tick(Rigidbody body, bool requested, float forwardSpeed, float topSpeed, float deltaTime)
        {
            IsBoosting = requested && (IsBoosting ? Gauge > 0f : Gauge >= MinActivationGauge);
            if (!IsBoosting)
            {
                Gauge = Mathf.Min(1f, Gauge + characteristics.RechargeRate * deltaTime);
                return;
            }

            Gauge = Mathf.Max(0f, Gauge - deltaTime / characteristics.Duration);
            if (forwardSpeed < topSpeed * OverspeedFactor)
                body.AddForce(body.transform.forward * characteristics.Acceleration, ForceMode.Acceleration);
        }
    }
}
