namespace Racing.Vehicles
{
    /// <summary>
    /// Momentary physical state of a car, used to drive visual and audio feedback.
    /// </summary>
    public enum CarPhysicsState
    {
        Idle,
        Accelerating,
        Cruising,
        Braking,
        Reversing,
        Drifting,
        Boosting,
        Airborne,
        Colliding
    }
}
