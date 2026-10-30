namespace Racing.Core
{
    /// <summary>
    /// Seed-based PRNG (PCG32) that produces identical sequences on server and client.
    /// Never replace with System.Random or UnityEngine.Random: their sequences are not guaranteed across platforms.
    /// </summary>
    public sealed class DeterministicRandom
    {
        const ulong Multiplier = 6364136223846793005UL;
        const ulong Increment = 1442695040888963407UL;

        ulong state;

        public DeterministicRandom(uint seed)
        {
            NextUInt();
            state += seed;
            NextUInt();
        }

        public uint NextUInt()
        {
            ulong previous = state;
            state = unchecked(previous * Multiplier + Increment);
            uint xorShifted = (uint)(((previous >> 18) ^ previous) >> 27);
            int rotation = (int)(previous >> 59);
            return (xorShifted >> rotation) | (xorShifted << (-rotation & 31));
        }

        /// <summary>Uniform float in [0, 1).</summary>
        public float NextFloat() => (NextUInt() >> 8) * (1f / 16777216f);

        public float Range(float min, float max) => min + (max - min) * NextFloat();

        public int Range(int minInclusive, int maxExclusive) =>
            minInclusive + (int)(NextUInt() % (uint)(maxExclusive - minInclusive));
    }
}
