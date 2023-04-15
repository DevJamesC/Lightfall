
namespace Opsive.UltimateCharacterController.Demo.BehaviorDesigner
{
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Traits;
    using UnityEngine;

    /// <summary>
    /// Helper functions for the demo behavior tree.
    /// </summary>
    public class DemoAgent : MonoBehaviour
    {
        [Tooltip("The ItemDefinition that should be used as ammo.")]
        [SerializeField] protected ItemDefinitionBase m_Ammo;

        private Health m_Health;
        private Inventory.InventoryBase m_Inventory;
        private IItemIdentifier m_ItemIdentifier;

        // Expose the health and ammo via a Behavior Designer property mapping.
        public float Health { get { return m_Health.HealthValue; } }
        public int Ammo { get { return m_ItemIdentifier != null ? m_Inventory.GetItemIdentifierAmount(m_ItemIdentifier) : int.MaxValue; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Start()
        {
            m_Health = GetComponent<Health>();
            m_Inventory = GetComponent<Inventory.InventoryBase>();
            m_ItemIdentifier = m_Ammo.CreateItemIdentifier();
        }
    }
}