using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ForceSystem
{
    public class DemoSpawnAreaOfEffect : MonoBehaviour
    {
        public GameObject explosionPrefab;

        public float rateOfSpawn;

        private float timeTillNextSpawn;
        public bool spawnOnStartOnly;
        // Start is called before the first frame update
        void Start()
        {
            timeTillNextSpawn = rateOfSpawn;
            if (spawnOnStartOnly)
                Spawn();
        }

        // Update is called once per frame
        void Update()
        {
            if (spawnOnStartOnly)
                return;

            if (timeTillNextSpawn > 0)
            {
                timeTillNextSpawn -= Time.deltaTime;
            }
            else
            {
                Spawn();
                timeTillNextSpawn = rateOfSpawn;
            }
        }
        private void Spawn()
        {
            Instantiate(explosionPrefab, transform);
        }
    }
}