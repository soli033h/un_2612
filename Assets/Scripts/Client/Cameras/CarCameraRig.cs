using System.Collections.Generic;
using Racing.Client.Cameras.Effects;
using Racing.Vehicles.Dynamics;
using UnityEngine;

namespace Racing.Client.Cameras
{
    /// <summary>
    /// Follows the target car and blends the active camera effects.
    /// The follow itself has no positional lag, so the car stays framed at any speed; only heading and height are smoothed.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CarCameraRig : MonoBehaviour
    {
        [SerializeField] CarController target;

        [Header("Follow")]
        [SerializeField] float distance = 6f;
        [SerializeField] float distanceAtTopSpeed = 7.5f;
        [SerializeField] float height = 2f;
        [SerializeField] float lookHeight = 1f;
        [SerializeField] float lookAhead = 3f;
        [Tooltip("How much the camera follows the direction of travel instead of the car's nose.")]
        [SerializeField, Range(0f, 1f)] float velocityAlignment = 0.35f;
        [SerializeField] float headingSharpness = 6f;
        [SerializeField] float heightSharpness = 10f;
        [SerializeField] float groundClearance = 0.4f;
        [SerializeField] float baseFieldOfView = 60f;

        [Header("Effects")]
        [SerializeField] CameraShake shake = new CameraShake();
        [SerializeField] SpeedFovEffect speedFov = new SpeedFovEffect();
        [SerializeField] DriftTiltEffect driftTilt = new DriftTiltEffect();

        readonly List<ICameraEffect> effects = new List<ICameraEffect>();
        readonly RaycastHit[] groundHits = new RaycastHit[8];
        Camera targetCamera;
        Vector3 heading = Vector3.forward;
        float pivotHeight;

        public CarController Target => target;
        public CameraShake Shake => shake;

        void Awake()
        {
            targetCamera = GetComponent<Camera>();
            effects.Add(shake);
            effects.Add(speedFov);
            effects.Add(driftTilt);
        }

        void Start()
        {
            if (target != null)
                SetTarget(target);
        }

        void OnDestroy()
        {
            if (target != null)
                target.Collided -= OnTargetCollided;
        }

        public void SetTarget(CarController car)
        {
            if (target != null)
                target.Collided -= OnTargetCollided;

            target = car;
            if (target == null)
                return;

            target.Collided += OnTargetCollided;
            heading = Flatten(target.transform.forward, Vector3.forward);
            pivotHeight = target.transform.position.y + lookHeight;
        }

        public void AddEffect(ICameraEffect effect)
        {
            if (!effects.Contains(effect))
                effects.Add(effect);
        }

        public bool RemoveEffect(ICameraEffect effect) => effects.Remove(effect);

        void LateUpdate()
        {
            if (target == null || !target.IsInitialized)
                return;

            float deltaTime = Time.deltaTime;
            Transform car = target.transform;

            Vector3 desiredHeading = Flatten(car.forward, heading);
            if (target.Speed > 3f)
            {
                Vector3 travel = Flatten(target.Body.linearVelocity, desiredHeading);
                desiredHeading = Vector3.Slerp(desiredHeading, travel, velocityAlignment);
            }
            heading = Vector3.Slerp(heading, desiredHeading, Sharpen(headingSharpness, deltaTime)).normalized;
            pivotHeight = Mathf.Lerp(pivotHeight, car.position.y + lookHeight, Sharpen(heightSharpness, deltaTime));

            var pivot = new Vector3(car.position.x, pivotHeight, car.position.z);
            float followDistance = Mathf.Lerp(distance, distanceAtTopSpeed, target.SpeedRatio);
            Vector3 position = pivot - heading * followDistance + Vector3.up * (height - lookHeight);
            position.y = Mathf.Max(position.y, GroundHeight(position) + groundClearance);
            Quaternion rotation = Quaternion.LookRotation(pivot + heading * lookAhead - position, Vector3.up);

            var offset = new CameraEffectOffset();
            foreach (ICameraEffect effect in effects)
                offset += effect.Evaluate(target, deltaTime);

            transform.SetPositionAndRotation(position + rotation * offset.Position, rotation * Quaternion.Euler(offset.Rotation));
            targetCamera.fieldOfView = baseFieldOfView + offset.FieldOfView;
        }

        void OnTargetCollided(float impactSpeed) => shake.AddImpact(impactSpeed);

        float GroundHeight(Vector3 position)
        {
            float ground = float.MinValue;
            int hitCount = Physics.RaycastNonAlloc(position + Vector3.up * 50f, Vector3.down, groundHits, 100f, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hitCount; i++)
                if (groundHits[i].rigidbody != target.Body)
                    ground = Mathf.Max(ground, groundHits[i].point.y);
            return ground;
        }

        static float Sharpen(float sharpness, float deltaTime) => 1f - Mathf.Exp(-sharpness * deltaTime);

        static Vector3 Flatten(Vector3 direction, Vector3 fallback)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : fallback;
        }
    }
}
