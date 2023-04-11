using Opsive.UltimateCharacterController.Inventory;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    [CreateAssetMenu(fileName = "NewItemDetails", menuName = "Lightfall/ New Item Details")]
    public class ItemDetailsScriptableObject : ScriptableObject
    {
        public ItemType ItemType;
        public string ItemName;
        public Sprite Image;
        [TextArea]
        public string ItemShortDescription;
        public List<StatDetailForUI> Stats;
        [Button("Set Weapon Stats")]
        private void SetWeaponStats()
        {
            if(Stats!=null)
                Stats.Clear();
            if(Stats==null)
                Stats = new List<StatDetailForUI>();

            Stats.Add(new StatDetailForUI("Presence", 0, "The mesure of a weapon's gravitas."));
            Stats.Add(new StatDetailForUI("Capacity", 0, "The mesure of a weapon's hunger."));
            Stats.Add(new StatDetailForUI("Integrity", 0, "The mesure of a weapon's candidness."));
            Stats.Add(new StatDetailForUI("Tempo", 0, "The mesure of a weapon's vehemence."));
            Stats.Add(new StatDetailForUI("Ruin", 0, "The mesure of a weapon's malice."));

        }

        [Serializable]
        public class StatDetailForUI
        {
            public string Name;
            [Range(0f, 1f)]
            public float StatPercent;
            public string Tooltip;

            public StatDetailForUI(string name, float percent, string tooltip)
            {
                Name = name;
                StatPercent = percent;
                Tooltip = tooltip;
            }
        }

        
    }
}
