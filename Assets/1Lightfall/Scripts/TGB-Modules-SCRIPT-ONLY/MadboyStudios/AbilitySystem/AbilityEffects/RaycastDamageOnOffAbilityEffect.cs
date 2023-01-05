using MBS.DamageSystem;
using MBS.ForceSystem;
using MBS.StatsAndTags;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.AbilitySystem
{
    public class RaycastDamageOnOffAbilityEffect : AbilityEffectBase
    {
        [SerializeField, Tooltip("The Icon to be shown under stats when viewing this ability. ONLY ASSIGN THIS IF THE ABILITY" +
    " HAS AN EFFECT (eg. an ability which is a 'Fire Detonator' should have an icon, but a normal ability should not). " +
    "It should be noted that modifiers can set the icon too, so individual upgrades may add the required assocaited icons.")]
        private Sprite AbilityEffectStatUIIcon;
        [SerializeField]
        private float damage;
        [SerializeField]
        private float range;
        [SerializeField]
        private LayerMask hitableLayers;
        [SerializeField]
        private Color raycastColor = Color.magenta;
        [SerializeField]
        private float fireIntervel = .25f;

        private float fireIntervelRemaining;

        public override void Use(AbilityWrapperBase abilityWrapper)
        {

            fireIntervelRemaining = 0;
            OnEffectFinishedInvoke();
        }

        public override void UseWhileInUse(AbilityWrapperBase abilityWrapper)
        {
            //OnOff abilites are automatically set to Deactivating in AbilityWrapperBase.
            //But might as well confirm that the ability is in fact deactivating before we do anything.
            //Also, if you want an OnOff ability to have a second use pattern (such as "Off" -> "On and +Defense" -> "On and +Offense" -> "Off"), you can make that change here...
            //by setting AbilityState to ActiveInBackground.
            //For an "always on" ability (Think Lucio buff from Overwatch), just have inactive give X buff, and active give Y buff OnUpdate.
            if (abilityWrapper.AbilityState == AbilityState.Deactivating)
            {
                Dispose(abilityWrapper);
                return;
            }
            //OnOff abilites do not need to call OnAbilityFinishedInvoke() when they turn off, because they generally call it at the end of Use();
        }

        public override void OnUpdate(AbilityWrapperBase abilityWrapper)
        {
            base.OnUpdate(abilityWrapper);


            if (abilityWrapper.AbilityState != AbilityState.InUseInBackground)
                return;


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

        public override void Dispose(AbilityWrapperBase abilityWrapperBase)
        {
            //since this does not do anything like "increase armor" or "decrease ability recharge", there is nothing to dispose of here.
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

            DamageData damageData = new DamageData(abilityWrapper, new ForceData(abilityWrapper, abilityWrapper.Force, hit.point), null, abilityWrapper.OriginTags);
            damageData.SetDamage(Damage);
            abilityWrapper.ModifierHandler.ApplyPreDamageProcessors(damageData, damageable);

            damageable.TakeDamage(damageData, hit.collider);

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

            return returnVal;
        }
    }
}
