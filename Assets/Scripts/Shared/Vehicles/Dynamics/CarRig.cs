using System.Collections.Generic;
using UnityEngine;

namespace Racing.Vehicles.Dynamics
{
    /// <summary>
    /// Physics parts of a built car. Wheels are ordered FL, FR, RL, RR.
    /// </summary>
    public sealed class CarRig
    {
        public CarRig(Rigidbody body, Wheel[] wheels, float wheelBottomHeight)
        {
            Body = body;
            Wheels = wheels;
            WheelBottomHeight = wheelBottomHeight;
        }

        public Rigidbody Body { get; }
        public IReadOnlyList<Wheel> Wheels { get; }

        /// <summary>Car-local height of the lowest tyre contact point at rest.</summary>
        public float WheelBottomHeight { get; }
    }
}
