using Opsive.Shared.Events;
using Opsive.UltimateCharacterController;
using Opsive.UltimateCharacterController.Game;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public class WaveManager : SingletonMonobehavior<WaveManager>
    {
        [SerializeField] private List<EnemyWave> enemyWaves;
        [SerializeField] private SpawnPoint enemySpawnpoint;
        [SerializeField, ReadOnly] private int currentWave;
        [SerializeField, ReadOnly] private bool waveIsActive;
        private int enemiesRemaining;

        public bool WaveIsActive { get { return waveIsActive; } }


        protected override void Awake()
        {
            base.Awake();

            waveIsActive = false;
            enemiesRemaining = 0;
            currentWave = 0;
        }

        private void OnWaveStart()
        {
            if (waveIsActive)
                return;
            if (currentWave >= enemyWaves.Count)
            {
                Debug.Log($"tried to start wave {currentWave + 1}, but there are only {enemyWaves.Count} enemy waves.");
                return;
            }


            //spawn enemy
            waveIsActive = true;
            foreach (var enemy in enemyWaves[currentWave].EnemyPrefabs)
            {
                SpawnEnemy(enemy, enemySpawnpoint);
            }

            currentWave++;
        }

        private void OnWaveEnd(int currentWave)
        {
            waveIsActive = false;

            if (currentWave >= enemyWaves.Count)
            {
                Opsive.Shared.Events.EventHandler.ExecuteEvent("OnAllEnemyWavesComplete");
            }
        }

        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            if (gameObject == null)
            {
                Debug.LogWarning("dead object cannot be removed from pool, because the reference has already been destroyed");
                return;
            }

            if (!waveIsActive)
                return;

            if (enemiesRemaining > 0)
            {
                enemiesRemaining--;
            }

            //if all enemies are dead, trigger OnWaveEnd
            if (enemiesRemaining <= 0)
                Opsive.Shared.Events.EventHandler.ExecuteEvent<int>("OnWaveEnd", currentWave);
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
                Opsive.Shared.Events.EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(spawnedObject, "OnDeath", OnDeath);
                enemiesRemaining++;
            }
            else
            {
                Debug.Log($"spawn placement failed for {enemyPrefab.name}");
            }


        }

        private void OnEnable()
        {
            Opsive.Shared.Events.EventHandler.RegisterEvent("OnWaveStart", OnWaveStart);
            Opsive.Shared.Events.EventHandler.RegisterEvent<int>("OnWaveEnd", OnWaveEnd);
        }

        private void OnDisable()
        {
            Opsive.Shared.Events.EventHandler.UnregisterEvent("OnWaveStart", OnWaveStart);
            Opsive.Shared.Events.EventHandler.UnregisterEvent<int>("OnWaveEnd", OnWaveEnd);

        }
    }

    [Serializable]
    public class EnemyWave
    {
        public List<GameObject> EnemyPrefabs;
    }
}
