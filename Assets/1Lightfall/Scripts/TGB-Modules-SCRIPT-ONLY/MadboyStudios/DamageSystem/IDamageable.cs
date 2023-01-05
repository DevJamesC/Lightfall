using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.DamageSystem
{
    public interface IDamageable
    {
        GameObject gameObject { get; }
        void TakeDamage(DamageData damageData, Collider colliderHit = null);
    }
}
