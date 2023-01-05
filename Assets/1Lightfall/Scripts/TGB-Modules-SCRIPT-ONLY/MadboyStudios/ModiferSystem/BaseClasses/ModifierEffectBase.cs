using MBS.AbilitySystem;
using MBS.ForceSystem;
using MBS.StatsAndTags;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    //NOTE: ModifierEffects are STATELESS... any state should be handled through EffectStateData
    [Serializable]
    public abstract class ModifierEffectBase : IModifierEffect
    {
        [SerializeField]
        protected Sprite UIDisplayIcon;
        [SerializeField]
        protected CrowdControlType crowdControlType = CrowdControlType.None;
        public CrowdControlType CrowdControlType { get => crowdControlType; }
        [SerializeField]
        [BoxGroup("Lists", false), Tooltip("The tags associated with this ability")]
        protected List<Tag> tags = new List<Tag>();
        public List<Tag> Tags { get => tags; }

        [SerializeField]
        [Title("$subclassName", null, TitleAlignments.Centered), PropertyOrder(-1), Tooltip("Set duration to 0 for an infinite duration.")]
        protected float duration;
        public virtual float Duration { get => duration; }
        public event Action OnEffectActivated = delegate { };
        public event Action OnEffectRemoved = delegate { };
        [SerializeReference]
        [BoxGroup("Lists"), Tooltip("The FX to activate on the target. These will be automatically deactivated when the effect is removed.")]
        protected List<ModifierFXBase> onActivatedFX = new List<ModifierFXBase>();


        private string subclassName;//used for GUI

        public virtual void EffectActivated(ModifierEntry targetEntry)
        {
            for (int i = 0; i < onActivatedFX.Count; i++)
            {
                onActivatedFX[i].Activate(targetEntry);
            }
        }

        public virtual void EffectRemoved(ModifierEntry targetEntry)
        {
            for (int i = 0; i < onActivatedFX.Count; i++)
            {
                onActivatedFX[i].Deactivate(targetEntry);
            }

        }

        public virtual void EffectUpdate(ModifierEntry targetEntry)
        {
        }

        public virtual void OnValidate()
        {
            subclassName = this.GetType().ToString();
        }

        public virtual List<AbilityUIStat> GetStats()
        {
            Debug.Log($"You are trying to get UI stats for modifier Effect {GetType()}, but it is unhandled");
            return new List<AbilityUIStat>();
        }

        public virtual void ApplyAbilitySystemUpgradesToEntries(ModifierEntry entryForEffect, AbilityWrapperBase abilityWrapper)
        {
            Debug.LogWarning($"you are trying to apply ability system upgrades to {this} from {abilityWrapper.AbilityBase.name}, but it is unhandled.");
        }
    }
}
