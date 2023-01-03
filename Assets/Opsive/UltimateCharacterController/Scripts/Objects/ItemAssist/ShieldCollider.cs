/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects.ItemAssist
{
    using Opsive.Shared.Events;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Character;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// The ShieldCollider component specifies the object that acts as a collider for the shield.
    /// </summary>
    public class ShieldCollider : MonoBehaviour
    {
        [Tooltip("A reference to the Shield item action.")]
        [SerializeField] protected ShieldAction m_ShieldAction;

        [Shared.Utility.NonSerialized] public ShieldAction ShieldAction { get { return m_ShieldAction; } set { m_ShieldAction = value; } }

        private bool m_FirstPersonPerspective;
        private Collider m_Collider;
        private GameObject m_Character;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Start()
        {
            if (m_ShieldAction == null) {
                Debug.LogError("Error: The shield is not assigned. Ensure the shield is created from the Item Manager.", this);
                return;
            }
            
            var firstPersonPerspectiveItem = m_ShieldAction.CharacterItem.FirstPersonPerspectiveItem?.GetVisibleObject()?.transform;
            if (firstPersonPerspectiveItem != null && (transform == firstPersonPerspectiveItem || transform.IsChildOf(firstPersonPerspectiveItem))) {
                m_FirstPersonPerspective = true;
            } else {
                m_FirstPersonPerspective = false;
            }
            
            var CharacterLocomotion = m_ShieldAction.gameObject.GetComponentInParent<UltimateCharacterLocomotion>();
            m_Character = CharacterLocomotion.gameObject;
            m_Collider = GetComponent<Collider>();
            m_Collider.enabled = CharacterLocomotion.FirstPersonPerspective == m_FirstPersonPerspective;

            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
        }

        /// <summary>
        /// The camera perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the camera in a first person view?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            // The collider should only be enabled for the corresponding perspective.
            m_Collider.enabled = m_FirstPersonPerspective == firstPersonPerspective;
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (m_Character == null) {
                return;
            }

            EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
        }
    }
}