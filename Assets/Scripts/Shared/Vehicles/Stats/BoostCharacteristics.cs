namespace Racing.Vehicles.Stats
{
    /// <summary>
    /// Boost power, duration and recharge rate.
    /// </summary>
    public sealed class BoostCharacteristics
    {
        public BoostCharacteristics(float acceleration, float duration, float rechargeRate)
        {
            Acceleration = acceleration;
            Duration = duration;
            RechargeRate = rechargeRate;
        }

        /// <summary>Extra forward acceleration while boosting, in m/s².</summary>
        public float Acceleration { get; }

        /// <summary>Seconds a full gauge lasts.</summary>
        public float Duration { get; }

        /// <summary>Gauge refilled per second while not boosting (gauge is 0..1).</summary>
        public float RechargeRate { get; }

        public override string ToString() =>
            $"boost(+{Acceleration:0.0} m/s², {Duration:0.0} s, {RechargeRate:0.00}/s)";
    }
}
