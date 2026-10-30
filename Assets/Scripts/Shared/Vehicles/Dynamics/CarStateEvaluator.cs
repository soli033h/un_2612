using UnityEngine;

namespace Racing.Vehicles.Dynamics
{
    /// <summary>
    /// Derives CarPhysicsState from the current physics values. Earlier checks win.
    /// </summary>
    public sealed class CarStateEvaluator
    {
        const float CollisionHoldTime = 0.35f;
        const float AirborneDelay = 0.2f;
        const float MovingSpeed = 1f;

        float collisionTimer;
        float airborneTimer;

        public void NotifyCollision() => collisionTimer = CollisionHoldTime;

        public CarPhysicsState Evaluate(
            bool grounded, float forwardSpeed, float throttle, float brake, bool handbrake, bool drifting, bool boosting, float deltaTime)
        {
            collisionTimer = Mathf.Max(0f, collisionTimer - deltaTime);
            airborneTimer = grounded ? 0f : airborneTimer + deltaTime;

            if (collisionTimer > 0f)
                return CarPhysicsState.Colliding;
            if (airborneTimer > AirborneDelay)
                return CarPhysicsState.Airborne;
            if (drifting)
                return CarPhysicsState.Drifting;
            if (boosting)
                return CarPhysicsState.Boosting;
            if (forwardSpeed < -MovingSpeed)
                return CarPhysicsState.Reversing;
            if ((brake > 0.1f || handbrake) && forwardSpeed > MovingSpeed)
                return CarPhysicsState.Braking;
            if (throttle > 0.1f)
                return CarPhysicsState.Accelerating;
            return Mathf.Abs(forwardSpeed) > MovingSpeed ? CarPhysicsState.Cruising : CarPhysicsState.Idle;
        }
    }
}
