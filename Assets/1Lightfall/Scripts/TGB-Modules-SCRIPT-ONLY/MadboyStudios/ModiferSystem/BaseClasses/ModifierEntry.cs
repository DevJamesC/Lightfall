using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace MBS.ModifierSystem
{
    [Serializable]
    public class ModifierEntry
    {

        [HideInInspector]
        public string Name;
        [InfoBox("$Name", InfoMessageType.None)]
        public float InitalDuration;
        public float RemainingDuration;
        public IOrigin Origin;
        public ModifierHandler Target;
        public IModifierEffect Effect;
        public EffectStateData EffectStateData;
        public bool HasDurationRemaining { get => !IsDurationExpired(); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="effect"></param>
        /// <param name="duration">A duration of 0 means infinite duration. A duration of any negative means it'll expire immediately</param>
        public ModifierEntry(IOrigin origin, ModifierHandler target, IModifierEffect effect, string name)
        {
            Name = $"{name} {effect.ToString()}\nFrom: {origin.gameObject.name}";
            InitalDuration = effect.Duration;
            RemainingDuration = InitalDuration;
            Origin = origin;
            Target = target;
            Effect = effect;
            EffectStateData = null;
        }

        public void EffectUpdate()
        {
            Effect.EffectUpdate(this);

            if (InitalDuration != 0)
                RemainingDuration -= Time.deltaTime;
        }

        private bool IsDurationExpired()
        {
            if (InitalDuration == 0)
                return false;

            if (RemainingDuration <= 0 || InitalDuration < 0)
                return true;

            return false;
        }

        /// <summary>
        /// Or just set InitalDuration or RemainingDuration to a negative number.
        /// </summary>
        public void RemoveModifier()
        {
            InitalDuration = -1;
        }
    }
}
