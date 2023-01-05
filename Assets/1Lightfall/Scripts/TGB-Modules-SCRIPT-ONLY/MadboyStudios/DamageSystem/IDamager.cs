using System;
using UnityEngine;

namespace MBS.DamageSystem
{
    public interface IDamager
    {
        GameObject gameObject { get; }
        float Damage { get; }

        DamageSourceType DamageSourceType { get; }

        event Action<IDamageable, DamageData> OnDealDamage;
        void DealDamage(IDamageable damageableHit, Vector3 hitPoint, Collider colliderHit = null);
    }

    public enum DamageSourceType
    {
        WeaponDamage,
        AbilityDamage,
        MeleeDamage,
        Undefined
    }
}