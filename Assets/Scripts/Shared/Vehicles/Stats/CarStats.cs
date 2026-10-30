namespace Racing.Vehicles.Stats
{
    /// <summary>
    /// Immutable performance values rolled once per car.
    /// Accelerations are in m/s² so they stay comparable regardless of the car's mass.
    /// </summary>
    public sealed class CarStats
    {
        public CarStats(
            float topSpeed,
            float acceleration,
            float maxSteerAngle,
            float steerSpeed,
            float highSpeedSteerFactor,
            float brakeDeceleration,
            float grip,
            float mass,
            TireCompound tire,
            DriftCharacteristics drift,
            BoostCharacteristics boost)
        {
            TopSpeed = topSpeed;
            Acceleration = acceleration;
            MaxSteerAngle = maxSteerAngle;
            SteerSpeed = steerSpeed;
            HighSpeedSteerFactor = highSpeedSteerFactor;
            BrakeDeceleration = brakeDeceleration;
            Grip = grip;
            Mass = mass;
            Tire = tire;
            Drift = drift;
            Boost = boost;
        }

        /// <summary>Top speed in m/s.</summary>
        public float TopSpeed { get; }

        /// <summary>Drive acceleration at low speed in m/s². Falls off towards top speed.</summary>
        public float Acceleration { get; }

        /// <summary>Front wheel steer angle at standstill, in degrees.</summary>
        public float MaxSteerAngle { get; }

        /// <summary>How fast the front wheels turn, in degrees per second.</summary>
        public float SteerSpeed { get; }

        /// <summary>Fraction of MaxSteerAngle left at top speed.</summary>
        public float HighSpeedSteerFactor { get; }

        /// <summary>Full-brake deceleration in m/s².</summary>
        public float BrakeDeceleration { get; }

        /// <summary>Tire friction stiffness multiplier, compound included.</summary>
        public float Grip { get; }

        /// <summary>Mass in kg.</summary>
        public float Mass { get; }

        public TireCompound Tire { get; }
        public DriftCharacteristics Drift { get; }
        public BoostCharacteristics Boost { get; }

        public override string ToString() =>
            $"top {TopSpeed * 3.6f:0} km/h, accel {Acceleration:0.0} m/s², steer {MaxSteerAngle:0}°, " +
            $"brake {BrakeDeceleration:0.0} m/s², grip {Grip:0.00}, {Mass:0} kg, {Tire}, {Drift}, {Boost}";
    }
}
