using MBS.DamageSystem;
using MBS.ForceSystem;
using MBS.StatsAndTags;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MBS.AbilitySystem
{
    public class RaycastDamageChannelAbilityEffect : AbilityEffectBase
    {
        [SerializeField, Tooltip("The Icon to be shown under stats when viewing this ability. ONLY ASSIGN THIS IF THE ABILITY" +
    " HAS AN EFFECT (eg. an ability which is a 'Fire Detonator' should have an icon, but a normal ability should not). " +
    "It should be noted that modifiers can set the icon too, so individual upgrades may add the required assocaited icons.")]
        private Sprite AbilityEffectStatUIIcon;
        [SerializeField]
        private float duration;
        [SerializeField]
        private float damage;
        [SerializeField]
        private float range;
        [SerializeField]
        private LayerMask hitableLayers;
        [SerializeField]
        private Color raycastColor = Color.yellow;
        [SerializeField]
        private float fireIntervel = .25f;

        private float durationRemaining;
        private float fireIntervelRemaining;

        public override void Use(AbilityWrapperBase abilityWrapper)
        {
            durationRemaining = abilityWrapper.GetStatChange(StatName.AbilityDuration, duration, true);
            fireIntervelRemaining = 0;
        }

        public override void UseWhileInUse(AbilityWrapperBase abilityWrapper)
        {
            //setting durationRemaining to 0 makes it so the next update will wrap up the ability.
            durationRemaining = 0;
            //Other abilites may need to call OnAbilityFinishedInvoke(), depending on thier implimentation
        }

        public override void OnUpdate(AbilityWrapperBase abilityWrapper)
        {
            base.OnUpdate(abilityWrapper);

            if (abilityWrapper.AbilityState != AbilityState.InUse)
                return;

            if (durationRemaining <= 0)
            {
                OnEffectFinishedInvoke();
                return;
            }

            durationRemaining -= Time.deltaTime;

            if (fireIntervelRemaining > 0)
            {
                fireIntervelRemaining -= Time.deltaTime;
                return;
            }
            else
            {
                fireIntervelRemaining = fireIntervel;
            }


            float realDamage = abilityWrapper.GetStatChange(StatName.AbilityDamage, damage, true);
            float realRadius = abilityWrapper.GetStatChange(StatName.AbilityRadius, range, true);
            Shoot(abilityWrapper, realDamage, realRadius);




        }

        private void Shoot(AbilityWrapperBase abilityWrapper, float Damage, float Range)
        {
            Ray ray = new Ray(abilityWrapper.gameObject.transform.position, abilityWrapper.gameObject.transform.forward);

            Debug.DrawRay(ray.origin, ray.direction * Range, raycastColor, .15f);
            if (!Physics.Raycast(ray, out RaycastHit hit, Range, hitableLayers))
                return;

            IDamageable damageable = hit.collider.gameObject.GetComponentInParent<IDamageable>();
            if (damageable == null)
                return;

            DamageData damageData = new DamageData();//abilityWrapper, new ForceData(abilityWrapper, abilityWrapper.Force, hit.point), null, abilityWrapper.OriginTags);
            damageData.SetDamage(Damage, ray.origin, ray.direction, abilityWrapper.Force, 1, 0, abilityWrapper.Origin, abilityWrapper.Origin, hit.collider);
            damageData.Amount = Damage;

            damageable.TakeDamage(damageData, hit.collider);
            abilityWrapper.ModifierHandler.ApplyPreDamageProcessors(damageData, damageable);

            abilityWrapper.DealDamage(damageable, hit.point);//does nothing more than invoke the dealDamage event

        }

        public override List<AbilityUIStat> GetStats()
        {
            List<AbilityUIStat> returnVal = new List<AbilityUIStat>();

            returnVal.Add(new AbilityUIStat()
            {
                StatName = StatName.AbilityDamage,
                StatNameDisplayName = "Damage",
                InitalValue = damage,
                CurrentValue = damage,
                MaxValue = damage,
                ProspectiveValue = damage,
                EffectIcon = AbilityEffectStatUIIcon
            });

            returnVal.Add(new AbilityUIStat()
            {
                StatName = StatName.AbilityRadius,
                StatNameDisplayName = "Range",
                statValueDisplaySuffix = " m",
                InitalValue = range,
                CurrentValue = range,
                MaxValue = range,
                ProspectiveValue = range
            });

            returnVal.Add(new AbilityUIStat()
            {
                StatName = StatName.AbilityDuration,
                StatNameDisplayName = "Duration",
                statValueDisplaySuffix = " Sec",
                InitalValue = duration,
                CurrentValue = duration,
                MaxValue = duration,
                ProspectiveValue = duration
            });

            return returnVal;
        }
    }
}
