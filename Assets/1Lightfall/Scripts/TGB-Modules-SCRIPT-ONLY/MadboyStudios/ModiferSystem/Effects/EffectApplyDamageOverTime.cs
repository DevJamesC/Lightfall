using MBS.AbilitySystem;
using MBS.DamageSystem;
using MBS.StatsAndTags;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public class EffectApplyDamageOverTime : ModifierEffectBase
    {
        [SerializeField]
        private string effectUITitle;
        public DamageData DamageData { get => damageData; }

        public event Action OnTick = delegate { };
        [SerializeReference]
        [BoxGroup("Lists")]
        private List<ModifierFXBase> onTickFX = new List<ModifierFXBase>();
        [SerializeField]
        private float ticksPerSecond = 1f;
        [SerializeField]
        private bool tickOnActivation;
        [SerializeField]
        private DamageData damageData = new DamageData(null, null);
        [InfoBox("$infoMessageString", InfoMessageType.None)]
        [SerializeReference]
        private EffectAlreadyExistsDoTImplimentation effectAlreadyExistsLogic;

        private float totalDamage;
        private float damagePerSecond;
        private string infoMessageString { get => $"Total Damage: {totalDamage}\nDamage per second: {damagePerSecond}"; }

        public override void EffectActivated(ModifierEntry targetEntry)
        {
            base.EffectActivated(targetEntry);

            //check if the target already has this effect on them. If so, determine what action to take
            if (effectAlreadyExistsLogic.HandleEffectAlreadyExists(targetEntry, targetEntry.Target.GetModifierEntries(targetEntry.Effect)))
                return;

            //add data in modifierHandler
            DamageOverTimeEffectStateData newData = new DamageOverTimeEffectStateData(
                1 / ticksPerSecond,
                new DoTDamager(targetEntry.Origin.gameObject, damageData),
                targetEntry.Target.GetComponent<IDamageable>());

            targetEntry.EffectStateData = newData;

            //check if it can take damage
            if (!newData.HasIDamageable)
            {
                targetEntry.InitalDuration = -1;
                return;
            }

            if (tickOnActivation)
                newData.DealDamage();
        }

        public override void EffectRemoved(ModifierEntry targetEntry)
        {
            base.EffectRemoved(targetEntry);

        }

        public override void EffectUpdate(ModifierEntry targetEntry)
        {
            //retrieve data from handler and incriment it
            DamageOverTimeEffectStateData data = targetEntry.EffectStateData as DamageOverTimeEffectStateData;

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
            totalDamage = TotalDamage();
            damagePerSecond = DPS();
            if (effectAlreadyExistsLogic != null)
            {
                effectAlreadyExistsLogic.OnValidate();
                effectAlreadyExistsLogic.PopulateTags(tags);
            }
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
                StatNameDisplayName = $"{effectUITitle} Damage",
                statValueDisplaySuffix = " Dps",
                CurrentValue = damagePerSecond,
                MaxValue = damagePerSecond,
                InitalValue = damagePerSecond,
                ProspectiveValue = damagePerSecond,
                EffectIcon = UIDisplayIcon
            });
            returnVal.Add(new AbilityUIStat
            {
                StatName = StatName.AbilityModifierEffectDuration,
                StatNameDisplayName = $"{effectUITitle} Duration",
                statValueDisplaySuffix = " Sec",
                CurrentValue = duration,
                MaxValue = duration,
                InitalValue = duration,
                ProspectiveValue = duration,
                EffectIcon = null
            });

            return returnVal;
        }

    }

    //Handles actually dealing damage and holding all damage related info (such as the object reference of the damage dealer)
    public class DoTDamager : IDamager
    {
        public GameObject gameObject => sourceObj;

        public float Damage => damageData.Damage;
        public DamageSourceType DamageSourceType { get; set; }

        public event Action<IDamageable, DamageData> OnDealDamage = delegate { };

        private GameObject sourceObj;
        private DamageData damageData;


        public void DealDamage(IDamageable damageableHit, Vector3 hitPoint, Collider collider = null)
        {
            DamageData damageDataInstance = damageData.GetShallowCopy();
            damageableHit.TakeDamage(damageDataInstance);
            OnDealDamage.Invoke(damageableHit, damageDataInstance);
        }

        public DoTDamager(GameObject damageSourceObj, DamageData damageData)
        {
            sourceObj = damageSourceObj;
            this.damageData = damageData;
            this.damageData.ChangeSource(this);
        }

        public void SetDamage(float newDamage)
        {
            damageData.SetDamage(newDamage);
        }
    }

    //This is the data object which stores the state of a particular instance of Damage over Time
    public class DamageOverTimeEffectStateData : EffectStateData
    {
        public bool HasIDamageable => target != null;
        public float timeTillNextTick;
        private DoTDamager damager;
        private IDamageable target;

        public DamageOverTimeEffectStateData(float tickDuration, DoTDamager damager, IDamageable target)
        {
            timeTillNextTick = tickDuration;
            this.damager = damager;
            this.target = target;
        }



        public void DealDamage()
        {
            damager.DealDamage(target, target.gameObject.transform.position);
        }
    }
}