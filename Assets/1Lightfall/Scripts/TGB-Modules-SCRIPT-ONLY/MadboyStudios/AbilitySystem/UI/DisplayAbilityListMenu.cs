using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MBS.AbilitySystem
{
    public class DisplayAbilityListMenu : MonoBehaviour
    {
        public GameObject ObjectWithAbilitiesToView;

        public AbilityOverviewButton abilityOverviewButtonPrefab;


        private List<AbilityAndUpgradePair> abilityUpgradePair;
        private List<AbilityOverviewButton> displayedAbilityElements;



        private void Awake()
        {
            abilityUpgradePair = new List<AbilityAndUpgradePair>();
            displayedAbilityElements = new List<AbilityOverviewButton>();

        }

        // Start is called before the first frame update
        void Start()
        {
            GetAbilityData(ObjectWithAbilitiesToView);
            DrawAbilityList();

        }

        private void GetAbilityData(GameObject sourceObject)
        {
            abilityUpgradePair.Clear();
            AbilityLoadout loadout = sourceObject.GetComponent<AbilityLoadout>();
            if (loadout == null)
            {
                Debug.LogWarning($"Trying to display abilites for {sourceObject.name}, which does not contain an AbilityLoadout.");
                return;
            }



            foreach (var abilityUpPair in loadout.Abilities)
            {
                abilityUpgradePair.Add(abilityUpPair);
            }
        }
        private void DrawAbilityList()
        {
            //clear current abilities displaying if any exist
            foreach (var item in displayedAbilityElements)
            {
                Destroy(item.gameObject);
            }
            displayedAbilityElements.Clear();

            //redraw abilities
            foreach (var abilityUpPair in abilityUpgradePair)
            {
                AbilityOverviewButton abilityOverviewButtonObj = Instantiate(abilityOverviewButtonPrefab, transform);
                string abilityTitle = abilityUpPair.AbilitySO.Title == "" ? abilityUpPair.AbilitySO.name : abilityUpPair.AbilitySO.Title;
                abilityOverviewButtonObj.Initalize(abilityUpPair, this, abilityTitle, " Level " + abilityUpPair.Upgrades.GetUIUpgradeLevel());
                abilityOverviewButtonObj.OnInitalizingAbilityDetail += () => { gameObject.SetActive(false); };

                displayedAbilityElements.Add(abilityOverviewButtonObj);
            }
        }

        private void OnDisable()
        {
            if (ObjectWithAbilitiesToView != null)
                ObjectWithAbilitiesToView.GetComponent<AbilityLoadout>().AbilitiesChanged -= DrawAbilityList;
        }

        private void OnEnable()
        {
            DrawAbilityList();
            ObjectWithAbilitiesToView.GetComponent<AbilityLoadout>().AbilitiesChanged += DrawAbilityList;
        }


    }
}
