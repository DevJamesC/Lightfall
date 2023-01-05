using MBS.ForceSystem;
using MBS.ModifierSystem;
using MBS.StatsAndTags;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.DamageSystem
{
    public class ExampleDamager : MonoBehaviour, IDamager, IForcer
    {
        public float Damage { get => damage.Damage; }

        public DamageSourceType DamageSourceType { get; set; }
        public int Force { get; set; }

        [SerializeField]
        private DamageData damage = new DamageData();
        [SerializeField]
        private float refireRate;
        [SerializeField]
        private LayerMask hitableLayers;

        public event Action<IDamageable, DamageData> OnDealDamage = delegate { };

        [SerializeField, ReadOnly]
        float refireCountdown;

        [SerializeField]
        private bool debugStats;

        //private DynamicModifierController modifierController;
        private ModifierHandler modifierHandler;
        private TagHandler tagHandler;

        private void Awake()
        {
            modifierHandler = GetComponent<ModifierHandler>();
            tagHandler = GetComponent<TagHandler>();
        }

        private void Start()
        {
            if (modifierHandler != null)
                modifierHandler.GetStatModifier(StatName.WeaponRateOfFire).OnValueChanged += RecalibrateRefire;
        }

        private void OnDestroy()
        {
            if (modifierHandler != null)
                modifierHandler.GetStatModifier(StatName.WeaponRateOfFire).OnValueChanged -= RecalibrateRefire;
        }


        private void OnEnable()
        {
            if (modifierHandler != null)
                refireCountdown = refireRate * modifierHandler.GetStatModifierValue(StatName.WeaponRateOfFire);
            else
                refireCountdown = refireRate;
        }

        private void Update()
        {
            TryFire();
            if (refireCountdown > 0)
                refireCountdown -= Time.deltaTime;
        }
        private void TryFire()
        {
            if (refireCountdown > 0)
                return;

            if (modifierHandler != null)
                refireCountdown = refireRate * modifierHandler.GetStatModifierValue(StatName.WeaponRateOfFire);
            else
                refireCountdown = refireRate;

            Fire();
        }
        private void Fire()
        {
            float distance = 100f;
            Ray ray = new Ray(transform.position, transform.forward);

            Debug.DrawRay(ray.origin, ray.direction * distance, Color.red, .15f);
            if (!Physics.Raycast(ray, out RaycastHit hit, distance, hitableLayers))
                return;

            IDamageable damageable = hit.collider.gameObject.GetComponentInParent<IDamageable>();
            if (damageable == null)
                return;

            DealDamage(damageable, hit.point, hit.collider);


        }

        public void DealDamage(IDamageable damageableHit, Vector3 hitPoint, Collider colliderHit = null)
        {
            DamageData damageData = damage.GetShallowCopy();
            damageData.ChangeSource(this, tagHandler.Tags);
            damageData.SetForceData(damageData.ForceData.GetShallowCopy(this, hitPoint));


            if (modifierHandler != null)
            {
                damageData.SetDamage(damageData.Damage * modifierHandler.GetStatModifierValue(StatName.WeaponDamage));
                modifierHandler.ApplyPreDamageProcessors(damageData, damageableHit);
            }



            damageableHit.TakeDamage(damageData, colliderHit);
            OnDealDamage.Invoke(damageableHit, damageData);

            if (debugStats)
            {
                Debug.Log($"{name} Damage: {damageData.DamageWithForce}");
            }

        }

        private void RecalibrateRefire(float newPercent)
        {
            float percentRefireCompleted = Mathf.Clamp01(refireCountdown / refireRate);
            refireCountdown = refireRate * newPercent;
            refireCountdown *= percentRefireCompleted;
        }
    }
}
