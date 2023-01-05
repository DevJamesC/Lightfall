using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MBS.ModifierSystem
{
    public class DamageOverTimeEffectAlreadyExistsTakeHigherDPS : EffectAlreadyExistsDoTImplimentation
    {

        [SerializeField, Range(0, 1),
            Tooltip("If the effect with the highest DPS has this percent less total damage than the effect " +
                    "with the highest remaining damage (usually the incoming effect), then take the lower DPS " +
                    "but higher damage.\n Eg: current effect has a DPS of 10, but a remaining duration of 1, " +
                    "while incoming has a DPS of 7 and a duration of 10. If the threshold is .25, then " +
                    "10 damage is less than 70*.25=17.5 damage, so the DPS 7 will be applied.")]
        private float takeHigherTotalDamageThreshold = .5f;


        private EffectApplyDamageOverTimeModifierEntryCollection incoming;
        List<EffectApplyDamageOverTimeModifierEntryCollection> current;

        public override bool HandleEffectAlreadyExists(ModifierEntry incomingEntry, List<ModifierEntry> existingEntries)
        {
            if (!ValidateInput(incomingEntry, existingEntries))
                return false;

            Setup(incomingEntry, existingEntries);

            //check which has the higher DPS and total damage
            EffectApplyDamageOverTimeModifierEntryCollection highestDPS = incoming;
            EffectApplyDamageOverTimeModifierEntryCollection highestTotalDamage = incoming;

            foreach (var currentEffect in current)
            {
                if (currentEffect.Effect.DPS() > highestDPS.Effect.DPS())
                    highestDPS = currentEffect;

                if (currentEffect.Effect.RemainingDamage(currentEffect.Entry.RemainingDuration) > highestTotalDamage.Effect.RemainingDamage(highestTotalDamage.Entry.RemainingDuration))
                    highestTotalDamage = currentEffect;
            }

            //if the highest DPS is not also the highest damage, decide
            EffectApplyDamageOverTimeModifierEntryCollection chosenEffect = highestDPS;


            if (highestDPS != highestTotalDamage)
            {
                //If the highst DPS is beyond the threshold of total damage lower than the highest total damage, then take the highst total damage instead of the highest DPS
                if (highestDPS.Effect.RemainingDamage(highestDPS.Entry.RemainingDuration) * takeHigherTotalDamageThreshold <
                    highestTotalDamage.Effect.RemainingDamage(highestTotalDamage.Entry.RemainingDuration))
                    chosenEffect = highestTotalDamage;
                //add an else if for if they are equal damage then prioritize the one with the longest duration? might not be necessary, but might be nice for comboing abilites.
            }

            //if the chosen effect is not the incoming effect, then terminate the incoming effect and return TRUE
            if (chosenEffect != incoming)
            {
                incoming.Entry.InitalDuration = -1;
                return true;
            }
            else//remove the "other" effects and return FALSE, effectivly refreshing or replaceing the effect
            {
                foreach (var existinEntry in existingEntries)
                {
                    existinEntry.InitalDuration = -1;
                }
                return false;
            }




        }

        //Maps the incoming entries to thier effects, so we can search a list by either effect or entry, and get the other out
        protected void Setup(ModifierEntry incomingEntry, List<ModifierEntry> existingEntries)
        {
            incoming = new EffectApplyDamageOverTimeModifierEntryCollection(incomingEntry.Effect as EffectApplyDamageOverTime, incomingEntry);

            current = new List<EffectApplyDamageOverTimeModifierEntryCollection>();

            existingEntries.Remove(incomingEntry);

            foreach (ModifierEntry other in existingEntries)
            {
                current.Add(new EffectApplyDamageOverTimeModifierEntryCollection(other.Effect as EffectApplyDamageOverTime, other));
            }
        }

        //used to map an effect to an entry for easy searching. Like a dictionary, but the entry can be stored in a list
        protected class EffectApplyDamageOverTimeModifierEntryCollection
        {
            public EffectApplyDamageOverTime Effect;
            public ModifierEntry Entry;

            public EffectApplyDamageOverTimeModifierEntryCollection(EffectApplyDamageOverTime effect, ModifierEntry entry)
            {
                Effect = effect;
                Entry = entry;
            }
        }
    }
}