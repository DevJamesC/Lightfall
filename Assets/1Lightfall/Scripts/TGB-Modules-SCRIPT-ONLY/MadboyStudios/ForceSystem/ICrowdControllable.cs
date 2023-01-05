using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ForceSystem
{
    public interface ICrowdControllable
    {
        public CrowdControlType ResistantToCrowndControl { get; }
        //public void HandleCrowdControl(CrowdControlType crowdControlType, AbilityEffectBase abilityEffect);
        //public void HandleCrowdControl(CrowdControlType crowdControlType, ModifierEntry modifierEffect);
    }

    public enum CrowdControlType
    {
        None,
        SoftCC,
        HardCC
    }
}
