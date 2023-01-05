using MBS.DamageSystem;
using MBS.StatsAndTags;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public class ModifierHandler : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        private List<ModifierEntry> modifierEntries;

        private Dictionary<StatName, DynamicStatModifier> dynamicStatModifiers;

        [SerializeField, ReadOnly]
        private List<string> dyamicStatListDisplay;

        /// <summary>
        /// This should be called by damage dealers to apply any pre damage logic to DamageData.PreDamageEffects
        /// </summary>
        public event Action<DamageData, IDamageable> OnPreDamageEffects = delegate { };

        private void Awake()
        {
            Setup();
        }

        public ModifierEntry AddEntry(IOrigin origin, IModifierEffect effect, string modifierName)
        {
            ModifierEntry newEntry = new ModifierEntry(origin, this, effect, modifierName);
            modifierEntries.Add(newEntry);
            newEntry.Effect.EffectActivated(newEntry);
            return newEntry;
        }

        //RemoveEntry used automatically when an entry expires, but can be used by "clearing" modifiers to do stuff like remove poison, or remove all buffs from an enemy
        public void RemoveEntry(ModifierEntry entry)
        {
            if (!modifierEntries.Contains(entry))
                return;

            entry.Effect.EffectRemoved(entry);
            modifierEntries.Remove(entry);
        }

        public List<ModifierEntry> GetModifierEntries(IModifierEffect effect)
        {
            List<ModifierEntry> returnVal = new List<ModifierEntry>();
            foreach (ModifierEntry entry in modifierEntries)
            {
                if (entry.Effect == effect)
                    returnVal.Add(entry);
            }
            return returnVal;
        }

        public List<ModifierEntry> GetModifierEntries<T>() where T : IModifierEffect
        {
            List<ModifierEntry> returnVal = new List<ModifierEntry>();
            foreach (ModifierEntry entry in modifierEntries)
            {
                Type entryType = entry.Effect.GetType();
                if (entryType == typeof(T))
                    returnVal.Add(entry);
                else if (entryType.IsSubclassOf(typeof(T)))
                    returnVal.Add(entry);
            }
            return returnVal;
        }

        /// <summary>
        /// This returns a stat modifier for another to change its value, and subscribe to its onValueChanged event.
        /// </summary>
        /// <param name="name"></param>
        public DynamicStatModifier GetStatModifier(StatName stat)
        {
            ValidateStatModifier(stat);
            return dynamicStatModifiers[stat];
        }

        /// <summary>
        /// This returns the value of a stat modifier
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public float GetStatModifierValue(StatName stat)
        {
            ValidateStatModifier(stat);
            return dynamicStatModifiers[stat].Value; //To set a special case, such as "damage is capped at 300%", go to DynamicStatModifiers.cs GetValue()
        }

        internal void ApplyPreDamageProcessors(DamageData damageData, IDamageable damageableHit)
        {
            OnPreDamageEffects.Invoke(damageData, damageableHit);
        }

        /// <summary>
        /// This adds or subtracts from the current stat modifier and fires the OnValueChanged event. This does NOT "Set" the modifier value!
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void ChangeStatModifierValue(StatName stat, float value, StatModifierType statType = StatModifierType.External)
        {
            GetStatModifier(stat).ChangeValue(value, statType);

            dyamicStatListDisplay.Clear();
            foreach (var keyVal in dynamicStatModifiers)
            {
                dyamicStatListDisplay.Add($"{keyVal.Key}: {keyVal.Value.Value}");
            }

        }

        // Update is called once per frame
        private void Update()
        {
            UpdateEntries();
        }

        private void UpdateEntries()
        {
            List<ModifierEntry> expiredEntries = null;

            foreach (ModifierEntry entry in modifierEntries)
            {
                if (!entry.HasDurationRemaining)
                {
                    if (expiredEntries == null)
                        expiredEntries = new List<ModifierEntry>();

                    expiredEntries.Add(entry);

                    continue;
                }

                entry.EffectUpdate();
            }

            if (expiredEntries != null)
                RemoveExpiredModifiers(expiredEntries);
        }

        private void RemoveExpiredModifiers(List<ModifierEntry> expiredEntries)
        {
            int count = expiredEntries.Count;
            for (int i = 0; i < count; i++)
            {
                RemoveEntry(expiredEntries[i]);
            }

            if (count > 0)
                expiredEntries.Clear();
        }

        /// <summary>
        /// This makes sure that the stat which is being modified actually has an entry in the dictonary. Lazy instanciation.
        /// </summary>
        /// <param name="stat"></param>
        private void ValidateStatModifier(StatName stat)
        {
            if (dynamicStatModifiers == null)
                Setup();

            if (dynamicStatModifiers.ContainsKey(stat))
                return;

            dynamicStatModifiers.Add(stat, new DynamicStatModifier(stat));
        }

        private void Setup()
        {
            if (modifierEntries == null)
                modifierEntries = new List<ModifierEntry>();
            if (dynamicStatModifiers == null)
                dynamicStatModifiers = new Dictionary<StatName, DynamicStatModifier>();
            if (dyamicStatListDisplay == null)
                dyamicStatListDisplay = new List<string>();
        }
    }
}
