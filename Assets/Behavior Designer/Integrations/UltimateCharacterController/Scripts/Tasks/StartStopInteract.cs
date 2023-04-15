using Opsive.Shared.Game;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.Traits;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.UltimateCharacterController
{
    [TaskDescription("Tries to start or stop the Interact ability.")]
    [TaskCategory("Ultimate Character Controller")]
    [TaskIcon("Assets/Behavior Designer/Integrations/UltimateCharacterController/Editor/Icon.png")]
    [HelpURL("https://www.opsive.com/support/documentation/behavior-designer/integrations/opsive-character-controllers/")]
    public class StartStopInteract : StartStopAbility
    {
        [Tooltip("A reference to the GameObject that the character should interact with.")]
        public SharedGameObject m_InteractableGameObject;

        /// <summary>
        /// Retrieves the specified ability.
        /// </summary>
        public override void OnStart()
        {
            base.OnStart();

            var interact = m_Ability as Interact;
            if (interact == null) {
                Debug.LogError("Error: The found ability is not an Interact ability.");
                return;
            }

            if (m_Start.Value) {
                interact.Interactable = m_InteractableGameObject.Value.GetCachedComponent<Interactable>();
                if (interact.Interactable == null) {
                    Debug.LogWarning("Warning: Unable to find the Interactable component on " + m_InteractableGameObject.Value);
                }
            }
        }

        /// <summary>
        /// Resets the objects back to their default values.
        /// </summary>
        public override void OnReset()
        {
            base.OnReset();
            m_InteractableGameObject = null;
        }
    }
}