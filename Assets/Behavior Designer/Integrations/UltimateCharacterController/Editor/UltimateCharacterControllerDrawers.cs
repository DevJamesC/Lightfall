/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace BehaviorDesigner.Editor.UltimateCharacterController.ObjectDrawers
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks.UltimateCharacterController;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Character.Effects;
    using Opsive.UltimateCharacterController.Inventory;
    using System.Collections.Generic;
    using System;
    using UnityEngine;
    using UnityEditor;

    /// <summary>
    /// Draws a custom inspector for the Behavior Designer - Ultimate Character Controller abilities.
    /// </summary>
    [CustomObjectDrawer(typeof(AbilityDrawerAttribute))]
    public class AbilityDrawer : ObjectDrawer
    {
        public override void OnGUI(GUIContent label)
        {
            (value as SharedString).Value = AbilityEffectDrawerHelper.DrawPopup(typeof(Ability), (value as SharedString).Value, label.text);
        }
    }

    /// <summary>
    /// Draws a custom inspector for the Behavior Designer - Ultimate Character Controller ItemSet abilities.
    /// </summary>
    [CustomObjectDrawer(typeof(ItemSetAbilityDrawerAttribute))]
    public class ItemSetAbilityDrawer : ObjectDrawer
    {
        public override void OnGUI(GUIContent label)
        {
            (value as SharedString).Value = AbilityEffectDrawerHelper.DrawPopup(typeof(ItemSetAbilityBase), (value as SharedString).Value, label.text);
        }
    }

    /// <summary>
    /// Draws a custom inspector for the Behavior Designer - Ultimate Character Controller effects.
    /// </summary>
    [CustomObjectDrawer(typeof(EffectDrawerAttribute))]
    public class EffectDrawer : ObjectDrawer
    {
        public override void OnGUI(GUIContent label)
        {
            (value as SharedString).Value = AbilityEffectDrawerHelper.DrawPopup(typeof(Effect), (value as SharedString).Value, label.text);
        }
    }

    /// <summary>
    /// Draws a custom inspector for the Behavior Designer - Ultimate Character Controller ItemSet categories.
    /// </summary>
    [CustomObjectDrawer(typeof(ItemSetCategoryDrawerAttribute))]
    public class ItemSetCategoryDrawer : ObjectDrawer
    {
        public override void OnGUI(GUIContent label)
        {
            // ItemSetManagerBase must exist for the categories to be populated.
            var itemSetManager = GameObject.FindObjectOfType<ItemSetManagerBase>();
            if (itemSetManager == null) {
                return;
            }
            itemSetManager.Initialize(false);

            if (itemSetManager.ItemSetGroups == null || itemSetManager.ItemSetGroups.Length == 0) {
                return;
            }

            // Draw a popup with all of the ItemSet groups.
            var categoryID = (value as SharedUInt).Value;
            var selected = -1;
            var categoryNames = new string[itemSetManager.ItemSetGroups.Length];
            for (int i = 0; i < categoryNames.Length; ++i) {
                categoryNames[i] = itemSetManager.ItemSetGroups[i].CategoryName;
                if (categoryID == itemSetManager.ItemSetGroups[i].CategoryID) {
                    selected = i;
                }
            }
            var newSelected = EditorGUILayout.Popup("ItemSet Category", (selected != -1 ? selected : 0), categoryNames);
            if (selected != newSelected || categoryID == 0) {
                (value as SharedUInt).Value = itemSetManager.ItemSetGroups[newSelected].CategoryID;
            }
        }
    }

    /// <summary>
    /// Helper methods for the Ulatimate Character Controller object drawers.
    /// </summary>
    public static class AbilityEffectDrawerHelper
    {
        private static Dictionary<Type, List<Type>> s_TypeListByType;
        private static int[] s_Indicies;
        private static string[] s_Names;

        /// <summary>
        /// Generic method which will draw the popup for all of the objects of the specified type.
        /// </summary>
        public static string DrawPopup(Type type, string value, string label)
        {
            if (s_TypeListByType == null || !s_TypeListByType.ContainsKey(type)) {
                if (s_TypeListByType == null) {
                    s_TypeListByType = new Dictionary<Type, List<Type>>();
                }

                // Find all of the objects of the specified type.
                var nameList = new List<string>();
                var typeList = new List<Type>();
                var indicies = new List<int>();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; ++i) {
                    var assemblyTypes = assemblies[i].GetTypes();
                    for (int j = 0; j < assemblyTypes.Length; ++j) {
                        if (type.IsAssignableFrom(assemblyTypes[j]) && !assemblyTypes[j].IsAbstract) {
                            typeList.Add(assemblyTypes[j]);
                            nameList.Add(assemblyTypes[j].Name);
                            indicies.Add(indicies.Count);
                        }
                    }
                }

                s_Names = nameList.ToArray();
                s_Indicies = indicies.ToArray();
                s_TypeListByType.Add(type, typeList);
                Array.Sort(s_Names, s_Indicies);
            }

            // Find the index of the type if it has already been specified.
            var types = s_TypeListByType[type];
            var index = 0;
            if (!string.IsNullOrEmpty(value)) {
                for (int i = 0; i < types.Count; ++i) {
                    if (types[s_Indicies[i]].FullName.Equals(value)) {
                        index = i;
                        break;
                    }
                }
            }

            // Gets a new type.
            index = EditorGUILayout.Popup(label, index, s_Names);
            if (!types[s_Indicies[index]].Name.Equals(value)) {
                value = types[s_Indicies[index]].FullName;
            }
            return value;
        }
    }
}