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
            SpawnEnemy();
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

        private void SpawnEnemy()
        {
            if (enemyToSpawn == null)
            {
                Debug.LogWarning($"You have a spawner \"{name}\" which has no object to spawn assigned");
                return;
            }
            if (enemySpawnpoint == null)
            {
                Debug.LogWarning($"You have a spawner \"{name}\" which has no spawnpoint assigned");
                return;
            }

            Vector3 position = enemySpawnpoint.transform.position;
            Quaternion rotation = enemySpawnpoint.transform.rotation;
            enemySpawnpoint.GetPlacement(enemyToSpawn, ref position, ref rotation);

            GameObject spawnedObject = Instantiate(enemyToSpawn, position, rotation);

            //register to the enemy onDeath event
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(spawnedObject, "OnDeath", OnDeath);
            enemiesRemaining++;
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
