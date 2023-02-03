using Opsive.Shared.Game;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace MBS.Lightfall
{
    public class MBSAbilityMonitor : CharacterMonitor
    {
        [SerializeField] private GameObject abilityUIDisplayPrefab;

        private UltimateCharacterLocomotion m_locomotion;
        private List<LightfallAbilityBase> m_lightfallAbilities;
        private List<AbilityMonitorUIData> abilityUIDisplayObjects;

        private bool delegatesPassedFlag;
        private bool waitingForSchedule;

        protected override void Awake()
        {
            base.Awake();
            abilityUIDisplayObjects = new List<AbilityMonitorUIData>();
            delegatesPassedFlag = false;
            waitingForSchedule = false;
        }

        protected override void OnAttachCharacter(GameObject character)
        {
            if (waitingForSchedule)
                return;

            waitingForSchedule = true;
            Scheduler.Schedule(.5f, () =>
            {
                waitingForSchedule = false;
                //remove any ability UI display objects in play
                if (m_Character != null)
                {
                    foreach (var obj in abilityUIDisplayObjects)
                    {
                        if (obj != null)
                            Destroy(obj.gameObject);
                    }
                }

                base.OnAttachCharacter(character);
                if (m_Character == null)
                    return;

                m_locomotion = m_Character.GetComponent<UltimateCharacterLocomotion>();
                if (m_locomotion == null)
                    return;

                m_lightfallAbilities = new List<LightfallAbilityBase>();

                //get activatable abilities
                foreach (var ability in m_locomotion.Abilities)
                {
                    LightfallAbilityBase lightfallAbility = ability as LightfallAbilityBase;
                    if (lightfallAbility != null)
                    {
                        if (lightfallAbility.UpgradeData == null)
                            continue;

                        if (lightfallAbility.abilitySO.AbilityType != AbilitySystem.AbilityType.Passive && lightfallAbility.UpgradeData.AbilityUnlocked)
                            m_lightfallAbilities.Add(lightfallAbility);

                        if (!delegatesPassedFlag)
                        {
                            lightfallAbility.UpgradeData.OnAbilityUpgrade += () => { OnAttachCharacter(character); };
                        }
                    }
                }
                delegatesPassedFlag = true;

                //order abilities
                List<LightfallAbilityBase> lightfallAbilitiesOrdered = new List<LightfallAbilityBase>();
                List<LightfallAbilityBase> tempLeft = new List<LightfallAbilityBase>();
                List<LightfallAbilityBase> tempCenter = new List<LightfallAbilityBase>();
                List<LightfallAbilityBase> tempRight = new List<LightfallAbilityBase>();
                foreach (var ability in m_lightfallAbilities)
                {
                    switch (ability.UIDisplayLocation)
                    {
                        case LightfallAbilityBase.UIDisplayLocationType.Right: tempRight.Add(ability); break;
                        case LightfallAbilityBase.UIDisplayLocationType.Any:
                        case LightfallAbilityBase.UIDisplayLocationType.Center: tempCenter.Add(ability); break;
                        case LightfallAbilityBase.UIDisplayLocationType.Left: tempLeft.Add(ability); break;

                    }
                }
                lightfallAbilitiesOrdered.AddRange(tempLeft);
                lightfallAbilitiesOrdered.AddRange(tempCenter);
                lightfallAbilitiesOrdered.AddRange(tempRight);

                //setup new ability UI display objects
                foreach (var ability in lightfallAbilitiesOrdered)
                {
                    AbilityMonitorUIData uiObjectData = GameObject.Instantiate(abilityUIDisplayPrefab, transform).GetComponent<AbilityMonitorUIData>();
                    if (uiObjectData == null)
                    {
                        Debug.Log($"Ability UI monitor prefab {abilityUIDisplayPrefab.name} does not have AbilityMonitorUIData at the root.");
                        Destroy(uiObjectData.gameObject);
                        return;
                    }
                    uiObjectData.gameObject.name += " " + ability.abilitySO.name;

                    abilityUIDisplayObjects.Add(uiObjectData);
                    uiObjectData.Image.sprite = ability.abilitySO.Icon;
                    uiObjectData.Text.text = ability.sharedChargeRemaining.ToString();
                    uiObjectData.Text.enabled = ability.sharedChargeMax > 0;

                    uiObjectData.OnAttachAbility(ability);

                }

            });

        }

        private void OnDisable()
        {
            delegatesPassedFlag = false;
        }
    }


}
