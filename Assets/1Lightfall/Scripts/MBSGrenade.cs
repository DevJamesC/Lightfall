using Opsive.Shared.Game;
using Opsive.UltimateCharacterController.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public class MBSGrenade : Grenade
    {
        public event Action<GameObject> OnSpawnDestructionObject = delegate { };

        public override void Destruct(Vector3 hitPosition, Vector3 hitNormal)
        {
            InvokeOnDestruct(hitPosition, hitNormal);
            m_ProjectileOwner?.OnProjectileDestruct(this, hitPosition, hitNormal);

            for (int i = 0; i < m_SpawnedObjectsOnDestruction.Length; ++i)
            {
                if (m_SpawnedObjectsOnDestruction[i] == null)
                {
                    continue;
                }

                var spawnedObject = m_SpawnedObjectsOnDestruction[i].Instantiate(hitPosition, hitNormal, m_NormalizedGravity);
                OnSpawnDestructionObject.Invoke(spawnedObject);
                if (spawnedObject == null)
                {
                    continue;
                }
                var explosion = spawnedObject.GetCachedComponent<MBSExplosion>();
                if (explosion != null)
                {
                    explosion.Explode(m_ImpactDamageData, m_Owner, m_OwnerDamageSource);
                }
            }

            // The component and collider no longer need to be enabled after the object has been destroyed.
            if (m_Collider != null)
            {
                m_Collider.enabled = false;
            }
            if (m_ParticleSystem != null)
            {
                m_ParticleSystem.Stop();
            }
            m_Destroyed = true;
            m_DestroyEvent = null;
            enabled = false;

            // The destructible should be destroyed.
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_MULTIPLAYER
            if (NetworkObjectPool.IsNetworkActive()) {
                // The object may have already been destroyed over the network.
                if (!m_GameObject.activeSelf) {
                    return;
                }
                NetworkObjectPool.Destroy(m_GameObject);
                return;
            }
#endif
            ObjectPoolBase.Destroy(m_GameObject);
        }
    }
}
