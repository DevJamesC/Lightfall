/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace BehaviorDesigner.Editor.UltimateCharacterController
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.UltimateCharacterController;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Editor.Managers;
    using Opsive.UltimateCharacterController.Utility.Builders;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Draws an inspector that can be used within the Inspector Manager.
    /// </summary>
    [OrderedEditorItem("Behavior Designer", 0)]
    public class BehaviorDesignerIntegration : IntegrationInspector
    {
        private GameObject m_Character;
        private VisualElement m_BuildButton;

        /// <summary>
        /// Draws the integration inspector.
        /// </summary>
        /// <summary>
        /// <param name="container">The parent VisualElement container.</param>
        public override void ShowIntegration(VisualElement container)
        {
            m_BuildButton = ManagerUtility.ShowControlBox("Agent Setup", "Sets up the character to be used with Behavior Designer.", ShowAgentSetup, "Setup Agent", SetupAgent, container, true);
            m_BuildButton.SetEnabled(IsValidCharacter());
        }

        /// <summary>
        /// Draws the agent setup fields.
        /// </summary>
        private void ShowAgentSetup(VisualElement container)
        {
            var characterField = new ObjectField("Character");
            characterField.objectType = typeof(GameObject);
            characterField.allowSceneObjects = true;
            characterField.value = m_Character;
            characterField.RegisterValueChangedCallback(c =>
            {
                m_Character = (GameObject)c.newValue;
                m_BuildButton.SetEnabled(IsValidCharacter());
            });
            container.Add(characterField);
        }

        /// <summary>
        /// Is the Character GameObject a valid character?
        /// </summary>
        private bool IsValidCharacter()
        {
            if (m_Character == null) {
                return false;
            }

            if (m_Character.GetComponent<UltimateCharacterLocomotion>() == null) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets up the agent.
        /// </summary>
        private void SetupAgent()
        {
            CharacterBuilder.AddAIAgent(m_Character);
            Opsive.Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<BehaviorTreeAgent>(m_Character);
            Opsive.Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<BehaviorTree>(m_Character);
        }
    }
}