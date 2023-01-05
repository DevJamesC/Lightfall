using MBS.DamageSystem;
using MBS.HealthSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.HealthSystem
{
    public class ValuePoolColliderModifier : MonoBehaviour
    {
        [SerializeField, Tooltip("Leave this blank and it will grab the first found health in parent objects.")]
        protected Health health;

        protected event Action<DamageData> OnTakeHit = delegate { };

        public Collider Collider { get; protected set; }


        protected virtual void Awake()
        {
            Collider = GetComponent<Collider>();
            if (health == null)
                health = GetComponentInParent<Health>();
        }

        protected virtual void Start()
        {
            health.AddColliderModifierToDictionary(this);
        }

        public void InvokeOnTakeHit(DamageData damageData)
        {
            OnTakeHit.Invoke(damageData);
        }
    }
}
