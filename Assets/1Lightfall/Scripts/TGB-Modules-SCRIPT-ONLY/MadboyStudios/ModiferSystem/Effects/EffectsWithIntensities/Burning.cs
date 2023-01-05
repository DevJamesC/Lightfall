using MBS.AbilitySystem;
using MBS.DamageSystem;
using MBS.StatsAndTags;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public class Burning : EffectWithIntensityBase
    {
        //[SerializeField]
        //private string effectUITitle = "Burning";
        public DamageData DamageData { get => damageData; }

        public event Action OnTick = delegate { };
        [SerializeReference, BoxGroup("Lists")]
        private List<ModifierFXBase> onTickFX = new List<ModifierFXBase>();
        [SerializeField]
        private float ticksPerSecond = 1f;
        [SerializeField]
        private bool tickOnActivation;
        [SerializeField, Tooltip("will only tick if it times out, not if removed by other effects")]
        private bool tickOnEnd;
        [InfoBox("$infoMessageString", InfoMessageType.None)]
        [SerializeField]
        private DamageData damageData = new DamageData(null, null);


        private float totalDamage;
        private float damagePerSecond;
        private string infoMessageString { get => $"Total Damage: {totalDamage}\nDamage per second: {damagePerSecond}"; }

        public override void EffectActivated(ModifierEntry targetEntry)
        {
            base.EffectActivated(targetEntry);
            int intensity = Setup(targetEntry);
            float sizeModifier = 0;

            ModifierHandler originHandler = targetEntry.Origin.gameObject.GetComponent<ModifierHandler>();
            if (originHandler != null)
                sizeModifier = originHandler.GetStatModifierValue(StatName.DetonationSize) - 1;

            BurningEffectData effectData = new BurningEffectData(1 / ticksPerSecond,
                new DoTDamager(targetEntry.Origin.gameObject, damageData.GetShallowCopy()),
                targetEntry.Target.GetComponent<IDamageable>(),
                intensity,
                sizeModifier
                );

            List<ModifierEntry> entries = targetEntry.Target.GetModifierEntries(this);
            foreach (var entry in entries)
            {
                Burning effect = entry.Effect as Burning;
                if (effect.effectCategory != effectCategory || entry == targetEntry)
                    continue;

                BurningEffectData data = entry.EffectStateData as BurningEffectData;
                effectData.timeTillNextTick = data.timeTillNextTick;
                break;
            }

            targetEntry.EffectStateData = effectData;

            //check if it can take damage
            if (!effectData.HasIDamageable)
            {
                targetEntry.InitalDuration = -1;
                return;
            }

            if (tickOnActivation)
                effectData.DealDamage();

        }

        public override void EffectRemoved(ModifierEntry targetEntry)
        {
            if (targetEntry.RemainingDuration > -1)
            {
                BurningEffectData data = targetEntry.EffectStateData as BurningEffectData;
                data.DealDamage();

                for (int i = 0; i < onTickFX.Count; i++)
                {
                    onTickFX[i].Activate(targetEntry);
                }

                OnTick.Invoke();
            }

            base.EffectRemoved(targetEntry);
        }

        public override void EffectUpdate(ModifierEntry targetEntry)
        {
            //retrieve data from handler and incriment it
            BurningEffectData data = targetEntry.EffectStateData as BurningEffectData;

            if (data.timeTillNextTick >= 0)
                data.timeTillNextTick -= Time.deltaTime;
            else
            {
                data.timeTillNextTick = 1 / ticksPerSecond;
                //deal damage
                data.DealDamage();

                for (int i = 0; i < onTickFX.Count; i++)
                {
                    onTickFX[i].Activate(targetEntry);
                }

                OnTick.Invoke();
            }
        }

        public override string ToString()
        {
            return $"{damagePerSecond}dps";
        }

        public override void OnValidate()
        {
            base.OnValidate();
            effectCategory = MajorEffects.Burning;
            totalDamage = TotalDamage();
            damagePerSecond = DPS();
        }

        public override void ApplyAbilitySystemUpgradesToEntries(ModifierEntry entry, AbilityWrapperBase abilityWrapper)
        {
            base.ApplyAbilitySystemUpgradesToEntries(entry, abilityWrapper);
            BurningEffectData data = entry.EffectStateData as BurningEffectData;
            if (data == null)
                return;

            //get and apply upgrades
            float realEffectDuration = abilityWrapper.GetStatChange(StatName.AbilityModifierEffectDuration, entry.RemainingDuration, true);
            float realBaseDamage = abilityWrapper.GetStatChange(StatName.AbilityModifierEffectDamage, data.damager.Damage, true);

            entry.InitalDuration = realEffectDuration;
            entry.RemainingDuration = realEffectDuration;
            data.SetDamage(realBaseDamage);

        }

        public float DPS()
        {
            float totalDamage = TotalDamage();
            float damagePerSecond = totalDamage / duration;

            return damagePerSecond;
        }

        public float TotalDamage()
        {

            float totalTicks = Mathf.Floor((ticksPerSecond * duration) - (tickOnActivation ? 0 : (1 / ticksPerSecond)));
            float totalDamage = totalTicks * damageData.Damage;

            return totalDamage;
        }

        public float RemainingDamage(float remainingDuration)
        {
            float remainingTicks = Mathf.Floor((ticksPerSecond * remainingDuration) - (tickOnActivation ? 0 : (1 / ticksPerSecond)));
            float remainingDamage = remainingTicks * damageData.Damage;

            return remainingDamage;
        }

        public override List<AbilityUIStat> GetStats()
        {
            List<AbilityUIStat> returnVal = new List<AbilityUIStat>();

            returnVal.Add(new AbilityUIStat
            {
                StatName = StatName.AbilityModifierEffectDamage,
                StatNameDisplayName = $"Burning Damage",
                statValueDisplaySuffix = " dps",
                CurrentValue = damagePerSecond,
                MaxValue = damagePerSecond,
                InitalValue = damagePerSecond,
                ProspectiveValue = damagePerSecond,
                EffectIcon = UIDisplayIcon
            });
            returnVal.Add(new AbilityUIStat
            {
                StatName = StatName.AbilityModifierEffectDuration,
                StatNameDisplayName = $"Burning Duration",
                statValueDisplaySuffix = " sec",
                CurrentValue = duration,
                MaxValue = duration,
                InitalValue = duration,
                ProspectiveValue = duration,
                EffectIcon = null
            });
            returnVal.Add(new AbilityUIStat
            {
                StatName = StatName.AbilityModifierEffectIntensity,
                StatNameDisplayName = $"Burning Intensity",
                CurrentValue = BaseIntensity,
                MaxValue = BaseIntensity,
                InitalValue = BaseIntensity,
                ProspectiveValue = BaseIntensity,
                EffectIcon = null
            });

            return returnVal;
        }
    }

    public class BurningEffectData : EffectWithIntensityData
    {
        public bool HasIDamageable => target != null;
        public float timeTillNextTick;
        public DoTDamager damager;
        private IDamageable target;
        private float baseDamage;



        public BurningEffectData(float tickDuration, DoTDamager damager, IDamageable target, int intensity, float detonationSizeModifier) : base(intensity, detonationSizeModifier)
        {

            timeTillNextTick = tickDuration;
            this.baseDamage = damager.Damage;
            this.damager = damager;
            this.target = target;
            switch (Intensity)
            {
                case 2:
                    damager.SetDamage(baseDamage * 1.15f);
                    //TODO: should impair the target (speed/ accuracy, AI effectivness) and have a chance to cause panic
                    break;
                case 3:
                    damager.SetDamage(baseDamage * 1.30f);
                    //TODO: should impair the target more (speed/ accuracy, AI effectivness), have a high chance to cause panic, and 
                    //have a high chance to apply this effect at intensity 1 to targets that touch the target.
                    break;
                case 4:
                    damager.SetDamage(baseDamage * 1.4f);
                    //TODO: should impair the target more (speed/ accuracy, AI effectivness), have a high chance to cause panic, and 
                    //have a high chance to apply this effect at intensity 2 to targets that touch the target, and intensity 1 to targets near the target
                    break;
            }
        }

        public void SetDamage(float damage)
        {
            baseDamage = damage;
            switch (Intensity)
            {
                case 2:
                    damager.SetDamage(baseDamage * 1.15f);
                    break;
                case 3:
                    damager.SetDamage(baseDamage * 1.30f);
                    break;
                case 4:
                    damager.SetDamage(baseDamage * 1.40f);
                    break;
            }
        }

        public void DealDamage()
        {
            damager.DealDamage(target, target.gameObject.transform.position);
        }
    }
}
