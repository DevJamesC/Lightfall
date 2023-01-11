using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Game;
using Opsive.UltimateCharacterController.Traits;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

namespace MBS.Lightfall
{
    public class SpawnOnStart : MonoBehaviour
    {

        [SerializeField] private GameObject objectToSpawn;
        [SerializeField] private SpawnPoint spawnPoint;

        private void Start()
        {
            SpawnLocalPlayer();
        }

        private void SpawnLocalPlayer()
        {
            if (objectToSpawn == null)
            {
                Debug.LogWarning($"You have a spawner on start \"{name}\" which has no object to spawn assigned");
                return;
            }
            if (spawnPoint == null)
            {
                Debug.LogWarning($"You have a spawner on start \"{name}\" which has no spawnpoint assigned");
                return;
            }

            Vector3 position = spawnPoint.transform.position;
            Quaternion rotation = spawnPoint.transform.rotation;
            spawnPoint.GetPlacement(objectToSpawn, ref position, ref rotation);

            GameObject spawnedObject = Instantiate(objectToSpawn, position, rotation);

            CameraController cameraController = Camera.main.GetComponent<CameraController>();
            cameraController.Character = spawnedObject;
            cameraController.enabled = true;

            // Execute OnCharacterImmediateTransformChange after OnRespawn to ensure all of the interested components are using the new position/rotation.
            EventHandler.ExecuteEvent(spawnedObject, "OnCharacterImmediateTransformChange", true);
        }

    }
}
