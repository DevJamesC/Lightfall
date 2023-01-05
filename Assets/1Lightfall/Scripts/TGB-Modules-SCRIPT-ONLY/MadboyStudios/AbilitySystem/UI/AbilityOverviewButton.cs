using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MBS.AbilitySystem
{
    public class AbilityOverviewButton : MonoBehaviour
    {
        public event Action OnInitalizingAbilityDetail = delegate { };

        [SerializeField]
        private Color UnupgradedColor;
        [SerializeField]
        private Color UpgradedColor;
        [SerializeField]
        private TextMeshProUGUI AbilityOverviewTitleDisplayPrefab;
        [SerializeField]
        private Image AbilityOverviewUpgradeDisplayPrefab;
        [SerializeField]
        private AbilityDetailUI abilityDetailUIPrefab;
        [SerializeField]
        private Sprite rootAbilitySprite;
        [SerializeField]
        private Sprite upgradeStartSprite;
        [SerializeField]
        private Sprite upgradeMiddleSprite;
        [SerializeField]
        private Sprite upgradeMiddleLeftSprite;
        [SerializeField]
        private Sprite upgradeMiddleRightSprite;
        [SerializeField]
        private Sprite upgradeEndSprite;
        [SerializeField]
        private Sprite upgradeEndLeftSprite;
        [SerializeField]
        private Sprite upgradeEndRightSprite;
        private AbilityAndUpgradePair abilityUpgradePair;

        public void Initalize(AbilityAndUpgradePair abilityUpgradePair, DisplayAbilityListMenu abilityListMenu, string abilityName, string abilityLevel)
        {
            this.abilityUpgradePair = abilityUpgradePair;
            GetComponent<Button>().onClick.AddListener(() => { InitalizeAbilityDetailUIPrefabOnClick(abilityListMenu); });

            //Instanciate objects to display ability overview
            Image rootAbility = Instantiate(AbilityOverviewUpgradeDisplayPrefab, transform);//new GameObject("AbilityRootImg", typeof(RectTransform), typeof(Image));
                                                                                            //GameObject spacer1 = new GameObject("spacer", typeof(RectTransform));
            TextMeshProUGUI abilityNameText = Instantiate(AbilityOverviewTitleDisplayPrefab, transform);//new GameObject("Abilityname", typeof(RectTransform), typeof(TextMeshProUGUI));
            abilityNameText.text = abilityName;
            //GameObject spacer2 = new GameObject("spacer", typeof(RectTransform));
            Image upgrade1 = Instantiate(AbilityOverviewUpgradeDisplayPrefab, transform);//new GameObject("Upgrade1", typeof(RectTransform), typeof(Image));
            Image upgrade2 = Instantiate(AbilityOverviewUpgradeDisplayPrefab, transform);//new GameObject("Upgrade2", typeof(RectTransform), typeof(Image));
                                                                                         //GameObject spacer3 = new GameObject("spacer", typeof(RectTransform));
            Image upgrade3 = Instantiate(AbilityOverviewUpgradeDisplayPrefab, transform);//new GameObject("Upgrade3", typeof(RectTransform), typeof(Image));
                                                                                         //GameObject spacer4 = new GameObject("spacer", typeof(RectTransform));
            Image upgrade4 = Instantiate(AbilityOverviewUpgradeDisplayPrefab, transform);//new GameObject("Upgrade4", typeof(RectTransform), typeof(Image));
            Image upgrade5 = Instantiate(AbilityOverviewUpgradeDisplayPrefab, transform);//new GameObject("Upgrade5", typeof(RectTransform), typeof(Image));

            //Configure each object
            ConfigureAbilityOverviewBlock(rootAbility, "0");

            //spacer1.transform.SetParent(transform);
            //spacer1.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 40);

            //abilityNameText.transform.SetParent(transform);
            //RectTransform abilityNameRect = abilityNameText.GetComponent<RectTransform>();
            //abilityNameRect.sizeDelta = new Vector2(150, 75);
            //TextMeshProUGUI abilityNameTMP = abilityNameText.GetComponent<TextMeshProUGUI>();
            //abilityNameTMP.text = abilityName;
            //abilityNameTMP.alignment = TextAlignmentOptions.Center;
            //abilityNameTMP.enableAutoSizing = true;
            //abilityNameTMP.fontSizeMax = 35;
            //abilityNameTMP.fontSizeMin = 25;
            //abilityNameTMP.overflowMode = TextOverflowModes.Overflow;

            //spacer2.transform.SetParent(transform);
            //spacer2.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 40);

            ConfigureAbilityOverviewBlock(upgrade1, "1");

            ConfigureAbilityOverviewBlock(upgrade2, "2");

            //spacer3.transform.SetParent(transform);
            //spacer3.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 40);

            ConfigureAbilityOverviewBlock(upgrade3, "3");

            //spacer4.transform.SetParent(transform);
            //spacer4.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 40);

            ConfigureAbilityOverviewBlock(upgrade4, "4");
            ConfigureAbilityOverviewBlock(upgrade5, "5");

        }

        private void ConfigureAbilityOverviewBlock(Image objImage, string level)
        {
            //obj.transform.SetParent(transform);

            Sprite targetSprite = null;
            Color targetColor = Color.black;
            switch (level)
            {
                case "0":
                    targetSprite = rootAbilitySprite;
                    targetColor = abilityUpgradePair.Upgrades.AbilityUnlocked ? UpgradedColor : UnupgradedColor;
                    break;
                case "1":
                    targetSprite = upgradeStartSprite;
                    targetColor = abilityUpgradePair.Upgrades.Upgrade1 ? UpgradedColor : UnupgradedColor;
                    break;
                case "2":
                    targetSprite = upgradeMiddleSprite;
                    targetColor = abilityUpgradePair.Upgrades.Upgrade2 ? UpgradedColor : UnupgradedColor;
                    break;
                case "3":
                    if ((!abilityUpgradePair.Upgrades.Upgrade3a && !abilityUpgradePair.Upgrades.Upgrade3b) || (abilityUpgradePair.Upgrades.Upgrade3a && abilityUpgradePair.Upgrades.Upgrade3b))
                    {
                        targetSprite = upgradeMiddleSprite;
                        targetColor = abilityUpgradePair.Upgrades.Upgrade3a ? UpgradedColor : UnupgradedColor;
                    }
                    else
                    {
                        targetSprite = abilityUpgradePair.Upgrades.Upgrade3a ? upgradeMiddleRightSprite : upgradeMiddleLeftSprite;
                        targetColor = UpgradedColor;
                    }
                    break;
                case "4":
                    if ((!abilityUpgradePair.Upgrades.Upgrade4a && !abilityUpgradePair.Upgrades.Upgrade4b) || (abilityUpgradePair.Upgrades.Upgrade4a && abilityUpgradePair.Upgrades.Upgrade4b))
                    {
                        targetSprite = upgradeMiddleSprite;
                        targetColor = abilityUpgradePair.Upgrades.Upgrade4a ? UpgradedColor : UnupgradedColor;
                    }
                    else
                    {
                        targetSprite = abilityUpgradePair.Upgrades.Upgrade4a ? upgradeMiddleRightSprite : upgradeMiddleLeftSprite;
                        targetColor = UpgradedColor;
                    }
                    break;
                case "5":
                    if ((!abilityUpgradePair.Upgrades.Upgrade5a && !abilityUpgradePair.Upgrades.Upgrade5b) || (abilityUpgradePair.Upgrades.Upgrade5a && abilityUpgradePair.Upgrades.Upgrade5b))
                    {
                        targetSprite = upgradeEndSprite;
                        targetColor = abilityUpgradePair.Upgrades.Upgrade5a ? UpgradedColor : UnupgradedColor;
                    }
                    else
                    {
                        targetSprite = abilityUpgradePair.Upgrades.Upgrade5a ? upgradeEndLeftSprite : upgradeEndRightSprite;
                        targetColor = UpgradedColor;
                    }
                    break;
            }

            //Image objImage = obj.GetComponent<Image>();
            //objImage.raycastTarget = false;
            objImage.sprite = targetSprite;
            objImage.color = targetColor;
        }

        private void InitalizeAbilityDetailUIPrefabOnClick(DisplayAbilityListMenu abilityListMenu)
        {
            OnInitalizingAbilityDetail.Invoke();

            AbilityDetailUI abilityDetailUIObj = Instantiate(abilityDetailUIPrefab, abilityListMenu.transform.parent);
            abilityDetailUIObj.AbilityUpgradePair = abilityUpgradePair;
            abilityDetailUIObj.DisplayAbilityListMenu = abilityListMenu;
            abilityDetailUIObj.Initalize();
        }
    }
}
