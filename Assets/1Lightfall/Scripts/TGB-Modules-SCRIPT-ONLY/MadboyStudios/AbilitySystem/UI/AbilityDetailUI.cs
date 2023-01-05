using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace MBS.AbilitySystem
{
    public class AbilityDetailUI : MonoBehaviour
    {
        [HideInInspector]
        public AbilityAndUpgradePair AbilityUpgradePair;
        [HideInInspector]
        public DisplayAbilityListMenu DisplayAbilityListMenu;
        [SerializeField]
        private TextMeshProUGUI upgradeTitle;
        [SerializeField]
        private TextMeshProUGUI upgradeDescription;
        [SerializeField]
        private UpgradeDetailButton upgradeDetailButtonPrefab;
        [SerializeField]
        private StatProgressBarUI statDisplayPrefab;
        [SerializeField]
        private StatEffectUI statEffectDisplayPrefab;
        [SerializeField]
        private GameObject parentObjectForStatDisplay;
        [SerializeField]
        private GameObject parentObjectForUpgradeButtons;
        [SerializeField]
        private GameObject parentObjectForStatEffectDisplay;
        [SerializeField]
        private GameObject horizontalUpgradeArranger;
        [SerializeField]
        private Button closeButton;
        [SerializeField]
        private Sprite rootAbilityOutline;
        [SerializeField]
        private Sprite startingUpgradeOutline;
        [SerializeField]
        private Sprite middleUpgradeOutline;
        [SerializeField]
        private Sprite endUpgradeOutline;


        private UpgradeDetailButton baseAbilityButtion;
        private UpgradeDetailButton upgrade1Button;
        private UpgradeDetailButton upgrade2Button;
        private UpgradeDetailButton upgrade3aButton;
        private UpgradeDetailButton upgrade3bButton;
        private UpgradeDetailButton upgrade4aButton;
        private UpgradeDetailButton upgrade4bButton;
        private UpgradeDetailButton upgrade5aButton;
        private UpgradeDetailButton upgrade5bButton;

        public void Initalize()
        {
            closeButton.onClick.AddListener(() => { DisplayAbilityListMenu.gameObject.SetActive(true); Destroy(gameObject); });

            baseAbilityButtion = SetupButton("0",
                new AbilityUpgradeProgressionContainer.AbilityUpgradeProgressionContainerEntry()
                {
                    Icon = AbilityUpgradePair.AbilitySO.Icon,
                    Title = AbilityUpgradePair.AbilitySO.Title,
                    Description = AbilityUpgradePair.AbilitySO.Description
                },
                rootAbilityOutline);

            //generate spacer for initalization here, and for later
            GameObject spacer = new GameObject("Spacer", typeof(RectTransform));
            RectTransform spacerRect = spacer.GetComponent<RectTransform>();
            spacerRect.sizeDelta = new Vector2(10, 0);
            spacer.transform.SetParent(parentObjectForUpgradeButtons.transform);

            upgrade1Button = SetupButton("1", AbilityUpgradePair.AbilitySO.PossibleUpgrades.Upgrade1, startingUpgradeOutline);
            upgrade2Button = SetupButton("2", AbilityUpgradePair.AbilitySO.PossibleUpgrades.Upgrade2, middleUpgradeOutline);
            upgrade3aButton = SetupButton("3a", AbilityUpgradePair.AbilitySO.PossibleUpgrades.Upgrade3a, middleUpgradeOutline);
            upgrade3bButton = SetupButton("3b", AbilityUpgradePair.AbilitySO.PossibleUpgrades.Upgrade3b, middleUpgradeOutline);
            upgrade4aButton = SetupButton("4a", AbilityUpgradePair.AbilitySO.PossibleUpgrades.Upgrade4a, middleUpgradeOutline);
            upgrade4bButton = SetupButton("4b", AbilityUpgradePair.AbilitySO.PossibleUpgrades.Upgrade4b, middleUpgradeOutline);
            upgrade5aButton = SetupButton("5a", AbilityUpgradePair.AbilitySO.PossibleUpgrades.Upgrade5a, endUpgradeOutline);
            upgrade5bButton = SetupButton("5b", AbilityUpgradePair.AbilitySO.PossibleUpgrades.Upgrade5b, endUpgradeOutline);

            //setup "side-by-side" upgrade buttons
            //Instanciate row 1
            GameObject horizontalArranger1 = Instantiate(horizontalUpgradeArranger, parentObjectForUpgradeButtons.transform);
            //generate spacer
            Instantiate(spacer, parentObjectForUpgradeButtons.transform);
            //Instanciate row 2
            GameObject horizontalArranger2 = Instantiate(horizontalUpgradeArranger, parentObjectForUpgradeButtons.transform);
            //generate spacer
            Instantiate(spacer, parentObjectForUpgradeButtons.transform);
            //Instanciate row 3
            GameObject horizontalArranger3 = Instantiate(horizontalUpgradeArranger, parentObjectForUpgradeButtons.transform);
            //set upgrade buttons to be children of the row objects
            upgrade3aButton.gameObject.transform.SetParent(horizontalArranger1.transform);
            upgrade3bButton.gameObject.transform.SetParent(horizontalArranger1.transform);
            upgrade4aButton.gameObject.transform.SetParent(horizontalArranger2.transform);
            upgrade4bButton.gameObject.transform.SetParent(horizontalArranger2.transform);
            upgrade5aButton.gameObject.transform.SetParent(horizontalArranger3.transform);
            upgrade5bButton.gameObject.transform.SetParent(horizontalArranger3.transform);
            //set the first spacer to be a bit bigger than the rest
            spacerRect.sizeDelta = new Vector2(10, 15);

            //setup unity navigation to the buttons properly
            SetupButtonNavigation(baseAbilityButtion.Button, closeButton, upgrade1Button.Button, null, closeButton);
            SetupButtonNavigation(upgrade1Button.Button, baseAbilityButtion.Button, upgrade2Button.Button, null, null);
            SetupButtonNavigation(upgrade2Button.Button, upgrade1Button.Button, upgrade3aButton.Button, upgrade3bButton.Button, upgrade3aButton.Button);
            SetupButtonNavigation(upgrade3aButton.Button, upgrade2Button.Button, upgrade4aButton.Button, upgrade3bButton.Button, closeButton);
            SetupButtonNavigation(upgrade3bButton.Button, upgrade2Button.Button, upgrade4bButton.Button, null, upgrade3aButton.Button);

            SetupButtonNavigation(upgrade4aButton.Button, upgrade3aButton.Button, upgrade5aButton.Button, upgrade4bButton.Button, closeButton);
            SetupButtonNavigation(upgrade4bButton.Button, upgrade3bButton.Button, upgrade5bButton.Button, null, upgrade4aButton.Button);

            SetupButtonNavigation(upgrade5aButton.Button, upgrade4aButton.Button, null, upgrade5bButton.Button, closeButton);
            SetupButtonNavigation(upgrade5bButton.Button, upgrade4bButton.Button, null, null, upgrade5aButton.Button);

            //add delegates
            AbilityUpgradePair.Upgrades.OnAbilityUpgrade += OnAbilityUpgrade;

            //Select baseAbility button by default
            EventSystem.current.SetSelectedGameObject(baseAbilityButtion.gameObject);
        }

        private UpgradeDetailButton SetupButton(string upgradeIndex, AbilityUpgradeProgressionContainer.AbilityUpgradeProgressionContainerEntry upgrade, Sprite outlineGraphic)
        {
            // instanciate button
            UpgradeDetailButton abilityUpgradeButton = Instantiate(upgradeDetailButtonPrefab, parentObjectForUpgradeButtons.transform);

            //provide button the abilityUpgradePair
            abilityUpgradeButton.abilityUpgradePair = AbilityUpgradePair;

            //provide button its index (the upgrade they are associated to) 
            abilityUpgradeButton.upgradeIndex = upgradeIndex;

            //apply outline graphic
            abilityUpgradeButton.outlineGraphic = outlineGraphic;

            //apply center graphic
            abilityUpgradeButton.centerGraphic = upgrade.Icon;

            //apply delegates
            abilityUpgradeButton.OnSelected += () =>
            {
                upgradeTitle.text = upgrade.Title;
                upgradeDescription.text = upgrade.Description;

                DestroyCurrentOldUI();

                InstanciateStatDisplay(upgradeIndex);

            };

            abilityUpgradeButton.OnDeselected += () =>
            {
                upgradeTitle.text = AbilityUpgradePair.AbilitySO.Title;
                upgradeDescription.text = AbilityUpgradePair.AbilitySO.Description;
            //set stats to abilityBase+purchased upgrades
        };

            abilityUpgradeButton.Initalize();


            abilityUpgradeButton.Button.onClick.AddListener(() =>
            {
                DestroyCurrentOldUI();

                InstanciateStatDisplay(upgradeIndex);
            });

            return abilityUpgradeButton;
        }

        private void DestroyCurrentOldUI()
        {
            //Display stats
            for (int i = 0; i < parentObjectForStatDisplay.transform.childCount; i++)
            {
                if (parentObjectForStatDisplay.transform.GetChild(i).gameObject != parentObjectForStatEffectDisplay)
                    Destroy(parentObjectForStatDisplay.transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < parentObjectForStatEffectDisplay.transform.childCount; i++)
            {
                Destroy(parentObjectForStatEffectDisplay.transform.GetChild(i).gameObject);
            }
        }

        private void InstanciateStatDisplay(string upgradeIndex)
        {
            List<AbilityUIStat> stats = AbilityUpgradePair.AbilitySO.GetStats(AbilityUpgradePair.Upgrades, upgradeIndex);
            foreach (AbilityUIStat stat in stats)
            {

                if (stat.CurrentValue == 0 && stat.InitalValue == 0 && stat.EffectIcon == null && stat.ProspectiveValue == 0)
                    continue;


                //instanciate stat UI display object
                if (stat.CurrentValue != 0 || stat.ProspectiveValue != 0)
                {
                    StatProgressBarUI statUI = Instantiate(statDisplayPrefab, parentObjectForStatDisplay.transform);
                    statUI.Initalize(stat);
                }
                //instanciate stat FX
                if (stat.EffectIcon != null)
                {
                    //need horizontal thingy as parent
                    StatEffectUI statEffectUI = Instantiate(statEffectDisplayPrefab, parentObjectForStatEffectDisplay.transform);
                    statEffectUI.forgroundImage.sprite = stat.EffectIcon;
                }
            }
            parentObjectForStatEffectDisplay.transform.SetAsLastSibling();
        }

        private void SetupButtonNavigation(Button source, Button up, Button down, Button right, Button left)
        {
            Navigation nav = source.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = up;
            nav.selectOnDown = down;
            nav.selectOnRight = right;
            nav.selectOnLeft = left;
            source.navigation = nav;
        }

        private void OnDestroy()
        {
            AbilityUpgradePair.Upgrades.OnAbilityUpgrade -= OnAbilityUpgrade;
        }

        private void OnAbilityUpgrade()
        {
            //initalize all buttons
            baseAbilityButtion.Initalize(false);
            upgrade1Button.Initalize(false);
            upgrade2Button.Initalize(false);
            upgrade3aButton.Initalize(false);
            upgrade3bButton.Initalize(false);
            upgrade4aButton.Initalize(false);
            upgrade4bButton.Initalize(false);
            upgrade5aButton.Initalize(false);
            upgrade5bButton.Initalize(false);
        }
    }
}
