using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.AbilitySystem
{
    [Serializable]
    public class AbilityUpgradeProgressData
    {
        private bool IsToggled;
        [SerializeField, ReadOnly] private string initalUpgrade;

        [ShowIf("IsToggled")] public bool AbilityUnlocked;
        [ShowIf("IsToggled")] public bool Upgrade1;
        [ShowIf("IsToggled")] public bool Upgrade2;
        [ShowIf("IsToggled")] public bool Upgrade3a;
        [ShowIf("IsToggled")] public bool Upgrade3b;
        [ShowIf("IsToggled")] public bool Upgrade4a;
        [ShowIf("IsToggled")] public bool Upgrade4b;
        [ShowIf("IsToggled")] public bool Upgrade5a;
        [ShowIf("IsToggled")] public bool Upgrade5b;

        public event Action OnAbilityUpgrade = delegate { };

        [SerializeField] private UpgradeEnum targetUpgrade;

        [SerializeField, Button("Try Upgrade")]
        private void DefaultSizedButton()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("can only tryUpgrade when in play mode");
                return;
            }

            string upgradeString = "";
            switch (targetUpgrade)
            {
                case UpgradeEnum.AbilityUnlocked: upgradeString = "0"; break;
                case UpgradeEnum.Upgrade1: upgradeString = "1"; break;
                case UpgradeEnum.Upgrade2: upgradeString = "2"; break;
                case UpgradeEnum.Upgrade3a: upgradeString = "3a"; break;
                case UpgradeEnum.Upgrade3b: upgradeString = "3b"; break;
                case UpgradeEnum.Upgrade4a: upgradeString = "4a"; break;
                case UpgradeEnum.Upgrade4b: upgradeString = "4b"; break;
                case UpgradeEnum.Upgrade5a: upgradeString = "5a"; break;
                case UpgradeEnum.Upgrade5b: upgradeString = "5b"; break;
            }

            TryUpgrade(upgradeString);
            SetInitalUpgradeString();
        }

        [SerializeField, Button("Toggle Inital Upgrade state")]
        private void ToggleSetInitalUpgrades()
        {
            IsToggled = !IsToggled;
        }

        //Used for UI interfacing
        public int GetUIUpgradeLevel()
        {
            if (!AbilityUnlocked)
                return -1;

            int returnVal = 0;

            if (Upgrade1)
                returnVal = 1;
            if (Upgrade2)
                returnVal = 2;
            if (Upgrade3a || Upgrade3b)
                returnVal = 3;
            if (Upgrade4a || Upgrade4b)
                returnVal = 4;
            if (Upgrade5a || Upgrade5b)
                returnVal = 5;

            return returnVal;
        }

        public List<bool> ToList()
        {
            List<bool> returnVal = new List<bool>();

            returnVal.Add(Upgrade1);
            returnVal.Add(Upgrade2);
            returnVal.Add(Upgrade3a);
            returnVal.Add(Upgrade3b);
            returnVal.Add(Upgrade4a);
            returnVal.Add(Upgrade4b);
            returnVal.Add(Upgrade5a);
            returnVal.Add(Upgrade5b);

            return returnVal;
        }

        /// <summary>
        /// Keys are "0" for BaseAbilityUnlock, and "1","2","3a","3b","4a","4b","5a","5b" for upgrades
        /// </summary>
        /// <param name="upgradeKey"></param>
        /// <returns></returns>
        public bool TryUpgrade(string upgradeKey)
        {
            bool upgraded = false;
            switch (upgradeKey)
            {
                case "0": if (CanUpgrade(upgradeKey)) { AbilityUnlocked = true; upgraded = true; } break;
                case "1": if (CanUpgrade(upgradeKey)) { Upgrade1 = true; upgraded = true; } break;
                case "2": if (CanUpgrade(upgradeKey)) { Upgrade2 = true; upgraded = true; } break;
                case "3a": if (CanUpgrade(upgradeKey)) { Upgrade3a = true; upgraded = true; } break;
                case "3b": if (CanUpgrade(upgradeKey)) { Upgrade3b = true; upgraded = true; } break;
                case "4a": if (CanUpgrade(upgradeKey)) { Upgrade4a = true; upgraded = true; } break;
                case "4b": if (CanUpgrade(upgradeKey)) { Upgrade4b = true; upgraded = true; } break;
                case "5a": if (CanUpgrade(upgradeKey)) { Upgrade5a = true; upgraded = true; } break;
                case "5b": if (CanUpgrade(upgradeKey)) { Upgrade5b = true; upgraded = true; } break;

            }

            if (upgraded)
                OnAbilityUpgrade.Invoke();

            return upgraded;
        }

        public bool CanUpgrade(string upgradeKey)
        {
            switch (upgradeKey)
            {
                case "0": if (!AbilityUnlocked) { return true; } break;
                case "1": if (AbilityUnlocked && !Upgrade1) { return true; } break;
                case "2": if (Upgrade1 && !Upgrade2) { return true; } break;
                case "3a": if (Upgrade2 && !Upgrade3a && !Upgrade3b) { return true; } break;
                case "3b": if (Upgrade2 && !Upgrade3a && !Upgrade3b) { return true; } break;
                case "4a": if ((Upgrade3a || Upgrade3b) && !Upgrade4a && !Upgrade4b) { return true; } break;
                case "4b": if ((Upgrade3a || Upgrade3b) && !Upgrade4a && !Upgrade4b) { return true; } break;
                case "5a": if ((Upgrade4a || Upgrade4b) && !Upgrade5a && !Upgrade5b) { return true; } break;
                case "5b": if ((Upgrade4a || Upgrade4b) && !Upgrade5a && !Upgrade5b) { return true; } break;

            }

            return false;
        }

        [Serializable]
        private enum UpgradeEnum
        {
            AbilityUnlocked,
            Upgrade1,
            Upgrade2,
            Upgrade3a,
            Upgrade3b,
            Upgrade4a,
            Upgrade4b,
            Upgrade5a,
            Upgrade5b

        }

        public void OnValidate()
        {
            SetInitalUpgradeString();
        }

        private void SetInitalUpgradeString()
        {
            initalUpgrade = " ";
            initalUpgrade += AbilityUnlocked ? "Unlocked " : "Locked ";
            initalUpgrade += Upgrade1 ? "[1] " : "[] ";
            initalUpgrade += Upgrade2 ? "[2] " : "[] ";
            initalUpgrade += OutputChoiceUpgradeString(Upgrade3a, Upgrade3b, 3);
            initalUpgrade += OutputChoiceUpgradeString(Upgrade4a, Upgrade4b, 4);
            initalUpgrade += OutputChoiceUpgradeString(Upgrade5a, Upgrade5b, 5);


            string OutputChoiceUpgradeString(bool choice1, bool choice2, int level)
            {
                string returnval = "";

                if (choice1 && choice2)
                    returnval += $"[{level}ab] ";
                else if (choice1 || choice2)
                {
                    string choice = choice1 ? "a" : "b";
                    returnval += $"[{level}{choice}] ";
                }
                else
                {
                    returnval += "[] ";
                }

                return returnval;
            }
        }

    }
}
