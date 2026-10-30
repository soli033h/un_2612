namespace Racing.Inputs
{
    /// <summary>
    /// Driving input source: local player, network or AI.
    /// </summary>
    public interface ICarInput
    {
        /// <summary>0..1</summary>
        float Throttle { get; }

        /// <summary>0..1. Reverses once the car has stopped.</summary>
        float Brake { get; }

        /// <summary>-1 (left) .. 1 (right)</summary>
        float Steer { get; }

        bool Handbrake { get; }
        bool Boost { get; }

        /// <summary>Held while the player asks to put the car back on its wheels.</summary>
        bool Recover { get; }
    }
}
