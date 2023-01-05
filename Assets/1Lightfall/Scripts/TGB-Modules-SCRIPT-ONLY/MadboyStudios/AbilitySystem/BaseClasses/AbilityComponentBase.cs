using MBS.ModifierSystem;
using MBS.StatsAndTags;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.AbilitySystem
{
    //used as a base class for monobehaviors that hold reference to and can cast a single ability
    public class AbilityComponentBase : MonoBehaviour
    {
        [SerializeField]
        protected AbilityBase ability;
        [SerializeField, ReadOnly]
        protected AbilityState abilityState;
        [SerializeField]
        protected AbilityUpgradeProgressData upgrades;

        protected AbilityWrapperBase wrappedAbility;
        protected ModifierHandler modifierHandler;
        protected TagHandler tagHandler;
        protected virtual void Awake()
        {
            modifierHandler = GetComponent<ModifierHandler>();
            tagHandler = GetComponent<TagHandler>();
        }

        // Start is called before the first frame update
        protected virtual void Start()
        {
            SetWrappedAbility();
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            wrappedAbility.Update();
            abilityState = wrappedAbility.AbilityState;
        }

        protected void SetWrappedAbility()
        {
            if (wrappedAbility != null)
                wrappedAbility.DisposeAbilityWrapper();
            wrappedAbility = ability.GetAbilityWrapper(upgrades, gameObject, modifierHandler, tagHandler);
        }
    }
}