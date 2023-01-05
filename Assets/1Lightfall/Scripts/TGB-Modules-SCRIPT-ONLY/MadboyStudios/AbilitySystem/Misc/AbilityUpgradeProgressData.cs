using System;
using System.Collections.Generic;


namespace MBS.AbilitySystem
{
    [Serializable]
    public class AbilityUpgradeProgressData
    {
        public bool AbilityUnlocked;
        public bool Upgrade1;
        public bool Upgrade2;
        public bool Upgrade3a;
        public bool Upgrade3b;
        public bool Upgrade4a;
        public bool Upgrade4b;
        public bool Upgrade5a;
        public bool Upgrade5b;

        public event Action OnAbilityUpgrade = delegate { };

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
    }
}
