
namespace BehaviorDesigner.Runtime.UltimateCharacterController
{
    using Opsive.Shared.Events;
    using Opsive.Shared.StateSystem;
    using UnityEngine;

    /// <summary>
    /// Automatically disables/enables the behavior tree when the character dies/respawns.
    /// </summary>
    public class BehaviorTreeAgent : StateBehavior
    {
        [Tooltip("When the agent dies should the behavior tree be paused instead of disabled?")]
        [SerializeField] protected bool m_PauseOnDeath = false;

        public bool PauseOnDeath { get { return m_PauseOnDeath; } set { m_PauseOnDeath = value; } }

        private BehaviorTree m_BehaviorTree;
        private bool m_BehaviorTreeEnabled;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_BehaviorTree = GetComponent<BehaviorTree>();

            if (m_BehaviorTree != null) {
                EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(gameObject, "OnDeath", OnDeath);
                EventHandler.RegisterEvent(gameObject, "OnRespawn", OnRespawn);
            }
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            m_BehaviorTreeEnabled = BehaviorManager.instance.IsBehaviorEnabled(m_BehaviorTree);
            if (m_BehaviorTreeEnabled) {
                m_BehaviorTree.DisableBehavior(m_PauseOnDeath);
            }
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            if (m_BehaviorTreeEnabled) {
                if (m_PauseOnDeath) {
                    BehaviorManager.instance.RestartBehavior(m_BehaviorTree);
                }
                m_BehaviorTree.EnableBehavior();
                m_BehaviorTreeEnabled = false;
            }
        }

        /// <summary>
        /// The GameObject was destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(gameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(gameObject, "OnRespawn", OnRespawn);
        }
    }
}