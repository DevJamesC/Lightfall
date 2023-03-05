using MBS.ModifierSystem;
using MBS.StatsAndTags;
using Opsive.UltimateCharacterController.Character;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    //This script changes the movespeed of the character based on the movespeed stat
    public class MoveSpeedStatHandler : MonoBehaviour
    {
        private UltimateCharacterLocomotion m_locomotion;
        private ModifierHandler m_modifierHandler;

        private Vector3 m_baseSpeed;

        // Start is called before the first frame update
        void Start()
        {
            m_locomotion = GetComponent<UltimateCharacterLocomotion>();
            m_modifierHandler = GetComponent<ModifierHandler>();
            m_baseSpeed = m_locomotion.MotorAcceleration;
            m_modifierHandler.GetStatModifier(StatName.MovementSpeed).OnValueChanged += MoveSpeedStatHandler_OnValueChanged;
        }

        private void MoveSpeedStatHandler_OnValueChanged(float newValue)
        {
            m_locomotion.MotorAcceleration = m_baseSpeed * newValue;
        }

        private void OnDestroy()
        {
            m_modifierHandler.GetStatModifier(StatName.MovementSpeed).OnValueChanged -= MoveSpeedStatHandler_OnValueChanged;

        }
    }
}
