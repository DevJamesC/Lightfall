using MBS.StatsAndTags;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.ModifierSystem
{
    public abstract class EffectAlreadyExistsBase<T>
    {
        [SerializeField, ReadOnly, Title("$subclassName", null, TitleAlignments.Centered), PropertyOrder(-1)]
        [Tooltip("The tags associated with the effect. 'EffectAlreadyExists' logic will look for matching tags to find similar or same abilities to evaluate against.")]
        protected List<Tag> tags;

        private string subclassName;//used for GUI

        /// <summary>
        /// Use this as a gate clause, where True is "duplicate exists and has been handled" and False is "No duplicates to handle". <br/>
        /// Generally, if it returns true, then return out of effect activation.
        /// </summary>
        /// <param name="incomingEntry"></param>
        /// <param name="existingEntries"></param>
        /// <returns></returns>
        public virtual bool HandleEffectAlreadyExists(ModifierEntry incomingEntry, List<ModifierEntry> existingEntries)
        {
            return ValidateInput(incomingEntry, existingEntries);
        }

        /// <summary>
        /// Null checks, then trims results based on tags to make sure all existing entries have the same tags as the incoming. for "exact ability match or bust", this will need to be refactored.
        /// </summary>
        /// <param name="incomingEntry"></param>
        /// <param name="existingEntries"></param>
        /// <returns></returns>
        protected virtual bool ValidateInput(ModifierEntry incomingEntry, List<ModifierEntry> existingEntries)
        {
            if (existingEntries == null)
                return false;
            if (incomingEntry == null)
                return false;

            //Trim existingEntries based on tags (checks that the existingEntries all contain the tags in the incoming entry). For an "Exact match" or bust, will need to refactor this block
            foreach (var existingEntry in new List<ModifierEntry>(existingEntries))
            {
                foreach (var incomingTag in incomingEntry.Effect.Tags)
                {
                    if (!existingEntry.Effect.Tags.Contains(incomingTag))
                    {
                        existingEntries.Remove(existingEntry);
                        break;
                    }
                }
            }

            if (existingEntries.Count == 0)
                return false;

            if (existingEntries.Count == 1 && incomingEntry == existingEntries[0])
                return false;



            return true;
        }



        //Called by classes that impliment EffectAlreadyExistsBase, such as EffectApplyDamageOverTime
        public void OnValidate()
        {
            subclassName = this.GetType().ToString();
        }

        //Called by classes that impliment EffectAlreadyExistsBase, such as EffectApplyDamageOverTime
        public void PopulateTags(List<Tag> tags)
        {
            if (tags == null)
                return;

            if (this.tags == null)
                this.tags = new List<Tag>();

            //remove this to allow EffectAlreadyExists to have tags that the triggering Effect does not
            foreach (var tag in new List<Tag>(this.tags))
            {
                if (!tags.Contains(tag))
                    this.tags.Remove(tag);
            }

            foreach (var tag in tags)
            {
                if (!this.tags.Contains(tag))
                    this.tags.Add(tag);
            }



        }
    }


    //Since Unity cannot SerializeReference generics, need an intermidiate class in order to serialize them in the inspector
    //For our effects, simply use/create EffectAlreadyExistsEFFECFTNAME, instead of EffectAlreadyExistsBase<EFFECTTYPE>
    [Serializable]
    public class EffectAlreadyExistsDoTImplimentation : EffectAlreadyExistsBase<EffectApplyDamageOverTime>
    {

    }

    [Serializable]
    public class EffectAlreadyExistsApplyModifierToDamagerImplimentation : EffectAlreadyExistsBase<EffectApplyModifierToDamager>
    {

    }
}