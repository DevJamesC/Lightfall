using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MBS.AbilitySystem
{
    public class UpgradeDetailButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        public Color UpgradedColor = Color.blue;
        public Color NotUpgradedColor = Color.gray;
        public Color LockedColor = Color.black;

        public event Action OnSelected = delegate { };
        public event Action OnDeselected = delegate { };

        [HideInInspector]
        public AbilityAndUpgradePair abilityUpgradePair;
        [HideInInspector]
        public string upgradeIndex;//which upgrade this button is supposed to represent
        [HideInInspector]
        public Sprite outlineGraphic;
        [HideInInspector]
        public Sprite centerGraphic;
        [SerializeField]
        private Image backgroundImage;
        [SerializeField]
        private Image centerImage;
        public Button Button { get; protected set; }

        //when not selected, needs to graphically show if the upgrade is aquired or not
        //when selected/ hovered(highlighted) needs to display info about the ability

        public void Initalize(bool withDelegates = true)
        {
            bool isUpgraded = CheckIfUpgraded();
            //background image
            if (isUpgraded)
                backgroundImage.color = UpgradedColor;
            else if (abilityUpgradePair.Upgrades.CanUpgrade(upgradeIndex))
                backgroundImage.color = NotUpgradedColor;
            else
                backgroundImage.color = LockedColor;

            backgroundImage.sprite = outlineGraphic;

            //center/ foreground image
            if (centerGraphic != null)
            {
                centerImage.sprite = centerGraphic;
                Color color = centerImage.color;
                color.a = 1;
                centerImage.color = color;
                //move background image to sit in the middle of the outline
                Vector3 pos;
                float changeAmount = withDelegates ? 15 : 0;
                switch (upgradeIndex)
                {

                    case "1":
                        pos = centerImage.rectTransform.position;
                        pos.y += changeAmount;
                        centerImage.rectTransform.position = pos;
                        break;
                    case "5a":
                        pos = centerImage.rectTransform.position;
                        pos.y -= changeAmount;
                        centerImage.rectTransform.position = pos;
                        break;
                    case "5b":
                        pos = centerImage.rectTransform.position;
                        pos.y -= changeAmount;
                        centerImage.rectTransform.position = pos;
                        break;
                }
            }

            //add listener to button to upgrade ability
            if (withDelegates)
            {
                Button = GetComponent<Button>();
                Button.onClick.AddListener(() => { TryUpgrade(); });
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);

        }
        public void OnPointerExit(PointerEventData eventData)
        {

        }

        public void OnSelect(BaseEventData eventData)
        {
            OnSelected.Invoke();

        }

        public void OnDeselect(BaseEventData eventData)
        {
            OnDeselected.Invoke();
        }

        private bool CheckIfUpgraded()
        {
            switch (upgradeIndex)
            {
                case "0":
                    return abilityUpgradePair.Upgrades.AbilityUnlocked;
                case "1":
                    return abilityUpgradePair.Upgrades.Upgrade1;
                case "2":
                    return abilityUpgradePair.Upgrades.Upgrade2;
                case "3a":
                    return abilityUpgradePair.Upgrades.Upgrade3a;
                case "3b":
                    return abilityUpgradePair.Upgrades.Upgrade3b;
                case "4a":
                    return abilityUpgradePair.Upgrades.Upgrade4a;
                case "4b":
                    return abilityUpgradePair.Upgrades.Upgrade4b;
                case "5a":
                    return abilityUpgradePair.Upgrades.Upgrade5a;
                case "5b":
                    return abilityUpgradePair.Upgrades.Upgrade5b;
                default:
                    return false;
            }
        }

        private void TryUpgrade()
        {
            //need to integrate with a "skill point" system to see if we can afford the upgrade

            //Tries to upgrade the ability (checks to make sure we do not have the ability unlocked already before upgrading)
            abilityUpgradePair.Upgrades.TryUpgrade(upgradeIndex);


        }


    }
}