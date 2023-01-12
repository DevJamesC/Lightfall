using MBS.ForceSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.DamageSystem
{
    public class ExampleDamageable : MonoBehaviour, IDamageable, IForceable
    {
        public void TakeDamage(DamageData damageData, Collider colliderHit = null)
        {
            Debug.Log($"{gameObject.name} recieved {damageData.Amount} damage from {damageData.DamageSource.SourceGameObject.name}");
        }

        public void TakeForce(ForceData forceData)
        {
            Debug.Log($"{gameObject.name} recieved {forceData.Force} damage from {forceData.Source.gameObject.name}");
        }
    }
}
