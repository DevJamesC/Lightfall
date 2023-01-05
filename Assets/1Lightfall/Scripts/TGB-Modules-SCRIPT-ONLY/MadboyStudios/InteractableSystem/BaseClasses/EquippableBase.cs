using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.InteractionSystem
{
    public class EquippableBase : MonoBehaviour, IInteractable
    {

        public event Action OnUse = delegate { };
        public event Action OnFinishUse = delegate { };
        /// <summary>
        /// Use this to initalize the object and cache any varibles
        /// </summary>
        /// <param name="handler"></param>
        public virtual void Equip(HandleEquipmentDemoForAbilityEquipment handler)
        {

        }
        /// <summary>
        /// Use this to save any 
        /// </summary>
        /// <param name="handler"></param>
        public virtual void Unequip(HandleEquipmentDemoForAbilityEquipment handler)
        {

        }

        public virtual void Use()
        {
            OnUseInvoke();
        }

        public void OnUseInvoke()
        {
            OnUse.Invoke();
        }

        public void OnFinishUseInvoke()
        {
            OnFinishUse.Invoke();
        }

    }
}
