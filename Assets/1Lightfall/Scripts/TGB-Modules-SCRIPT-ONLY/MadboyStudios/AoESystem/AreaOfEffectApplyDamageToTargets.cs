using MBS.DamageSystem;
using MBS.ModifierSystem;
using MBS.StatsAndTags;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.AoeSystem
{
    public class AreaOfEffectApplyDamageToTargets : MonoBehaviour, IOrigin, IDamager
    {
        public new GameObject gameObject { get; private set; }
        public List<Tag> OriginTags { get; private set; }

        float IDamager.Damage { get => Damage.Amount; }
        public DamageSourceType DamageSourceType => DamageSourceType.Undefined;

        [SerializeField]
        private DamageData Damage;
        [SerializeField]
        private float PercentDamageDropoffInSecondaryRadius = 60;
        [SerializeField, Tooltip("Only used if the AoE script is not an instant AoE")]
        private float tickRate = .25f;

        private AreaOfEffectBase areaOfEffectComponent;

        public event Action<IDamageable, DamageData> OnDealDamage = delegate { };

        private DamageData instanceDamage;

        private float timeTillNextTick;

        private void Awake()
        {
            gameObject = transform.gameObject;
            areaOfEffectComponent = GetComponent<AreaOfEffectBase>();
            TagHandler tagHandler = GetComponent<TagHandler>();
            if (tagHandler != null)
                OriginTags = tagHandler.Tags;

            if (areaOfEffectComponent != null)
            {
                areaOfEffectComponent.OnInsidePrimaryRadius += AreaOfEffectComponent_OnInsidePrimaryRadius;
                areaOfEffectComponent.OnInsideSecondaryRadius += AreaOfEffectComponent_OnInsideSecondaryRadius;
            }
            else
                Debug.Log($"{GetType()} on {gameObject.name} needs AreaOfEffectBase subclass to work! Add the component to resolve.");

            //Damage.ForceData.SetPointOfForce(transform);
            timeTillNextTick = 0;

        }

        private void Update()
        {
            if (timeTillNextTick <= 0)
                timeTillNextTick = tickRate;

            if (timeTillNextTick > 0)
                timeTillNextTick -= Time.deltaTime;
        }

        private void AreaOfEffectComponent_OnInsideSecondaryRadius(Collider collider)
        {
            if (timeTillNextTick > 0)
                return;

            IDamageable damageable = collider.gameObject.GetComponentInParent<IDamageable>();
            if (damageable == null)
                return;

            instanceDamage = Damage.Copy();
            Debug.Log("Need to rework AoE Damage to work with Opsive Damage...");
            //instanceDamage.SetDamage(instanceDamage.Amount * (1 - (PercentDamageDropoffInSecondaryRadius / 100)));
            //instanceDamage.SetForceData(instanceDamage.ForceData.GetShallowCopy());
            //instanceDamage.ForceData.SetForce(instanceDamage.ForceData.Force * (1 - (PercentDamageDropoffInSecondaryRadius / 100)));

            //instanceDamage.ChangeSource(this, OriginTags);
            DealDamage(damageable, collider.bounds.center, collider);

        }

        private void AreaOfEffectComponent_OnInsidePrimaryRadius(Collider collider)
        {
            if (timeTillNextTick > 0)
                return;

            IDamageable damageable = collider.gameObject.GetComponentInParent<IDamageable>();
            if (damageable == null)
                return;

            instanceDamage = Damage.Copy();
            Debug.Log("Need to rework AoE Damage to work with Opsive Damage...");
            //instanceDamage.ChangeSource(this, OriginTags);
            DealDamage(damageable, collider.bounds.center, collider);


        }

        public void SetDamage(DamageData newDamage)
        {
            Damage = newDamage;
        }
        public void SetOrigin(GameObject newOrigin)
        {
            gameObject = newOrigin;
            TagHandler tagHandler = newOrigin.GetComponent<TagHandler>();
            if (tagHandler != null)
                OriginTags = tagHandler.Tags;
        }

        private void OnValidate()
        {
            AreaOfEffect instanceAreaOfEffect = GetComponent<AreaOfEffect>();

            if (instanceAreaOfEffect == null)
                return;

            Debug.Log("Need to rework AoE Damage to work with Opsive Damage...");
            //Damage.ForceData.ExplosiveRadius = instanceAreaOfEffect.SecondaryRadius;
        }

        public void DealDamage(IDamageable damageableHit, Vector3 hitPoint, Collider colliderHit = null)
        {
            damageableHit.TakeDamage(instanceDamage, GetComponent<Collider>());
            OnDealDamage.Invoke(damageableHit, instanceDamage);
        }
    }
}
