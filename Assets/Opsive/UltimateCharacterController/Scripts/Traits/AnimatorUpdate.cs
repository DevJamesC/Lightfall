/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Traits
{
    using Opsive.UltimateCharacterController.Game;
    using UnityEngine;

    /// <summary>
    /// Updates the Animator component at a fixed delta time.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimatorUpdate : MonoBehaviour, IKinematicObject
    {
        private Animator m_Animator;

        /// <summary>
        /// Cache the componetn references.
        /// </summary>
        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_Animator.enabled = false;
        }

        /// <summary>
        /// Registers the object with the Simulation Manager.
        /// </summary>
        private void OnEnable()
        {
            SimulationManager.RegisterKinematicObject(this);
        }

        /// <summary>
        /// Updates the Animator at a fixed delta time.
        /// </summary>
        public void Move()
        {
            m_Animator.Update(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Unregisters the object with the Simulation Manager.
        /// </summary>
        private void OnDisable()
        {
            SimulationManager.UnregisterKinematicObject(this);
        }
    }
}