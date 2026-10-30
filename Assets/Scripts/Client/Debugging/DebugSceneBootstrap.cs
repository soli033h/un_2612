using Racing.Client.Cameras;
using Racing.Client.Players;
using Racing.Vehicles.Dynamics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racing.Client.Debugging
{
    /// <summary>
    /// Until the network spawner exists: if a scene has no Player, turn the first car model in it
    /// into a locally driven car and attach the main camera to it. Does nothing once a Player is placed by hand.
    /// </summary>
    static class DebugSceneBootstrap
    {
        const string CarMarkerBone = "BONE_WHEEL_FL";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void SpawnLocalPlayer()
        {
            if (Object.FindAnyObjectByType<Player>() != null)
                return;

            GameObject car = FindCarModel(SceneManager.GetActiveScene());
            if (car == null)
                return;

            if (!car.TryGetComponent(out CarController vehicle))
                vehicle = car.AddComponent<CarController>();

            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = new GameObject("Main Camera").AddComponent<Camera>();
                camera.tag = "MainCamera";
            }
            if (!camera.TryGetComponent(out CarCameraRig cameraRig))
                cameraRig = camera.gameObject.AddComponent<CarCameraRig>();

            var playerObject = new GameObject("Local Player");
            Player player = playerObject.AddComponent<Player>();
            player.Bind(vehicle, cameraRig);
            playerObject.AddComponent<DebugHud>().Bind(player);
        }

        static GameObject FindCarModel(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    if (child.name == CarMarkerBone)
                        return root;
            return null;
        }
    }
}
