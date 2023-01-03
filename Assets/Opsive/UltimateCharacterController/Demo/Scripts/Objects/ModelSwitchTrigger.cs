/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Object
{
    using Opsive.UltimateCharacterController.Demo.Zones;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Switches the character model to the next model when the character enters the trigger.
    /// </summary>
    public class ModelSwitchTrigger : MonoBehaviour, IZoneTrigger
    {
        [Tooltip("The LayerMask of the character.")]
        [SerializeField] protected LayerMask m_LayerMask = 1 << LayerManager.Character;

        private GameObject m_ActiveCharacter;

        /// <summary>
        /// An object has entered the trigger.
        /// </summary>
        /// <param name="other">The object that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask) || m_ActiveCharacter != null) {
                return;
            }

            var modelManager = other.GetComponentInParent<Character.ModelManager>();
            if (modelManager == null) {
                return;
            }

            modelManager.ModelIndexMap.TryGetValue(modelManager.ActiveModel, out var modelIndex);
            modelIndex = (modelIndex + 1) % modelManager.AvailableModels.Length;
            modelManager.ActiveModel = modelManager.AvailableModels[modelIndex];

            m_ActiveCharacter = modelManager.gameObject;
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The object that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask) || m_ActiveCharacter == null) {
                return;
            }

            var modelManager = other.GetComponentInParent<Character.ModelManager>();
            if (modelManager == null) {
                return;
            }

            if (modelManager.gameObject == m_ActiveCharacter) {
                m_ActiveCharacter = null;
            }
        }

        /// <summary>
        /// Resets the zone after the character exits.
        /// </summary>
        /// <param name="character">The character that exited the zone.</param>
        public void ExitZone(GameObject character)
        {
            // Ensure the first character is selected.
            var modelManager = character.GetComponent<Character.ModelManager>();
            modelManager.ActiveModel = modelManager.AvailableModels[0];
        }
    }
}