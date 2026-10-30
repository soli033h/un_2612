using UnityEngine;

namespace Racing.Vehicles.Stats
{
    /// <summary>
    /// Point budget and min/max ranges used when rolling CarStats.
    /// Each Vector2 is (value at rating 0, value at rating 1).
    /// </summary>
    [CreateAssetMenu(menuName = "Racing/Car Stat Ranges")]
    public sealed class CarStatRanges : ScriptableObject
    {
        [Header("Budget")]
        [Tooltip("Sum of all seven ratings (each 0..1). Every car gets the same budget, which is what keeps them balanced.")]
        public float ratingBudget = 3.5f;
        [Range(0f, 0.5f)] public float minRating = 0.2f;
        [Range(0.5f, 1f)] public float maxRating = 0.8f;

        [Header("Rated values")]
        public Vector2 topSpeedKmh = new Vector2(200f, 270f);
        public Vector2 accelerationMs2 = new Vector2(5.5f, 9f);
        public Vector2 maxSteerAngle = new Vector2(26f, 36f);
        public Vector2 steerSpeed = new Vector2(120f, 220f);
        public Vector2 highSpeedSteerFactor = new Vector2(0.25f, 0.45f);
        public Vector2 brakeDecelerationMs2 = new Vector2(9f, 14f);
        public Vector2 gripStiffness = new Vector2(1.15f, 1.6f);
        public Vector2 boostAccelerationMs2 = new Vector2(3.5f, 7f);
        public Vector2 boostDurationSeconds = new Vector2(2f, 3.5f);
        public Vector2 boostRechargePerSecond = new Vector2(0.08f, 0.16f);

        [Header("Unrated values")]
        public Vector2 massKg = new Vector2(1300f, 1650f);
        [Tooltip("Grip gained by Soft and lost by Hard compounds.")]
        [Range(0f, 0.2f)] public float compoundGripStep = 0.08f;

        static CarStatRanges defaults;

        public static CarStatRanges Default
        {
            get
            {
                if (defaults == null)
                {
                    defaults = CreateInstance<CarStatRanges>();
                    defaults.hideFlags = HideFlags.DontSave;
                }
                return defaults;
            }
        }
    }
}
