using Opsive.Shared.Events;
using Opsive.UltimateCharacterController;
using Opsive.UltimateCharacterController.Game;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public class WaveManager : SingletonMonobehavior<WaveManager>
    {

        [SerializeField] private GameObject enemyToSpawn;
        [SerializeField] private GameObject enemyToSpawn2;
        [SerializeField] private SpawnPoint enemySpawnpoint;
        [SerializeField, ReadOnly] private int currentWave;
        [SerializeField, ReadOnly] private bool isWaveActive;
        private int enemiesRemaining;



        protected override void Awake()
        {
            base.Awake();

            isWaveActive = false;
            enemiesRemaining = 0;
            currentWave = 0;
        }

        private void OnWaveStart()
        {
            if (isWaveActive)
                return;

            currentWave++;
            //spawn enemy
            isWaveActive = true;
            SpawnEnemy(enemyToSpawn, enemySpawnpoint);
            SpawnEnemy(enemyToSpawn2, enemySpawnpoint);
        }

        private void OnWaveEnd(int currentWave)
        {
            isWaveActive = false;
        }

        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            if (gameObject == null)
            {
                Debug.LogWarning("dead object cannot be removed from pool, because the reference has already been destroyed");
                return;
            }

            if (!isWaveActive)
                return;

            if (enemiesRemaining > 0)
            {
                enemiesRemaining--;
            }

            //if all enemies are dead, trigger OnWaveEnd
            if (enemiesRemaining <= 0)
                EventHandler.ExecuteEvent(gameObject, "OnWaveEnd", currentWave);
        }

        private void SpawnEnemy(GameObject enemyPrefab, SpawnPoint spawnpoint)
        {
            if (enemyPrefab == null)
            {
                Debug.LogWarning($"You have a spawner \"{name}\" which has no object to spawn assigned");
                return;
            }
            if (spawnpoint == null)
            {
                Debug.LogWarning($"You have a spawner \"{name}\" which has no spawnpoint assigned");
                return;
            }

            Vector3 position = spawnpoint.transform.position;
            Quaternion rotation = spawnpoint.transform.rotation;
            float size = enemyPrefab.GetComponentInChildren<Collider>().bounds.size.magnitude;
            if (spawnpoint.GetPlacement(enemyPrefab, ref position, ref rotation, size))
            {
                GameObject spawnedObject = Instantiate(enemyPrefab, position, rotation);
                //register to the enemy onDeath event
                EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(spawnedObject, "OnDeath", OnDeath);
                enemiesRemaining++;
            }
            else
            {
                Debug.Log($"spawn placement failed for {enemyPrefab.name}");
            }


        }

        private void OnEnable()
        {
            EventHandler.RegisterEvent(gameObject, "OnWaveStart", OnWaveStart);
            EventHandler.RegisterEvent<int>(gameObject, "OnWaveEnd", OnWaveEnd);
        }

        private void OnDisable()
        {
            EventHandler.UnregisterEvent(gameObject, "OnWaveStart", OnWaveStart);
            EventHandler.UnregisterEvent<int>(gameObject, "OnWaveEnd", OnWaveEnd);
        }
    }
}
