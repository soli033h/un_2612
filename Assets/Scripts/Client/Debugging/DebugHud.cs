using Racing.Client.Players;
using Racing.Vehicles.Dynamics;
using UnityEngine;

namespace Racing.Client.Debugging
{
    /// <summary>
    /// Minimal on-screen readout of the local car for testing builds.
    /// </summary>
    public sealed class DebugHud : MonoBehaviour
    {
        const string Controls = "W/S throttle, brake/reverse   A/D steer   Space handbrake   Shift boost   E reset";

        [SerializeField] Player player;

        GUIStyle speedStyle;
        GUIStyle textStyle;

        public void Bind(Player player) => this.player = player;

        void OnGUI()
        {
            if (player == null || player.Car == null || player.Vehicle == null || !player.Vehicle.IsInitialized)
                return;

            if (speedStyle == null)
            {
                speedStyle = new GUIStyle(GUI.skin.label) { fontSize = 40, fontStyle = FontStyle.Bold };
                textStyle = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            }

            CarController car = player.Vehicle;
            GUILayout.BeginArea(new Rect(20f, 20f, 700f, 400f));
            GUILayout.Label($"{Mathf.Abs(car.Speed) * 3.6f:0} km/h", speedStyle);
            GUILayout.Label($"State: {player.Car.State}", textStyle);
            GUILayout.Label($"Boost: {car.BoostGauge * 100f:0}%   Drift angle: {car.DriftAngle:0}°", textStyle);
            GUILayout.Label($"Seed: {player.Car.Seed}   {player.Car.Stats.Tire}   {player.Car.Stats.Mass:0} kg", textStyle);
            GUILayout.Label(Controls, textStyle);
            GUILayout.EndArea();
        }
    }
}
