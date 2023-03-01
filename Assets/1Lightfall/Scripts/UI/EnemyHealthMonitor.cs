using Opsive.Shared.Camera;
using Opsive.Shared.Events;
using Opsive.Shared.Game;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Items;
using Opsive.UltimateCharacterController.Traits;
using Opsive.UltimateCharacterController.UI;
using Opsive.UltimateCharacterController.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Opsive.UltimateCharacterController.Utility.UnityEngineUtility;

namespace MBS.Lightfall
{
    public class EnemyHealthMonitor : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider shieldSlider;
        [SerializeField] private Color healthColor;
        [SerializeField] private Color armorColor;
        [SerializeField] private Color shieldColor;

        private Transform currentTarget;
        private AttributeManager currentAttributeManager;
        private Attribute currentHealth;
        private Attribute currentArmor;
        private Attribute currentShield;
        private Image healthFillRectImage;
        private Image shieldFillRectImage;

        private void Start()
        {
            CrosshairsMonitor crosshairMonitor = transform.parent.GetComponentInChildren<CrosshairsMonitor>(true);
            if (crosshairMonitor != null)
                crosshairMonitor.OnAquiredTarget += CrosshairMonitor_OnAquiredTarget;
            else
                Debug.LogWarning("Enemy Health Monitor requires a Crosshair to detect when an enemy is being pointed at");

            healthSlider.gameObject.SetActive(false);
            shieldSlider.gameObject.SetActive(false);
            healthFillRectImage = healthSlider.fillRect.GetComponent<Image>();
            shieldFillRectImage = shieldSlider.fillRect.GetComponent<Image>();
            if (shieldFillRectImage != null)
                shieldFillRectImage.color = shieldColor;
        }

        private void CrosshairMonitor_OnAquiredTarget(Transform target)
        {
            if (target == currentTarget)
                return;

            if (currentTarget != null)
            {
                EventHandler.UnregisterEvent<Attribute>(currentAttributeManager.gameObject, "OnAttributeUpdateValue", OnUpdateValue);
                if (target == null)
                {
                    currentTarget = null;
                    healthSlider.gameObject.SetActive(false);
                    shieldSlider.gameObject.SetActive(false);
                    return;
                }
            }

            currentTarget = target;
            currentAttributeManager = currentTarget.GetComponent<AttributeManager>();
            if (currentAttributeManager == null)
                return;

            currentHealth = currentAttributeManager.GetAttribute("Health");
            currentArmor = currentAttributeManager.GetAttribute("Armor");
            currentShield = currentAttributeManager.GetAttribute("Shield");
            if (healthFillRectImage != null)
                healthFillRectImage.color = healthColor;
            if (currentHealth != null)
                OnUpdateValue(currentHealth);
            if (currentArmor != null)
                OnUpdateValue(currentArmor);
            if (currentShield != null)
                OnUpdateValue(currentShield);

            EventHandler.RegisterEvent<Attribute>(currentAttributeManager.gameObject, "OnAttributeUpdateValue", OnUpdateValue);
        }

        /// <summary>
        /// The attribute's value has been updated.
        /// </summary>
        /// <param name="attribute">The attribute that was updated.</param>
        protected virtual void OnUpdateValue(Attribute attribute)
        {
            if (attribute == currentHealth)
            {
                healthSlider.value = (currentHealth.Value - currentHealth.MinValue) / (currentHealth.MaxValue - currentHealth.MinValue);
                healthSlider.gameObject.SetActive(healthSlider.value > 0);

            }
            if (attribute == currentShield)
            {
                shieldSlider.value = (currentShield.Value - currentShield.MinValue) / (currentShield.MaxValue - currentShield.MinValue);
                healthSlider.gameObject.SetActive(healthSlider.value > 0);
            }
            if (attribute == currentArmor)
            {
                if (healthFillRectImage != null)
                    healthFillRectImage.color = currentArmor.Value > 0 ? armorColor : healthColor;
            }
        }
    }
}
