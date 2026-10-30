using Racing.Core;
using Racing.Vehicles.Stats;
using UnityEngine;

namespace Racing.Vehicles
{
    /// <summary>
    /// Vehicle model. The constructor builds Stats and PaintColor from the server seed,
    /// so every peer that knows the seed ends up with the same car.
    /// </summary>
    public sealed class Car
    {
        public Car(uint seed, CarStatRanges ranges = null)
        {
            Seed = seed;
            var random = new DeterministicRandom(seed);
            Stats = CarStatsGenerator.Generate(random, ranges != null ? ranges : CarStatRanges.Default);
            PaintColor = Color.HSVToRGB(random.NextFloat(), random.Range(0.55f, 0.9f), random.Range(0.55f, 0.95f));
        }

        public uint Seed { get; }
        public CarStats Stats { get; }
        public Color PaintColor { get; }
        public CarPhysicsState State { get; internal set; } = CarPhysicsState.Idle;
    }
}
