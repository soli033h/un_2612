using UnityEngine;

namespace Racing.Vehicles.Dynamics
{
    /// <summary>
    /// One WheelCollider plus the visual bones it drives (one set per LOD).
    /// Spin bones follow the wheel's full pose; steer bones (brake calipers) follow suspension and steering only.
    /// </summary>
    public sealed class Wheel
    {
        readonly Transform[] spinBones;
        readonly Pose[] spinOffsets;
        readonly Transform[] steerBones;
        readonly Pose[] steerOffsets;

        public Wheel(WheelCollider collider, bool isFront, bool isLeft, Vector3 restCenter, Transform[] spinBones, Transform[] steerBones)
        {
            Collider = collider;
            IsFront = isFront;
            IsLeft = isLeft;
            this.spinBones = spinBones;
            this.steerBones = steerBones;

            Quaternion inverse = Quaternion.Inverse(collider.transform.rotation);
            spinOffsets = CaptureOffsets(spinBones, restCenter, inverse);
            steerOffsets = CaptureOffsets(steerBones, restCenter, inverse);
        }

        public WheelCollider Collider { get; }
        public bool IsFront { get; }
        public bool IsLeft { get; }
        public bool IsGrounded => Collider.isGrounded;

        /// <summary>0 = fully compressed, 1 = fully extended or in the air.</summary>
        public float SuspensionTravel()
        {
            if (!Collider.GetGroundHit(out WheelHit hit))
                return 1f;

            float distance = -Collider.transform.InverseTransformPoint(hit.point).y - Collider.radius;
            return Mathf.Clamp01(distance / Collider.suspensionDistance);
        }

        public void SyncVisuals()
        {
            Collider.GetWorldPose(out Vector3 center, out Quaternion spin);
            Apply(spinBones, spinOffsets, center, spin);

            Quaternion steer = Collider.transform.rotation * Quaternion.Euler(0f, Collider.steerAngle, 0f);
            Apply(steerBones, steerOffsets, center, steer);
        }

        static Pose[] CaptureOffsets(Transform[] bones, Vector3 center, Quaternion inverse)
        {
            var offsets = new Pose[bones.Length];
            for (int i = 0; i < bones.Length; i++)
                offsets[i] = new Pose(inverse * (bones[i].position - center), inverse * bones[i].rotation);
            return offsets;
        }

        static void Apply(Transform[] bones, Pose[] offsets, Vector3 center, Quaternion rotation)
        {
            for (int i = 0; i < bones.Length; i++)
                bones[i].SetPositionAndRotation(center + rotation * offsets[i].position, rotation * offsets[i].rotation);
        }
    }
}
