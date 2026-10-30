using Racing.Core;
using UnityEngine;

namespace Racing.Vehicles.Stats
{
    /// <summary>
    /// Distributes the point budget into a balanced CarStats.
    /// The order of random calls is part of the protocol: changing it changes every car for a given seed.
    /// </summary>
    public static class CarStatsGenerator
    {
        enum Rating { TopSpeed, Acceleration, Handling, Braking, Grip, Drift, Boost, Count }

        const int TransferStepsPerRating = 8;
        const float DriftJitter = 0.15f;
        const float CompoundDriftShift = 0.1f;
        const float MassDriftShift = 0.1f;

        public static CarStats Generate(DeterministicRandom random, CarStatRanges ranges)
        {
            float[] ratings = DistributeBudget(random, (int)Rating.Count, ranges.ratingBudget, ranges.minRating, ranges.maxRating);
            float massT = random.NextFloat();
            var tire = (TireCompound)random.Range(0, 3);

            // Soft tires grip more but slide less willingly; Hard is the opposite. Heavier cars are harder to unsettle.
            float compoundGrip = tire switch
            {
                TireCompound.Soft => 1f + ranges.compoundGripStep,
                TireCompound.Hard => 1f - ranges.compoundGripStep,
                _ => 1f
            };
            float entryShift = tire switch
            {
                TireCompound.Soft => -CompoundDriftShift,
                TireCompound.Hard => CompoundDriftShift,
                _ => 0f
            };
            entryShift -= (massT - 0.5f) * 2f * MassDriftShift;

            float handling = ratings[(int)Rating.Handling];
            float boost = ratings[(int)Rating.Boost];

            return new CarStats(
                topSpeed: Lerp(ranges.topSpeedKmh, ratings[(int)Rating.TopSpeed]) / 3.6f,
                acceleration: Lerp(ranges.accelerationMs2, ratings[(int)Rating.Acceleration]),
                maxSteerAngle: Lerp(ranges.maxSteerAngle, handling),
                steerSpeed: Lerp(ranges.steerSpeed, handling),
                highSpeedSteerFactor: Lerp(ranges.highSpeedSteerFactor, handling),
                brakeDeceleration: Lerp(ranges.brakeDecelerationMs2, ratings[(int)Rating.Braking]),
                grip: Lerp(ranges.gripStiffness, ratings[(int)Rating.Grip]) * compoundGrip,
                mass: Lerp(ranges.massKg, massT),
                tire: tire,
                drift: CreateDrift(random, ratings[(int)Rating.Drift], entryShift),
                boost: new BoostCharacteristics(
                    Lerp(ranges.boostAccelerationMs2, boost),
                    Lerp(ranges.boostDurationSeconds, boost),
                    Lerp(ranges.boostRechargePerSecond, boost)));
        }

        /// <summary>
        /// Starts every rating at the average and moves random amounts between pairs,
        /// so the sum always equals the budget and no rating leaves [min, max].
        /// </summary>
        static float[] DistributeBudget(DeterministicRandom random, int count, float budget, float min, float max)
        {
            var ratings = new float[count];
            float average = Mathf.Clamp(budget / count, min, max);
            for (int i = 0; i < count; i++)
                ratings[i] = average;

            for (int step = 0; step < count * TransferStepsPerRating; step++)
            {
                int from = random.Range(0, count);
                int to = random.Range(0, count);
                if (from == to)
                    continue;

                float room = Mathf.Min(ratings[from] - min, max - ratings[to]);
                float amount = room * random.NextFloat() * 0.5f;
                ratings[from] -= amount;
                ratings[to] += amount;
            }
            return ratings;
        }

        /// <summary>Jitters the three drift values around the rating while keeping their sum.</summary>
        static DriftCharacteristics CreateDrift(DeterministicRandom random, float rating, float entryShift)
        {
            float a = random.Range(-DriftJitter, DriftJitter);
            float b = random.Range(-DriftJitter, DriftJitter);
            return new DriftCharacteristics(
                Mathf.Clamp01(rating + a + entryShift),
                Mathf.Clamp01(rating + b),
                Mathf.Clamp01(rating - a - b));
        }

        static float Lerp(Vector2 range, float t) => Mathf.Lerp(range.x, range.y, t);
    }
}
