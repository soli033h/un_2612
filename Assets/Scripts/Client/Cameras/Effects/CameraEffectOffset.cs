using UnityEngine;

namespace Racing.Client.Cameras.Effects
{
    /// <summary>
    /// Position, rotation and FOV offset returned by an ICameraEffect. Offsets from all effects are summed.
    /// </summary>
    public struct CameraEffectOffset
    {
        /// <summary>Camera-local, in metres.</summary>
        public Vector3 Position;

        /// <summary>Camera-local Euler angles, in degrees.</summary>
        public Vector3 Rotation;

        /// <summary>Degrees added to the base field of view.</summary>
        public float FieldOfView;

        public CameraEffectOffset(Vector3 position, Vector3 rotation, float fieldOfView)
        {
            Position = position;
            Rotation = rotation;
            FieldOfView = fieldOfView;
        }

        public static CameraEffectOffset operator +(CameraEffectOffset a, CameraEffectOffset b) =>
            new CameraEffectOffset(a.Position + b.Position, a.Rotation + b.Rotation, a.FieldOfView + b.FieldOfView);
    }
}
