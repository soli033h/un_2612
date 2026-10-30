using Racing.Vehicles.Stats;
using UnityEngine;

namespace Racing.Vehicles.Dynamics
{
    /// <summary>
    /// Drift detection and slide control.
    /// While drifting, rear grip drops, steering sets the yaw rate and the throttle helps keep speed.
    /// </summary>
    public sealed class DriftSystem
    {
        const float MinSpeed = 8f;
        const float EnterSlipAngle = 12f;
        const float ExitSlipAngle = 6f;
        const float ExitDelay = 0.35f;
        const float MaxSlipAngle = 55f;
        const float GripRecoveryRate = 3f;
        const float FullControlSpeed = 20f;

        readonly DriftCharacteristics characteristics;
        float exitTimer;
        float entrySpeed;

        public DriftSystem(DriftCharacteristics characteristics)
        {
            this.characteristics = characteristics;
        }

        public bool IsDrifting { get; private set; }

        /// <summary>Signed angle between the car's nose and its velocity, in degrees. Positive when sliding to the right of the nose.</summary>
        public float SlipAngle { get; private set; }

        /// <summary>Multiplier for the rear tyres' sideways stiffness.</summary>
        public float RearGripMultiplier { get; private set; } = 1f;

        public void Tick(Rigidbody body, float steer, float throttle, bool handbrake, bool grounded, float deltaTime)
        {
            Transform transform = body.transform;
            Vector3 up = transform.up;
            Vector3 velocity = Vector3.ProjectOnPlane(body.linearVelocity, up);
            float speed = velocity.magnitude;
            float forwardSpeed = Vector3.Dot(velocity, transform.forward);
            SlipAngle = forwardSpeed > 1f ? Vector3.SignedAngle(transform.forward, velocity, up) : 0f;

            UpdateDriftFlag(speed, forwardSpeed, steer, throttle, handbrake, grounded, deltaTime);

            float targetGrip = IsDrifting ? Mathf.Lerp(0.8f, 0.45f, characteristics.Entry) * (handbrake ? 0.8f : 1f) : 1f;
            RearGripMultiplier = Mathf.MoveTowards(RearGripMultiplier, targetGrip, GripRecoveryRate * deltaTime);

            if (!IsDrifting)
                return;

            ApplyAngleControl(body, up, steer, speed);
            ApplySpeedRetention(body, velocity, speed, throttle);
        }

        void UpdateDriftFlag(float speed, float forwardSpeed, float steer, float throttle, bool handbrake, bool grounded, float deltaTime)
        {
            if (!grounded || forwardSpeed < MinSpeed * 0.5f)
            {
                IsDrifting = false;
                return;
            }

            float absSlip = Mathf.Abs(SlipAngle);
            if (!IsDrifting)
            {
                bool handbrakeEntry = handbrake && speed > MinSpeed && Mathf.Abs(steer) > 0.2f;
                bool powerEntry = speed > MinSpeed && throttle > 0.5f
                    && absSlip > EnterSlipAngle * Mathf.Lerp(1.3f, 0.7f, characteristics.Entry);
                if (handbrakeEntry || powerEntry)
                {
                    IsDrifting = true;
                    exitTimer = 0f;
                    entrySpeed = speed;
                }
                return;
            }

            exitTimer = absSlip < ExitSlipAngle && !handbrake ? exitTimer + deltaTime : 0f;
            if (exitTimer > ExitDelay)
                IsDrifting = false;
        }

        void ApplyAngleControl(Rigidbody body, Vector3 up, float steer, float speed)
        {
            float control = characteristics.AngleControl;
            float targetYawRate = steer * Mathf.Lerp(1.2f, 2.2f, control) * Mathf.Clamp01(speed / FullControlSpeed);

            // Past the max angle, rotate the nose back towards the velocity so the car does not spin out.
            float overshoot = Mathf.Max(0f, Mathf.Abs(SlipAngle) - MaxSlipAngle);
            targetYawRate += Mathf.Sign(SlipAngle) * overshoot * 0.1f;

            float yawRate = Vector3.Dot(body.angularVelocity, up);
            body.AddTorque(up * ((targetYawRate - yawRate) * Mathf.Lerp(1.5f, 4f, control)), ForceMode.Acceleration);
        }

        void ApplySpeedRetention(Rigidbody body, Vector3 velocity, float speed, float throttle)
        {
            if (throttle < 0.1f || speed < 0.1f || speed >= entrySpeed)
                return;

            float push = Mathf.Lerp(1f, 6f, characteristics.SpeedRetention) * throttle;
            body.AddForce(velocity / speed * push, ForceMode.Acceleration);
        }
    }
}
