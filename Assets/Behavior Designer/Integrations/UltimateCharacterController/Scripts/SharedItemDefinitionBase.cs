using Opsive.Shared.Inventory;

namespace BehaviorDesigner.Runtime.Tasks.UltimateCharacterController
{
    /// <summary>
    /// Creates a SharedVariable for the ItemType class.
    /// </summary>
    [System.Serializable]
    public class SharedItemDefinitionBase : SharedVariable<ItemDefinitionBase>
    {
        public static implicit operator SharedItemDefinitionBase(ItemDefinitionBase value) { var sharedVariable = new SharedItemDefinitionBase(); sharedVariable.SetValue(value); return sharedVariable; }
    }
}