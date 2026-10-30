using Racing.Client.Cameras;
using Racing.Client.Inputs;
using Racing.Client.Vehicles;
using Racing.Inputs;
using Racing.Vehicles;
using Racing.Vehicles.Dynamics;
using Racing.Vehicles.Stats;
using UnityEngine;

namespace Racing.Client.Players
{
    /// <summary>
    /// Local player: owns the controlled Car, the CarCameraRig and the input source.
    /// </summary>
    public sealed class Player : MonoBehaviour
    {
        [SerializeField] CarController vehicle;
        [SerializeField] CarCameraRig cameraRig;
        [Tooltip("0 rolls a random seed. In multiplayer the server provides it.")]
        [SerializeField] uint carSeed;
        [Tooltip("Optional. Uses the built-in defaults when empty.")]
        [SerializeField] CarStatRanges statRanges;

        PlayerCarInput playerInput;

        public Car Car { get; private set; }
        public CarController Vehicle => vehicle;
        public CarCameraRig CameraRig => cameraRig;
        public ICarInput CarInput => playerInput;

        /// <summary>Assigns the vehicle and camera when the player is created from code. Call before Start.</summary>
        public void Bind(CarController vehicle, CarCameraRig cameraRig)
        {
            this.vehicle = vehicle;
            this.cameraRig = cameraRig;
        }

        void Start()
        {
            if (vehicle == null)
            {
                Debug.LogError($"{name}: no vehicle assigned.", this);
                enabled = false;
                return;
            }

            uint seed = carSeed != 0 ? carSeed : (uint)Random.Range(1, int.MaxValue);
            Car = new Car(seed, statRanges);
            playerInput = new PlayerCarInput();
            vehicle.Initialize(Car, playerInput);

            if (!vehicle.TryGetComponent(out CarPaint paint))
                paint = vehicle.gameObject.AddComponent<CarPaint>();
            paint.Apply(Car.PaintColor);

            if (cameraRig != null)
                cameraRig.SetTarget(vehicle);

            Debug.Log($"Car seed {seed}: {Car.Stats}", this);
        }

        void OnDestroy() => playerInput?.Dispose();
    }
}
