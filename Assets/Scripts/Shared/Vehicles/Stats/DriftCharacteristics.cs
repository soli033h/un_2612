namespace Racing.Vehicles.Stats
{
    /// <summary>
    /// Drift entry, slide angle control and speed retention while drifting. All values are 0..1.
    /// </summary>
    public sealed class DriftCharacteristics
    {
        public DriftCharacteristics(float entry, float angleControl, float speedRetention)
        {
            Entry = entry;
            AngleControl = angleControl;
            SpeedRetention = speedRetention;
        }

        /// <summary>How easily the rear breaks loose. Higher means less rear grip while drifting.</summary>
        public float Entry { get; }

        /// <summary>How strongly steering input controls the slide angle.</summary>
        public float AngleControl { get; }

        /// <summary>How much speed the throttle keeps while sliding.</summary>
        public float SpeedRetention { get; }

        public override string ToString() =>
            $"drift(entry {Entry:0.00}, control {AngleControl:0.00}, retention {SpeedRetention:0.00})";
    }
}
