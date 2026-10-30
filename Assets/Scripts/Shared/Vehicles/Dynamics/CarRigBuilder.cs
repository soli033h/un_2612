using System;
using System.Collections.Generic;
using Racing.Vehicles.Stats;
using UnityEngine;

namespace Racing.Vehicles.Dynamics
{
    /// <summary>
    /// Turns a plain car model into a drivable rig at runtime: LODGroup, Rigidbody, body collider and WheelColliders.
    /// Expects wheel bones named BONE_WHEEL_FL/FR/RL/RR and, optionally, BONE_CALIPER_* bones.
    /// LOD children are detected by a "_LOD{n}" name suffix.
    /// </summary>
    public static class CarRigBuilder
    {
        const string WheelBonePrefix = "BONE_WHEEL_";
        const string CaliperBonePrefix = "BONE_CALIPER_";
        const string LodMarker = "_LOD";
        const float FallbackWheelRadius = 0.34f;
        static readonly string[] WheelIds = { "FL", "FR", "RL", "RR" };

        public static CarRig Build(GameObject root, CarStats stats, CarTuning tuning)
        {
            Transform rootTransform = root.transform;
            Transform[] lods = FindLods(rootTransform);
            Transform measureRoot = lods.Length > 0 ? lods[0] : rootTransform;

            FaceForward(rootTransform, measureRoot);
            if (lods.Length > 1)
                EnsureLodGroup(root, lods);

            Rigidbody body = ConfigureBody(root, stats);

            var centers = new Vector3[WheelIds.Length];
            var radii = new float[WheelIds.Length];
            for (int i = 0; i < WheelIds.Length; i++)
                MeasureWheel(rootTransform, measureRoot, WheelIds[i], out centers[i], out radii[i]);

            float lowestCenter = float.MaxValue;
            float wheelBottom = float.MaxValue;
            float averageCenter = 0f;
            float averageRadius = 0f;
            for (int i = 0; i < WheelIds.Length; i++)
            {
                lowestCenter = Mathf.Min(lowestCenter, centers[i].y);
                wheelBottom = Mathf.Min(wheelBottom, centers[i].y - radii[i]);
                averageCenter += centers[i].y / WheelIds.Length;
                averageRadius += radii[i] / WheelIds.Length;
            }

            AddBodyCollider(rootTransform, LocalBounds(rootTransform, measureRoot.GetComponentsInChildren<Renderer>(true)), lowestCenter - averageRadius * 0.4f);

            var wheels = new Wheel[WheelIds.Length];
            for (int i = 0; i < WheelIds.Length; i++)
                wheels[i] = CreateWheel(rootTransform, WheelIds[i], centers[i], radii[i], stats, tuning);
            wheels[0].Collider.ConfigureVehicleSubsteps(5f, 12, 15);

            // Set after the colliders exist so Unity does not recompute it. Low and centred keeps the car planted.
            float frontZ = (centers[0].z + centers[1].z) * 0.5f;
            float rearZ = (centers[2].z + centers[3].z) * 0.5f;
            body.centerOfMass = new Vector3(0f, averageCenter - averageRadius * 0.3f, (frontZ + rearZ) * 0.5f);

            return new CarRig(body, wheels, wheelBottom);
        }

        static Transform[] FindLods(Transform root)
        {
            var lods = new SortedList<int, Transform>();
            foreach (Transform child in root)
            {
                int marker = child.name.LastIndexOf(LodMarker, StringComparison.OrdinalIgnoreCase);
                if (marker >= 0
                    && int.TryParse(child.name.Substring(marker + LodMarker.Length), out int level)
                    && !lods.ContainsKey(level))
                    lods.Add(level, child);
            }

            var result = new Transform[lods.Count];
            lods.Values.CopyTo(result, 0);
            return result;
        }

        /// <summary>Physics code drives along +Z, so turn the model around if its front axle is behind the rear one.</summary>
        static void FaceForward(Transform root, Transform measureRoot)
        {
            Transform front = FindFirst(measureRoot, WheelBonePrefix + "FL");
            Transform rear = FindFirst(measureRoot, WheelBonePrefix + "RL");
            if (front == null || rear == null)
                return;
            if (root.InverseTransformPoint(front.position).z >= root.InverseTransformPoint(rear.position).z)
                return;

            var children = new List<Transform>();
            foreach (Transform child in root)
                children.Add(child);
            foreach (Transform child in children)
                child.RotateAround(root.position, root.up, 180f);
        }

        static void EnsureLodGroup(GameObject root, Transform[] lods)
        {
            if (root.TryGetComponent(out LODGroup _))
                return;

            var levels = new LOD[lods.Length];
            for (int i = 0; i < lods.Length; i++)
                levels[i] = new LOD(0.5f * Mathf.Pow(0.4f, i), lods[i].GetComponentsInChildren<Renderer>(true));

            LODGroup group = root.AddComponent<LODGroup>();
            group.SetLODs(levels);
            group.RecalculateBounds();
        }

        static Rigidbody ConfigureBody(GameObject root, CarStats stats)
        {
            if (!root.TryGetComponent(out Rigidbody body))
                body = root.AddComponent<Rigidbody>();

            body.mass = stats.Mass;
            body.linearDamping = 0.005f;
            body.angularDamping = 0.3f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            return body;
        }

        static void MeasureWheel(Transform root, Transform measureRoot, string id, out Vector3 center, out float radius)
        {
            Transform bone = FindFirst(measureRoot, WheelBonePrefix + id) ?? FindFirst(root, WheelBonePrefix + id);
            if (bone == null)
                throw new InvalidOperationException($"{root.name}: missing wheel bone {WheelBonePrefix}{id}.");

            Renderer[] renderers = bone.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                center = root.InverseTransformPoint(bone.position);
                radius = FallbackWheelRadius;
                return;
            }

            Bounds tyre = LocalBounds(root, renderers);
            center = tyre.center;
            radius = Mathf.Max(tyre.extents.y, tyre.extents.z);
        }

        static void AddBodyCollider(Transform root, Bounds bounds, float bottom)
        {
            var collider = new GameObject("Body Collider");
            collider.transform.SetParent(root, false);

            Vector3 min = new Vector3(bounds.min.x + 0.05f, Mathf.Max(bounds.min.y, bottom), bounds.min.z);
            Vector3 max = new Vector3(bounds.max.x - 0.05f, bounds.max.y, bounds.max.z);
            BoxCollider box = collider.AddComponent<BoxCollider>();
            box.center = (min + max) * 0.5f;
            box.size = max - min;
            box.sharedMaterial = new PhysicsMaterial("Car Body")
            {
                dynamicFriction = 0.2f,
                staticFriction = 0.2f,
                bounciness = 0.05f,
                frictionCombine = PhysicsMaterialCombine.Minimum
            };
        }

        static Wheel CreateWheel(Transform root, string id, Vector3 center, float radius, CarStats stats, CarTuning tuning)
        {
            var wheelObject = new GameObject("Wheel Collider " + id);
            wheelObject.transform.SetParent(root, false);
            // With targetPosition 0.5 the wheel rests halfway down its travel, which is where the model has it.
            wheelObject.transform.localPosition = center + Vector3.up * (tuning.suspensionDistance * 0.5f);

            WheelCollider collider = wheelObject.AddComponent<WheelCollider>();
            collider.radius = radius;
            collider.mass = tuning.wheelMass;
            collider.wheelDampingRate = 0.25f;
            collider.suspensionDistance = tuning.suspensionDistance;
            collider.forceAppPointDistance = radius * 0.5f;

            float cornerMass = stats.Mass / WheelIds.Length;
            float omega = 2f * Mathf.PI * tuning.suspensionFrequency;
            collider.suspensionSpring = new JointSpring
            {
                spring = cornerMass * omega * omega,
                damper = 2f * tuning.suspensionDampingRatio * cornerMass * omega,
                targetPosition = 0.5f
            };
            collider.forwardFriction = new WheelFrictionCurve
            {
                extremumSlip = 0.4f, extremumValue = 1f, asymptoteSlip = 0.8f, asymptoteValue = 0.5f, stiffness = stats.Grip
            };
            collider.sidewaysFriction = new WheelFrictionCurve
            {
                extremumSlip = 0.2f, extremumValue = 1f, asymptoteSlip = 0.5f, asymptoteValue = 0.75f, stiffness = stats.Grip
            };

            return new Wheel(
                collider,
                isFront: id[0] == 'F',
                isLeft: center.x < 0f,
                restCenter: root.TransformPoint(center),
                spinBones: FindAll(root, WheelBonePrefix + id),
                steerBones: FindAll(root, CaliperBonePrefix + id));
        }

        static Bounds LocalBounds(Transform space, Renderer[] renderers)
        {
            var result = new Bounds();
            bool any = false;
            foreach (Renderer renderer in renderers)
            {
                Bounds local = renderer.localBounds;
                Matrix4x4 toSpace = space.worldToLocalMatrix * renderer.localToWorldMatrix;
                for (int corner = 0; corner < 8; corner++)
                {
                    var sign = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f, (corner & 4) == 0 ? -1f : 1f);
                    Vector3 point = toSpace.MultiplyPoint3x4(local.center + Vector3.Scale(local.extents, sign));
                    if (any)
                        result.Encapsulate(point);
                    else
                        result = new Bounds(point, Vector3.zero);
                    any = true;
                }
            }
            return result;
        }

        static Transform FindFirst(Transform root, string name)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (string.Equals(t.name, name, StringComparison.OrdinalIgnoreCase))
                    return t;
            return null;
        }

        static Transform[] FindAll(Transform root, string name)
        {
            var matches = new List<Transform>();
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (string.Equals(t.name, name, StringComparison.OrdinalIgnoreCase))
                    matches.Add(t);
            return matches.ToArray();
        }
    }
}
