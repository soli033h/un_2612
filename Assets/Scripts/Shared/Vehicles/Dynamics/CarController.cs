using System;
using Racing.Inputs;
using Racing.Vehicles.Stats;
using UnityEngine;

namespace Racing.Vehicles.Dynamics
{
    /// <summary>
    /// Drives the Rigidbody and wheels from ICarInput and Car.Stats. Rear-wheel drive.
    /// Runs the same way on the server and on clients; call Initialize once before the first physics step.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CarController : MonoBehaviour
    {
        const int DrivenWheelCount = 2;
        const float ImpactThreshold = 2f;
        const float GroundProbeHeight = 500f;

        [SerializeField] CarTuning tuning = new CarTuning();

        readonly RaycastHit[] groundHits = new RaycastHit[8];
        CarRig rig;
        DriftSystem drift;
        BoostSystem boost;
        CarStateEvaluator stateEvaluator;
        float steerAngle;
        float flippedTimer;
        bool recoverHeld;

        /// <summary>Raised with the impact speed (m/s) along the contact normal.</summary>
        public event Action<float> Collided;
        public event Action<CarPhysicsState, CarPhysicsState> StateChanged;

        public Car Car { get; private set; }
        public ICarInput CarInput { get; private set; }
        public bool IsInitialized => rig != null;
        public Rigidbody Body => rig?.Body;

        /// <summary>Signed forward speed in m/s.</summary>
        public float Speed { get; private set; }
        public float SpeedRatio => Car == null ? 0f : Mathf.Clamp01(Mathf.Abs(Speed) / Car.Stats.TopSpeed);
        public float SteerAngle => steerAngle;
        public int GroundedWheels { get; private set; }
        public bool IsDrifting => drift != null && drift.IsDrifting;
        public float DriftAngle => drift?.SlipAngle ?? 0f;
        public bool IsBoosting => boost != null && boost.IsBoosting;
        public float BoostGauge => boost?.Gauge ?? 0f;

        public void Initialize(Car car, ICarInput input)
        {
            if (rig != null)
                throw new InvalidOperationException($"{name} is already initialized.");

            Car = car ?? throw new ArgumentNullException(nameof(car));
            CarInput = input ?? throw new ArgumentNullException(nameof(input));
            rig = CarRigBuilder.Build(gameObject, car.Stats, tuning);
            drift = new DriftSystem(car.Stats.Drift);
            boost = new BoostSystem(car.Stats.Boost);
            stateEvaluator = new CarStateEvaluator();
            ResetToGround();
        }

        /// <summary>Puts the car upright on the ground below its current position, keeping its heading.</summary>
        public void ResetToGround()
        {
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.01f)
                forward = Vector3.ProjectOnPlane(-transform.up, Vector3.up);
            if (forward.sqrMagnitude < 0.01f)
                forward = Vector3.forward;

            PlaceOnGround(transform.position, Quaternion.LookRotation(forward.normalized, Vector3.up));
        }

        void FixedUpdate()
        {
            if (rig == null)
                return;

            float deltaTime = Time.fixedDeltaTime;
            CarStats stats = Car.Stats;
            Rigidbody body = rig.Body;

            Speed = Vector3.Dot(body.linearVelocity, transform.forward);
            GroundedWheels = CountGroundedWheels();
            if (HandleRecovery(deltaTime))
                return;

            float throttle = Mathf.Clamp01(CarInput.Throttle);
            float brake = Mathf.Clamp01(CarInput.Brake);
            float steer = Mathf.Clamp(CarInput.Steer, -1f, 1f);
            bool handbrake = CarInput.Handbrake;

            drift.Tick(body, steer, throttle, handbrake, GroundedWheels >= 3, deltaTime);
            ApplySteering(steer, stats, deltaTime);
            ApplyDriveAndBrakes(throttle, brake, handbrake, stats);
            ApplyFriction(stats);
            ApplyAntiRoll(rig.Wheels[0], rig.Wheels[1]);
            ApplyAntiRoll(rig.Wheels[2], rig.Wheels[3]);
            ApplyAssists(body, stats, steer);
            boost.Tick(body, CarInput.Boost, Speed, stats.TopSpeed, deltaTime);
            UpdateState(throttle, brake, handbrake, deltaTime);
        }

        void LateUpdate()
        {
            if (rig == null)
                return;

            foreach (Wheel wheel in rig.Wheels)
                wheel.SyncVisuals();
        }

        void OnCollisionEnter(Collision collision)
        {
            if (rig == null || collision.contactCount == 0)
                return;

            float impact = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, collision.GetContact(0).normal));
            if (impact < ImpactThreshold)
                return;

            stateEvaluator.NotifyCollision();
            Collided?.Invoke(impact);
        }

        int CountGroundedWheels()
        {
            int count = 0;
            foreach (Wheel wheel in rig.Wheels)
                if (wheel.IsGrounded)
                    count++;
            return count;
        }

        /// <summary>Returns true when the car was reset this step.</summary>
        bool HandleRecovery(float deltaTime)
        {
            bool recoverPressed = CarInput.Recover && !recoverHeld;
            recoverHeld = CarInput.Recover;

            bool flipped = Vector3.Dot(transform.up, Vector3.up) < 0.3f && Mathf.Abs(Speed) < 2f;
            flippedTimer = flipped ? flippedTimer + deltaTime : 0f;
            if (!recoverPressed && flippedTimer < tuning.flippedResetDelay)
                return false;

            flippedTimer = 0f;
            ResetToGround();
            return true;
        }

        void ApplySteering(float steer, CarStats stats, float deltaTime)
        {
            // Full lock stays available while drifting so the player can counter-steer.
            float maxAngle = stats.MaxSteerAngle;
            if (!drift.IsDrifting)
                maxAngle *= Mathf.Lerp(1f, stats.HighSpeedSteerFactor, SpeedRatio);

            steerAngle = Mathf.MoveTowards(steerAngle, steer * maxAngle, stats.SteerSpeed * deltaTime);
            foreach (Wheel wheel in rig.Wheels)
                if (wheel.IsFront)
                    wheel.Collider.steerAngle = steerAngle;
        }

        void ApplyDriveAndBrakes(float throttle, float brake, bool handbrake, CarStats stats)
        {
            float mass = rig.Body.mass;
            bool reversing = brake > 0.1f && throttle < 0.1f && Speed < 1.5f;
            float driveAcceleration;
            float brakeDeceleration = 0f;

            if (reversing)
            {
                driveAcceleration = Speed > -tuning.reverseSpeedLimit ? -brake * stats.Acceleration * 0.5f : 0f;
            }
            else
            {
                driveAcceleration = throttle * stats.Acceleration * TorqueCurve(Speed / stats.TopSpeed);
                brakeDeceleration = brake * stats.BrakeDeceleration;
                if (throttle < 0.05f && brake < 0.05f)
                    brakeDeceleration = tuning.engineBrakeDeceleration;
            }

            foreach (Wheel wheel in rig.Wheels)
            {
                WheelCollider collider = wheel.Collider;
                float radius = collider.radius;

                if (!wheel.IsFront)
                {
                    float torque = driveAcceleration * mass * radius / DrivenWheelCount;
                    if (!drift.IsDrifting && collider.GetGroundHit(out WheelHit hit) && Mathf.Abs(hit.forwardSlip) > tuning.tractionSlipLimit)
                        torque *= 0.5f;
                    collider.motorTorque = torque;
                }

                float bias = wheel.IsFront ? tuning.frontBrakeBias : 1f - tuning.frontBrakeBias;
                float brakeTorque = brakeDeceleration * mass * radius * bias * 0.5f;
                if (handbrake && !wheel.IsFront)
                    brakeTorque += tuning.handbrakeDeceleration * mass * radius * 0.5f;
                collider.brakeTorque = brakeTorque;
            }
        }

        /// <summary>Drive force falloff towards top speed. 1 at standstill, 0 at top speed.</summary>
        static float TorqueCurve(float speedRatio)
        {
            if (speedRatio <= 0f)
                return 1f;
            float squared = speedRatio * speedRatio;
            return Mathf.Clamp01(1f - squared * squared);
        }

        void ApplyFriction(CarStats stats)
        {
            foreach (Wheel wheel in rig.Wheels)
            {
                WheelCollider collider = wheel.Collider;

                WheelFrictionCurve forward = collider.forwardFriction;
                forward.stiffness = stats.Grip;
                collider.forwardFriction = forward;

                WheelFrictionCurve sideways = collider.sidewaysFriction;
                sideways.stiffness = stats.Grip * (wheel.IsFront ? 1f : drift.RearGripMultiplier);
                collider.sidewaysFriction = sideways;
            }
        }

        void ApplyAntiRoll(Wheel left, Wheel right)
        {
            Rigidbody body = rig.Body;
            float force = (left.SuspensionTravel() - right.SuspensionTravel()) * tuning.antiRollPerKg * body.mass;
            if (left.IsGrounded)
                body.AddForceAtPosition(left.Collider.transform.up * -force, left.Collider.transform.position);
            if (right.IsGrounded)
                body.AddForceAtPosition(right.Collider.transform.up * force, right.Collider.transform.position);
        }

        void ApplyAssists(Rigidbody body, CarStats stats, float steer)
        {
            Vector3 up = transform.up;
            if (GroundedWheels == 0)
            {
                Vector3 levelAxis = Vector3.Cross(up, Vector3.up);
                Vector3 tumble = Vector3.ProjectOnPlane(body.angularVelocity, Vector3.up);
                body.AddTorque(levelAxis * tuning.airLeveling - tumble, ForceMode.Acceleration);
                return;
            }

            float downforce = tuning.downforceAtTopSpeed * body.mass * Physics.gravity.magnitude / (stats.TopSpeed * stats.TopSpeed);
            body.AddForce(-up * (downforce * Speed * Speed));

            if (drift.IsDrifting)
                return;

            Vector3 right = transform.right;
            float lateralSpeed = Vector3.Dot(body.linearVelocity, right);
            body.AddForce(-right * (lateralSpeed * tuning.lateralGripAssist), ForceMode.Acceleration);

            float yawRate = Vector3.Dot(body.angularVelocity, up);
            body.AddTorque(-up * (yawRate * tuning.yawStabilityAssist * (1f - Mathf.Abs(steer))), ForceMode.Acceleration);
        }

        void UpdateState(float throttle, float brake, bool handbrake, float deltaTime)
        {
            CarPhysicsState next = stateEvaluator.Evaluate(
                GroundedWheels > 0, Speed, throttle, brake, handbrake, drift.IsDrifting, boost.IsBoosting, deltaTime);
            if (next == Car.State)
                return;

            CarPhysicsState previous = Car.State;
            Car.State = next;
            StateChanged?.Invoke(previous, next);
        }

        void PlaceOnGround(Vector3 position, Quaternion rotation)
        {
            Rigidbody body = rig.Body;
            float groundHeight = position.y;
            float closest = float.MaxValue;
            int hitCount = Physics.RaycastNonAlloc(
                position + Vector3.up * GroundProbeHeight, Vector3.down, groundHits, GroundProbeHeight * 2f, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hitCount; i++)
            {
                if (groundHits[i].rigidbody == body || groundHits[i].distance >= closest)
                    continue;
                closest = groundHits[i].distance;
                groundHeight = groundHits[i].point.y;
            }

            var target = new Vector3(position.x, groundHeight - rig.WheelBottomHeight + tuning.spawnClearance, position.z);
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = target;
            body.rotation = rotation;
            transform.SetPositionAndRotation(target, rotation);
            steerAngle = 0f;
        }
    }
}
