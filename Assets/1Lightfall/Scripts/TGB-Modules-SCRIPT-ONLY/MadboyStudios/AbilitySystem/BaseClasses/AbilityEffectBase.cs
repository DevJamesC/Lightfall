using MBS.ForceSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.AbilitySystem
{
    [Serializable]
    public class AbilityEffectBase : IShallowCloneable<AbilityEffectBase>
    {
        //These may be used by upgrades to apply initalization or onUpdate effects... If completely unused, may remove them.
        public List<AbilityEffectOnInitalizedLogicBase> AbilityEffectOnInitalizedLogics { get; set; }
        public List<AbilityEffectOnUpdateLogicBase> abilityEffectOnUpdateLogics { get; set; }


        [SerializeField, ReadOnly, Title("$subclassName", null, TitleAlignments.Centered)]
        private string subclassName;//used for GUI

        public event Action OnEffectFinished = delegate { };
        [SerializeField]
        protected CrowdControlType crowdControlType = CrowdControlType.None;
        public CrowdControlType CrowdControlType { get => crowdControlType; }

        public virtual bool CanBeCanceled { get { return true; } }


        /// <summary>
        /// Use() is called when the ability is "used".
        /// Must call OnAbilityFinishedInvoke() to tell the abilitywrapper the effect has finished.
        /// Channel abilites should instead call OnAbilityFinishedInvoke() when they finish channeling, ie. in OnUpdate() when the channel runs out, or in UseWhileInUse()
        /// </summary>
        /// <param name="abilityWrapper"></param>
        public virtual void Use(AbilityWrapperBase abilityWrapper)
        {
            OnEffectFinished.Invoke();
        }

        /// <summary>
        /// Overriden by Channel or OnOff abilites. Used by them to cancel channels or turn off the OnOff abilities.
        /// </summary>
        /// <param name="abilityWrapper"></param>
        public virtual void UseWhileInUse(AbilityWrapperBase abilityWrapper)
        {

        }

        /// <summary>
        /// Init() is called by outside class when effect is constructed
        /// </summary>
        /// <param name="abilityWrapper"></param>
        public void Init(AbilityWrapperBase abilityWrapper)
        {
            if (AbilityEffectOnInitalizedLogics != null)
            {
                foreach (var onInitLogic in AbilityEffectOnInitalizedLogics)
                {
                    onInitLogic.OnInit(abilityWrapper);
                }
            }



            OnStart(abilityWrapper);
        }
        /// <summary>
        /// OnStart() can be overriden by child classes. This triggers when the ability is constructed
        /// </summary>
        /// <param name="abilityWrapper"></param>
        protected virtual void OnStart(AbilityWrapperBase abilityWrapper)
        {
            abilityWrapper.OnFinishUse += Dispose;
        }

        /// <summary>
        /// OnUpdate() is called every frame. OnOff or Channel abilites should probably use this more than Use()
        /// </summary>
        /// <param name="abilityWrapper"></param>
        public virtual void OnUpdate(AbilityWrapperBase abilityWrapper)
        {
            if (abilityEffectOnUpdateLogics != null)
            {
                foreach (var onUpdateLogic in abilityEffectOnUpdateLogics)
                {
                    onUpdateLogic.OnUpdate(abilityWrapper);
                }
            }
        }



        internal void OnValidate()
        {
            subclassName = this.GetType().ToString();
        }

        protected virtual void OnEffectFinishedInvoke()
        {
            OnEffectFinished.Invoke();
        }

        public virtual void CancelEffect()
        {
            //OnEffectFinished.Invoke();
        }

        public virtual List<AbilityUIStat> GetStats()
        {
            Debug.LogWarning($"You are trying to display stats for ability effect {GetType()}, but it has no override for GetStats.");
            return new List<AbilityUIStat>();
        }

        /// <summary>
        /// Effects that are passive or OnOff should override this method to impliment the undoing of their effects.
        /// Dispose is automatically called as a delegte on abilityWrapper.OnFinishUse
        /// </summary>
        /// <param name="abilityWrapperBase"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void Dispose(AbilityWrapperBase abilityWrapperBase)
        {
            OnEffectFinished.Invoke();
        }

        internal AbilityEffectBase GetShallowCopy()
        {
            return (AbilityEffectBase)MemberwiseClone();
        }
    }
}