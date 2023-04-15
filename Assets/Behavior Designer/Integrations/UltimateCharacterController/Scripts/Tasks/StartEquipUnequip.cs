/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace BehaviorDesigner.Runtime.Tasks.UltimateCharacterController
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using UnityEngine;

    [TaskDescription("Tries to start the equip unequip ability.")]
    [TaskCategory("Ultimate Character Controller")]
    [TaskIcon("Assets/Behavior Designer/Integrations/UltimateCharacterController/Editor/Icon.png")]
    [HelpURL("https://www.opsive.com/support/documentation/behavior-designer/integrations/opsive-character-controllers/")]
    public class StartEquipUnequip : Action
    {
        [Tooltip("A reference to the agent. If null it will be retrieved from the current GameObject.")]
        public SharedGameObject m_TargetGameObject;
        [Tooltip("The category that the ability should respond to.")]
        [ItemSetCategoryDrawer] public SharedUInt m_CategoryID;
        [Tooltip("The ItemSet index that should be equipped or unequipped.")]
        public SharedInt m_ItemSetIndex;

        private EquipUnequip m_EquipUnequip;

        private GameObject m_PrevTarget;
        private UltimateCharacterLocomotion m_CharacterLocomotion;

        /// <summary>
        /// Retrieves the equip unequip ability.
        /// </summary>
        public override void OnStart()
        {
            var target = GetDefaultGameObject(m_TargetGameObject.Value);
            if (target != m_PrevTarget) {
                m_CharacterLocomotion = target.GetCachedComponent<UltimateCharacterLocomotion>();
                // Find the specified ability.
                var abilities = m_CharacterLocomotion.GetAbilities<EquipUnequip>();
                // The category ID must match.
                for (int i = 0; i < abilities.Length; ++i) {
                    if (abilities[i].ItemSetCategoryID == m_CategoryID.Value) {
                        m_EquipUnequip = abilities[i];
                        break;
                    }
                }
                if (m_EquipUnequip == null) {
                    // If the EquipUnequip ability can't be found but there is only one EquipUnequip ability added to the character then use that ability.
                    if (abilities.Length == 1) {
                        m_EquipUnequip = abilities[0];
                    } else {
                        Debug.LogWarning($"Error: Unable to find a Equip Unequip ability with category id {m_CategoryID.Value}.");
                        return;
                    }
                }
                m_PrevTarget = target;
            }
        }

        /// <summary>
        /// Tries to start or stop the use of the current item.
        /// </summary>
        /// <returns>Success if the item was used.</returns>
        public override TaskStatus OnUpdate()
        {
            if (m_EquipUnequip == null) {
                return TaskStatus.Failure;
            }

            // The EquipUnequip ability has been found - start the equip or unequip.
            return m_EquipUnequip.StartEquipUnequip(m_ItemSetIndex.Value) ? TaskStatus.Success : TaskStatus.Failure;
        }

        /// <summary>
        /// Resets the objects back to their default values.
        /// </summary>
        public override void OnReset()
        {
            m_TargetGameObject = null;
            m_CategoryID = 0;
            m_ItemSetIndex = 0;
        }
    }
}