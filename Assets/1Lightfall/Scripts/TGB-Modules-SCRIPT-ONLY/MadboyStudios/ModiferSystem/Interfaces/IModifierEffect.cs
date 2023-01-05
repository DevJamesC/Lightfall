using MBS.StatsAndTags;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public interface IModifierEffect
    {
        public float Duration { get; }
        public List<Tag> Tags { get; }

        public void EffectActivated(ModifierEntry targetEntry);
        public void EffectUpdate(ModifierEntry targetEntry);
        public void EffectRemoved(ModifierEntry targetEntry);

        //public void ApplyAbilitySystemUpgradesToEntries(ModifierEntry entryForEffect, AbilityWrapperBase abilityWrapper);
    }
}