using Racing.Vehicles.Dynamics;

namespace Racing.Client.Cameras.Effects
{
    /// <summary>
    /// A camera effect that contributes position, rotation and FOV offsets each frame.
    /// </summary>
    public interface ICameraEffect
    {
        CameraEffectOffset Evaluate(CarController car, float deltaTime);
    }
}
