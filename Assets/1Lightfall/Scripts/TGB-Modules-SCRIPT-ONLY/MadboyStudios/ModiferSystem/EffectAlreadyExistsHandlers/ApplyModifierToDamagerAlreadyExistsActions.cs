using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public class ApplyModifierToDamagerAlreadyExistsActions : EffectAlreadyExistsApplyModifierToDamagerImplimentation
    {
        [SerializeField]
        private EffectAction effectAction;

        public override bool HandleEffectAlreadyExists(ModifierEntry incomingEntry, List<ModifierEntry> existingEntries)
        {

            if (!ValidateInput(incomingEntry, existingEntries))
                return false;

            existingEntries.Remove(incomingEntry);


            switch (effectAction)
            {
                case EffectAction.ReplaceAllExisting:
                    ReplaceExisting(incomingEntry, existingEntries);

                    return false;

                case EffectAction.DoubleUp:
                    return false;

                case EffectAction.SetAllExistingToDurationOfIncoming:
                    SetExistingToDurationOfIncoming(incomingEntry, existingEntries);
                    return true;
            }


            return false;
        }

        private void ReplaceExisting(ModifierEntry incomingEntry, List<ModifierEntry> existingEntries)
        {
            foreach (var entry in existingEntries)
            {
                entry.RemoveModifier();
            }
        }

        private void SetExistingToDurationOfIncoming(ModifierEntry incomingEntry, List<ModifierEntry> existingEntries)
        {
            foreach (var entry in existingEntries)
            {
                entry.RemainingDuration = incomingEntry.RemainingDuration;
            }
        }

        [Serializable]
        private enum EffectAction
        {
            ReplaceAllExisting,
            DoubleUp,
            SetAllExistingToDurationOfIncoming
        }

    }
}
