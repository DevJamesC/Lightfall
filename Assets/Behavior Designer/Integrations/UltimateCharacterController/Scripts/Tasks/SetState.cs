using Opsive.Shared.StateSystem;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.UltimateCharacterController
{
    [TaskDescription("Sets the set on the target GameObject.")]
    [TaskCategory("Ultimate Character Controller")]
    [TaskIcon("Assets/Behavior Designer/Integrations/UltimateCharacterController/Editor/Icon.png")]
    [HelpURL("https://www.opsive.com/support/documentation/behavior-designer/integrations/opsive-character-controllers/")]
    public class SetState : Action
    {
        [Tooltip("A reference to the agent. If null it will be retrieved from the current GameObject.")]
        public SharedGameObject m_TargetGameObject;
        [Tooltip("The name of the state that should be set.")]
        public SharedString m_StateName;
        [Tooltip("Should the state name be activated?")]
        public SharedBool m_ActivateState = true;

        private GameObject m_Target;

        /// <summary>
        /// Retrieves the target.
        /// </summary>
        public override void OnStart()
        {
            var target = GetDefaultGameObject(m_TargetGameObject.Value);
            if (target != m_Target) {
                m_Target = target;
            }
        }

        /// <summary>
        /// Tries to set the state.
        /// </summary>
        /// <returns>Success if the state was set.</returns>
        public override TaskStatus OnUpdate()
        {
            if (m_Target == null) {
                return TaskStatus.Failure;
            }

            StateManager.SetState(m_Target, m_StateName.Value, m_ActivateState.Value);
            return TaskStatus.Success;
        }

        /// <summary>
        /// Resets the objects back to their default values.
        /// </summary>
        public override void OnReset()
        {
            m_TargetGameObject = null;
            m_StateName = string.Empty;
            m_ActivateState = true;
        }
    }
}